/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - LlmChatController (우측 AI 채팅창)
 * ==============================================================================
 *
 * 1. 담당 범위
 *    - 화면 오른쪽 AI 채팅창의 서버 쪽 기능 전부를 맡습니다.
 *    - 쓰는 테이블: llm_chat_session(세션) / llm_chat_msg(메시지)
 *                   llm_chat_session_arch, llm_chat_msg_arch(2일 지난 대화의 백업본)
 *
 * 2. 대화 세션 (요건 ①: 로그인한 동안 세션과 로그가 유지된다)
 *    - OpenSession : 로그인 직후 화면이 부릅니다. 그 회원의 열려 있는(ACTIVE) 세션이 있으면
 *                    그걸 그대로 돌려주고, 없으면 새로 만듭니다.
 *                    그래서 새로고침을 해도 하던 대화가 이어집니다.
 *    - CloseSession: 로그아웃할 때 부릅니다. 세션을 CLOSED 로 닫습니다.
 *                    닫아도 기록은 지우지 않습니다. 지난 대화 목록에서 다시 열어 볼 수 있습니다.
 *
 * 3. 질문과 답변 (요건 ③④: 이전 대화 + 중앙 화면의 전 과정 작업상태를 참조한다)
 *    두 가지 방식을 모두 둡니다. 화면은 기본으로 스트리밍을 씁니다.
 *      - SendStream : SSE 로 답변 조각을 오는 대로 흘려보냅니다(권장).
 *      - Send       : 완성된 답변 1건을 한 번에 돌려줍니다(대체 경로).
 *    둘 다 처리 순서는 같습니다.
 *             ⓐ 로그인·소유자 확인
 *             ⓑ ProjectContextBuilder 로 "프로젝트 생성부터 지금까지" 전 과정을 모읍니다
 *             ⓒ 같은 세션의 직전 대화를 꺼냅니다
 *             ⓓ ⓑ+ⓒ+질문을 한 덩어리 글로 합쳐 생성형 AI 서버에 보냅니다
 *                (그 서버는 요청 항목이 question 하나뿐이라 따로 담을 자리가 없습니다.
 *                 ChatPromptBuilder 가 이 합치는 일을 맡습니다)
 *             ⓔ 질문과 답을 각각 한 줄씩 llm_chat_msg 에 남깁니다
 *                (이때 그 순간의 작업상태 스냅샷도 함께 저장해 나중에 추적할 수 있게 합니다)
 *                DB 에 남기는 질문은 합쳐 보낸 긴 글이 아니라 사용자가 입력한 원래 문장입니다.
 *
 * 4. 백업 (요건 ②: 2일 지난 대화 이력의 로그를 백업한다)
 *    - Archive : 관리자가 직접 백업을 돌릴 때 씁니다.
 *                평소에는 LlmChatArchiveHostedService 가 주기적으로 알아서 돕니다.
 *
 * 5. 접근 제한
 *    - 모든 조회·저장은 repCustId(로그인한 고객 ID)를 기준으로 자기 것만 다룹니다.
 *      프로젝트를 함께 넘길 때는 ProjectAccess 로 그 프로젝트가 본인 것인지도 확인합니다.
 *      (이 프로젝트의 다른 컨트롤러들과 같은 방식입니다)
 * ==============================================================================
 */
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Services;
using ecopack.Api.Support;

