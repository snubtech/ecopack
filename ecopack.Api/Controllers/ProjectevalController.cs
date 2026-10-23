using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Support;
using ecopack.Api.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

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
        /// <summary>
        /// AI 패키지 모의평가 질문 조회 API
        /// </summary>
        ///  GetLatestEvalQuestions  packLevel:포장차수, appliedMaterial:적용소재  
        [HttpGet("GetLatestEvalQuestions")]
        public async Task<IActionResult> GetLatestEvalQuestions([FromQuery] string packLevel, [FromQuery] string appliedMaterial)
        {
            // DB 엔티티를 그대로 내려주지 않고 화면이 문항을 그리는 데 필요한 항목만 담아 보낸다
            var list = await _context.If200s
                .AsNoTracking()
                .Where(x => x.PackLevel == packLevel && x.AppliedMaterial == appliedMaterial)
                .OrderBy(x => x.DspSeq)
                .Select(x => new If200EvalQueryDto
                {
                    Idx = x.Idx,
                    asmtShtHdrId = x.AsmtShtHdrId,
                    PackLevel = x.PackLevel,
                    PackLevelNm = x.PackLevelNm,
                    AppliedMaterial = x.AppliedMaterial,
                    EcoPackLarType = x.EcoPackLarType,
                    EcoPackLarTypeNm = x.EcoPackLarTypeNm,
                    EcoPackArea = x.EcoPackArea,
                    EcoPackAreaNm = x.EcoPackAreaNm,
                    AsmtQstId = x.AsmtQstId,
                    AsmtQstNm = x.AsmtQstNm,
                    AsmtQstItemId = x.AsmtQstItemId,
                    AsmtQstItemNm = x.AsmtQstItemNm,
                    PrtAsmtQstItemId = x.PrtAsmtQstItemId,
                    NextAsmtQstId = x.NextAsmtQstId,
                    ScoringCriteria = x.ScoringCriteria,
                    NatRglAls = x.NatRglAls,
                    DsgnRecmImp = x.DsgnRecmImp
                })
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

            // 한 번에 한 프로젝트·한 포장차수의 답만 저장한다(아래에서 그 프로젝트의 기존 답을 지우고 다시 넣기 때문).
            // 그리고 본인이 만든 프로젝트여야 한다.
            var custId = User.CustId();
            var targetPrjId = saveDtos[0].prjid;
            var targetLevel = saveDtos[0].packLevel;
            if (saveDtos.Any(d => d.prjid != targetPrjId || d.packLevel != targetLevel))
            {
                return BadRequest(new { success = false, message = "한 번에 한 프로젝트의 평가만 저장할 수 있습니다." });
            }
            if (!await ProjectAccess.IsOwnerAsync(_context, targetPrjId, custId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var firstItem = saveDtos.First();

                // 점수는 화면이 보낸 값을 믿지 않고 기준표(if200)의 배점으로 다시 정한다.
                // 고른 보기(질문 ID + 보기 ID)가 그 포장차수·적용소재의 기준표에 없으면 저장하지 않는다.
                var packLevels = saveDtos.Select(d => d.packLevel).Distinct().ToList();
                var materials = saveDtos.Select(d => d.appliedMaterial).Distinct().ToList();
                var criteria = await _context.If200s
                    .AsNoTracking()
                    .Where(x => packLevels.Contains(x.PackLevel) && materials.Contains(x.AppliedMaterial))
                    .Select(x => new { x.PackLevel, x.AppliedMaterial, x.AsmtQstId, x.AsmtQstItemId, x.ScoringCriteria })
                    .ToListAsync();

                var scoreByOption = new Dictionary<(string?, string?, string?, string?), string?>();
                foreach (var c in criteria)
                {
                    scoreByOption.TryAdd((c.PackLevel, c.AppliedMaterial, c.AsmtQstId, c.AsmtQstItemId), c.ScoringCriteria);
                }

                var scores = new List<string>(saveDtos.Count);
                foreach (var dto in saveDtos)
                {
                    if (!scoreByOption.TryGetValue((dto.packLevel, dto.appliedMaterial, dto.asmtQstId, dto.asmtQstItemId), out var score))
                    {
                        return BadRequest(new { success = false, message = "평가 기준표에 없는 문항 또는 보기가 포함되어 있습니다." });
                    }
                    // 배점이 비어 있는 보기는 0점으로 본다(화면이 하던 처리와 같다)
                    scores.Add(string.IsNullOrWhiteSpace(score) ? "0" : score);
                }

                // 동일한 프로젝트 및 포장차수의 기존 평가 데이터 삭제
                var existingData = await _context.AiPkgEvalInfoBscs
                    .Where(x => x.Prjid == firstItem.prjid && x.PackLevel == firstItem.packLevel)
                    .ToListAsync();

                if (existingData.Any())
                {
                    _context.AiPkgEvalInfoBscs.RemoveRange(existingData);
                }

                // DTO 데이터를 엔티티 모델로 변환하여 일괄 추가 (NextAsmtQstId 매핑 추가)
                var newEntities = saveDtos.Select((dto, i) => new AiPkgEvalInfoBsc
                {
                    Prjid = dto.prjid,
                    Prjuserid = custId!, // 작성자는 토큰의 고객 ID 로 기록한다(화면이 보낸 값은 쓰지 않는다)
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
                    ScoringCriteria = scores[i],
                    Asmtpoint = scores[i],
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
                return this.Failure(ex, "서버 내부 오류가 발생했습니다.");
            }
        }

        [HttpGet("GetSavedEvalResults")]
        public async Task<IActionResult> GetSavedEvalResults([FromQuery] string prjId, [FromQuery] string packLevel)
        {
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { message = "필수 파라미터가 누락되었습니다." });
            }

            // 본인이 만든 프로젝트의 평가만 볼 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, User.CustId()))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                var list = await _context.AiPkgEvalInfoBscs
                    .AsNoTracking()
                    .Where(x => x.Prjid == prjId && x.PackLevel == packLevel)
                    .ToListAsync();

                // DB 엔티티를 그대로 내려주지 않고 화면에 필요한 항목만 담아 보낸다
                return Ok(list.Select(AiPkgEvalItemDto.From).ToList());
            }
            catch (Exception ex)
            {
                return this.Failure(ex, "서버 내부 오류가 발생했습니다.");
            }
        }
        /// <summary>
        /// 모의평가 최종 결과 요약 조회 API
        /// </summary>
        [HttpGet("GetEvalSummary")]
        public async Task<IActionResult> GetEvalSummary([FromQuery] string prjId, [FromQuery] string packLevel)
        {
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { message = "필수 파라미터가 누락되었습니다." });
            }

            // 본인이 만든 프로젝트의 평가만 볼 수 있다
            if (!await ProjectAccess.IsOwnerAsync(_context, prjId, User.CustId()))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            try
            {
                // 1. 저장된 평가 항목 조회
                var savedList = await _context.AiPkgEvalInfoBscs
                    .Where(x => x.Prjid == prjId && x.PackLevel == packLevel)
                    .ToListAsync();

                if (!savedList.Any())
                {
                    return Ok(new AiPkgEvalSummaryResponseDto
                    {
                        TotalScore = 0,
                        MaxScore = 100,
                        SavedItems = new List<AiPkgEvalItemDto>()
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
                    // 합계가 0이면 0점 그대로 돌려준다(예전에는 0이면 89점으로 바꿔 내려줬다)
                    TotalScore = calculatedTotalScore,
                    MaxScore = 100,
                    SavedItems = savedList.Select(AiPkgEvalItemDto.From).ToList()
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return this.Failure(ex, "서버 내부 오류가 발생했습니다.");
            }
        }

    }
}