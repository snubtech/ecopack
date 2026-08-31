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

        // 주소가 /api/common/Appliedmaterial 로 아주 짧고 깔끔해집니다.
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
        //주소가 /api/common/mattype ]
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
                

    }
}