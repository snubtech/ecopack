using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 라이브러리 &gt; 환경규제 화면용 API. 대상 테이블은 if004(국가규제정보 목록).
    /// 라우트: api/NationRegulation
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NationRegulationController : ControllerBase
    {
        private readonly AppDbContext _context;

        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 5000;

        public NationRegulationController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/NationRegulation/GetFilters
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetFilters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var rows = await _context.If004
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.PackLevel, x.PackLevelNm,
                        x.AppliedMaterial, x.AppliedMaterialNm,
                        x.CountryCode, x.CountryCodeNm
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

                var dto = new NationRegulationFiltersDto
                {
                    PackLevels       = Distinct(rows, x => x.PackLevel,       x => x.PackLevelNm),
                    AppliedMaterials = Distinct(rows, x => x.AppliedMaterial, x => x.AppliedMaterialNm),
                    Countries        = Distinct(rows, x => x.CountryCode,     x => x.CountryCodeNm)
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
        // GET: api/NationRegulation/GetList
        // pageSize 를 0 이하로 보내면 전체를 반환한다(엑셀 내려받기용).
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetList")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? packLevel,
            [FromQuery] string? appliedMaterial,
            [FromQuery] string? countryCode,
            [FromQuery] string? relatedReg,
            [FromQuery] string? regItem,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                var query = _context.If004.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(packLevel))       query = query.Where(x => x.PackLevel == packLevel);
                if (!string.IsNullOrWhiteSpace(appliedMaterial)) query = query.Where(x => x.AppliedMaterial == appliedMaterial);
                if (!string.IsNullOrWhiteSpace(countryCode))     query = query.Where(x => x.CountryCode == countryCode);

                if (!string.IsNullOrWhiteSpace(relatedReg))
                {
                    var r = relatedReg.Trim();
                    query = query.Where(x => x.RelatedReg != null && x.RelatedReg.Contains(r));
                }
                if (!string.IsNullOrWhiteSpace(regItem))
                {
                    var g = regItem.Trim();
                    query = query.Where(x => x.RegItem != null && x.RegItem.Contains(g));
                }

                var totalCount = await query.CountAsync();

                query = query
                    .OrderBy(x => x.CountryCodeNm)
                    .ThenBy(x => x.PackLevel)
                    .ThenBy(x => x.RelatedReg)
                    .ThenBy(x => x.Idx);

                var all = pageSize <= 0;
                if (!all)
                {
                    if (page < 1) page = 1;
                    if (pageSize > MaxPageSize) pageSize = MaxPageSize;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                var items = await query
                    .Select(x => new NationRegulationRowDto
                    {
                        Idx = x.Idx,
                        NatRegId = x.NatRegId,
                        PackLevelNm = x.PackLevelNm,
                        AppliedMaterialNm = x.AppliedMaterialNm,
                        CountryCodeNm = x.CountryCodeNm,
                        RelatedReg = x.RelatedReg,
                        RegItem = x.RegItem,
                        DtlCont = x.DtlCont,
                        UnitNm = x.UnitNm,
                        MinCont = x.MinCont,
                        MinOperatorNm = x.MinOperatorNm,
                        PrepDeadline = x.PrepDeadline,
                        PrepDeadlineEnd = x.PrepDeadlineEnd,
                        DecisionOut = x.DecisionOut,
                        IsRequired = x.IsRequired,
                        Memo = x.Memo,
                        OriginalText = x.OriginalText,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new NationRegulationPageDto
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
