using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Services; // IExternalAiService, ExternalAiService를 위해 추가해주면 좋습니다.
using ecopack.Api.Support;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. MySQL 데이터베이스 연결 설정 추가
//    접속 문자열은 환경변수 ECOPACK_DB_CONNECTION 을 먼저 읽고, 없으면 appsettings.json 의 값을 쓴다.
//    지금은 모두 설정 파일 값을 쓰고 있다. 접속 정보(비밀번호 포함)를 설정 파일에서 빼려면
//    각 PC·서버에 환경변수를 넣은 뒤 appsettings.json 의 DefaultConnection 을 지우면 된다(코드 수정 불필요).
var dbFromEnvironment = Environment.GetEnvironmentVariable("ECOPACK_DB_CONNECTION");
var connectionString = !string.IsNullOrWhiteSpace(dbFromEnvironment)
    ? dbFromEnvironment
    : builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. 컨트롤러 및 API 문서(OpenAPI) 설정 추가
// 화면 폼은 모든 값을 글자로 다루므로 날짜 항목이 빈 문자열("")로 올 수 있다.
// 그대로 두면 날짜 변환에 실패해 요청이 거절되므로, 빈 문자열을 값 없음으로 받아 준다.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ecopack.Api.Support.EmptyStringDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new ecopack.Api.Support.EmptyStringDateOnlyConverter());
    });
builder.Services.AddOpenApi(); // 기존 템플릿의 OpenAPI 설정 유지

// 2-1. 입력값 검증 실패(400) 응답 모양을 다른 응답과 같게 맞춘다.
//      DTO 에 붙인 검증 속성([Required], [StringLength] 등)에 걸리면 컨트롤러에 들어가기 전에 여기서 끊긴다.
//      화면은 message 를 그대로 보여 주므로 첫 번째 오류 문구를 message 에 싣는다.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors
                    .Select(e =>
                        // 형식이 깨진 JSON 이나 형 변환 실패는 파서의 내부 문구(자료형 이름, 위치 등)가 들어 있어 일반 문구로 바꾼다
                        string.IsNullOrEmpty(e.ErrorMessage) || e.Exception != null
                            || e.ErrorMessage.Contains("LineNumber:", StringComparison.Ordinal)
                            || e.ErrorMessage.Contains("Path: $", StringComparison.Ordinal)
                            || e.ErrorMessage.Contains("JSON", StringComparison.OrdinalIgnoreCase)
                            ? "값의 형식이 올바르지 않습니다."
                        // 프레임워크가 만든 영문 기본 문구(필수값 누락)는 우리말로 바꾼다
                        : e.ErrorMessage.EndsWith("field is required.", StringComparison.Ordinal) ? "필수 항목이 누락되었습니다."
                        : e.ErrorMessage)
                    .ToArray());

        var first = errors.Values.SelectMany(v => v).FirstOrDefault() ?? "입력값이 올바르지 않습니다.";
        return new BadRequestObjectResult(new { success = false, message = first, errors });
    };
});

// 2-0. 인증(로그인 토큰)과 권한
//      - 모든 API 는 기본적으로 로그인(유효한 토큰)이 있어야 부를 수 있다. (FallbackPolicy)
//        로그인·회원가입·아이디 중복확인처럼 로그인 전에 써야 하는 API 만 [AllowAnonymous] 로 열어 둔다.
//        새 컨트롤러를 만들 때 따로 표시하지 않아도 자동으로 보호된다.
//      - 누가 보낸 요청인지는 토큰에서만 알아낸다. 화면이 보낸 고객 ID(repCustId)는 쓰지 않는다.
//      - 관리용 API 는 Admin:RepCustIds 설정에 적힌 고객 ID 만 부를 수 있다. 비어 있으면 아무도 부를 수 없다.
var jwtSettings = JwtSettings.Load(builder.Configuration);
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<TokenService>();

