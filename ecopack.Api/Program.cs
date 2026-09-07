using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ecopack.Api.Data;
using ecopack.Api.Support;

var builder = WebApplication.CreateBuilder(args);

// 1. MySQL 데이터베이스 연결 설정 추가
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 1-1. 로그인 인증(JWT) 설정 추가
// - 로그인(POST /auth/login) 성공 시 서버가 서명한 토큰을 내려주고,
//   이후 요청은 그 토큰을 Authorization: Bearer 헤더로 실어 보낸다.
// - 서명 키(Jwt:Key)는 커밋되지 않는 dotnet user-secrets에 둔다.
//   최초 설정: dotnet user-secrets set "Jwt:Key" "<임의의 긴 문자열>"
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key 설정이 없습니다. 'dotnet user-secrets set \"Jwt:Key\" \"<임의의 긴 문자열>\"' 로 등록해 주세요.");
}

builder.Services.AddSingleton<JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "ecopack.Api",
            ValidAudience = jwtSection["Audience"] ?? "ecopack.Web",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30), // 만료 판정 여유시간을 짧게 둔다 (기본 5분은 너무 김)
        };
    });

builder.Services.AddAuthorization();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // 개발 환경에서 Swagger UI 등을 쓰고 싶다면 아래와 같이 활용할 수 있습니다.
    // (필요 시 기존 Swagger 패키지로 교체 가능)
}

app.UseHttpsRedirection();

// 1-2. 인증(토큰 검증) → 인가([Authorize] 여부 판단) 순서로 등록.
//      MapControllers보다 반드시 앞에 있어야 컨트롤러 진입 전에 걸러진다.
app.UseAuthentication();
app.UseAuthorization();

// 첨부문서(wwwroot/uploads/**) 정적 서빙 — 기술문서 화면의 첨부파일 다운로드용
app.UseStaticFiles();

// 3. 컨트롤러 라우팅 매핑 추가 (만들어둔 ProductsController가 동작하도록 연결)
app.MapControllers();

// 기존 템플릿에 있던 날씨 예제 API도 그대로 유지해 둡니다 (테스트용으로 삭제 안 함)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}