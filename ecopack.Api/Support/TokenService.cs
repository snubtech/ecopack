/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - TokenService (로그인 토큰)
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 로그인에 성공한 회원에게 서명된 토큰(JWT)을 발급합니다. (Issue)
 *    - 화면은 이후 모든 요청에 이 토큰을 실어 보내고(Authorization: Bearer ...),
 *      서버는 토큰의 서명과 만료 시간을 확인해 "누가 보낸 요청인지"를 스스로 알아냅니다.
 *    - 예전에는 화면이 보낸 고객 ID(repCustId)를 그대로 믿었습니다.
 *      이제 고객 ID 는 토큰에서만 꺼냅니다. (User.CustId())
 *
 * 2. 서명 키 (환경변수 ECOPACK_JWT_KEY)
 *    - 서명과 검증에 같은 키 하나를 쓰는 대칭 키 방식(HS256)입니다.
 *      토큰을 발급하는 서버와 검증하는 서버가 같아서 이 방식으로 충분합니다.
 *    - 이 키를 아는 사람은 누구의 토큰이든 만들 수 있으므로 소스에 적지 않습니다.
 *      키는 환경변수로만 받습니다. appsettings.json 에 적어도 읽지 않습니다(실수로 저장소에 올라가는 것을 막기 위해).
 *        Windows PowerShell : $env:ECOPACK_JWT_KEY = '32자 이상의 임의 문자열'
 *        macOS / Linux      : export ECOPACK_JWT_KEY='32자 이상의 임의 문자열'
 *    - 키를 설정하지 않아도 서버는 켜집니다(개발·운영 모두).
 *      그때는 서버가 켜질 때마다 임시 키를 새로 만들어 메모리에만 두고 씁니다.
 *      키를 따로 보관·관리하지 않아도 되는 대신 아래 두 가지를 감수해야 합니다.
 *        · 서버를 다시 켜면 그 전에 받은 토큰은 못 쓰게 되어 모두 다시 로그인해야 합니다.
 *        · 서버를 여러 대로 늘리면 서버마다 키가 달라 로그인이 유지되지 않습니다(그때는 키를 설정해야 합니다).
 *
 * 3. 만료 시간 (Jwt:ExpireMinutes, 기본 30분)
 *    - 토큰은 발급 후 이 시간이 지나면 서버가 받지 않습니다.
 *    - 화면은 사용자가 쓰고 있는 동안 주기적으로 새 토큰을 받아(/api/auth/refresh) 이어 갑니다.
 *      아무것도 하지 않고 만료 시간이 지나면 다시 로그인해야 합니다.
 * ==============================================================================
 */
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ecopack.Api.Support
{
    /// <summary>토큰 서명·검증에 쓰는 설정. 서버가 켜질 때 한 번 만들어 계속 쓴다.</summary>
    public class JwtSettings
    {
        public const string Issuer = "ecopack.Api";
        public const string Audience = "ecopack.Web";

        /// <summary>서명 키를 넣는 환경변수 이름</summary>
        public const string KeyEnvironmentVariable = "ECOPACK_JWT_KEY";

        /// <summary>고객 ID 를 담는 항목 이름</summary>
        public const string CustIdClaim = "sub";

        public required SymmetricSecurityKey Key { get; init; }
        public required TimeSpan Lifetime { get; init; }

        /// <summary>임시 키를 쓰고 있는지(키를 설정하지 않은 경우)</summary>
        public bool UsingTemporaryKey { get; init; }

        public static JwtSettings Load(IConfiguration config)
        {
            // 키는 환경변수로만 받는다. 설정 파일(appsettings.json)의 값은 일부러 읽지 않는다.
            var raw = Environment.GetEnvironmentVariable(KeyEnvironmentVariable);

            byte[] keyBytes;
            var temporary = false;
            if (string.IsNullOrWhiteSpace(raw))
            {
                // 키를 설정하지 않았으면 이번 실행 동안만 쓸 임시 키를 만든다(메모리에만 있고 어디에도 저장하지 않는다)
                keyBytes = RandomNumberGenerator.GetBytes(32);
                temporary = true;
            }
            else
            {
                keyBytes = Encoding.UTF8.GetBytes(raw);
                if (keyBytes.Length < 32)
                {
                    throw new InvalidOperationException("로그인 토큰 서명 키(ECOPACK_JWT_KEY)는 32자 이상이어야 합니다.");
                }
            }

            var minutes = Math.Clamp(config.GetValue("Jwt:ExpireMinutes", 30), 5, 24 * 60);
            return new JwtSettings
            {
                Key = new SymmetricSecurityKey(keyBytes),
                Lifetime = TimeSpan.FromMinutes(minutes),
                UsingTemporaryKey = temporary
            };
        }
    }

    public class TokenService
    {
        private readonly JwtSettings _settings;
        private readonly JsonWebTokenHandler _handler = new();

        public TokenService(JwtSettings settings)
        {
            _settings = settings;
        }

        /// <summary>토큰이 유효한 시간(초). 화면이 갱신 시점을 정하는 데 쓴다.</summary>
        public int ExpiresInSeconds => (int)_settings.Lifetime.TotalSeconds;

        /// <summary>이 고객 ID 의 토큰을 발급한다.</summary>
        public string Issue(string repCustId)
        {
            var now = DateTime.UtcNow;
            return _handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = JwtSettings.Issuer,
                Audience = JwtSettings.Audience,
                Subject = new ClaimsIdentity(new[] { new Claim(JwtSettings.CustIdClaim, repCustId) }),
                IssuedAt = now,
                NotBefore = now,
                Expires = now + _settings.Lifetime,
                SigningCredentials = new SigningCredentials(_settings.Key, SecurityAlgorithms.HmacSha256)
            });
        }
    }

    public static class CurrentUser
    {
        /// <summary>
        /// 지금 요청을 보낸 회원의 고객 ID. 토큰에서 꺼낸 값이다(요청이 보낸 값이 아니다).
        /// 로그인하지 않았으면 null.
        /// </summary>
        public static string? CustId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(JwtSettings.CustIdClaim)?.Value;
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }
    }

    /// <summary>권한 정책 이름</summary>
    public static class AuthPolicies
    {
        /// <summary>관리용 API. Admin:RepCustIds 설정에 적힌 고객 ID 만 쓸 수 있다.</summary>
        public const string Admin = "admin";
    }
}
