import { useState } from 'react';
import SidebarNav from './SidebarNav';
import AssistantPanel from './AssistantPanel';
import Prjdefault from '../../pages/Prjdefault';
//import SamplePage from '../../pages/sampage';
import Projects from '../../pages/Projects';
import Prjtemplate from '../../pages/Prjtemplate'; 
import PrimaryTd from '../../pages/PrimaryTd';
import PrimaryDoc from '../../pages/PrimaryDoc';
import { navigationGroups } from '../../config/navigation';

const DashboardLayout = ({ onLogout }) => {
    const [currentMenu, setCurrentMenu] = useState('project-history');
    const [isCollapsed, setIsCollapsed] = useState(false); // 사이드바 접힘 상태

    // state와 useEffect를 쓰지 않고, 렌더링될 때 세션에서 바로 읽어옵니다.
    // (이렇게 하면 setState 연쇄 호출 경고와 'assigned but never used' 경고가 원천 차단됩니다)
    const projectInfo = {
        id: sessionStorage.getItem('currentPrjId') || '',
        name: sessionStorage.getItem('currentPrjNm') || '',
        packLevel: sessionStorage.getItem('currentPackLevel') || ''
    };

    const pageInfo = (() => {
        for (const group of navigationGroups) {
            const found = group.items.find((item) => item.id === currentMenu);
            if (found) {
                return { name: found.label, category: group.label };
            }
        }
        return { name: currentMenu, category: '기본평가' };
    })();

    const renderBusinessContent = () => {
        switch (currentMenu) {
            case 'project-history':
                return <Projects onSelectItem={setCurrentMenu} />;
            case 'start-project':
                return <Prjdefault onSelectItem={setCurrentMenu} />;
            case 'prjdefault': // 'prjdefault'로 와도 Prjdefault 컴포넌트를 띄운다!
                return <Prjdefault onSelectItem={setCurrentMenu} />;
            case 'prjtemplate': // 'prjtemplate'로 와도 Prjdefault 컴포넌트를 띄운다!
                return <Prjtemplate onSelectItem={setCurrentMenu} />;
            case 'td': // 기술문서(모듈 A) — primary_td
                return <PrimaryTd onSelectItem={setCurrentMenu} />;
            case 'doc': // DOC 적합성 선언서 — primary_doc
                return <PrimaryDoc onSelectItem={setCurrentMenu} />;
            default:
                return (
                    <div style={{ width: '100%', height: '100%', boxSizing: 'border-box' }}>
                        <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '0.5rem', color: '#111827' }}>
                            {pageInfo.name} 화면
                        </h3>
                        <p style={{ color: '#4b5563', fontSize: '0.9rem' }}>
                            현재 선택하신 [{pageInfo.category} &gt; {pageInfo.name}] 메뉴의 비즈니스 컨텍스트가 이 영역에 표시됩니다.
                        </p>
                    </div>
                );
        }
    };

    return (
        /* 💡 핵심: 외부 CSS의 grid 고정을 무력화하고 상태에 따라 그리드 컬럼 너비를 동적으로 변경 */
        <div
            className="dashboard-shell"
            style={{
                display: 'grid',
                gridTemplateColumns: isCollapsed ? '70px 1fr 320px' : '260px 1fr 320px',
                transition: 'grid-template-columns 0.3s ease',
                width: '100vw',
                height: '100vh',
                boxSizing: 'border-box',
                overflow: 'hidden'
            }}
        >
            {/* 1. 좌측 네비게이션바 */}
            <aside
                className="dashboard-panel"
                style={{
                    width: '100%',
                    height: '100%',
                    overflow: 'hidden',
                    boxSizing: 'border-box'
                }}
            >
                <SidebarNav
                    activeItemId={currentMenu}
                    onSelectItem={setCurrentMenu}
                    onLogout={onLogout}
                    isCollapsed={isCollapsed}
                    onToggleCollapse={() => setIsCollapsed(!isCollapsed)}
                />
            </aside>

            {/* 2. 중앙 비즈니스 컨텍스트 영역 (1fr이 남은 공간을 알아서 가득 채움) */}
            <main className="main-panel dashboard-panel" style={{
                padding: '0px',
                boxSizing: 'border-box',
                overflowY: 'auto',
                display: 'flex',
                flexDirection: 'column',
                height: '100%',
                minWidth: 0
            }}>
                {/* 상단 페이지 정보 영역 */}
                <div style={{
                    padding: '0.5rem 1.5rem',
                    borderBottom: '1px solid #e5e7eb',
                    backgroundColor: '#ffffff',
                    flexShrink: 0,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between'
                }}>
                    <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.35rem' }}>
                        <span style={{ fontSize: '0.75rem', color: '#9ca3af' }}>
                            {pageInfo.category} &gt;
                        </span>
                        <span style={{ fontSize: '0.85rem', fontWeight: '500', color: '#4b5563' }}>
                            {pageInfo.name}
                        </span>
                    </div>

                    {/* 💡 세션에 저장된 프로젝트 정보가 있을 때 상단 우측에 표시하여 경고 해결 */}
                    {projectInfo.id && (
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '1.0rem' }}>
                            <span style={{ backgroundColor: '#f3f4f6', padding: '2px 8px', borderRadius: '4px', color: '#374151' }}>
                                📌현재 프로젝트명: <b>{projectInfo.name}</b> 번호:({projectInfo.id})
                            </span>
                            {projectInfo.packLevel && (
                                <span style={{ backgroundColor: '#e0f2fe', color: '#0369a1', padding: '2px 6px', borderRadius: '4px', fontWeight: '500' }}>
                                    {projectInfo.packLevel}차
                                </span>
                            )}
                        </div>
                    )}
                </div>

                {/* 실제 동적 컨텐츠 영역 */}
                <div style={{ flex: 1, padding: '1.5rem', overflowY: 'auto', boxSizing: 'border-box' }}>
                    {renderBusinessContent()}
                </div>
            </main>

            {/* 3. 우측 AI 챗봇 패널 */}
            <aside className="dashboard-panel assistant-panel" style={{ height: '100%', boxSizing: 'border-box' }}>
                <AssistantPanel />
            </aside>
        </div>
    );
};

export default DashboardLayout;