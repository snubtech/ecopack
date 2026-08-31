import { useState, useEffect, useRef } from 'react';
import axios from 'axios';

function PackTemplatePage() {
    // 1. 상태(State) 정의
    const [templateList, setTemplateList] = useState([]); // 하단 리스트 데이터
    const [selectedItem, setSelectedItem] = useState(null); // 상단에 보여줄 선택된 상세 정보
    const isFetchedRef = useRef(false); // 💡 React StrictMode 중복 호출 방지용 Ref

    // 2. 페이지가 처음 뜰 때 API 호출 (세션 스토리지의 조건값 포함)
    useEffect(() => {
        // 개발 모드(StrictMode)에서 두 번 실행되는 것 방지
        if (isFetchedRef.current) return;
        isFetchedRef.current = true;

        const fetchTemplates = async () => {
            try {
                const params = {
                    packLevel: sessionStorage.getItem('currentPackLevel') || '1',
                    appliedMaterial: sessionStorage.getItem('currentMaterial') || '',
                    matType: sessionStorage.getItem('currentMatType') || '',
                    page: 1,
                    pageSize: 12 // 💡 백엔드가 페이징을 지원하도록 수정되었으므로 페이지 단위로 요청
                };

                console.log("템플릿 요청 파라미터:", params);

                const response = await axios.get('/api/Projects/template', { params });
                const resData = response.data;

                console.log("받아온 템플릿 데이터:", resData);

                // 💡 백엔드가 { items, totalCount } 형태로 주므로 items를 추출
                const items = resData.items || resData;

                if (items && items.length > 0) {
                    setTemplateList(items);
                    setSelectedItem(items[0]); // 초기값 설정
                }
            } catch (error) {
                console.error('템플릿 데이터를 불러오는 중 에러 발생:', error);
            }
        };

        fetchTemplates();
    }, []);

    return (
        <div className="container" style={{ padding: '20px' }}>

            {/* [상단 영역] 선택된 이미지의 상세 정보 */}
            <div className="top-detail-section" style={{ border: '1px solid #ddd', padding: '20px', marginBottom: '20px', borderRadius: '8px', background: '#fff' }}>
                {selectedItem ? (
                    <div style={{ display: 'flex', gap: '20px', alignItems: 'center' }}>
                        <div style={{ width: '120px', height: '120px', background: '#f0f0f0', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            {selectedItem.fileData ? (
                                <img
                                    src={`data:image/png;base64,${selectedItem.fileData}`}
                                    alt={selectedItem.fileNm}
                                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                                />
                            ) : (
                                <span>이미지 없음</span>
                            )}
                        </div>
                        <div>
                            {/* 요청하신 대로 상단 카드 내의 '플라스틱 상자 / 플라스틱' 텍스트 영역 제거됨 */}
                            <h3 style={{ margin: '0 0 8px 0' }}>{selectedItem.subject || selectedItem.packLevelNm}</h3>
                            <p style={{ margin: '0 0 8px 0', color: '#444' }}>{selectedItem.dsgnExpCon || '설명이 없습니다.'}</p>
                            <span style={{ fontSize: '12px', color: '#666' }}>
                                분류: {selectedItem.matTypeNm} / {selectedItem.appliedMaterialNm}
                            </span>
                        </div>
                    </div>
                ) : (
                    <p style={{ margin: 0, color: '#888' }}>상단에서 확인할 템플릿을 선택해 주세요.</p>
                )}
            </div>

            {/* [하단 영역] 이미지 리스트 및 테두리 박스 */}
            <div className="bottom-list-section">
                <h4 style={{ marginBottom: '10px' }}>적용 소재 및 패키지 선택</h4>

                <div style={{
                    border: '1px solid #ddd',
                    borderRadius: '8px',
                    padding: '20px',
                    minHeight: '180px',
                    background: '#fff',
                    boxSizing: 'border-box'
                }}>
                    {templateList.length > 0 ? (
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '15px' }}>
                            {templateList.map((item, index) => {
                                const isSelected = selectedItem?.packDsgnTplId === item.packDsgnTplId;

                                return (
                                    <div
                                        key={index}
                                        onClick={() => setSelectedItem(item)}
                                        style={{
                                            border: isSelected ? '2px solid #2e7d32' : '1px solid #ddd',
                                            padding: '15px',
                                            borderRadius: '8px',
                                            cursor: 'pointer',
                                            background: isSelected ? '#f4f9f4' : '#fff'
                                        }}
                                    >
                                        {/* 카드 상단의 분류명 표시 영역 (유지 또는 삭제 선택 가능) */}
                                        <div style={{ fontSize: '14px', fontWeight: 'bold', marginBottom: '8px' }}>
                                            {item.matTypeNm} / {item.appliedMaterialNm}
                                        </div>
                                        <div style={{ fontSize: '12px', color: '#555', marginBottom: '10px' }}>
                                            {item.subject}
                                        </div>
                                        <div style={{ width: '100%', height: '80px', background: '#e0e0e0', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                                            {item.fileData ? (
                                                <img
                                                    src={`data:image/png;base64,${item.fileData}`}
                                                    alt={item.fileNm}
                                                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
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
                </div>
            </div>

        </div>
    );
}

export default PackTemplatePage;