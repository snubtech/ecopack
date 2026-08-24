import { clearAccessToken, request, setAccessToken } from './client'

export const authApi = {
    // 매개변수나 보낼 데이터 키를 백엔드 DTO와 일치시킵니다.
    login: async (repCustId, repCustPwd) => {
        const response = await request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({
                RepCustId: repCustId,
                RepCustPwd: repCustPwd
            }),
        })

        const payload = response.data
        setAccessToken(payload.accessToken)

        return payload
    },

    logout: async () => {
        try {
            await request('/auth/logout', { method: 'POST' })
        } finally {
            clearAccessToken()
        }
    },

    getMe: () => request('/auth/me'),
}