var adminIds = (builder.Configuration.GetSection("Admin:RepCustIds").Get<string[]>() ?? Array.Empty<string>())
    .Where(id => !string.IsNullOrWhiteSpace(id))
    .Select(id => id.Trim())
    .ToHashSet(StringComparer.Ordinal);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // 토큰의 항목 이름(sub)을 다른 이름으로 바꾸지 않고 그대로 쓴다
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSettings.Key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtSettings.CustIdClaim
        };
        options.Events = new JwtBearerEvents
        {
            // 토큰이 없거나 만료된 경우. 화면은 401 을 받으면 로그인 화면으로 돌아간다.
            OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted) return;
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    new { success = false, message = "로그인이 필요합니다. 다시 로그인해 주세요." });
            },
            // 로그인은 했지만 권한이 없는 경우(관리용 API)
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted) return;
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    new { success = false, message = "이 기능을 사용할 권한이 없습니다." });
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy(AuthPolicies.Admin, policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => adminIds.Contains(context.User.CustId() ?? "")));
});

// 2-2. 감사 로그와 로그인 시도 제한(둘 다 서버 메모리·파일만 쓰고 DB 는 건드리지 않는다)
builder.Services.AddSingleton<IAuditLog, AuditLog>();
builder.Services.AddSingleton<LoginGuard>();

// 2-3. 요청 횟수 제한
//      - auth     : 로그인·회원가입·아이디 중복확인. 비밀번호 대입과 아이디 수집을 느리게 한다.
//      - ai-chat  : AI 채팅 질문. 외부 AI 서버 호출 비용이 드는 요청이다.
//      - ai-image : AI 이미지 생성·3D 변환 요청. 가장 무거운 요청이다.
//      1분당 횟수이고 RateLimit 설정으로 바꿀 수 있다.
//      로그인 전 요청(auth)은 접속 IP 별로, 로그인 후 요청(AI)은 회원별로 센다.
//      (프록시 뒤에 두면 모든 요청이 프록시 IP 로 보이므로 IP 기준에는 ForwardedHeaders 설정이 함께 필요하다)
builder.Services.AddRateLimiter(options =>
{
    void AddPolicy(string name, string configKey, int defaultPerMinute)
    {
        var perMinute = Math.Max(1, builder.Configuration.GetValue($"RateLimit:{configKey}", defaultPerMinute));
        options.AddPolicy(name, http => RateLimitPartition.GetFixedWindowLimiter(
            // 로그인한 요청은 회원별로, 아니면 접속 IP 별로 센다
            http.User.CustId() is { } custId ? "user:" + custId : "ip:" + (http.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = perMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    }

    AddPolicy(RateLimitPolicies.Auth, "AuthPerMinute", 20);
    AddPolicy(RateLimitPolicies.AiChat, "AiChatPerMinute", 20);
    AddPolicy(RateLimitPolicies.AiImage, "AiImagePerMinute", 6);

    options.OnRejected = async (context, ct) =>
    {
        var http = context.HttpContext;
        http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        http.Response.Headers.RetryAfter = "60";
        http.RequestServices.GetRequiredService<IAuditLog>()
            .Write(http, "RATE_LIMITED", http.User.CustId(), null);
        await http.Response.WriteAsJsonAsync(
            new { success = false, message = "요청이 너무 많습니다. 잠시 후 다시 시도해 주세요." }, ct);
    };
});

// 3. 우측 AI 채팅창(LLM) 설정
//    - appsettings.json 의 "LlmChat" 구역을 읽는다.
//    - 연동 대상은 사내 생성형 AI 서버다(Swagger: {BaseUrl}/docs).
//    - 지금은 인증이 없지만, 나중에 서버가 API Key 나 Bearer 를 요구하면
//      코드 수정 없이 AuthType / AuthValue 설정만 바꾸면 된다.
builder.Services.Configure<ecopack.Api.Support.LlmChatOptions>(
    builder.Configuration.GetSection(ecopack.Api.Support.LlmChatOptions.SectionName));

// AI 서버 호출용 HttpClient.
// 매번 new HttpClient() 를 만들면 연결이 쌓여 포트가 마르므로 팩토리를 통해 받아 쓴다.
builder.Services.AddHttpClient(ecopack.Api.Services.EcoAiChatService.HttpClientName);

// 챗봇 호출은 상태가 없어 싱글턴으로 둔다.
builder.Services.AddSingleton<ecopack.Api.Services.IEcoAiChatService,
                              ecopack.Api.Services.EcoAiChatService>();

// 대화 백업은 DbContext(스코프)를 쓰므로 스코프라 둔다.
builder.Services.AddScoped<ecopack.Api.Services.ILlmChatArchiveService,
                           ecopack.Api.Services.LlmChatArchiveService>();

// 2일 지난 대화를 주기적으로 백업하는 백그라운드 작업.
// LlmChat:ArchiveIntervalHours 를 0 으로 두면 돌지 않는다(수동 API 는 그대로 사용 가능).
builder.Services.AddHostedService<ecopack.Api.Services.LlmChatArchiveHostedService>();

// 3-1. 기준정보 연계 API(IF005 환경영향평가정보) 수집 배치
//      - appsettings.json 의 "EdbApi" 구역을 읽는다. API 키는 환경변수 ECOPACK_EDB_API_KEY 를 권한다.
//      - EdbApi:If005SyncIntervalHours 를 0 으로 두면 자동 수집을 하지 않는다(수동 API 는 그대로 사용 가능).
builder.Services.Configure<ecopack.Api.Support.EdbApiOptions>(
    builder.Configuration.GetSection(ecopack.Api.Support.EdbApiOptions.SectionName));
builder.Services.AddHttpClient(ecopack.Api.Services.If005SyncService.HttpClientName)
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        // 기준정보 서버는 자체 서명 인증서라 기본 검증에서 거절된다.
        // 검증을 끄지 않고, 설정한 지문(EdbApi:CertThumbprint)과 같은 인증서만 예외로 받아 준다.
        var thumb = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ecopack.Api.Support.EdbApiOptions>>()
                      .Value.CertThumbprint?.Replace(":", "").Trim();
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, cert, _, errors) =>
                errors == System.Net.Security.SslPolicyErrors.None
                || (!string.IsNullOrEmpty(thumb) && cert != null
                    && string.Equals(cert.GetCertHashString(), thumb, StringComparison.OrdinalIgnoreCase))
        };
    });
