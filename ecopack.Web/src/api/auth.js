import http, { clearAccessToken, errorMessage, setAccessToken } from './http'

/**
 * 서버 호출을 실행하고 응답 본문을 돌려준다.
 * 실패하면 서버가 준 안내 문구(message)를 담은 Error 를 던진다. 화면은 err.message 를 그대로 보여 준다.
 */
async function call(promise, fallback = '요청을 처리하지 못했습니다. 잠시 후 다시 시도해 주세요.') {
    try {
        const response = await promise
        return response.data
    } catch (error) {
        throw new Error(errorMessage(error, fallback), { cause: error })
    }
}

/**
 * [인증 관련 API 통신 모음 객체]
 * 로그인, 로그아웃, 내 정보 확인 등 회원 인증과 관련된 서버 요청들을 모아둔 곳입니다.
 * 누구의 요청인지는 서버가 로그인 토큰에서 알아내므로, 아이디를 따로 실어 보내지 않습니다.
 */
export const authApi = {
    /**
     * [로그인 요청 함수]
     * 아이디와 비밀번호를 보내고, 성공하면 서버가 준 토큰을 보관합니다.
     *
     * @param {string} repCustId - 사용자 아이디
     * @param {string} repCustPwd - 사용자 비밀번호
     */
    login: async (repCustId, repCustPwd) => {
        const response = await call(
            // 아이디·비밀번호가 틀렸을 때의 401 은 "세션 만료"가 아니므로 공통 401 처리를 타지 않게 한다
            http.post('/api/auth/login', { RepCustId: repCustId, RepCustPwd: repCustPwd }, { skipAuthRedirect: true }),
            '로그인하지 못했습니다. 잠시 후 다시 시도해 주세요.',
        )
        const payload = response.data

        setAccessToken(payload.accessToken, payload.expiresIn)

        // 회원가입 때 입력한 회사·담당자 정보(profile)도 함께 담아
        // 기술문서·적합성선언서 화면이 자동으로 채워 쓸 수 있게 합니다.
        sessionStorage.setItem('prjuserid', JSON.stringify({
            repCustId: payload.repCustId,
            profile: payload.profile ?? null
        }));

        return payload
    },

    /**
     * [로그아웃 요청 함수]
     * 서버에 로그아웃을 알리고(감사 기록용), 브라우저에 보관한 토큰을 지웁니다.
     */
    logout: async () => {
        try {
            await http.post('/api/auth/logout', null, { skipAuthRedirect: true })
        } catch {
            // 서버에 알리지 못해도 로그아웃은 진행한다
        } finally {
            clearAccessToken()
        }
    },

    /**
     * [토큰 갱신]
     * 아직 유효한 토큰으로 새 토큰을 받습니다. 사용 중에 AuthProvider 가 주기적으로 부릅니다.
     */
    refresh: async () => {
        const response = await call(http.post('/api/auth/refresh'))
        setAccessToken(response.data.accessToken, response.data.expiresIn)
    },

    /**
     * [내 정보 조회 함수]
     * 지금 가진 토큰이 아직 유효한지, 누구인지 서버에 확인합니다. 화면이 시작할 때 부릅니다.
     */
    getMe: () => call(http.get('/api/auth/me')),

    /** [아이디 중복확인] 회원가입 화면에서 아이디를 쓸 수 있는지 서버에 물어봅니다. */
    checkId: (repCustId) => call(http.get('/api/auth/checkId', { params: { repCustId } })),

    /**
     * [회원가입]
     * customer 테이블에 새 계정을 만듭니다. 성공하면 로그인 화면으로 돌아가 로그인할 수 있습니다.
     * @param {object} form 아이디·비밀번호와 회사/담당자 정보
     */
    join: (form) => call(http.post('/api/auth/join', form)),

    /** [회원정보 조회] 로그인한 본인의 정보를 가져옵니다. */
    getProfile: () => call(http.get('/api/auth/profile')),

    /**
     * [회원정보 수정]
     * 비밀번호를 바꿀 때만 repCustPwd 와 currentPwd 를 함께 담아 보냅니다.
     */
    updateProfile: (form) => call(http.post('/api/auth/profile', form)),
}
