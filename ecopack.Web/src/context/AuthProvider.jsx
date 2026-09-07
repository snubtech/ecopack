/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useMemo, useState } from 'react'
import axios from 'axios'
import { authApi } from '../api/auth'
import { getAccessToken, clearAccessToken } from '../api/client'

/**
 * [1] 로그인 정보를 앱 전체(어디서든)에 공유하기 위한 '빈 도화지(Context)'를 만듭니다.
 * 이 도화지에 로그인한 유저 정보나 로그인/로그아웃 기능을 담아둘 거예요.
 */
const AuthContext = createContext(null)

/**
 * [JWT를 axios 공통 헤더에 실어 두기]
 * - projects.js / primaryTd.js 등 대부분의 화면은 axios를 직접 불러다 쓰지,
 *   Authorization 헤더를 챙겨 주는 client.js의 request()를 거치지 않는다.
 * - 그래서 로그인 토큰을 axios.defaults(전역 기본값)에 한 번 실어 두면,
 *   앞으로 어디서 axios.get/post를 부르든 자동으로 헤더에 실려 나간다.
 * - 토큰 없이 [Authorize] 컨트롤러를 부르면 401이 나므로 반드시 필요하다.
 */
function applyAuthHeader(token) {
    if (token) {
        axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
    } else {
        delete axios.defaults.headers.common['Authorization']
    }
}

// 새로고침 등으로 모듈이 다시 로드돼도, 세션에 토큰이 남아 있으면 헤더를 곧바로 복원한다.
applyAuthHeader(getAccessToken())

/**
 * [2] 로그인 상태와 기능을 관리하고, 하위 컴포넌트들에게 공급해 주는 울타리(Provider) 컴포넌트입니다.
 * 앱의 최상단(대문)을 이 컴포넌트로 감싸주면, 안에 있는 모든 화면에서 로그인 정보를 쓸 수 있습니다.
 */
export function AuthProvider({ children }) {

    // 💡 [수정 포인트 1] 세션 스토리지에서 'prjuserid' 키로 저장된 유저 정보를 가져옵니다.
    const [user, setUser] = useState(() => {
        try {
            const savedUser = sessionStorage.getItem('prjuserid');
            return savedUser ? JSON.parse(savedUser) : null;
        } catch (e) {
            console.error("저장된 유저 정보를 불러오는 중 에러 발생:", e);
            return null;
        }
    })

    /**
     * 다른 컴포넌트들에게 공유해 줄 '데이터와 기능 보따리'를 만듭니다.
     * useMemo는 불필요하게 다시 계산되는 걸 막아주는 최적화 도구입니다.
     */
    const value = useMemo(() => ({
        // 현재 로그인된 유저 정보 (없으면 null)
        user,

        // 유저 정보가 있으면 true(로그인 됨), 없으면 false(로그인 안 됨)
        isAuthenticated: Boolean(user),

        // 🔑 [로그인 함수]
        async login(userNo, password) {
            const res = await authApi.login(userNo, password)
            const userData = res.data || res

            // 1. 리액트 상태(상자)에 유저 정보 쏙 넣기!
            setUser(userData)

            // 💡 [수정 포인트 2] 세션 스토리지 키를 'prjuserid'로 지정하여 안전하게 저장합니다.
            sessionStorage.setItem('prjuserid', JSON.stringify(userData));

            // 서버가 내려준 진짜 로그인 토큰을 axios 공통 헤더에 실어 둔다.
            // (authApi.login 안에서 이미 sessionStorage에도 저장해 뒀다)
            applyAuthHeader(getAccessToken())

            return userData
        },

        // 🔄 [회원정보 갱신]
        // 회원정보 수정 후, 화면(사이드바 이름 등)과 세션에 새 프로필을 반영한다.
        updateProfile(profile) {
            setUser((prev) => {
                const next = { ...(prev || {}), profile }
                sessionStorage.setItem('prjuserid', JSON.stringify(next))
                return next
            })
        },

        // 🚪 [로그아웃 함수]
        async logout() {
            await authApi.logout()

            // 1. 리액트 상태 비우기
            setUser(null)

            // 💡 [수정 포인트 3] 로그아웃 시 'prjuserid'와 액세스 토큰을 깔끔하게 지웁니다.
            sessionStorage.removeItem('prjuserid');
            clearAccessToken();

            // axios 공통 헤더에 실어 뒀던 토큰도 지운다. 안 지우면 로그아웃 후에도
            // 이전 토큰이 계속 실려 나가 서버가 여전히 그 사람으로 착각한다.
            applyAuthHeader(null)
        },
    }), [user]) // user 값이 바뀔 때만 보따리를 새로 갱신합니다.

    // AuthContext 울타리를 쳐서, 그 안에 있는 자식 컴포넌트들(children)에게 보따리를 전달합니다.
    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

/**
 * [3] 컴포넌트 안에서 로그인 정보나 로그인/로그아웃 기능을 쏙 빼다 쓰기 위해 부르는 헬퍼 함수입니다.
 */
export function useAuth() {
    return useContext(AuthContext)
}