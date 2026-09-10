/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - EcoAiChatService (생성형 AI 서버 호출)
 * ==============================================================================
 *
 * 1. 담당 범위
 *    - 사내 생성형 AI 서버(Swagger: {BaseUrl}/docs)의 챗봇 API 두 개를 부릅니다.
 *      · POST /api/chat        동기. 완성된 답변 1건을 받는다.
 *      · POST /api/chat_stream 스트리밍. SSE 로 답변 조각을 순차로 받는다.
 *
 * 2. 인증 (명세서 [접근정보])
 *    - 지금은 서버가 인증을 요구하지 않습니다(AuthType = None).
 *    - 다만 명세서에 "추후 필요 시 API Key 또는 Bearer Token 방식 적용 가능"이라 적혀 있어,
 *      나중에 서버가 인증을 켜더라도 코드를 고치지 않고 설정만 바꿔 대응할 수 있게 해 둡니다.
 *        AuthType = "ApiKey" → ApiKeyHeader 이름의 헤더에 값을 싣는다 (기본 X-API-Key)
 *        AuthType = "Bearer" → Authorization: Bearer {값}
 *      값은 설정(AuthValue)보다 환경변수 ECOPACK_LLM_API_KEY 를 쓰는 편이 안전합니다.
 *
 * 3. SSE 읽는 법 (명세서 [1-2. 질의응답(SSE 스트리밍)])
 *    - 이벤트는 빈 줄로 구분되고, 각 이벤트는 "event: 종류" 와 "data: {JSON}" 두 줄입니다.
 *    - delta : 답변 조각. 여러 번 온다.        data {"text": "..."}
 *    - done  : 마지막 1회.                      data {"elapsed_ms": 0, "warning": ""}
 *    - error : 처리 중 오류.                    data {"code": "...", "message": "..."}
 *    - 스트리밍이 시작된 뒤에는 HTTP 상태코드를 바꿀 수 없으므로,
 *      도중에 난 오류는 상태코드가 아니라 error 이벤트로 옵니다. 그래서 error 를 꼭 봐야 합니다.
 *
 * 4. 오류 (명세서 [4. 공통 오류 규격])
 *    - 서버는 {"detail": {"code": "...", "message": "..."}} 형태로 오류를 돌려줍니다.
 *      이 형태를 읽어 사용자에게 보여 줄 문구로 바꿉니다.
 *
 * 5. 실패해도 서버를 세우지 않는다
 *    - 연결 실패·시간 초과·오류 응답 모두 예외를 밖으로 던지지 않고,
 *      안내 문구가 담긴 결과로 돌려줍니다. 채팅창만 사과 문구를 띄우고 나머지 화면은 멀쩡합니다.
 * ==============================================================================
 */
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ecopack.Api.Support;

namespace ecopack.Api.Services
{
    /// <summary>동기 답변 한 건의 결과</summary>
    public class EcoAiChatResult
    {
        public string Answer { get; set; } = "";
        public string? AiModelNm { get; set; }
        public int ElpsMsVal { get; set; }

        /// <summary>서버가 준 부분 실패 안내. 답변이 정상이면 비어 있다.</summary>
        public string? Warning { get; set; }

        /// <summary>실패했을 때만 채워진다. 채워져 있으면 Answer 는 안내 문구다.</summary>
        public string? ErrCntn { get; set; }
        public bool Success => string.IsNullOrEmpty(ErrCntn);
    }

    /// <summary>스트리밍 도중 올라오는 이벤트</summary>
    public enum EcoAiEventKind { Delta, Done, Error }

    public class EcoAiChatEvent
    {
        public EcoAiEventKind Kind { get; set; }

        /// <summary>Delta 일 때의 답변 조각</summary>
        public string? Text { get; set; }

        /// <summary>Done 일 때의 소요 시간(ms)</summary>
        public int ElpsMsVal { get; set; }

        /// <summary>Done 일 때의 부분 실패 안내</summary>
        public string? Warning { get; set; }

        /// <summary>Error 일 때의 오류 코드</summary>
        public string? Code { get; set; }

