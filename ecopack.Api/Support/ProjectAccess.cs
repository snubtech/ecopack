/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ProjectAccess (프로젝트 소유자 확인)
 * ==============================================================================
 * 
 * 1. 하는 일
 *    - 어떤 프로젝트가 지금 로그인한 회원의 것인지 확인합니다.
 *    - 기본평가 하위 화면들(프로젝트 현황·기본사항·기술문서·적합성 선언서)이
 *      다른 사람의 프로젝트를 열거나 저장하지 못하도록 막는 데 함께 씁니다.
 * 
 * 2. 판단 기준 (IsOwnerAsync)
 *    - project 테이블의 repCustId(고객 ID) 또는 prjuserid(로그인 아이디)가
 *      요청한 회원과 같으면 본인 것으로 봅니다.
 *    - 예전에 만들어진 데이터는 repCustId 가 비어 있고 prjuserid 만 있는 경우가 있어
 *      두 컬럼을 모두 확인합니다.
 * 
 * 3. 알아둘 점 (2026-09 JWT 도입 이후)
 *    - repCustId 는 더 이상 화면이 자기 입으로 말하는 값(쿼리파라미터)이 아니다.
 *      로그인(POST /auth/login) 성공 시 서버가 서명해 내려준 JWT에서 서버가
 *      직접 꺼낸 값(User.GetRepCustId())만 이 함수에 넘긴다.
 *    - 예전에는 클라이언트가 아무 repCustId나 적어 보내면 그 사람 행세가 가능했다.
 *      지금은 토큰 서명을 검증한 뒤의 값만 오므로 위·변조가 불가능하다.
 * ==============================================================================
 */
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;

namespace ecopack.Api.Support
{
    /// <summary>
    /// 프로젝트가 요청한 회원의 것인지 확인한다.
    /// 기본평가 하위 화면들이 사용자별로만 보이도록 하는 데 쓴다.
    /// </summary>
    public static class ProjectAccess
    {
        /// <summary>
        /// 해당 프로젝트가 이 회원의 것인지 확인한다.
        /// 고객 ID가 비어 있으면(로그인 정보를 못 받은 경우) 접근을 막는다.
        /// </summary>
        public static async Task<bool> IsOwnerAsync(AppDbContext context, string? prjId, string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(prjId) || string.IsNullOrWhiteSpace(repCustId))
            {
                return false;
            }

            return await context.Project
                .AsNoTracking()
                .AnyAsync(p => p.PrjId == prjId
                               && (p.RepCustId == repCustId || p.Prjuserid == repCustId));
        }

        /// <summary>다른 회원의 자료에 접근할 때 돌려줄 안내 문구</summary>
        public const string DeniedMessage = "본인이 만든 프로젝트만 조회·수정할 수 있습니다.";
    }
}
