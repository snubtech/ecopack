using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 라이브러리 &gt; 소재물성 화면용 API. 대상 테이블은 if001(소재/물성 데이터 목록).
    /// 라우트: api/MaterialProperty
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialPropertyController : ControllerBase
    {
        private readonly AppDbContext _context;

        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 5000;

        public MaterialPropertyController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/MaterialProperty/GetFilters
        // 조회조건 셀렉트 7종을 한 번에 내려준다.
        // (화면 진입 시 요청 1번으로 끝내기 위해 개별 엔드포인트로 나누지 않음)
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetFilters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var rows = await _context.If001
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.PackLevel, x.PackLevelNm,
                        x.AppliedMaterial, x.AppliedMaterialNm,
                        x.MatUse, x.MatUseNm,
                        x.MatType, x.MatTypeNm,
                        x.MatForm, x.MatFormNm,
                        x.Item, x.ItemNm,
                        x.Unit, x.UnitNm
                    })
                    .ToListAsync();

                // 코드가 비어 있는 행은 셀렉트에서 제외하고, 코드 기준으로 중복을 제거한다.
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

                var dto = new MaterialPropertyFiltersDto
                {
                    PackLevels       = Distinct(rows, x => x.PackLevel,       x => x.PackLevelNm),
                    AppliedMaterials = Distinct(rows, x => x.AppliedMaterial, x => x.AppliedMaterialNm),
                    MatUses          = Distinct(rows, x => x.MatUse,          x => x.MatUseNm),
                    MatTypes         = Distinct(rows, x => x.MatType,         x => x.MatTypeNm),
                    MatForms         = Distinct(rows, x => x.MatForm,         x => x.MatFormNm),
                    Items            = Distinct(rows, x => x.Item,            x => x.ItemNm),
                    Units            = Distinct(rows, x => x.Unit,            x => x.UnitNm)
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
        // GET: api/MaterialProperty/GetList
        // 조회조건으로 걸러진 목록을 페이징해서 내려준다.
        // pageSize 를 0 이하로 보내면 전체를 반환한다(엑셀 내려받기용).
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetList")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? packLevel,
            [FromQuery] string? appliedMaterial,
            [FromQuery] string? matUse,
            [FromQuery] string? matType,
            [FromQuery] string? matForm,
            [FromQuery] string? item,
            [FromQuery] string? unit,
            [FromQuery] string? keywords,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                var query = _context.If001.AsNoTracking().AsQueryable();

                // 값이 비어 있는 조건(= '전체')은 건너뛴다
                if (!string.IsNullOrWhiteSpace(packLevel))       query = query.Where(x => x.PackLevel == packLevel);
                if (!string.IsNullOrWhiteSpace(appliedMaterial)) query = query.Where(x => x.AppliedMaterial == appliedMaterial);
                if (!string.IsNullOrWhiteSpace(matUse))          query = query.Where(x => x.MatUse == matUse);
                if (!string.IsNullOrWhiteSpace(matType))         query = query.Where(x => x.MatType == matType);
                if (!string.IsNullOrWhiteSpace(matForm))         query = query.Where(x => x.MatForm == matForm);
                if (!string.IsNullOrWhiteSpace(item))            query = query.Where(x => x.Item == item);
                if (!string.IsNullOrWhiteSpace(unit))            query = query.Where(x => x.Unit == unit);

                // 키워드: 화면에 보이는 텍스트 컬럼들을 대상으로 부분 일치 검색
                if (!string.IsNullOrWhiteSpace(keywords))
                {
                    var k = keywords.Trim();
                    query = query.Where(x =>
                        (x.AppliedMaterialNm != null && x.AppliedMaterialNm.Contains(k)) ||
                        (x.MatUseNm          != null && x.MatUseNm.Contains(k)) ||
                        (x.MatTypeNm         != null && x.MatTypeNm.Contains(k)) ||
                        (x.MatFormNm         != null && x.MatFormNm.Contains(k)) ||
                        (x.ItemNm            != null && x.ItemNm.Contains(k)) ||
                        (x.UnitNm            != null && x.UnitNm.Contains(k)) ||
                        (x.AcceptableRange   != null && x.AcceptableRange.Contains(k)));
                }

                var totalCount = await query.CountAsync();

                query = query
                    .OrderBy(x => x.PackLevel)
                    .ThenBy(x => x.AppliedMaterialNm)
                    .ThenBy(x => x.ItemNm)
                    .ThenBy(x => x.Idx);

                var all = pageSize <= 0;
                if (!all)
                {
                    if (page < 1) page = 1;
                    if (pageSize > MaxPageSize) pageSize = MaxPageSize;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                var items = await query
                    .Select(x => new MaterialPropertyRowDto
                    {
                        Idx = x.Idx,
                        MatPrtBasId = x.MatPrtBasId,
                        PackLevelNm = x.PackLevelNm,
                        AppliedMaterialNm = x.AppliedMaterialNm,
                        MatUseNm = x.MatUseNm,
                        MatTypeNm = x.MatTypeNm,
                        MatFormNm = x.MatFormNm,
                        ItemNm = x.ItemNm,
                        UnitNm = x.UnitNm,
                        AcceptableRange = x.AcceptableRange,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new MaterialPropertyPageDto
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
