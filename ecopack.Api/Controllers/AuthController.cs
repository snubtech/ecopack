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
 *    - 수정은 비밀번호를 뺀 나머지 정보를 먼저 반영하고,
 *      비밀번호는 현재 비밀번호가 맞을 때만 바꿉니다.
 *    - 요청에 담기지 않은 항목(null)은 기존 값을 그대로 둡니다.
 * ==============================================================================
 */
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
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

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/auth/login
        // ─────────────────────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.RepCustId))
            {
                return BadRequest(new { message = "아이디와 비밀번호를 입력해 주세요." });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.RepCustId == loginDto.RepCustId);

            if (customer == null || !VerifyPassword(customer, loginDto.RepCustPwd))
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 잘못되었습니다." });
            }

            customer.LastLgnDtm = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                data = new
                {
                    accessToken = "mock-jwt-token-sample",
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

                return Ok(new
                {
                    success = true,
                    repCustId = customer.RepCustId,
                    message = "회원가입이 완료되었습니다. 가입한 계정으로 로그인해 주세요."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "회원가입 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/auth/profile?repCustId=xxx
        // 회원정보 조회. 기술문서·적합성선언서 화면의 자동 채움에도 사용한다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] string repCustId)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "필수 파라미터(repCustId)가 누락되었습니다." });
            }

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
            if (dto == null || string.IsNullOrWhiteSpace(dto.RepCustId))
            {
                return BadRequest(new { success = false, message = "필수 값(repCustId)이 누락되었습니다." });
            }

            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.RepCustId == dto.RepCustId);
                if (customer == null)
                {
                    return NotFound(new { success = false, message = "회원정보를 찾을 수 없습니다." });
                }

                // 비밀번호 변경은 현재 비밀번호가 맞을 때만 허용한다
                if (!string.IsNullOrWhiteSpace(dto.RepCustPwd))
                {
                    if (!VerifyPassword(customer, dto.CurrentPwd))
                    {
                        return BadRequest(new { success = false, message = "현재 비밀번호가 일치하지 않습니다." });
                    }
                    customer.RepCustPwd = Hasher.HashPassword(customer, dto.RepCustPwd);
                }

                ApplyProfile(dto, customer);
                customer.PrvcChgDt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "회원정보가 수정되었습니다.", data = ToProfile(customer) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "회원정보 수정 중 오류가 발생했습니다.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 로그인 직후 프론트엔드가 호출하는 /auth/me
        // ─────────────────────────────────────────────────────────────
        [HttpGet("me")]
        public async Task<IActionResult> GetMe([FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return Ok(new { data = new { repCustId = (string?)null } });
            }

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

        [HttpPost("logout")]
        public IActionResult Logout()
        {
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

            // ASP.NET Core PasswordHasher 결과는 Base64 이고 충분히 길다.
            // 짧은 값은 예전 평문으로 보고 문자열 비교로 처리한다.
            if (stored.Length >= 40)
            {
                var result = Hasher.VerifyHashedPassword(customer, stored, input);
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
