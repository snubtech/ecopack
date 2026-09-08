import { useState, useEffect } from 'react';
import axios from 'axios';

const Prjdefaultresult = ({ prjId, packLevel }) => {
    const [materials, setMaterials] = useState([]);
    const [environments, setEnvironments] = useState([]);
    const [processFlows, setProcessFlows] = useState([]);
    const [carconInfo, setCarconInfo] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);

                const appliedMaterial = sessionStorage.getItem('currentMaterial') || '';
                const matType = sessionStorage.getItem('currentMatType') || '';
                const matform = sessionStorage.getItem('currentMatForm') || '';
                const currentExportCountry = sessionStorage.getItem('currentExportCountry') || '';


                // 1. 물성 정보 조회
                const matRes = await axios.get('/api/Projects/Getmaterial', {
                    params: { prjId, packLevel, appliedMaterial, matType }
                });
                setMaterials(matRes.data || []);

                // 2. 환경규제 정보 조회
                const envRes = await axios.get('/api/Projects/Getenvironment', {
                    params: { prjId, packLevel, appliedMaterial, exportCountry: currentExportCountry }
                });
                setEnvironments(envRes.data || []);

                // 3. 공정도 정보 조회
                const procRes = await axios.get('/api/Projects/Getprocessflow', {
                    params: { prjId, appliedMaterial, matType }
                });
                setProcessFlows(procRes.data || []);

                // 4. 탄소배출량 정보 조회
                const carRes = await axios.get('/api/Projects/Getcarconinfo', {
                    params: { prjId, packLevel, appliedMaterial, matform }
                });
                setCarconInfo(carRes.data || null);

            } catch (error) {
                console.error("데이터 조회 중 오류 발생:", error);
            } finally {
                setLoading(false);
            }
        };

        if (prjId && packLevel) {
            fetchData();
        }
    }, [prjId, packLevel]);

    if (loading) {
        return <div style={{ padding: '2rem', textAlign: 'center' }}>데이터를 불러오는 중입니다...</div>;
    }

    // 탄소배출량 차트 데이터 가공 헬퍼
    const getCarbonData = (matVal, procVal, scrapVal, sumVal) => {
        const m = Number(matVal) || 0;
        const p = Number(procVal) || 0;
        const s = Number(scrapVal) || 0;
        const total = Number(sumVal) || (m + p + s) || 1;

        const mRate = (m / total) * 100;
        const pRate = (p / total) * 100;
        const sRate = (s / total) * 100;

        const p1 = mRate;
        const p2 = p1 + pRate;

        return {
            m, p, s, total,
            mRate: mRate.toFixed(1),
            pRate: pRate.toFixed(1),
            sRate: sRate.toFixed(1),
            gradientStyle: `conic-gradient(#064e3b 0% ${p1}%, #10b981 ${p1}% ${p2}%, #9ca3af ${p2}% 100%)`
        };
    };

    const massData = getCarbonData(
        carconInfo?.massCo2Mat,
        carconInfo?.massCo2Proc,
        carconInfo?.massCo2Scrap,
        carconInfo?.massCo2Sum
    );

    const unitData = getCarbonData(
        carconInfo?.unitCo2Mat,
        carconInfo?.unitCo2Proc,
        carconInfo?.unitCo2Scrap,
        carconInfo?.unitCo2Sum
    );

    return (
        <div style={{ padding: '1rem', boxSizing: 'border-box', backgroundColor: '#f9fafb', minHeight: '100%' }}>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 'bold', marginBottom: '1.5rem', color: '#111827' }}>
                📌 기본평가 결과 조회 (포장차수: {packLevel}차)
            </h2>

            {/* 1. 물성 정보 */}
            <div style={sectionStyle}>
                <h3 style={titleStyle}>1. 물성</h3>
                <div style={contentLayoutContainer}>
                    <div style={tableAreaStyle}>
                        <div style={tableWrapperStyle}>
                            <table style={tableStyle}>
                                <thead>
                                    <tr style={thTrStyle}>
                                        <th style={thStyle}>성능항목명</th>
                                        <th style={thStyle}>단위</th>
                                        <th style={thStyle}>기준값 범위</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {materials.length > 0 ? (
                                        materials.map((item, index) => (
                                            <tr key={index} style={trStyle}>
                                                <td style={tdStyle}>{item.itemName || item.item}</td>
                                                <td style={tdStyle}>{item.unitNm || item.unit || '-'}</td>
                                                <td style={tdStyle}>{item.acceptableRange ?? '-'}</td>
                                            </tr>
                                        ))
                                    ) : (
                                        <tr>
                                            <td colSpan="3" style={emptyTdStyle}>조회된 물성 정보가 없습니다.</td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <div style={imageAreaStyle}>
                        <div style={imageCardStyle}>
                            <div style={imageTitleStyle}>패키지 이미지</div>
                            <div style={imageBoxStyle}>
                                <span style={noImageTextStyle}>No Image</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* 2. 환경규제 정보 */}
            <div style={sectionStyle}>
                <h3 style={titleStyle}>2. 환경규제정보</h3>
                <div style={tableWrapperStyle}>
                    <table style={tableStyle}>
                        <thead>
                            <tr style={thTrStyle}>
                                <th style={thStyle}>관련 규정</th>
                                <th style={thStyle}>규제 항목</th>
                                <th style={thStyle}>규제 내용</th>
                            </tr>
                        </thead>
                        <tbody>
                            {environments.length > 0 ? (
                                environments.map((env, index) => (
                                    <tr key={index} style={trStyle}>
                                        <td style={tdStyle}>{env.relatedReg || '-'}</td>
                                        <td style={tdStyle}>{env.regItem || '-'}</td>
                                        <td style={tdStyle}>{env.dtlCont || '-'}</td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan="3" style={emptyTdStyle}>조회된 환경규제 정보가 없습니다.</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* 3. 공정도 */}
            <div style={sectionStyle}>
                <h3 style={titleStyle}>3. 공정도</h3>
                {processFlows.length > 0 ? (
                    <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
                        {processFlows.map((proc, index) => (
                            <div key={index} style={{ flex: '1', minWidth: '300px', padding: '0.75rem', border: '1px solid #e5e7eb', borderRadius: '6px', background: '#fff' }}>
                                <div style={{ fontWeight: '600', marginBottom: '0.5rem', fontSize: '0.85rem', color: '#374151' }}>
                                    재질 구성: {proc.matCompNm || proc.matComp || '-'}
                                </div>
                                {proc.memoImg ? (
                                    <div
                                        style={{ fontSize: '0.8rem', overflowX: 'auto', textAlign: 'center' }}
                                        dangerouslySetInnerHTML={{ __html: proc.memoImg }}
                                    />
                                ) : (
                                    <div style={{ color: '#9ca3af', fontSize: '0.75rem', textAlign: 'center' }}>등록된 공정도 이미지가 없습니다.</div>
                                )}
                            </div>
                        ))}
                    </div>
                ) : (
                    <div style={{ color: '#9ca3af', fontSize: '0.75rem', textAlign: 'center', padding: '1rem' }}>조회된 공정도 정보가 없습니다.</div>
                )}
            </div>

            {/* 4. 탄소배출량 정보 (두 카드를 가로 한 줄로 나란히 배치) */}
            <div style={sectionStyle}>
                <h3 style={titleStyle}>4. 탄소배출량</h3>
                <p style={{ fontSize: '0.8rem', color: '#6b7280', marginBottom: '1.2rem' }}>
                    전 과정 평가(LCA) 기반 탄소배출량 정보입니다.
                </p>

                {carconInfo ? (
                    <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap' }}>

                        {/* 중량 당 탄소배출량 카드 */}
                        <div style={carbonCardStyle}>
                            <div style={carbonHeaderStyle}>중량 당 탄소배출량 <span style={{ fontSize: '0.75rem', fontWeight: 'normal', color: '#6b7280' }}>(kgCO2eq/kg)</span></div>
                            <div style={carbonContentLayout}>
                                {/* 도넛 차트 영역 */}
                                <div style={donutContainerStyle}>
                                    <div style={{ ...donutStyle, background: massData.gradientStyle }}>
                                        <div style={donutHoleStyle}>
                                            <div style={{ fontSize: '0.65rem', color: '#4b5563', fontWeight: '600' }}>총 중량<br />탄소배출량</div>
                                            <div style={{ fontSize: '0.85rem', fontWeight: 'bold', color: '#111827', marginTop: '2px' }}>{massData.total.toFixed(3)}</div>
                                        </div>
                                    </div>
                                    <div style={legendListStyle}>
                                        <div style={legendItemStyle}><span style={{ ...legendDotStyle, backgroundColor: '#064e3b' }}></span>원료</div>
                                        <div style={legendItemStyle}><span style={{ ...legendDotStyle, backgroundColor: '#10b981' }}></span>제조</div>
                                        <div style={legendItemStyle}><span style={{ ...legendDotStyle, backgroundColor: '#9ca3af' }}></span>폐기</div>
                                    </div>
                                </div>

                                {/* 상세 수치 영역 */}
                                <div style={carbonStatsStyle}>
                                    <div style={statColStyle}>
                                        <div style={badgeStyle('#064e3b')}>원료</div>
                                        <div style={statValueStyle}>{massData.m.toFixed(3)}</div>
                                        <div style={statRateStyle}>{massData.mRate}%</div>
                                    </div>
                                    <div style={statColStyle}>
                                        <div style={badgeStyle('#10b981')}>제조</div>
                                        <div style={statValueStyle}>{massData.p.toFixed(3)}</div>
                                        <div style={statRateStyle}>{massData.pRate}%</div>
                                    </div>
                                    <div style={statColStyle}>
                                        <div style={badgeStyle('#6b7280')}>폐기</div>
                                        <div style={statValueStyle}>{massData.s.toFixed(3)}</div>
                                        <div style={statRateStyle}>{massData.sRate}%</div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* 단위 당 탄소배출량 카드 */}
                        <div style={carbonCardStyle}>
                            <div style={carbonHeaderStyle}>단위 당 탄소배출량 <span style={{ fontSize: '0.75rem', fontWeight: 'normal', color: '#6b7280' }}>(kgCO2eq/관리단위)</span></div>
                            <div style={carbonContentLayout}>
                                {/* 도넛 차트 영역 */}
                                <div style={donutContainerStyle}>
                                    <div style={{ ...donutStyle, background: unitData.gradientStyle }}>
                                        <div style={donutHoleStyle}>
                                            <div style={{ fontSize: '0.65rem', color: '#4b5563', fontWeight: '600' }}>총 단위<br />탄소배출량</div>
                                            <div style={{ fontSize: '0.85rem', fontWeight: 'bold', color: '#111827', marginTop: '2px' }}>{unitData.total.toFixed(3)}</div>
                                        </div>
                                    </div>
                                    <div style={legendListStyle}>
                                        <div style={legendItemStyle}><span style={{ ...legendDotStyle, backgroundColor: '#064e3b' }}></span>원료</div>
                                        <div style={legendItemStyle}><span style={{ ...legendDotStyle, backgroundColor: '#10b981' }}></span>제조</div>
                                        <div style={legendItemStyle}><span style={{ ...legendDotStyle, backgroundColor: '#9ca3af' }}></span>폐기</div>
                                    </div>
                                </div>

                                {/* 상세 수치 영역 */}
                                <div style={carbonStatsStyle}>
                                    <div style={statColStyle}>
                                        <div style={badgeStyle('#064e3b')}>원료</div>
                                        <div style={statValueStyle}>{unitData.m.toFixed(3)}</div>
                                        <div style={statRateStyle}>{unitData.mRate}%</div>
                                    </div>
                                    <div style={statColStyle}>
                                        <div style={badgeStyle('#10b981')}>제조</div>
                                        <div style={statValueStyle}>{unitData.p.toFixed(3)}</div>
                                        <div style={statRateStyle}>{unitData.pRate}%</div>
                                    </div>
                                    <div style={statColStyle}>
                                        <div style={badgeStyle('#6b7280')}>폐기</div>
                                        <div style={statValueStyle}>{unitData.s.toFixed(3)}</div>
                                        <div style={statRateStyle}>{unitData.sRate}%</div>
                                    </div>
                                </div>
                            </div>
                        </div>

                    </div>
                ) : (
                    <p style={{ color: '#6b7280', fontSize: '0.85rem' }}>조회된 탄소배출량 정보가 없습니다.</p>
                )}
            </div>
        </div>
    );
};

// 스타일 정의
const sectionStyle = {
    marginBottom: '2rem',
    background: '#ffffff',
    padding: '1.5rem',
    borderRadius: '8px',
    boxShadow: '0 1px 3px rgba(0,0,0,0.1)'
};

const titleStyle = {
    fontSize: '1.1rem',
    fontWeight: '600',
    marginBottom: '0.5rem',
    color: '#374151',
    borderBottom: '2px solid #e5e7eb',
    paddingBottom: '0.5rem'
};

const contentLayoutContainer = {
    display: 'flex',
    gap: '1.5rem',
    alignItems: 'flex-start',
    flexWrap: 'wrap'
};

const tableAreaStyle = {
    flex: '5',
    minWidth: '300px'
};

const imageAreaStyle = {
    flex: '2',
    minWidth: '180px'
};

const imageCardStyle = {
    background: '#fdfdfd',
    border: '1px solid #e5e7eb',
    borderRadius: '6px',
    padding: '0.75rem',
    textAlign: 'center',
    height: '100%',
    display: 'flex',
    flexDirection: 'column'
};

const imageTitleStyle = {
    fontSize: '0.85rem',
    fontWeight: '600',
    color: '#4b5563',
    marginBottom: '0.5rem'
};

const imageBoxStyle = {
    flex: '1',
    minHeight: '180px',
    backgroundColor: '#f3f4f6',
    border: '1px dashed #d1d5db',
    borderRadius: '4px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center'
};

const noImageTextStyle = {
    color: '#9ca3af',
    fontSize: '0.85rem',
    fontWeight: '500'
};

const tableWrapperStyle = {
    overflowX: 'auto'
};

const tableStyle = {
    width: '100%',
    borderCollapse: 'collapse',
    textAlign: 'left',
    fontSize: '0.75rem'
};

const thTrStyle = {
    backgroundColor: '#f3f4f6',
    borderBottom: '1px solid #d1d5db'
};

const thStyle = {
    padding: '0.15rem 0.5rem',
    fontWeight: '600',
    color: '#4b5563',
    lineHeight: '1.2'
};

const trStyle = {
    borderBottom: '1px solid #e5e7eb'
};

const tdStyle = {
    padding: '0.15rem 0.5rem',
    color: '#1f2937',
    lineHeight: '1.2'
};

const emptyTdStyle = {
    padding: '1rem',
    textAlign: 'center',
    color: '#9ca3af',
    fontSize: '0.75rem'
};

// 탄소배출량 카드 가로 배치를 위한 스타일 (flex: 1 및 minWidth 조정)
const carbonCardStyle = {
    flex: '1',
    minWidth: '450px',
    border: '1px solid #e5e7eb',
    borderRadius: '8px',
    padding: '1.2rem',
    backgroundColor: '#ffffff',
    boxSizing: 'border-box'
};

const carbonHeaderStyle = {
    fontSize: '0.95rem',
    fontWeight: '600',
    color: '#374151',
    borderBottom: '1px solid #f3f4f6',
    paddingBottom: '0.75rem',
    marginBottom: '1rem'
};

const carbonContentLayout = {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: '1rem',
    flexWrap: 'wrap'
};

const donutContainerStyle = {
    display: 'flex',
    alignItems: 'center',
    gap: '1rem'
};

const donutStyle = {
    width: '85px',
    height: '85px',
    borderRadius: '50%',
    position: 'relative',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    boxShadow: 'inset 0 0 0 2px rgba(255,255,255,0.2)'
};

const donutHoleStyle = {
    width: '55px',
    height: '55px',
    backgroundColor: '#ffffff',
    borderRadius: '50%',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    textAlign: 'center'
};

const legendListStyle = {
    display: 'flex',
    flexDirection: 'column',
    gap: '0.3rem'
};

const legendItemStyle = {
    display: 'flex',
    alignItems: 'center',
    gap: '0.4rem',
    fontSize: '0.75rem',
    color: '#4b5563',
    fontWeight: '500'
};

const legendDotStyle = {
    width: '8px',
    height: '8px',
    borderRadius: '50%',
    display: 'inline-block'
};

const carbonStatsStyle = {
    display: 'flex',
    gap: '1rem',
    borderLeft: '1px solid #f3f4f6',
    paddingLeft: '1rem'
};

const statColStyle = {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    gap: '0.3rem'
};

const badgeStyle = (bgColor) => ({
    backgroundColor: bgColor,
    color: '#ffffff',
    padding: '0.15rem 0.5rem',
    borderRadius: '10px',
    fontSize: '0.65rem',
    fontWeight: '600'
});

const statValueStyle = {
    fontSize: '0.85rem',
    fontWeight: 'bold',
    color: '#111827',
    marginTop: '0.2rem'
};

const statRateStyle = {
    fontSize: '0.7rem',
    color: '#6b7280',
    fontWeight: '500'
};

export default Prjdefaultresult;