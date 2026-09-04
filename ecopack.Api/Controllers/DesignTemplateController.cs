using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Support;

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 라이브러리 &gt; 디자인 템플릿 화면용 API.
    /// 목록은 if002(패키징디자인템플릿정보 목록), 상세는 if002a(한 건 조회)를 사용한다.
    /// 라우트: api/DesignTemplate
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DesignTemplateController : ControllerBase
    {
        private readonly AppDbContext _context;

        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 5000;

        public DesignTemplateController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/DesignTemplate/GetFilters
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetFilters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var rows = await _context.If002
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.PackLevel, x.PackLevelNm,
                        x.AppliedMaterial, x.AppliedMaterialNm,
                        x.MatType, x.MatTypeNm
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

                var dto = new DesignTemplateFiltersDto
                {
                    PackLevels       = Distinct(rows, x => x.PackLevel,       x => x.PackLevelNm),
                    AppliedMaterials = Distinct(rows, x => x.AppliedMaterial, x => x.AppliedMaterialNm),
                    MatTypes         = Distinct(rows, x => x.MatType,         x => x.MatTypeNm)
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
        // GET: api/DesignTemplate/GetList
        // pageSize 를 0 이하로 보내면 전체를 반환한다(엑셀 내려받기용).
        // 목록에는 이미지가 실리지 않도록 if002 만 조회한다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetList")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? subject,
            [FromQuery] string? packLevel,
            [FromQuery] string? appliedMaterial,
            [FromQuery] string? matType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                var query = _context.If002.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(subject))
                {
                    var s = subject.Trim();
                    query = query.Where(x => x.Subject != null && x.Subject.Contains(s));
                }
                if (!string.IsNullOrWhiteSpace(packLevel))       query = query.Where(x => x.PackLevel == packLevel);
                if (!string.IsNullOrWhiteSpace(appliedMaterial)) query = query.Where(x => x.AppliedMaterial == appliedMaterial);
                if (!string.IsNullOrWhiteSpace(matType))         query = query.Where(x => x.MatType == matType);

                var totalCount = await query.CountAsync();

                query = query
                    .OrderBy(x => x.PackLevel)
                    .ThenBy(x => x.AppliedMaterialNm)
                    .ThenBy(x => x.MatTypeNm)
                    .ThenBy(x => x.Idx);

                var all = pageSize <= 0;
                if (!all)
                {
                    if (page < 1) page = 1;
                    if (pageSize > MaxPageSize) pageSize = MaxPageSize;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                var items = await query
                    .Select(x => new DesignTemplateRowDto
                    {
                        Idx = x.Idx,
                        PackDsgnTplId = x.PackDsgnTplId,
                        PackLevelNm = x.PackLevelNm,
                        AppliedMaterialNm = x.AppliedMaterialNm,
                        MatTypeNm = x.MatTypeNm,
                        Subject = x.Subject,
                        DsgnTypeNm = x.DsgnTypeNm,
                        DsgnTypeCdVal = x.DsgnTypeCdVal,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new DesignTemplatePageDto
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
        // GET: api/DesignTemplate/GetDetail?packDsgnTplId=xxx
        // 참조 화면의 행 펼침 영역과 같은 항목(설명 3종 + 이미지)을 내려준다.
        // 응답에 이미지가 실리므로 행을 펼칠 때만 호출한다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetDetail")]
        public async Task<IActionResult> GetDetail([FromQuery] string packDsgnTplId)
        {
            if (string.IsNullOrWhiteSpace(packDsgnTplId))
            {
                return BadRequest(new { success = false, message = "필수 파라미터(packDsgnTplId)가 누락되었습니다." });
            }

            try
            {
                var a = await _context.If002a
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PackDsgnTplId == packDsgnTplId);

                if (a == null)
                {
                    return NotFound(new { success = false, message = "디자인 템플릿 상세 정보를 찾을 수 없습니다." });
                }

                var dto = new DesignTemplateDetailDto
                {
                    PackDsgnTplId = a.PackDsgnTplId,
                    Subject = a.Subject,
                    DsgnExpCon = a.DsgnExpCon,
                    DsgnFeatDscr = a.DsgnFeatDscr,
                    OperDscr = a.OperDscr,
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
