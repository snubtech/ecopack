import { useState } from 'react'
//import { useAuth } from '../context/useAuth'
//import { useAuth } from "../hooks/useAuth"; // 또는 폴더 위치에 맞는 상대 경로 (예: ../hooks/useAuth)
import { useAuth } from "../context/AuthProvider";

export default function LoginPage({ onJoin, joinedId }) {
  const { login } = useAuth()
  // 회원가입을 막 마치고 돌아온 경우 아이디를 미리 채워 준다
  const [userNo, setUserNo] = useState(joinedId || '')
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
              {joinedId
          ? <p className="lead">회원가입이 완료되었습니다. 가입한 계정으로 로그인해 주세요.</p>
          : <p className="lead">PACKAGEVIEW에 오신 것을 환영합니다.</p>}

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

          <button
            type="button"
            className="join-link-button"
            onClick={onJoin}
            disabled={submitting}
          >
            회원가입
          </button>
        </form>

        <style>{`
          .join-link-button {
            width: 100%;
            margin-top: 10px;
            padding: 11px 16px;
            border: 1px solid #cbd5e1;
            background: #fff;
            color: #334155;
            border-radius: 6px;
            cursor: pointer;
            font: inherit;
            font-size: 14px;
            font-weight: 500;
          }
          .join-link-button:hover:not(:disabled) { background: #f8fafc; }
          .join-link-button:disabled { opacity: .45; cursor: not-allowed; }
        `}</style>
      </section>
    </div>
  )
}
