using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. MySQL 데이터베이스 연결 설정 추가
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. 컨트롤러 및 API 문서(OpenAPI) 설정 추가
builder.Services.AddControllers();
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