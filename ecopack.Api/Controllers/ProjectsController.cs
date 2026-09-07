using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Support;

namespace ecopack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProjectsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/projects (최근 프로젝트 목록 조회)
        // 프로젝트는 만든 사람만 볼 수 있다.
        // 화면이 로그인한 고객 ID(repCustId)를 함께 보내면 그 회원의 것만 돌려준다.
        // 예전 데이터는 repCustId 가 비어 있고 prjuserid 만 있는 경우가 있어 두 컬럼을 모두 본다.
        [HttpGet("GetProjects")]
        public async Task<IActionResult> GetProjects([FromQuery] string? repCustId)
        {
            var query = _context.Project.AsQueryable();

            if (!string.IsNullOrWhiteSpace(repCustId))
            {
                query = query.Where(x => x.RepCustId == repCustId || x.Prjuserid == repCustId);
            }
            else
            {
                // 로그인 정보를 받지 못하면 남의 프로젝트가 보이지 않도록 빈 목록을 돌려준다
                query = query.Where(x => false);
            }

            var list = await query
                .OrderByDescending(x => x.PrjId) // 최신순 정렬
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
        [HttpPost("CreateProject")]
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

            // 프로젝트에는 그 프로젝트를 만든 회원의 정보를 함께 남긴다.
            // 화면에서 넘어오지 않는 회사·담당자 항목은 customer(고객기본)에서 직접 읽어 채운다.
            // 고객 ID는 화면이 보내준 repCustId 를 쓰고, 없으면 로그인 아이디(prjuserid)로 찾는다.
            var custId = !string.IsNullOrWhiteSpace(dto.RepCustId) ? dto.RepCustId : dto.Prjuserid;
            Customer? member = null;
            if (!string.IsNullOrWhiteSpace(custId))
            {
                member = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.RepCustId == custId);
            }

            // 화면에서 직접 입력한 값이 있으면 그 값을, 없으면 회원정보를 쓴다
            static string? Pick(string? typed, string? fromMember) =>
                !string.IsNullOrWhiteSpace(typed) ? typed : fromMember;

            var newProject = new Project
            {
                PrjId = targetPrjId, // 👈 채번된(또는 전달받은) ID 사용
                PrjNm = dto.PrjNm,
                RepCustId = Pick(dto.RepCustId, member?.RepCustId ?? custId),
                BizNo = Pick(dto.BizNo, member?.BizNo),
                BizNm = Pick(dto.BizNm, member?.BizNm),
                RepNm = Pick(dto.RepNm, member?.RepNm) ?? "담당자미정",
                RoleNm = Pick(dto.RoleNm, member?.RoleNm),
                IndstNm = Pick(dto.IndstNm, member?.IndstNm),
                CntryNm = Pick(dto.CntryNm, member?.CntryNm),
                AddrCd = Pick(dto.AddrCd, member?.AddrCd),
                DtlAddr1 = Pick(dto.DtlAddr1, member?.DtlAddr1),
                DtlAddr2 = Pick(dto.DtlAddr2, member?.DtlAddr2),
                EmlAddr = Pick(dto.EmlAddr, member?.EmlAddr),
                RepTelNo = Pick(dto.RepTelNo, member?.RepTelNo),
                MblTelNo = Pick(dto.MblTelNo, member?.MblTelNo),
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

            // 프로젝트를 만들면 그 차수의 기술문서와 적합성 선언서 기본 폼을 함께 만들어 둔다.
            // 문서 번호를 미리 채번해 두어, 화면에서는 곧바로 내용만 채우면 되게 한다.
            CreateDocumentsFor(newProject, member);

            await _context.SaveChangesAsync();

            return Ok(new { success = true, prjId = targetPrjId, message = "프로젝트가 성공적으로 등록되었습니다." });
        }

        /// <summary>
        /// 프로젝트 상세 정보 신규/수정 저장용 (Upsert)
        /// POST: api/projects/detail
        /// </summary>
        [HttpPost("detail")]
        public async Task<IActionResult> SaveProjectDetail([FromBody] ProjectDetailSaveDto dto, [FromQuery] string? repCustId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 본인이 만든 프로젝트만 저장할 수 있다.
            // 고객 ID는 쿼리로 받은 값을 먼저 쓰고, 없으면 화면이 담아 보낸 작성자 아이디를 쓴다.
            var owner = !string.IsNullOrWhiteSpace(repCustId) ? repCustId : dto.Prjuserid;
            if (!await ProjectAccess.IsOwnerAsync(_context, dto.PrjId, owner))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                // 💡 DbSet 이름을 'ProjectDetail'로 정확히 매칭
                var detail = await _context.ProjectDetail
                    .FirstOrDefaultAsync(x => x.PrjId == dto.PrjId && x.PackLevel == dto.PackLevel);

                if (detail == null)
                {
                    // 1. 신규 저장 (Insert)
                    detail = new ProjectDetail
                    {
                        PrjId = dto.PrjId,
                        PackLevel = dto.PackLevel,
                        PrjRevNo = dto.PrjRevNo,
                        PackLevelNm = dto.PackLevelNm,
                        AppliedMaterial = dto.AppliedMaterial,
                        AppliedMaterialNm = dto.AppliedMaterialNm,
                        MatUse = dto.MatUse,
                        MatUseNm = dto.MatUseNm,
                        MatType = dto.MatType,
                        MatTypeNm = dto.MatTypeNm,
                        MatForm = dto.MatForm,
                        MatFormNm = dto.MatFormNm,
                        PackDsgnTplId = dto.PackDsgnTplId,
                        Projstatus = dto.Projstatus,
                        PrdExpCntry = dto.PrdExpCntry,
                        PrdExpCntryNm = dto.PrdExpCntryNm,
                        Prjuserid = dto.Prjuserid, // 💡 신규 저장 시 반영
                        Updatedate = DateTime.Now
                    };

                    _context.ProjectDetail.Add(detail);
                }
                else
                {
                    // 2. 수정 저장 (Update)
                    detail.PrjRevNo = dto.PrjRevNo;
                    detail.PackLevelNm = dto.PackLevelNm;
                    detail.AppliedMaterial = dto.AppliedMaterial;
                    detail.AppliedMaterialNm = dto.AppliedMaterialNm;
                    detail.MatUse = dto.MatUse;
                    detail.MatUseNm = dto.MatUseNm;
                    detail.MatType = dto.MatType;
                    detail.MatTypeNm = dto.MatTypeNm;
                    detail.MatForm = dto.MatForm;
                    detail.MatFormNm = dto.MatFormNm;
                    detail.PackDsgnTplId = dto.PackDsgnTplId;
                    detail.Projstatus = dto.Projstatus;
                    detail.PrdExpCntry = dto.PrdExpCntry;
                    detail.PrdExpCntryNm = dto.PrdExpCntryNm;
                    detail.Prjuserid = dto.Prjuserid; // 💡 수정 저장 시 반영
                    detail.Updatedate = DateTime.Now;

                    _context.ProjectDetail.Update(detail);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "성공적으로 저장되었습니다.", data = detail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "저장 중 오류가 발생했습니다.", error = ex.Message });
            }
        }
        [HttpGet("Getdetail")]
        public async Task<IActionResult> GetProjectDetail([FromQuery] string prjId, [FromQuery] string packLevel, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { success = false, message = "필수 파라미터(prjId, packLevel)가 누락되었습니다." });
            }

            // 본인이 만든 프로젝트만 열 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }
            try
            {
                var detail = await _context.ProjectDetail
                    .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel);

                if (detail == null)
                {
                    return NotFound(new { success = false, message = "해당하는 프로젝트 상세 정보를 찾을 수 없습니다." });
                }
                return Ok(detail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "조회 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE: api/Projects/DeleteProject?prjId=xxx&packLevel=2
        // 프로젝트를 포장차수 단위로 지운다.
        // 한 프로젝트에 1/2/3차가 있을 때 2차만 지우면 2차 행과
        // 2차 기술문서·적합성 선언서(secondary_*)만 사라지고 나머지 차수는 남는다.
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("DeleteProject")]
        public async Task<IActionResult> DeleteProject(
            [FromQuery] string prjId,
            [FromQuery] string packLevel,
            [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId) || string.IsNullOrWhiteSpace(packLevel))
            {
                return BadRequest(new { success = false, message = "필수 파라미터(prjId, packLevel)가 누락되었습니다." });
            }

            // 본인이 만든 프로젝트만 지울 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var project = await _context.Project
                    .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel);

                if (project == null)
                {
                    return NotFound(new { success = false, message = "해당 포장차수의 프로젝트를 찾을 수 없습니다." });
                }

                var removed = new List<string>();

                // 1) 그 차수의 기술문서 / 적합성 선언서
                switch (packLevel)
                {
                    case "1":
                        var td1 = await _context.PrimaryTd.Where(x => x.PrjId == prjId).ToListAsync();
                        var dc1 = await _context.PrimaryDoc.Where(x => x.PrjId == prjId).ToListAsync();
                        if (td1.Count > 0) { _context.PrimaryTd.RemoveRange(td1); removed.Add($"기술문서 {td1.Count}건"); }
                        if (dc1.Count > 0) { _context.PrimaryDoc.RemoveRange(dc1); removed.Add($"적합성선언서 {dc1.Count}건"); }
                        break;
                    case "2":
                        var td2 = await _context.SecondaryTd.Where(x => x.PrjId == prjId).ToListAsync();
                        var dc2 = await _context.SecondaryDoc.Where(x => x.PrjId == prjId).ToListAsync();
                        if (td2.Count > 0) { _context.SecondaryTd.RemoveRange(td2); removed.Add($"기술문서 {td2.Count}건"); }
                        if (dc2.Count > 0) { _context.SecondaryDoc.RemoveRange(dc2); removed.Add($"적합성선언서 {dc2.Count}건"); }
                        break;
                    case "3":
                        var td3 = await _context.TertiaryTd.Where(x => x.PrjId == prjId).ToListAsync();
                        var dc3 = await _context.TertiaryDoc.Where(x => x.PrjId == prjId).ToListAsync();
                        if (td3.Count > 0) { _context.TertiaryTd.RemoveRange(td3); removed.Add($"기술문서 {td3.Count}건"); }
                        if (dc3.Count > 0) { _context.TertiaryDoc.RemoveRange(dc3); removed.Add($"적합성선언서 {dc3.Count}건"); }
                        break;
                }

                // 2) 그 차수의 기본사항(프로젝트 상세)
                var details = await _context.ProjectDetail
                    .Where(x => x.PrjId == prjId && x.PackLevel == packLevel)
                    .ToListAsync();
                if (details.Count > 0)
                {
                    _context.ProjectDetail.RemoveRange(details);
                    removed.Add($"기본사항 {details.Count}건");
                }

                // 3) 프로젝트 행
                _context.Project.Remove(project);

                await _context.SaveChangesAsync();

                // 4) 그 차수에 올린 첨부파일 폴더도 정리한다 (DB 정리가 끝난 뒤에 지운다)
                DeleteUploadFolders(prjId, packLevel);

                return Ok(new
                {
                    success = true,
                    prjId,
                    packLevel,
                    message = removed.Count > 0
                        ? $"{packLevel}차 프로젝트와 {string.Join(", ", removed)}을(를) 삭제했습니다."
                        : $"{packLevel}차 프로젝트를 삭제했습니다."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "삭제 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        /// <summary>해당 차수의 첨부문서 / 근거문서 폴더를 통째로 지운다.</summary>
        private void DeleteUploadFolders(string prjId, string packLevel)
        {
            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                // 1차는 uploads/td, 2·3차는 uploads/td2, uploads/td3 를 쓴다
                var suffix = packLevel == "1" ? "" : packLevel;
                foreach (var kind in new[] { "td", "doc" })
                {
                    var dir = Path.Combine(webRoot, "uploads", kind + suffix, prjId);
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, recursive: true);
                    }
                }
            }
            catch (Exception ex)
            {
                // 파일 정리에 실패해도 삭제 자체는 이미 끝났으므로 요청을 실패시키지 않는다
                Console.WriteLine($"[DeleteProject] 첨부파일 정리 실패 prjId={prjId} packLevel={packLevel}: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 프로젝트를 만들 때 그 차수의 문서 기본 폼을 함께 만든다.
        // 1차는 primary_*, 2차는 secondary_*, 3차는 tertiary_* 테이블을 쓴다.
        // 회사·담당자 정보는 회원정보에서 가져와 채우고, 문서번호는 여기서 채번한다.
        // ─────────────────────────────────────────────────────────────
        private void CreateDocumentsFor(Project project, Customer? member)
        {
            var level = project.PackLevel;
            if (level != "1" && level != "2" && level != "3")
            {
                return; // 포장차수가 없으면 문서를 만들지 않는다
            }

            var prjId    = project.PrjId;

            // 채번: TD/DOC-{차수}-{프로젝트번호}. 프로젝트번호(prjId)가 이미 유일하므로
            // 별도 타임스탬프 없이 이것만으로 문서 ID도 유일해진다. Primary/Secondary/
            // TertiaryTdController.Save() 의 자체 채번 로직도 같은 규칙을 쓰므로,
            // 여기서 만든 문서가 없어 그쪽에서 새로 채번하더라도 같은 ID가 나온다.
            var techDocId = $"TD-{level}-{prjId}";
            var declDocId = $"DOC-{level}-{prjId}";

            var prjfNm   = project.PrjNm;
            var bizNm    = member?.BizNm    ?? project.BizNm;
            var cntryNm  = member?.CntryNm  ?? project.CntryNm;
            var repNm    = member?.RepNm    ?? project.RepNm;
            var roleNm   = member?.RoleNm   ?? project.RoleNm;
            var emlAddr  = member?.EmlAddr  ?? project.EmlAddr;
            var mblTelNo = member?.MblTelNo ?? project.MblTelNo;
            var today    = DateOnly.FromDateTime(DateTime.Now);
            var now      = DateTime.Now;

            switch (level)
            {
                case "1":
                    _context.PrimaryTd.Add(new PrimaryTd
                    {
                        Pkg1TechDocId = techDocId, PrjId = prjId, PrjfNm = prjfNm,
                        BizNm = bizNm, CntryNm = cntryNm, DocNo = techDocId,
                        RevNo = "Rev.01", LastWrtDtm = now,
                        BizNm2 = bizNm, RepNm = repNm, RoleNm = roleNm,
                        EmlAddr = emlAddr, MbTelNo = mblTelNo
                    });
                    _context.PrimaryDoc.Add(new PrimaryDoc
                    {
                        Pkg1DocId = declDocId, PrjId = prjId, PrjfNm = prjfNm,
                        Pkg1TechDocId = techDocId, RevNo = "Rev.01", LastWrtDt = today,
                        BizNm = bizNm, RepNm = repNm, RoleNm = roleNm,
                        EmlAddr = emlAddr, MbTelNo = mblTelNo, CntryNm = cntryNm,
                        BizNm2 = bizNm, RepNm2 = repNm, RoleNm2 = roleNm, SbstTot = "총합"
                    });
                    break;

                case "2":
                    _context.SecondaryTd.Add(new SecondaryTd
                    {
                        Pkg2TechDocId = techDocId, PrjId = prjId, PrjfNm = prjfNm,
                        BizNm = bizNm, CntryNm = cntryNm, DocNo = techDocId,
                        RevNo = "Rev.01", LastWrtDtm = now,
                        BizNm2 = bizNm, RepNm = repNm, RoleNm = roleNm,
                        EmlAddr = emlAddr, MbTelNo = mblTelNo
                    });
                    _context.SecondaryDoc.Add(new SecondaryDoc
                    {
                        Pkg2DocId = declDocId, PrjId = prjId, PrjfNm = prjfNm,
                        Pkg2TechDocId = techDocId, RevNo = "Rev.01", LastWrtDt = today,
                        BizNm = bizNm, RepNm = repNm, RoleNm = roleNm,
                        EmlAddr = emlAddr, MbTelNo = mblTelNo, CntryNm = cntryNm,
                        BizNm2 = bizNm, RepNm2 = repNm, RoleNm2 = roleNm, SbstTot = "총합"
                    });
                    break;

                case "3":
                    _context.TertiaryTd.Add(new TertiaryTd
                    {
                        Pkg3TechDocId = techDocId, PrjId = prjId, PrjfNm = prjfNm,
                        BizNm = bizNm, CntryNm = cntryNm, DocNo = techDocId,
                        RevNo = "Rev.01", LastWrtDtm = now,
                        BizNm2 = bizNm, RepNm = repNm, RoleNm = roleNm,
                        EmlAddr = emlAddr, MbTelNo = mblTelNo
                    });
                    _context.TertiaryDoc.Add(new TertiaryDoc
                    {
                        Pkg3DocId = declDocId, PrjId = prjId, PrjfNm = prjfNm,
                        Pkg3TechDocId = techDocId, RevNo = "Rev.01", LastWrtDt = today,
                        BizNm = bizNm, RepNm = repNm, RoleNm = roleNm,
                        EmlAddr = emlAddr, MbTelNo = mblTelNo, CntryNm = cntryNm,
                        BizNm2 = bizNm, RepNm2 = repNm, RoleNm2 = roleNm, SbstTot = "총합"
                    });
                    break;
            }
        }

        [HttpGet("template")]
        public async Task<IActionResult> GetProjecttemplate(
            [FromQuery] string? packLevel,
            [FromQuery] string? appliedMaterial,
            [FromQuery] string? matType,
            [FromQuery] int page = 1,       // 프론트에서 넘어오는 페이지 번호 (기본값 1)
            [FromQuery] int pageSize = 20)  // 프론트에서 넘어오는 페이지당 개수 (기본값 20)
        {
            try
            {
                var query = _context.If002a.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(packLevel))
                {
                    query = query.Where(x => x.PackLevel == packLevel);
                }
                if (!string.IsNullOrEmpty(appliedMaterial))
                {
                    query = query.Where(x => x.AppliedMaterial == appliedMaterial);
                }
                if (!string.IsNullOrEmpty(matType))
                {
                    query = query.Where(x => x.MatType == matType);
                }

                // 1. 전체 데이터 개수 계산 (페이지네이션 UI 계산용)
                var totalCount = await query.CountAsync();

                // 2. 페이징 적용하여 해당 페이지의 25개만 조회
                var list = await query
                    .OrderBy(x => x.PackLevel)
                    .ThenBy(x => x.AppliedMaterial)
                    .ThenBy(x => x.MatType)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ProjecttemplateListDto
                    {
                        Idx = x.Idx,
                        PackDsgnTplId = x.PackDsgnTplId,
                        PackLevelNm = x.PackLevelNm,
                        MatTypeNm = x.MatTypeNm,
                        Subject = x.Subject,
                        DsgnTypeNm = x.DsgnTypeNm,
                        DsgnTypeCdVal = x.DsgnTypeCdVal,
                        DsgnExpCon = x.DsgnExpCon,
                        AppliedMaterialNm = x.AppliedMaterialNm,
                        DsgnFeatDscr = x.DsgnFeatDscr,
                        OperDscr = x.OperDscr,
                        PackLevel = x.PackLevel,
                        MatType = x.MatType,
                        AppliedMaterial = x.AppliedMaterial,
                        FileNm = x.FileNm,
                        FileData = x.FileData // 이미지를 포함하되 25개로 제한되어 속도가 빠름
                    })
                    .ToListAsync();

                // 3. 프론트가 요구하는 구조({ totalCount, items })로 반환
                return Ok(new
                {
                    totalCount = totalCount,
                    items = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("templateUpdate")]
        public async Task<IActionResult> templateUpdate([FromBody] ProjecttemplateUpdateDto dto)
        {
            try
            {
                // 1. prjId와 packLevel을 기준으로 프로젝트 디테일(또는 대상 테이블) 조회
                // 예시: _context.If001 (프로젝트 디테일 테이블)
                var projectDetail = await _context.ProjectDetail
                    .FirstOrDefaultAsync(x => x.PrjId == dto.PrjId && x.PackLevel == dto.PackLevel);
                if (projectDetail == null)
                {
                    return NotFound(new { message = "해당하는 프로젝트 정보를 찾을 수 없습니다." });
                }
                // 2. packDsgnTplId 갱신
                projectDetail.PackDsgnTplId = dto.PackDsgnTplId;
                projectDetail.Prjuserid = dto.Prjuserid;
                projectDetail.Updatedate = DateTime.Now; // 필요시 수정일자 추가
                projectDetail.Projstatus = "template"; // 필요시 상태 변경

                // 3. 데이터베이스 저장
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "성공적으로 저장되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}