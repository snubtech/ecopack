using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    [Route("api/[controller]")] // ➡️ /api/common
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommonController(AppDbContext context)
        {
            _context = context;
        }

        // 주소가 /api/common/Appliedmaterial 로 아주 짧고 깔끔해집니다. 적용소재
        [HttpGet("material")]
        public async Task<IActionResult> GetMaterialProperty()
        {
            var list = await _context.If001
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
        //주소가 /api/common/mattype ]  포장재구분
        [HttpGet("mattype")]
        public async Task<IActionResult> GetMattypeProperty()
        {
            var list = await _context.If001
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