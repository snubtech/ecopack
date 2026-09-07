/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ClaimsPrincipalExtensions (로그인한 회원 ID 꺼내기)
 * ==============================================================================
 *
 * 1. 왜 필요한가
 *    - 예전에는 "나는 이 사람이다"를 클라이언트가 쿼리파라미터(repCustId)로
 *      자기 입으로 말하고, 서버는 그 말을 그대로 믿고 DB와 비교했다.
 *      그래서 다른 사람의 repCustId를 그냥 적어 보내면 그 사람 행세가 가능했다.
 *    - 지금은 로그인(POST /auth/login) 성공 시 서버가 서명한 JWT를 내려주고,
 *      이후 요청은 그 토큰을 Authorization: Bearer 헤더로 실어 보낸다.
 *      서명을 검증한 토큰 안의 클레임만 "진짜 로그인한 사람"으로 믿는다.
 *
 * 2. 하는 일
 *    - [Authorize]가 붙은 컨트롤러에서 `User.GetRepCustId()`로 토큰에 담긴
 *      고객 ID(repCustId)를 꺼낸다. 토큰이 없거나 위·변조됐으면 애초에
 *      미들웨어 단계에서 401로 막히므로, 여기까지 왔다면 검증된 값이다.
 * ==============================================================================
 */
using System.Security.Claims;

namespace ecopack.Api.Support
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>JWT에 로그인한 회원의 고객 ID(repCustId)를 담아 두는 클레임 이름.</summary>
        public const string RepCustIdClaimType = "repCustId";

        /// <summary>검증된 토큰에서 로그인한 회원의 고객 ID를 꺼낸다. 없으면 null.</summary>
        public static string? GetRepCustId(this ClaimsPrincipal user) =>
            user.FindFirst(RepCustIdClaimType)?.Value;
    }
}
