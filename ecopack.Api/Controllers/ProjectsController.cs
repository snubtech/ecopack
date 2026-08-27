using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/projects (최근 프로젝트 목록 조회)
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var list = await _context.Project
                .OrderByDescending(x => x.PrjFcrtDt) // 최신순 정렬
                .Select(x => new ProjectListDto
                {
                    PrjId = x.PrjId,
                    PrjNm = x.PrjNm,
                    RepCustId = x.RepCustId,
                    BizNo = x.BizNo,
                    BizNm = x.BizNm,
                    RepNm = x.RepNm,
                    RoleNm = x.RoleNm,
                    IndstNm = x.IndstNm,
                    CntryNm = x.CntryNm,
                    AddrCd = x.AddrCd,
                    DtlAddr1 = x.DtlAddr1,
                    DtlAddr2 = x.DtlAddr2,
                    EmlAddr = x.EmlAddr,
                    RepTelNo = x.RepTelNo,
                    MblTelNo = x.MblTelNo,
                    PrdExpCntryNm1 = x.PrdExpCntryNm1,
                    PrdExpCntryNm2 = x.PrdExpCntryNm2,
                    PrdExpCntryNm3 = x.PrdExpCntryNm3,
                    PrdExpCntryNm4 = x.PrdExpCntryNm4,
                    PrdExpCntryNm5 = x.PrdExpCntryNm5,
                    PrdExpCntryNm6 = x.PrdExpCntryNm6,
                    PrdExpCntryNm7 = x.PrdExpCntryNm7,
                    PrdExpCntryNm8 = x.PrdExpCntryNm8,
                    PrdPkgSeq1 = x.PrdPkgSeq1,
                    PrdPkgSeq2 = x.PrdPkgSeq2,
                    PrdPkgSeq3 = x.PrdPkgSeq3,
                    PrjRevNo = x.PrjRevNo,
                    Prjuserid = x.Prjuserid,
                    Prjmemo = x.Prjmemo,
                    PackLevel = x.PackLevel,
                    PrjFcrtDt = x.PrjFcrtDt
                })
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/projects (신규 프로젝트 등록)
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("전달된 데이터가 없습니다.");
            }

            // 💡 프론트에서 prjId를 보내주었으면 그 걸 쓰고, 없으면 새로 채번!
            string targetPrjId = !string.IsNullOrEmpty(dto.PrjId)
                ? dto.PrjId
                : $"{DateTime.Now:yyyyMMddHHmmssfff}";

            var newProject = new Project
            {
                PrjId = targetPrjId, // 👈 채번된(또는 전달받은) ID 사용
                PrjNm = dto.PrjNm,
                RepCustId = dto.RepCustId,
                BizNo = dto.BizNo,
                BizNm = dto.BizNm,
                RepNm = dto.RepNm,
                RoleNm = dto.RoleNm,
                IndstNm = dto.IndstNm,
                CntryNm = dto.CntryNm,
                AddrCd = dto.AddrCd,
                DtlAddr1 = dto.DtlAddr1,
                DtlAddr2 = dto.DtlAddr2,
                EmlAddr = dto.EmlAddr,
                RepTelNo = dto.RepTelNo,
                MblTelNo = dto.MblTelNo,
                PrdExpCntryNm1 = dto.PrdExpCntryNm1,
                PrdExpCntryNm2 = dto.PrdExpCntryNm2,
                PrdExpCntryNm3 = dto.PrdExpCntryNm3,
                PrdExpCntryNm4 = dto.PrdExpCntryNm4,
                PrdExpCntryNm5 = dto.PrdExpCntryNm5,
                PrdExpCntryNm6 = dto.PrdExpCntryNm6,
                PrdExpCntryNm7 = dto.PrdExpCntryNm7,
                PrdExpCntryNm8 = dto.PrdExpCntryNm8,
                PrdPkgSeq1 = dto.PrdPkgSeq1,
                PrdPkgSeq2 = dto.PrdPkgSeq2,
                PrdPkgSeq3 = dto.PrdPkgSeq3,
                PrjRevNo = "Rev.01",
                Prjuserid = dto.Prjuserid,
                Prjmemo = dto.Prjmemo,
                PackLevel = dto.PackLevel,
                PrjFcrtDt = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Project.Add(newProject);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, prjId = targetPrjId, message = "프로젝트가 성공적으로 등록되었습니다." });
        }
    }
}