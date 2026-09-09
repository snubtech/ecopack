//using System.Net.Http.Json;

//public interface IExternalAiService
//{
//    Task<AiJobResponseWrapper> CreateAiJobAsync(CreateAiJobRequestDto dto);
//    Task<AiJobStatusDto> GetAiJobStatusAsync(string jobId);
//    Task<AiJobResultDto> GetAiJobResultUrlsAsync(string jobId);
//    Task<byte[]> DownloadAiJobResultFileAsync(string jobId, int index);
//    Task<JobActionResponseDto> CancelAiJobAsync(string jobId);

//    Task<GlbJobResponseWrapper> CreateGlbJobAsync(CreateGlbJobRequestDto dto);
//    Task<GlbJobStatusDto> GetGlbJobStatusAsync(string jobId);
//    Task<GlbJobResultDto> GetGlbJobResultUrlAsync(string jobId);
//    Task<byte[]> DownloadGlbJobResultFileAsync(string jobId);
//    Task<JobActionResponseDto> CancelGlbJobAsync(string jobId);
//}

//public class ExternalAiService : IExternalAiService
//{
//    private readonly HttpClient _httpClient;

//    public ExternalAiService(HttpClient httpClient)
//    {
//        _httpClient = httpClient;
//    }

//    // 2D AI
//    public async Task<AiJobResponseWrapper> CreateAiJobAsync(CreateAiJobRequestDto dto)
//    {
//        var res = await _httpClient.PostAsJsonAsync("/api/v1/jobs", dto);
//        res.EnsureSuccessStatusCode();
//        return await res.Content.ReadFromJsonAsync<AiJobResponseWrapper>() ?? throw new InvalidOperationException();
//    }

//    public async Task<AiJobStatusDto> GetAiJobStatusAsync(string jobId) =>
//        await _httpClient.GetFromJsonAsync<AiJobStatusDto>($"/api/v1/jobs/{jobId}");

//    public async Task<AiJobResultDto> GetAiJobResultUrlsAsync(string jobId) =>
//        await _httpClient.GetFromJsonAsync<AiJobResultDto>($"/api/v1/jobs/{jobId}/result");

//    public async Task<byte[]> DownloadAiJobResultFileAsync(string jobId, int index)
//    {
//        var res = await _httpClient.GetAsync($"/api/v1/jobs/{jobId}/result/file/{index}");
//        res.EnsureSuccessStatusCode();
//        return await res.Content.ReadAsByteArrayAsync();
//    }

//    public async Task<JobActionResponseDto> CancelAiJobAsync(string jobId)
//    {
//        var res = await _httpClient.PostAsync($"/api/v1/jobs/{jobId}/cancel", null);
//        res.EnsureSuccessStatusCode();
//        return await res.Content.ReadFromJsonAsync<JobActionResponseDto>() ?? throw new InvalidOperationException();
//    }

//    // 3D GLB
//    public async Task<GlbJobResponseWrapper> CreateGlbJobAsync(CreateGlbJobRequestDto dto)
//    {
//        var res = await _httpClient.PostAsJsonAsync("/api/v1/glb-jobs", dto);
//        res.EnsureSuccessStatusCode();
//        return await res.Content.ReadFromJsonAsync<GlbJobResponseWrapper>() ?? throw new InvalidOperationException();
//    }

//    public async Task<GlbJobStatusDto> GetGlbJobStatusAsync(string jobId) =>
//        await _httpClient.GetFromJsonAsync<GlbJobStatusDto>($"/api/v1/glb-jobs/{jobId}");

//    public async Task<GlbJobResultDto> GetGlbJobResultUrlAsync(string jobId) =>
//        await _httpClient.GetFromJsonAsync<GlbJobResultDto>($"/api/v1/glb-jobs/{jobId}/result");

//    public async Task<byte[]> DownloadGlbJobResultFileAsync(string jobId)
//    {
//        var res = await _httpClient.GetAsync($"/api/v1/glb-jobs/{jobId}/result/file");
//        res.EnsureSuccessStatusCode();
//        return await res.Content.ReadAsByteArrayAsync();
//    }

//    public async Task<JobActionResponseDto> CancelGlbJobAsync(string jobId)
//    {
//        var res = await _httpClient.PostAsync($"/api/v1/glb-jobs/{jobId}/cancel", null);
//        res.EnsureSuccessStatusCode();
//        return await res.Content.ReadFromJsonAsync<JobActionResponseDto>() ?? throw new InvalidOperationException();
//    }
//}