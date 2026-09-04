import { clearAccessToken, request, setAccessToken } from './client'

/**
 * [인증 관련 API 통신 모음 객체]
 * 로그인, 로그아웃, 내 정보 확인 등 회원 인증과 관련된 서버 요청들을 모아둔 곳입니다.
 */
export const authApi = {
    /**
     * [로그인 요청 함수]
     * 사용자가 입력한 아이디와 비밀번호를 받아 백엔드 서버로 로그인 요청을 보냅니다.
     * 
     * @param {string} repCustId - 사용자 아이디
     * @param {string} repCustPwd - 사용자 비밀번호
     */
    login: async (repCustId, repCustPwd) => {
        // 매개변수나 보낼 데이터 키를 백엔드 DTO(C# 등 서버 규격)와 정확히 일치시킵니다.
        const response = await request('/auth/login', {
            method: 'POST', // 데이터를 서버로 보내서 확인받는 POST 방식
            body: JSON.stringify({
                RepCustId: repCustId,  //좌측이 dto의 변수명,우측이 프론트에서 입력받은 변수명 
                RepCustPwd: repCustPwd //좌측이 dto의 변수명,우측이 프론트에서 입력받은 변수명
            }),
        })
     
        // 서버가 응답해 준 결과물 안에서 데이터 꾸러미(payload)를 꺼냅니다.
        const payload = response.data

        // 로그인 성공 시 서버가 보내준 진짜 토큰(열쇠)을 세션 스토리지에 쏙 저장합니다.
        setAccessToken(payload.accessToken)

        // 💡 세션 스토리지 키를 'prjuserid'로 지정하여 저장합니다.
        //    회원가입 때 입력한 회사·담당자 정보(profile)도 함께 담아
        //    기술문서·적합성선언서 화면이 자동으로 채워 쓸 수 있게 합니다.
        sessionStorage.setItem('prjuserid', JSON.stringify({
            repCustId: payload.repCustId,
            profile: payload.profile ?? null
        }));

        // 화면이나 컨트롤 타워(AuthProvider)에서 쓸 수 있도록 유저/토큰 정보가 담긴 payload를 돌려줍니다.
        return payload
    },

    /**
     * [로그아웃 요청 함수]
     * 서버에 로그아웃 요청을 알리고, 브라우저에 저장되어 있던 토큰을 깔끔하게 폐기합니다.
     */
    logout: async () => {
        try {
            // 서버에 "저 로그아웃합니다!" 하고 POST 요청을 보냅니다.
            await request('/auth/logout', { method: 'POST' })
        } finally {
            // 서버 통신이 성공하든 실패하든 상관없이, 브라우저 창고에 있던 토큰(별명: ecopack.accessToken)은 무조건 지워버립니다.
            clearAccessToken()
        }
    },

    /**
     * [내 정보 조회 함수]
     * "지금 이 토큰을 가진 사람이 누구지?" 하고 서버에 물어봐서 현재 로그인한 유저의 프로필 정보를 가져옵니다.
     */
    getMe: (repCustId) =>
        request(`/auth/me${repCustId ? `?repCustId=${encodeURIComponent(repCustId)}` : ''}`),

    /**
     * [아이디 중복확인]
     * 회원가입 화면에서 아이디를 쓸 수 있는지 서버에 물어봅니다.
     */
    checkId: (repCustId) =>
        request(`/auth/checkId?repCustId=${encodeURIComponent(repCustId)}`),

    /**
     * [회원가입]
     * customer 테이블에 새 계정을 만듭니다. 성공하면 로그인 화면으로 돌아가 로그인할 수 있습니다.
     * @param {object} form 아이디·비밀번호와 회사/담당자 정보
     */
    join: (form) =>
        request('/auth/join', { method: 'POST', body: JSON.stringify(form) }),

    /**
     * [회원정보 조회]
     */
    getProfile: (repCustId) =>
        request(`/auth/profile?repCustId=${encodeURIComponent(repCustId)}`),

    /**
     * [회원정보 수정]
     * 비밀번호를 바꿀 때만 repCustPwd 와 currentPwd 를 함께 담아 보냅니다.
     */
    updateProfile: (form) =>
        request('/auth/profile', { method: 'POST', body: JSON.stringify(form) }),
}