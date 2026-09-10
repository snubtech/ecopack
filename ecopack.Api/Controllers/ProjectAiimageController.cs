using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Services;

namespace ecopack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectAiimageController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IExternalAiService _aiExternalService;

        public ProjectAiimageController(AppDbContext dbContext, IExternalAiService aiExternalService)
        {
            _dbContext = dbContext;
            _aiExternalService = aiExternalService;
        }

        [HttpPost("GetDesignTemplate")]
        public async Task<IActionResult> GetDesignTemplate([FromBody] DesignTemplateRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.PrjId) || string.IsNullOrEmpty(request.PackLevel))
            {
                return BadRequest(new { message = "프로젝트 ID 또는 패키지 레벨이 누락되었습니다." });
            }

            var projectDetail = await _dbContext.ProjectDetail
                .FirstOrDefaultAsync(pd => pd.PrjId == request.PrjId && pd.PackLevel == request.PackLevel);

            if (projectDetail == null || string.IsNullOrEmpty(projectDetail.PackDsgnTplId))
            {
                return NotFound(new { message = "해당하는 디자인 템플릿 정보를 찾을 수 없습니다." });
            }

            var if02Data = await _dbContext.If002a
                .FirstOrDefaultAsync(if02 => if02.PackDsgnTplId == projectDetail.PackDsgnTplId);

            if (if02Data == null)
            {
                return NotFound(new { message = "해당하는 디자인 템플릿 데이터를 찾을 수 없습니다." });
            }

            return Ok(new { fileData = if02Data.FileData });
        }

        [HttpPost("Generate2DImage")]
        public async Task<IActionResult> Generate2DImage([FromBody] Generate2DRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.PrjId) || string.IsNullOrEmpty(request.PackLevel) || string.IsNullOrEmpty(request.AppliedMaterial))
            {
                return BadRequest(new { message = "요청 파라미터 정보가 누락되었습니다." });
            }

            var evalList = await _dbContext.AiPkgEvalInfoBscs
                .Where(x => x.Prjid == request.PrjId
                            && x.PackLevel == request.PackLevel
                            && x.AppliedMaterial == request.AppliedMaterial)
                .ToListAsync();

            evalList = evalList
                .Where(x => !string.IsNullOrEmpty(x.DsgnRecmImp))
                .OrderByDescending(x => x.EcoPackLarType)
                .ToList();

            string ecoFix = string.Join("; ", evalList
                .GroupBy(x => x.EcoPackLarType)
                .Select(g => $"{g.Key}=={string.Join(" / ", g.Select(x => x.DsgnRecmImp?.Replace("\r", "").Replace("\n", " ") ?? string.Empty))}"));

            var projectDetail = await _dbContext.ProjectDetail
                .FirstOrDefaultAsync(pd => pd.PrjId == request.PrjId && pd.PackLevel == request.PackLevel);

            string templateData = string.Empty;
            if (projectDetail != null && !string.IsNullOrEmpty(projectDetail.PackDsgnTplId))
            {
                var if02Data = await _dbContext.If002a
                    .FirstOrDefaultAsync(if02 => if02.PackDsgnTplId == projectDetail.PackDsgnTplId);

                if (if02Data != null)
                {
                    templateData = if02Data.FileData ?? string.Empty;
                }
            }

            var requestDto = new ecopack.Api.Dtos.CreateAiJobRequestDto
            {
                RequestId = $"{request.PrjId}_{request.PackLevel}",
                Prompt = !string.IsNullOrEmpty(ecoFix) ? string.Empty : $"{request.AppliedMaterial} packaging design update",
                Material = request.AppliedMaterial,
                EcoFix = ecoFix,
                Image = templateData
            };

            AiJobResponseWrapper apiResponse = await _aiExternalService.CreateAiJobAsync(requestDto);

            if (apiResponse == null || !apiResponse.Success)
            {
                Console.WriteLine($"[AI API 오류 발생] Response: {System.Text.Json.JsonSerializer.Serialize(apiResponse)}");
                return StatusCode(500, new { message = "외부 AI 이미지 생성 API 호출에 실패했습니다.", response = apiResponse });
            }

            var aiImageEntity = new ProjectAiImage
            {
                PrjId = request.PrjId,
                PackLevel = request.PackLevel,
                JobId = apiResponse.Job?.JobId ?? string.Empty,
                Status = apiResponse.Job?.Status ?? string.Empty,
                CreatedAt = DateTime.Now
            };

            _dbContext.ProjectAiImages.Add(aiImageEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                jobId = apiResponse.Job?.JobId,
                status = apiResponse.Job?.Status,
                message = "AI 이미지 생성 작업 요청 및 저장이 완료되었습니다."
            });
        }

        [HttpPost("GetAiJobStatus")]
        public async Task<IActionResult> GetAiJobStatus([FromBody] AiJobStatusRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.JobId))
            {
                return BadRequest(new { message = "작업 ID(JobId)가 누락되었습니다." });
            }

            try
            {
                AiJobStatusDto statusResponse = await _aiExternalService.GetAiJobStatusAsync(request.JobId);

                if (statusResponse == null)
                {
                    return StatusCode(500, new { message = "AI 작업 상태 조회 응답이 비어 있습니다." });
                }

                var aiImageEntity = await _dbContext.ProjectAiImages
                    .FirstOrDefaultAsync(x => x.JobId == request.JobId);

                if (aiImageEntity != null)
                {
                    aiImageEntity.Status = statusResponse.Status ?? aiImageEntity.Status;
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = statusResponse.Success,
                    jobId = statusResponse.JobId,
                    status = statusResponse.Status,
                    progress = statusResponse.Progress,
                    message = statusResponse.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI 상태 조회 오류] {ex.Message}");
                return StatusCode(500, new { message = "AI 작업 상태 조회 중 오류가 발생했습니다.", error = ex.Message });
            }
        }
    }

    public class DesignTemplateRequestDto
    {
        public string? PrjId { get; set; }
        public string? PackLevel { get; set; }
    }

    public class Generate2DRequestDto
    {
        public string? PrjId { get; set; }
        public string? PackLevel { get; set; }
        public string? AppliedMaterial { get; set; }
    }

    public class AiJobStatusRequestDto
    {
        public string? JobId { get; set; }
    }
}