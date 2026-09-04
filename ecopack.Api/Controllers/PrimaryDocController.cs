/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - PrimaryDocController (적합성 선언서)
 * ==============================================================================
 * 
 * 1. 담당 범위
 *    - 적합성 선언서 화면이 쓰는 API 입니다. 대상 테이블은 primary_doc(1차포장적합성선언서기본) 입니다.
 *    - 문서는 프로젝트 단위로 한 건이며, 프로젝트 ID(prjId)로 찾습니다.
 * 
 * 2. Get — 문서 조회
 *    - 아직 작성 전이면 isNew=true 와 빈 문서를 돌려줍니다.
 *    - 이때 같은 프로젝트의 기술문서가 있으면 그 문서 번호를 미리 담아 보내
 *      화면에서 기술문서 번호가 자동으로 연결되게 합니다.
 * 
 * 3. Save — 신규/수정 통합 저장 (Upsert)
 *    - 신규일 때 문서 ID를 DOC-{포장차수}-{yyyyMMddHHmmssfff} 규칙으로 채번합니다.
 *    - 개정번호(revNo)는 Rev.01, 물질 총합행 라벨(sbstTot)은 '총합' 을 기본값으로 채웁니다.
 *    - 기술문서 번호가 비어 있으면 같은 프로젝트의 기술문서를 찾아 연결합니다.
 *    - 저장할 때마다 발행일(lastWrtDt)을 서버 현재 날짜로 갱신합니다.
 * 
 * 4. UploadEvdDoc / DeleteEvdDoc — 근거문서(부속서)
 *    - 파일은 wwwroot/uploads/doc/{프로젝트ID}/ 아래에 두고,
 *      evdDocUrl{슬롯} 에 경로를, evdDocNm{슬롯} 에 확장자 포함 원본 파일명을 기록합니다.
 * 
 * 5. DTO ↔ 엔티티 매핑 (ToDto / ApplyDtoToEntity)
 *    - 이름이 같은 것끼리 리플렉션으로 옮깁니다.
 *    - 값이 null 이면 기존 DB 값을 두고, 빈 문자열이면 DB를 비웁니다.
 *    - primary_td 와 달리 컬럼이 모두 varchar(길이 제한)라, 화면에서도 입력 길이를 함께 제한합니다.
 * ==============================================================================
 */
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 1차포장 적합성선언서(primary_doc / DOC) 화면용 API.
    /// 라우트: api/PrimaryDoc
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PrimaryDocController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        /// <summary>포장차수 고정값. primary=1 / secondary=2 / tertiary=3</summary>
        private const string PackLevel = "1";

        /// <summary>근거문서(부속서) 슬롯 개수 (evdDocNm1~8 / evdDocUrl1~8)</summary>
        private const int EvdDocSlotCount = 8;

        private const long MaxEvdDocBytes = 20 * 1024 * 1024; // 20MB

        public PrimaryDocController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ─────────────────────────────────────────────────────────────
        // 채번: DOC-{차수}-{yyyyMMddHHmmssfff}
        // ─────────────────────────────────────────────────────────────
        private static string NewDocId() =>
            $"DOC-{PackLevel}-{DateTime.Now:yyyyMMddHHmmssfff}";

        // ─────────────────────────────────────────────────────────────
        // GET: api/PrimaryDoc/Get?prjId=xxx
        // 해당 프로젝트의 적합성선언서를 조회한다. 없으면 빈 DTO(신규 작성용)를 돌려준다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("Get")]
        public async Task<IActionResult> Get([FromQuery] string prjId)
        {
            if (string.IsNullOrWhiteSpace(prjId))
            {
                return BadRequest(new { success = false, message = "필수 파라미터(prjId)가 누락되었습니다." });
            }

            try
            {
                var entity = await _context.PrimaryDoc
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PrjId == prjId);

                if (entity == null)
                {
                    // 신규: 화면이 고정문구 기본값을 채워 넣을 수 있도록 키만 담아서 반환.
                    // 기술문서(primary_td)가 이미 있으면 그 ID를 미리 연결해 준다.
                    var techDocId = await _context.PrimaryTd
                        .AsNoTracking()
                        .Where(x => x.PrjId == prjId)
                        .Select(x => x.Pkg1TechDocId)
                        .FirstOrDefaultAsync();

                    return Ok(new
                    {
                        success = true,
                        isNew = true,
                        data = new PrimaryDocDto { PrjId = prjId, Pkg1TechDocId = techDocId }
                    });
                }

                return Ok(new { success = true, isNew = false, data = ToDto(entity) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "조회 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/PrimaryDoc/Save
        // 신규/수정 통합 저장(Upsert). 저장 시 lastWrtDt(발행일)를 현재 날짜로 갱신한다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] PrimaryDocDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PrjId))
            {
                return BadRequest(new { success = false, message = "필수 값(prjId)이 누락되었습니다." });
            }

            try
            {
                var entity = await _context.PrimaryDoc
                    .FirstOrDefaultAsync(x => x.PrjId == dto.PrjId);

                var isNew = entity == null;

                if (isNew)
                {
                    entity = new PrimaryDoc
                    {
                        Pkg1DocId = string.IsNullOrWhiteSpace(dto.Pkg1DocId)
                            ? NewDocId()
                            : dto.Pkg1DocId
                    };
                    _context.PrimaryDoc.Add(entity);
                }

                ApplyDtoToEntity(dto, entity!);

                // 개정번호 초기값
                if (string.IsNullOrWhiteSpace(entity!.RevNo))
                {
                    entity.RevNo = "Rev.01";
                }

                // 물질 총합행 라벨 기본값 (DB DEFAULT '총합' 과 동일)
                if (string.IsNullOrWhiteSpace(entity.SbstTot))
                {
                    entity.SbstTot = "총합";
                }

                // 기술문서가 있으면 연결 ID를 채워 둔다
                if (string.IsNullOrWhiteSpace(entity.Pkg1TechDocId))
                {
                    entity.Pkg1TechDocId = await _context.PrimaryTd
                        .AsNoTracking()
                        .Where(x => x.PrjId == dto.PrjId)
                        .Select(x => x.Pkg1TechDocId)
                        .FirstOrDefaultAsync();
                }

                // 발행일은 항상 서버 기준 현재 날짜로 갱신
                entity.LastWrtDt = DateOnly.FromDateTime(DateTime.Now);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    isNew,
                    pkg1DocId = entity.Pkg1DocId,
                    lastWrtDt = entity.LastWrtDt,
                    message = isNew ? "적합성선언서가 생성되었습니다." : "적합성선언서가 저장되었습니다.",
                    data = ToDto(entity)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "저장 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/PrimaryDoc/UploadEvdDoc   (multipart/form-data)
        // 근거문서(부속서)를 업로드하고 evdDocUrl{slot} / evdDocNm{slot} 에 반영한다.
        // 문서명은 확장자를 포함한 원본 파일명 그대로 저장한다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("UploadEvdDoc")]
        [RequestSizeLimit(MaxEvdDocBytes)]
        public async Task<IActionResult> UploadEvdDoc(
            [FromForm] string prjId,
            [FromForm] int slot,
            IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(prjId))
            {
                return BadRequest(new EvdDocUploadResultDto { Success = false, Message = "prjId가 필요합니다." });
            }
            if (slot < 1 || slot > EvdDocSlotCount)
            {
                return BadRequest(new EvdDocUploadResultDto { Success = false, Message = $"slot은 1~{EvdDocSlotCount} 범위여야 합니다." });
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest(new EvdDocUploadResultDto { Success = false, Message = "업로드할 파일이 없습니다." });
            }
            if (file.Length > MaxEvdDocBytes)
            {
                return BadRequest(new EvdDocUploadResultDto { Success = false, Message = "파일 크기는 20MB를 넘을 수 없습니다." });
            }

            try
            {
                var entity = await _context.PrimaryDoc.FirstOrDefaultAsync(x => x.PrjId == prjId);
                if (entity == null)
                {
                    return NotFound(new EvdDocUploadResultDto
                    {
                        Success = false,
                        Message = "적합성선언서를 먼저 저장한 뒤 근거문서를 올려주세요."
                    });
                }

                // 확장자 포함 원본 파일명 (경로 조작 방지를 위해 파일명만 취함)
                var originalNm = Path.GetFileName(file.FileName);

                // 실제 저장 파일명은 충돌 방지를 위해 슬롯 + 타임스탬프 기반으로 별도 생성
                var ext = Path.GetExtension(originalNm);
                var storedNm = $"{slot}_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";

                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var saveDir = Path.Combine(webRoot, "uploads", "doc", prjId);
                Directory.CreateDirectory(saveDir);

                var savePath = Path.Combine(saveDir, storedNm);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/uploads/doc/{prjId}/{storedNm}";

                SetStringProperty(entity, $"EvdDocUrl{slot}", url);
                SetStringProperty(entity, $"EvdDocNm{slot}", originalNm);
                entity.LastWrtDt = DateOnly.FromDateTime(DateTime.Now);

                await _context.SaveChangesAsync();

                return Ok(new EvdDocUploadResultDto
                {
                    Success = true,
                    Slot = slot,
                    FileNm = originalNm,
                    FileUrl = url,
                    Message = "근거문서가 업로드되었습니다."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new EvdDocUploadResultDto
                {
                    Success = false,
                    Message = "업로드 중 오류가 발생했습니다: " + ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE: api/PrimaryDoc/DeleteEvdDoc?prjId=xxx&slot=1
        // 근거문서 슬롯을 비운다. (물리 파일도 함께 삭제)
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("DeleteEvdDoc")]
        public async Task<IActionResult> DeleteEvdDoc([FromQuery] string prjId, [FromQuery] int slot)
        {
            if (string.IsNullOrWhiteSpace(prjId) || slot < 1 || slot > EvdDocSlotCount)
            {
                return BadRequest(new { success = false, message = "prjId와 slot(1~8)이 필요합니다." });
            }

            try
            {
                var entity = await _context.PrimaryDoc.FirstOrDefaultAsync(x => x.PrjId == prjId);
                if (entity == null)
                {
                    return NotFound(new { success = false, message = "적합성선언서를 찾을 수 없습니다." });
                }

                var url = GetStringProperty(entity, $"EvdDocUrl{slot}");
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                    var physical = Path.Combine(webRoot, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(physical))
                    {
                        System.IO.File.Delete(physical);
                    }
                }

                SetStringProperty(entity, $"EvdDocUrl{slot}", null);
                SetStringProperty(entity, $"EvdDocNm{slot}", null);
                entity.LastWrtDt = DateOnly.FromDateTime(DateTime.Now);

                await _context.SaveChangesAsync();

                return Ok(new { success = true, slot, message = "근거문서가 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "삭제 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ═════════════════════════════════════════════════════════════
        // 매핑 헬퍼 — PrimaryTdController 와 동일한 방식
        //  DTO는 전부 string? 이고, 동일 이름 기준 리플렉션으로 엔티티에 옮긴다.
        //  null  = 요청에 담기지 않은 항목 → 기존 DB 값 유지
        //  ""    = 화면에서 비운 항목 → DB를 null 로 지움
        // ═════════════════════════════════════════════════════════════

        private static readonly PropertyInfo[] DtoProps =
            typeof(PrimaryDocDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        private static readonly Dictionary<string, PropertyInfo> EntityProps =
            typeof(PrimaryDoc)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.Ordinal);

        /// <summary>저장 시 별도 처리하므로 일괄 매핑에서 제외하는 항목</summary>
        private static readonly HashSet<string> SkipOnWrite =
            new(StringComparer.Ordinal) { nameof(PrimaryDocDto.Pkg1DocId), nameof(PrimaryDocDto.LastWrtDt) };

        private static PrimaryDocDto ToDto(PrimaryDoc entity)
        {
            var dto = new PrimaryDocDto();

            foreach (var dp in DtoProps)
            {
                if (!dp.CanWrite || !EntityProps.TryGetValue(dp.Name, out var ep))
                {
                    continue;
                }

                var value = ep.GetValue(entity);

                if (dp.PropertyType == typeof(string))
                {
                    dp.SetValue(dto, value as string);
                }
                else if (dp.PropertyType == ep.PropertyType)
                {
                    dp.SetValue(dto, value);
                }
            }

            return dto;
        }

        private static void ApplyDtoToEntity(PrimaryDocDto dto, PrimaryDoc entity)
        {
            foreach (var dp in DtoProps)
            {
                if (SkipOnWrite.Contains(dp.Name) || !EntityProps.TryGetValue(dp.Name, out var ep) || !ep.CanWrite)
                {
                    continue;
                }

                if (dp.PropertyType != typeof(string) || ep.PropertyType != typeof(string))
                {
                    continue;
                }

                // null 은 "요청에 없음" 이므로 건드리지 않는다
                if (dp.GetValue(dto) is not string raw)
                {
                    continue;
                }

                ep.SetValue(entity, string.IsNullOrWhiteSpace(raw) ? null : raw);
            }
        }

        private static void SetStringProperty(PrimaryDoc entity, string name, string? value)
        {
            if (EntityProps.TryGetValue(name, out var p) && p.CanWrite)
            {
                p.SetValue(entity, value);
            }
        }

        private static string? GetStringProperty(PrimaryDoc entity, string name) =>
            EntityProps.TryGetValue(name, out var p) ? p.GetValue(entity) as string : null;
    }
}
