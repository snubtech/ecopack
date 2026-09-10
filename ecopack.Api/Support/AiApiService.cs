using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ecopack.Api.Dtos;

namespace ecopack.Api.Dtos
{
    /// <summary>
    /// 외부 AI 이미지 생성 및 3D GLB 변환 서버 연동 인터페이스
    /// </summary>
    public interface IExternalAiService
    {
        /// <summary>2D AI 이미지 작업 생성</summary>
        Task<AiJobResponseWrapper> CreateAiJobAsync(CreateAiJobRequestDto dto);

        /// <summary>2D AI 이미지 작업 상태 조회</summary>
        Task<AiJobStatusDto> GetAiJobStatusAsync(string jobId);

        /// <summary>2D AI 이미지 작업 결과 URL 목록 조회</summary>
        Task<AiJobResultDto> GetAiJobResultUrlsAsync(string jobId);

        /// <summary>2D AI 이미지 결과 파일 다운로드</summary>
        Task<byte[]> DownloadAiJobResultFileAsync(string jobId, int index);

        /// <summary>2D AI 이미지 작업 취소</summary>
        Task<JobActionResponseDto> CancelAiJobAsync(string jobId);

        /// <summary>3D GLB 변환 작업 생성</summary>
        Task<GlbJobResponseWrapper> CreateGlbJobAsync(CreateGlbJobRequestDto dto);

        /// <summary>3D GLB 변환 작업 상태 조회</summary>
        Task<GlbJobStatusDto> GetGlbJobStatusAsync(string jobId);

        /// <summary>3D GLB 변환 작업 결과 URL 조회</summary>
        Task<GlbJobResultDto> GetGlbJobResultUrlAsync(string jobId);

        /// <summary>3D GLB 결과 파일 직접 다운로드</summary>
        Task<byte[]> DownloadGlbJobResultFileAsync(string jobId);

        /// <summary>3D GLB 변환 작업 취소</summary>
        Task<JobActionResponseDto> CancelGlbJobAsync(string jobId);
    }

    /// <summary>
    /// 외부 AI 이미지 생성 및 3D GLB 변환 서버 연동 서비스 구현체
    /// </summary>
    public class ExternalAiService : IExternalAiService
    {
        private readonly HttpClient _httpClient;

        public ExternalAiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- 2D AI API ---

        public async Task<AiJobResponseWrapper> CreateAiJobAsync(CreateAiJobRequestDto dto)
        {
            var res = await _httpClient.PostAsJsonAsync("/api/v1/jobs", dto);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<AiJobResponseWrapper>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }

        public async Task<AiJobStatusDto> GetAiJobStatusAsync(string jobId)
        {
            var res = await _httpClient.GetAsync($"/api/v1/jobs/{jobId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<AiJobStatusDto>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }

        public async Task<AiJobResultDto> GetAiJobResultUrlsAsync(string jobId)
        {
            var res = await _httpClient.GetAsync($"/api/v1/jobs/{jobId}/result");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<AiJobResultDto>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }

        public async Task<byte[]> DownloadAiJobResultFileAsync(string jobId, int index)
        {
            var res = await _httpClient.GetAsync($"/api/v1/jobs/{jobId}/result/file/{index}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsByteArrayAsync();
        }

        public async Task<JobActionResponseDto> CancelAiJobAsync(string jobId)
        {
            var res = await _httpClient.PostAsync($"/api/v1/jobs/{jobId}/cancel", null);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<JobActionResponseDto>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }

        // --- 3D GLB API ---

        public async Task<GlbJobResponseWrapper> CreateGlbJobAsync(CreateGlbJobRequestDto dto)
        {
            var res = await _httpClient.PostAsJsonAsync("/api/v1/glb-jobs", dto);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<GlbJobResponseWrapper>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }

        public async Task<GlbJobStatusDto> GetGlbJobStatusAsync(string jobId)
        {
            var res = await _httpClient.GetAsync($"/api/v1/glb-jobs/{jobId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<GlbJobStatusDto>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }

        public async Task<GlbJobResultDto> GetGlbJobResultUrlAsync(string jobId)
        {
            var res = await _httpClient.GetAsync($"/api/v1/glb-jobs/{jobId}/result");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<GlbJobResultDto>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }

        public async Task<byte[]> DownloadGlbJobResultFileAsync(string jobId)
        {
            var res = await _httpClient.GetAsync($"/api/v1/glb-jobs/{jobId}/result/file");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsByteArrayAsync();
        }

        public async Task<JobActionResponseDto> CancelGlbJobAsync(string jobId)
        {
            var res = await _httpClient.PostAsync($"/api/v1/glb-jobs/{jobId}/cancel", null);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<JobActionResponseDto>() ?? throw new InvalidOperationException("Failed to deserialize response.");
        }
    }
}