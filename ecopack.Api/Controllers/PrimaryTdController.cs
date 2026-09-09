/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - PrimaryTdController (기술문서)
 * ==============================================================================
 * 
 * 1. 담당 범위
 *    - 기술문서 화면이 쓰는 API 입니다. 대상 테이블은 primary_td(1차포장기술서기본) 입니다.
 *    - 문서는 프로젝트 단위로 한 건이며, 프로젝트 ID(prjId)로 찾습니다.
 * 
 * 2. Get — 문서 조회
 *    - 해당 프로젝트의 기술문서를 돌려줍니다.
 *    - 아직 작성 전이면 isNew=true 와 빈 문서를 돌려주어, 화면이 고정 문구를 채워 넣게 합니다.
 * 
 * 3. Save — 신규/수정 통합 저장 (Upsert)
 *    - 같은 프로젝트의 문서가 있으면 수정하고, 없으면 새로 만듭니다.
 *    - 신규일 때 문서 ID를 TD-{포장차수}-{프로젝트번호} 규칙으로 채번합니다.
 *      프로젝트번호(prjId)는 이미 유일하므로 문서 ID도 그것만으로 유일해집니다.
 *      포장차수는 1차(판매) 고정이며, 2·3차는 별도 화면이 생길 때 값만 바꾸면 됩니다.
 *    - 문서번호(docNo)는 기술문서 번호(TD-1-...)와 같은 값으로 고정합니다.
 *      적합성 선언서가 참조하는 번호와 어긋나면 안 되므로 화면에서 고칠 수 없게 했습니다.
 *    - 개정번호(revNo)가 비어 있으면 Rev.01 을 넣고,
 *      저장할 때마다 작성일시(lastWrtDtm)를 서버 현재 시각으로 갱신합니다.
 * 
 * 4. UploadAtchDoc / DownloadAtchDoc / DeleteAtchDoc — 첨부문서
 *    - 파일은 wwwroot 밖(App_Data/uploads/projects/td/{포장차수}/{프로젝트번호}/)에 두고,
 *      atchDocUrl{슬롯} 에는 그 안에서의 상대경로를, atchDocNm{슬롯} 에는 확장자 포함
 *      원본 파일명을 기록합니다. wwwroot 밖이라 URL로 직접 접근할 수 없고,
 *      Download 액션이 소유자 확인을 통과한 요청에만 파일을 내려줍니다.
 *    - 실제 저장 파일명은 슬롯 번호와 타임스탬프로 새로 만들어 이름 충돌을 막습니다.
 *    - 업로드 확장자는 화이트리스트(UploadPolicy)로 제한합니다: doc/ppt/excel/hwp/pdf.
 *    - 삭제하면 컬럼을 비우고 서버의 실제 파일도 지웁니다.
 * 
 * 5. UploadMfrDrw / DeleteMfrDrw — 제조 도면
 *    - 3번 항목의 제조 도면은 첨부문서와 달리 이미지 한 장만 관리한다.
 *      png/jpg/jpeg/svg 파일만 허용하고, 다시 올리면 이전 이미지는 지운다.
 *    - 화면에는 base64 데이터 URI로 곧바로 그릴 수 있게 내려준다(ToDto 참고).
 * 
 * 6. 8. 제조 공정의 공정도 이미지
 *    - 기본 평가지 조회 화면의 '3. 공정도'와 같은 데이터(If003a)를
 *      화면이 /api/Projects/Getprocessflow 로 직접 불러와 보여준다.
 *      이 컨트롤러에는 별도 API가 없다.
 * 
 * 7. DTO ↔ 엔티티 매핑 (ToDto / ApplyDtoToEntity)
 *    - 항목이 200개가 넘어 수기로 옮기지 않고 이름이 같은 것끼리 리플렉션으로 옮깁니다.
 *    - 화면은 모든 값을 글자로 다루므로, DB가 숫자인 컬럼은 여기서 변환합니다.
 *      ('1,234.56' → 1234.56, '12개' → 12 처럼 단위나 구분자가 섞여 있어도 숫자만 뽑습니다)
 *    - 값이 null 이면 '요청에 담기지 않은 항목' 으로 보고 기존 DB 값을 그대로 둡니다.
 *      빈 문자열이면 '화면에서 지운 항목' 으로 보고 DB를 비웁니다.
 * ==============================================================================
 */
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Support;

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 1차포장 기술문서(primary_td / 기술문서 모듈 A) 화면용 API.
    /// 라우트: api/PrimaryTd
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PrimaryTdController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        /// <summary>포장차수 고정값. primary=1 / secondary=2 / tertiary=3</summary>
        private const string PackLevel = "1";

        /// <summary>첨부문서 슬롯 개수 (atchDocNm1~8 / atchDocUrl1~8)</summary>
        private const int AtchDocSlotCount = 8;

        private const long MaxAtchDocBytes = 20 * 1024 * 1024; // 20MB

        public PrimaryTdController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ─────────────────────────────────────────────────────────────
        // 채번: TD-{차수}-{프로젝트번호}
        // ─────────────────────────────────────────────────────────────
        private static string NewTechDocId(string prjId) =>
            $"TD-{PackLevel}-{prjId}";

        // ─────────────────────────────────────────────────────────────
        // GET: api/PrimaryTd/Get?prjId=xxx
        // 해당 프로젝트의 기술문서를 조회한다. 없으면 빈 DTO(신규 작성용)를 돌려준다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("Get")]
        public async Task<IActionResult> Get([FromQuery] string prjId, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId))
            {
                return BadRequest(new { success = false, message = "필수 파라미터(prjId)가 누락되었습니다." });
            }

            // 본인이 만든 프로젝트의 문서만 열 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var entity = await _context.PrimaryTd
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PrjId == prjId);

                if (entity == null)
                {
                    // 신규: 화면이 고정문구 기본값을 채워 넣을 수 있도록 키만 담아서 반환
                    return Ok(new { success = true, isNew = true, data = new PrimaryTdDto { PrjId = prjId } });
                }

                return Ok(new { success = true, isNew = false, data = ToDto(entity, _env) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "조회 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/PrimaryTd/Save
        // 신규/수정 통합 저장(Upsert). 저장 시 lastWrtDtm 을 현재 타임스탬프로 갱신한다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] PrimaryTdDto dto, [FromQuery] string? repCustId)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PrjId))
            {
                return BadRequest(new { success = false, message = "필수 값(prjId)이 누락되었습니다." });
            }

            // 본인이 만든 프로젝트의 문서만 저장할 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, dto.PrjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var entity = await _context.PrimaryTd
                    .FirstOrDefaultAsync(x => x.PrjId == dto.PrjId);

                var isNew = entity == null;

                if (isNew)
                {
                    entity = new PrimaryTd
                    {
                        // 프론트가 기존 ID를 보내오면 그대로 쓰고, 없으면 채번
                        Pkg1TechDocId = string.IsNullOrWhiteSpace(dto.Pkg1TechDocId)
                            ? NewTechDocId(dto.PrjId)
                            : dto.Pkg1TechDocId
                    };
                    _context.PrimaryTd.Add(entity);
                }

                ApplyDtoToEntity(dto, entity!);

                // 개정번호 초기값
                if (string.IsNullOrWhiteSpace(entity!.RevNo))
                {
                    entity.RevNo = "Rev.01";
                }

                // 문서번호(docNo)는 기술문서 번호와 같은 값으로 고정한다.
                // 적합성 선언서가 참조하는 기술문서 번호(pkg1TechDocId)와 절대 어긋나지 않도록
                // 화면에서 무엇이 넘어오든 채번된 번호로 덮어쓴다. (화면에서도 읽기 전용)
                entity.DocNo = entity.Pkg1TechDocId;

                // 저장 시각은 항상 서버 기준 현재 타임스탬프로 갱신
                entity.LastWrtDtm = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    isNew,
                    pkg1TechDocId = entity.Pkg1TechDocId,
                    lastWrtDtm = entity.LastWrtDtm,
                    message = isNew ? "기술문서가 생성되었습니다." : "기술문서가 저장되었습니다.",
                    data = ToDto(entity, _env)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "저장 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/PrimaryTd/DownloadAtchDoc?prjId=xxx&slot=1
        // 첨부문서 원본 파일을 내려준다. 소유자 확인을 통과해야만 내려받을 수 있다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("DownloadAtchDoc")]
        public async Task<IActionResult> DownloadAtchDoc([FromQuery] string prjId, [FromQuery] int slot, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId) || slot < 1 || slot > AtchDocSlotCount)
            {
                return BadRequest(new { success = false, message = "prjId와 slot(1~8)이 필요합니다." });
            }

            // 본인이 만든 프로젝트의 문서만 내려받을 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            var entity = await _context.PrimaryTd.AsNoTracking().FirstOrDefaultAsync(x => x.PrjId == prjId);
            if (entity == null)
            {
                return NotFound(new { success = false, message = "기술문서를 찾을 수 없습니다." });
            }

            var relativePath = GetStringProperty(entity, $"AtchDocUrl{slot}");
            var originalNm = GetStringProperty(entity, $"AtchDocNm{slot}") ?? $"attachment{slot}";
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return NotFound(new { success = false, message = "첨부된 파일이 없습니다." });
            }

            var physicalPath = UploadPolicy.ToPhysicalPath(_env, relativePath);
            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(new { success = false, message = "파일을 찾을 수 없습니다." });
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            return File(bytes, "application/octet-stream", originalNm);
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/PrimaryTd/UploadMfrDrw   (multipart/form-data)
        // 제조 도면 이미지를 업로드하고 mfrDrwUrl 에 반영한다. 이미지 파일만 허용하며,
        // 슬롯이 아니라 한 장만 유지하므로 다시 올리면 이전 이미지를 지운다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("UploadMfrDrw")]
        [RequestSizeLimit(MaxAtchDocBytes)]
        public async Task<IActionResult> UploadMfrDrw(
            [FromForm] string prjId,
            IFormFile file,
            [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId))
            {
                return BadRequest(new { success = false, message = "prjId가 필요합니다." });
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "업로드할 파일이 없습니다." });
            }
            if (file.Length > MaxAtchDocBytes)
            {
                return BadRequest(new { success = false, message = $"파일 크기는 {UploadPolicy.MaxFileSizeDisplay}를 넘을 수 없습니다." });
            }
            if (!UploadPolicy.IsImageExtensionAllowed(file.FileName))
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"이미지 파일({UploadPolicy.AllowedImageExtensionsDisplay})만 업로드할 수 있습니다."
                });
            }

            // 본인이 만든 프로젝트의 문서에만 올릴 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var entity = await _context.PrimaryTd.FirstOrDefaultAsync(x => x.PrjId == prjId);
                if (entity == null)
                {
                    return NotFound(new { success = false, message = "기술문서를 먼저 저장한 뒤 제조 도면을 올려주세요." });
                }

                // 슬롯이 아니라 한 장만 유지하므로, 다시 올리면 이전 파일부터 지운다
                if (!string.IsNullOrWhiteSpace(entity.MfrDrwUrl))
                {
                    var oldPhysical = UploadPolicy.ToPhysicalPath(_env, entity.MfrDrwUrl);
                    if (System.IO.File.Exists(oldPhysical))
                    {
                        System.IO.File.Delete(oldPhysical);
                    }
                }

                var originalNm = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(originalNm);
                var storedNm = $"drawing_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";

                var saveDir = UploadPolicy.GetProjectDocDirectory(_env, "td", PackLevel, prjId);
                Directory.CreateDirectory(saveDir);

                var savePath = Path.Combine(saveDir, storedNm);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                entity.MfrDrwUrl = UploadPolicy.ToRelativePath("td", PackLevel, prjId, storedNm);
                entity.LastWrtDtm = DateTime.Now;

                await _context.SaveChangesAsync();

                var bytes = await System.IO.File.ReadAllBytesAsync(savePath);
                var imageDataUri = ImageDataUri.FromFile(originalNm, Convert.ToBase64String(bytes));

                return Ok(new { success = true, imageDataUri, message = "제조 도면이 업로드되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "업로드 중 오류가 발생했습니다: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE: api/PrimaryTd/DeleteMfrDrw?prjId=xxx
        // 제조 도면 이미지를 지운다. (물리 파일도 함께 삭제)
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("DeleteMfrDrw")]
        public async Task<IActionResult> DeleteMfrDrw([FromQuery] string prjId, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId))
            {
                return BadRequest(new { success = false, message = "prjId가 필요합니다." });
            }

            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var entity = await _context.PrimaryTd.FirstOrDefaultAsync(x => x.PrjId == prjId);
                if (entity == null)
                {
                    return NotFound(new { success = false, message = "기술문서를 찾을 수 없습니다." });
                }

                if (!string.IsNullOrWhiteSpace(entity.MfrDrwUrl))
                {
                    var physical = UploadPolicy.ToPhysicalPath(_env, entity.MfrDrwUrl);
                    if (System.IO.File.Exists(physical))
                    {
                        System.IO.File.Delete(physical);
                    }
                }

                entity.MfrDrwUrl = null;
                entity.LastWrtDtm = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "제조 도면을 삭제했습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "삭제 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/PrimaryTd/UploadAtchDoc   (multipart/form-data)
        // 첨부문서를 업로드하고 atchDocUrl{slot} / atchDocNm{slot} 에 반영한다.
        // 문서명은 확장자를 포함한 원본 파일명 그대로 저장한다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("UploadAtchDoc")]
        [RequestSizeLimit(MaxAtchDocBytes)]
        public async Task<IActionResult> UploadAtchDoc(
            [FromForm] string prjId,
            [FromForm] int slot,
            IFormFile file,
            [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId))
            {
                return BadRequest(new AtchDocUploadResultDto { Success = false, Message = "prjId가 필요합니다." });
            }
            if (slot < 1 || slot > AtchDocSlotCount)
            {
                return BadRequest(new AtchDocUploadResultDto { Success = false, Message = $"slot은 1~{AtchDocSlotCount} 범위여야 합니다." });
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest(new AtchDocUploadResultDto { Success = false, Message = "업로드할 파일이 없습니다." });
            }
            if (file.Length > MaxAtchDocBytes)
            {
                return BadRequest(new AtchDocUploadResultDto { Success = false, Message = $"파일 크기는 {UploadPolicy.MaxFileSizeDisplay}를 넘을 수 없습니다." });
            }
            if (!UploadPolicy.IsExtensionAllowed(file.FileName))
            {
                return BadRequest(new AtchDocUploadResultDto
                {
                    Success = false,
                    Message = $"허용되지 않는 파일 형식입니다. ({UploadPolicy.AllowedExtensionsDisplay} 파일만 업로드할 수 있습니다.)"
                });
            }

            // 본인이 만든 프로젝트의 문서에만 파일을 올릴 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var entity = await _context.PrimaryTd.FirstOrDefaultAsync(x => x.PrjId == prjId);
                if (entity == null)
                {
                    return NotFound(new AtchDocUploadResultDto
                    {
                        Success = false,
                        Message = "기술문서를 먼저 저장한 뒤 첨부문서를 올려주세요."
                    });
                }

                // 확장자 포함 원본 파일명 (경로 조작 방지를 위해 파일명만 취함)
                var originalNm = Path.GetFileName(file.FileName);

                // 실제 저장 파일명은 충돌 방지를 위해 슬롯 + 타임스탬프 기반으로 별도 생성
                var ext = Path.GetExtension(originalNm);
                var storedNm = $"{slot}_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";

                var saveDir = UploadPolicy.GetProjectDocDirectory(_env, "td", PackLevel, prjId);
                Directory.CreateDirectory(saveDir);

                var savePath = Path.Combine(saveDir, storedNm);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // DB엔 공개 URL이 아니라 업로드 루트 기준 상대경로만 저장한다.
                // 실제 내려받기는 DownloadAtchDoc이 소유자 확인 후 처리한다.
                var relativePath = UploadPolicy.ToRelativePath("td", PackLevel, prjId, storedNm);

                SetStringProperty(entity, $"AtchDocUrl{slot}", relativePath);
                SetStringProperty(entity, $"AtchDocNm{slot}", originalNm);
                entity.LastWrtDtm = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new AtchDocUploadResultDto
                {
                    Success = true,
                    Slot = slot,
                    FileNm = originalNm,
                    FileUrl = relativePath,
                    Message = "첨부문서가 업로드되었습니다."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AtchDocUploadResultDto
                {
                    Success = false,
                    Message = "업로드 중 오류가 발생했습니다: " + ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE: api/PrimaryTd/DeleteAtchDoc?prjId=xxx&slot=1
        // 첨부문서 슬롯을 비운다. (물리 파일도 함께 삭제)
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("DeleteAtchDoc")]
        public async Task<IActionResult> DeleteAtchDoc([FromQuery] string prjId, [FromQuery] int slot, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId) || slot < 1 || slot > AtchDocSlotCount)
            {
                return BadRequest(new { success = false, message = "prjId와 slot(1~8)이 필요합니다." });
            }

            // 본인이 만든 프로젝트의 문서에서만 파일을 지울 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var entity = await _context.PrimaryTd.FirstOrDefaultAsync(x => x.PrjId == prjId);
                if (entity == null)
                {
                    return NotFound(new { success = false, message = "기술문서를 찾을 수 없습니다." });
                }

                var relativePath = GetStringProperty(entity, $"AtchDocUrl{slot}");
                if (!string.IsNullOrWhiteSpace(relativePath))
                {
                    var physical = UploadPolicy.ToPhysicalPath(_env, relativePath);
                    if (System.IO.File.Exists(physical))
                    {
                        System.IO.File.Delete(physical);
                    }
                }

                SetStringProperty(entity, $"AtchDocUrl{slot}", null);
                SetStringProperty(entity, $"AtchDocNm{slot}", null);
                entity.LastWrtDtm = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, slot, message = "첨부문서가 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "삭제 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ═════════════════════════════════════════════════════════════
        // 매핑 헬퍼
        //  DTO는 화면이 전부 텍스트 입력이라 숫자 컬럼도 string? 으로 다룬다.
        //  필드가 190개가 넘어 수기 매핑 대신 동일 이름 기준 리플렉션으로 변환한다.
        //  → DTO에 없는 엔티티 컬럼(matComplItem3~4, bom7~8 등)은 손대지 않아
        //    기존 DB 값이 그대로 보존된다.
        // ═════════════════════════════════════════════════════════════

        private static readonly PropertyInfo[] DtoProps =
            typeof(PrimaryTdDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        private static readonly Dictionary<string, PropertyInfo> EntityProps =
            typeof(PrimaryTd)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.Ordinal);

        /// <summary>저장 시 별도 처리하므로 일괄 매핑에서 제외하는 항목.
        /// 첨부문서명(AtchDocNm)은 업로드/삭제 액션에서만 바뀌어야 하므로 Save로는 못 바꾸게 막는다.</summary>
        private static readonly HashSet<string> SkipOnWrite =
            new(StringComparer.Ordinal) { nameof(PrimaryTdDto.Pkg1TechDocId), nameof(PrimaryTdDto.LastWrtDtm), nameof(PrimaryTdDto.MfrDrwUrl),
            nameof(PrimaryTdDto.AtchDocNm1), nameof(PrimaryTdDto.AtchDocNm2), nameof(PrimaryTdDto.AtchDocNm3), nameof(PrimaryTdDto.AtchDocNm4), nameof(PrimaryTdDto.AtchDocNm5), nameof(PrimaryTdDto.AtchDocNm6), nameof(PrimaryTdDto.AtchDocNm7), nameof(PrimaryTdDto.AtchDocNm8) };

        private static PrimaryTdDto ToDto(PrimaryTd entity, IWebHostEnvironment env)
        {
            var dto = new PrimaryTdDto();

            foreach (var dp in DtoProps)
            {
                if (!dp.CanWrite || !EntityProps.TryGetValue(dp.Name, out var ep))
                {
                    continue;
                }

                var value = ep.GetValue(entity);

                if (dp.PropertyType == typeof(string))
                {
                    dp.SetValue(dto, ToInvariantString(value));
                }
                else if (dp.PropertyType == ep.PropertyType)
                {
                    dp.SetValue(dto, value);
                }
            }

            // 제조 도면: DB엔 파일 경로만 있고, 화면엔 바로 그릴 수 있는 데이터 URI로 바꿔서 내려준다.
            dto.MfrDrwUrl = BuildMfrDrwDataUri(entity.MfrDrwUrl, env);

            return dto;
        }

        /// <summary>제조 도면 상대경로를 읽어 화면이 바로 그릴 수 있는 data URI로 바꾼다. 파일이 없으면 null.</summary>
        private static string? BuildMfrDrwDataUri(string? relativePath, IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var physicalPath = UploadPolicy.ToPhysicalPath(env, relativePath);
            if (!System.IO.File.Exists(physicalPath))
            {
                return null;
            }

            var bytes = System.IO.File.ReadAllBytes(physicalPath);
            return ImageDataUri.FromFile(Path.GetFileName(relativePath), Convert.ToBase64String(bytes));
        }

        private static void ApplyDtoToEntity(PrimaryTdDto dto, PrimaryTd entity)
        {
            foreach (var dp in DtoProps)
            {
                if (SkipOnWrite.Contains(dp.Name) || !EntityProps.TryGetValue(dp.Name, out var ep) || !ep.CanWrite)
                {
                    continue;
                }

                var raw = dp.GetValue(dto);

                if (dp.PropertyType == typeof(string))
                {
                    // null  = 요청에 아예 담기지 않은 항목 → 기존 DB 값을 건드리지 않는다
                    // ""    = 화면에서 값을 비운 것 → DB를 null 로 지운다
                    // (화면은 모든 항목을 빈 문자열로라도 항상 전송하므로 지우기 동작에 영향 없음)
                    if (raw is null)
                    {
                        continue;
                    }
                    ep.SetValue(entity, ConvertFromString((string)raw, ep.PropertyType));
                }
                else if (dp.PropertyType == ep.PropertyType)
                {
                    ep.SetValue(entity, raw);
                }
            }
        }

        private static string? ToInvariantString(object? value) => value switch
        {
            null => null,
            string s => s,
            decimal d => d.ToString("0.##", CultureInfo.InvariantCulture),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            _ => value.ToString()
        };

        /// <summary>화면에서 온 문자열을 엔티티 프로퍼티 타입으로 변환. 빈 값은 null.</summary>
        private static object? ConvertFromString(string? raw, Type target)
        {
            var underlying = Nullable.GetUnderlyingType(target) ?? target;

            if (underlying == typeof(string))
            {
                return string.IsNullOrWhiteSpace(raw) ? null : raw;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            // "2.4 kg", "1,200" 처럼 단위/구분자가 섞여 들어와도 숫자만 뽑아 파싱
            var cleaned = new string(raw.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
            if (cleaned.Length == 0)
            {
                return null;
            }

            if (underlying == typeof(decimal))
            {
                return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;
            }
            if (underlying == typeof(int))
            {
                return int.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var i) ? i : null;
            }
            if (underlying == typeof(long))
            {
                return long.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var l) ? l : null;
            }
            if (underlying == typeof(DateTime))
            {
                return DateTime.TryParse(raw, out var dt) ? dt : null;
            }

            return null;
        }

        private static void SetStringProperty(PrimaryTd entity, string name, string? value)
        {
            if (EntityProps.TryGetValue(name, out var p) && p.CanWrite)
            {
                p.SetValue(entity, value);
            }
        }

        private static string? GetStringProperty(PrimaryTd entity, string name) =>
            EntityProps.TryGetValue(name, out var p) ? p.GetValue(entity) as string : null;
    }
}
