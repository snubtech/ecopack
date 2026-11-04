using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net; // 1. URL 디코딩을 위해 반드시 추가해야 합니다.
using System.Threading.Tasks;

public class SqlInjectionCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SqlInjectionCheckMiddleware> _logger;

    public SqlInjectionCheckMiddleware(RequestDelegate next, ILogger<SqlInjectionCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. 요청 정보 수집 (URL 경로 및 쿼리스트링)
        var path = context.Request.Path.Value ?? string.Empty;
        var rawQueryString = context.Request.QueryString.Value ?? string.Empty;

        // ★ 핵심 수정: ZAP 등에서 넘어오는 인코딩된 문자열(%27, %20 등)을 원래 문자로 디코딩
        var decodedQueryString = WebUtility.UrlDecode(rawQueryString);
        var fullInput = path + decodedQueryString;

        // 2. SQL Injection 위험 패턴 검사
        if (IsSqlInjectionDetected(fullInput))
        {
            // 3. 평가위원이 요구하는 로그 항목 준비 (시각, URL, 차단 여부)
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // 서버 콘솔 또는 로그 파일에 기록
            _logger.LogWarning($"[SECURITY ALERT] 시각: {timestamp} | IP: {clientIp} | URL: {path} | 차단여부: BLOCKED (SQL Injection)");

            // 4. 즉시 차단 응답 반환 (400 Bad Request)
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync("{\"error\": \"Blocked by Security Middleware\"}");
            return;
        }

        // 정상 요청인 경우 다음 단계로 진행
        await _next(context);
    }

    private bool IsSqlInjectionDetected(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;

        // 탐지할 주요 공격 패턴 키워드
        string[] maliciousPatterns = { "' OR '1'='1", "UNION SELECT", "DROP TABLE", "--" };

        foreach (var pattern in maliciousPatterns)
        {
            if (input.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}