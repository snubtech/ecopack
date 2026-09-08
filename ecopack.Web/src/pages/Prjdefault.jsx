import { useState, useEffect } from 'react';
import { getMaterialProperty, getMattypes, getMatForms } from '../api/commonCode';
import { SaveProjectDetail, GetProjectDetail } from "../api/projects";
import { getCurrentCustomerId } from '../utils/memberProfile';

export default function Prjdefault({ onSelectItem }) {
    const [loading, setLoading] = useState(true);

    const [materialList, setMaterialList] = useState([]);
    const [matTypesList, setMatTypesList] = useState([]);
    const [matFormsList, setMatFormsList] = useState([]); // 5번 항목 전용 목록 상태

    // 폼 입력 상태 관리
    // 💡 프로젝트명은 신규 프로젝트 작성 시 입력한 이름과 같은 값이라, 세션에 담겨 온
    //    값으로 화면이 뜨는 즉시(공통코드·상세정보 조회를 기다리지 않고) 채워 둔다.
    //    아래 useEffect의 네트워크 요청 중 하나라도 실패해도 이름 칸은 항상 채워져 있다.
    const [projectName, setProjectName] = useState(() => {
        const currentPrjId = sessionStorage.getItem('currentPrjId');
        const savedNm = sessionStorage.getItem('currentPrjNm') || '';
        return currentPrjId && currentPrjId !== 'DEFAULT_PRJ_ID' ? savedNm : (savedNm || 'Foldable EPP Box');
    });
    const [material, setMaterial] = useState('');
    const [env, setEnv] = useState('');
    const [matType, setMatType] = useState('');
    const [matForm, setMatForm] = useState('');

    const currentPackLevel = sessionStorage.getItem('currentPackLevel') || '1';

    // 1. 초기 공통 코드 로드 (소재, 포장재 종류)
    useEffect(() => {
        const initializeData = async () => {
            try {
                setLoading(true);
                const [matPropData, matTypesData] = await Promise.all([
                    getMaterialProperty(),
                    getMattypes()
                ]);

                if (matPropData) setMaterialList(matPropData);
                if (matTypesData) setMatTypesList(matTypesData);

                const currentPrjId = sessionStorage.getItem('currentPrjId');

                if (currentPrjId && currentPrjId !== 'DEFAULT_PRJ_ID') {
                    // 2. 프로젝트명은 이미 초기 상태값으로 채워져 있다(useState 초기화 함수 참고).
                    //    project_detail 테이블엔 프로젝트명 컬럼이 없어 상세 조회로는 받아올 수 없고,
                    //    기본사항을 한 번도 저장한 적 없는 프로젝트는 상세 조회가 404를 낸다.
                    //    이 경우도 정상 상황이므로 아래 catch에서 조용히 넘어간다.
                    try {
                        const detailData = await GetProjectDetail(currentPrjId, currentPackLevel);

                        if (detailData) {
                            setMaterial(detailData.appliedMaterial || '');
                            setEnv(detailData.matUse || '');
                            setMatType(detailData.matType || '');
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
            }
        };

        initializeData();
        // currentPackLevel은 세션에서 마운트 시점에 한 번만 읽어오면 되는 값이라 의도적으로 뺐다.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // 3. 적용 소재(material)나 포장재 종류(matType)가 바뀔 때마다 조건에 맞는 소재 형태(matForm) 목록을 가져옴
    useEffect(() => {
        const fetchMatForms = async () => {
            if (!material || !matType) {
                setMatFormsList([]);
                return;
            }

            try {
                const formsData = await getMatForms(currentPackLevel, material, matType);
                setMatFormsList(formsData || []);

                // 만약 기존에 선택된 matForm이 새로 바뀐 목록에 없다면 초기화
                if (!formsData.some(item => item.matForm === matForm)) {
                    setMatForm('');
                }
            } catch (error) {
                console.error('소재 형태 목록 불러오기 실패:', error);
            }
        };

        fetchMatForms();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [material, matType, currentPackLevel]);

    // 세션 스토리지 저장 로직 공통화 함수
    const saveToSessionStorage = () => {
        sessionStorage.setItem('currentPrjNm', projectName);
        sessionStorage.setItem('currentMaterial', material);
        sessionStorage.setItem('currentEnv', env);
        sessionStorage.setItem('currentMatType', matType);
        sessionStorage.setItem('currentMatForm', matForm);
    };

    // [저장 버튼 클릭 핸들러]
    const handleSave = async () => {
        saveToSessionStorage();

        const currentPrjId = sessionStorage.getItem('currentPrjId') || 'DEFAULT_PRJ_ID';

        // ⚠️ 'prjuserid' 는 로그인 정보가 통째로 JSON 문자열로 들어 있는 세션 키라
        //    그대로 보내면 값이 깨질 뿐 아니라, 세션이 비어 있을 때 'system' 같은
        //    고정 문자열로 채워지면 다른 사람과 소유자가 겹치는 사고로 이어진다.
        //    반드시 파싱된 실제 고객 ID(repCustId)를 사용한다.
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

    // 💡 삭제 버튼은 [프로젝트 현황] 표의 [관리] 열로 옮겼다 (Projects.jsx).
    //    여기서는 수정 화면에서 바로 지우다 실수로 잘못 누르는 걸 막기 위해 뺐다.

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

                {/* 5. 소재의 형태 선택 영역 (적용 소재 + 포장재 종류로 조건 필터링된 목록) */}
                <div className="form-group">
                    <label className="form-label">5. 소재의 형태를 선택해 주세요</label>
                    <select
                        id="selectMatForm"
                        className="form-select"
                        value={matForm}
                        onChange={(e) => setMatForm(e.target.value)}
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
