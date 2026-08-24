import { useState, useEffect } from 'react';
import axios from 'axios';

export default function SamplePage() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);

    // 폼 입력 상태 관리
    const [projectName, setProjectName] = useState('Foldable EPP Box');

    // 체크박스 상태 관리 (객체 형태로 다중 선택 관리)
    const [countries, setCountries] = useState({
        usa: true,
        eu: false,
        china: false,
        korea: false
    });

    const [packagingTypes, setPackagingTypes] = useState({
        primary: true,
        secondary: false,
        tertiary: false
    });

    // 셀렉트 박스 상태 관리
    const [material, setMaterial] = useState('EPP (발포 폴리프로필렌)');
    const [env, setEnv] = useState('냉장 / 냉동 유통');
    const [recycling, setRecycling] = useState('PCR 30% 이상 적용');
    const [cert, setCert] = useState('EU PPWR 적합성 등급');

    useEffect(() => {
        axios.get('/api/products')
            .then(response => {
                setProducts(response.data);
                setLoading(false);
            })
            .catch(error => {
                console.error('에러 발생:', error);
                setLoading(false);
            });
    }, []);

    // 국가 체크박스 토글 함수
    const toggleCountry = (key) => {
        setCountries(prev => ({ ...prev, [key]: !prev[key] }));
    };

    // 포장 차수 체크박스 토글 함수
    const togglePackaging = (key) => {
        setPackagingTypes(prev => ({ ...prev, [key]: !prev[key] }));
    };

    // 다음 단계 클릭 시 데이터 확인용 함수
    const handleNextStep = () => {
        const formData = {
            projectName,
            countries,
            packagingTypes,
            material,
            env,
            recycling,
            cert
        };
        console.log('수집된 폼 데이터:', formData);
        alert('데이터가 성공적으로 수집되었습니다. 콘솔을 확인해 주세요!');
        // TODO: 여기서 폼 데이터를 서버로 보내거나 다음 페이지로 이동하는 로직을 추가합니다.
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

                <div className="form-group">
                    <label className="form-label">3. 진단할 포장 차수를 선택해 주세요</label>
                    <div className="checkbox-group">
                        <label className={`custom-check-box ${packagingTypes.primary ? 'checked' : ''}`}>
                            <input
                                type="checkbox"
                                checked={packagingTypes.primary}
                                onChange={() => togglePackaging('primary')}
                            /> 제품포장 (Primary)
                        </label>
                        <label className={`custom-check-box ${packagingTypes.secondary ? 'checked' : ''}`}>
                            <input
                                type="checkbox"
                                checked={packagingTypes.secondary}
                                onChange={() => togglePackaging('secondary')}
                            /> 운송포장 (Secondary)
                        </label>
                        <label className={`custom-check-box ${packagingTypes.tertiary ? 'checked' : ''}`}>
                            <input
                                type="checkbox"
                                checked={packagingTypes.tertiary}
                                onChange={() => togglePackaging('tertiary')}
                            /> 수송포장 (Tertiary)
                        </label>
                    </div>
                </div>

                <div className="form-group">
                    <label className="form-label">4. 적용 소재를 선택해 주세요</label>
                    <select
                        id="selectMaterial"
                        className="form-select"
                        value={material}
                        onChange={(e) => setMaterial(e.target.value)}
                    >
                        <option>EPP (발포 폴리프로필렌)</option>
                        <option>PET (폴리에틸렌 테레프탈레이트)</option>
                        <option>Bio-Paper (친환경 종이)</option>
                        <option>LDPE / HDPE</option>
                    </select>
                </div>

                <div className="form-group">
                    <label className="form-label">5. 사용 환경을 선택해 주세요</label>
                    <select
                        id="selectEnv"
                        className="form-select"
                        value={env}
                        onChange={(e) => setEnv(e.target.value)}
                    >
                        <option>상온 유통</option>
                        <option>냉장 / 냉동 유통</option>
                        <option>장기 보관 (1년 이상)</option>
                        <option>고온 살균 / 멸균 공정</option>
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

                <div className="form-group">
                    <label className="form-label">7. 목표로 하는 친환경 인증 또는 등급을 선택해 주세요</label>
                    <select
                        id="selectCert"
                        className="form-select"
                        value={cert}
                        onChange={(e) => setCert(e.target.value)}
                    >
                        <option>EU PPWR 적합성 등급</option>
                        <option>탄소 발자국 (LCA) 저감 인증</option>
                        <option>국내 친환경 포장 마크</option>
                        <option>ESG 경영 연계 친환경 인증</option>
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