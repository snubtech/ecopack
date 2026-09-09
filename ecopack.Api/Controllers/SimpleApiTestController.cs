using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ecopack.Api.Controllers
{
    [Route("api/v1/jobs")]
    [ApiController]
    public class SimpleApiTestController : ControllerBase
    {
        private readonly ILogger<SimpleApiTestController> _logger;

        public SimpleApiTestController(ILogger<SimpleApiTestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 테스트용 AI 이미지/3D 작업 생성 API
        /// </summary>
        [HttpPost]
        public IActionResult CreateTestJob([FromBody] CreateAiJobRequestDto dto)
        {
            // 📌 여기에 F9 중단점을 찍어두고 프론트엔드에서 버튼을 눌러보세요!
            _logger.LogInformation("테스트 API 호출 수신 - RequestId: {RequestId}, Prompt: {Prompt}, EcoFix: {EcoFix}",
                dto.RequestId, dto.Prompt, dto.EcoFix);

            if (string.IsNullOrEmpty(dto.Prompt))
            {
                return BadRequest(new { success = false, message = "프롬프트 내용은 필수입니다." });
            }

            // 가상의 작업 ID 생성
            string generatedJobId = "test_job_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            var response = new
            {
                requestId = dto.RequestId,
                success = true,
                message = "테스트 요청이 성공적으로 처리되었습니다.",
                job = new
                {
                    jobId = generatedJobId,
                    status = "QUEUED",
                    prompt = dto.Prompt,
                    material = dto.Material,
                    ecoFix = dto.EcoFix,
                    createdAt = DateTime.UtcNow
                }
            };

            return Ok(response);
        }
    }

    /// <summary>
    /// 요청 데이터 전송 객체 (DTO)
    /// </summary>
    public class CreateAiJobRequestDto
    {
        public string RequestId { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string EcoFix { get; set; } = string.Empty;
        public string? Image { get; set; }
    }
}