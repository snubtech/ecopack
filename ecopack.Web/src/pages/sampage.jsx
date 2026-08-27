import { useState, useEffect } from 'react';
import axios from 'axios';
//import { getIf001 } from '../api/commonCode';
// 👇 방금 만든 공용 API 함수 임포트 (파일 경로에 맞춰 조정)
import { getMaterialProperty } from '../api/commonCode';
import { getPackLevels } from '../api/commonCode';

export default function SamplePage() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);

    // DB에서 가져온 공통 코드 목록을 담을 상태
    const [materialList, setMaterialList] = useState([]);
    const [PackLevelsList, setPackLevelsList] = useState([]);

    // 폼 입력 상태 관리
    const [projectName, setProjectName] = useState('Foldable EPP Box');

    // 국가 체크박스 상태 관리
    const [countries, setCountries] = useState({
        usa: true,
        eu: false,
        china: false,
        korea: false
    });

    // 💡 3. 포장 차수 복수 선택을 위한 상태 (코드값 배열로 관리)
    const [selectedPackLevels, setSelectedPackLevels] = useState([]);

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

        // 페이지가 처음 뜰 때 if001 테이블 데이터 불러오기
        getMaterialProperty().then(data => {
            if (data) {
                setMaterialList(data);
            }
        });
        getPackLevels().then(data => {
            if (data) {
                setPackLevelsList(data);
            }
        });


    }, []);

    // 국가 체크박스 토글 함수
    const toggleCountry = (key) => {
        setCountries(prev => ({ ...prev, [key]: !prev[key] }));
    };

    // 💡 3. 포장 차수 체크박스 토글 함수 (복수 선택 처리)
    const togglePackLevel = (code) => {
        setSelectedPackLevels(prev => {
            if (prev.includes(code)) {
                // 이미 체크되어 있으면 제거 (체크 해제)
                return prev.filter(item => item !== code);
            } else {
                // 체크되어 있지 않으면 추가 (체크)
                return [...prev, code];
            }
        });
    };

    const handleNextStep = () => {
        const formData = {
            projectName,
            countries,
            packLevels: selectedPackLevels, // 💡 선택된 복수 포장 차수 코드 배열
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
                    <span>안녕하세요! 7가지 항목에 관한 내용을 작성해 주세요</span>
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

                <div className="form-group">
                    <label className="form-label">2. 제품을 수출할 국가를 선택해 주세요</label>
                    <div className="checkbox-group">
                        <label className={`custom-check-box ${countries.usa ? 'checked' : ''}`}>
                            <input
                                type="checkbox"
                                checked={countries.usa}
                                onChange={() => toggleCountry('usa')}
                            /> 미국 (USA)
                        </label>
                        <label className={`custom-check-box ${countries.eu ? 'checked' : ''}`}>
                            <input
                                type="checkbox"
                                checked={countries.eu}
                                onChange={() => toggleCountry('eu')}
                            /> 유럽 (EU)
                        </label>
                        <label className={`custom-check-box ${countries.china ? 'checked' : ''}`}>
                            <input
                                type="checkbox"
                                checked={countries.china}
                                onChange={() => toggleCountry('china')}
                            /> 중국 (China)
                        </label>
                        <label className={`custom-check-box ${countries.korea ? 'checked' : ''}`}>
                            <input
                                type="checkbox"
                                checked={countries.korea}
                                onChange={() => toggleCountry('korea')}
                            /> 대한민국 (Korea)
                        </label>
                    </div>
                </div>

                {/* 💡 3. 진단할 포장 차수 선택 영역 (DB 데이터 기반 동적 체크박스 - 복수 선택) */}
                <div className="form-group">
                    <label className="form-label">3. 진단할 포장 차수를 선택해 주세요 (복수 선택 가능)</label>
                    <div className="checkbox-group">
                        {PackLevelsList
                            // packLevel 기준으로 중복 제거하여 고유 목록만 추출
                            .filter((item, index, self) =>
                                index === self.findIndex(t => t.packLevel === item.packLevel)
                            )
                            .map((item, index) => {
                                const isChecked = selectedPackLevels.includes(item.packLevel);
                                return (
                                    <label key={`pack-${index}`} className={`custom-check-box ${isChecked ? 'checked' : ''}`}>
                                        <input
                                            type="checkbox"
                                            checked={isChecked}
                                            onChange={() => togglePackLevel(item.packLevel)}
                                        /> {item.packLevelNm}
                                    </label>
                                );
                            })}
                    </div>
                </div>

                {/* 4. 적용 소재 선택 영역 (DB 데이터 연동) */}
                <div className="form-group">
                    <label className="form-label">4. 적용 소재를 선택해 주세요</label>
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
                    <label className="form-label">5. 사용 환경을 선택해 주세요</label>
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
                    <label className="form-label">6. 재생원료 사용 및 리사이클링 여부를 선택해 주세요</label>
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