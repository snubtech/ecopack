/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ClaudeChatService (AI 답변 생성)
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 우측 채팅창의 질문을 Anthropic Claude 에 보내고 답변 글을 받아 옵니다.
 *
 * 2. 무엇을 같이 보내나
 *    - (가) 역할 지시문: "에코패키징 업무를 돕는 도우미" 라는 성격과 답변 규칙
 *    - (나) 작업 상태 전문: ProjectContextBuilder 가 모아 온, 프로젝트 생성부터
 *          기본사항·모의평가·TD·DOC 까지의 전 과정 내용
 *    - (다) 직전 대화: 같은 세션의 최근 주고받은 말
 *    이 세 가지를 합쳐 보내기 때문에 "이전 대화"와 "중앙 화면 작업상태"를 모두 참조합니다.
 *
 * 3. 왜 스트리밍으로 호출하나
 *    - 화면에는 완성된 답변을 한 번에 돌려주지만(단건 JSON),
 *      서버가 Claude 를 부를 때는 스트리밍으로 받습니다.
 *      출력이 길어져도 HTTP 제한 시간에 걸리지 않게 하려는 목적입니다.
 *
 * 4. 프롬프트 캐시
 *    - 역할 지시문과 작업 상태는 같은 세션 안에서 잘 바뀌지 않으므로 캐시 표시를 답니다.
 *      두 번째 질문부터는 같은 앞부분을 다시 계산하지 않아 비용과 시간이 줄어듭니다.
 * ==============================================================================
 */
using System.Diagnostics;
using System.Text;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Options;
using ecopack.Api.Support;

namespace ecopack.Api.Services
{
    /// <summary>AI 답변 한 건의 결과</summary>
    public class ClaudeChatResult
    {
        public string Answer { get; set; } = "";
        public string? AiModelNm { get; set; }
        public int? InTknCnt { get; set; }
        public int? OutTknCnt { get; set; }
        public int ElpsMsVal { get; set; }

        /// <summary>실패했을 때만 채워진다. 채워져 있으면 Answer 는 안내 문구다.</summary>
        public string? ErrCntn { get; set; }
        public bool Success => string.IsNullOrEmpty(ErrCntn);
    }

    /// <summary>대화 한 줄(과거 이력을 넘길 때 쓴다)</summary>
    public record ClaudeChatTurn(string Role, string Content);

    public interface IClaudeChatService
    {
        Task<ClaudeChatResult> AskAsync(
            string question,
            string workContextText,
            IReadOnlyList<ClaudeChatTurn> history,
            CancellationToken ct = default);
    }

    public class ClaudeChatService : IClaudeChatService
    {
        private readonly LlmChatOptions _opt;
        private readonly ILogger<ClaudeChatService> _logger;
        private readonly AnthropicClient? _client;

        /// <summary>
        /// 도우미의 성격과 답변 규칙. 이 글은 바뀌지 않으므로 캐시 대상 앞부분이 된다.
        /// </summary>
        private const string Persona = """
            당신은 친환경 포장 개발 플랫폼 'EcoPack(PackageView)'의 업무 도우미입니다.
            화면 오른쪽 채팅창에서 사용자의 질문에 답합니다.

            [사용자가 하는 일]
            사용자는 포장재를 개발하면서 다음 순서로 작업합니다.
              ① 프로젝트 생성       — 제품명, 수출 대상국, 진행할 포장차수(1차/2차/3차)를 정한다
              ② 기본사항(데이터셋)  — 차수별로 적용소재·사용환경·포장재 구분·소재 구성을 고른다
              ③ 디자인 템플릿       — 포장 디자인 템플릿을 선택한다
              ④ 모의평가            — 에코패키징 설문에 답해 영역별 점수를 받는다
              ⑤ TD (기술문서)       — 제품 규격·BOM·시험결과·제조공정을 정리한다
              ⑥ DOC (적합성 선언서) — 규격 충족 여부를 선언하고 근거문서를 붙인다
            포장차수는 1차(제품포장) / 2차(운송포장) / 3차(수송포장)를 뜻합니다.

            [답변 규칙]
            - 아래 '현재 작업 상태'에 담긴 내용을 근거로 답하세요. 그 안에 있는 값은 실제 저장된 값입니다.
            - 사용자가 지금 보고 있는 화면만 보지 말고, ①부터 지금까지의 전 과정을 함께 살펴 답하세요.
              예를 들어 TD 질문이라도 기본사항에서 고른 소재와 모의평가 점수를 함께 근거로 삼습니다.
            - 작업 상태에 없는 값은 지어내지 마세요. 없으면 "아직 입력되지 않았다"라고 말하고,
              그 값을 채우려면 어느 화면으로 가야 하는지 알려 주세요.
            - 다음 단계가 무엇인지 물으면, 아직 비어 있는 단계를 짚어 순서대로 안내하세요.
            - 한국어로, 실무자에게 말하듯 간결하게 답하세요. 불필요한 인사말은 붙이지 않습니다.
            - 표로 보여 주는 편이 나은 내용은 마크다운 표를 쓰세요.
            - 법규·인증 판단은 참고 의견임을 밝히고, 최종 확인은 해당 기관 기준을 따르라고 안내하세요.
            """;