builder.Services.AddScoped<ecopack.Api.Services.IIf005SyncService,
                           ecopack.Api.Services.If005SyncService>();
builder.Services.AddHostedService<ecopack.Api.Services.If005SyncHostedService>();

// 4. 외부 AI 이미지/3D 서버 연동 서비스 설정 추가 (appsettings.json의 이미지서버 주소 자동 주입)
builder.Services.AddHttpClient<ecopack.Api.Dtos.IExternalAiService, ecopack.Api.Dtos.ExternalAiService>(client =>
{
    // appsettings.json의  ExternalAiSettings 설정 파일의 값을 그대로 믿고 사용 (설정이 누락되었다면 여기서 바로 예외가 나므로 실수를 즉시 발견 가능)
    var baseUrl = builder.Configuration["ExternalAiSettings:BaseUrl"]
                  ?? throw new InvalidOperationException("ExternalAiSettings:BaseUrl is not configured in appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);

    // 지금 이미지 AI 서버는 인증을 요구하지 않는다. 나중에 서버가 API 키를 요구하면
    // 환경변수 ECOPACK_IMAGE_AI_API_KEY 에 키를 넣기만 하면 모든 요청에 실어 보낸다(코드 수정 불필요).
    // 헤더 이름은 ExternalAiSettings:ApiKeyHeader 로 바꿀 수 있다(기본 X-API-Key). 키는 설정 파일에 적지 않는다.
    var imageAiKey = Environment.GetEnvironmentVariable("ECOPACK_IMAGE_AI_API_KEY");
    if (!string.IsNullOrWhiteSpace(imageAiKey))
    {
        client.DefaultRequestHeaders.Add(
            builder.Configuration["ExternalAiSettings:ApiKeyHeader"] ?? "X-API-Key", imageAiKey);
    }
});

// (참고) IHttpContextAccessor는 다른 곳에서 필요할 수 있어 남겨두거나 필요 없으면 제거 가능합니다.
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (jwtSettings.UsingTemporaryKey)
{
    app.Logger.LogWarning(
        "로그인 토큰 서명 키(ECOPACK_JWT_KEY)가 설정되지 않아 이번 실행 동안만 쓰는 임시 키를 만들었다. 서버를 다시 켜면 모두 다시 로그인해야 한다.");
}
if (string.IsNullOrWhiteSpace(dbFromEnvironment))
{
    app.Logger.LogWarning(
        "DB 접속 정보를 설정 파일(appsettings.json)에서 읽었다. 운영 전에 환경변수 ECOPACK_DB_CONNECTION 으로 옮길 것.");
}
if (adminIds.Count == 0)
{
    app.Logger.LogWarning("관리자(Admin:RepCustIds)가 설정되지 않았다. 관리용 API(수동 백업·수집)는 아무도 부를 수 없다.");
}

// 전역 예외 처리기. 컨트롤러가 잡지 않은 예외가 여기로 온다.
// 예외 내용(스택, SQL, 경로 등)은 서버 로그에만 남기고 응답에는 정해진 문구만 싣는다.
// 개발 환경에서도 같은 응답을 내도록 가장 먼저 등록한다.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, message) = ex switch
    {
        InputValidationException v => (StatusCodes.Status400BadRequest, v.Message),
        BadHttpRequestException bad => (bad.StatusCode, "요청 형식이 올바르지 않습니다."),
        _ => (StatusCodes.Status500InternalServerError, ApiFailure.DefaultMessage)
    };

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new { success = false, message });
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous(); // API 문서는 개발 환경에서만, 로그인 없이 볼 수 있게 둔다

    // 개발 환경에서 Swagger UI 등을 쓰고 싶다면 아래와 같이 활용할 수 있습니다.
    // (필요 시 기존 Swagger 패키지로 교체 가능)
}
else
{
    // 운영에서는 브라우저가 이후 접속을 HTTPS 로만 하도록 알린다(HSTS).
    // 개발 PC 의 localhost 까지 HTTPS 로 묶이지 않게 개발 환경에서는 켜지 않는다.
    app.UseHsts();
}

