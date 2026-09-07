/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - JwtTokenService (로그인 토큰 발급)
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 로그인에 성공한 회원의 고객 ID(repCustId)를 담아 서명된 JWT를 만든다.
 *    - 서명 키(Jwt:Key)는 appsettings에 커밋하지 않고 dotnet user-secrets로
 *      로컬 PC에만 둔다 (appsettings에는 Issuer/Audience/만료시간만 있음).
 *
 * 2. 만료 시간
 *    - 기본 30분. 화면의 30분 무활동 자동 로그아웃 타이머(App.jsx)와 맞춘 값이다.
 *    - 만료되면 프론트는 401을 받고 다시 로그인해야 한다 (내부 업무툴이라
 *      refresh token 없이 단일 access token만 사용).
 *
 * 3. 검증은 Program.cs 의 AddJwtBearer 설정이 담당한다.
 *    여기서는 발급만 하고, 서명·만료 검증은 미들웨어가 매 요청마다 자동으로 한다.
 * ==============================================================================
 */
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ecopack.Api.Support
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>로그인한 회원의 고객 ID를 담은 JWT 문자열을 만든다.</summary>
        public string CreateToken(string repCustId)
        {
            var section = _config.GetSection("Jwt");

            var key = section["Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                // 개발 중 흔한 실수 방지: 서명 키를 안 넣고 그냥 실행하면 바로 알아챌 수 있게 한다.
                // dotnet user-secrets set "Jwt:Key" "<임의의 긴 문자열>"
                throw new InvalidOperationException(
                    "Jwt:Key 설정이 없습니다. 'dotnet user-secrets set \"Jwt:Key\" \"<임의의 긴 문자열>\"' 로 등록해 주세요.");
            }

            var issuer = section["Issuer"] ?? "ecopack.Api";
            var audience = section["Audience"] ?? "ecopack.Web";
            var expiryMinutes = section.GetValue<int?>("ExpiryMinutes") ?? 30;

            var claims = new[]
            {
                new Claim(ClaimsPrincipalExtensions.RepCustIdClaimType, repCustId),
                new Claim(JwtRegisteredClaimNames.Sub, repCustId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
