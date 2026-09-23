import { useState } from 'react'
import { useAuth } from "../context/AuthProvider"
import loginHeroImg from '../assets/loginbg.png'

export default function LoginPage({ onJoin, joinedId }) {
    const { login } = useAuth()
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
        <div className="center-card-login-wrapper">
            {/* 전체 카드 박스 */}
            <div className="login-card-box">

                {/* 1. 좌측 비주얼 이미지 영역 (16:9 비율 공간 확보) */}
                <div className="card-hero-pane">
                    <img src={loginHeroImg} alt="친환경 패키지 비주얼" className="card-hero-img" />
                </div>

                {/* 2. 우측 로그인 폼 영역 */}
                <div className="card-form-pane">
                    <div className="card-form-content">
                        <div className="card-header">
                            <h1>로그인</h1>
                            {joinedId
                                ? <p className="desc">회원가입이 완료되었습니다. 로그인해 주세요.</p>
                                : <p className="desc">패키지의 새로운 설계를 시작하세요.</p>}
                        </div>

                        <form className="card-auth-form" onSubmit={handleSubmit}>
                            <div className="input-group">
                                <label>아이디 (userno)</label>
                                <input
                                    type="text"
                                    value={userNo}
                                    onChange={(e) => setUserNo(e.target.value)}
                                    placeholder="아이디를 입력하세요."
                                    autoComplete="username"
                                    required
                                />
                            </div>

                            <div className="input-group">
                                <label>비밀번호</label>
                                <input
                                    type="password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="비밀번호를 입력하세요."
                                    autoComplete="current-password"
                                    required
                                />
                            </div>

                            {error && <p className="error-msg">{error}</p>}

                            <button type="submit" className="btn-primary" disabled={submitting}>
                                {submitting ? '로그인 중...' : '로그인 →'}
                            </button>

                            <button
                                type="button"
                                className="btn-secondary"
                                onClick={onJoin}
                                disabled={submitting}
                            >
                                회원가입
                            </button>
                        </form>
                    </div>
                </div>

            </div>

            <style>{`
        .center-card-login-wrapper {
          position: fixed;
          top: 0;
          left: 0;
          width: 100vw;
          height: 100vh;
          background-color: #f3f4f6;
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 9999;
          margin: 0;
          padding: 20px;
          box-sizing: border-box;
        }

        .center-card-login-wrapper *, .center-card-login-wrapper *:before, .center-card-login-wrapper *:after {
          box-sizing: border-box;
        }

        /* 중앙 로그인 카드 박스: 이미지 비율(16:9)에 맞춘 고정 높이와 너비 설정 */
        .login-card-box {
          display: flex;
          width: 100%;
          max-width: 980px;
          height: 410px; /* 이미지 원본 비율에 맞게 높이 최적화 */
          background-color: #ffffff;
          border-radius: 12px;
          overflow: hidden;
          box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
        }

        /* 좌측 이미지 영역: flex 비율 구조 유지하면서 100% 꽉 채우기 */
        .card-hero-pane {
          flex: 1.75; /* 기존 7:3 비율의 시각적 균형 유지 */
          height: 100%;
          background-color: #0b2317;
          position: relative;
          overflow: hidden;
          display: flex;
        }

        .card-hero-img {
          width: 100%;
          height: 100%;
          /* 이미지가 영역에 딱 맞게 들어가며 위아래 여백을 완전히 없앰 */
          object-fit: fill; 
          display: block;
        }

        /* 우측 폼 영역 */
        .card-form-pane {
          flex: 1;
          height: 100%;
          display: flex;
          align-items: center;
          justify-content: center;
          background-color: #ffffff;
          padding: 30px;
          overflow-y: auto;
        }

        .card-form-content {
          width: 100%;
          max-width: 280px;
        }

        .card-header h1 {
          font-size: 24px;
          font-weight: 700;
          color: #111827;
          margin: 0 0 4px 0;
        }

        .card-header .desc {
          font-size: 12px;
          color: #6b7280;
          margin: 0 0 16px 0;
        }

        .input-group {
          margin-bottom: 12px;
        }

        .input-group label {
          display: block;
          font-size: 11px;
          font-weight: 600;
          color: #374151;
          margin-bottom: 4px;
        }

        .input-group input {
          width: 100%;
          padding: 10px 12px;
          border: 1px solid #d1d5db;
          border-radius: 6px;
          font-size: 13px;
          outline: none;
          background-color: #ffffff;
          transition: border-color 0.2s, box-shadow 0.2s;
        }

        .input-group input:focus {
          border-color: #059669;
          box-shadow: 0 0 0 3px rgba(5, 150, 105, 0.1);
        }

        .btn-primary {
          width: 100%;
          margin-top: 4px;
          padding: 11px;
          background: #059669;
          color: white;
          border: none;
          border-radius: 6px;
          font-size: 14px;
          font-weight: 600;
          cursor: pointer;
          transition: background 0.2s;
        }

        .btn-primary:hover:not(:disabled) {
          background: #047857;
        }

        .btn-secondary {
          width: 100%;
          margin-top: 8px;
          padding: 9px;
          border: 1px solid #d1d5db;
          background: #ffffff;
          color: #374151;
          border-radius: 6px;
          cursor: pointer;
          font-size: 13px;
          font-weight: 500;
          transition: background 0.2s;
        }

        .btn-secondary:hover:not(:disabled) {
          background: #f9fafb;
        }

        .error-msg {
          color: #dc2626;
          font-size: 12px;
          margin-bottom: 8px;
        }

        /* 반응형 모바일 화면 대응 */
        @media (max-width: 768px) {
          .center-card-login-wrapper {
            padding: 0;
          }
          .login-card-box {
            height: 100vh;
            max-width: 100%;
            border-radius: 0;
            flex-direction: column;
          }
          .card-hero-pane {
            display: none;
          }
          .card-form-pane {
            flex: 1;
            height: 100vh;
          }
        }
      `}</style>
        </div>
    )
}