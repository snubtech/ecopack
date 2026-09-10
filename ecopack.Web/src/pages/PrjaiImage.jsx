import { useState, useEffect } from 'react';
import { getDesignTemplate, generate2DImage } from '../api/projects';

export default function PrjaiImage() {
    const [templateFileData, setTemplateFileData] = useState(null);

    const [aiImages] = useState([
        { label: 'H PRI', url: null },
        { label: 'MONO', url: null },
        { label: 'LIGHT', url: null }
    ]);

    const [glbImages] = useState([
        { label: 'H PRI', url: null },
        { label: 'MONO', url: null },
        { label: 'LIGHT', url: null }
    ]);

    // sessionStorage에서 필요한 정보들을 꺼내는 헬퍼 함수
    const getSessionData = () => {
        const prjId = sessionStorage.getItem('currentPrjId') || '';
        const packLevel = sessionStorage.getItem('currentPackLevel') || '1';
        const appliedMaterial = sessionStorage.getItem('currentMaterial') || ''; // 복원

        console.log('호출정보:', { prjId, packLevel, appliedMaterial });
        return { prjId, packLevel, appliedMaterial };
    };

    // 1. 디자인 템플릿 데이터 조회
    const fetchDesignTemplate = async () => {
        try {
            const { prjId, packLevel } = getSessionData();

            if (!prjId) {
                console.warn('프로젝트 번호(currentPrjId)가 없습니다.');
                return;
            }

            console.log('디자인 템플릿 조회 파라미터:', { prjId, packLevel });
            const response = await getDesignTemplate(prjId, packLevel);
            if (response && response.fileData) {
                setTemplateFileData(response.fileData);
            }
        } catch (error) {
            console.error('디자인 템플릿 조회 중 오류 발생:', error);
        }
    };

    // 2. 2D 이미지 생성 버튼 클릭 시
    const handleGenerate2D = async () => {
        try {
            const { prjId, packLevel, appliedMaterial } = getSessionData();

            if (!prjId || !appliedMaterial) {
                alert('프로젝트 번호 또는 적용 소재 정보가 부족합니다.');
                return;
            }

            const payload = {
                prjId,
                packLevel,
                appliedMaterial
            };
            console.log('2D 이미지 생성 파라미터:', { prjId, packLevel, appliedMaterial });
            const response = await generate2DImage(payload);
            if (response && response.success) {
                alert('AI 이미지 생성 요청 및 저장이 완료되었습니다.');
            }
        } catch (error) {
            console.error('2D AI 이미지 생성 요청 실패:', error);
            alert('AI 이미지 생성 요청 중 오류가 발생했습니다.');
        }
    };

    const handleGenerate3D = (item) => {
        console.log('3D 변환 요청 대상:', item.label);
    };

    const handleReloadAi = (item) => {
        console.log('AI 이미지 새로고침 대상:', item.label);
    };

    // 프로젝트 현황 버튼 클릭 시 핸들러
    const handleProjectStatus = () => {
        console.log('프로젝트 현황 버튼 클릭됨');
    };

    // 페이지 로드 시 기본 디자인 템플릿 자동 로드
    useEffect(() => {
        let isMounted = true;

        const initPage = async () => {
            const sessionData = getSessionData();

            if (sessionData.prjId) {
                try {
                    const response = await getDesignTemplate(sessionData.prjId, sessionData.packLevel);
                    if (isMounted && response && response.fileData) {
                        setTemplateFileData(response.fileData);
                    }
                } catch (error) {
                    if (isMounted) {
                        console.error('기본 디자인 템플릿 로드 실패:', error);
                    }
                }
            }
        };

        initPage();

        return () => {
            isMounted = false;
        };
    }, []);

    return (
        <div style={{
            display: 'grid',
            gridTemplateColumns: '1fr 1.25fr',
            gap: '1.2rem',
            padding: '1.2rem',
            boxSizing: 'border-box',
            height: '100%',
            backgroundColor: '#f9fafb',
            overflowY: 'auto',
            alignItems: 'start'
        }}>
            {/* [좌측] 3D 이미지 렌더링 뷰어 영역 */}
            <div style={{
                display: 'flex',
                flexDirection: 'column',
                backgroundColor: '#f3f4f6',
                borderRadius: '8px',
                border: '1px solid #e5e7eb',
                padding: '1rem',
                gap: '0.8rem'
            }}>
                <div style={{ fontWeight: '600', color: '#374151', fontSize: '0.9rem' }}>
                    3D 이미지 렌더링 뷰어
                </div>

                <div style={{
                    width: '100%',
                    height: '580px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: '#9ca3af',
                    border: '2px dashed #d1d5db',
                    borderRadius: '6px',
                    backgroundColor: '#ffffff'
                }}>
                    {/* 실제 3D 뷰어 컴포넌트가 들어갈 자리 */}
                </div>

                {/* 하단 툴바 (아이콘들만 배치) */}
                <div style={{
                    display: 'flex',
                    justifyContent: 'flex-start',
                    alignItems: 'center',
                    gap: '1.5rem',
                    padding: '0.5rem 1rem',
                    backgroundColor: '#1f2937',
                    borderRadius: '6px'
                }}>
                    <button title="회전/이동" style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1rem' }}>🌐</button>
                    <button title="선택" style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1rem' }}>👆</button>
                    <button title="확대/축소" style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1rem' }}>🔍</button>
                </div>

                {/* 뷰어 박스 우측 하단 바깥쪽에 녹색 스타일로 배치한 프로젝트현황 버튼 */}
                <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '0.2rem' }}>
                    <button onClick={handleProjectStatus} style={projectBtnStyle}>
                        프로젝트현황
                    </button>
                </div>
            </div>

            {/* [우측] 제어 및 결과 영역 */}
            <div style={{
                display: 'flex',
                flexDirection: 'column',
                gap: '1rem'
            }}>
                {/* 1. 기본디자인템플릿 카드 */}
                <div style={panelCardStyle}>
                    <div style={panelTitleStyle}>
                        기본디자인템플릿
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div style={{
                            width: '330px',
                            height: '130px',
                            backgroundColor: '#f3f4f6',
                            border: '1px dashed #d1d5db',
                            borderRadius: '6px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            overflow: 'hidden',
                            color: '#9ca3af',
                            fontSize: '0.85rem'
                        }}>
                            {/* {templateFileData ? <img src={templateFileData} alt="디자인 템플릿" style={{ width: '100%', height: '100%', objectFit: 'cover' }} /> : 'Image'} */}
                            {templateFileData ? (
                                <img
                                    src={templateFileData.startsWith('data:') ? templateFileData : `data:image/png;base64,${templateFileData}`}
                                    alt="디자인 템플릿"
                                    style={ { width: '100%', height: '100%', objectFit: 'contain' }}
                                />
                            ) : 'Image'}
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', width: '120px' }}>
                            <button onClick={handleGenerate2D} style={btnGreenStyle}>AI이미지생성</button>
                            <button onClick={fetchDesignTemplate} style={btnGreenStyle}>Reload</button>
                        </div>
                    </div>
                </div>

                {/* 2. AI 이미지 요청 이후 수신된 이미지 카드 */}
                <div style={panelCardStyle}>
                    <div style={panelTitleStyle}>
                        AI 이미지 요청 이후 수신된 이미지
                    </div>
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.6rem' }}>
                        {aiImages.map((item) => (
                            <div key={item.label} style={thumbCardStyle}>
                                <div style={{ fontSize: '0.75rem', fontWeight: '600', color: '#4b5563', textAlign: 'center', marginBottom: '0.3rem' }}>
                                    {item.label}
                                </div>
                                <div style={thumbImageBoxStyle}>
                                    {item.url ? <img src={item.url} alt={item.label} style={{ width: '100%', height: '100%', objectFit: 'cover' }} /> : 'Image'}
                                </div>
                                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem', width: '100%' }}>
                                    <button onClick={() => handleGenerate3D(item)} style={btnSmGreenStyle}>3D 이미지 생성</button>
                                    <button onClick={() => handleReloadAi(item)} style={btnSmGreenStyle}>Reload</button>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                {/* 3. 3D 변환 요청 이후 수신된 이미지 카드 */}
                <div style={panelCardStyle}>
                    <div style={panelTitleStyle}>
                        3D 변환 요청 이후 수신된 이미지
                    </div>
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.6rem' }}>
                        {glbImages.map((item) => (
                            <div key={item.label} style={thumbCardStyle}>
                                <div style={{ fontSize: '0.75rem', fontWeight: '600', color: '#4b5563', textAlign: 'center', marginBottom: '0.3rem' }}>
                                    {item.label}
                                </div>
                                <div style={thumbImageBoxStyle}>
                                    {item.url ? <img src={item.url} alt={item.label} style={{ width: '100%', height: '100%', objectFit: 'cover' }} /> : 'Image'}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}

// 스타일 헬퍼 객체들
const btnGreenStyle = {
    backgroundColor: '#10b981',
    color: '#ffffff',
    border: 'none',
    padding: '0.45rem 0.8rem',
    borderRadius: '4px',
    cursor: 'pointer',
    fontWeight: '500',
    fontSize: '0.8rem',
    textAlign: 'center',
    width: '100%'
};

const btnSmGreenStyle = {
    backgroundColor: '#10b981',
    color: '#ffffff',
    border: 'none',
    padding: '0.3rem 0.4rem',
    borderRadius: '4px',
    cursor: 'pointer',
    fontWeight: '500',
    fontSize: '0.7rem',
    textAlign: 'center',
    width: '100%'
};

const projectBtnStyle = {
    backgroundColor: '#10b981',
    color: '#ffffff',
    border: 'none',
    padding: '0.45rem 1rem',
    borderRadius: '4px',
    cursor: 'pointer',
    fontWeight: '500',
    fontSize: '0.8rem',
    textAlign: 'center'
};

const panelCardStyle = {
    backgroundColor: '#ffffff',
    border: '1px solid #e5e7eb',
    borderRadius: '8px',
    padding: '0.8rem 1rem',
    boxShadow: '0 1px 2px rgba(0,0,0,0.04)'
};

const panelTitleStyle = {
    fontSize: '0.95rem',
    fontWeight: '700',
    color: '#1f2937',
    marginBottom: '0.6rem'
};

const thumbCardStyle = {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    backgroundColor: '#f9fafb',
    border: '1px solid #e5e7eb',
    borderRadius: '6px',
    padding: '0.5rem'
};

const thumbImageBoxStyle = {
    width: '100%',
    height: '140px',
    backgroundColor: '#f3f4f6',
    border: '1px dashed #d1d5db',
    borderRadius: '4px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    overflow: 'hidden',
    color: '#9ca3af',
    fontSize: '0.75rem',
    marginBottom: '0.5rem'
};