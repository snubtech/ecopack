using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. MySQL 데이터베이스 연결 설정 추가
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // 개발 환경에서 Swagger UI 등을 쓰고 싶다면 아래와 같이 활용할 수 있습니다.
    // (필요 시 기존 Swagger 패키지로 교체 가능)
}

app.UseHttpsRedirection();

// ⚠️ 첨부문서(TD/DOC 근거문서)는 이제 wwwroot 밖(App_Data/uploads)에 저장한다.
// 예전엔 여기서 UseStaticFiles()로 wwwroot/uploads를 통째로 공개해서,
// 로그인 없이 URL만 알면 누구나 남의 프로젝트 첨부파일을 내려받을 수 있었다.
// 지금은 각 컨트롤러의 Download 액션이 소유자 확인을 거친 뒤에만 파일을 내려주므로
// wwwroot에 공개로 서빙할 게 없어 UseStaticFiles() 자체를 쓰지 않는다.

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