        /// <summary>Error 일 때의 오류 문구</summary>
        public string? Message { get; set; }
    }

    public interface IEcoAiChatService
    {
        /// <summary>동기 호출. 완성된 답변 1건을 받는다.</summary>
        Task<EcoAiChatResult> AskAsync(string question, CancellationToken ct = default);

        /// <summary>스트리밍 호출. 답변 조각을 오는 대로 흘려보낸다.</summary>
        IAsyncEnumerable<EcoAiChatEvent> StreamAsync(string question, CancellationToken ct = default);

        /// <summary>대화 로그에 남길 서비스 이름</summary>
        string ServiceNm { get; }
    }

    /// <summary>
    /// AI 서버가 "답을 만들지 않고 되돌려보낸" 응답인지 가려낸다.
    ///
    /// 이 서버는 앞단에 질문을 거르는 장치가 있어서, 사용자의 프로젝트 상태를 묻는 물음을
    /// 답변 생성 없이 안내 문구 한 줄로 돌려보낼 때가 있다.
    /// (실측: 0.1초 만에 돌아온다. 생성에 실패한 것이 아니라 앞단에서 막힌 것이다)
    /// 이 경우 우리 DB 에 답이 있으므로 LocalAnswerBuilder 로 직접 답을 만들어 대신 내보낸다.
    /// </summary>
    public static class EcoAiRejection
    {
        /// <summary>돌려보낼 때 서버가 쓰는 문구의 앞부분</summary>
        private const string Marker = "이전 대화 내용을 찾을 수 없습니다";

        /// <summary>판별에 필요한 최소 글자 수. 스트리밍에서 이 길이만큼만 모아 보면 된다.</summary>
        public static int MarkerLength => Marker.Length;

        /// <summary>답변 전문이 되돌려보낸 문구인지</summary>
        public static bool IsRejection(string? answer) =>
            !string.IsNullOrWhiteSpace(answer)
            && answer.TrimStart().StartsWith(Marker, StringComparison.Ordinal);

        /// <summary>
        /// 지금까지 모은 조각이 되돌려보낸 문구의 시작과 같은지.
        /// 스트리밍에서 첫 조각들만 잠깐 붙들어 두고 판단할 때 쓴다.
        /// </summary>
        public static bool MayBeRejection(string sofar)
        {
            var t = sofar.TrimStart();
            var n = Math.Min(t.Length, Marker.Length);
            return string.CompareOrdinal(t, 0, Marker, 0, n) == 0;
        }
    }

    public class EcoAiChatService : IEcoAiChatService
    {
        /// <summary>HttpClient 이름. Program.cs 의 AddHttpClient 와 맞춰야 한다.</summary>
        public const string HttpClientName = "EcoAiChat";

        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _factory;
        private readonly LlmChatOptions _opt;
        private readonly ILogger<EcoAiChatService> _logger;

        public string ServiceNm => _opt.ServiceNm;

        public EcoAiChatService(
            IHttpClientFactory factory,
            IOptions<LlmChatOptions> options,
            ILogger<EcoAiChatService> logger)
        {
            _factory = factory;
            _opt = options.Value;
            _logger = logger;
        }

        // ═════════════════════════════════════════════════════════
        // 동기 호출 — POST /api/chat
        // ═════════════════════════════════════════════════════════
        public async Task<EcoAiChatResult> AskAsync(string question, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                using var req = BuildRequest(_opt.ChatPath, question, streaming: false);
                using var client = CreateClient();

                using var res = await client.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct);
                var body = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                {
                    var (code, msg) = ParseError(body);
                    _logger.LogWarning("AI 서버 오류 {Status} {Code} {Msg}", (int)res.StatusCode, code, msg);
                    return Failure(
                        UserFacing(code, msg),
                        $"HTTP {(int)res.StatusCode} {code}: {msg}",
                        sw);
                }

                var dto = JsonSerializer.Deserialize<ChatResponseDto>(body, JsonOpt);
                sw.Stop();

