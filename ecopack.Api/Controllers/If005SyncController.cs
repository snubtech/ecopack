/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - If005SyncController
 * ==============================================================================
 *
 * 1. 하는 일
 *    - IF005(환경영향평가정보) 수집을 지금 바로 한 번 돌리는 수동 실행 API 입니다. (Run)
 *    - 평소에는 If005SyncHostedService 가 주기적으로 알아서 돕니다.
 *    - 테이블을 새로 만든 직후처럼 바로 데이터를 채워야 할 때 씁니다.
 * ==============================================================================
 */
using Microsoft.AspNetCore.Mvc;
using ecopack.Api.Services;

namespace ecopack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class If005SyncController : ControllerBase
    {
        private readonly IIf005SyncService _sync;

        public If005SyncController(IIf005SyncService sync)
        {
            _sync = sync;
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/If005Sync/Run
        // IF005 목록을 받아 if005 테이블을 통째로 교체한다(수동 실행).
        // ─────────────────────────────────────────────────────────────
        [HttpPost("Run")]
        public async Task<IActionResult> Run(CancellationToken ct)
        {
            var result = await _sync.RunAsync(ct);
            var success = string.IsNullOrEmpty(result.ErrCntn);
            return Ok(new
            {
                success,
                data = result,
                message = success
                    ? $"IF005 {result.FetchedCount}건을 받아 {result.SavedCount}건을 저장했습니다."
                    : result.ErrCntn
            });
        }
    }
}
