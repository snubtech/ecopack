import { useState, useEffect, useRef } from 'react';
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { getDesignTemplate, generate2DImage, getAiJobStatus, createGlbJob, getGlbJobStatus } from '../api/projects';

// 3D 모델(GLB) 렌더링용 재사용 컴포넌트
function ThreeDViewer({ glbUrl, enableControls = true, height = '100%', fitScale = 2.5, cameraZ = 5, yOffset = 0 }) {
    const mountRef = useRef(null);

    useEffect(() => {
        const currentMount = mountRef.current;
        if (!currentMount || !glbUrl) return;

        const width = currentMount.clientWidth;
        const currentHeight = currentMount.clientHeight;

        const scene = new THREE.Scene();
        scene.background = new THREE.Color(0xffffff);

        const camera = new THREE.PerspectiveCamera(45, width / currentHeight, 0.1, 1000);
        camera.position.set(0, 1.5, cameraZ);

        const renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setSize(width, currentHeight);
        renderer.setPixelRatio(window.devicePixelRatio);
        currentMount.appendChild(renderer.domElement);

        const ambientLight = new THREE.AmbientLight(0xffffff, 1.2);
        scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 1.5);
        directionalLight.position.set(5, 10, 7);
        scene.add(directionalLight);

        let controls;
        if (enableControls) {
            controls = new OrbitControls(camera, renderer.domElement);
            controls.enableDamping = true;
        }

        let loadedModel = null;

        const loader = new GLTFLoader();
        loader.load(
            glbUrl,
            (gltf) => {
                loadedModel = gltf.scene;

                const box = new THREE.Box3().setFromObject(loadedModel);
                const center = box.getCenter(new THREE.Vector3());
                const size = box.getSize(new THREE.Vector3());

                const maxDim = Math.max(size.x, size.y, size.z);
                const scale = fitScale / maxDim;
                loadedModel.scale.set(scale, scale, scale);

                loadedModel.position.x = -center.x * scale;
                loadedModel.position.y = (-center.y * scale) + (size.y * scale * yOffset);
                loadedModel.position.z = -center.z * scale;

                scene.add(loadedModel);
            },
            undefined,
            (error) => {
                console.error('GLB 모델 로드 중 오류 발생:', error);
            }
        );

        let animationFrameId;
        const animate = () => {
            animationFrameId = requestAnimationFrame(animate);
            if (controls) controls.update();
            renderer.render(scene, camera);
        };
        animate();

        const handleResize = () => {
            if (!currentMount) return;
            const newWidth = currentMount.clientWidth;
            const newHeight = currentMount.clientHeight;
            camera.aspect = newWidth / newHeight;
            camera.updateProjectionMatrix();
            renderer.setSize(newWidth, newHeight);
        };
        window.addEventListener('resize', handleResize);

        return () => {
            window.removeEventListener('resize', handleResize);
            cancelAnimationFrame(animationFrameId);
            if (loadedModel) {
                scene.remove(loadedModel);
            }
            renderer.dispose();
            if (currentMount.contains(renderer.domElement)) {
                currentMount.removeChild(renderer.domElement);
            }
        };
    }, [glbUrl, enableControls, fitScale, cameraZ, yOffset]);

    return (
        <div
            ref={mountRef}
            style={{ width: '100%', height: height, borderRadius: '4px', overflow: 'hidden' }}
        />
    );
}

