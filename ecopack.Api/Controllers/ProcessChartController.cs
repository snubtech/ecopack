/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ProcessChartController (라이브러리 > 공정도)
 * ==============================================================================
 * 
 * 1. 담당 범위
 *    - 공정도 화면이 쓰는 조회 API 입니다.
 *    - 목록은 if003(공정도 목록), 상세(이미지)는 if003a(한 건 조회)를 씁니다.
 * 
 * 2. GetFilters / GetList — 조회조건과 목록
 *    - 조회조건은 적용소재 한 종류이고, 템플릿명은 부분일치로 찾습니다.
 *    - 파일존재여부는 if003a 에 파일이 있는지로 계산합니다.
 *      이때 이미지 본문은 끌어오지 않고 ID 목록만 조회해 목록이 무거워지지 않게 했습니다.
 * 
 * 3. GetDetail — 공정도 이미지
 *    - 응답에 이미지가 실리므로 화면에서 행을 펼칠 때만 부릅니다.
 *    - memoImg 는 <img src="data:..."> 가 들어간 HTML 이라 그대로 내려보내지 않고,
 *      ImageDataUri 로 이미지 주소만 뽑아 전달합니다(HTML 주입 방지).
 * 
 * 4. 공통 규칙
 *    - 조회조건 중 비어 있는 값은 '전체' 로 보고 그 조건을 건너뜁니다.
 *    - GetList 는 pageSize 를 0 이하로 보내면 전체를 돌려줍니다(화면의 엑셀 내려받기용).
 *    - 조회 전용 화면이므로 등록·수정·삭제 기능은 두지 않았습니다.
 * ==============================================================================
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Support;

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 라이브러리 &gt; 공정도 화면용 API.
    /// 목록은 if003(패키징소재생산공정도정보 목록), 상세는 if003a(한 건 조회)를 사용한다.
    /// 라우트: api/ProcessChart
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessChartController : ControllerBase
    {
        private readonly AppDbContext _context;

        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 5000;

        public ProcessChartController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/ProcessChart/GetFilters
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetFilters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var rows = await _context.If003
                    .AsNoTracking()
                    .Select(x => new { x.AppliedMaterial, x.AppliedMaterialNm })
                    .ToListAsync();

                var dto = new ProcessChartFiltersDto
                {
                    AppliedMaterials = rows
                        .Where(x => !string.IsNullOrWhiteSpace(x.AppliedMaterial))
                        .GroupBy(x => x.AppliedMaterial)
                        .Select(g => new CodeNameDto
                        {
                            Code = g.Key,
                            Name = g.Select(x => x.AppliedMaterialNm).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? g.Key
                        })
                        .OrderBy(x => x.Name, StringComparer.CurrentCulture)
                        .ToList()
                };

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "조회조건을 불러오는 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/ProcessChart/GetList
        // pageSize 를 0 이하로 보내면 전체를 반환한다(엑셀 내려받기용).
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetList")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? subject,
            [FromQuery] string? appliedMaterial,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                var query = _context.If003.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(subject))
                {
                    var s = subject.Trim();
                    query = query.Where(x => x.Subject != null && x.Subject.Contains(s));
                }
                if (!string.IsNullOrWhiteSpace(appliedMaterial))
                {
                    query = query.Where(x => x.AppliedMaterial == appliedMaterial);
                }

                var totalCount = await query.CountAsync();

                query = query.OrderBy(x => x.AppliedMaterialNm).ThenBy(x => x.Subject).ThenBy(x => x.Idx);

                var all = pageSize <= 0;
                if (!all)
                {
                    if (page < 1) page = 1;
                    if (pageSize > MaxPageSize) pageSize = MaxPageSize;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                var rows = await query
                    .Select(x => new
                    {
                        x.Idx, x.PackMmftProcId, x.Subject, x.AppliedMaterialNm,
                        x.MatTypeNm, x.MatCompNm, x.MatFormNm, x.CreatedAt
                    })
                    .ToListAsync();

                // 파일존재여부는 if003a 에 파일이 있는지로 판단한다.
                // 이미지 본문(longtext)을 끌어오지 않도록 ID 목록만 조회한다.
                var ids = rows.Select(r => r.PackMmftProcId).ToList();
                var withFile = await _context.If003a
                    .AsNoTracking()
                    .Where(a => ids.Contains(a.PackMmftProcId)
                                && ((a.FileData != null && a.FileData != "") || (a.MemoImg != null && a.MemoImg != "")))
                    .Select(a => a.PackMmftProcId)
                    .Distinct()
                    .ToListAsync();
                var fileSet = withFile.ToHashSet(StringComparer.Ordinal);

                var items = rows.Select(x => new ProcessChartRowDto
                {
                    Idx = x.Idx,
                    PackMmftProcId = x.PackMmftProcId,
                    Subject = x.Subject,
                    AppliedMaterialNm = x.AppliedMaterialNm,
                    MatTypeNm = x.MatTypeNm,
                    MatCompNm = x.MatCompNm,
                    MatFormNm = x.MatFormNm,
                    FileExistYn = fileSet.Contains(x.PackMmftProcId) ? "Y" : "N",
                    CreatedAt = x.CreatedAt
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = new ProcessChartPageDto
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = all ? 1 : page,
                        PageSize = all ? totalCount : pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "목록 조회 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/ProcessChart/GetDetail?packMmftProcId=xxx
        // 공정도 이미지를 내려준다. 응답에 이미지가 실리므로 행을 펼칠 때만 호출한다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetDetail")]
        public async Task<IActionResult> GetDetail([FromQuery] string packMmftProcId)
        {
            if (string.IsNullOrWhiteSpace(packMmftProcId))
            {
                return BadRequest(new { success = false, message = "필수 파라미터(packMmftProcId)가 누락되었습니다." });
            }

            try
            {
                var a = await _context.If003a
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PackMmftProcId == packMmftProcId);

                if (a == null)
                {
                    return NotFound(new { success = false, message = "공정도 상세 정보를 찾을 수 없습니다." });
                }

                var dto = new ProcessChartDetailDto
                {
                    PackMmftProcId = a.PackMmftProcId,
                    Subject = a.Subject,
                    AppliedMaterialNm = a.AppliedMaterialNm,
                    MatTypeNm = a.MatTypeNm,
                    MatCompNm = a.MatCompNm,
                    MatFormNm = a.MatFormNm,
                    FileNm = a.FileNm,
                    FileImageUri = ImageDataUri.FromFile(a.FileNm, a.FileData),
                    MemoImageUris = ImageDataUri.ExtractAll(a.MemoImg)
                };

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "상세 조회 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

    }
}
