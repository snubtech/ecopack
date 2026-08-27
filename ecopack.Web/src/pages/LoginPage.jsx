import { useState } from 'react'
//import { useAuth } from '../context/useAuth'
//import { useAuth } from "../hooks/useAuth"; // 또는 폴더 위치에 맞는 상대 경로 (예: ../hooks/useAuth)
import { useAuth } from "../context/AuthProvider";

export default function LoginPage() {
  const { login } = useAuth()
  const [userNo, setUserNo] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setSubmitting(true)

    try {
      await login(userNo, password)
    } catch (err) {
      setError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="auth-page">
      <section className="auth-card">
              <p className="eyebrow">PACKAGEVIEW Login</p>
        <h1>로그인</h1>
              <p className="lead">PACKAGEVIEW에 오신 것을 환영합니다.</p>

        <form className="auth-form" onSubmit={handleSubmit}>
          <label>
            아이디 (userno)
            <input
              type="text"
              value={userNo}
              onChange={(event) => setUserNo(event.target.value)}
              autoComplete="username"
              required
            />
          </label>

          <label>
            비밀번호
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password"
              required
            />
          </label>

          {error && <p className="error">{error}</p>}

          <button type="submit" className="primary-button" disabled={submitting}>
            {submitting ? '로그인 중...' : '로그인'}
          </button>
        </form>
      </section>
    </div>
  )
}
