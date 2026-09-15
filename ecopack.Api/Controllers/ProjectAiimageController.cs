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
        private readonly ILogger<ProjectAiimageController> _logger; // 1. 로거 선언

        public ProjectAiimageController(AppDbContext dbContext, IExternalAiService aiExternalService, ILogger<ProjectAiimageController> logger)
        {
            _dbContext = dbContext;
            _aiExternalService = aiExternalService;
            _logger = logger; // 2. 생성자 주입 완료
        }

        /// <summary>
        /// [페이지 로드 시] ProjectAiImages 존재 여부를 먼저 확인하고, 
        /// 없으면 기본 템플릿을, 있으면 2D/3D의 전체 상태 및 결과 이미지를 함께 조회하여 반환
        /// </summary>
        [HttpGet("GetDesignTemplate")]
        public async Task<IActionResult> GetDesignTemplate([FromQuery] string prjId, [FromQuery] string packLevel)
        {
            // 1. 필수 파라미터 유효성 검증
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { success = false, message = "프로젝트 ID 또는 패키지 레벨이 누락되었습니다." });
            }

            // 2. ProjectDetail 조회 및 템플릿 ID 확인 (먼저 시작되어야 하는 필수 로직)
            var projectDetail = await _dbContext.ProjectDetail
                .FirstOrDefaultAsync(pd => pd.PrjId == prjId && pd.PackLevel == packLevel);

            if (projectDetail == null || string.IsNullOrEmpty(projectDetail.PackDsgnTplId))
            {
                return NotFound(new { success = false, message = "해당하는 디자인 템플릿 정보를 찾을 수 없습니다." });
            }

            // 3. 템플릿 원본 파일(If002a 등) 조회
            var if02Data = await _dbContext.If002a
                .FirstOrDefaultAsync(if02 => if02.PackDsgnTplId == projectDetail.PackDsgnTplId);

            // 4. AI 이미지 작업 이력 조회 (_dbContext 및 ProjectAiImages로 수정)
            var aiImageEntity = await _dbContext.ProjectAiImages
                .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel);

            // 5. 최종 응답 데이터 구성 (기본 템플릿과 AI 상태 정보 통합)
            var response = new
            {
                success = true,
                prjId = prjId,
                packLevel = packLevel,

                // 기본 템플릿 파일 데이터 (If002a 테이블의 파일 데이터)
                fileData = if02Data?.FileData,

                // AI 작업 상태 정보들 (이력이 없으면 기본값 세팅)
                existsInAiImages = aiImageEntity != null,  //데이타있으면 true, 없으면 false
                jobId = aiImageEntity?.JobId,
                status = aiImageEntity?.Status ?? "NONE",
                progress = aiImageEntity?.Progress ?? 0,
                isSuccess = aiImageEntity?.Success ?? false,
                statusMessage = aiImageEntity?.StatusMessage,
                errorMessage = aiImageEntity?.ErrorMessage,
                req2ddate = aiImageEntity?.req2ddate,
                req3ddate = aiImageEntity?.req3ddate,

                glbJobId = aiImageEntity?.GlbJobId,
                status3d = aiImageEntity?.Status3d ?? "NONE",
                progress3d = aiImageEntity?.Progress3d ?? 0,
                isSuccess3d = aiImageEntity?.Success3d ?? false,
                statusMessage3d = aiImageEntity?.StatusMessage3d,
                errorMessage3d = aiImageEntity?.ErrorMessage3d,

                // 2D 결과 이미지들
                resultDataOriginal = aiImageEntity?.ResultDataOriginal,
                resultDataModerate = aiImageEntity?.ResultDataModerate,
                resultDataRedesign = aiImageEntity?.ResultDataRedesign,

                // 3D GLB 변환요청 시간
                req3ddateoriginal = aiImageEntity?.req3ddateoriginal,
                req3ddatemoderate = aiImageEntity?.req3ddatemoderate,
                req3ddateredesign = aiImageEntity?.req3ddateredesign,

                // 3D GLB 변환요청 변환율,진행율
                progressoriginal = aiImageEntity?.progressoriginal ?? 0,
                progressmoderate = aiImageEntity?.progressmoderate ?? 0,
                progressredesign = aiImageEntity?.progressredesign ?? 0,

                // 3D GLB 변환요청 JOBID
                GlbJobIdOriginal = aiImageEntity?.GlbJobIdOriginal,
                GlbJobIdModerate = aiImageEntity?.GlbJobIdModerate,
                GlbJobIdRedesign = aiImageEntity?.GlbJobIdRedesign,

                // 3D GLB 변환요청 glb이미지
                resultDataGlbOriginal = aiImageEntity?.ResultDataGlbOriginal,
                resultDataGlbModerate = aiImageEntity?.ResultDataGlbModerate,
                resultDataGlbRedesign = aiImageEntity?.ResultDataGlbRedesign
            };

            return Ok(response);
        }

        [HttpPost("Generate2DImage")]
        public async Task<IActionResult> Generate2DImage([FromBody] Generate2DRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.PrjId) || string.IsNullOrEmpty(request.PackLevel) || string.IsNullOrEmpty(request.AppliedMaterial))
            {
                return BadRequest(new { message = "요청 파라미터 정보가 누락되었습니다." });
            }

            //ecofix 데이타 생성 시작
            var evalList = await _dbContext.AiPkgEvalInfoBscs
                .Where(x => x.Prjid == request.PrjId
                            && x.PackLevel == request.PackLevel
                            && x.AppliedMaterial == request.AppliedMaterial)
                .ToListAsync();
            evalList = evalList
                .Where(x => !string.IsNullOrEmpty(x.DsgnRecmImp))
                .OrderByDescending(x => x.EcoPackLarType)
                .ToList();

            // EcoPackLarType == DsgnRecmImp,DsgnRecmImp,DsgnRecmImp, 이런식으로 묶어서 ecoFix 문자열 생성
            string ecoFix = string.Join("; ", evalList
                .GroupBy(x => x.EcoPackLarType)
                .Select(g => $"{g.Key}=={string.Join(" / ", g.Select(x => x.DsgnRecmImp?.Replace("\r", "").Replace("\n", " ") ?? string.Empty))}"));
            //ecofix 데이타 생성 끝

            //// CreateAiJobAsync 호출시 ProjectDetail에서 이미지이름과 If002a에서 디자인템플릿 정보 가져오기 
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
            // CreateAiJobRequestDto 구성
            var requestDto = new ecopack.Api.Dtos.CreateAiJobRequestDto
            {
                RequestId = $"{request.PrjId}_{request.PackLevel}",
                Prompt = !string.IsNullOrEmpty(ecoFix) ? string.Empty : $"{request.AppliedMaterial} packaging design update",
                Material = request.AppliedMaterial,
                EcoFix = ecoFix,
                Image = templateData
            };
            
            // 한글 깨짐 방지용 JSON 직렬화 옵션 추가
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            //_logger.LogInformation("[AI 요청 데이터 전송] {Payload}", System.Text.Json.JsonSerializer.Serialize(requestDto, jsonOptions));
            // AI이미지 생성 요청
            AiJobResponseWrapper apiResponse = await _aiExternalService.CreateAiJobAsync(requestDto);

            if (apiResponse == null || !apiResponse.Success)
            {
                _logger.LogWarning("[AI API 오류 발생] Response: {Response}", System.Text.Json.JsonSerializer.Serialize(apiResponse, jsonOptions));
                return StatusCode(500, new { message = "외부 AI 이미지 생성 API 호출에 실패했습니다.", response = apiResponse });
            }

            // 1. 기존에 해당 프로젝트/포장차수 이력이 있는지 조회
            var aiImageEntity = await _dbContext.ProjectAiImages
                .FirstOrDefaultAsync(x => x.PrjId == request.PrjId && x.PackLevel == request.PackLevel);

            if (aiImageEntity != null)
            {
                // 2-A. 기존 데이터가 존재하지만 AI이미지요청을 다신 선택하였으므로  신규JobID 정보로 갱신하고 이전  진행데이타 및 상태 초기화
                aiImageEntity.JobId = apiResponse.Job?.JobId ?? string.Empty;
                aiImageEntity.Status = apiResponse.Job?.Status ?? string.Empty;
                aiImageEntity.CreatedAt = DateTime.Now;
                aiImageEntity.req2ddate = DateTime.Now;
                aiImageEntity.req3ddate = null;

                aiImageEntity.Progress = 0;
                aiImageEntity.Success = false;
                aiImageEntity.StatusMessage = null;
                aiImageEntity.ErrorMessage = null;

                aiImageEntity.GlbJobId = null;
                aiImageEntity.Status3d = "NONE";
                aiImageEntity.Progress3d = 0;
                aiImageEntity.Success3d = false;
                aiImageEntity.StatusMessage3d = null;
                aiImageEntity.ErrorMessage3d = null;

                // AI추천 2D 결과 이미지 3장
                aiImageEntity.ResultDataOriginal = null;
                aiImageEntity.ResultDataModerate = null;
                aiImageEntity.ResultDataRedesign = null;
                // 3D GLB 변환요청 시간
                aiImageEntity.req3ddateoriginal = null;
                aiImageEntity.req3ddatemoderate = null;
                aiImageEntity.req3ddateredesign = null;
                // 3D GLB 변환요청 변환율,진행율
                aiImageEntity.progressoriginal = 0;
                aiImageEntity.progressmoderate = 0;
                aiImageEntity.progressredesign = 0;
                // 3D GLB 변환요청 JOBID
                aiImageEntity.GlbJobIdOriginal = null;
                aiImageEntity.GlbJobIdModerate = null;
                aiImageEntity.GlbJobIdRedesign = null;
                // 3D GLB 변환요청 glb이미지
                aiImageEntity.ResultDataGlbOriginal = null;
                aiImageEntity.ResultDataGlbModerate = null;
                aiImageEntity.ResultDataGlbRedesign = null;
            }
            else
            {
                // 2-B. 이력이 없으면 새로 생성하여 추가
                aiImageEntity = new ProjectAiImage
                {
                    PrjId = request.PrjId,
                    PackLevel = request.PackLevel,
                    JobId = apiResponse.Job?.JobId ?? string.Empty,
                    Status = apiResponse.Job?.Status ?? string.Empty,
                    req2ddate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                _dbContext.ProjectAiImages.Add(aiImageEntity);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                jobId = apiResponse.Job?.JobId,
                status = apiResponse.Job?.Status,
                message = "AI 이미지 생성 작업 요청 및 갱신이 완료되었습니다."
            });
        }


        [HttpGet("GetAiJobStatus/{prjId}/{packLevel}")]
        public async Task<IActionResult> GetAiJobStatus(string prjId, string packLevel)
        {
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { message = "프로젝트 번호 또는 패키지 레벨이 누락되었습니다." });
            }

            try
            {
                // 1. DB에서 해당 프로젝트와 차수의 JobId 조회
                var aiImageEntity = await _dbContext.ProjectAiImages
                    .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel);

                if (aiImageEntity == null || string.IsNullOrEmpty(aiImageEntity.JobId))
                {
                    return NotFound(new { message = "저장된 AI 작업 ID(JobId)를 찾을 수 없습니다." });
                }

                // 2. 외부 AI 서버에 상태 조회 요청
                AiJobStatusDto statusResponse = await _aiExternalService.GetAiJobStatusAsync(aiImageEntity.JobId);

                if (statusResponse == null)
                {
                    return StatusCode(500, new { message = "AI 작업 상태 조회 응답이 비어 있습니다." });
                }

                // 3. 상태 업데이트
                aiImageEntity.Status = statusResponse.Status ?? aiImageEntity.Status;
                aiImageEntity.Progress = statusResponse.Progress;

                // 4. 진행률이 100(완료)일 경우 3개의 결과 파일 다운로드 처리
                if (statusResponse.Progress == 100)
                {
                    try
                    {
                        // 0: original, 1: moderate, 2: redesign 파일을 병렬로 다운로드
                        var task0 = _aiExternalService.DownloadAiJobResultFileAsync(aiImageEntity.JobId, 0);
                        var task1 = _aiExternalService.DownloadAiJobResultFileAsync(aiImageEntity.JobId, 1);
                        var task2 = _aiExternalService.DownloadAiJobResultFileAsync(aiImageEntity.JobId, 2);

                        await Task.WhenAll(task0, task1, task2);

                        // 필요한 경우 바이트 배열을 Base64 문자열로 변환하여 엔티티에 저장하거나 응답에 포함
                        aiImageEntity.ResultDataOriginal = $"data:image/png;base64,{Convert.ToBase64String(await task0)}";
                        aiImageEntity.ResultDataModerate = $"data:image/png;base64,{Convert.ToBase64String(await task1)}";
                        aiImageEntity.ResultDataRedesign = $"data:image/png;base64,{Convert.ToBase64String(await task2)}";

                        aiImageEntity.Success = true;
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "[AI 결과 파일 다운로드 오류] {Message}", fileEx.Message);
                        // 파일 다운로드 실패 시 처리가 필요하다면 이 곳에 작성
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = statusResponse.Success,
                    jobId = statusResponse.JobId,
                    status = statusResponse.Status,
                    progress = statusResponse.Progress,
                    req2ddate = aiImageEntity.req2ddate,
                    message = statusResponse.Message,


                    // 100 완료 상태일 때 프론트에서 이미지 바로 렌더링용으로 쓸 수 있게 같이 반환
                    resultDataOriginal = aiImageEntity.ResultDataOriginal,
                    resultDataModerate = aiImageEntity.ResultDataModerate,
                    resultDataRedesign = aiImageEntity.ResultDataRedesign
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AI 상태 조회 오류] {Message}", ex.Message);
                return StatusCode(500, new { message = "AI 작업 상태 조회 중 오류가 발생했습니다.", error = ex.Message });
            }
        }


        /// <summary>
        /// 3D 이미지(GLB) 변환 요청 API
        /// </summary>
        [HttpPost("CreateGlbJob")]
        public async Task<IActionResult> CreateGlbJob([FromBody] frontGlbRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.PrjId) || string.IsNullOrEmpty(request.PackLevel) || string.IsNullOrEmpty(request.imagelabal) || string.IsNullOrEmpty(request.Image))
            {
                return BadRequest(new { success = false, message = "요청 파라미터(PrjId, PackLevel, Image) 정보가 누락되었습니다." });
            }

            try
            {
                // 1. 외부 AI 서비스에 보낼 DTO 구성 (명세서 규격 맞춤)
                var requestDto = new ecopack.Api.Dtos.CreateGlbJobRequestDto
                {
                    RequestId = $"{request.PrjId}_{request.PackLevel}",
                    Image = request.Image // Base64 문자열
                };

                // JSON 직렬화 옵션 (한글 깨짐 방지 등)
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 2. 외부 AI 3D 변환 API 호출
                GlbJobResponseWrapper apiResponse = await _aiExternalService.CreateGlbJobAsync(requestDto);

                if (apiResponse == null || !apiResponse.Success)
                {
                    _logger.LogWarning("[3D AI API 오류 발생] Response: {Response}", System.Text.Json.JsonSerializer.Serialize(apiResponse, jsonOptions));
                    return StatusCode(500, new { success = false, message = "외부 3D AI 변환 API 호출에 실패했습니다.", response = apiResponse });
                }

                // 3. DB 엔티티 조회 (ProjectAiImages)
                var aiImageEntity = await _dbContext.ProjectAiImages
                    .FirstOrDefaultAsync(x => x.PrjId == request.PrjId && x.PackLevel == request.PackLevel);

                if (aiImageEntity == null)
                {
                    return NotFound(new { success = false, message = "해당 프로젝트의 2D AI 작업 이력을 찾을 수 없습니다. 먼저 2D 생성을 진행해주세요." });
                }

                // 4. 3D Job 관련 정보 갱신 및 이전 3D 결과 초기화
                //aiImageEntity.GlbJobId = apiResponse.Job?.JobId ?? string.Empty;
                //aiImageEntity.Status3d = apiResponse.Job?.Status ?? "QUEUED";
                //aiImageEntity.Progress3d = 0;
                //aiImageEntity.Success3d = false;
                //aiImageEntity.StatusMessage3d = null;
                //aiImageEntity.ErrorMessage3d = null;
                //aiImageEntity.req3ddate = DateTime.Now;
                if (request.imagelabal == "H PRI")
                {
                    aiImageEntity.GlbJobIdOriginal = apiResponse.Job?.JobId ?? string.Empty;
                    aiImageEntity.req3ddateoriginal = DateTime.Now;
                    aiImageEntity.progressoriginal = 0;
                    aiImageEntity.ResultDataGlbOriginal = null;  // 3D 결과 데이터 초기화
                }
                if(request.imagelabal == "MONO")
                {
                    aiImageEntity.GlbJobIdModerate = apiResponse.Job?.JobId ?? string.Empty;
                    aiImageEntity.req3ddatemoderate = DateTime.Now;
                    aiImageEntity.progressmoderate = 0;
                    aiImageEntity.ResultDataGlbModerate = null;  // 3D 결과 데이터 초기화
                }
                if(request.imagelabal == "LIGHT")
                {
                    aiImageEntity.GlbJobIdRedesign = apiResponse.Job?.JobId ?? string.Empty;
                    aiImageEntity.req3ddateredesign = DateTime.Now;
                    aiImageEntity.progressredesign = 0;
                    aiImageEntity.ResultDataGlbRedesign = null;  // 3D 결과 데이터 초기화
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    jobId = apiResponse.Job?.JobId,
                    status = apiResponse.Job?.Status,
                    message = "3D 이미지 변환 작업 요청이 완료되었습니다."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[3D 변환 요청 오류] {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "3D 변환 요청 중 오류가 발생했습니다.", error = ex.Message });
            }
        }
        /// <summary>
        /// 3D 이미지(GLB) 작업 상태 조회 API
        /// </summary>
        /// <summary>
        /// 3D 이미지(GLB) 작업 상태 조회 API
        /// </summary>
        [HttpGet("GetGlbJobStatus/{prjId}/{packLevel}/{itemlabel}")]
        public async Task<IActionResult> GetGlbJobStatus(string prjId, string packLevel, string itemlabel)
        {
            if (string.IsNullOrEmpty(prjId) || string.IsNullOrEmpty(packLevel))
            {
                return BadRequest(new { success = false, message = "프로젝트 번호 또는 패키지 레벨이 누락되었습니다." });
            }

            try
            {
                // 1. DB에서 해당 프로젝트와 차수의 GlbJobId 조회 조건 분기
                ProjectAiImage? aiImageEntity = null;

                if (itemlabel == "H PRI")
                {
                    aiImageEntity = await _dbContext.ProjectAiImages
                        .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel && !string.IsNullOrEmpty(x.GlbJobIdOriginal));
                }
                else if (itemlabel == "MONO")
                {
                    aiImageEntity = await _dbContext.ProjectAiImages
                        .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel && !string.IsNullOrEmpty(x.GlbJobIdModerate));
                }
                else if (itemlabel == "LIGHT")
                {
                    aiImageEntity = await _dbContext.ProjectAiImages
                        .FirstOrDefaultAsync(x => x.PrjId == prjId && x.PackLevel == packLevel && !string.IsNullOrEmpty(x.GlbJobIdRedesign));
                }

                if (aiImageEntity == null)
                {
                    return NotFound(new { success = false, message = "저장된 3D 작업 ID(GlbJobId)를 찾을 수 없습니다." });
                }

                // 2. 외부 AI 서버에 상태 조회 요청 (statusResponse를 메서드 상단 스코프에 선언)
                GlbJobStatusDto? statusResponse = null;

                if (itemlabel == "H PRI")
                {
                    statusResponse = await _aiExternalService.GetGlbJobStatusAsync(aiImageEntity.GlbJobIdOriginal);
                }
                else if (itemlabel == "MONO")
                {
                    statusResponse = await _aiExternalService.GetGlbJobStatusAsync(aiImageEntity.GlbJobIdModerate);
                }
                else if (itemlabel == "LIGHT")
                {
                    statusResponse = await _aiExternalService.GetGlbJobStatusAsync(aiImageEntity.GlbJobIdRedesign);
                }

                if (statusResponse == null)
                {
                    return StatusCode(500, new { success = false, message = "3D 작업 상태 조회 응답이 비어 있습니다." });
                }

                // 3. DB 엔티티 상태 갱신
                if (itemlabel == "H PRI")
                {
                    aiImageEntity.GlbJobIdOriginal = statusResponse.JobId ?? aiImageEntity.GlbJobIdOriginal;
                    aiImageEntity.progressoriginal = statusResponse.Progress;
                }
                else if (itemlabel == "MONO")
                {
                    aiImageEntity.GlbJobIdModerate = statusResponse.JobId ?? aiImageEntity.GlbJobIdModerate;
                    aiImageEntity.progressmoderate = statusResponse.Progress;
                }
                else if (itemlabel == "LIGHT")
                {
                    aiImageEntity.GlbJobIdRedesign = statusResponse.JobId ?? aiImageEntity.GlbJobIdRedesign;
                    aiImageEntity.progressredesign = statusResponse.Progress;
                }

                // 4. 작업이 완료(COMPLETE 또는 Progress 100)되었을 때 결과 파일 다운로드 처리
                if (statusResponse.Status == "COMPLETE" || statusResponse.Progress == 100)
                {
                    try
                    {
                        if (itemlabel == "H PRI")
                        {
                            byte[] fileBytes = await _aiExternalService.DownloadGlbJobResultFileAsync(aiImageEntity.GlbJobIdOriginal);
                            aiImageEntity.ResultDataGlbOriginal = $"data:model/gltf-binary;base64,{Convert.ToBase64String(fileBytes)}";
                        }
                        else if (itemlabel == "MONO")
                        {
                            byte[] fileBytes = await _aiExternalService.DownloadGlbJobResultFileAsync(aiImageEntity.GlbJobIdModerate);
                            aiImageEntity.ResultDataGlbModerate = $"data:model/gltf-binary;base64,{Convert.ToBase64String(fileBytes)}";
                        }
                        else if (itemlabel == "LIGHT")
                        {
                            byte[] fileBytes = await _aiExternalService.DownloadGlbJobResultFileAsync(aiImageEntity.GlbJobIdRedesign);
                            aiImageEntity.ResultDataGlbRedesign = $"data:model/gltf-binary;base64,{Convert.ToBase64String(fileBytes)}";
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "[3D 결과 파일 다운로드 오류] {Message}", fileEx.Message);
                        aiImageEntity.ErrorMessage3d = fileEx.Message;
                    }
                }

                await _dbContext.SaveChangesAsync();

                // 5. 프론트엔드로 응답 반환
                return Ok(new
                {
                    success = statusResponse.Success,
                    jobId = statusResponse.JobId,
                    status = statusResponse.Status,
                    progress = statusResponse.Progress,
                    message = statusResponse.Message,

                    // 3D GLB 변환요청 시간
                    req3ddateoriginal = aiImageEntity?.req3ddateoriginal,
                    req3ddatemoderate = aiImageEntity?.req3ddatemoderate,
                    req3ddateredesign = aiImageEntity?.req3ddateredesign,

                    // 3D GLB 변환요청 변환율,진행율
                    progressoriginal = aiImageEntity?.progressoriginal ?? 0,
                    progressmoderate = aiImageEntity?.progressmoderate ?? 0,
                    progressredesign = aiImageEntity?.progressredesign ?? 0,

                    // 3D GLB 변환요청 JOBID
                    GlbJobIdOriginal = aiImageEntity?.GlbJobIdOriginal,
                    GlbJobIdModerate = aiImageEntity?.GlbJobIdModerate,
                    GlbJobIdRedesign = aiImageEntity?.GlbJobIdRedesign,

                    // 3D GLB 변환요청 glb이미지
                    resultDataGlbOriginal = aiImageEntity?.ResultDataGlbOriginal,
                    resultDataGlbModerate = aiImageEntity?.ResultDataGlbModerate,
                    resultDataGlbRedesign = aiImageEntity?.ResultDataGlbRedesign

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[3D 상태 조회 오류] {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "3D 작업 상태 조회 중 오류가 발생했습니다.", error = ex.Message });
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