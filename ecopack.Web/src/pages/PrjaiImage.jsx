import { useState, useEffect } from 'react';
import { getDesignTemplate, generate2DImage, getAiJobStatus } from '../api/projects'; // 💡 getAiJobStatus 임포트 추가

export default function PrjaiImage() {
    const [templateFileData, setTemplateFileData] = useState(null);

    // 2D 이미지 3종 상태 관리 (H PRI, MONO, LIGHT)
    const [aiImages, setAiImages] = useState([
        { label: 'H PRI', url: null },
        { label: 'MONO', url: null },
        { label: 'LIGHT', url: null }
    ]);

    // 3D GLB 이미지 3종 상태 관리 (H PRI, MONO, LIGHT)
    const [glbImages, setGlbImages] = useState([
        { label: 'H PRI', url: null },
        { label: 'MONO', url: null },
        { label: 'LIGHT', url: null }
    ]);

    // 2D 및 3D 작업 전체 상태 관리용 state (상태, 진행률, 메시지 등)
    const [jobStatus, setJobStatus] = useState({
        existsInAiImages: false,
        jobId: '',
        status: 'NONE',
        progress: 0,
        isSuccess: false,
        statusMessage: '대기 중',

        glbJobId: '',
        status3d: 'NONE',
        progress3d: 0,
        isSuccess3d: false,
        statusMessage3d: '대기 중'
    });

    // sessionStorage에서 필요한 정보들을 꺼내는 헬퍼 함수
    const getSessionData = () => {
        const prjId = sessionStorage.getItem('currentPrjId') || '';
        const packLevel = sessionStorage.getItem('currentPackLevel') || '1';
        const appliedMaterial = sessionStorage.getItem('currentMaterial') || '';

        console.log('호출정보:', { prjId, packLevel, appliedMaterial });
        return { prjId, packLevel, appliedMaterial };
    };

    // 서버 응답 데이터를 받아서 상태(State)에 일괄 반영하는 공통 함수 (기본 템플릿용)
    const applyServerResponse = (response) => {
        if (!response) return;

        applyAiStatusResponse(response);

        // fileData가 존재하면 기본 템플릿에 항상 렌더링
        if (response.fileData) {
            setTemplateFileData(formatBase64(response.fileData));
        }

        if (response.existsInAiImages) {
            // 1. 2D 결과 데이터 매핑
            setAiImages([
                { label: 'H PRI', url: formatBase64(response.resultDataOriginal) },
                { label: 'MONO', url: formatBase64(response.resultDataModerate) },
                { label: 'LIGHT', url: formatBase64(response.resultDataRedesign) }
            ]);

            // 2. 3D GLB 결과 데이터 매핑
            setGlbImages([
                { label: 'H PRI', url: formatBase64(response.resultDataGlbOriginal) },
                { label: 'MONO', url: formatBase64(response.resultDataGlbModerate) },
                { label: 'LIGHT', url: formatBase64(response.resultDataGlbRedesign) }
            ]);
        }
    };

    // 💡 AI 작업 상태 응답만 별도로 갱신하는 공통 함수 정의
    const applyAiStatusResponse = (response) => {
        if (!response) return;

        setJobStatus({
            existsInAiImages: response.existsInAiImages || false,
            jobId: response.jobId || '',
            status: response.status || 'NONE',
            progress: response.progress || 0,
            isSuccess: response.isSuccess || false,
            statusMessage: response.statusMessage || '대기 중',
            req2ddate: response.req2ddate || null,
            req3ddate: response.req3ddate || null,

            glbJobId: response.glbJobId || '',
            status3d: response.status3d || 'NONE',
            progress3d: response.progress3d || 0,
            isSuccess3d: response.isSuccess3d || false,
            statusMessage3d: response.statusMessage3d || '대기 중'
        });

        // 💡 백엔드에서 전달된 2D 이미지 3종 Base64 데이터가 존재하면 즉시 썸네일 배열에 반영
        if (response.resultDataOriginal || response.resultDataModerate || response.resultDataRedesign) {
            setAiImages([
                { label: 'H PRI', url: formatBase64(response.resultDataOriginal) },
                { label: 'MONO', url: formatBase64(response.resultDataModerate) },
                { label: 'LIGHT', url: formatBase64(response.resultDataRedesign) }
            ]);
        }

        if (response.resultDataGlbOriginal || response.resultDataGlbModerate || response.resultDataGlbRedesign) {
            setGlbImages([
                { label: 'H PRI', url: formatBase64(response.resultDataGlbOriginal) },
                { label: 'MONO', url: formatBase64(response.resultDataGlbModerate) },
                { label: 'LIGHT', url: formatBase64(response.resultDataGlbRedesign) }
            ]);
        }
    };

    // Base64 포맷터 헬퍼 함수
    const formatBase64 = (data) => {
        if (!data) return null;
        return data.startsWith('data:') ? data : `data:image/png;base64,${data}`;
    };

    // 💡 AI 작업 진행 상태만 단독으로 확인하는 함수 (중복 제거 완료)
    const handleCheckAiStatus = async () => {
        try {
            const { prjId, packLevel } = getSessionData();

            if (!prjId) {
                console.warn('프로젝트 번호(currentPrjId)가 없습니다.');
                return;
            }

            const response = await getAiJobStatus(prjId, packLevel);
            applyAiStatusResponse(response);
            console.log('AI 작업 상태 조회 응답:', response);
        } catch (error) {
            console.error('AI 작업 상태 조회 중 오류 발생:', error);
        }
    };

    // 2D 이미지 생성 버튼 클릭 시
    const handleGenerate2D = async () => {
        try {
            const { prjId, packLevel, appliedMaterial } = getSessionData();

            if (!prjId || !appliedMaterial) {
                alert('프로젝트 번호 또는 적용 소재 정보가 부족합니다.');
                return;
            }

            const payload = { prjId, packLevel, appliedMaterial };
            const response = await generate2DImage(payload);

            if (response) {
                alert('AI 이미지 생성 작업 요청 및 저장이 완료되었습니다.');
                // fetchDesignTemplate 함수가 정의되어 있지 않다면 아래 호출 부를 적절히 수정하거나 정의해야 합니다.
                // 현재 코드 기준 초기화 로직이나 상태 조회 등을 연동할 수 있습니다.
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
        console.log('AI 이미지 개별 새로고침 대상:', item.label);
        handleCheckAiStatus();
    };

    const handleProjectStatus = () => {
        console.log('프로젝트 현황 버튼 클릭됨');
    };

    useEffect(() => {
        let isMounted = true;

        const initPage = async () => {
            const sessionData = getSessionData();
            if (sessionData.prjId) {
                try {
                    const response = await getDesignTemplate(sessionData.prjId, sessionData.packLevel);
                    if (isMounted && response) {
                        applyServerResponse(response);
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
                            {templateFileData ? (
                                <img
                                    src={templateFileData}
                                    alt="디자인 템플릿"
                                    style={{ width: '100%', height: '100%', objectFit: 'contain' }}
                                />
                            ) : 'Image'}
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', width: '130px' }}>
                            <button onClick={handleGenerate2D} style={btnGreenStyle}>AI이미지(2D)생성</button>
                            <button onClick={handleCheckAiStatus} style={btnGreenStyle}>요청상태확인</button>
                        </div>
                    </div>
                </div>

                {/* 2. AI 이미지 요청 이후 수신된 이미지 카드 */}
                <div style={panelCardStyle}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.6rem' }}>
                        <div style={panelTitleStyle}>AI 이미지 요청 이후 수신된 이미지</div>
                        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>                            
                            <div style={statusBadgeBoxStyle}>
                                <div style={{ display: 'flex', gap: '15px' }}>
                                    <span>상태: <b>{jobStatus.status}</b></span>
                                    <span>진행률: <b>{jobStatus.progress}%</b></span>
                                    <span>생성시간: <b>{jobStatus.req2ddate}</b></span>
                                </div>
                                <div style={{ fontSize: '0.7rem', color: '#6b7280', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', width: '100%', textAlign: 'right' }}>
                                    메시지: {jobStatus.statusMessage || '-'} (Job ID: {jobStatus.jobId || '없음'})
                                </div>
                            </div>
                        </div>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.6rem' }}>
                        {aiImages.map((item) => (
                            <div key={item.label} style={thumbCardStyle}>
                                <div style={{ fontSize: '0.75rem', fontWeight: '600', color: '#4b5563', textAlign: 'center', marginBottom: '0.3rem' }}>
                                    {item.label}
                                </div>
                                <div style={thumbImageBoxStyle}>
                                    {item.url ? (
                                        <img src={item.url} alt={item.label} style={{ width: '100%', height: '100%', objectFit: 'contain', maxHeight: '95%' }} />
                                    ) : 'Image'}
                                </div>
                                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem', width: '100%' }}>
                                    <button onClick={() => handleGenerate3D(item)} style={btnSmGreenStyle}>3D 이미지 생성</button>
                                    <button onClick={() => handleReloadAi(item)} style={btnSmGreenStyle}>요청상태확인</button>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                {/* 3. 3D 변환 요청 이후 수신된 이미지 카드 */}
                <div style={panelCardStyle}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.6rem' }}>
                        <div style={panelTitleStyle}>3D 변환 요청 이후 수신된 이미지</div>
                        <div style={statusBadgeBoxStyle}>
                            <div style={{ display: 'flex', gap: '15px' }}>
                                <span>상태: <b>{jobStatus.status3d}</b></span>
                                <span>진행률: <b>{jobStatus.progress3d}%</b></span>
                                <span>생성시간: <b>{jobStatus.req3ddate}</b></span>
                            </div>
                            <div style={{ fontSize: '0.7rem', color: '#6b7280', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', width: '100%', textAlign: 'right' }}>
                                메시지: {jobStatus.statusMessage3d || '-'} (Job ID: {jobStatus.glbJobId || '없음'})
                            </div>
                        </div>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.6rem' }}>
                        {glbImages.map((item) => (
                            <div key={item.label} style={thumbCardStyle}>
                                <div style={{ fontSize: '0.75rem', fontWeight: '600', color: '#4b5563', textAlign: 'center', marginBottom: '0.3rem' }}>
                                    {item.label}
                                </div>
                                <div style={thumbImageBoxStyle}>
                                    {item.url ? (
                                        <img src={item.url} alt={item.label} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                                    ) : 'Image'}
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
const statusBadgeBoxStyle = {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'flex-end',
    backgroundColor: '#f8fafc',
    border: '1px solid #e2e8f0',
    borderRadius: '4px',
    padding: '0.3rem 0.6rem',
    fontSize: '0.75rem',
    color: '#334151',
    gap: '0.1rem',
    width: '400px'
};

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

//const btnSmGreenStyleWidth = {
//    backgroundColor: '#10b981',
//    color: '#ffffff',
//    border: 'none',
//    padding: '0.3rem 0.6rem',
//    borderRadius: '4px',
//    cursor: 'pointer',
//    fontWeight: '500',
//    fontSize: '0.75rem',
//    height: '100%',
//    textAlign: 'center'
//};

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
    marginBottom: '0',
    paddingTop: '0.2rem'
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
    marginBottom: '0.5rem',
    padding: '5px',          
    boxSizing: 'border-box'  
};