export default function PrjaiImage() {
    // 💡 1. 로딩 상태 선언
    const [isLoading, setIsLoading] = useState(false);
    const [loadingMessage, setLoadingMessage] = useState('데이터를 불러오는 중입니다...');

    const [templateFileData, setTemplateFileData] = useState(null);

    const [aiImages, setAiImages] = useState([
        { label: 'H PRI', url: null },
        { label: 'MONO', url: null },
        { label: 'LIGHT', url: null }
    ]);

    const [glbImages, setGlbImages] = useState([
        { label: 'H PRI', url: null, jobId: '', progress: 0, reqDate: '-' },
        { label: 'MONO', url: null, jobId: '', progress: 0, reqDate: '-' },
        { label: 'LIGHT', url: null, jobId: '', progress: 0, reqDate: '-' }
    ]);

    const [currentGlbUrl, setCurrentGlbUrl] = useState(null);

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

    const getSessionData = () => {
        const prjId = sessionStorage.getItem('currentPrjId') || '';
        const packLevel = sessionStorage.getItem('currentPackLevel') || '1';
        const appliedMaterial = sessionStorage.getItem('currentMaterial') || '';
        return { prjId, packLevel, appliedMaterial };
    };

    const applyServerResponse = (response) => {
        if (!response) return;
        applyAiStatusResponse(response);

        if (response.fileData) {
            setTemplateFileData(formatBase64(response.fileData));
        }
    };

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

        if (response.resultDataOriginal || response.resultDataModerate || response.resultDataRedesign) {
            setAiImages([
                { label: 'H PRI', url: formatBase64(response.resultDataOriginal) },
                { label: 'MONO', url: formatBase64(response.resultDataModerate) },
                { label: 'LIGHT', url: formatBase64(response.resultDataRedesign) }
            ]);
        }

        const sharedJobId = response.glbJobId || '';
        const sharedDate = response.req3ddate || '-';

        const newGlbList = [
            {
                label: 'H PRI',
                url: formatBase64(response.resultDataGlbOriginal),
                jobId: response.glbJobIdOriginal || sharedJobId,
                progress: response.progressoriginal,
                reqDate: response.req3ddateoriginal || sharedDate
            },
            {
                label: 'MONO',
                url: formatBase64(response.resultDataGlbModerate),
                jobId: response.glbJobIdModerate || sharedJobId,
                progress: response.progressmoderate,
                reqDate: response.req3ddatemoderate || sharedDate
            },
            {
                label: 'LIGHT',
                url: formatBase64(response.resultDataGlbRedesign),
                jobId: response.glbJobIdRedesign || sharedJobId,
                progress: response.progressredesign,
                reqDate: response.req3ddateredesign || sharedDate
            }
        ];
        setGlbImages(newGlbList);
        if (newGlbList[0].url && !currentGlbUrl) {
            setCurrentGlbUrl(newGlbList[0].url);
        }
    };

    const formatBase64 = (data) => {
        if (!data) return null;
        if (data.startsWith('data:') || data.startsWith('blob:') || data.startsWith('http')) {
            return data;
        }
        return `data:application/octet-stream;base64,${data}`;
    };

    const handleCheckAiStatus = async () => {
        setLoadingMessage('AI 작업 상태를 조회하는 중입니다...');
        setIsLoading(true);
        try {
            const { prjId, packLevel } = getSessionData();
            if (!prjId) return;
            const response = await getAiJobStatus(prjId, packLevel);
            applyAiStatusResponse(response);
        } catch (error) {
            console.error('AI 작업 상태 조회 중 오류 발생:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleGenerate2D = async () => {
        setLoadingMessage('AI 2D 이미지를 생성하는 중입니다...');
        setIsLoading(true);
        try {
            const { prjId, packLevel, appliedMaterial } = getSessionData();
            if (!prjId || !appliedMaterial) {
                alert('프로젝트 번호 또는 적용 소재 정보가 부족합니다.');
                return;
            }
            const payload = { prjId, packLevel, appliedMaterial };
            await generate2DImage(payload);
            alert('AI 이미지 생성 작업 요청 및 저장이 완료되었습니다.');
        } catch (error) {
            console.error('2D AI 이미지 생성 요청 실패:', error);
            alert('AI 이미지 생성 요청 중 오류가 발생했습니다.');
        } finally {
            setIsLoading(false);
        }
    };

    const handleGenerate3D = async (item) => {
        setLoadingMessage(`[${item.label}] 3D 변환을 요청하는 중입니다...`);
        setIsLoading(true);
        try {
            const { prjId, packLevel } = getSessionData();
            if (!item.url) {
                alert("전송할 이미지 데이터가 없습니다.");
                return;
            }
            const base64Data = item.url.includes('base64,')
                ? item.url.split('base64,')[1]
                : item.url;

            const payload = { prjId: prjId, packLevel: packLevel, image: base64Data, imagelabal: item.label };
            const response = await createGlbJob(payload);

            if (response && response.success) {
                alert(`[${item.label}] 3D 변환 작업이 시작되었습니다. (Job ID: ${response.jobId || response.job?.jobId || '-'})`);
            } else {
                alert(`[${item.label}] 3D 변환 요청이 접수되었습니다.`);
            }
        } catch (error) {
            console.error("3D 변환 요청 실패:", error);
            alert("3D 변환 요청 중 오류가 발생했습니다.");
        } finally {
            setIsLoading(false);
        }
    };

    const handle3dstatusReload = async (item) => {
        //setLoadingMessage(`[${item.label}] 3D 상태를 조회하는 중입니다...`);
        setIsLoading(true);
        try {
            const { prjId, packLevel } = getSessionData();
            if (!prjId || !packLevel) {
                alert("프로젝트 정보 또는 패키지 레벨이 누락되었습니다.");
                return;
            }
            var itemLabel = item.label;
            const response = await getGlbJobStatus(prjId, packLevel, itemLabel);

            const data = response;

            if (data && data.success) {
                setJobStatus(prev => ({
                    ...prev,
                    status3d: data.status,
                    progress3d: data.progress,
                    statusMessage3d: data.message
                }));

                setGlbImages(prevImages => prevImages.map(img => {
                    if (img.label === item.label) {
                        let targetUrl = img.url;
                        let targetJobId = img.jobId;
                        let targetProgress = img.progress;
                        let targetDate = img.reqDate;

                        if (item.label === 'H PRI') {
                            if (data.resultDataGlbOriginal) targetUrl = formatBase64(data.resultDataGlbOriginal);
                            targetJobId = data.glbJobIdOriginal;
                            targetProgress = data.progressoriginal !== undefined ? data.progressoriginal : (data.progress || img.progress);
                            targetDate = data.req3ddateoriginal || data.req3ddate || img.reqDate;
                        } else if (item.label === 'MONO') {
                            if (data.resultDataGlbModerate) targetUrl = formatBase64(data.resultDataGlbModerate);
                            targetJobId = data.glbJobIdModerate;
                            targetProgress = data.progressmoderate !== undefined ? data.progressmoderate : (data.progress || img.progress);
                            targetDate = data.req3ddatemoderate || data.req3ddate || img.reqDate;
                        } else if (item.label === 'LIGHT') {
                            if (data.resultDataGlbRedesign) targetUrl = formatBase64(data.resultDataGlbRedesign);
                            targetJobId = data.glbJobIdRedesign;
                            targetProgress = data.progressredesign !== undefined ? data.progressredesign : (data.progress || img.progress);
                            targetDate = data.req3ddateredesign || data.req3ddate || img.reqDate;
                        }

                        if (targetUrl) {
                            setCurrentGlbUrl(targetUrl);
                        }

                        return {
                            ...img,
                            url: targetUrl,
                            jobId: targetJobId,
                            progress: targetProgress,
                            reqDate: targetDate
                        };
                    }
                    return img;
                }));

               // alert(`[${item.label}] 3D 상태 조회 완료 (상태: ${data.status})`);
            } else {
                alert(data?.message || "3D 작업 상태를 불러오지 못했습니다.");
            }
        } catch (error) {
            console.error("3D 상태 조회 중 오류 발생:", error);
            alert("3D 작업 상태 조회 통신 중 오류가 발생했습니다.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        let isMounted = true;
        const initPage = async () => {
            const sessionData = getSessionData();
            if (sessionData.prjId) {
                setLoadingMessage('저장된 기본본디자인,AI추천이미지,3D변환이미지를 불러오는 중입니다...');
                setIsLoading(true);
                try {
                    const response = await getDesignTemplate(sessionData.prjId, sessionData.packLevel);
                    if (isMounted && response) {
                        applyServerResponse(response);
                    }
                } catch (error) {
                    if (isMounted) console.error('기본 디자인 로드 실패:', error);
                } finally {
                    if (isMounted) setIsLoading(false);
                }
            }
        };
        initPage();
        return () => { isMounted = false; };
    }, []);

    return (
        // 💡 2. 가장 바깥쪽 부모 div에 position: 'relative'와 minHeight 추가
        <div style={{
            position: 'relative',
            minHeight: '100%',
            boxSizing: 'border-box',
            display: 'grid',
            gridTemplateColumns: '1fr 1.25fr',
            gap: '1.2rem',
            padding: '1.2rem',
            backgroundColor: '#f9fafb',
            overflowY: 'auto',
            alignItems: 'start'
        }}>

            {/* 💡 3. 로딩 오버레이 컴포넌트 삽입 */}
            {isLoading && (
                <div style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0,
                    backgroundColor: 'rgba(0, 0, 0, 0.4)',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    zIndex: 9999,
                    color: '#ffffff'
                }}>
                    <div style={{ marginBottom: '1rem', fontSize: '1.5rem' }}>⏳</div>
                    <div style={{ fontWeight: 'bold' }}>{loadingMessage}</div>
                </div>
            )}

            {/* 💡 4. 기존 페이지 콘텐츠 영역 */}
            {/* [좌측] 메인 3D 이미지 렌더링 뷰어 영역 */}
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
                    3D 이미지 렌더링 뷰어 {currentGlbUrl ? '' : '(대기 중)'}
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
                    backgroundColor: '#ffffff',
                    position: 'relative',
                    overflow: 'hidden'
                }}>
                    {currentGlbUrl ? (
                        <ThreeDViewer
                            glbUrl={currentGlbUrl}
                            enableControls={true}
                            height="100%"
                            fitScale={2.5}
                            cameraZ={5}
                            yOffset={0}
                        />
                    ) : (
                        <div style={{ textAlign: 'center' }}>
                            <p>3D 모델 데이터가 없습니다.</p>
                            <p style={{ fontSize: '0.75rem' }}>하단 카드에서 3D 변환을 진행해주세요.</p>
                        </div>
                    )}
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
                    <button onClick={() => console.log('프로젝트현황 클릭')} style={projectBtnStyle}>
                        프로젝트현황
                    </button>
                </div>
            </div>

            {/* [우측] 제어 및 결과 영역 */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                {/* 1. 기본디자인템플릿 카드 */}
                <div style={panelCardStyle}>
                    <div style={panelTitleStyle}>기본디자인템플릿</div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div style={{
                            width: '330px', height: '130px', backgroundColor: '#f3f4f6',
                            border: '1px dashed #d1d5db', borderRadius: '6px', display: 'flex',
                            alignItems: 'center', justifyContent: 'center', overflow: 'hidden', color: '#9ca3af', fontSize: '0.85rem'
                        }}>
                            {templateFileData ? (
                                <img src={templateFileData} alt="디자인 템플릿" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
                            ) : 'Image'}
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', width: '130px' }}>
                            <button onClick={handleGenerate2D} style={btnGreenStyle}>AI이미지(2D)생성</button>
                            <button onClick={handleCheckAiStatus} style={btnGreenStyle}>요청상태확인</button>
                        </div>
                    </div>
                </div>

                {/* 2. AI 이미지 요청 이후 수신된 이미지 카드 (2D) */}
                <div style={panelCardStyle}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.6rem' }}>
                        <div style={panelTitleStyle}>AI 이미지 요청 이후 수신된 이미지</div>
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
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                {/* 3. 3D 변환 요청 이후 수신된 모델 카드 */}
                <div style={panelCardStyle}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.6rem' }}>
                        <div style={panelTitleStyle}>3D 변환 요청 이후 수신된 이미지</div>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.6rem' }}>
                        {glbImages.map((item) => (
                            <div
                                key={item.label}
                                style={{
                                    ...thumbCardStyle,
                                    border: currentGlbUrl === item.url ? '2px solid #10b981' : '1px solid #e5e7eb',
                                    cursor: item.url ? 'pointer' : 'default',
                                    padding: '0.4rem'
                                }}
                                onClick={() => {
                                    if (item.url) setCurrentGlbUrl(item.url);
                                }}>

                                <div style={{
                                    display: 'flex',
                                    justifyContent: 'space-between',
                                    alignItems: 'center',
                                    width: '100%',
                                    marginBottom: '0.3rem'
                                }}>
                                    <div style={{ fontSize: '0.75rem', fontWeight: '600', color: '#4b5563', display: 'flex', alignItems: 'center', gap: '4px' }}>
                                        <span>{item.label}</span>
                                        {currentGlbUrl === item.url && <span style={{ fontSize: '0.7rem' }}>📌</span>}
                                    </div>
                                    <button
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handle3dstatusReload(item);
                                        }}
                                        style={{ ...btnSmGreenStyle, width: 'auto', padding: '0.2rem 0.5rem', fontSize: '0.65rem', margin: 0 }}
                                    >
                                        Reload
                                    </button>
                                </div>

                                <div style={{
                                    width: '100%',
                                    backgroundColor: '#f8fafc',
                                    border: '1px solid #e2e8f0',
                                    borderRadius: '4px',
                                    padding: '0.25rem 0.4rem',
                                    fontSize: '0.65rem',
                                    color: '#334151',
                                    marginBottom: '0.4rem',
                                    display: 'flex',
                                    flexDirection: 'column',
                                    gap: '0.1rem',
                                    boxSizing: 'border-box'
                                }}>
                                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                        <span>진행: <b>{item.progress}%</b></span>
                                        <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: '85px' }} title={item.jobId}>
                                            ID: <b>{item.jobId || '-'}</b>
                                        </span>
                                    </div>
                                    <div style={{ fontSize: '0.6rem', color: '#64748b', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                        시간: {item.reqDate || '-'}
                                    </div>
                                </div>

                                <div style={thumbImageBoxStyle}>
                                    {item.url ? (
                                        <ThreeDViewer
                                            glbUrl={item.url}
                                            enableControls={false}
                                            height="115px"
                                            fitScale={3}
                                            cameraZ={6}
                                            yOffset={0.5}
                                        />
                                    ) : (
                                        <span>3D 대기 중</span>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

        </div> // <-- 가장 바깥쪽 부모 div 닫기
    );
}

const statusBadgeBoxStyle = {
    display: 'flex', flexDirection: 'column', alignItems: 'flex-end',
    backgroundColor: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: '4px',
    padding: '0.3rem 0.6rem', fontSize: '0.75rem', color: '#334151', gap: '0.1rem', width: '400px'
};

const btnGreenStyle = {
    backgroundColor: '#10b981', color: '#ffffff', border: 'none',
    padding: '0.45rem 0.8rem', borderRadius: '4px', cursor: 'pointer',
    fontWeight: '500', fontSize: '0.8rem', textAlign: 'center', width: '100%'
};

const btnSmGreenStyle = {
    backgroundColor: '#10b981', color: '#ffffff', border: 'none',
    padding: '0.3rem 0.4rem', borderRadius: '4px', cursor: 'pointer',
    fontWeight: '500', fontSize: '0.7rem', textAlign: 'center', width: '100%'
};

const projectBtnStyle = {
    backgroundColor: '#10b981', color: '#ffffff', border: 'none',
    padding: '0.45rem 1rem', borderRadius: '4px', cursor: 'pointer',
    fontWeight: '500', fontSize: '0.8rem', textAlign: 'center'
};

const panelCardStyle = {
    backgroundColor: '#ffffff', border: '1px solid #e5e7eb',
    borderRadius: '8px', padding: '0.8rem 1rem', boxShadow: '0 1px 2px rgba(0,0,0,0.04)'
};

const panelTitleStyle = {
    fontSize: '0.95rem', fontWeight: '700', color: '#1f2937', marginBottom: '0', paddingTop: '0.2rem'
};

const thumbCardStyle = {
    display: 'flex', flexDirection: 'column', alignItems: 'center',
    backgroundColor: '#f9fafb', border: '1px solid #e5e7eb', borderRadius: '6px', padding: '0.5rem'
};

const thumbImageBoxStyle = {
    width: '100%', height: '140px', backgroundColor: '#f3f4f6',
    border: '1px dashed #d1d5db', borderRadius: '4px', display: 'flex',
    alignItems: 'center', justifyContent: 'center', overflow: 'hidden',
    color: '#9ca3af', fontSize: '0.75rem', marginBottom: '0.5rem',
    padding: '2px', boxSizing: 'border-box'
};