        public ClaudeChatService(IOptions<LlmChatOptions> options, ILogger<ClaudeChatService> logger)
        {
            _opt = options.Value;
            _logger = logger;

            // 키는 설정값 우선, 없으면 환경변수. 둘 다 없으면 클라이언트를 만들지 않고
            // 호출 시점에 안내 문구를 돌려준다(서버가 죽지 않게).
            var apiKey = !string.IsNullOrWhiteSpace(_opt.ApiKey)
                ? _opt.ApiKey
                : Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("ANTHROPIC_API_KEY 가 없어 AI 채팅이 비활성화된다. 환경변수 또는 appsettings 의 LlmChat:ApiKey 를 설정하라.");
                _client = null;
            }
            else
            {
                _client = new AnthropicClient { ApiKey = apiKey };
            }
        }

        public async Task<ClaudeChatResult> AskAsync(
            string question,
            string workContextText,
            IReadOnlyList<ClaudeChatTurn> history,
            CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            if (_client == null)
            {
                return new ClaudeChatResult
                {
                    Answer = "AI 응답 키가 설정되지 않아 답변할 수 없습니다. 관리자에게 ANTHROPIC_API_KEY 설정을 요청해 주세요.",
                    ErrCntn = "ANTHROPIC_API_KEY not configured",
                    ElpsMsVal = (int)sw.ElapsedMilliseconds
                };
            }

            // ── 대화 목록 조립: 과거 이력 → 이번 질문 ──────────────────
            var messages = new List<MessageParam>();
            foreach (var turn in history)
            {
                if (string.IsNullOrWhiteSpace(turn.Content)) continue;
                messages.Add(new MessageParam
                {
                    Role = turn.Role == "assistant" ? Role.Assistant : Role.User,
                    Content = turn.Content
                });
            }
            messages.Add(new MessageParam { Role = Role.User, Content = question });

            // 첫 메시지는 반드시 user 여야 하고, 같은 역할이 연달아 오면 안 된다.
            messages = Normalize(messages);

            // ── 시스템 프롬프트: 고정 지시문 + 작업 상태 ────────────────
            // 두 블록 모두 세션 안에서 잘 바뀌지 않으므로 뒤쪽에 캐시 표시를 단다.
            var systemBlocks = new List<TextBlockParam>
            {
                new() { Text = Persona },
                new()
                {
                    Text = "# 현재 작업 상태 (DB 에서 읽어 온 실제 값)\n\n" + workContextText,
                    CacheControl = new CacheControlEphemeral()
                }
            };

            var parameters = new MessageCreateParams
            {
                Model = _opt.Model,
                MaxTokens = _opt.MaxTokens,
                System = systemBlocks,
                OutputConfig = new OutputConfig { Effort = ParseEffort(_opt.Effort) },
                Messages = messages
            };

            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(_opt.TimeoutSeconds));

