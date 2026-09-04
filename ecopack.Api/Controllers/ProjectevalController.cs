using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ecopack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectevalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectevalController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetLatestEvalQuestions")]
        public async Task<IActionResult> GetLatestEvalQuestions([FromQuery] string packLevel, [FromQuery] string appliedMaterial)
        {
            var list = await _context.If200s
                .Where(x => x.PackLevel == packLevel && x.AppliedMaterial == appliedMaterial)
                .OrderBy(x => x.DspSeq)
                .ToListAsync();

            return Ok(list);
        }
    }
}