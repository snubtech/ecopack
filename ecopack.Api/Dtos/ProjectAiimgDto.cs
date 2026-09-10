using ecopack.Api.Data; // 💡 AiPkgEvalInfoBsc 엔티티가 있는 네임스페이스 추가
namespace ecopack.Api.Dtos
{

	// --- 2D 이미지 관련 DTO ---
	public class CreateAiJobRequestDto
	{
		public string RequestId { get; set; }  // 프로젝트번호_포장차수
		public string? Prompt { get; set; }   //  일단은 빈칸으로
		public string? Material { get; set; } //  적용소재.
		public string? EcoFix { get; set; }   //  개선안.
		public string? Image { get; set; }    //  디자인템프릿 base64 또는 이미지 데이터
	}

	public class AiJobResponseWrapper
	{
		public string RequestId { get; set; }
		public bool Success { get; set; }
		public JobInfo Job { get; set; }
	}

	public class JobInfo
	{
		public string JobId { get; set; }
		public string Status { get; set; }
	}

	public class AiJobStatusDto
	{
		public string JobId { get; set; }
		public bool Success { get; set; }
		public string Status { get; set; }
		public int Progress { get; set; }
		public string Message { get; set; }
	}

	public class AiJobResultDto
	{
		public string JobId { get; set; }
		public List<string> Urls { get; set; }
	}

	// --- 3D GLB 관련 DTO ---
	public class CreateGlbJobRequestDto
	{
		public string RequestId { get; set; }
		public string SourceImageId { get; set; }
	}

	public class GlbJobResponseWrapper
	{
		public string RequestId { get; set; }
		public bool Success { get; set; }
		public JobInfo Job { get; set; }
	}

	public class GlbJobStatusDto
	{
		public string JobId { get; set; }
		public bool Success { get; set; }
		public string Status { get; set; }
		public int Progress { get; set; }
		public string Message { get; set; }
	}

	public class GlbJobResultDto
	{
		public string JobId { get; set; }
		public string Url { get; set; }
	}

	public class JobActionResponseDto
	{
		public string JobId { get; set; }
		public bool Success { get; set; }
		public string Status { get; set; }
		public string Message { get; set; }
	}
}