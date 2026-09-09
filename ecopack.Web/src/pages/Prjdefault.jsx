import { useState, useEffect, useRef } from 'react';
import { getMaterialProperty, getMattypes, getMatForms } from '../api/commonCode';
import { SaveProjectDetail, GetProjectDetail } from "../api/projects";
import { getCurrentCustomerId } from '../utils/memberProfile';

export default function Prjdefault({ onSelectItem }) {
    const [loading, setLoading] = useState(true);

    const [materialList, setMaterialList] = useState([]);
    const [matTypesList, setMatTypesList] = useState([]);
    const [matFormsList, setMatFormsList] = useState([]);

    const [projectName, setProjectName] = useState(() => {
        const currentPrjId = sessionStorage.getItem('currentPrjId');
        const savedNm = sessionStorage.getItem('currentPrjNm') || '';
        return currentPrjId && currentPrjId !== 'DEFAULT_PRJ_ID' ? savedNm : (savedNm || 'Foldable EPP Box');
    });

    const [material, setMaterial] = useState('');
    const [env, setEnv] = useState('');
    const [matType, setMatType] = useState('');
    const [matForm, setMatForm] = useState('');

    // 초기 로딩 중인지 체크하는 플래그 (타이밍 충돌 방지용)
    const isInitialLoading = useRef(true);

    const currentPackLevel = sessionStorage.getItem('currentPackLevel') || '1';

    // 1. 초기 공통 코드 로드 및 상세 정보 조회
    useEffect(() => {
        const initializeData = async () => {
            try {
                setLoading(true);
                isInitialLoading.current = true;

                const [matPropData, matTypesData] = await Promise.all([
                    getMaterialProperty(),
                    getMattypes()
                ]);

                if (matPropData) setMaterialList(matPropData);
                if (matTypesData) setMatTypesList(matTypesData);

                const currentPrjId = sessionStorage.getItem('currentPrjId');

                if (currentPrjId && currentPrjId !== 'DEFAULT_PRJ_ID') {
                    try {
                        const detailData = await GetProjectDetail(currentPrjId, currentPackLevel);
                        console.log("📌 데이터를 불러왔습니다.", detailData);
                        if (detailData) {
                            setMaterial(detailData.appliedMaterial || '');
                            setEnv(detailData.matUse || '');
                            setMatType(detailData.matType || '');
                            // 세션보다 서버 데이터를 먼저 확실하게 심어줌
                            setMatForm(detailData.matForm || '');

                            sessionStorage.setItem('currentMaterial', detailData.appliedMaterial || '');
                            sessionStorage.setItem('currentEnv', detailData.matUse || '');
                            sessionStorage.setItem('currentMatType', detailData.matType || '');
                            sessionStorage.setItem('currentMatForm', detailData.matForm || '');
                        }
                    } catch (detailError) {
                        console.warn('저장된 기본사항이 아직 없어 넘어갑니다.', detailError);
                    }
                } else {
                    setMaterial(sessionStorage.getItem('currentMaterial') || '');
                    setEnv(sessionStorage.getItem('currentEnv') || '');
                    setMatType(sessionStorage.getItem('currentMatType') || '');
                    setMatForm(sessionStorage.getItem('currentMatForm') || '');
                }
            } catch (error) {
                console.error('초기 데이터 로딩 에러:', error);
            } finally {
                setLoading(false);
                // 초기 로딩 완료 후 잠시 뒤 플래그 해제
                setTimeout(() => {
                    isInitialLoading.current = false;
                }, 500);
            }
        };

        initializeData();
    }, []);

    // 2. 적용 소재(material)나 포장재 종류(matType)가 바뀔 때마다 조건에 맞는 소재 형태(matForm) 목록을 가져옴
    useEffect(() => {
        const fetchMatForms = async () => {
            if (!material || !matType) {
                setMatFormsList([]);
                return;
            }

            try {
                const formsData = await getMatForms(currentPackLevel, material, matType);
                console.log("📦 받아온 소재 형태 목록:", formsData);
                setMatFormsList(formsData || []);

                // 초기 로딩 중이 아닐 때만, 목록에 현재 선택된 matForm이 없으면 초기화 수행
                if (!isInitialLoading.current && formsData) {
                    const exists = formsData.some(item => item.matForm === matForm);
                    if (!exists && matForm !== '') {
                        console.log("⚠️ 기존 선택된 형식이 목록에 없어 초기화합니다.");
                        setMatForm('');
                    }
                }
            } catch (error) {
                console.error('소재 형태 목록 불러오기 실패:', error);
            }
        };

        fetchMatForms();
    }, [material, matType, currentPackLevel]);

    const saveToSessionStorage = () => {
        sessionStorage.setItem('currentPrjNm', projectName);
        sessionStorage.setItem('currentMaterial', material);
        sessionStorage.setItem('currentEnv', env);
        sessionStorage.setItem('currentMatType', matType);
        sessionStorage.setItem('currentMatForm', matForm);
    };

    const handleSave = async () => {
        saveToSessionStorage();
        const currentPrjId = sessionStorage.getItem('currentPrjId') || 'DEFAULT_PRJ_ID';

        const dto = {
            prjId: currentPrjId,
            packLevel: currentPackLevel,
            projectName,
            appliedMaterial: material,
            matUse: env,
            matType: matType,
            matForm: matForm,
            prjuserid: getCurrentCustomerId()
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
            onSelectItem('prjtemplate');
        } else {
            console.error("onSelectItem이 함수가 아닙니다!");
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

                {/* 5. 소재의 형태 선택 영역 */}
                <div className="form-group">
                    <label className="form-label">5. 소재의 형태를 선택해 주세요</label>
                    <select
                        id="selectMatForm"
                        className="form-select"
                        value={matForm}
                        onChange={(e) => {
                            console.log("선택된 소재 형태:", e.target.value);
                            setMatForm(e.target.value);
                        }}
                        disabled={!material || !matType}
                    >
                        <option value="">
                            {!material || !matType
                                ? '-- 적용소재와 포장재 종류를 먼저 선택해주세요 --'
                                : '-- 소재의 형태를 선택해주세요 --'}
                        </option>
                        {matFormsList.map((item, index) => (
                            <option key={`matForm-${index}`} value={item.matForm}>
                                {item.matFormNm}
                            </option>
                        ))}
                    </select>
                </div>

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