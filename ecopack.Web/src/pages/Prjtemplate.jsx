import { useState, useEffect, useRef } from 'react';
import axios from 'axios';
import { templateUpdate } from '../api/projects'; //api /projects.js에서 templateUpdate 함수를 가져옵니다.

// 1. 세션 스토리지에 데이터 저장
// sessionStorage.setItem('currentPrjNm');
// sessionStorage.setItem('currentPrjId');
// sessionStorage.setItem('currentPackLevel');  포장차수
// sessionStorage.setItem('currentExportCountry'); 수출국가.
// sessionStorage.setItem('currentMaterial'); // 적용소재 세션 저장
// sessionStorage.setItem('currentEnv');  // 사용환경 세션 저장
// sessionStorage.setItem('currentMatType'); // 포장재 종류 세션 저장
function PackTemplatePage({ onSelectItem }) {
    // 1. 상태(State) 정의
    const [templateList, setTemplateList] = useState([]); // 하단 리스트 데이터
    const [selectedItem, setSelectedItem] = useState(null); // 상단에 보여줄 선택된 상세 정보

    // 💡 페이지네이션 관련 상태
    const [currentPage, setCurrentPage] = useState(1);       // 현재 페이지 번호
    const [totalCount, setTotalCount] = useState(0);         // 전체 데이터 개수
    const pageSize = 20;                                     // 페이지당 표시 개수 (20개)

    const isFirstRun = useRef(true); // 초기 마운트 시 중복 호출 방지용

    // 2. 페이지 번호(currentPage)가 바뀌거나 처음 뜰 때 API 호출
    useEffect(() => {
        const fetchTemplates = async () => {
            try {
                const params = {
                    packLevel: sessionStorage.getItem('currentPackLevel') || '1',
                    appliedMaterial: sessionStorage.getItem('currentMaterial') || '',
                    matType: sessionStorage.getItem('currentMatType') || '',
                    page: currentPage,
                    pageSize: pageSize
                };

                const response = await axios.get('/api/Projects/template', { params });
                const resData = response.data;

                const items = resData.items || [];
                const total = resData.totalCount || 0;

                setTemplateList(items);
                setTotalCount(total);

                if (isFirstRun.current && items.length > 0) {
                    setSelectedItem(items[0]);
                    isFirstRun.current = false;
                }
            } catch (error) {
                console.error('템플릿 데이터를 불러오는 중 에러 발생:', error);
            }
        };

        fetchTemplates();
    }, [currentPage]);

    // [저장 버튼 클릭 핸들러]
    const handleSave = async () => {
        if (!selectedItem) {
            alert('저장할 템플릿을 선택해주세요.');
            return;
        }

        const prjId = sessionStorage.getItem('currentPrjId') || '';
        const packLevel = selectedItem.packLevel || sessionStorage.getItem('currentPackLevel') || '1';
        // userid는   prjuserid라는 이름으로  json으로 저장되어 있으므로 파싱 후 repCustId를 가져옵니다.
        // sessionStorage에서 prjuserid를 가져와 JSON으로 파싱
        // userid만  로그인처리때문에...상이함.
        const sessionUser = JSON.parse(sessionStorage.getItem('prjuserid') || '{}');
        const prjuserid = sessionUser.repCustId || '';
       
        if (!prjId) {
            alert('프로젝트 ID(prjId)를 찾을 수 없습니다. 이전 단계를 확인해 주세요.');
            return;
        }

        const dto = {
            prjId: prjId,
            packLevel: packLevel,
            packDsgnTplId: selectedItem.packDsgnTplId,
            prjuserid: prjuserid
        };

        try {
            const result = await templateUpdate(dto);
            console.log("업데이트 결과:", result);
            alert('성공적으로 저장되었습니다.');
        } catch (error) {
            console.error('저장 중 에러 발생:', error);
            alert('저장 중 오류가 발생했습니다.');
        }
    };
    const handleNextStep = async () => {
        if (!selectedItem) {
            alert('먼저 템플릿을 선택해 주세요.');
            return;
        }

        // 선택된 템플릿 정보를 세션에 안전하게 저장
        sessionStorage.setItem('currentPackDsgnTplId', selectedItem.packDsgnTplId);

        if (typeof onSelectItem === 'function') {
            console.log("onSelectItem 함수 실행됨!");
            onSelectItem('prjeval'); // 'prjeval'(평가지) 메뉴로 상태 변경 요청
        } else {
            console.error("onSelectItem이 함수가 아닙니다! 부모에서 전달받았는지 확인하세요.");
        }
    };
    // 전체 페이지 수 계산
    const totalPages = Math.ceil(totalCount / pageSize);

    return (
        <div className="container" style={{ padding: '20px', boxSizing: 'border-box', width: '100%' }}>

            {/* [상단 영역] 선택된 이미지의 상세 정보 및 우측 세로 버튼 그룹 */}
            <div className="top-detail-section" style={{ border: '1px solid #ddd', padding: '20px', marginBottom: '20px', borderRadius: '8px', background: '#fff', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                {selectedItem ? (
                    <div style={{ display: 'flex', gap: '20px', alignItems: 'center', flex: 1 }}>
                        <div style={{ width: '120px', height: '120px', background: '#f0f0f0', borderRadius: '6px', overflow: 'hidden', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
                            {selectedItem.fileData ? (
                                <img
                                    src={`data:image/png;base64,${selectedItem.fileData}`}
                                    alt={selectedItem.fileNm}
                                    style={{ width: '100%', height: '100%', objectFit: 'contain' }}
                                />
                            ) : (
                                <span>이미지 없음</span>
                            )}
                        </div>
                        <div>
                            <h3 style={{ margin: '0 0 2px 0' }}>
                                {selectedItem.packDsgnTplId} {selectedItem.subject ? `/ ${selectedItem.subject}` : ''}
                            </h3>
                            <p style={{ margin: '0 0 8px 0', color: '#444' }}>{selectedItem.dsgnExpCon || '설명이 없습니다.'}</p>
                            <p style={{ margin: '0 0 8px 0', color: '#444' }}>{selectedItem.operDscr || '설명이 없습니다.'}</p>
                            <span style={{ fontSize: '12px', color: '#666' }}>
                                분류: {selectedItem.matTypeNm} / {selectedItem.appliedMaterialNm}
                            </span>
                        </div>
                    </div>
                ) : (
                    <p style={{ margin: 0, color: '#888', flex: 1 }}>상단에서 확인할 템플릿을 선택해 주세요.</p>
                )}

                {/* 우측 세로 방향 버튼 그룹 */}
                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', marginLeft: '20px', flexShrink: 0 }}>
                    <button
                        onClick={() => { alert('취소되었습니다.'); }}
                        style={{ padding: '8px 16px', border: '1px solid #ccc', borderRadius: '4px', background: '#fff', cursor: 'pointer', width: '80px' }}
                    >
                        취소
                    </button>
                    <button
                        onClick={handleSave}
                        style={{ padding: '8px 16px', border: 'none', borderRadius: '4px', background: '#1976d2', color: '#fff', cursor: 'pointer', width: '80px' }}
                    >
                        저장
                    </button>
                    <button
                        onClick={handleNextStep}
                        style={{ padding: '8px 16px', border: 'none', borderRadius: '4px', background: '#2e7d32', color: '#fff', cursor: 'pointer', width: '80px' }}
                    >
                        다음
                    </button>
                </div>
            </div>

            {/* [하단 영역] 이미지 리스트 및 테두리 박스 */}
            <div className="bottom-list-section">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '10px' }}>
                    <h4 style={{ margin: 0 }}>적용 소재 및 패키지 선택</h4>
                    <span style={{ fontSize: '13px', color: '#666' }}>전체 {totalCount}개 중 (페이지 {currentPage}/{totalPages || 1})</span>
                </div>

                <div style={{
                    border: '1px solid #ddd',
                    borderRadius: '8px',
                    padding: '20px',
                    minHeight: '180px',
                    background: '#fff',
                    boxSizing: 'border-box',
                    width: '100%'
                }}>
                    {templateList.length > 0 ? (
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '15px', width: '100%', boxSizing: 'border-box' }}>
                            {templateList.map((item, index) => {
                                const isSelected = selectedItem?.packDsgnTplId === item.packDsgnTplId;

                                return (
                                    <div
                                        key={item.packDsgnTplId || index}
                                        onClick={() => setSelectedItem(item)}
                                        style={{
                                            border: isSelected ? '2px solid #2e7d32' : '1px solid #ddd',
                                            padding: '12px',
                                            borderRadius: '8px',
                                            cursor: 'pointer',
                                            background: isSelected ? '#f4f9f4' : '#fff',
                                            display: 'flex',
                                            flexDirection: 'column',
                                            justifyContent: 'space-between',
                                            boxSizing: 'border-box',
                                            minWidth: 0
                                        }}
                                    >
                                        <div>
                                            <div style={{ fontSize: '13px', fontWeight: 'bold', marginBottom: '6px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                                {item.packDsgnTplId} / {item.matTypeNm} / {item.appliedMaterialNm}
                                            </div>
                                            <div style={{ fontSize: '11px', color: '#555', marginBottom: '8px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                                {item.subject}
                                            </div>
                                        </div>

                                        <div style={{ width: '100%', height: '110px', background: '#f0f0f0', borderRadius: '4px', overflow: 'hidden', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                                            {item.fileData ? (
                                                <img
                                                    src={`data:image/png;base64,${item.fileData}`}
                                                    alt={item.fileNm}
                                                    style={{ width: '100%', height: '100%', objectFit: 'contain' }}
                                                />
                                            ) : (
                                                <span>📦</span>
                                            )}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    ) : (
                        <div style={{ textAlign: 'center', padding: '40px 0', color: '#666' }}>
                            선택된 템플릿 항목이 없습니다. 이전 단계에서 조건을 다시 확인해 주세요.
                        </div>
                    )}

                    {/* [하단 페이지네이션 번호 버튼 UI] */}
                    {totalPages > 1 && (
                        <div style={{ display: 'flex', justifyContent: 'center', gap: '8px', marginTop: '25px' }}>
                            {Array.from({ length: totalPages }, (_, i) => i + 1).map((pageNum) => (
                                <button
                                    key={pageNum}
                                    onClick={() => setCurrentPage(pageNum)}
                                    style={{
                                        padding: '6px 12px',
                                        border: '1px solid #ddd',
                                        borderRadius: '4px',
                                        background: currentPage === pageNum ? '#2e7d32' : '#fff',
                                        color: currentPage === pageNum ? '#fff' : '#333',
                                        cursor: 'pointer',
                                        fontWeight: currentPage === pageNum ? 'bold' : 'normal'
                                    }}
                                >
                                    {pageNum}
                                </button>
                            ))}
                        </div>
                    )}

                </div>
            </div>

        </div>
    );
}

export default PackTemplatePage;