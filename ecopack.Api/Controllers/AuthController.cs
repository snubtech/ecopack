using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;
using ecopack.Api.Dtos;

namespace ecopack.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.RepCustId == loginDto.RepCustId && c.RepCustPwd == loginDto.RepCustPwd);

            if (customer == null)
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 잘못되었습니다." });
            }

            return Ok(new
            {
                data = new
                {
                    accessToken = "mock-jwt-token-sample",
                    repCustId = customer.RepCustId
                },
                message = "로그인 성공"
            });
        }

        // 로그인 직후 프론트엔드가 호출하는 /auth/me 추가
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            // 현재는 간단히 Mock 데이터를 주거나 인증 통과 응답을 반환합니다.
            return Ok(new
            {
                data = new
                {
                    repCustId = "test" // 필요시 연동
                }
            });
        }

        // 로그아웃 요청 대응용 엔드포인트 추가 (에러 방지용)
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "로그아웃 성공" });
        }
    }
}