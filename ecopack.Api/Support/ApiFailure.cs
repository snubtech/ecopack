/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ApiFailure (오류 응답 공통 처리)
 * ==============================================================================
 *
 * 1. 왜 필요한가
 *    - 예전에는 컨트롤러마다 catch 에서 예외 문구(ex.Message)를 응답에 그대로 실었다.
 *      예외 문구에는 SQL, 테이블·컬럼 이름, 서버 경로 같은 내부 정보가 들어 있다.
 *    - 이제 예외 상세는 서버 로그에만 남기고, 화면에는 미리 정한 안내 문구만 돌려준다.
 *
 * 2. Failure(ex, message) — 컨트롤러의 catch 에서 쓴다
 *    - 입력값이 잘못된 경우(InputValidationException)는 400 과 그 안내 문구를 돌려준다.
 *    - 그 밖의 예외는 로그에 남기고 500 과 message 만 돌려준다.
 *
 * 3. catch 가 없는 곳에서 난 예외는 Program.cs 의 전역 예외 처리기가 같은 방식으로 처리한다.
 * ==============================================================================
 */
using Microsoft.AspNetCore.Mvc;

namespace ecopack.Api.Support
{
    /// <summary>
    /// 입력값이 잘못됐을 때 던지는 예외. Message 는 화면에 그대로 보여 줘도 되는 안내 문구여야 한다.
    /// </summary>
    public class InputValidationException : Exception
    {
        public InputValidationException(string message) : base(message) { }
    }

    public static class ApiFailure
    {
        /// <summary>서버 오류 시 화면에 보여 줄 기본 문구</summary>
        public const string DefaultMessage = "요청을 처리하는 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요.";

        /// <summary>
        /// 컨트롤러의 catch 에서 오류 응답을 만든다. 예외 상세는 로그로만 보내고 응답에는 싣지 않는다.
        /// </summary>
        public static IActionResult Failure(this ControllerBase controller, Exception ex, string message)
        {
            if (ex is InputValidationException)
            {
                return controller.BadRequest(new { success = false, message = ex.Message });
            }

            Logger(controller).LogError(ex, "{Path} 처리 중 오류 — {Message}",
                controller.HttpContext.Request.Path.Value, message);
            return controller.StatusCode(500, new { success = false, message });
        }

        /// <summary>응답은 그대로 두고 예외만 로그에 남길 때 쓴다(응답 형식이 따로 정해진 곳).</summary>
        public static void LogFailure(this ControllerBase controller, Exception ex, string message) =>
            Logger(controller).LogError(ex, "{Path} 처리 중 오류 — {Message}",
                controller.HttpContext.Request.Path.Value, message);

        private static ILogger Logger(ControllerBase controller) =>
            controller.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(controller.GetType());
    }
}