app.UseHttpsRedirection();

// 모든 응답에 보안 헤더를 붙인다.
// 이 서버는 JSON 과 첨부파일만 내려주고 화면(HTML)은 내려주지 않으므로 가장 엄격한 값으로 둔다.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";      // 내려준 파일을 브라우저가 다른 형식으로 추측해 실행하지 못하게 한다
    headers["X-Frame-Options"] = "DENY";                // 다른 사이트가 응답을 프레임에 넣지 못하게 한다
    headers["Referrer-Policy"] = "no-referrer";
    headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    await next();
});

// ⚠️ 첨부문서(TD/DOC 근거문서)는 이제 wwwroot 밖(App_Data/uploads)에 저장한다.
// 예전엔 여기서 UseStaticFiles()로 wwwroot/uploads를 통째로 공개해서,
// 로그인 없이 URL만 알면 누구나 남의 프로젝트 첨부파일을 내려받을 수 있었다.
// 지금은 각 컨트롤러의 Download 액션이 소유자 확인을 거친 뒤에만 파일을 내려주므로
// wwwroot에 공개로 서빙할 게 없어 UseStaticFiles() 자체를 쓰지 않는다.

// 토큰을 읽어 누가 보낸 요청인지 알아낸다. 아래의 감사 로그와 요청 제한이 이 정보를 쓰므로 먼저 둔다.
app.UseAuthentication();

// 감사 로그: 권한 거부(403)와 삭제 요청을 한곳에서 기록한다.
// 컨트롤러마다 따로 적지 않아도 되도록 응답이 나간 뒤 상태 코드를 보고 남긴다.
// "누가"는 토큰에서 꺼낸 고객 ID 다.
app.Use(async (context, next) =>
{
    await next();

    var status = context.Response.StatusCode;
    var denied = status == StatusCodes.Status403Forbidden;
    var deleted = HttpMethods.IsDelete(context.Request.Method) && status is >= 200 and < 300;
    if (denied || deleted)
    {
        var query = context.Request.Query;
        context.RequestServices.GetRequiredService<IAuditLog>().Write(
            context,
            denied ? "ACCESS_DENIED" : "DELETE",
            context.User.CustId(),
            new
            {
                status,
                prjId = query["prjId"].ToString(),
                packLevel = query["packLevel"].ToString(),
                slot = query["slot"].ToString()
            });
    }
});

app.UseRateLimiter();

// 로그인 여부와 권한을 확인한다. 통과하지 못하면 컨트롤러에 들어가지 않는다.
app.UseAuthorization();

// 4. 컨트롤러 라우팅 매핑 추가
app.MapControllers();

app.Run();