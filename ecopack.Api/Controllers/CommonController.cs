//using Microsoft.AspNetCore.Authorization; // 이 네임스페이스를 꼭 추가해야 합니다!
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    [Route("api/[controller]")] // ➡️ /api/common
    [ApiController]
    //[Authorize] // <--- 이 한 줄을 추가하면 이 컨트롤러의 모든 API는 로그인한 사용자만 호출할 수 있습니다!
    public class CommonController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommonController(AppDbContext context)
        {
            _context = context;
        }

        // 주소가 /api/common/Appliedmaterial 로 아주 짧고 깔끔해집니다. 적용소재
        [HttpGet("material")]
        public async Task<IActionResult> GetMaterialProperty([FromQuery] string packLevel)
        {
            // packLevel 필터링 Where 조건 추가
            var query = _context.If001.AsQueryable();

            if (!string.IsNullOrEmpty(packLevel))
            {
                query = query.Where(x => x.PackLevel == packLevel);
            }
            var list = await query
                .GroupBy(x => new { x.AppliedMaterial, x.AppliedMaterialNm })
                .Select(g => new MaterialPropertyDto
                {
                    AppliedMaterial = g.Key.AppliedMaterial,
                    AppliedMaterialNm = g.Key.AppliedMaterialNm,

                    PackLevel = g.Select(x => x.PackLevel).FirstOrDefault(),
                    PackLevelNm = g.Select(x => x.PackLevelNm).FirstOrDefault(),
                    MatUse = g.Select(x => x.MatUse).FirstOrDefault(),
                    MatUseNm = g.Select(x => x.MatUseNm).FirstOrDefault(),
                    MatType = g.Select(x => x.MatType).FirstOrDefault(),
                    MatTypeNm = g.Select(x => x.MatTypeNm).FirstOrDefault(),
                    MatForm = g.Select(x => x.MatForm).FirstOrDefault(),
                    MatFormNm = g.Select(x => x.MatFormNm).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(list);
        }
        // 주소가 /api/common/packlevels 로 아주 짧고 깔끔해집니다. 포장차수
        [HttpGet("packlevels")]
        public async Task<IActionResult> GetPackLevels()
        {
            var list = await _context.If001
                .GroupBy(x => new { x.PackLevel, x.PackLevelNm })
                .Select(g => new PackLevelDto
                {
                    PackLevel = g.Key.PackLevel,
                    PackLevelNm = g.Key.PackLevelNm
                })
                .ToListAsync();

            return Ok(list);
        }
        // 주소가 /api/common/mattype 포장재구분
        [HttpGet("mattype")]
        public async Task<IActionResult> GetMattypeProperty([FromQuery] string packLevel)     
        {
            // 💡 packLevel 값이 전달된 경우에만 검증
            if (!string.IsNullOrEmpty(packLevel))
            {
                // 1. int.TryParse로 숫자로 변환 가능한지 체크 ('abc' 같은 문자 차단)
                // 2. 변환된 숫자가 1 이상 3 이하인지 체크 (범위를 벗어난 숫자 차단)
                if (!int.TryParse(packLevel, out int level) || level < 1 || level > 3)
                {
                    return BadRequest(new { success = false, message = "포장 차수는 1부터 3 사이의 숫자만 입력할 수 있습니다." });
                }
            }

            var query = _context.If001.AsQueryable();

            if (!string.IsNullOrEmpty(packLevel))
            {
                query = query.Where(x => x.PackLevel == packLevel);
            }

            var list = await query
                .GroupBy(x => new { x.MatType, x.MatTypeNm })
                .Select(g => new MattypePropertyDto
                {
                    MatType = g.Key.MatType,
                    MatTypeNm = g.Key.MatTypeNm
                })
                .ToListAsync();

            return Ok(list);
        }
        //주소가 /api/common/mat-forms ]  포장재소재구성
        [HttpGet("matforms")]
        public async Task<IActionResult> GetMatForms(
        [FromQuery] string packLevel,
        [FromQuery] string appliedMaterial,
        [FromQuery] string matType)
        {
            var query = _context.If001.AsQueryable();

            if (!string.IsNullOrEmpty(packLevel))
                query = query.Where(x => x.PackLevel == packLevel);

            if (!string.IsNullOrEmpty(appliedMaterial))
                query = query.Where(x => x.AppliedMaterial == appliedMaterial);

            if (!string.IsNullOrEmpty(matType))
                query = query.Where(x => x.MatType == matType);

            var list = await query
                .GroupBy(x => new { x.MatForm, x.MatFormNm, x.PackLevelNm, x.AppliedMaterial, x.MatType, x.MatTypeNm })
                .Select(g => new MaterialPropertyDto
                {
                    MatForm = g.Key.MatForm,
                    MatFormNm = g.Key.MatFormNm,
                    PackLevelNm = g.Key.PackLevelNm,
                    AppliedMaterial = g.Key.AppliedMaterial,
                    MatType = g.Key.MatType,
                    MatTypeNm = g.Key.MatTypeNm
                })
                .OrderBy(x => x.MatFormNm)
                .ToListAsync();

            return Ok(list);
        }


    }
}