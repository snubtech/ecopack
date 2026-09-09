import { useState, useEffect } from 'react';
import { getCurrentCustomerId } from '../utils/memberProfile';
import { GetProjectDetailReport, getMaterial, getEnvironment, getProcessFlow, getCarconInfo, SaveProjectDetailReport } from '../api/projects';

const Prjdefaultresult = ({ prjId, packLevel, onSelectItem }) => {
    const [materials, setMaterials] = useState([]);
    const [environments, setEnvironments] = useState([]);
    const [processFlows, setProcessFlows] = useState([]);
    const [carconInfo, setCarconInfo] = useState(null);
    const [loading, setLoading] = useState(true);

    const [reportMeta, setReportMeta] = useState({
        item: '',
        itemNm: '',
        unit: '',
        unitNm: '',
        acceptableRange: '',
        relatedReg: '',
        regItem: '',
        dtlCont: '',
        matComp: '',
        matCompNm: '',
        memoImg: '',
        fileData: '',
        packLevelNm: '',
        appliedMaterial: '',
        appliedMaterialNm: '',
        prdExpCntry: '',
        prdExpCntryNm: '',
        matType: '',
        matTypeNm: '',
        matForm: '',
        matFormNm: '',
    });

    useEffect(() => {
        let isMounted = true;

        const fetchData = async () => {
            try {
                setLoading(true);

                const appliedMaterial = sessionStorage.getItem('currentMaterial') || '';
                const matType = sessionStorage.getItem('currentMatType') || '';
                const matform = sessionStorage.getItem('currentMatForm') || '';
                const currentExportCountry = sessionStorage.getItem('currentExportCountry') || '';

                console.log("📌 전송 파라미터 확인:", { prjId, packLevel, appliedMaterial, matType, matform, currentExportCountry });

                        setCarconInfo({
                            massCo2Mat: savedReportData.massCo2Mat,
                            massCo2Proc: savedReportData.massCo2Proc,
                            massCo2Scrap: savedReportData.massCo2Scrap,
                            massCo2Sum: savedReportData.massCo2Sum,
                            unitCo2Mat: savedReportData.unitCo2Mat,
                            unitCo2Proc: savedReportData.unitCo2Proc,
                            unitCo2Scrap: savedReportData.unitCo2Scrap,
                            unitCo2Sum: savedReportData.unitCo2Sum,
                        });

                        setLoading(false);
                        return;
                    
                } catch (err) {
                    console.log("저장된 상세 리포트가 없음. 신규 분석 데이터 조회 프로세스로 진행합니다.", err);
                }

                // 저장된 데이터가 없을 경우 정상적인 신규 분석 데이터 조회 수행
                const matData = await getMaterial(currentPackLevel, appliedMaterial, matType);
                if (!isMounted) return;
                setMaterials(matData || []);

                const envData = await getEnvironment(currentPackLevel, appliedMaterial, currentExportCountry);
                if (!isMounted) return;
                setEnvironments(envData || []);

                const procData = await getProcessFlow(appliedMaterial, matType);
                if (!isMounted) return;
                setProcessFlows(procData || []);

                const carData = await getCarconInfo(currentPackLevel, appliedMaterial, matform);
                if (!isMounted) return;
                setCarconInfo(carData || null);

            } catch (error) {
                console.error("데이터 조회 중 오류 발생:", error);
            } finally {
                if (isMounted) setLoading(false);
            }
        };

        if (prjId && packLevel) {
            fetchData();
        }

        return () => {
            isMounted = false;
        };
    
    }, [prjId, packLevel]);

    const handleSave = async () => {
        try {
            const currentPrjId = prjId || sessionStorage.getItem('currentPrjId') || '';
            const currentPackLevel = packLevel || sessionStorage.getItem('currentPackLevel') || '';
            const userId = getCurrentCustomerId() || '';

            const appliedMaterial = sessionStorage.getItem('currentMaterial') || '';
            const appliedMaterialNm = sessionStorage.getItem('currentMaterialNm') || '';
            const prdExpCntry = sessionStorage.getItem('currentExportCountry') || '';
            const prdExpCntryNm = sessionStorage.getItem('currentExportCountryNm') || '';
            const matType = sessionStorage.getItem('currentMatType') || '';
            const matTypeNm = sessionStorage.getItem('currentMatTypeNm') || '';
            const matForm = sessionStorage.getItem('currentMatForm') || '';
            const matFormNm = sessionStorage.getItem('currentMatFormNm') || '';

            // 첫 번째 공정 정보에서 재질 구성 추출 (있는 경우)
            const firstProc = processFlows[0] || {};

            const saveData = {
                prjId: currentPrjId,
                packLevel: currentPackLevel,
                prjuserid: userId,
                packLevelNm: reportMeta.packLevelNm || '',
                appliedMaterial: appliedMaterial,
                appliedMaterialNm: appliedMaterialNm,
                prdExpCntry: prdExpCntry,
                prdExpCntryNm: prdExpCntryNm,
                matType: matType,
                matTypeNm: matTypeNm,
                matForm: matForm,
                matFormNm: matFormNm,

                // 탄소 배출량 공통 데이터
                massCo2Mat: String(carconInfo?.massCo2Mat || 0),
                massCo2Proc: String(carconInfo?.massCo2Proc || 0),
                massCo2Scrap: String(carconInfo?.massCo2Scrap || 0),
                massCo2Sum: String(carconInfo?.massCo2Sum || 0),
                unitCo2Mat: String(carconInfo?.unitCo2Mat || 0),
                unitCo2Proc: String(carconInfo?.unitCo2Proc || 0),
                unitCo2Scrap: String(carconInfo?.unitCo2Scrap || 0),
                unitCo2Sum: String(carconInfo?.unitCo2Sum || 0),

                // 공정도 관련 단건 정보
                matComp: firstProc.matComp || '',
                matCompNm: firstProc.matCompNm || '',
                memoImg: firstProc.memoImg || '',
                fileData: reportMeta.fileData || '',

                // 1. 물성 정보 리스트 (실제 state인 materials 반영)
                materials: (materials || []).map(item => ({
                    item: item.item || '',
                    itemName: item.itemName || item.item || '',
                    unit: item.unit || '',
                    unitNm: item.unitNm || '',
                    acceptableRange: typeof item.acceptableRange === 'object'
                        ? JSON.stringify(item.acceptableRange)
                        : String(item.acceptableRange || '')
                })),

                // 2. 환경 규제 정보 리스트 (실제 state인 environments 반영)
                environments: (environments || []).map(env => ({
                    relatedReg: env.relatedReg || '',
                    regItem: env.regItem || '',
                    dtlCont: env.dtlCont || ''
                }))
            };

            // 정의된 API 호출 함수 사용
            await SaveProjectDetailReport(saveData);
            alert('저장되었습니다.');
        } catch (error) {
            console.error("저장 중 오류 발생:", error);
            alert('저장에 실패했습니다.');
        }
    };
    const handleNextStep = async () => {
        // 필요 시 저장 로직 추가 가능 (현재는 세션 저장 및 탭 전환 수행)
        if (typeof onSelectItem === 'function') {
            onSelectItem('prjeval');
        } else {
            console.error("onSelectItem이 함수가 아닙니다!");
        }
    };

    if (loading) {
        return <div style={{ padding: '2rem', textAlign: 'center' }}>데이터를 불러오는 중입니다...</div>;
    }

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
                📌 기본평가 결과 조회 (프로젝트 번호: {prjId} / 포장차수: {packLevel}차)
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

            {/* 4. 탄소배출량 정보 */}
            <div style={sectionStyle}>
                <h3 style={titleStyle}>4. 탄소배출량</h3>
                <p style={{ fontSize: '0.8rem', color: '#6b7280', marginBottom: '1.2rem' }}>
                    전 과정 평가(LCA) 기반 탄소배출량 정보입니다.
                </p>

                {carconInfo ? (
                    <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap' }}>
                        <div style={carbonCardStyle}>
                            <div style={carbonHeaderStyle}>중량 당 탄소배출량 <span style={{ fontSize: '0.75rem', fontWeight: 'normal', color: '#6b7280' }}>(kgCO2eq/kg)</span></div>
                            <div style={carbonContentLayout}>
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

                        <div style={carbonCardStyle}>
                            <div style={carbonHeaderStyle}>단위 당 탄소배출량 <span style={{ fontSize: '0.75rem', fontWeight: 'normal', color: '#6b7280' }}>(kgCO2eq/관리단위)</span></div>
                            <div style={carbonContentLayout}>
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

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '2rem', paddingBottom: '1rem' }}>
                <button
                    onClick={handleSave}
                    style={{
                        padding: '0.6rem 1.2rem',
                        backgroundColor: '#10b981',
                        color: '#ffffff',
                        border: 'none',
                        borderRadius: '6px',
                        fontWeight: '600',
                        fontSize: '0.85rem',
                        cursor: 'pointer'
                    }}
                >
                    저장
                </button>
                <button
                    className="btn-primary"
                    onClick={handleNextStep}
                    style={{
                        padding: '0.6rem 1.2rem',
                        backgroundColor: '#374151',
                        color: '#ffffff',
                        border: 'none',
                        borderRadius: '6px',
                        fontWeight: '600',
                        fontSize: '0.85rem',
                        cursor: 'pointer'
                    }}
                >
                    다음 &gt;
                </button>
            </div>
        </div>
    );
};

// 스타일 정의 객체들...
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