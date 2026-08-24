import { useAuth } from '../../context/AuthContext'

export default function AppLayout({ children }) {
  const { user, logout } = useAuth()

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true" />
          <div>
            <strong>EcoPack</strong>
            <span>Research Platform</span>
          </div>
        </div>

        <div className="header-actions">
          {user && (
            <span className="user-chip">
              {user.userName} ({user.userNo})
            </span>
          )}
          <button type="button" className="ghost-button" onClick={() => logout()}>
            로그아웃
          </button>
        </div>
      </header>
      <main>{children}</main>
    </div>
  )
}