                var text = new StringBuilder();
                int? inputTokens = null;
                int? outputTokens = null;

                // 브라우저에는 한 번에 돌려주지만, Claude 호출은 스트리밍으로 받아
                // 답변이 길어져도 HTTP 제한 시간에 걸리지 않게 한다.
                await foreach (var ev in _client.Messages.CreateStreaming(parameters, cancellationToken: timeout.Token))
                {
                    if (ev.TryPickContentBlockDelta(out var blockDelta)
                        && blockDelta.Delta.TryPickText(out var textDelta))
                    {
                        text.Append(textDelta.Text);
                    }
                    else if (ev.TryPickStart(out var start))
                    {
                        inputTokens = (int?)start.Message.Usage?.InputTokens;
                    }
                    else if (ev.TryPickDelta(out var msgDelta))
                    {
                        outputTokens = (int?)msgDelta.Usage?.OutputTokens;
                    }
                }

                sw.Stop();
                var answer = text.ToString().Trim();

                if (string.IsNullOrEmpty(answer))
                {
                    return new ClaudeChatResult
                    {
                        Answer = "답변을 만들지 못했습니다. 질문을 조금 더 구체적으로 적어 다시 물어봐 주세요.",
                        AiModelNm = _opt.Model,
                        InTknCnt = inputTokens,
                        OutTknCnt = outputTokens,
                        ElpsMsVal = (int)sw.ElapsedMilliseconds,
                        ErrCntn = "empty response"
                    };
                }

                return new ClaudeChatResult
                {
                    Answer = answer,
                    AiModelNm = _opt.Model,
                    InTknCnt = inputTokens,
                    OutTknCnt = outputTokens,
                    ElpsMsVal = (int)sw.ElapsedMilliseconds
                };
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                sw.Stop();
                _logger.LogWarning("AI 응답이 {Sec}초 안에 오지 않아 중단했다.", _opt.TimeoutSeconds);
                return new ClaudeChatResult
                {
                    Answer = "응답이 너무 오래 걸려 중단했습니다. 잠시 후 다시 시도해 주세요.",
                    AiModelNm = _opt.Model,
                    ElpsMsVal = (int)sw.ElapsedMilliseconds,
                    ErrCntn = $"timeout after {_opt.TimeoutSeconds}s"
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "AI 응답 생성 중 오류");
                return new ClaudeChatResult
                {
                    Answer = "AI 응답 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요.",
                    AiModelNm = _opt.Model,
                    ElpsMsVal = (int)sw.ElapsedMilliseconds,
                    ErrCntn = $"{ex.GetType().Name}: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 대화 목록을 API 규칙에 맞게 다듬는다.
        /// 첫 줄은 user 로 시작해야 하고, 같은 역할이 연달아 오면 하나로 합친다.
        /// (저장된 이력에 AI 답변만 남아 있는 등 예외 상황에서 400 이 나지 않게 한다)
        /// </summary>
        private static List<MessageParam> Normalize(List<MessageParam> messages)
        {
            var result = new List<MessageParam>();
            foreach (var m in messages)
            {
                // 맨 앞의 assistant 는 버린다
                if (result.Count == 0 && m.Role == Role.Assistant) continue;

                if (result.Count > 0 && result[^1].Role == m.Role)
                {
                    // 같은 역할이 이어지면 줄바꿈으로 합친다
                    var prev = result[^1];
                    result[^1] = new MessageParam
                    {
                        Role = m.Role,
                        Content = $"{prev.Content}\n\n{m.Content}"
                    };
                    continue;
                }

                result.Add(m);
            }
            return result;
        }

        /// <summary>설정 문자열을 Effort 값으로 바꾼다. 모르는 값이면 medium.</summary>
        private static Effort ParseEffort(string? value) => (value ?? "").Trim().ToLowerInvariant() switch
        {
            "low" => Effort.Low,
            "high" => Effort.High,
            "max" => Effort.Max,
            _ => Effort.Medium,
        };
    }
}
