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

        public ProjectsController(AppDbContext context)
        {
            _context = context;
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