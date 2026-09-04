import { useEffect, useRef, useState } from 'react'

import { AuthProvider, useAuth } from './context/AuthProvider'
import DashboardLayout from './components/layout/DashboardLayout'
import LoginPage from './pages/LoginPage.jsx'
import JoinPage from './pages/JoinPage.jsx'
import './styles/global.css'
import './styles/dashboard.css'

function AppContent() {
    const { isAuthenticated, loading, logout } = useAuth()
    const timerRef = useRef(null)

    // 로그인 화면 ↔ 회원가입 화면 전환 (로그인 전에만 쓰인다)
    const [showJoin, setShowJoin] = useState(false)
    const [joinedId, setJoinedId] = useState('')

    useEffect(() => {
        // 💡 handleLogout을 useEffect 내부로 이동시킵니다.
        const handleLogout = async () => {
            try {
                if (logout) {
                    await logout()
                }
            } catch (error) {
                console.error('로그아웃 처리 중 오류 발생:', error)
            }

            localStorage.removeItem('ecopack.accessToken')
            localStorage.removeItem('token')
            localStorage.removeItem('user')
            window.location.reload()
        }

        if (!isAuthenticated) {
            if (timerRef.current) clearTimeout(timerRef.current)
            return
        }

        const handleIdleTimeout = () => {
            handleLogout()
        }

        const resetTimer = () => {
            if (timerRef.current) clearTimeout(timerRef.current)
            timerRef.current = setTimeout(handleIdleTimeout, 30 * 60 * 1000)
        }

        const events = ['mousemove', 'keydown', 'mousedown', 'touchstart', 'scroll']
        events.forEach((event) => {
            window.addEventListener(event, resetTimer)
        })

        resetTimer()

        return () => {
            if (timerRef.current) clearTimeout(timerRef.current)
            events.forEach((event) => {
                window.removeEventListener(event, resetTimer)
            })
        }
    }, [isAuthenticated, logout]) // 💡 이제 린트 경고가 발생하지 않습니다.

    if (loading) {
        return <p className="loading-text">세션 확인 중...</p>
    }

    if (!isAuthenticated) {
        // 회원가입을 마치면 로그인 화면으로 돌아와 가입한 아이디로 바로 로그인한다
        if (showJoin) {
            return (
                <JoinPage
                    onDone={(id) => { setJoinedId(id); setShowJoin(false) }}
                    onCancel={() => setShowJoin(false)}
                />
            )
        }
        return <LoginPage onJoin={() => setShowJoin(true)} joinedId={joinedId} />
    }

    // 외부에서 쓸 수 있도록 별도의 핸들러가 필요하다면 여기서 선언하거나 처리할 수 있습니다.
    const handleManualLogout = async () => {
        try {
            if (logout) await logout()
        } catch (error) {
            console.error('로그아웃 오류:', error)
        }
        localStorage.removeItem('ecopack.accessToken')
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        window.location.reload()
    }

    return <DashboardLayout onLogout={handleManualLogout} />
}

function App() {
    return (
        <AuthProvider>
            <AppContent />
        </AuthProvider>
    )
}

export default App