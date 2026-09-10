import { useState, useEffect } from 'react';
import { getEvalSummary } from '../api/projects';

function Prjevalresult({ onSelectItem }) {
    const [loading, setLoading] = useState(true);
    const [totalScore, setTotalScore] = useState(0);
    const [maxScore, setMaxScore] = useState(100);
    const [savedItems, setSavedItems] = useState([]);

    const prjId = sessionStorage.getItem('currentPrjId') || '';
    const packLevel = sessionStorage.getItem('currentPackLevel') || '3';

    const sessionUser = JSON.parse(sessionStorage.getItem('prjuserid') || '{}');
    const prjUserId = sessionUser.repCustId || '';

    useEffect(() => {
        const fetchEvalResultData = async () => {
            if (!prjId) {
                setLoading(false);
                return;
            }

            try {
                setLoading(true);
                const data = await getEvalSummary(prjId, prjUserId, packLevel);

                if (data) {
                    const items = data.savedItems || [];

                    let calculatedTotal = 0;
                    let calculatedMax = 0;

                    items.forEach((item) => {
                        const criteriaStr = String(item.scoringCriteria ?? '').toLowerCase();
                        const pointStr = String(item.asmtpoint ?? '').toLowerCase();

                        const isPassFail = criteriaStr === 'pass' || pointStr === 'pass';

                        if (!isPassFail) {
                            const point = parseInt(item.asmtpoint, 10) || 0;
                            const rawCriteria = item.scoringCriteria ?? item.maxPoint ?? item.itemMaxPoint ?? item.maxScore ?? 10;
                            const maxP = !isNaN(parseInt(rawCriteria, 10)) ? parseInt(rawCriteria, 10) : 10;

                            calculatedTotal += point;
                            calculatedMax += maxP;
                        }
                    });

                    setTotalScore(calculatedTotal);
                    setMaxScore(calculatedMax > 0 ? calculatedMax : (data.maxScore || 100));
                    setSavedItems(items);
                }
            } catch (error) {
                console.error('평가 결과 로딩 실패:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchEvalResultData();
    }, [prjId, prjUserId, packLevel]);

    // 버튼 클릭 핸들러 추가
    const handleGoProjectStatus = () => {
        //console.log('프로젝트 현황 보기 이동');
        if (typeof onSelectItem === 'function') {
            onSelectItem('project-history');
        } else {
            console.error("onSelectItem이 함수가 아닙니다!");
        }
        // 예: navigate('/project-status'); 또는 라우팅 로직 구현
    };

    const handleGoAiImage = () => {
        console.log('AI 추천 이미지 생성 이동');
        if (typeof onSelectItem === 'function') {
            onSelectItem('PrjaiImage');
        } else {
            console.error("onSelectItem이 함수가 아닙니다!");
        }
        // 예: navigate('/ai-image'); 또는 라우팅 로직 구현
    };

    if (loading) {
        return <div style={{ textAlign: 'center', padding: '50px', fontSize: '16px', color: '#57606a' }}>평가 결과를 불러오는 중입니다...</div>;
    }

    return (
        <div style={styles.container}>
            {/* 1. 좌측 패널 (종합점수, 6각형 차트, 영역별 점수 요약, 하단 버튼 2개) */}
            <div style={styles.leftPanel}>
                <div style={styles.scoreCard}>
                    <h4 style={styles.scoreTitle}>모의평가 종합점수</h4>
                    <div style={styles.scoreValue}>
                        {totalScore} <span style={styles.scoreMax}>/ {maxScore}</span>
                    </div>
                </div>

                <div style={styles.chartContainer}>
                    <div style={styles.radarPlaceholder}>
                        <div style={styles.hexagonPolygon1}></div>
                        <div style={styles.hexagonPolygon2}></div>
                        <div style={styles.hexagonPolygon3}></div>
                        <span style={{ position: 'absolute', top: '4px', fontSize: '10px', fontWeight: '600', color: '#57606a' }}>SAFETY</span>
                        <span style={{ position: 'absolute', top: '35%', left: '4px', fontSize: '10px', fontWeight: '600', color: '#57606a' }}>REDUCE</span>
                        <span style={{ position: 'absolute', bottom: '20px', left: '16px', fontSize: '10px', fontWeight: '600', color: '#57606a' }}>REUSE</span>
                        <span style={{ position: 'absolute', bottom: '4px', fontSize: '10px', fontWeight: '600', color: '#57606a' }}>RECYCLE</span>
                        <span style={{ position: 'absolute', bottom: '20px', right: '16px', fontSize: '10px', fontWeight: '600', color: '#57606a' }}>REPLACE</span>
                        <span style={{ position: 'absolute', top: '35%', right: '4px', fontSize: '10px', fontWeight: '600', color: '#57606a' }}>INNOVATION</span>
                    </div>
                </div>

                {/* 좌측 하단 영역별 점수 현황 동적 렌더링 영역 */}
                <div style={styles.areaScoreBox}>
                    {(() => {
                        const areaScoreMap = {};
                        savedItems.forEach((item) => {
                            const areaKey = item.ecoPackLarType || item.ecoPackAreaNm || 'SAFETY';
                            if (!areaScoreMap[areaKey]) {
                                areaScoreMap[areaKey] = { currentScore: 0, maxScore: 0, hasNumeric: false };
                            }

                            const criteriaStr = String(item.scoringCriteria ?? '').toLowerCase();
                            const pointStr = String(item.asmtpoint ?? '').toLowerCase();
                            const isPassFail = criteriaStr === 'pass' || pointStr === 'pass';

                            if (!isPassFail) {
                                const point = parseInt(item.asmtpoint, 10) || 0;
                                const rawCriteria = item.scoringCriteria ?? item.maxPoint ?? item.itemMaxPoint ?? item.maxScore ?? 10;
                                const maxP = !isNaN(parseInt(rawCriteria, 10)) ? parseInt(rawCriteria, 10) : 10;

                                areaScoreMap[areaKey].currentScore += point;
                                areaScoreMap[areaKey].maxScore += maxP;
                                areaScoreMap[areaKey].hasNumeric = true;
                            }
                        });

                        const areaScores = Object.entries(areaScoreMap);

                        if (areaScores.length === 0) {
                            return <div style={{ fontSize: '13px', color: '#8c959f', textAlign: 'center' }}>영역별 점수 정보가 없습니다.</div>;
                        }

                        return areaScores.map(([areaName, scores], idx) => (
                            <div key={idx} style={styles.areaScoreRow}>
                                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                    <span style={styles.dotIcon}></span>
                                    <span style={styles.areaNameText}>{areaName}</span>
                                </div>
                                <div style={styles.areaScoreValue}>
                                    {scores.hasNumeric ? (
                                        <>
                                            <span style={styles.currentScoreNum}>{scores.currentScore}</span>
                                            <span style={styles.maxScoreNum}> / {scores.maxScore}</span>
                                        </>
                                    ) : (
                                        <span style={{ fontSize: '14px', fontWeight: '700', color: '#1a7f37' }}>PASS</span>
                                    )}
                                </div>
                            </div>
                        ));
                    })()}
                </div>

                {/* 좌측 패널 하단 버튼 2개 추가 */}
                <div style={styles.buttonGroup}>
                    <button style={styles.actionButtonSecondary} onClick={handleGoProjectStatus}>
                        프로젝트현황보기
                    </button>
                    <button style={styles.actionButtonPrimary} onClick={handleGoAiImage}>
                        AI추천이미지생성
                    </button>
                </div>
            </div>

            {/* 2. 우측 패널 (영역별 규제 분석 및 디자인 개선 방안 카드 리스트) */}
            <div style={styles.rightPanel}>
                {(() => {
                    const groupedMap = {};
                    savedItems.forEach((item) => {
                        const areaKey = item.ecoPackLarType || item.ecoPackAreaNm || 'SAFETY';

                        if (!groupedMap[areaKey]) {
                            groupedMap[areaKey] = {
                                larType: areaKey,
                                items: []
                            };
                        }
                        groupedMap[areaKey].items.push(item);
                    });

                    const groupedList = Object.values(groupedMap);

                    if (groupedList.length === 0) {
                        return (
                            <div style={{ textAlign: 'center', padding: '40px', color: '#8c959f', backgroundColor: '#fff', borderRadius: '12px', border: '1px solid #d0d7de' }}>
                                저장된 평가 결과 데이터가 없습니다. 문항을 먼저 풀고 저장해 주세요.
                            </div>
                        );
                    }

                    return groupedList.map((group, index) => {
                        const uniqueRegs = [...new Set(group.items.map(i => i.natRglAls || i.NatRglAls).filter(Boolean))];
                        const uniqueImps = [...new Set(group.items.map(i => i.dsgnRecmImp || i.DsgnRecmImp || i.dsgn_recm_imp).filter(Boolean))];

                        const isAllPass = group.items.every(i =>
                            String(i.scoringCriteria ?? '').toLowerCase() === 'pass' ||
                            String(i.asmtpoint ?? '').toLowerCase() === 'pass'
                        );

                        return (
                            <div key={group.larType || index} style={styles.card}>
                                <div style={styles.cardHeader}>
                                    <strong style={styles.cardTitleNum}>
                                        {index + 1}. {group.larType}
                                    </strong>
                                    <span style={styles.statusBadge}>{isAllPass ? 'PASS' : 'appropriate'}</span>
                                </div>

                                <div style={styles.cardSubBox}>
                                    <div style={{ marginBottom: '3px' }}>
                                        <strong style={{ color: '#1f2328', fontSize: '12px' }}>[국가별 규제제도 분석]</strong>
                                        {uniqueRegs.length > 0 ? (
                                            uniqueRegs.map((reg, idx) => (
                                                <div key={idx} style={{ marginTop: '1px', lineHeight: '1.4', fontSize: '11.5px' }}>
                                                    {reg}
                                                </div>
                                            ))
                                        ) : (
                                            <div style={{ marginTop: '1px', color: '#8c959f', fontSize: '11.5px' }}>관련 규제 및 가이드라인 정보가 없습니다.</div>
                                        )}
                                    </div>

                                    {uniqueImps.length > 0 && (
                                        <div style={{ marginTop: '4px', borderTop: '1px solid #d0d7de', paddingTop: '3px' }}>
                                            <strong style={{ color: '#1f2328', fontSize: '12px' }}>[디자인 추천을 위한 개선 방안]</strong>
                                            {uniqueImps.map((imp, idx) => (
                                                <div key={idx} style={{ marginTop: '1px', lineHeight: '1.4', fontSize: '11.5px' }}>
                                                    • {imp}
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            </div>
                        );
                    });
                })()}
            </div>
        </div>
    );
}

const styles = {
    container: {
        display: 'flex',
        gap: '24px',
        padding: '24px',
        backgroundColor: '#f6f8fa',
        minHeight: '100vh',
        boxSizing: 'border-box',
        fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif'
    },
    leftPanel: {
        width: '32%',
        display: 'flex',
        flexDirection: 'column',
        gap: '16px'
    },
    scoreCard: {
        backgroundColor: '#1f883d',
        color: '#ffffff',
        padding: '28px 20px',
        borderRadius: '12px',
        textAlign: 'center',
        boxShadow: '0 4px 12px rgba(31, 136, 61, 0.2)'
    },
    scoreTitle: {
        margin: '0 0 10px 0',
        fontSize: '15px',
        fontWeight: '500',
        letterSpacing: '-0.5px'
    },
    scoreValue: {
        fontSize: '48px',
        fontWeight: '800',
        margin: '0',
        lineHeight: '1'
    },
    scoreMax: {
        fontSize: '20px',
        fontWeight: '400',
        opacity: '0.8'
    },
    chartContainer: {
        backgroundColor: '#ffffff',
        padding: '20px',
        borderRadius: '12px',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        boxShadow: '0 1px 3px rgba(0,0,0,0.05)'
    },
    radarPlaceholder: {
        width: '200px',
        height: '200px',
        position: 'relative',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        background: 'radial-gradient(circle, #f0f3f6 0%, #ffffff 70%)',
        borderRadius: '50%',
        border: '1px solid #eaeef2'
    },
    hexagonPolygon1: {
        width: '120px',
        height: '104px',
        backgroundColor: 'rgba(46, 160, 67, 0.15)',
        clipPath: 'polygon(25% 0%, 75% 0%, 100% 50%, 75% 100%, 25% 100%, 0% 50%)',
        position: 'absolute',
        border: 'none'
    },
    hexagonPolygon2: {
        width: '150px',
        height: '130px',
        clipPath: 'polygon(25% 0%, 75% 0%, 100% 50%, 75% 100%, 25% 100%, 0% 50%)',
        position: 'absolute',
        border: '1px solid #eaeef2'
    },
    hexagonPolygon3: {
        width: '80px',
        height: '69px',
        clipPath: 'polygon(25% 0%, 75% 0%, 100% 50%, 75% 100%, 25% 100%, 0% 50%)',
        position: 'absolute',
        border: '1px dashed #d0d7de'
    },
    areaScoreBox: {
        backgroundColor: '#ffffff',
        padding: '16px 20px',
        borderRadius: '12px',
        boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
        display: 'flex',
        flexDirection: 'column',
        gap: '12px'
    },
    areaScoreRow: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        fontSize: '14px',
        borderBottom: '1px solid #f0f3f6',
        paddingBottom: '8px'
    },
    dotIcon: {
        width: '8px',
        height: '8px',
        backgroundColor: '#1f883d',
        borderRadius: '50%',
        display: 'inline-block'
    },
    areaNameText: {
        fontWeight: '600',
        color: '#24292f'
    },
    areaScoreValue: {
        display: 'flex',
        alignItems: 'baseline'
    },
    currentScoreNum: {
        fontSize: '20px',
        fontWeight: '800',
        color: '#1f883d',
        letterSpacing: '-0.5px'
    },
    maxScoreNum: {
        fontSize: '13px',
        fontWeight: '500',
        color: '#8c959f',
        marginLeft: '2px'
    },
    buttonGroup: {
        display: 'flex',
        gap: '10px',
        marginTop: '4px'
    },
    actionButtonSecondary: {
        flex: 1,
        backgroundColor: '#ffffff',
        color: '#24292f',
        border: '1px solid #d0d7de',
        padding: '12px 10px',
        borderRadius: '10px',
        fontSize: '13px',
        fontWeight: '600',
        cursor: 'pointer',
        boxShadow: '0 1px 2px rgba(0,0,0,0.02)',
        textAlign: 'center'
    },
    actionButtonPrimary: {
        flex: 1,
        backgroundColor: '#1f883d',
        color: '#ffffff',
        border: 'none',
        padding: '12px 10px',
        borderRadius: '10px',
        fontSize: '13px',
        fontWeight: '600',
        cursor: 'pointer',
        boxShadow: '0 2px 6px rgba(31, 136, 61, 0.2)',
        textAlign: 'center'
    },
    rightPanel: {
        width: '68%',
        display: 'flex',
        flexDirection: 'column',
        gap: '12px'
    },
    card: {
        backgroundColor: '#ffffff',
        padding: '10px 14px',
        borderRadius: '10px',
        border: '1px solid #d0d7de',
        boxShadow: '0 1px 3px rgba(0,0,0,0.02)'
    },
    cardHeader: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '4px'
    },
    cardTitleNum: {
        fontSize: '14px',
        fontWeight: '700',
        color: '#1f2328'
    },
    statusBadge: {
        backgroundColor: '#dafbe1',
        color: '#1a7f37',
        padding: '2px 8px',
        borderRadius: '6px',
        fontSize: '11px',
        fontWeight: '600'
    },
    cardSubBox: {
        backgroundColor: '#f6f8fa',
        padding: '6px 8px',
        borderRadius: '6px',
        color: '#57606a'
    }
};

export default Prjevalresult;