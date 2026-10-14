/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - AuthController (로그인 / 회원가입 / 회원정보)
 * ==============================================================================
 * 
 * 1. 담당 범위
 *    - 로그인과 회원가입, 회원정보 조회·수정을 맡습니다. 대상 테이블은 customer(고객기본) 입니다.
 * 
 * 2. 비밀번호 저장 방식
 *    - 예전에는 비밀번호를 평문 그대로 비교했지만, 지금은 해시로 저장합니다.
 *    - 새로 가입하는 계정은 처음부터 해시로 넣고,
 *      예전에 평문으로 저장돼 있던 계정은 로그인에 성공한 그 시점에 해시로 바꿔 둡니다.
 *      (VerifyPassword 참고. 로그인에 쓰는 비밀번호 자체는 그대로라 기존 계정도 문제없이 들어옵니다)
 *    - 주의: DB를 여러 PC가 함께 쓰므로, 예전 코드가 도는 PC에서는 해시를 이해하지 못해
 *      로그인이 막힙니다. 이 변경이 반영된 코드로 모두 맞춰야 합니다.
 * 
 * 3. Login — 로그인
 *    - 아이디로 계정을 찾고 비밀번호를 검증한 뒤 최종로그인일시를 갱신합니다.
 *    - 응답에 회원 프로필을 함께 실어, 기술문서·적합성선언서 화면이
 *      제조사·담당자 정보를 자동으로 채우는 데 쓰게 합니다.
 * 
 * 4. CheckId / Join — 아이디 중복확인과 회원가입
 *    - 가입 시 아이디 중복을 다시 검사해 같은 아이디가 두 번 만들어지지 않게 합니다.
 *    - 가입일과 정보변경일을 서버가 기록합니다.
 * 
 * 5. GetProfile / UpdateProfile — 회원정보
 *    - 본인 것만 조회·수정합니다. 누구인지는 로그인 토큰에서 알아냅니다(화면이 보낸 아이디는 쓰지 않습니다).
 *    - 수정은 비밀번호를 뺀 나머지 정보를 먼저 반영하고,
 *      비밀번호는 현재 비밀번호가 맞을 때만 바꿉니다.
 *    - 요청에 담기지 않은 항목(null)은 기존 값을 그대로 둡니다.
 *
 * 6. 로그인 토큰
 *    - 로그인에 성공하면 서명된 토큰(JWT)을 발급합니다. 화면은 이후 모든 요청에 이 토큰을 싣습니다.
 *    - 토큰은 정해진 시간(기본 30분)이 지나면 만료됩니다.
 *      화면은 사용 중에 Refresh 로 새 토큰을 받아 이어 가고, 쓰지 않고 두면 다시 로그인해야 합니다.
 *    - 로그인·회원가입·아이디 중복확인·로그아웃만 토큰 없이 부를 수 있습니다([AllowAnonymous]).
 * ==============================================================================
 */
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Support;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    /// <summary>
    /// 회원 인증 / 회원가입 / 회원정보 관리. 대상 테이블은 customer(고객기본).
    /// 라우트: api/auth
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// 비밀번호는 ASP.NET Core 내장 해셔로 저장한다.
        /// 기존에 평문으로 저장된 계정은 로그인 시 평문 비교로 받아준 뒤 해시로 승격한다.
        /// </summary>
        private static readonly PasswordHasher<Customer> Hasher = new();

        /// <summary>
        /// 새로 가입하는 아이디의 형식: 영문·숫자와 . _ - @ 만, 4~20자.
        /// 20자인 이유: 프로젝트 테이블의 작성자 아이디 칸(prjuserid)이 20자라서, 더 긴 아이디는 프로젝트를 만들 수 없다.
        /// </summary>
        private static readonly System.Text.RegularExpressions.Regex NewIdPattern =
            new(@"^[A-Za-z0-9._@-]{4,20}$", System.Text.RegularExpressions.RegexOptions.Compiled);

        private const string NewIdRuleText = "아이디는 영문·숫자와 . _ - @ 만 사용해 4~20자로 입력해 주세요.";

        private readonly LoginGuard _guard;
        private readonly IAuditLog _audit;
        private readonly TokenService _tokens;

        public AuthController(AppDbContext context, LoginGuard guard, IAuditLog audit, TokenService tokens)
        {
            _context = context;
            _guard = guard;
            _audit = audit;
            _tokens = tokens;
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/auth/login
        // ─────────────────────────────────────────────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.RepCustId))
            {
                return BadRequest(new { message = "아이디와 비밀번호를 입력해 주세요." });
            }

            // 비밀번호를 연달아 틀려 잠긴 아이디는 비밀번호를 확인하지 않고 바로 돌려보낸다
            var locked = _guard.LockedFor(loginDto.RepCustId);
            if (locked != null)
            {
                _audit.Write(HttpContext, "LOGIN_BLOCKED", loginDto.RepCustId);
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    message = $"로그인 시도가 너무 많습니다. {Math.Max(1, (int)Math.Ceiling(locked.Value.TotalMinutes))}분 후 다시 시도해 주세요."
                });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.RepCustId == loginDto.RepCustId);

            if (customer == null || !VerifyPassword(customer, loginDto.RepCustPwd))
            {
                // 없는 아이디도 똑같이 센다(잠금 여부로 아이디가 있는지 알 수 없게)
                var nowLocked = _guard.RecordFailure(loginDto.RepCustId);
                _audit.Write(HttpContext, nowLocked ? "LOGIN_LOCKED" : "LOGIN_FAIL", loginDto.RepCustId);
                return Unauthorized(new { message = "아이디 또는 비밀번호가 잘못되었습니다." });
            }

            _guard.RecordSuccess(loginDto.RepCustId);
            customer.LastLgnDtm = DateTime.Now;
            await _context.SaveChangesAsync();
            _audit.Write(HttpContext, "LOGIN_OK", customer.RepCustId);

            return Ok(new
            {
                data = new
                {
                    accessToken = _tokens.Issue(customer.RepCustId),
                    expiresIn = _tokens.ExpiresInSeconds, // 토큰이 유효한 시간(초)
                    repCustId = customer.RepCustId,
                    profile = ToProfile(customer)
                },
                message = "로그인 성공"
            });
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/auth/checkId?repCustId=xxx
        // 회원가입 화면의 아이디 중복확인
        // ─────────────────────────────────────────────────────────────
        [HttpGet("checkId")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<IActionResult> CheckId([FromQuery] string repCustId)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "아이디를 입력해 주세요." });
            }

            var exists = await _context.Customers.AnyAsync(c => c.RepCustId == repCustId);
            return Ok(new
            {
                success = true,
                available = !exists,
                message = exists ? "이미 사용 중인 아이디입니다." : "사용할 수 있는 아이디입니다."
            });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/auth/join
        // 회원가입. 성공하면 로그인 화면에서 바로 로그인할 수 있다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("join")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<IActionResult> Join([FromBody] CustomerJoinDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RepCustId) || string.IsNullOrWhiteSpace(dto.RepCustPwd))
            {
                return BadRequest(new { success = false, message = "아이디와 비밀번호는 필수입니다." });
            }
            if (dto.RepCustId.Length > 50)
            {
                return BadRequest(new { success = false, message = "아이디는 50자를 넘을 수 없습니다." });
            }
            // 새로 만드는 아이디는 형식을 제한한다(이미 있는 아이디에는 적용하지 않는다)
            if (!NewIdPattern.IsMatch(dto.RepCustId))
            {
                return BadRequest(new { success = false, message = NewIdRuleText });
            }
            var pwdError = PasswordPolicy.Check(dto.RepCustPwd, dto.RepCustId);
            if (pwdError != null)
            {
                return BadRequest(new { success = false, message = pwdError });
            }

            try
            {
                if (await _context.Customers.AnyAsync(c => c.RepCustId == dto.RepCustId))
                {
                    return Conflict(new { success = false, message = "이미 사용 중인 아이디입니다." });
                }

                var customer = new Customer { RepCustId = dto.RepCustId };
                ApplyProfile(dto, customer);
                customer.RepCustPwd = Hasher.HashPassword(customer, dto.RepCustPwd);
                customer.JoinDt = DateTime.Now;
                customer.PrvcChgDt = DateTime.Now;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                _audit.Write(HttpContext, "JOIN", customer.RepCustId);

                return Ok(new
                {
                    success = true,
                    repCustId = customer.RepCustId,
                    message = "회원가입이 완료되었습니다. 가입한 계정으로 로그인해 주세요."
                });
            }
            catch (Exception ex)
            {
                return this.Failure(ex, "회원가입 중 오류가 발생했습니다.");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/auth/profile?repCustId=xxx
        // 회원정보 조회. 기술문서·적합성선언서 화면의 자동 채움에도 사용한다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // 본인 정보만 볼 수 있다. 누구인지는 토큰에서 알아낸다.
            var repCustId = User.CustId();

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.RepCustId == repCustId);

            if (customer == null)
            {
                return NotFound(new { success = false, message = "회원정보를 찾을 수 없습니다." });
            }

            return Ok(new { success = true, data = ToProfile(customer) });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/auth/profile
        // 회원정보 수정. 비밀번호는 CurrentPwd 가 맞을 때만 바꾼다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] CustomerProfileUpdateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "전달된 데이터가 없습니다." });
            }

            // 본인 정보만 고칠 수 있다. 화면이 보낸 아이디는 덮어써서 쓰지 않는다.
            dto.RepCustId = User.CustId()!;

            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.RepCustId == dto.RepCustId);
                if (customer == null)
                {
                    return NotFound(new { success = false, message = "회원정보를 찾을 수 없습니다." });
                }

                // 비밀번호 변경은 현재 비밀번호가 맞을 때만 허용한다
                var passwordChanged = false;
                if (!string.IsNullOrWhiteSpace(dto.RepCustPwd))
                {
                    // 현재 비밀번호를 맞춰 보는 것도 로그인 시도와 같이 센다(이 화면으로 비밀번호를 대입하지 못하게)
                    if (_guard.LockedFor(dto.RepCustId) != null)
                    {
                        _audit.Write(HttpContext, "PASSWORD_CHANGE_BLOCKED", dto.RepCustId);
                        return StatusCode(StatusCodes.Status429TooManyRequests,
                            new { success = false, message = "시도가 너무 많습니다. 잠시 후 다시 시도해 주세요." });
                    }
                    if (!VerifyPassword(customer, dto.CurrentPwd))
                    {
                        _guard.RecordFailure(dto.RepCustId);
                        _audit.Write(HttpContext, "PASSWORD_CHANGE_FAIL", dto.RepCustId);
                        return BadRequest(new { success = false, message = "현재 비밀번호가 일치하지 않습니다." });
                    }
                    var pwdError = PasswordPolicy.Check(dto.RepCustPwd, dto.RepCustId);
                    if (pwdError != null)
                    {
                        return BadRequest(new { success = false, message = pwdError });
                    }
                    customer.RepCustPwd = Hasher.HashPassword(customer, dto.RepCustPwd);
                    passwordChanged = true;
                }

                ApplyProfile(dto, customer);
                customer.PrvcChgDt = DateTime.Now;

                await _context.SaveChangesAsync();
                _audit.Write(HttpContext, passwordChanged ? "PASSWORD_CHANGE" : "PROFILE_UPDATE", customer.RepCustId);

                return Ok(new { success = true, message = "회원정보가 수정되었습니다.", data = ToProfile(customer) });
            }
            catch (Exception ex)
            {
                return this.Failure(ex, "회원정보 수정 중 오류가 발생했습니다.");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 로그인 직후 프론트엔드가 호출하는 /auth/me
        // ─────────────────────────────────────────────────────────────
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            // 화면이 시작할 때 "지금 가진 토큰이 아직 유효한지, 누구인지"를 확인하는 데 쓴다.
            // 토큰이 없거나 만료됐으면 여기까지 오지 못하고 401 이 나간다.
            var repCustId = User.CustId();

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.RepCustId == repCustId);

            return Ok(new
            {
                data = customer == null
                    ? new { repCustId = (string?)null, profile = (CustomerProfileDto?)null }
                    : new { repCustId = (string?)customer.RepCustId, profile = (CustomerProfileDto?)ToProfile(customer) }
            });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/auth/refresh
        // 아직 유효한 토큰으로 새 토큰을 받는다. 화면이 사용 중에 주기적으로 부른다.
        // 토큰이 이미 만료됐으면 401 이 나가고, 다시 로그인해야 한다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var repCustId = User.CustId()!;

            // 탈퇴·삭제된 계정이 예전 토큰으로 계속 갱신하지 못하게 계정이 있는지 확인한다
            if (!await _context.Customers.AsNoTracking().AnyAsync(c => c.RepCustId == repCustId))
            {
                return Unauthorized(new { success = false, message = "로그인이 필요합니다. 다시 로그인해 주세요." });
            }

            return Ok(new
            {
                data = new
                {
                    accessToken = _tokens.Issue(repCustId),
                    expiresIn = _tokens.ExpiresInSeconds
                }
            });
        }

        [HttpPost("logout")]
        [AllowAnonymous] // 토큰이 이미 만료된 뒤에 눌러도 오류 없이 끝나야 한다
        public IActionResult Logout()
        {
            if (User.CustId() is { } who)
            {
                _audit.Write(HttpContext, "LOGOUT", who);
            }
            return Ok(new { message = "로그아웃 성공" });
        }

        // ═════════════════════════════════════════════════════════════
        // 헬퍼
        // ═════════════════════════════════════════════════════════════

        /// <summary>
        /// 저장된 비밀번호를 검증한다.
        /// 해시로 저장된 값이면 해시 검증, 예전 평문 값이면 문자열 비교로 받아준 뒤 해시로 승격한다.
        /// </summary>
        private bool VerifyPassword(Customer customer, string? input)
        {
            if (input == null || customer.RepCustPwd == null)
            {
                return false;
            }

            var stored = customer.RepCustPwd;

            // 해시인지 예전 평문인지는 길이가 아니라 형식으로 가린다.
            // 길이로 가리면 40자 이상인 평문 비밀번호가 해시로 오인돼 검증 중 예외가 난다.
            if (LooksLikePasswordHash(stored))
            {
                PasswordVerificationResult result;
                try
                {
                    result = Hasher.VerifyHashedPassword(customer, stored, input);
                }
                catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException)
                {
                    // 형식은 해시처럼 보이지만 내용이 깨진 값. 서버 오류가 아니라 로그인 실패로 처리한다.
                    return false;
                }

                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    customer.RepCustPwd = Hasher.HashPassword(customer, input);
                }
                return result != PasswordVerificationResult.Failed;
            }

            if (stored != input)
            {
                return false;
            }

            // 평문으로 저장돼 있던 계정을 이 시점에 해시로 올린다
            customer.RepCustPwd = Hasher.HashPassword(customer, input);
            return true;
        }

        /// <summary>
        /// 저장된 값이 ASP.NET Core PasswordHasher 가 만든 해시 형식인지 본다.
        /// 해시는 Base64 이고, 풀었을 때 첫 바이트가 형식 표시다(0x00 = 예전 V2, 0x01 = 지금 쓰는 V3).
        /// </summary>
        private static bool LooksLikePasswordHash(string stored)
        {
            var buffer = new byte[stored.Length];
            if (!Convert.TryFromBase64String(stored, buffer, out var length) || length == 0)
            {
                return false;
            }

            return buffer[0] switch
            {
                0x00 => length == 49,   // 표시 1 + salt 16 + 해시 32
                0x01 => length >= 45,   // 머리말 13 + salt 최소 16 + 해시 최소 16
                _ => false
            };
        }

        /// <summary>입력값을 엔티티에 옮긴다. null 인 항목은 기존 값을 유지한다.</summary>
        private static void ApplyProfile(CustomerJoinDto dto, Customer customer)
        {
            static string? Trim(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

            if (dto.BizNo      != null) customer.BizNo      = Trim(dto.BizNo);
            if (dto.CustTypeNm != null) customer.CustTypeNm = Trim(dto.CustTypeNm);
            if (dto.BizNm      != null) customer.BizNm      = Trim(dto.BizNm);
            if (dto.RepNm      != null) customer.RepNm      = Trim(dto.RepNm);
            if (dto.RoleNm     != null) customer.RoleNm     = Trim(dto.RoleNm);
            if (dto.IndstNm    != null) customer.IndstNm    = Trim(dto.IndstNm);
            if (dto.CntryNm    != null) customer.CntryNm    = Trim(dto.CntryNm);
            if (dto.AddrCd     != null) customer.AddrCd     = Trim(dto.AddrCd);
            if (dto.DtlAddr1   != null) customer.DtlAddr1   = Trim(dto.DtlAddr1);
            if (dto.DtlAddr2   != null) customer.DtlAddr2   = Trim(dto.DtlAddr2);
            if (dto.EmlAddr    != null) customer.EmlAddr    = Trim(dto.EmlAddr);
            if (dto.RepTelNo   != null) customer.RepTelNo   = Trim(dto.RepTelNo);
            if (dto.MblTelNo   != null) customer.MblTelNo   = Trim(dto.MblTelNo);
        }

        private static CustomerProfileDto ToProfile(Customer c) => new()
        {
            RepCustId = c.RepCustId,
            BizNo = c.BizNo,
            CustTypeNm = c.CustTypeNm,
            BizNm = c.BizNm,
            RepNm = c.RepNm,
            RoleNm = c.RoleNm,
            IndstNm = c.IndstNm,
            CntryNm = c.CntryNm,
            AddrCd = c.AddrCd,
            DtlAddr1 = c.DtlAddr1,
            DtlAddr2 = c.DtlAddr2,
            EmlAddr = c.EmlAddr,
            RepTelNo = c.RepTelNo,
            MblTelNo = c.MblTelNo,
            JoinDt = c.JoinDt,
            LastLgnDtm = c.LastLgnDtm
        };
    }
}
