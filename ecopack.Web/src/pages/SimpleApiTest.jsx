import { useState } from 'react';

export default function SimpleApiTest() {
    const [requestId, setRequestId] = useState(() => "test_req_" + Date.now());
    const [promptText, setPromptText] = useState('친환경 크라프트 골판지 박스');
    const [material, setMaterial] = useState('PLASTIC');

    const [ecoFix, setEcoFix] = useState(
        "SAFETY==• 아마존(Amazon) FFP 인증: 일반적인 운송 조건을 견딜 수 있는 포장 및 제품의 기능을 평가하기 위해 ISTA 6-Amazon.com (SIOC) 또는 Overboxing 시험 방법을 통과해야 인증을 부여합니다.\n" +
        "• 대만: 이커머스 배송 포장은 제품을 적절히 보호하되, 규정을 넘어선 과도한 완충이나 포장을 지양해야 합니다.; " +
        "SAFETY==• EU PPWR 제5조 4항: 포장재 내 중금속 4종의 농도 합이 100 mg/kg을 초과하는 경우 시장 출시가 엄격히 금지됩니다.\n" +
        "• 미국 독성포장감축법(TPCH): 제조업체 및 유통업체는 포장재 제조 시 4대 중금속의 의도적 첨가를 금지하며 우발적 농도를 100 ppm 이하로 제한합니다.\n" +
        "• 인도네시아 식약청(BPOM) 제20호 규정 : 플라스틱 및 종이 포장재의 중금속 잔류 및 용출을 100 ppm 이하로 엄격히 관리합니다; " +
        "REDUCE==• EU PPWR 제10조 (포장재 최소화): 포장재는 기능성(제품 보호 등) 보장"
    );

    const [image, setImage] = useState('');
    const [imageFileName, setImageFileName] = useState('');

    const [resultData, setResultData] = useState(null);
    const [loading, setLoading] = useState(false);

    const [jobIdInput, setJobIdInput] = useState('');
    const [jobStatusData, setJobStatusData] = useState(null);
    const [statusLoading, setStatusLoading] = useState(false);
    const [downloadLoading, setDownloadLoading] = useState(false);

    const handleImageFileChange = (e) => {
        const file = e.target.files[0];
        if (!file) return;

        setImageFileName(file.name);

        const reader = new FileReader();
        reader.onloadend = () => {
            setImage(reader.result);
        };
        reader.readAsDataURL(file);
    };

    const handleTestSubmit = async () => {
        try {
            setLoading(true);
            const response = await fetch('/api/v1/jobs', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    requestId: requestId,
                    prompt: promptText,
                    material: material,
                    ecoFix: ecoFix,
                    image: image
                })
            });

            const data = await response.json();
            setResultData(data);

            if (data?.job?.jobId) {
                setJobIdInput(data.job.jobId);
            }
        } catch (error) {
            console.error("API 호출 에러:", error);
            setResultData({ error: error.message });
        } finally {
            setLoading(false);
        }
    };

    const handleCheckStatus = async () => {
        if (!jobIdInput.trim()) {
            alert('조회할 jobId를 입력해주세요.');
            return;
        }

        try {
            setStatusLoading(true);
            const response = await fetch(`/api/v1/jobs/${jobIdInput.trim()}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                }
            });

            const text = await response.text();
            let data;
            try {
                data = text ? JSON.parse(text) : { message: "Empty response (204 No Content or Success)" };
            } catch {
                data = { rawText: text };
            }

            if (!response.ok) {
                setJobStatusData({ statusCode: response.status, ...data });
            } else {
                setJobStatusData(data);
            }
        } catch (error) {
            console.error("상태 조회 에러:", error);
            setJobStatusData({ error: error.message });
        } finally {
            setStatusLoading(false);
        }
    };

    const handleDownloadImage = async () => {
        if (!jobIdInput.trim()) {
            alert('다운로드할 jobId를 입력해주세요.');
            return;
        }

        try {
            setDownloadLoading(true);
            const response = await fetch(`/api/v1/jobs/${jobIdInput.trim()}/download`, {
                method: 'GET',
            });

            if (!response.ok) {
                const text = await response.text();
                throw new Error(text || `다운로드 실패 (상태 코드: ${response.status})`);
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${jobIdInput.trim()}_result.png`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error("이미지 다운로드 에러:", error);
            alert(`이미지 다운로드 중 오류가 발생했습니다: ${error.message}`);
        } finally {
            setDownloadLoading(false);
        }
    };

    return (
        <div style={{
            display: 'flex',
            gap: '12px',
            alignItems: 'flex-start',
            maxWidth: '1600px',
            margin: '0 auto',
            padding: '12px',
            fontFamily: 'sans-serif',
            fontSize: '12px',
            boxSizing: 'border-box'
        }}>
            <div style={{ flex: 1, minWidth: '0' }}>
                <h2 style={{ fontSize: '15px', margin: '0 0 10px 0' }}>🛠️ AI 작업 생성 API 실시간 테스트</h2>

                <div style={{ display: 'flex', gap: '8px', marginBottom: '8px' }}>
                    <div style={{ flex: 1 }}>
                        <label style={{ display: 'block', marginBottom: '2px', fontWeight: 'bold' }}>RequestId:</label>
                        <input
                            type="text"
                            value={requestId}
                            onChange={(e) => setRequestId(e.target.value)}
                            style={{ width: '100%', padding: '5px 8px', fontSize: '12px', boxSizing: 'border-box' }}
                        />
                    </div>
                    <div style={{ flex: 1 }}>
                        <label style={{ display: 'block', marginBottom: '2px', fontWeight: 'bold' }}>Material (소재):</label>
                        <input
                            type="text"
                            value={material}
                            onChange={(e) => setMaterial(e.target.value)}
                            style={{ width: '100%', padding: '5px 8px', fontSize: '12px', boxSizing: 'border-box' }}
                        />
                    </div>
                </div>

                <div style={{ marginBottom: '8px' }}>
                    <label style={{ display: 'block', marginBottom: '2px', fontWeight: 'bold' }}>Prompt (프롬프트):</label>
                    <input
                        type="text"
                        value={promptText}
                        onChange={(e) => setPromptText(e.target.value)}
                        style={{ width: '100%', padding: '5px 8px', fontSize: '12px', boxSizing: 'border-box' }}
                    />
                </div>

                <div style={{ marginBottom: '8px' }}>
                    <label style={{ display: 'block', marginBottom: '2px', fontWeight: 'bold', color: '#0056b3' }}>
                        EcoFix (복합 규격 및 긴 텍스트 입력 영역):
                    </label>
                    <textarea
                        value={ecoFix}
                        onChange={(e) => setEcoFix(e.target.value)}
                        rows={3}
                        style={{
                            width: '100%', padding: '6px 8px', fontSize: '11px', fontFamily: 'monospace',
                            lineHeight: '1.3', resize: 'vertical', border: '1px solid #ccc', borderRadius: '3px', boxSizing: 'border-box'
                        }}
                    />
                </div>

                <div style={{ marginBottom: '10px', padding: '8px', background: '#f9f9f9', border: '1px solid #ddd', borderRadius: '3px' }}>
                    <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
                        Image (이미지 파일 업로드 또는 Base64 직접 입력):
                    </label>

                    <input
                        type="file"
                        accept="image/*"
                        onChange={handleImageFileChange}
                        style={{ marginBottom: '4px', display: 'block', fontSize: '11px' }}
                    />
                    {imageFileName && <p style={{ fontSize: '11px', color: '#666', margin: '0 0 4px 0' }}>선택된 파일: {imageFileName}</p>}

                    <textarea
                        value={image}
                        onChange={(e) => setImage(e.target.value)}
                        placeholder="Base64 코드 또는 URL 입력"
                        rows={1}
                        style={{
                            width: '100%', padding: '5px 8px', fontSize: '11px', fontFamily: 'monospace',
                            resize: 'vertical', border: '1px solid #ccc', borderRadius: '3px', boxSizing: 'border-box'
                        }}
                    />

                    {image && (
                        <div style={{ marginTop: '4px' }}>
                            <span style={{ fontSize: '11px', fontWeight: 'bold' }}>미리보기:</span>
                            <div style={{ maxWidth: '80px', maxHeight: '80px', overflow: 'hidden', marginTop: '2px', border: '1px solid #ccc' }}>
                                <img src={image} alt="미리보기" style={{ width: '100%', height: 'auto', display: 'block' }} />
                            </div>
                        </div>
                    )}
                </div>

                <button
                    onClick={handleTestSubmit}
                    disabled={loading}
                    style={{ padding: '8px 12px', backgroundColor: '#007bff', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '3px', fontSize: '13px', width: '100%', fontWeight: 'bold', marginBottom: '12px' }}
                >
                    {loading ? '요청 전송 중...' : 'API 호출하기 (POST)'}
                </button>

                <div style={{ padding: '8px', background: '#eef6fc', border: '1px solid #bce8f1', borderRadius: '3px' }}>
                    <h3 style={{ margin: '0 0 6px 0', fontSize: '12px', color: '#31708f' }}>🔍 작업 상태 조회 및 결과 다운로드</h3>
                    <div style={{ display: 'flex', gap: '6px' }}>
                        <input
                            type="text"
                            value={jobIdInput}
                            onChange={(e) => setJobIdInput(e.target.value)}
                            placeholder="jobId 입력 (예: img-job-001)"
                            style={{ flex: 1, padding: '5px 8px', fontSize: '11px', boxSizing: 'border-box' }}
                        />
                        <button
                            onClick={handleCheckStatus}
                            disabled={statusLoading}
                            style={{ padding: '5px 10px', backgroundColor: '#5bc0de', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '3px', fontWeight: 'bold', fontSize: '11px' }}
                        >
                            {statusLoading ? '조회중' : '상태조회'}
                        </button>
                        <button
                            onClick={handleDownloadImage}
                            disabled={downloadLoading}
                            style={{ padding: '5px 10px', backgroundColor: '#28a745', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '3px', fontWeight: 'bold', fontSize: '11px' }}
                        >
                            {downloadLoading ? '다운중' : '이미지 다운로드'}
                        </button>
                    </div>
                </div>
            </div>

            <div style={{
                flex: 1,
                minWidth: '0',
                position: 'sticky',
                top: '12px',
                height: 'calc(100vh - 24px)',
                display: 'flex',
                flexDirection: 'column',
                background: '#f4f4f4',
                padding: '10px',
                borderRadius: '4px',
                boxSizing: 'border-box'
            }}>
                <h3 style={{ marginTop: 0, marginBottom: '4px', fontSize: '13px' }}>작업 생성 결과 (POST):</h3>
                <pre style={{
                    flex: 1,
                    margin: '0 0 10px 0',
                    whiteSpace: 'pre-wrap',
                    wordBreak: 'break-all',
                    overflowY: 'auto',
                    background: '#fff',
                    padding: '8px',
                    border: '1px solid #ddd',
                    borderRadius: '3px',
                    fontSize: '11px',
                    boxSizing: 'border-box'
                }}>
                    {resultData ? JSON.stringify(resultData, null, 2) : '아직 작업 생성 호출 전입니다.'}
                </pre>

                <h3 style={{ marginTop: 0, marginBottom: '4px', fontSize: '13px' }}>상태 조회 결과 (GET):</h3>
                <pre style={{
                    flex: 1,
                    margin: 0,
                    whiteSpace: 'pre-wrap',
                    wordBreak: 'break-all',
                    overflowY: 'auto',
                    background: '#fff',
                    padding: '8px',
                    border: '1px solid #ddd',
                    borderRadius: '3px',
                    fontSize: '11px',
                    boxSizing: 'border-box'
                }}>
                    {jobStatusData ? JSON.stringify(jobStatusData, null, 2) : '아직 상태 조회 전입니다.'}
                </pre>
            </div>
        </div>
    );
}