                var answer = (dto?.Answer ?? "").Trim();
                if (answer.Length == 0)
                {
                    return Failure("답변을 만들지 못했습니다. 질문을 조금 더 구체적으로 적어 다시 물어봐 주세요.",
                                   "empty answer", sw);
                }

                return new EcoAiChatResult
                {
                    Answer = answer,
                    AiModelNm = _opt.ServiceNm,
                    // 서버가 알려 준 소요 시간을 우선 쓰고, 없으면 우리가 잰 값을 쓴다
                    ElpsMsVal = dto!.ElapsedMs > 0 ? dto.ElapsedMs : (int)sw.ElapsedMilliseconds,
                    Warning = string.IsNullOrWhiteSpace(dto.Warning) ? null : dto.Warning
                };
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning("AI 응답이 {Sec}초 안에 오지 않아 중단했다.", _opt.TimeoutSeconds);
                return Failure("응답이 너무 오래 걸려 중단했습니다. 잠시 후 다시 시도해 주세요.",
                               $"timeout after {_opt.TimeoutSeconds}s", sw);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AI 서버에 연결하지 못했다. {Url}", _opt.BaseUrl);
                return Failure("AI 서버에 연결할 수 없습니다. 잠시 후 다시 시도해 주세요.",
                               $"connect failed: {ex.Message}", sw);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 응답 처리 중 오류");
                return Failure("AI 응답 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요.",
                               $"{ex.GetType().Name}: {ex.Message}", sw);
            }
        }

        // ═════════════════════════════════════════════════════════
        // 스트리밍 호출 — POST /api/chat_stream (SSE)
        // ═════════════════════════════════════════════════════════
        public async IAsyncEnumerable<EcoAiChatEvent> StreamAsync(
            string question,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            HttpClient? client = null;
            HttpRequestMessage? req = null;
            HttpResponseMessage? res = null;
            Stream? stream = null;

            // 스트림을 여는 단계에서 난 오류는 error 이벤트 하나로 바꿔 내보낸다.
            // (yield 는 try/catch 안에서 쓸 수 없어 열기와 읽기를 나눠 둔다)
            EcoAiChatEvent? openError = null;
            try
            {
                client = CreateClient();
                req = BuildRequest(_opt.ChatStreamPath, question, streaming: true);

                res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!res.IsSuccessStatusCode)
                {
                    var body = await res.Content.ReadAsStringAsync(ct);
                    var (code, msg) = ParseError(body);
                    _logger.LogWarning("AI 스트리밍 시작 실패 {Status} {Code} {Msg}", (int)res.StatusCode, code, msg);
                    openError = new EcoAiChatEvent
                    {
                        Kind = EcoAiEventKind.Error,
                        Code = code,
                        Message = UserFacing(code, msg)
                    };
                }
                else
                {
                    stream = await res.Content.ReadAsStreamAsync(ct);
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                openError = new EcoAiChatEvent
                {
                    Kind = EcoAiEventKind.Error,
                    Code = "TIMEOUT",
                    Message = "응답이 너무 오래 걸려 중단했습니다. 잠시 후 다시 시도해 주세요."
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AI 서버에 연결하지 못했다. {Url}", _opt.BaseUrl);
                openError = new EcoAiChatEvent
                {
                    Kind = EcoAiEventKind.Error,
                    Code = "CONNECT_FAILED",
                    Message = "AI 서버에 연결할 수 없습니다. 잠시 후 다시 시도해 주세요."
                };
            }

            if (openError != null)
            {
                res?.Dispose(); req?.Dispose(); client?.Dispose();
                yield return openError;
                yield break;
            }

            // ── 여기서부터 SSE 를 한 줄씩 읽는다 ────────────────────
            try
            {
                using var reader = new StreamReader(stream!, Encoding.UTF8);

                string? eventName = null;
                var dataBuf = new StringBuilder();

                while (!ct.IsCancellationRequested)
                {
                    string? line;
                    EcoAiChatEvent? readError = null;
                    try
                    {
                        line = await reader.ReadLineAsync(ct);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        // 이미 흘려보낸 조각은 살아 있으므로, 끊긴 사실만 알리고 끝낸다
                        _logger.LogWarning(ex, "AI 스트리밍이 도중에 끊겼다.");
                        readError = new EcoAiChatEvent
                        {
                            Kind = EcoAiEventKind.Error,
                            Code = "STREAM_BROKEN",
                            Message = "답변을 받는 도중 연결이 끊겼습니다."
                        };
                        line = null;
                    }

                    if (readError != null) { yield return readError; break; }
                    if (line == null) break; // 스트림 종료

                    // 빈 줄 = 이벤트 하나가 끝났다는 뜻
                    if (line.Length == 0)
                    {
                        if (eventName != null && dataBuf.Length > 0)
                        {
                            var parsed = ToEvent(eventName, dataBuf.ToString());
                            if (parsed != null)
                            {
                                yield return parsed;
                                if (parsed.Kind != EcoAiEventKind.Delta) yield break; // done / error 면 끝
                            }
                        }
                        eventName = null;
                        dataBuf.Clear();
                        continue;
                    }

                    if (line.StartsWith("event:", StringComparison.Ordinal))
                    {
                        eventName = line[6..].Trim();
                    }
                    else if (line.StartsWith("data:", StringComparison.Ordinal))
                    {
                        // data 가 여러 줄로 나뉘어 올 수 있어 이어 붙인다
                        if (dataBuf.Length > 0) dataBuf.Append('\n');
                        dataBuf.Append(line[5..].TrimStart());
                    }
                    // ":" 로 시작하는 주석 줄 등 나머지는 무시한다
                }
            }
            finally
            {
                stream?.Dispose();
                res?.Dispose();
                req?.Dispose();
                client?.Dispose();
            }
        }

        // ═════════════════════════════════════════════════════════
        // 헬퍼
        // ═════════════════════════════════════════════════════════

        private HttpClient CreateClient()
        {
            var client = _factory.CreateClient(HttpClientName);
            client.BaseAddress = new Uri(_opt.BaseUrl.TrimEnd('/'));
            client.Timeout = TimeSpan.FromSeconds(_opt.TimeoutSeconds);
            return client;
        }

        /// <summary>요청 하나를 만든다. 인증 설정이 켜져 있으면 여기서 헤더를 붙인다.</summary>
        private HttpRequestMessage BuildRequest(string path, string question, bool streaming)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, path)
            {
                // 서버가 받는 항목은 question 하나뿐이다
                Content = new StringContent(
                    JsonSerializer.Serialize(new { question }),
                    Encoding.UTF8,
                    "application/json")
            };

            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(
                streaming ? "text/event-stream" : "application/json"));

            ApplyAuth(req);
            return req;
        }

        /// <summary>
        /// 인증 헤더를 붙인다.
        /// 현재 서버는 인증을 요구하지 않으므로 기본값(None)에서는 아무것도 붙이지 않는다.
        /// </summary>
        private void ApplyAuth(HttpRequestMessage req)
        {
            var type = (_opt.AuthType ?? "None").Trim();
            if (type.Equals("None", StringComparison.OrdinalIgnoreCase)) return;

            // 설정값이 비어 있으면 환경변수를 쓴다. 키를 소스에 두지 않기 위함이다.
            var value = !string.IsNullOrWhiteSpace(_opt.AuthValue)
                ? _opt.AuthValue
                : Environment.GetEnvironmentVariable("ECOPACK_LLM_API_KEY");

            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogWarning(
                    "AuthType 이 {Type} 인데 인증 값이 없다. LlmChat:AuthValue 또는 환경변수 ECOPACK_LLM_API_KEY 를 설정하라.",
                    type);
                return;
            }

            if (type.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", value);
            }
            else if (type.Equals("ApiKey", StringComparison.OrdinalIgnoreCase))
            {
                var header = string.IsNullOrWhiteSpace(_opt.ApiKeyHeader) ? "X-API-Key" : _opt.ApiKeyHeader;
                req.Headers.TryAddWithoutValidation(header, value);
            }
            else
            {
                _logger.LogWarning("알 수 없는 AuthType 이다: {Type}. None / ApiKey / Bearer 중 하나여야 한다.", type);
            }
        }

        /// <summary>SSE 이벤트 한 덩어리를 우리 형태로 바꾼다.</summary>
        private EcoAiChatEvent? ToEvent(string eventName, string data)
        {
            try
            {
                switch (eventName)
                {
                    case "delta":
                    {
                        var d = JsonSerializer.Deserialize<DeltaDto>(data, JsonOpt);
                        if (string.IsNullOrEmpty(d?.Text)) return null;
                        return new EcoAiChatEvent { Kind = EcoAiEventKind.Delta, Text = d.Text };
                    }
                    case "done":
                    {
                        var d = JsonSerializer.Deserialize<DoneDto>(data, JsonOpt);
                        return new EcoAiChatEvent
                        {
                            Kind = EcoAiEventKind.Done,
                            ElpsMsVal = d?.ElapsedMs ?? 0,
                            Warning = string.IsNullOrWhiteSpace(d?.Warning) ? null : d!.Warning
                        };
                    }
                    case "error":
                    {
                        var d = JsonSerializer.Deserialize<ErrorDto>(data, JsonOpt);
                        return new EcoAiChatEvent
                        {
                            Kind = EcoAiEventKind.Error,
                            Code = d?.Code,
                            Message = UserFacing(d?.Code, d?.Message)
                        };
                    }
                    default:
                        return null; // 명세에 없는 이벤트는 무시한다
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "SSE 이벤트를 읽지 못했다. event={Event}", eventName);
                return null;
            }
        }

        /// <summary>서버 오류 응답 {"detail":{"code","message"}} 을 읽는다.</summary>
        private static (string? Code, string? Message) ParseError(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("detail", out var detail)
                    && detail.ValueKind == JsonValueKind.Object)
                {
                    var code = detail.TryGetProperty("code", out var c) ? c.GetString() : null;
                    var msg = detail.TryGetProperty("message", out var m) ? m.GetString() : null;
                    return (code, msg);
                }
            }
            catch (JsonException)
            {
                // 형식이 다르면 아래 기본값으로 넘어간다
            }
            return (null, null);
        }

        /// <summary>오류 코드를 사용자에게 보여 줄 문구로 바꾼다.</summary>
        private static string UserFacing(string? code, string? serverMessage) => code switch
        {
            "CHAT_PROCESSING_FAILED" => "답변을 만드는 중 문제가 생겼습니다. 잠시 후 다시 시도해 주세요.",
            "INVALID_REQUEST"        => "질문 형식이 올바르지 않습니다. 내용을 확인해 주세요.",
            "INTERNAL_ERROR"         => "AI 서버에 문제가 생겼습니다. 잠시 후 다시 시도해 주세요.",
            _ => string.IsNullOrWhiteSpace(serverMessage)
                    ? "AI 응답 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요."
                    : serverMessage
        };

        private EcoAiChatResult Failure(string userMessage, string err, Stopwatch sw)
        {
            sw.Stop();
            return new EcoAiChatResult
            {
                Answer = userMessage,
                AiModelNm = _opt.ServiceNm,
                ElpsMsVal = (int)sw.ElapsedMilliseconds,
                ErrCntn = err
            };
        }

        // ── 서버 응답 형태 ────────────────────────────────────────
        private class ChatResponseDto
        {
            public string? Question { get; set; }
            public string? Answer { get; set; }

            /// <summary>서버가 elapsed_ms 로 내려 준다</summary>
            [System.Text.Json.Serialization.JsonPropertyName("elapsed_ms")]
            public int ElapsedMs { get; set; }

            public string? Warning { get; set; }
        }

        private class DeltaDto
        {
            public string? Text { get; set; }
        }

        private class DoneDto
        {
            [System.Text.Json.Serialization.JsonPropertyName("elapsed_ms")]
            public int ElapsedMs { get; set; }
            public string? Warning { get; set; }
        }

        private class ErrorDto
        {
            public string? Code { get; set; }
            public string? Message { get; set; }
        }
    }
}
