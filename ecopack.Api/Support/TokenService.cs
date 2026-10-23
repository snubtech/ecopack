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
 *
 * 4. 최대 로그인 유지 시간 (Jwt:MaxSessionHours, 기본 12시간)
 *    - 토큰에는 처음 로그인한 시각(auth_time)이 들어 있고, 갱신해도 이 값은 바뀌지 않습니다.
 *    - 로그인한 지 이 시간이 지나면 갱신을 거절하고, 새 토큰의 만료도 이 시각을 넘지 않게 자릅니다.
 *      토큰을 도둑맞아도 갱신으로 끝없이 늘려 쓸 수 없게 하기 위해서입니다.
 *
 * 5. 토큰 버전 (ver, customer.tknver)
 *    - 토큰에는 발급할 때의 회원 토큰 버전이 들어 있습니다.
 *    - 로그아웃·비밀번호 변경 때 회원의 버전을 1 올리고, 서버는 요청마다 토큰의 버전과 DB 의 버전을 비교합니다(Program.cs).
 *      그래서 그 전에 발급된 토큰은 만료 전이라도 즉시 못 쓰게 됩니다.
 *    - 로그아웃은 그 회원의 모든 브라우저·PC 에서 함께 로그아웃됩니다(토큰 버전이 회원 단위라서).
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

        /// <summary>처음 로그인한 시각(유닉스 초)을 담는 항목 이름. 갱신해도 바뀌지 않는다.</summary>
        public const string AuthTimeClaim = "auth_time";

        /// <summary>회원 토큰 버전(customer.tknver)을 담는 항목 이름</summary>
        public const string VersionClaim = "ver";

        public required SymmetricSecurityKey Key { get; init; }
        public required TimeSpan Lifetime { get; init; }

        /// <summary>처음 로그인한 뒤 갱신으로 이어 쓸 수 있는 최대 시간</summary>
        public required TimeSpan MaxSession { get; init; }

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
            var maxHours = Math.Clamp(config.GetValue("Jwt:MaxSessionHours", 12), 1, 72);
            return new JwtSettings
            {
                Key = new SymmetricSecurityKey(keyBytes),
                Lifetime = TimeSpan.FromMinutes(minutes),
                MaxSession = TimeSpan.FromHours(maxHours),
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

        /// <summary>
        /// 새 토큰을 발급한다.
        /// </summary>
        /// <param name="repCustId">고객 ID</param>
        /// <param name="version">회원의 현재 토큰 버전(customer.tknver)</param>
        /// <param name="authTime">처음 로그인한 시각. 로그인할 때는 null(지금), 갱신할 때는 기존 토큰의 값을 넘긴다.</param>
        public IssuedToken Issue(string repCustId, int version, DateTimeOffset? authTime = null)
        {
            var now = DateTimeOffset.UtcNow;
            var loginAt = authTime ?? now;

            // 최대 로그인 유지 시간을 넘지 않게 만료를 자른다
            var expires = now + _settings.Lifetime;
            var sessionEnd = loginAt + _settings.MaxSession;
            if (expires > sessionEnd) expires = sessionEnd;

            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = JwtSettings.Issuer,
                Audience = JwtSettings.Audience,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtSettings.CustIdClaim, repCustId),
                    new Claim(JwtSettings.AuthTimeClaim, loginAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtSettings.VersionClaim, version.ToString(), ClaimValueTypes.Integer32)
                }),
                IssuedAt = now.UtcDateTime,
                NotBefore = now.UtcDateTime,
                Expires = expires.UtcDateTime,
                SigningCredentials = new SigningCredentials(_settings.Key, SecurityAlgorithms.HmacSha256)
            });

            return new IssuedToken(token, Math.Max(0, (int)(expires - now).TotalSeconds));
        }

        /// <summary>처음 로그인한 뒤 최대 로그인 유지 시간이 지났는지</summary>
        public bool SessionExpired(DateTimeOffset authTime)
            => DateTimeOffset.UtcNow >= authTime + _settings.MaxSession;
    }

    /// <summary>발급한 토큰과 유효 시간(초). 화면은 유효 시간으로 갱신 시점을 정한다.</summary>
    public record IssuedToken(string AccessToken, int ExpiresIn);

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

        /// <summary>토큰에 담긴 처음 로그인 시각. 없거나 읽을 수 없으면 null.</summary>
        public static DateTimeOffset? AuthTime(this ClaimsPrincipal user)
            => long.TryParse(user.FindFirst(JwtSettings.AuthTimeClaim)?.Value, out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds)
                : null;

        /// <summary>토큰에 담긴 회원 토큰 버전. 없거나 읽을 수 없으면 null.</summary>
        public static int? TokenVersion(this ClaimsPrincipal user)
            => int.TryParse(user.FindFirst(JwtSettings.VersionClaim)?.Value, out var version) ? version : null;
    }

    /// <summary>권한 정책 이름</summary>
    public static class AuthPolicies
    {
        /// <summary>관리용 API. Admin:RepCustIds 설정에 적힌 고객 ID 만 쓸 수 있다.</summary>
        public const string Admin = "admin";
    }
}
