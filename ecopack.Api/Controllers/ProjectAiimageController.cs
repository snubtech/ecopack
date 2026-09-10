//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//[ApiController]
//[Route("api/projects/ai-jobs")]
//public class ProjectAiimageController : ControllerBase
//{
//    private readonly IAiApiService _aiService;
//    private readonly ApplicationDbContext _context;

//    public ProjectAiimageController(IAiApiService aiService, ApplicationDbContext context)
//    {
//        _aiService = aiService;
//        _context = context;
//    }

//    // --- 2D AI 이미지 작업 엔드포인트 ---

//    [HttpPost("ai/trigger")]
//    public async Task<IActionResult> TriggerAiJob([FromBody] AiJobTriggerRequestDto dto)
//    {
//        var externalRes = await _aiService.CreateAiJobAsync(new CreateAiJobRequestDto
//        {
//            RequestId = dto.RequestId,
//            Prompt = dto.Prompt,
//            Material = dto.Material,
//            EcoFix = dto.EcoFix,
//            Image = dto.Image
//        });

//        var report = await _context.ProjectDetailReport
//            .FirstOrDefaultAsync(x => x.PrjId == dto.PrjId && x.PackLevel == dto.PackLevel && x.Prjuserid == dto.PrjUserId);

//        if (report != null)
//        {
//            report.AiJobId = externalRes.Job.JobId;
//            report.AiJobStatus = externalRes.Job.Status;
//            await _context.SaveChangesAsync();
//        }

//        return Ok(new { success = true, jobId = externalRes.Job.JobId, status = externalRes.Job.Status });
//    }

//    [HttpGet("ai/{jobId}/sync")]
//    public async Task<IActionResult> SyncAiJob(
//        string jobId, 
//        [FromQuery] string prjId, 
//        [FromQuery] string packLevel, 
//        [FromQuery] string prjUserId,
//        [FromQuery] int index = 0)
//    {
//        var statusRes = await _aiService.GetAiJobStatusAsync(jobId);

//        var report = await _context.ProjectDetailReport
//            .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel && x.Prjuserid == prjUserId);

//        if (report == null) return NotFound(new { success = false, message = "리포트를 찾을 수 없습니다." });

//        report.AiJobStatus = statusRes.Status;

//        if (statusRes.Status == "SUCCEEDED" || statusRes.Status == "COMPLETE")
//        {
//            var fileBytes = await _aiService.DownloadAiJobResultFileAsync(jobId, index);
//            string savedPath = SaveFileLocally(fileBytes, jobId, $"ai_{index}", "png");
//            report.AiResultFilePath = savedPath;
//        }

//        await _context.SaveChangesAsync();

//        return Ok(new { success = true, status = statusRes.Status, progress = statusRes.Progress, filePath = report.AiResultFilePath });
//    }

//    [HttpPost("ai/{jobId}/cancel")]
//    public async Task<IActionResult> CancelAiJob(string jobId, [FromQuery] string prjId, [FromQuery] string packLevel, [FromQuery] string prjUserId)
//    {
//        var cancelRes = await _aiService.CancelAiJobAsync(jobId);

//        var report = await _context.ProjectDetailReport
//            .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel && x.Prjuserid == prjUserId);

//        if (report != null)
//        {
//            report.AiJobStatus = cancelRes.Status;
//            await _context.SaveChangesAsync();
//        }

//        return Ok(cancelRes);
//    }

//    // --- 3D GLB 작업 엔드포인트 ---

//    [HttpPost("glb/trigger")]
//    public async Task<IActionResult> TriggerGlbJob([FromBody] GlbJobTriggerRequestDto dto)
//    {
//        var externalRes = await _aiService.CreateGlbJobAsync(new CreateGlbJobRequestDto
//        {
//            RequestId = dto.RequestId,
//            SourceImageId = dto.SourceImageId
//        });

//        var report = await _context.ProjectDetailReport
//            .FirstOrDefaultAsync(x => x.PrjId == dto.PrjId && x.PackLevel == dto.PackLevel && x.Prjuserid == dto.PrjUserId);

//        if (report != null)
//        {
//            report.GlbJobId = externalRes.Job.JobId;
//            report.GlbJobStatus = externalRes.Job.Status;
//            await _context.SaveChangesAsync();
//        }

//        return Ok(new { success = true, jobId = externalRes.Job.JobId, status = externalRes.Job.Status });
//    }

//    [HttpGet("glb/{jobId}/sync")]
//    public async Task<IActionResult> SyncGlbJob(
//        string jobId, 
//        [FromQuery] string prjId, 
//        [FromQuery] string packLevel, 
//        [FromQuery] string prjUserId)
//    {
//        var statusRes = await _aiService.GetGlbJobStatusAsync(jobId);

//        var report = await _context.ProjectDetailReport
//            .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel && x.Prjuserid == prjUserId);

//        if (report == null) return NotFound(new { success = false, message = "리포트를 찾을 수 없습니다." });

//        report.GlbJobStatus = statusRes.Status;

//        if (statusRes.Status == "SUCCEEDED" || statusRes.Status == "COMPLETE")
//        {
//            var fileBytes = await _aiService.DownloadGlbJobResultFileAsync(jobId);
//            string savedPath = SaveFileLocally(fileBytes, jobId, "glb_model", "glb");
//            report.GlbResultFilePath = savedPath;
//        }

//        await _context.SaveChangesAsync();

//        return Ok(new { success = true, status = statusRes.Status, progress = statusRes.Progress, filePath = report.GlbResultFilePath });
//    }

//    [HttpPost("glb/{jobId}/cancel")]
//    public async Task<IActionResult> CancelGlbJob(string jobId, [FromQuery] string prjId, [FromQuery] string packLevel, [FromQuery] string prjUserId)
//    {
//        var cancelRes = await _aiService.CancelGlbJobAsync(jobId);

//        var report = await _context.ProjectDetailReport
//            .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel && x.Prjuserid == prjUserId);

//        if (report != null)
//        {
//            report.GlbJobStatus = cancelRes.Status;
//            await _context.SaveChangesAsync();
//        }

//        return Ok(cancelRes);
//    }

//    private string SaveFileLocally(byte[] bytes, string jobId, string prefix, string extension)
//    {
//        string folderPath = Path.Combine("wwwroot", "uploads", "results");
//        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

//        string fileName = $"{prefix}_{jobId}.{extension}";
//        string fullPath = Path.Combine(folderPath, fileName);
//        System.IO.File.WriteAllBytes(fullPath, bytes);

//        return $"/uploads/results/{fileName}";
//    }
//}