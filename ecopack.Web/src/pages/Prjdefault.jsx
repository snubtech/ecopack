import { useState, useEffect } from 'react';
import { getMaterialProperty, getMattypes } from '../api/commonCode';
import { getMatForms } from '../api/commonCode'; // 새로 만든 API 임포트
import { SaveProjectDetail, GetProjectDetail, deleteProject } from "../api/projects";

export default function Prjdefault({ onSelectItem }) {
    const [loading, setLoading] = useState(true);

    const [materialList, setMaterialList] = useState([]);
    const [matTypesList, setMatTypesList] = useState([]);
    const [matFormsList, setMatFormsList] = useState([]); // 5번 항목 전용 목록 상태

    const [projectName, setProjectName] = useState('');
    const [material, setMaterial] = useState('');
    const [env, setEnv] = useState('');
    const [matType, setMatType] = useState('');
    const [matForm, setMatForm] = useState('');

    const currentPackLevel = sessionStorage.getItem('currentPackLevel') || '3';

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
                    const detailData = await GetProjectDetail(currentPrjId, currentPackLevel);

                    if (detailData) {
                        setProjectName(detailData.projectName || sessionStorage.getItem('currentPrjNm') || '');
                        setMaterial(detailData.appliedMaterial || '');
                        setEnv(detailData.matUse || '');
                        setMatType(detailData.matType || '');
                        setMatForm(detailData.matForm || '');

                        sessionStorage.setItem('currentMaterial', detailData.appliedMaterial || '');
                        sessionStorage.setItem('currentEnv', detailData.matUse || '');
                        sessionStorage.setItem('currentMatType', detailData.matType || '');
                        sessionStorage.setItem('currentMatForm', detailData.matForm || '');
                    }
                } else {
                    setProjectName(sessionStorage.getItem('currentPrjNm') || 'Foldable EPP Box');
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
        const sessionUser = JSON.parse(sessionStorage.getItem('prjuserid') || '{}');
        const prjUserId = sessionUser.repCustId || '';
        const dto = {
            prjId: currentPrjId,
            packLevel: currentPackLevel,
            projectName,
            appliedMaterial: material,
            matUse: env,
            matType: matType,
            matForm: matForm,
            prjuserid: prjUserId 
        };

        try {
            await SaveProjectDetail(dto);
            alert('입력하신 정보가 저장되었습니다.');
        } catch (error) {
            console.error('저장 실패:', error);
            alert('저장 중 오류가 발생했습니다.');
        }
    };

    const handleDelete = async () => {
        const currentPrjId = sessionStorage.getItem('currentPrjId');
        if (!currentPrjId || currentPrjId === 'DEFAULT_PRJ_ID') {
            alert('삭제할 프로젝트가 없습니다.');
            return;
        }

        const ok = window.confirm(`${currentPackLevel}차 포장 프로젝트를 삭제하시겠습니까?`);
        if (!ok) return;

        try {
            const result = await deleteProject(currentPrjId, currentPackLevel);
            alert(result?.message || '삭제되었습니다.');

            ['currentPrjId', 'currentPrjNm', 'currentPackLevel', 'currentExportCountry',
                'currentMaterial', 'currentEnv', 'currentMatType', 'currentMatForm', 'currentPackDsgnTplId']
                .forEach((key) => sessionStorage.removeItem(key));

            if (typeof onSelectItem === 'function') {
                onSelectItem('project-history');
            }
        } catch (error) {
            console.error('프로젝트 삭제 실패:', error);
        }
    };

    const handleNextStep = async () => {
        saveToSessionStorage();
        if (typeof onSelectItem === 'function') {
            onSelectItem('prjtemplate');
        }
    };

    return (
        <div style={{ background: 'transparent', padding: '0px', width: '100%', height: '100%', boxSizing: 'border-box', overflowY: 'auto', margin: 0 }}>
            <div className="form-container">
                <div className="form-top-notice">
                    <div className="form-top-icon">P</div>
                    <span>안녕하세요! 항목에 관한 내용을 작성해 주세요</span>
                </div>

                <div className="form-group">
                    <label className="form-label">1. 기본 정보를 입력해 주세요</label>
                    <input
                        type="text"
                        className="form-input"
                        value={projectName}
                        onChange={(e) => setProjectName(e.target.value)}
                        placeholder="프로젝트명 (제품명)을 15글자 내외로 입력해 주세요"
                    />
                </div>

                {/* 2. 적용 소재 선택 */}
                <div className="form-group">
                    <label className="form-label">2. 적용 소재를 선택해 주세요</label>
                    <select
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

                {/* 3. 사용 환경 선택 */}
                <div className="form-group">
                    <label className="form-label">3. 사용 환경을 선택해 주세요</label>
                    <select
                        className="form-select"
                        value={env}
                        onChange={(e) => setEnv(e.target.value)}
                    >
                        <option value="">-- 사용 환경을 선택해주세요 --</option>
                        {materialList
                            .filter((item, index, self) => index === self.findIndex(t => t.matUse === item.matUse))
                            .map((item, index) => (
                                <option key={`env-${index}`} value={item.matUse}>
                                    {item.matUseNm}
                                </option>
                            ))}
                    </select>
                </div>

                {/* 4. 포장재 종류 선택 */}
                <div className="form-group">
                    <label className="form-label">4. 포장재 종류를 선택해 주세요</label>
                    <select
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

                {/* 5. 소재의 형태 선택 (조건별 필터링된 결과 연동) */}
                <div className="form-group">
                    <label className="form-label">5. 소재의 형태를 선택해 주세요</label>
                    <select
                        className="form-select"
                        value={matForm}
                        onChange={(e) => setMatForm(e.target.value)}
                        disabled={!material || !matType} // 소재와 포장재 종류가 먼저 선택되어야 활성화
                    >
                        <option value="">
                            {!material || !matType ? '-- 적용소재와 포장재 종류를 먼저 선택해주세요 --' : '-- 소재의 형태를 선택해주세요 --'}
                        </option>
                        {matFormsList.map((item, index) => (
                            <option key={`matForm-${index}`} value={item.matForm}>
                                {item.matFormNm}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="form-footer-buttons" style={{ display: 'flex', gap: '10px', justifyContent: 'flex-end' }}>
                    <button className="btn-secondary-line" onClick={handleDelete} style={{ marginRight: 'auto', color: '#dc2626', borderColor: '#fca5a5' }}>삭제</button>
                    <button className="btn-secondary-line" onClick={() => console.log('취소 클릭')}>취소</button>
                    <button className="btn-secondary-line" onClick={handleSave} style={{ backgroundColor: '#f3f4f6' }}>저장</button>
                    <button className="btn-primary" onClick={handleNextStep}>다음단계</button>
                </div>
            </div>

            {loading && <p style={{ color: '#9ca3af', marginTop: '20px' }}>데이터를 불러오는 중...</p>}
        </div>
    );
}