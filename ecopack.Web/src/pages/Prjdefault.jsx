import { useState, useEffect } from 'react';
import { getMaterialProperty, getMattypes } from '../api/commonCode';
import { SaveProjectDetail, GetProjectDetail } from "../api/projects";

export default function Prjdefault({ onSelectItem }) {
    const [loading, setLoading] = useState(true);

    // DB에서 가져온 공통 코드 목록을 담을 상태
    const [materialList, setMaterialList] = useState([]);
    const [matTypesList, setMatTypesList] = useState([]);

    // 폼 입력 상태 관리
    const [projectName, setProjectName] = useState('');
    const [material, setMaterial] = useState('');
    const [env, setEnv] = useState('');
    const [matType, setMatType] = useState('');

    useEffect(() => {
        const initializeData = async () => {
            try {
                // 1. 공통 코드 먼저 불러오기
                const [matPropData, matTypesData] = await Promise.all([
                    getMaterialProperty(),
                    getMattypes()
                ]);

                if (matPropData) setMaterialList(matPropData);
                if (matTypesData) setMatTypesList(matTypesData);

                // 2. 세션에 저장된 프로젝트 ID와 포장 차수 가져오기
                const currentPrjId = sessionStorage.getItem('currentPrjId');
                const currentPackLevel = sessionStorage.getItem('currentPackLevel') || '1';

                if (currentPrjId && currentPrjId !== 'DEFAULT_PRJ_ID') {
                    // 3. 서버에 해당 프로젝트 상세 정보 조회 요청
                    const detailData = await GetProjectDetail(currentPrjId, currentPackLevel);

                    if (detailData) {
                        setProjectName(detailData.projectName || sessionStorage.getItem('currentPrjNm') || '');
                        setMaterial(detailData.appliedMaterial || '');
                        setEnv(detailData.matUse || '');
                        setMatType(detailData.matType || '');

                        sessionStorage.setItem('currentMaterial', detailData.appliedMaterial || '');
                        sessionStorage.setItem('currentEnv', detailData.matUse || '');
                        sessionStorage.setItem('currentMatType', detailData.matType || '');
                    }
                } else {
                    setProjectName(sessionStorage.getItem('currentPrjNm') || 'Foldable EPP Box');
                    setMaterial(sessionStorage.getItem('currentMaterial') || '');
                    setEnv(sessionStorage.getItem('currentEnv') || '');
                    setMatType(sessionStorage.getItem('currentMatType') || '');
                }

            } catch (error) {
                console.error('프로젝트 상세 정보 또는 공통 코드를 불러오는 중 에러 발생:', error);
            } finally {
                setLoading(false);
            }
        };

        initializeData();
    }, []);

    // 세션 스토리지 저장 로직 공통화 함수
    const saveToSessionStorage = () => {
        sessionStorage.setItem('currentPrjNm', projectName);
        sessionStorage.setItem('currentMaterial', material);
        sessionStorage.setItem('currentEnv', env);
        sessionStorage.setItem('currentMatType', matType);
    };

    // [저장 버튼 클릭 핸들러]
    const handleSave = async () => {
        saveToSessionStorage();

        const currentPrjId = sessionStorage.getItem('currentPrjId') || 'DEFAULT_PRJ_ID';
        const currentPackLevel = sessionStorage.getItem('currentPackLevel') || '1';

        const dto = {
            prjId: currentPrjId,
            packLevel: currentPackLevel,
            projectName,
            appliedMaterial: material,
            matUse: env,
            matType: matType,
            prjuserid: sessionStorage.getItem('prjuserid') || 'system'
        };

        try {
            const result = await SaveProjectDetail(dto);
            console.log('저장 성공 결과:', result);
            alert('입력하신 정보가 저장되었습니다.');
        } catch (error) {
            console.error('저장 실패:', error);
            alert('저장 중 오류가 발생했습니다.');
        }
    };

    const handleNextStep = async () => {
        saveToSessionStorage();
        if (typeof onSelectItem === 'function') {
            console.log("onSelectItem 함수 실행됨!");
            onSelectItem('prjtemplate');
        } else {
            console.error("onSelectItem이 함수가 아닙니다! 부모에서 전달받았는지 확인하세요.");
        }
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

                {/* 2. 적용 소재 선택 영역 */}
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

                {/* 3. 사용 환경 선택 영역 */}
                <div className="form-group">
                    <label className="form-label">3. 사용 환경을 선택해 주세요</label>
                    <select
                        id="selectEnv"
                        className="form-select"
                        value={env}
                        onChange={(e) => setEnv(e.target.value)}>
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

                {/* 4. 포장재 종류 선택 영역 */}
                <div className="form-group">
                    <label className="form-label">4. 포장재 종류를 선택해 주세요</label>
                    <select
                        id="selectmattype"
                        className="form-select"
                        value={matType}
                        onChange={(e) => setMatType(e.target.value)}
                    >
                        <option value="">-- 포장재 종류를 선택해주세요 --</option>
                        {matTypesList.map((item, index) => (
                            <option key={`matType-${index}`} value={item.matType}>
                                {item.matTypeNm}
                            </option>
                        ))}
                    </select>
                </div>

                {/* 하단 버튼 영역 */}
                <div className="form-footer-buttons" style={{ display: 'flex', gap: '10px', justifyContent: 'flex-end' }}>
                    <button className="btn-secondary-line" onClick={() => console.log('취소 클릭')}>취소</button>
                    <button className="btn-secondary-line" onClick={handleSave} style={{ backgroundColor: '#f3f4f6' }}>저장</button>
                    <button className="btn-primary" onClick={handleNextStep}>다음단계</button>
                </div>
            </div>

            {loading && <p style={{ color: '#9ca3af', marginTop: '20px' }}>데이터를 불러오는 중...</p>}
        </div>
    );
}