namespace ecopack.Api.Controllers
{
    /// <summary>우측 AI 채팅창. 라우트: api/LlmChat</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LlmChatController : ControllerBase
    {
        /// <summary>질문 글자수 상한. 지나치게 긴 입력을 막는다.</summary>
        private const int MaxQuestionLength = 4000;

        private readonly AppDbContext _context;
        private readonly IEcoAiChatService _chat;
        private readonly ILlmChatArchiveService _archive;
        private readonly LlmChatOptions _opt;

        private static readonly JsonSerializerOptions SnapshotJsonOpt = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// SSE 로 내보낼 JSON 설정.
        /// 화면(JS)이 그대로 읽으므로 항목 이름은 camelCase 로 맞추고,
        /// 한글이 \uXXXX 로 부풀지 않도록 이스케이프를 느슨하게 둔다.
        /// </summary>
        private static readonly JsonSerializerOptions SseJsonOpt = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public LlmChatController(
            AppDbContext context,
            IEcoAiChatService chat,
            ILlmChatArchiveService archive,
            IOptions<LlmChatOptions> options)
        {
            _context = context;
            _chat = chat;
            _archive = archive;
            _opt = options.Value;
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/OpenSession
        // 로그인 직후 대화 세션을 연다. 이미 열려 있으면 그 세션을 그대로 쓴다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("OpenSession")]
        public async Task<IActionResult> OpenSession([FromBody] LlmChatOpenSessionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.RepCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }

            // 로그인 중에는 세션 하나를 계속 쓴다. 새로고침해도 하던 대화가 이어지도록.
            var session = await _context.LlmChatSession
                .Where(s => s.RepCustId == dto.RepCustId && s.ChatSesStatCd == "ACTIVE")
                .OrderByDescending(s => s.FrstCrtDtm)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                session = new LlmChatSession
                {
                    ChatSesId = "S" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    RepCustId = dto.RepCustId,
                    PrjId = Blank(dto.PrjId),
                    PackLevel = Blank(dto.PackLevel),
                    ChatSesStatCd = "ACTIVE",
                    ChatMsgCnt = 0,
                    LgnDtm = DateTime.Now,
                    FrstCrtDtm = DateTime.Now,
                    LastUpdDtm = DateTime.Now
                };
                _context.LlmChatSession.Add(session);
                await _context.SaveChangesAsync();
            }

            var messages = await _context.LlmChatMsg
                .AsNoTracking()
                .Where(m => m.ChatSesId == session.ChatSesId)
                .OrderBy(m => m.ChatMsgSeq)
                .Select(m => ToMsgDto(m))
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new LlmChatHistoryDto
                {
                    Session = ToSessionDto(session, archived: false),
                    Messages = messages
                }
            });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/CloseSession?chatSesId=xxx&repCustId=xxx
        // 로그아웃할 때 세션을 닫는다. 기록은 남는다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("CloseSession")]
        public async Task<IActionResult> CloseSession([FromQuery] string chatSesId, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }

            var session = await _context.LlmChatSession
                .FirstOrDefaultAsync(s => s.ChatSesId == chatSesId && s.RepCustId == repCustId);

            if (session == null)
            {
                // 이미 백업으로 넘어갔거나 없는 세션. 로그아웃을 막을 이유는 없다.
                return Ok(new { success = true, message = "닫을 세션이 없습니다." });
            }

            session.ChatSesStatCd = "CLOSED";
            session.LastUpdDtm = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "대화 세션을 닫았습니다." });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/SendStream
        // 질문을 보내고 답변을 SSE 로 흘려받는다. 화면이 기본으로 쓰는 경로.
        //
        // 명세서상 답변에 수 초~수십 초가 걸릴 수 있어, 다 만들어질 때까지 기다리지 않고
        // 조각이 오는 대로 화면에 내보낸다.
        //
        // ⚠️ 한 번 흘려보내기 시작하면 HTTP 상태코드를 바꿀 수 없다.
        //    그래서 로그인·소유자·입력값 확인은 반드시 첫 바이트를 쓰기 전에 끝낸다.
        //    그 뒤에 생긴 문제는 상태코드가 아니라 error 이벤트로 알린다.
        //
        // 내보내는 이벤트 (상단 AI 서버의 규격을 그대로 따른다)
        //   event: delta  data: {"text":"..."}                        답변 조각
        //   event: done   data: {"chatSesId":..., "userMessage":..., 전송 종료 + 저장 결과
        //                        "assistantMessage":..., "contextSteps":[...]}
        //   event: error  data: {"code":"...","message":"..."}        오류
        // ─────────────────────────────────────────────────────────────
        [HttpPost("SendStream")]
        public async Task SendStream([FromBody] LlmChatSendDto dto, CancellationToken ct)
        {
            // ── 흘려보내기 전에 끝내야 하는 검사들 ──
            var prep = await PrepareAsync(dto, ct);
            if (prep.Error != null)
            {
                Response.StatusCode = prep.Error.StatusCode;
                await Response.WriteAsJsonAsync(
                    new { success = false, message = prep.Error.Message }, ct);
                return;
            }

            // ── SSE 로 응답한다고 알린다 ──
            Response.StatusCode = 200;
            Response.ContentType = "text/event-stream; charset=utf-8";
            Response.Headers["Cache-Control"] = "no-cache";
            // nginx/IIS 같은 중간 서버가 응답을 모아 두었다가 한꺼번에 보내면
            // 스트리밍이 의미가 없어진다. 모으지 말라고 알린다.
            Response.Headers["X-Accel-Buffering"] = "no";

            // ASP.NET Core 자체 버퍼도 꺼야 조각이 즉시 나간다
            var bodyFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();
            bodyFeature?.DisableBuffering();

            var answer = new StringBuilder();
            var elapsedMs = 0;
            string? warning = null;
            string? errCntn = null;

            // AI 서버가 답을 만들지 않고 되돌려보내는 경우가 있다.
            // 그 문구가 화면에 잠깐 스쳤다가 바뀌면 보기 나쁘므로,
            // 판별에 필요한 만큼(안내 문구의 앞부분 길이)만 조각을 붙들어 두었다가
            // 되돌려보낸 것이 아님이 확인되면 그때 한꺼번에 내보낸다.
            // 붙들어 두는 양이 스무 글자 남짓이라 체감되는 지연은 없다.
            var hold = new StringBuilder();
            var decided = false;   // 되돌려보낸 것인지 판단이 끝났는가
            var rejected = false;  // 되돌려보낸 응답인가

            async Task FeedAsync(string text)
            {
                if (rejected) return; // 되돌려보낸 문구는 화면에 내보내지 않는다

                if (decided)
                {
                    answer.Append(text);
                    await WriteEventAsync("delta", new { text }, ct);
                    return;
                }

                hold.Append(text);
                var sofar = hold.ToString();

                if (!EcoAiRejection.MayBeRejection(sofar))
                {
                    // 안내 문구와 다르다 → 정상 답변이다. 붙들어 둔 것을 한꺼번에 내보낸다.
                    decided = true;
                    answer.Append(sofar);
                    await WriteEventAsync("delta", new { text = sofar }, ct);
                    hold.Clear();
                }
                else if (sofar.TrimStart().Length >= EcoAiRejection.MarkerLength)
                {
                    // 안내 문구와 끝까지 같다 → 되돌려보낸 응답이다. 내보내지 않는다.
                    decided = true;
                    rejected = true;
                    hold.Clear();
                }
                // 아직 판단할 만큼 모이지 않았으면 계속 붙들어 둔다
            }

            try
            {
                await foreach (var ev in _chat.StreamAsync(prep.ComposedQuestion!, ct))
                {
                    switch (ev.Kind)
                    {
                        case EcoAiEventKind.Delta:
                            await FeedAsync(ev.Text ?? "");
                            break;

                        case EcoAiEventKind.Done:
                            elapsedMs = ev.ElpsMsVal;
                            warning = ev.Warning;
                            break;

                        case EcoAiEventKind.Error:
                            errCntn = $"{ev.Code}: {ev.Message}";
                            await WriteEventAsync("error",
                                new { code = ev.Code, message = ev.Message }, ct);
                            break;
                    }
                }

                // 스트림이 끝났는데 아직 붙들고 있던 조각이 있으면 내보낸다
                // (답변이 안내 문구 길이보다 짧게 끝난 경우)
                if (!decided && hold.Length > 0)
                {
                    var rest = hold.ToString();
                    if (EcoAiRejection.IsRejection(rest))
                    {
                        rejected = true;
                    }
                    else
                    {
                        answer.Append(rest);
                        await WriteEventAsync("delta", new { text = rest }, ct);
                    }
                    hold.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                // 사용자가 화면을 닫거나 새로고침한 경우. 여기까지 받은 답변은 아래에서 저장한다.
                errCntn = "client disconnected";
            }
            catch (Exception ex)
            {
                errCntn = $"{ex.GetType().Name}: {ex.Message}";
                await WriteEventAsync("error",
                    new { code = "INTERNAL_ERROR", message = "AI 응답 중 오류가 발생했습니다." },
                    CancellationToken.None);
            }

            // ── AI 서버가 되돌려보냈으면 우리 DB 값으로 직접 답한다 ──
            // 이 물음들의 답은 이미 우리 DB 에 있다. 지어내는 것이 아니라 저장된 값을 옮겨 적는 것이다.
            var answeredLocally = false;
            if (rejected || (answer.Length == 0 && errCntn == null))
            {
                var local = LocalAnswerBuilder.Build(prep.WorkContext!);
                answer.Clear();
                answer.Append(local);
                answeredLocally = true;
                errCntn = "answered locally (AI 서버가 질문을 되돌려보냄)";

                // 붙들어 두느라 아직 아무것도 안 나갔으므로 여기서 한 번에 내보낸다
                await WriteEventAsync("delta", new { text = local }, CancellationToken.None);
            }
            else if (answer.Length == 0)
            {
                // 오류로 한 글자도 못 받은 경우. 화면이 비지 않게 안내 문구를 남긴다.
                answer.Append("AI 응답 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요.");
                await WriteEventAsync("delta",
                    new { text = answer.ToString() }, CancellationToken.None);
            }

            // ── 대화를 남긴다 ──
            // 도중에 끊겼더라도 여기까지 받은 답변은 기록한다. 이력이 통째로 비는 편보다 낫다.
            // 사용자가 연결을 끊었을 수 있으므로 저장에는 취소 토큰을 쓰지 않는다.
            var saved = await SaveTurnAsync(
                prep, answer.ToString().Trim(), elapsedMs, warning, errCntn, CancellationToken.None,
                answeredLocally: answeredLocally);

            // ── 마무리 이벤트 ──
            // 화면이 임시로 띄워 둔 말풍선을 실제 기록으로 바꿔 넣을 수 있도록
            // 저장된 메시지와 참조한 작업 단계를 함께 실어 보낸다.
            await WriteEventAsync("done", new
            {
                chatSesId = prep.Session!.ChatSesId,
                userMessage = ToMsgDto(saved.UserMsg),
                assistantMessage = ToMsgDto(saved.AiMsg),
                elpsMsVal = elapsedMs,
                warning,
                answeredLocally,
                contextSteps = prep.WorkContext!.Steps
            }, CancellationToken.None);
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/Send
        // 질문을 보내고 완성된 답변 1건을 받는다.
        // 스트리밍이 막히는 환경(응답을 모아 두는 중간 서버 등)을 위한 대체 경로.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] LlmChatSendDto dto, CancellationToken ct)
        {
            var prep = await PrepareAsync(dto, ct);
            if (prep.Error != null)
            {
                return StatusCode(prep.Error.StatusCode,
                    new { success = false, message = prep.Error.Message });
            }

            var answer = await _chat.AskAsync(prep.ComposedQuestion!, ct);

            // AI 서버가 답을 만들지 않고 되돌려보냈으면, 우리 DB 값으로 직접 답을 만든다.
            var answerText = answer.Answer;
            var localUsed = false;
            if (EcoAiRejection.IsRejection(answerText))
            {
                answerText = LocalAnswerBuilder.Build(prep.WorkContext!);
                localUsed = true;
            }

            var saved = await SaveTurnAsync(
                prep, answerText, answer.ElpsMsVal, answer.Warning,
                localUsed ? "answered locally (AI 서버가 질문을 되돌려보냄)" : answer.ErrCntn, ct,
                answeredLocally: localUsed);

            return Ok(new
            {
                success = answer.Success || localUsed,
                data = new LlmChatAnswerDto
                {
                    ChatSesId = prep.Session!.ChatSesId,
                    UserMessage = ToMsgDto(saved.UserMsg),
                    AssistantMessage = ToMsgDto(saved.AiMsg),
                    AiModelNm = localUsed ? "ecopack-local" : answer.AiModelNm,
                    ElpsMsVal = answer.ElpsMsVal,
                    Warning = answer.Warning,
                    AnsweredLocally = localUsed,
                    ContextSteps = prep.WorkContext!.Steps
                },
                message = (answer.Success || localUsed)
                    ? null
                    : "AI 응답에 문제가 있어 안내 문구를 대신 표시합니다."
            });
        }

        // ═════════════════════════════════════════════════════════════
        // Send 와 SendStream 이 함께 쓰는 준비 · 저장 로직
        // ═════════════════════════════════════════════════════════════

        private record PrepError(int StatusCode, string Message);

        /// <summary>질문을 보내기 직전까지의 준비 결과</summary>
        private class PrepResult
        {
            /// <summary>검사에 걸렸으면 채워진다. 채워져 있으면 나머지는 비어 있다.</summary>
            public PrepError? Error { get; set; }

            public LlmChatSession? Session { get; set; }
            public ProjectWorkContext? WorkContext { get; set; }

            /// <summary>사용자가 입력한 원래 질문. DB 와 화면에는 이것이 남는다.</summary>
            public string? Question { get; set; }

            /// <summary>작업상태·직전대화를 합쳐 AI 서버로 보낼 글</summary>
            public string? ComposedQuestion { get; set; }

            public string? RepCustId { get; set; }
            public string? PrjId { get; set; }
            public string? PackLevel { get; set; }
            public string? CurMenuId { get; set; }
        }

        /// <summary>
        /// 입력값 확인 → 소유자 확인 → 세션 확보 → 작업상태 수집 → 직전 대화 수집 →
        /// AI 서버에 보낼 글 조립까지를 한 번에 한다.
        /// 스트리밍은 첫 바이트를 쓰기 전에 이 과정을 모두 끝내야 하므로 따로 떼어 두었다.
        /// </summary>
        private async Task<PrepResult> PrepareAsync(LlmChatSendDto dto, CancellationToken ct)
        {
            var r = new PrepResult();

            if (string.IsNullOrWhiteSpace(dto?.RepCustId))
            {
                r.Error = new PrepError(400, "로그인 정보(repCustId)가 필요합니다.");
                return r;
            }

            var question = (dto.Question ?? "").Trim();
            if (question.Length == 0)
            {
                r.Error = new PrepError(400, "질문을 입력해 주세요.");
                return r;
            }
            if (question.Length > MaxQuestionLength)
            {
                r.Error = new PrepError(400, $"질문은 {MaxQuestionLength}자를 넘을 수 없습니다.");
                return r;
            }

            // ⓐ 프로젝트를 함께 넘겼다면 본인 것인지 확인한다
            var prjId = Blank(dto.PrjId);
            if (prjId != null && !await ProjectAccess.IsOwnerAsync(_context, prjId, dto.RepCustId))
            {
                r.Error = new PrepError(403, ProjectAccess.DeniedMessage);
                return r;
            }

            // 세션 확보. 화면이 세션 ID 를 안 보냈거나 그 사이 백업으로 넘어갔으면 새로 연다.
            var session = string.IsNullOrWhiteSpace(dto.ChatSesId)
                ? null
                : await _context.LlmChatSession
                    .FirstOrDefaultAsync(s => s.ChatSesId == dto.ChatSesId && s.RepCustId == dto.RepCustId, ct);

            if (session == null)
            {
                session = await _context.LlmChatSession
                    .Where(s => s.RepCustId == dto.RepCustId && s.ChatSesStatCd == "ACTIVE")
                    .OrderByDescending(s => s.FrstCrtDtm)
                    .FirstOrDefaultAsync(ct);
            }

            if (session == null)
            {
                session = new LlmChatSession
                {
                    ChatSesId = "S" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    RepCustId = dto.RepCustId,
                    PrjId = prjId,
                    PackLevel = Blank(dto.PackLevel),
                    ChatSesStatCd = "ACTIVE",
                    LgnDtm = DateTime.Now,
                    FrstCrtDtm = DateTime.Now,
                    LastUpdDtm = DateTime.Now
                };
                _context.LlmChatSession.Add(session);
                await _context.SaveChangesAsync(ct);
            }

            // ⓑ 프로젝트 생성부터 지금까지의 전 과정 작업 상태를 모은다
            var workContext = await ProjectContextBuilder.BuildAsync(
                _context, dto.RepCustId, prjId, Blank(dto.PackLevel), Blank(dto.CurMenuId), ct);

            // ⓒ 같은 세션의 직전 대화를 꺼낸다 (최근 것부터 N개 → 시간순으로 되돌림)
            var recent = await _context.LlmChatMsg
                .AsNoTracking()
                .Where(m => m.ChatSesId == session.ChatSesId)
                .OrderByDescending(m => m.ChatMsgSeq)
                .Take(_opt.HistoryTurns)
                .ToListAsync(ct);

            var history = recent
                .OrderBy(m => m.ChatMsgSeq)
                .Where(m => !string.IsNullOrWhiteSpace(m.ChatMsgCntn))
                .Select(m => new ChatTurn(m.ChatRoleCd, m.ChatMsgCntn!))
                .ToList();

            // ⓓ AI 서버는 question 한 항목만 받으므로, 위 재료를 글 하나로 합친다
            r.Session = session;
            r.WorkContext = workContext;
            r.Question = question;
            r.ComposedQuestion = ChatPromptBuilder.Build(question, workContext.Text, history, _opt);
            r.RepCustId = dto.RepCustId;
            r.PrjId = prjId;
            r.PackLevel = Blank(dto.PackLevel);
            r.CurMenuId = Blank(dto.CurMenuId);
            return r;
        }

        private record SavedTurn(LlmChatMsg UserMsg, LlmChatMsg AiMsg);

        /// <summary>ⓔ 질문과 답을 각각 한 줄씩 남기고 세션 정보를 갱신한다.</summary>
        private async Task<SavedTurn> SaveTurnAsync(
            PrepResult prep, string answerText, int elapsedMs,
            string? warning, string? errCntn, CancellationToken ct,
            bool answeredLocally = false)
        {
            var session = prep.Session!;

            var nextSeq = await _context.LlmChatMsg
                .Where(m => m.ChatSesId == session.ChatSesId)
                .MaxAsync(m => (int?)m.ChatMsgSeq, ct) ?? 0;

            var now = DateTime.Now;

            var userMsg = new LlmChatMsg
            {
                ChatSesId = session.ChatSesId,
                RepCustId = prep.RepCustId!,
                ChatMsgSeq = ++nextSeq,
                ChatRoleCd = "user",
                // 합쳐 보낸 긴 글이 아니라 사용자가 입력한 원래 질문을 남긴다
                ChatMsgCntn = prep.Question,
                PrjId = prep.PrjId,
                PackLevel = prep.PackLevel,
                CurMenuId = prep.CurMenuId,
                WrkStatSnpsCntn = SafeSerialize(prep.WorkContext!.Snapshot),
                FrstCrtDtm = now
            };

            // 서버가 부분 실패를 알려 왔으면(warning) 오류 칸에 함께 남겨 나중에 원인을 찾을 수 있게 한다
            var err = errCntn;
            if (!string.IsNullOrWhiteSpace(warning))
            {
                err = string.IsNullOrWhiteSpace(err) ? $"warning: {warning}" : $"{err} / warning: {warning}";
            }

            var aiMsg = new LlmChatMsg
            {
                ChatSesId = session.ChatSesId,
                RepCustId = prep.RepCustId!,
                ChatMsgSeq = ++nextSeq,
                ChatRoleCd = "assistant",
                ChatMsgCntn = answerText,
                PrjId = prep.PrjId,
                PackLevel = prep.PackLevel,
                CurMenuId = prep.CurMenuId,
                // 어느 쪽이 답했는지 남겨 둔다. 나중에 로컬 답변 비율을 세어 볼 수 있다.
                AiModelNm = answeredLocally ? "ecopack-local" : _chat.ServiceNm,
                ElpsMsVal = elapsedMs,
                ErrCntn = err,
                FrstCrtDtm = now
            };

            _context.LlmChatMsg.AddRange(userMsg, aiMsg);

            // 세션 정보 갱신. 제목이 비어 있으면 첫 질문으로 만든다.
            if (string.IsNullOrWhiteSpace(session.ChatSesNm))
            {
                var q = prep.Question!;
                session.ChatSesNm = q.Length > 60 ? q[..60] + "…" : q;
            }
            session.ChatMsgCnt += 2;
            session.LastMsgDtm = now;
            session.LastUpdDtm = now;
            // 대화 도중 프로젝트를 열었다면 세션에도 반영해 둔다
            if (prep.PrjId != null)
            {
                session.PrjId = prep.PrjId;
                session.PackLevel = prep.PackLevel;
            }

            await _context.SaveChangesAsync(ct);
            return new SavedTurn(userMsg, aiMsg);
        }

        /// <summary>SSE 이벤트 한 개를 내보내고 즉시 밀어 낸다.</summary>
        private async Task WriteEventAsync(string eventName, object data, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(data, SseJsonOpt);
            // 명세대로 "event:" / "data:" 두 줄을 쓰고 빈 줄로 이벤트를 끝낸다
            await Response.WriteAsync($"event: {eventName}\ndata: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/LlmChat/GetSessions?repCustId=xxx&includeArchived=true
        // 지난 대화 목록. 백업으로 넘어간 세션도 함께 볼 수 있다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetSessions")]
        public async Task<IActionResult> GetSessions(
            [FromQuery] string? repCustId,
            [FromQuery] bool includeArchived = true,
            [FromQuery] int take = 50)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }
            if (take is < 1 or > 200) take = 50;

            var live = await _context.LlmChatSession
                .AsNoTracking()
                .Where(s => s.RepCustId == repCustId)
                .OrderByDescending(s => s.LastMsgDtm ?? s.FrstCrtDtm)
                .Take(take)
                .ToListAsync();

            var list = live.Select(s => ToSessionDto(s, archived: false)).ToList();

            if (includeArchived)
            {
                var archived = await _context.LlmChatSessionArch
                    .AsNoTracking()
                    .Where(s => s.RepCustId == repCustId)
                    .OrderByDescending(s => s.LastMsgDtm ?? s.FrstCrtDtm)
                    .Take(take)
                    .ToListAsync();

                list.AddRange(archived.Select(s => new LlmChatSessionDto
                {
                    ChatSesId = s.ChatSesId,
                    ChatSesNm = s.ChatSesNm,
                    ChatSesStatCd = s.ChatSesStatCd,
                    PrjId = s.PrjId,
                    PackLevel = s.PackLevel,
                    ChatMsgCnt = s.ChatMsgCnt,
                    LastMsgDtm = s.LastMsgDtm,
                    FrstCrtDtm = s.FrstCrtDtm ?? DateTime.MinValue,
                    Archived = true
                }));
            }

            list = list
                .OrderByDescending(s => s.LastMsgDtm ?? s.FrstCrtDtm)
                .Take(take)
                .ToList();

            return Ok(new { success = true, data = list });
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/LlmChat/GetHistory?chatSesId=xxx&repCustId=xxx
        // 세션 하나의 전체 대화. 백업된 세션이면 백업 테이블에서 읽어 온다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetHistory")]
        public async Task<IActionResult> GetHistory([FromQuery] string chatSesId, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }
            if (string.IsNullOrWhiteSpace(chatSesId))
            {
                return BadRequest(new { success = false, message = "세션 ID가 필요합니다." });
            }

            // 소유자가 아니면 애초에 검색되지 않으므로, 없으면 404 로 끝난다
            var session = await _context.LlmChatSession
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ChatSesId == chatSesId && s.RepCustId == repCustId);

            if (session != null)
            {
                var messages = await _context.LlmChatMsg
                    .AsNoTracking()
                    .Where(m => m.ChatSesId == chatSesId)
                    .OrderBy(m => m.ChatMsgSeq)
                    .Select(m => ToMsgDto(m))
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new LlmChatHistoryDto { Session = ToSessionDto(session, archived: false), Messages = messages }
                });
            }

            // 살아 있는 세션에 없으면 백업 쪽을 본다
            var arch = await _context.LlmChatSessionArch
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ChatSesId == chatSesId && s.RepCustId == repCustId);

            if (arch == null)
            {
                return NotFound(new { success = false, message = "대화를 찾을 수 없습니다." });
            }

            var archMessages = await _context.LlmChatMsgArch
                .AsNoTracking()
                .Where(m => m.ChatSesId == chatSesId)
                .OrderBy(m => m.ChatMsgSeq)
                .Select(m => new LlmChatMsgDto
                {
                    ChatMsgId = m.ChatMsgId,
                    ChatMsgSeq = m.ChatMsgSeq,
                    ChatRoleCd = m.ChatRoleCd,
                    ChatMsgCntn = m.ChatMsgCntn,
                    PrjId = m.PrjId,
                    PackLevel = m.PackLevel,
                    CurMenuId = m.CurMenuId,
                    ErrCntn = m.ErrCntn,
                    FrstCrtDtm = m.FrstCrtDtm ?? DateTime.MinValue
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new LlmChatHistoryDto
                {
                    Session = new LlmChatSessionDto
                    {
                        ChatSesId = arch.ChatSesId,
                        ChatSesNm = arch.ChatSesNm,
                        ChatSesStatCd = arch.ChatSesStatCd,
                        PrjId = arch.PrjId,
                        PackLevel = arch.PackLevel,
                        ChatMsgCnt = arch.ChatMsgCnt,
                        LastMsgDtm = arch.LastMsgDtm,
                        FrstCrtDtm = arch.FrstCrtDtm ?? DateTime.MinValue,
                        Archived = true
                    },
                    Messages = archMessages
                }
            });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/Archive
        // 2일 지난 대화 이력을 지금 바로 백업한다(관리자용 수동 실행).
        // 평소에는 LlmChatArchiveHostedService 가 주기적으로 알아서 돈다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("Archive")]
        public async Task<IActionResult> Archive(CancellationToken ct)
        {
            var result = await _archive.RunAsync(ct);
            return Ok(new
            {
                success = string.IsNullOrEmpty(result.ErrCntn),
                data = result,
                message = $"기준일 {result.CutoffDtm:yyyy-MM-dd} 이전 대화 {result.ArchivedSessions}건(메시지 {result.ArchivedMessages}건)을 백업했습니다."
            });
        }

        // ═════════════════════════════════════════════════════════════
        // 헬퍼
        // ═════════════════════════════════════════════════════════════

        /// <summary>화면이 빈 문자열을 보내는 경우가 많아, 값 없음으로 바꿔 준다.</summary>
        private static string? Blank(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        /// <summary>스냅샷 직렬화. 실패해도 대화 저장 자체를 막지 않는다.</summary>
        private static string? SafeSerialize(object? snapshot)
        {
            if (snapshot == null) return null;
            try
            {
                return JsonSerializer.Serialize(snapshot, SnapshotJsonOpt);
            }
            catch
            {
                return null;
            }
        }

        private static LlmChatMsgDto ToMsgDto(LlmChatMsg m) => new()
        {
            ChatMsgId = m.ChatMsgId,
            ChatMsgSeq = m.ChatMsgSeq,
            ChatRoleCd = m.ChatRoleCd,
            ChatMsgCntn = m.ChatMsgCntn,
            PrjId = m.PrjId,
            PackLevel = m.PackLevel,
            CurMenuId = m.CurMenuId,
            ErrCntn = m.ErrCntn,
            FrstCrtDtm = m.FrstCrtDtm
        };

        private static LlmChatSessionDto ToSessionDto(LlmChatSession s, bool archived) => new()
        {
            ChatSesId = s.ChatSesId,
            ChatSesNm = s.ChatSesNm,
            ChatSesStatCd = s.ChatSesStatCd,
            PrjId = s.PrjId,
            PackLevel = s.PackLevel,
            ChatMsgCnt = s.ChatMsgCnt,
            LastMsgDtm = s.LastMsgDtm,
            FrstCrtDtm = s.FrstCrtDtm,
            Archived = archived
        };
    }
}
