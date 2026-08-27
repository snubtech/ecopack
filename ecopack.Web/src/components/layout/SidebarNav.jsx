import { navigationGroups } from '../../config/navigation';
import { useAuth } from "../../context/AuthProvider";

function SettingsIcon() {
    return (
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z" />
            <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06-.06a1.65 1.65 0 0 0-.33 1.82V9c0 .69.4 1.3 1 1.51h.09a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1Z" />
        </svg>
    );
}

function HamburgerIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <line x1="3" y1="12" x2="21" y2="12" />
            <line x1="3" y1="6" x2="21" y2="6" />
            <line x1="3" y1="18" x2="21" y2="18" />
        </svg>
    );
}

export default function SidebarNav({ activeItemId, onSelectItem, onLogout, isCollapsed, onToggleCollapse }) {
    const { user } = useAuth();

    // 💡 [수정 포인트] user 객체 안의 repCustId를 최우선으로 가져오고, 없으면 기본값 '사용자'를 띄웁니다.
    const displayName = user?.repCustId || user?.companyName || user?.userName || '사용자';

    const handleLogout = () => {
        // 💡 무조건 세션 스토리지와 토큰을 먼저 깔끔하게 비워줍니다!
        sessionStorage.removeItem('prjuserid');
        localStorage.removeItem('token'); // 혹시 로컬스토리지에 남아있을지 모르는 토큰 정리

        // 만약 부모 컴포넌트에서 넘겨준 별도의 로그아웃 함수가 있다면 실행하고, 아니면 로그인 페이지로 이동합니다.
        if (typeof onLogout === 'function') {
            onLogout();
            return;
        }

        window.location.href = '/login';
    };

    return (
        <aside
            className={`dashboard-panel sidebar-nav ${isCollapsed ? 'collapsed' : ''}`}
            style={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                overflowX: 'hidden',
                paddingTop: '0px'
            }}
        >
            {/* 햄버거 버튼 영역 */}
            <div style={{
                height: '36px',
                minHeight: '36px',
                padding: '0 1rem',
                display: 'flex',
                alignItems: 'center',
                justifyContent: isCollapsed ? 'center' : 'flex-end',
                borderBottom: '1px solid #e5e7eb',
                flexShrink: 0,
                backgroundColor: '#ffffff'
            }}>
                <button
                    type="button"
                    className="icon-button"
                    onClick={onToggleCollapse}
                    aria-label="메뉴 접기/펼치기"
                    title="메뉴 접기/펼치기"
                    style={{ background: 'none', border: 'none', cursor: 'pointer', padding: '0.2rem', borderRadius: '4px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                >
                    <HamburgerIcon />
                </button>
            </div>

            {/* 브랜드 로고 영역 */}
            {!isCollapsed && (
                <div className="sidebar-brand" style={{ flexShrink: 0 }}>
                    <div className="logo-mark" aria-hidden="true" />
                    <div>
                        <strong className="logo-title">PACKAGEVIEW</strong>
                        <span className="logo-subtitle">ECO Packaging System</span>
                    </div>
                </div>
            )}

            {/* 유저 카드 영역 */}
            {!isCollapsed && (
                <div className="user-card" style={{ flexShrink: 0 }}>
                    <div className="user-card-top">
                        <div className="user-avatar">{displayName.slice(0, 1)}</div>
                        <div className="user-meta">
                            <strong>{displayName}</strong>
                            <button
                                type="button"
                                className="icon-button"
                                aria-label="설정"
                                title="로그아웃"
                                onClick={handleLogout}
                            >
                                <SettingsIcon />
                            </button>
                        </div>
                    </div>
                    <p className="user-welcome">
                        Welcome {displayName}.
                    </p>
                </div>
            )}

            {/* 메뉴 영역 */}
            <nav className="sidebar-menu" style={{ flex: 1, overflowY: 'auto' }}>
                {navigationGroups.map((group) => (
                    <section key={group.id} className="menu-group">
                        {!isCollapsed && <h2>{group.label}</h2>}
                        <ul>
                            {group.items.map((item) => (
                                <li key={item.id}>
                                    <button
                                        type="button"
                                        className={activeItemId === item.id ? 'menu-item active' : 'menu-item'}
                                        onClick={() => onSelectItem(item.id)}
                                        title={isCollapsed ? item.label : ''}
                                        style={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}
                                    >
                                        {isCollapsed ? item.label.slice(0, 2) : item.label}
                                    </button>
                                </li>
                            ))}
                        </ul>
                    </section>
                ))}
            </nav>
        </aside>
    );
}