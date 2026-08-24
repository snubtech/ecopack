import { useEffect, useState } from 'react'
import { useAuth } from '../context/useAuth'
import { api } from '../api/client'

export default function HomePage() {
  const { user } = useAuth()
  const [health, setHealth] = useState(null)
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(true)

  const [checkedAt, setCheckedAt] = useState(null)

  async function loadHealth() {
    setLoading(true)
    setError(null)

    try {
      const response = await api.getHealth()
      setHealth(response)
      setCheckedAt(new Date())
    } catch (err) {
      setError(err.message)
      setHealth(null)
    } finally {
      setLoading(false)
    }
  }

    useEffect(() => {
        const fetchHealth = async () => {
            try {
                await loadHealth()
            } catch (error) {
                console.error(error)
            }
        }

        fetchHealth()
    }, [])

  return (
    <section className="hero">
      <p className="eyebrow">EcoPack Platform</p>
      <h1>친환경 포장 솔루션을 위한 기본 프레임</h1>
      <p className="lead">
        {user?.userName}님, EcoPack 플랫폼에 로그인되었습니다.
        {user?.companyName ? ` (${user.companyName})` : ''}
        도메인 기능은 이 구조 위에 추가하면 됩니다.
      </p>

      <div className="status-card">
        <div className="status-card-header">
          <h2>API 상태</h2>
          <button
            type="button"
            className="ghost-button"
            onClick={loadHealth}
            disabled={loading}
          >
            {loading ? '확인 중...' : '다시 확인'}
          </button>
        </div>
        {loading && !health && <p>연결 확인 중...</p>}
        {error && <p className="error">연결 실패: {error}</p>}
        {health?.data && (
          <dl>
            <div>
              <dt>상태</dt>
              <dd>{health.data.status}</dd>
            </div>
            <div>
              <dt>서비스</dt>
              <dd>{health.data.service}</dd>
            </div>
            <div>
              <dt>응답 시각</dt>
              <dd>{new Date(health.data.timestamp).toLocaleString()}</dd>
            </div>
            {checkedAt && (
              <div>
                <dt>확인 시각</dt>
                <dd>{checkedAt.toLocaleString()}</dd>
              </div>
            )}
          </dl>
        )}
      </div>
    </section>
  )
}
