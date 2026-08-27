/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useMemo, useState } from 'react'
import { authApi } from '../api/auth'

/**
 * [1] 로그인 정보를 앱 전체(어디서든)에 공유하기 위한 '빈 도화지(Context)'를 만듭니다.
 * 이 도화지에 로그인한 유저 정보나 로그인/로그아웃 기능을 담아둘 거예요.
 */
const AuthContext = createContext(null)

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

            return userData
        },

        // 🚪 [로그아웃 함수]
        async logout() {
            await authApi.logout()

            // 1. 리액트 상태 비우기
            setUser(null)

            // 💡 [수정 포인트 3] 로그아웃 시 'prjuserid'와 액세스 토큰을 깔끔하게 지웁니다.
            sessionStorage.removeItem('prjuserid');
            sessionStorage.removeItem('ecopack.accessToken');
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