import { useState, useEffect } from 'react';
import { getMaterialProperty, getMattypes } from '../api/commonCode';
import { SaveProjectDetail, GetProjectDetail, deleteProject } from "../api/projects";
import { getCurrentCustomerId } from '../utils/memberProfile';

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
                    // 3. 프로젝트명은 project_detail 테이블에 별도 컬럼이 없어 상세 조회로는 받아올 수 없다.
                    //    프로젝트 목록에서 넘어올 때 세션에 담아 둔 이름을 우선 기본값으로 채워 둔다.
                    //    (이 값을 먼저 넣어 둬야, 아래 상세 조회가 실패해도 이름 칸이 비지 않는다)
                    setProjectName(sessionStorage.getItem('currentPrjNm') || '');

                    try {
                        // 4. 서버에 해당 프로젝트 상세 정보 조회 요청
                        //    기본사항을 한 번도 저장한 적 없는 프로젝트는 상세 정보가 없어 404가 온다.
                        //    이 경우도 정상 상황이므로 아래 catch에서 조용히 넘어가고, 위에서 넣어 둔
                        //    프로젝트명 기본값만 유지한다.
                        const detailData = await GetProjectDetail(currentPrjId, currentPackLevel);

                        if (detailData) {
                            setMaterial(detailData.appliedMaterial || '');
                            setEnv(detailData.matUse || '');
                            setMatType(detailData.matType || '');

                            sessionStorage.setItem('currentMaterial', detailData.appliedMaterial || '');
                            sessionStorage.setItem('currentEnv', detailData.matUse || '');
                            sessionStorage.setItem('currentMatType', detailData.matType || '');
                        }
                    } catch (detailError) {
                        console.warn('저장된 기본사항이 아직 없어 프로젝트명만 기본값으로 채웁니다.', detailError);
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

    // [삭제 버튼 클릭 핸들러]
    // 세션에 잡혀 있는 포장차수 하나만 지웁니다.
    // 예를 들어 1/2/3차가 있는 프로젝트에서 2차 화면에서 삭제를 누르면
    // 2차 프로젝트와 2차 기술문서(TD)/적합성선언서(DOC)만 지워지고 1차와 3차는 그대로 남습니다.
    const handleDelete = async () => {
        const currentPrjId = sessionStorage.getItem('currentPrjId');
        const currentPackLevel = sessionStorage.getItem('currentPackLevel') || '1';

        if (!currentPrjId || currentPrjId === 'DEFAULT_PRJ_ID') {
            alert('삭제할 프로젝트가 없습니다. 프로젝트 현황에서 프로젝트를 선택해 주세요.');
            return;
        }

        // 문서까지 함께 지워지는 작업이라 되돌릴 수 없으므로 한 번 더 확인합니다.
        const ok = window.confirm(
            `${currentPackLevel}차 포장 프로젝트를 삭제합니다.\n` +
            `해당 차수의 기술문서(TD)와 적합성 선언서(DOC)도 함께 삭제되며 되돌릴 수 없습니다.\n\n` +
            '삭제하시겠습니까?'
        );
        if (!ok) return;

        try {
            const result = await deleteProject(currentPrjId, currentPackLevel);
            alert(result?.message || '삭제되었습니다.');

            // 지워진 프로젝트가 세션에 남아 있으면 다음 화면에서 없는 데이터를 조회하게 되므로 정리합니다.
            ['currentPrjId', 'currentPrjNm', 'currentPackLevel', 'currentExportCountry',
                'currentMaterial', 'currentEnv', 'currentMatType', 'currentPackDsgnTplId']
                .forEach((key) => sessionStorage.removeItem(key));

            // 삭제 후에는 목록에서 결과를 바로 확인할 수 있게 프로젝트 현황으로 돌려보냅니다.
            if (typeof onSelectItem === 'function') {
                onSelectItem('project-history');
            }
        } catch (error) {
            console.error('프로젝트 삭제 실패:', error);
            alert(error?.response?.data?.message || '삭제 중 오류가 발생했습니다.');
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
                    {/* 삭제는 되돌릴 수 없는 동작이라 다른 버튼과 색을 구분해 왼쪽 끝에 둡니다. */}
                    <button
                        className="btn-secondary-line"
                        onClick={handleDelete}
                        style={{ marginRight: 'auto', color: '#dc2626', borderColor: '#fca5a5' }}
                    >
                        삭제
                    </button>
                    <button className="btn-secondary-line" onClick={() => console.log('취소 클릭')}>취소</button>
                    <button className="btn-secondary-line" onClick={handleSave} style={{ backgroundColor: '#f3f4f6' }}>저장</button>
                    <button className="btn-primary" onClick={handleNextStep}>다음단계</button>
                </div>
            </div>

            {loading && <p style={{ color: '#9ca3af', marginTop: '20px' }}>데이터를 불러오는 중...</p>}
        </div>
    );
}