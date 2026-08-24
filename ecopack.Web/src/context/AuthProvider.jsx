import { useMemo, useState, useEffect } from 'react'
import { authApi } from '../api/auth'
import { clearAccessToken, getAccessToken } from '../api/client'
import { AuthContext } from './AuthContext'

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        let isMounted = true

        const initializeAuth = async () => {
            const token = getAccessToken()
            if (!token) {
                if (isMounted) setLoading(false)
                return
            }

            try {
                const response = await authApi.getMe()
                if (isMounted) {
                    setUser(response.data)
                }
            } catch {
                clearAccessToken()
            } finally {
                if (isMounted) {
                    setLoading(false)
                }
            }
        }

        initializeAuth()

        return () => {
            isMounted = false
        }
    }, [])

    const value = useMemo(
        () => ({
            user,
            loading,
            isAuthenticated: Boolean(user),
            async login(userNo, password) {
                const response = await authApi.login(userNo, password)
                const userData = response.data || response
                setUser(userData)
                return userData
            },
            async logout() {
                await authApi.logout()
                setUser(null)
            },
        }),
        [user, loading],
    )

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}