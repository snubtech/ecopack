/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - AuditLog (감사 로그)
 * ==============================================================================
 *
 * 1. 무엇을 남기나
 *    - 로그인 성공·실패·잠금, 회원가입, 회원정보·비밀번호 변경
 *    - 권한 거부(403), 요청 제한 초과(429)
 *    - 삭제 요청(DELETE)
 *    누가(고객 ID), 어디서(IP), 무엇을(주소·대상), 결과가 어땠는지를 한 줄씩 남긴다.
 *
 * 2. 어디에 남기나
 *    - App_Data/logs/audit-yyyyMMdd.log 에 하루 한 파일, 한 줄에 JSON 한 건.
 *      (폴더는 Audit:Directory 설정으로 바꿀 수 있다)
 *    - 서버 로그(ILogger)에도 같은 내용을 남긴다.
 *
 * 3. 남기지 않는 것
 *    - 비밀번호, 요청 본문은 남기지 않는다.
 *    - 값은 JSON 으로 적으므로 줄바꿈 등을 넣어 기록을 위조할 수 없다.
 *
 * 4. 알아둘 점
 *    - "누가"는 로그인 토큰에서 꺼낸 고객 ID 다. 로그인 전 사건(로그인 실패 등)은 입력한 아이디를 적는다.
 *    - 파일에 못 쓰더라도 요청 처리는 실패시키지 않는다.
 * ==============================================================================
 */
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ecopack.Api.Support
{
    public interface IAuditLog
    {
        /// <summary>감사 기록 한 건을 남긴다.</summary>
        /// <param name="eventName">사건 이름(예: LOGIN_FAIL, ACCESS_DENIED, DELETE)</param>
        /// <param name="actor">누가(고객 ID). 모르면 null</param>
        /// <param name="detail">무엇을(대상·사유 등)</param>
        void Write(HttpContext? http, string eventName, string? actor, object? detail = null);
    }

    public class AuditLog : IAuditLog
    {
        private static readonly object FileLock = new();
        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            // 한글을 \uXXXX 로 바꾸지 않아 사람이 바로 읽을 수 있게 한다(줄바꿈·따옴표는 여전히 이스케이프된다)
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly string _dir;
        private readonly ILogger<AuditLog> _logger;

        public AuditLog(IWebHostEnvironment env, IConfiguration config, ILogger<AuditLog> logger)
        {
            _dir = Path.Combine(env.ContentRootPath, config["Audit:Directory"] ?? Path.Combine("App_Data", "logs"));
            _logger = logger;
        }

        public void Write(HttpContext? http, string eventName, string? actor, object? detail = null)
        {
            var now = DateTime.Now;
            var line = JsonSerializer.Serialize(new
            {
                at = now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                @event = eventName,
                actor = string.IsNullOrWhiteSpace(actor) ? null : actor,
                ip = http?.Connection.RemoteIpAddress?.ToString(),
                method = http?.Request.Method,
                path = http?.Request.Path.Value,
                detail
            }, JsonOpt);

            _logger.LogInformation("AUDIT {Line}", line);

            try
            {
                lock (FileLock)
                {
                    Directory.CreateDirectory(_dir);
                    File.AppendAllText(Path.Combine(_dir, $"audit-{now:yyyyMMdd}.log"), line + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // 감사 파일을 못 써도 요청은 계속 처리한다. 서버 로그에는 이미 남아 있다.
                _logger.LogError(ex, "감사 로그 파일 쓰기 실패");
            }
        }
    }
}
