/* eslint-disable react-refresh/only-export-components */
import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react'
import { authApi } from '../api/auth'
import { closeChatSession } from '../api/llmChat'
import { UNAUTHORIZED_EVENT, clearAccessToken, getAccessToken, getTokenAge } from '../api/http'

/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - AuthProvider (로그인 상태 관리)
 * ==============================================================================
 *
 * 1. 로그인 여부를 어떻게 판단하나
 *    - 예전에는 브라우저에 저장된 값이 있으면 로그인한 것으로 봤습니다.
 *      저장값은 누구나 직접 넣을 수 있으므로 그것만으로는 믿을 수 없습니다.
 *    - 지금은 화면이 시작할 때 서버에 "이 토큰이 아직 유효한가"를 물어봅니다(/auth/me).
 *      확인이 끝날 때까지는 loading 이 true 이고, App 은 "세션 확인 중"을 보여 줍니다.
 *
 * 2. 토큰이 만료되면
 *    - 서버가 401 을 주면 공통 호출 모듈(http.js)이 알림을 보내고,
 *      여기서 로그인 상태를 지워 로그인 화면으로 돌아갑니다.
 *
 * 3. 토큰 갱신
 *    - 토큰에는 만료 시간이 있습니다(기본 30분). 사용자가 화면을 쓰고 있는 동안에는
 *      유효 시간의 절반이 지나면 새 토큰으로 바꿔 받아, 작업 중에 끊기지 않게 합니다.
 *    - 아무 조작 없이 두면 갱신하지 않으므로 만료 시간이 지나면 다시 로그인해야 합니다.
 * ==============================================================================
 */
const AuthContext = createContext(null)

const USER_KEY = 'prjuserid'
const CHAT_SESSION_KEY = 'ecopack.llmSessionId'

function readSavedUser() {
    // 토큰이 없으면 저장된 사용자 정보가 있어도 로그인한 것으로 보지 않는다
    if (!getAccessToken()) return null
    try {
        const saved = sessionStorage.getItem(USER_KEY)
        return saved ? JSON.parse(saved) : null
    } catch (e) {
        console.error('저장된 유저 정보를 불러오는 중 에러 발생:', e)
        return null
    }
}

function clearSession() {
    sessionStorage.removeItem(USER_KEY)
    sessionStorage.removeItem(CHAT_SESSION_KEY)
    clearAccessToken()
}

export function AuthProvider({ children }) {
    const [user, setUser] = useState(readSavedUser)
    // 토큰이 있으면 서버 확인이 끝날 때까지 기다린다
    const [loading, setLoading] = useState(() => Boolean(getAccessToken()))
    // 로그인 화면에 보여 줄 안내(세션 만료 등)
    const [notice, setNotice] = useState('')

    // ── 1. 시작할 때 서버에 토큰이 유효한지 확인한다 ─────────────────
    useEffect(() => {
        if (!getAccessToken()) {
            clearSession()
            return
        }

        let alive = true
        authApi.getMe()
            .then((res) => {
                if (!alive) return
                const me = res?.data
                if (!me?.repCustId) {
                    clearSession()
                    setUser(null)
                    return
                }
                // 서버가 알려 준 값으로 맞춘다(저장값을 고쳐 다른 사람인 척할 수 없게)
                const next = { repCustId: me.repCustId, profile: me.profile ?? null }
                sessionStorage.setItem(USER_KEY, JSON.stringify(next))
                setUser(next)
            })
            .catch(() => {
                // 401 이면 아래의 UNAUTHORIZED_EVENT 처리가 이미 정리했다.
                // 서버에 닿지 못한 경우에도 확인되지 않은 채로 들여보내지 않는다.
                if (!alive) return
                clearSession()
                setUser(null)
            })
            .finally(() => { if (alive) setLoading(false) })

        return () => { alive = false }
    }, [])

    // ── 2. 서버가 401 을 주면 로그인 화면으로 돌아간다 ───────────────
    useEffect(() => {
        const onUnauthorized = () => {
            clearSession()
            setUser((prev) => {
                if (prev) setNotice('로그인 시간이 만료되었습니다. 다시 로그인해 주세요.')
                return null
            })
        }
        window.addEventListener(UNAUTHORIZED_EVENT, onUnauthorized)
        return () => window.removeEventListener(UNAUTHORIZED_EVENT, onUnauthorized)
    }, [])

    // ── 3. 사용 중에는 토큰을 갱신한다 ────────────────────────────────
    const lastActivity = useRef(0)
    useEffect(() => {
        if (!user) return undefined

        const markActive = () => { lastActivity.current = Date.now() }
        markActive() // 로그인했거나 화면을 막 연 시점도 사용 중으로 본다
        const events = ['mousemove', 'keydown', 'mousedown', 'touchstart', 'scroll']
        events.forEach((ev) => window.addEventListener(ev, markActive, { passive: true }))

        let refreshing = false
        const timer = setInterval(async () => {
            const age = getTokenAge()
            if (!age || refreshing) return

            const halfPassed = age.ageMs > age.lifetimeMs / 2
            const recentlyActive = Date.now() - lastActivity.current < 5 * 60 * 1000
            if (!halfPassed || !recentlyActive) return

            refreshing = true
            try {
                await authApi.refresh()
            } catch {
                // 갱신에 실패해도 지금 토큰은 만료 전까지 쓸 수 있다. 만료되면 401 처리가 정리한다.
            } finally {
                refreshing = false
            }
        }, 30 * 1000)

        return () => {
            clearInterval(timer)
            events.forEach((ev) => window.removeEventListener(ev, markActive))
        }
    }, [user])

    const login = useCallback(async (userNo, password) => {
        const res = await authApi.login(userNo, password)
        const userData = res.data || res
        const next = { repCustId: userData.repCustId, profile: userData.profile ?? null }

        setNotice('')
        setUser(next)
        sessionStorage.setItem(USER_KEY, JSON.stringify(next))
        lastActivity.current = Date.now()
        return userData
    }, [])

    // 회원정보 수정 후, 화면(사이드바 이름 등)과 세션에 새 프로필을 반영한다.
    const updateProfile = useCallback((profile) => {
        setUser((prev) => {
            const next = { ...(prev || {}), profile }
            sessionStorage.setItem(USER_KEY, JSON.stringify(next))
            return next
        })
    }, [])

    const logout = useCallback(async () => {
        // 우측 AI 채팅창의 대화 세션을 먼저 닫습니다.
        // 닫아도 대화 기록은 지우지 않습니다(상태만 CLOSED). 지난 대화 목록에서 다시 볼 수 있고,
        // 2일이 지나면 서버가 알아서 백업 테이블과 JSON 파일로 옮깁니다.
        // 토큰을 지운 뒤에 부르면 인증이 풀려 실패하므로 반드시 로그아웃보다 먼저 호출합니다.
        await closeChatSession(sessionStorage.getItem(CHAT_SESSION_KEY))

        await authApi.logout()

        setUser(null)
        clearSession()
    }, [])

    const value = useMemo(() => ({
        user,
        // 서버가 확인해 준 사용자가 있을 때만 로그인한 것으로 본다
        isAuthenticated: Boolean(user) && !loading,
        loading,
        notice,
        login,
        updateProfile,
        logout,
    }), [user, loading, notice, login, updateProfile, logout])

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

/** 컴포넌트 안에서 로그인 정보나 로그인/로그아웃 기능을 꺼내 쓰는 헬퍼 */
export function useAuth() {
    return useContext(AuthContext)
}
