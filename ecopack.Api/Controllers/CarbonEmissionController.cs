/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - CarbonEmissionController (라이브러리 > 탄소배출량)
 * ==============================================================================
 * 
 * 1. 담당 범위
 *    - 탄소배출량 화면이 쓰는 조회 API 입니다. 대상 테이블은 if005(환경영향평가정보) 입니다.
 * 
 * 2. GetFilters — 조회조건 셀렉트 목록
 *    - 포장차수·적용소재·소재의 구성 3종을 한 번에 돌려줍니다.
 *    - 포장차수는 이름순이 아니라 코드순(1 판매 → 2 그룹 → 3 운송)으로 정렬합니다.
 * 
 * 3. GetList — 목록 조회
 *    - 중량 당·단위당 탄소배출량(원료/제조/폐기/합계)과 물리적 인자, 원료물질 구성을 돌려줍니다.
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

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 라이브러리 &gt; 탄소배출량 화면용 API. 대상 테이블은 if005(환경영향평가정보 목록).
    /// 라우트: api/CarbonEmission
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CarbonEmissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 5000;

        public CarbonEmissionController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/CarbonEmission/GetFilters
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetFilters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var rows = await _context.If005
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.PackLevel, x.PackLevelNm,
                        x.AppliedMaterial, x.AppliedMaterialNm,
                        x.MatForm, x.MatFormNm
                    })
                    .ToListAsync();

                static List<CodeNameDto> Distinct<T>(IEnumerable<T> src, Func<T, string?> code, Func<T, string?> name) =>
                    src.Where(x => !string.IsNullOrWhiteSpace(code(x)))
                       .GroupBy(code)
                       .Select(g => new CodeNameDto
                       {
                           Code = g.Key,
                           Name = g.Select(name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? g.Key
                       })
                       .OrderBy(x => x.Name, StringComparer.CurrentCulture)
                       .ToList();

                var dto = new CarbonEmissionFiltersDto
                {
                    PackLevels       = Distinct(rows, x => x.PackLevel,       x => x.PackLevelNm),
                    AppliedMaterials = Distinct(rows, x => x.AppliedMaterial, x => x.AppliedMaterialNm),
                    MatForms         = Distinct(rows, x => x.MatForm,         x => x.MatFormNm)
                };

                // 포장차수는 코드 순(1 판매 → 2 그룹 → 3 운송)이 자연스럽다
                dto.PackLevels = dto.PackLevels.OrderBy(x => x.Code, StringComparer.Ordinal).ToList();

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "조회조건을 불러오는 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/CarbonEmission/GetList
        // pageSize 를 0 이하로 보내면 전체를 반환한다(엑셀 내려받기용).
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetList")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? packLevel,
            [FromQuery] string? appliedMaterial,
            [FromQuery] string? matForm,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                var query = _context.If005.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(packLevel))       query = query.Where(x => x.PackLevel == packLevel);
                if (!string.IsNullOrWhiteSpace(appliedMaterial)) query = query.Where(x => x.AppliedMaterial == appliedMaterial);
                if (!string.IsNullOrWhiteSpace(matForm))         query = query.Where(x => x.MatForm == matForm);

                var totalCount = await query.CountAsync();

                query = query
                    .OrderBy(x => x.PackLevel)
                    .ThenBy(x => x.AppliedMaterialNm)
                    .ThenBy(x => x.MatFormNm)
                    .ThenBy(x => x.Idx);

                var all = pageSize <= 0;
                if (!all)
                {
                    if (page < 1) page = 1;
                    if (pageSize > MaxPageSize) pageSize = MaxPageSize;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                var items = await query
                    .Select(x => new CarbonEmissionRowDto
                    {
                        Idx = x.Idx,
                        EnvImpAssId = x.EnvImpAssId,
                        PackLevelNm = x.PackLevelNm,
                        AppliedMaterialNm = x.AppliedMaterialNm,
                        MatFormNm = x.MatFormNm,
                        MassCo2Mat = x.MassCo2Mat,
                        MassCo2Proc = x.MassCo2Proc,
                        MassCo2Scrap = x.MassCo2Scrap,
                        MassCo2Sum = x.MassCo2Sum,
                        UnitCo2Mat = x.UnitCo2Mat,
                        UnitCo2Proc = x.UnitCo2Proc,
                        UnitCo2Scrap = x.UnitCo2Scrap,
                        UnitCo2Sum = x.UnitCo2Sum,
                        UnitCo2MgtVal = x.UnitCo2MgtVal,
                        AreaDensity = x.AreaDensity,
                        Density = x.Density,
                        MatCompCon = x.MatCompCon,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new CarbonEmissionPageDto
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
    }
}
