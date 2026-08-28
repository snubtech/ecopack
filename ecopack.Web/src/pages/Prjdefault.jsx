import { useState, useEffect } from 'react';
import axios from 'axios';
import { getMaterialProperty } from '../api/commonCode';

export default function Prjdefault() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);

    // DB에서 가져온 공통 코드 목록을 담을 상태 (소재/환경용)
    const [materialList, setMaterialList] = useState([]);

    // 폼 입력 상태 관리
    const [projectName, setProjectName] = useState('Foldable EPP Box');

    // 셀렉트 박스 상태 관리
    const [material, setMaterial] = useState('');
    const [env, setEnv] = useState('');
    const [recycling, setRecycling] = useState('PCR 30% 이상 적용');

    useEffect(() => {
        // 기존 상품 목록 조회
        axios.get('/api/products')
            .then(response => {
                setProducts(response.data);
                setLoading(false);
            })
            .catch(error => {
                console.error('에러 발생:', error);
                setLoading(false);
            });

        // 페이지가 처음 뜰 때 소재/환경 공통 코드 데이터 불러오기
        getMaterialProperty().then(data => {
            if (data) {
                setMaterialList(data);
            }
        });
    }, []);

    const handleNextStep = () => {
        // 세션 스토리지에서 필요한 정보 가져오기
        const currentCountry = sessionStorage.getItem('currentExportCountry') || '';
        const currentPackLevel = sessionStorage.getItem('currentPackLevel') || ''; // 이미 세션에 있다면 여기서 활용 가능

        const formData = {
            projectName,
            countries: currentCountry,
            packLevel: currentPackLevel, // 세션에 있는 포장 차수 사용 시 추가
            material,
            env,
            recycling
        };
        console.log('수집된 폼 데이터:', formData);
        alert('데이터가 성공적으로 수집되었습니다. 콘솔을 확인해 주세요!');
    };

    return (
        <div style={{
            background: 'transparent',
            padding: '0px',
            width: '100%',
            height: '100%',
            boxSizing: 'border-box',
            overflowY: 'auto',
            margin: 0
        }}>
            <div className="form-container">
                <div className="form-top-notice">
                    <div className="form-top-icon">P</div>
                    <span>안녕하세요! 항목에 관한 내용을 작성해 주세요</span>
                </div>

                <div className="form-group">
                    <label className="form-label">1. 기본 정보를 입력해 주세요</label>
                    <input
                        type="text"
                        id="inputProjName"
                        className="form-input"
                        value={projectName}
                        onChange={(e) => setProjectName(e.target.value)}
                        placeholder="프로젝트명 (제품명)을 15글자 내외로 입력해 주세요"
                    />
                </div>

               
                {/* 4. 적용 소재 선택 영역 (DB 데이터 연동) */}
                <div className="form-group">
                    <label className="form-label">2. 적용 소재를 선택해 주세요</label>
                    <select
                        id="selectMaterial"
                        className="form-select"
                        value={material}
                        onChange={(e) => setMaterial(e.target.value)}
                    >
                        <option value="">-- 적용소재를 선택해주세요 --</option>
                        {materialList.map((item) => (
                            <option key={item.appliedMaterial} value={item.appliedMaterial}>
                                {item.appliedMaterialNm}
                            </option>
                        ))}
                    </select>
                </div>

                {/* 5. 사용 환경 선택 영역 (DB 데이터 연동) */}
                <div className="form-group">
                    <label className="form-label">3. 사용 환경을 선택해 주세요</label>
                    <select
                        id="selectEnv"
                        className="form-select"
                        value={env}
                        onChange={(e) => setEnv(e.target.value)}
                    >
                        <option value="">-- 사용 환경을 선택해주세요 --</option>
                        {materialList
                            .filter((item, index, self) =>
                                index === self.findIndex(t => t.matUse === item.matUse)
                            )
                            .map((item, index) => (
                                <option key={`env-${index}`} value={item.matUse}>
                                    {item.matUseNm}
                                </option>
                            ))}
                    </select>
                </div>

                <div className="form-group">
                    <label className="form-label">4. 재생원료 사용 및 리사이클링 여부를 선택해 주세요</label>
                    <select
                        id="selectRecycling"
                        className="form-select"
                        value={recycling}
                        onChange={(e) => setRecycling(e.target.value)}
                    >
                        <option>PCR 30% 이상 적용</option>
                        <option>PCR 50% 이상 적용</option>
                        <option>100% 단일소재 (Monomaterial) 재활용 용이</option>
                        <option>해당 없음 / 신재 100%</option>
                    </select>
                </div>

                <div className="form-footer-buttons">
                    <button className="btn-secondary-line" onClick={() => console.log('취소 클릭')}>취소</button>
                    <button className="btn-primary" onClick={handleNextStep}>다음단계</button>
                </div>
            </div>

            {/* 서버 데이터 로딩 영역 */}
            {loading ? (
                <p style={{ color: '#9ca3af', marginTop: '20px' }}>데이터를 불러오는 중...</p>
            ) : (
                <ul style={{ color: '#374151', paddingLeft: '20px', marginTop: '20px' }}>
                    {products.length > 0 ? (
                        products.map((item, index) => (
                            <li key={index} style={{ marginBottom: '8px' }}>{item.name}</li>
                        ))
                    ) : (
                        <p style={{ color: '#9ca3af' }}></p>
                    )}
                </ul>
            )}
        </div>
    );
}