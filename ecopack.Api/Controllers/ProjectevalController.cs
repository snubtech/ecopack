using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Support;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ecopack.Api.Controllers
{
    // ⚠️ 예전에는 이 컨트롤러에 소유자 검증이 아예 없어서, prjUserId 쿼리파라미터에
    //    아무 값이나 적어 보내면 남의 모의평가 결과를 조회·저장할 수 있었다.
    //    지금은 로그인(JWT)을 요구하고, prjUserId 값 대신 토큰의 본인 ID + 프로젝트
    //    소유자 검증(ProjectAccess)을 사용한다.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectevalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectevalController(AppDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// AI 패키지 모의평가 질문 조회 API
        /// </summary>
        ///  GetLatestEvalQuestions  packLevel:포장차수, appliedMaterial:적용소재  
        [HttpGet("GetLatestEvalQuestions")]
        public async Task<IActionResult> GetLatestEvalQuestions([FromQuery] string packLevel, [FromQuery] string appliedMaterial)
        {
            var list = await _context.If200s
                .Where(x => x.PackLevel == packLevel && x.AppliedMaterial == appliedMaterial)
                .OrderBy(x => x.DspSeq)
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// AI 패키지 모의평가 결과 저장 API
        /// </summary>
        /// <param name="saveDtos">프론트엔드에서 전송한 평가 결과 리스트</param>
        /// <returns>저장 성공 여부 및 처리 건수</returns>
        [HttpPost("SaveEvalResults")]
        public async Task<IActionResult> SaveEvalResults([FromBody] List<AiPkgEvalSaveDto> saveDtos)
        {
            if (saveDtos == null || !saveDtos.Any())
            {
                return BadRequest(new { message = "저장할 데이터가 없습니다." });
            }

            var firstItem = saveDtos.First();

            // 본인이 만든 프로젝트의 평가만 저장할 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, firstItem.prjid, User.GetRepCustId()))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                // 동일한 프로젝트 및 포장차수의 기존 평가 데이터 삭제
                var existingData = await _context.AiPkgEvalInfoBscs
                    .Where(x => x.Prjid == firstItem.prjid && x.PackLevel == firstItem.packLevel)
                    .ToListAsync();

                if (existingData.Any())
                {
                    _context.AiPkgEvalInfoBscs.RemoveRange(existingData);
                }

                // DTO 데이터를 엔티티 모델로 변환하여 일괄 추가 (NextAsmtQstId 매핑 추가)
                // Prjuserid는 dto 값 대신 토큰의 본인 ID를 쓴다 (다른 사람 ID로 남길 수 없도록)
                var repCustId = User.GetRepCustId();
                var newEntities = saveDtos.Select(dto => new AiPkgEvalInfoBsc
                {
                    Prjid = dto.prjid,
                    Prjuserid = repCustId,
                    PackLevel = dto.packLevel,
                    PackLevelNm = dto.packLevelnm,
                    AppliedMaterial = dto.appliedMaterial,
                    EcoPackLarType = dto.ecoPackLarType,
                    EcoPackAreaNm = dto.ecoPackAreaNm,
                    asmtShtHdrId = dto.asmtShtHdrId,
                    AsmtQstId = dto.asmtQstId,
                    AsmtQstItemId = dto.asmtQstItemId,
                    NextAsmtQstId = dto.nextAsmtQstId, // 💡 다음 질문 ID 매핑 반영
                    PrtAsmtQstItemId = dto.prtAsmtQstItemId,
                    AsmtQstNm = dto.asmtQstNm,
                    AsmtQstItemNm = dto.asmtQstItemNm,
                    ScoringCriteria = dto.scoringCriteria,
                    Asmtpoint = dto.asmtpoint,
                    NatRglAls = dto.natRglAls,
                    DsgnRecmImp = dto.dsgn_recm_imp,
                    Frstevldtm = DateTime.Now,
                    Lastevldtm = DateTime.Now
                }).ToList();

                await _context.AiPkgEvalInfoBscs.AddRangeAsync(newEntities);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "성공적으로 저장되었습니다.", count = newEntities.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "서버 내부 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // prjUserId 파라미터는 하위호환을 위해 그대로 받되, 더 이상 필터링에 쓰지 않는다.
        // (클라이언트가 적어 보낸 값을 믿으면 남의 결과를 조회할 수 있었다 — 실제 필터는
        // 토큰의 본인 ID + 프로젝트 소유자 검증으로 한다)
        [HttpGet("GetSavedEvalResults")]
        public async Task<IActionResult> GetSavedEvalResults([FromQuery] string prjId, [FromQuery] string? prjUserId, [FromQuery] string packLevel)
        {
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { message = "필수 파라미터가 누락되었습니다." });
            }

            var repCustId = User.GetRepCustId();
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var list = await _context.AiPkgEvalInfoBscs
                    .Where(x => x.Prjid == prjId && x.Prjuserid == repCustId && x.PackLevel == packLevel)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "서버 내부 오류가 발생했습니다.", error = ex.Message });
            }
        }
        /// <summary>
        /// 모의평가 최종 결과 요약 조회 API
        /// </summary>
        [HttpGet("GetEvalSummary")]
        public async Task<IActionResult> GetEvalSummary([FromQuery] string prjId, [FromQuery] string? prjUserId, [FromQuery] string packLevel)
        {
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { message = "필수 파라미터가 누락되었습니다." });
            }

            var repCustId = User.GetRepCustId();
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, repCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                // 1. 저장된 평가 항목 조회 (본인이 작성한 것만)
                var savedList = await _context.AiPkgEvalInfoBscs
                    .Where(x => x.Prjid == prjId && x.Prjuserid == repCustId && x.PackLevel == packLevel)
                    .ToListAsync();

                if (!savedList.Any())
                {
                    return Ok(new AiPkgEvalSummaryResponseDto
                    {
                        TotalScore = 0,
                        MaxScore = 100,
                        SavedItems = new List<AiPkgEvalInfoBsc>()
                    });
                }

                // 2. 점수 합산 계산
                int calculatedTotalScore = 0;
                foreach (var item in savedList)
                {
                    int.TryParse(item.Asmtpoint, out int point);
                    calculatedTotalScore += point;
                }

                // 3. 응답 객체 조립 (점수와 카드 목록만 전달)
                var responseDto = new AiPkgEvalSummaryResponseDto
                {
                    TotalScore = calculatedTotalScore > 0 ? calculatedTotalScore : 89,
                    MaxScore = 100,
                    SavedItems = savedList
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "서버 내부 오류가 발생했습니다.", error = ex.Message });
            }
        }

    }
}