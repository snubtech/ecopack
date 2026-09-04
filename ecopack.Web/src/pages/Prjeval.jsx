/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - PrjevalPage 컴포넌트
 * ==============================================================================
 * 
 * 1. 초기 데이터 로드 (useEffect)
 *    - 컴포넌트가 처음 실행되면 세션에 저장된 포장 차수(packLevel)와 소재(appliedMaterial)를 읽습니다.
 *    - 서버에서 1차원 배열(Flat Data) 형태의 평가지 문항 원본 데이터를 비동기로 가져옵니다.
 * 
 * 2. 데이터 구조 변환 (transformEvalData)
 *    - 서버에서 받아온 흩어진 원본 데이터를 화면 렌더링이 쉽도록 
 *      '1단계(최상위) -> 2단계(하위) -> 3단계(심층)'의 3단 트리 구조(Tree)로 조립합니다.
 * 
 * 3. 사용자 인터랙션 및 상태 관리 (handleOptionChange)
 *    - 사용자가 화면의 라디오 버튼(보기/선택지)을 클릭하면, 
 *      'answers' 상태값에 어떤 질문에 어떤 보기를 골랐는지 기록합니다.
 * 
 * 4. 데이터 저장 및 제출 가공 (createEvalSaveList)
 *    - 사용자가 [저장] 또는 [다음] 버튼을 누를 때 실행됩니다.
 *    - 사용자가 선택한 답변들과 하위 문항들의 점수, 규제 코멘트 등을 모두 합산하여 
 *      서버로 전송할 최종 리스트(Payload) 형태로 깨끗하게 가공합니다.
 * 
 * 5. 화면 렌더링 (JSX)
 *    - 조립된 'evalData'를 바탕으로 테이블을 그리고, 
 *      1단계 보기 선택에 따라 2단계, 3단계 하위 문항들이 동적으로 나타나도록 구성되어 있습니다.
 * ==============================================================================
 */
/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 유지보수 가이드] - PrjevalPage 컴포넌트
 * ==============================================================================
 * 
 * 1. 프로그램 주요 흐름
 *    - ① 초기 데이터 로드: 컴포넌트 마운트 시 세션 정보(차수, 소재)를 읽어 서버에서 평가지 원본 데이터를 가져옵니다.
 *    - ② 데이터 구조 변환: 1차원 배열(Flat Data) 형태의 원본을 3단계 트리 구조(Tree)로 조립합니다.
 *    - ③ 사용자 인터랙션: 사용자가 라디오 버튼(보기/선택지)을 고르면 'answers' 상태가 갱신됩니다.
 *    - ④ 데이터 저장/제출: [저장] 또는 [다음] 버튼 클릭 시 선택한 답변과 하위 점수를 합산해 서버 전송용 리스트를 만듭니다.
 * 
 * 2. 주요 변수 및 상태 (State & Ref)
 *    - evalData         : 화면 렌더링에 사용되는 3단계 트리 구조의 평가지 데이터 객체
 *    - answers          : 사용자가 선택한 답변 상태 ({ [질문ID]: 선택된보기ID })
 *    - loading          : 데이터를 불러오는 동안 화면 로딩 상태를 제어하는 불리언(Boolean) 값
 *    - isFetchingRef    : API가 중복으로 호출되는 것을 방지하기 위한 락(Lock) Ref 변수
 *    - prjId / packLevel / appliedMaterial / prjUserId : 세션에서 가져오는 프로젝트 및 환경 설정값
 * 
 * 3. 핵심 함수 목록
 *    - transformEvalData(rawData)    : 1차원 원본 데이터를 3단계(최상위-하위-심층) 계층 구조로 변환
 *    - parseOptionData(row)          : 반복되는 보기(선택지) 객체를 일관된 형태로 만들어주는 헬퍼 함수
 *    - handleOptionChange(qId, oId)  : 사용자가 라디오 버튼을 선택했을 때 답변 상태(answers)를 업데이트
 *    - createEvalSaveList()          : 사용자의 선택과 하위 문항 점수/코멘트를 모두 모아 서버 전송용 데이터로 가공
 *    - handleSave() / handleNext()   : 임시 저장 및 다음 단계 이동 버튼 클릭 시 비즈니스 로직 처리 핸들러
 * ==============================================================================
 */


import { useState, useEffect, useRef } from 'react';
// import { useNavigate } from 'react-router-dom';
import { getLatestEvalQuestions } from '../api/projects';

function PrjevalPage({ onSelectItem }) {
    // 상태 관리: 평가지 데이터, 사용자 답변 상태, 로딩 상태
    const [evalData, setEvalData] = useState(null);
    const [answers, setAnswers] = useState({});
    const [loading, setLoading] = useState(true);

    // 세션 스토리지에서 프로젝트 및 패키지 환경 정보 로드
    const prjId = sessionStorage.getItem('currentPrjId') || '';
    const packLevel = sessionStorage.getItem('currentPackLevel') || '1';
    const appliedMaterial = sessionStorage.getItem('currentMaterial') || '';
    const prjUserId = sessionStorage.getItem('currentUserId') || 'system_user'; // 사용자 ID 확보

    // API 중복 호출 방지용 락(Lock) Ref
    const isFetchingRef = useRef(false);

    /**
     * [데이터 변환 함수: Flat Array -> 3단계 계층 구조(Tree)로 변환]
     * 서버에서 받아오는 원본 데이터(rawData)는 모든 질문, 보기, 하위 문항들이 
     * 한 줄씩(1차원 배열 형태) 풀헤쳐져서 옵니다. 
     * 이를 화면에 계층적으로 그리기 위해 1단계(최상위) -> 2단계(하위) -> 3단계(심층 하위) 구조로 조립합니다.
     */
    const transformEvalData = (rawData) => {
        // 1. 데이터가 배열 형식이 아니라면 에러 방지를 위해 빈 구조를 리턴합니다.
        if (!Array.isArray(rawData)) return { questions: [] };

        const questionsList = []; // 최종 완성될 트리 구조의 질문 목록 배열
        const questionMap = new Map(); // 중복 질문 등록을 방지하기 위한 Map 객체 (빠른 검색용)

        /**
         * [헬퍼 함수: 보기(Option) 객체 생성기]
         * 반복되는 보기 데이터 매핑 코드를 줄이기 위해, 행 데이터에서 필요한 속성을 추출해 객체로 만들어줍니다.
         */
        const parseOptionData = (row) => ({
            optionId: row.asmtQstItemId,         // 보기 고유 ID
            optionText: row.asmtQstItemNm,       // 보기 텍스트 (라벨 이름)
            score: row.scoringCriteria ?? row.score ?? 0, // 보기 배점 (없으면 0점)
            natRglAls: row.natRglAls || '',      // 국가 규제 분석 내용
            dsgnRecmImp: row.dsgnRecmImp || '',  // 디자인 개선 추천 내용
            subQuestions: []                     // 이 보기를 선택했을 때 나타날 하위 문항 배열 (초기엔 빈 배열)
        });

        // -------------------------------------------------------------------------
        // [1단계 작업] 최상위 질문(부모 ID가 없는 항목)과 그에 속한 기본 보기(옵션)들을 먼저 매핑합니다.
        // -------------------------------------------------------------------------
        rawData.forEach(row => {
            // prtAsmtQstItemId(부모 보기 ID)가 없다면 최상위 질문이라는 뜻입니다.
            if (!row.prtAsmtQstItemId) {

                // 아직 등록되지 않은 새로운 질문ID라면, 질문 객체를 새로 만들어 목록에 추가합니다.
                if (!questionMap.has(row.asmtQstId)) {
                    // 서버 데이터마다 키 이름이 다를 수 있으므로 안전하게 여러 후보 필드에서 값을 찾습니다.
                    const rawAreaCode = row.ecoPackLarType || row.ecopacklartype || row.ecoPackArea || row.ecopackarea || row.asmtAreaCode || '';
                    const rawMaterial = row.appliedMaterial || row.appliedmaterial || row.material || row.packageMaterial || appliedMaterial || '';
                    const rawAreaName = row.ecoPackAreaNm || row.ecopackareaynm || row.ecoPackLarTypeNm || row.ecopacklartypenm || row.ecoPackArea || row.ecopackarea || row.areaName || row.asmtAreaNm || '';

                    const newQuestion = {
                        questionId: row.asmtQstId,       // 질문 ID
                        questionTitle: row.asmtQstNm,     // 질문 제목
                        areaCode: rawAreaCode,           // 영역 코드
                        material: rawMaterial,           // 적용 소재
                        areaName: rawAreaName,           // 영역 대분류 명칭
                        options: []                      // 1단계 보기 목록
                    };
                    // 1. Map 객체에 질문 ID를 키로 등록하여, 나중에 중복으로 들어오는지 빠르게 검사할 수 있게 합니다.
                    questionMap.set(row.asmtQstId, newQuestion);
                    // 2. 화면에 순서대로 렌더링하기 위해 최종 질문 목록 배열(questionsList)에도 담아줍니다.
                    questionsList.push(newQuestion);
                }

                // 해당 최상위 질문에 딸린 보기(Option) 데이터가 있다면 추가합니다.
                if (row.asmtQstItemId) {
                    const currentQuestion = questionMap.get(row.asmtQstId);
                    // 이미 추가된 중복 보기인지 검사 후 아닐 때만 삽입
                    const existsOption = currentQuestion.options.some(opt => opt.optionId === row.asmtQstItemId);
                    if (!existsOption) {
                        currentQuestion.options.push(parseOptionData(row));
                    }
                }
            }
        });

        // -------------------------------------------------------------------------
        // [2단계 및 3단계 작업] 부모 보기 ID(prtAsmtQstItemId)가 존재하는 하위 종속 문항들을 알맞은 위치에 끼워 넣습니다.
        // -------------------------------------------------------------------------
        rawData.forEach(row => {
            if (row.prtAsmtQstItemId) { // 부모 ID가 존재하므로 하위 문항입니다.

                // -----------------------------------------------------------------
                // 탐색 시나리오 A: 이 하위 문항의 부모가 '1단계 보기'인 경우 (즉, 2단계 문항)
                // -----------------------------------------------------------------
                for (let question of questionsList) {
                    const parentOption = question.options.find(opt => opt.optionId === row.prtAsmtQstItemId);

                    if (parentOption) { // 부모 보기를 찾았다면!
                        let subQ = parentOption.subQuestions.find(sq => sq.subQuestionId === row.asmtQstId);

                        // 아직 해당 2단계 문항이 배열에 없으면 새로 생성해서 넣습니다.
                        if (!subQ) {
                            subQ = {
                                subQuestionId: row.asmtQstId,
                                subQuestionTitle: row.asmtQstNm,
                                options: [] // 2단계 문항의 보기들
                            };
                            parentOption.subQuestions.push(subQ);
                        }

                        // 2단계 문항 안의 보기(옵션) 추가
                        if (row.asmtQstItemId) {
                            const existsSubOpt = subQ.options.some(opt => opt.optionId === row.asmtQstItemId);
                            if (!existsSubOpt) {
                                subQ.options.push(parseOptionData(row));
                            }
                        }
                        return; // 찾아서 처리했으므로 현재 row 반복문 탈출
                    }
                }

                // -----------------------------------------------------------------
                // 탐색 시나리오 B: 이 하위 문항의 부모가 '2단계 보기'인 경우 (즉, 3단계 심층 문항)
                // -----------------------------------------------------------------
                for (let question of questionsList) {
                    for (let option of question.options) {
                        for (let subQ of option.subQuestions || []) {
                            // 2단계 문항의 보기 중에서 부모 ID와 일치하는 것을 찾습니다.
                            const parentSubOpt = subQ.options.find(subOpt => subOpt.optionId === row.prtAsmtQstItemId);

                            if (parentSubOpt) { // 2단계 부모 보기를 찾았다면!
                                if (!parentSubOpt.subQuestions) {
                                    parentSubOpt.subQuestions = [];
                                }

                                let deepSubQ = parentSubOpt.subQuestions.find(dsq => dsq.subQuestionId === row.asmtQstId);

                                // 3단계 심층 문항이 아직 없다면 새로 생성
                                if (!deepSubQ) {
                                    deepSubQ = {
                                        subQuestionId: row.asmtQstId,
                                        subQuestionTitle: row.asmtQstNm,
                                        options: []
                                    };
                                    parentSubOpt.subQuestions.push(deepSubQ);
                                }

                                // 3단계 문항 안의 보기(옵션) 추가
                                if (row.asmtQstItemId) {
                                    const existsDeepOpt = deepSubQ.options.some(opt => opt.optionId === row.asmtQstItemId);
                                    if (!existsDeepOpt) {
                                        deepSubQ.options.push({
                                            optionId: row.asmtQstItemId,
                                            optionText: row.asmtQstItemNm,
                                            score: row.scoringCriteria ?? row.score ?? 0,
                                            natRglAls: row.natRglAls || '',
                                            dsgnRecmImp: row.dsgnRecmImp || ''
                                        });
                                    }
                                }
                                return; // 처리 완료 후 탈출
                            }
                        }
                    }
                }
            }
        });

        // 최종 완성된 3단 계층 트리 구조 배열을 객체 형태로 반환합니다[cite: 1].
        return { questions: questionsList };
    };

    // 컴포넌트 마운트 시 최신 평가지 문항 데이터 비동기 로드
    useEffect(() => {
        const fetchEvalQuestions = async () => {
            if (isFetchingRef.current) return;
            isFetchingRef.current = true;

            try {
                setLoading(true);
                const data = await getLatestEvalQuestions(packLevel, appliedMaterial);
                const formattedData = transformEvalData(data);
                setEvalData(formattedData);
            } catch (error) {
                console.error('평가지 문항을 불러오는 중 에러 발생:', error);
                alert('평가지 데이터를 불러오지 못했습니다. 이전 단계를 확인해 주세요.');
            } finally {
                setLoading(false);
                isFetchingRef.current = false;
            }
        };

        fetchEvalQuestions();
    }, [packLevel, appliedMaterial]);

    // 라디오 버튼 선택 시 사용자의 선택 상태(answers)를 업데이트하는 핸들러
    const handleOptionChange = (questionId, optionId) => {
        setAnswers(prev => ({
            ...prev,
            [questionId]: optionId
        }));
    };

    // 이전 단계로 이동 (취소 버튼 클릭 시)
    const handleCancel = () => {
        if (window.confirm('작성 중인 내용이 저장되지 않을 수 있습니다. 이전 단계로 이동하시겠습니까?')) {
            if (typeof onSelectItem === 'function') {
                onSelectItem('prjtemplate');
            }
        }
    };

    /**
     * [저장용 데이터 생성 함수]
     * 화면에 보이는 테이블의 각 행(Row) 기준에 맞춰, 
     * 사용자가 선택한 답변과 하위 단계(2, 3차)에서 얻은 점수 및 코멘트를 모두 모아서 
     * 서버로 전송할 리스트(EvalSaveList) 형태로 가공합니다.
     */
    const createEvalSaveList = () => {
        const evalSaveList = [];

        // 데이터가 없으면 빈 배열 반환
        if (!evalData || !evalData.questions) return evalSaveList;

        // 1단계 최상위 질문들을 하나씩 순회합니다.
        evalData.questions.forEach((question) => {
            // 사용자가 이 질문에 대해 선택한 보기 ID를 가져옵니다.
            const selectedOptId = answers[question.questionId];

            // 보기를 선택했고, 보기 목록이 존재할 경우에만 처리합니다.
            if (selectedOptId && question.options) {
                const option = question.options.find(opt => opt.optionId === selectedOptId);

                if (option) {
                    // 기본 1단계에서 얻은 점수와 코멘트로 초기화합니다.
                    let totalScore = parseFloat(option.score || 0);
                    let finalNatRglAls = option.natRglAls || '';
                    let finalDsgnRecmImp = option.dsgnRecmImp || '';

                    /**
                     * [재귀 함수] 2단계, 3단계 하위 문항이 있다면 파고들어가서 
                     * 점수를 누적하고 최신 코멘트를 갱신합니다.
                     */
                    const accumulateSubAnswers = (subQuestions) => {
                        if (!subQuestions) return;

                        subQuestions.forEach((subQ) => {
                            // 하위 문항에서 사용자가 선택한 보기 ID 확인
                            const subOptId = answers[subQ.subQuestionId];
                            if (subOptId && subQ.options) {
                                const subOpt = subQ.options.find(so => so.optionId === subOptId);
                                if (subOpt) {
                                    // 하위 문항 점수를 총점에 더해줍니다.
                                    totalScore += parseFloat(subOpt.score || 0);

                                    // 코멘트가 존재하면 최신 내용으로 덮어씁니다.
                                    if (subOpt.natRglAls) finalNatRglAls = subOpt.natRglAls;
                                    if (subOpt.dsgnRecmImp) finalDsgnRecmImp = subOpt.dsgnRecmImp;

                                    // 더 깊은 단계(3단계 등)가 남아있다면 재귀 호출로 계속 탐색합니다.
                                    if (subOpt.subQuestions && subOpt.subQuestions.length > 0) {
                                        accumulateSubAnswers(subOpt.subQuestions);
                                    }
                                }
                            }
                        });
                    };

                    // 1단계 보기 아래에 하위 문항들이 있다면 탐색을 시작합니다.
                    if (option.subQuestions && option.subQuestions.length > 0) {
                        accumulateSubAnswers(option.subQuestions);
                    }

                    // 화면의 한 행(Row)과 1:1로 대응되는 최종 서버 전송용 객체를 완성하여 배열에 담습니다.
                    evalSaveList.push({
                        prjid: prjId,                        // 프로젝트 고유 ID
                        prjuserid: prjUserId,                // 사용자 ID
                        packLevel: packLevel,                // 포장 차수 (예: 1)
                        packLevelnm: `판매(${packLevel}차)`, // 포장 차수 명칭
                        appliedMaterial: appliedMaterial,    // 적용된 포장 소재
                        ecoPackLarType: question.areaCode || 'DEFAULT', // 영역 코드 (SAFETY 등)
                        ecoPackAreaNm: question.areaName || '',         // 영역 대분류 명칭
                        asmtShtHdrId: question.asmtShtHdrId || '',      // 평가 시트 헤더 ID
                        asmtQstId: question.questionId,      // 질문 ID
                        asmtQstItemId: option.optionId,      // 선택된 보기 ID
                        prtAsmtQstItemId: null,              // 1단계이므로 부모 보기 ID는 없음
                        asmtQstNm: question.questionTitle,   // 질문 제목
                        asmtQstItemNm: option.optionText,    // 보기 텍스트
                        scoringCriteria: String(totalScore), // 하위 점수까지 모두 합산된 기준/배점 점수
                        asmtpoint: String(totalScore),       // 하위 점수까지 모두 합산된 사용자 획득 점수
                        natRglAls: finalNatRglAls,           // 최종 정리된 규제 분석 내용
                        dsgn_recm_imp: finalDsgnRecmImp      // 최종 정리된 개선 방안 내용
                    });
                }
            }
        });

        return evalSaveList; // 완성된 저장 리스트 반환
    };

    // [임시 저장 버튼 클릭 핸들러]
    const handleSave = async () => {
        if (!prjId) {
            alert('프로젝트 ID(prjId)를 찾을 수 없습니다.');
            return;
        }

        try {
            // 저장용 데이터 리스트 생성
            const payload = createEvalSaveList();
            console.log("저장할 평가 리스트 데이터:", payload);

            // TODO: 백엔드 API 연동 위치 (예: await saveEvalResults(payload);)

            alert(`성공적으로 임시 저장되었습니다. (총 ${payload.length}개 항목)`);
        } catch (error) {
            console.error('저장 중 에러 발생:', error);
            alert('저장 중 오류가 발생했습니다.');
        }
    };

    // [다음 단계 이동 버튼 클릭 핸들러]
    const handleNext = async () => {
        try {
            // 제출용 데이터 리스트 생성
            const payload = createEvalSaveList();
            console.log("최종 제출 평가 리스트 데이터:", payload);

            // TODO: 백엔드 API 연동 위치 (예: await submitEvalResults(payload);)

            alert('평가가 완료되었습니다. 다음 단계로 이동합니다.');
            if (typeof onSelectItem === 'function') {
                onSelectItem('nextStepKey');
            }
        } catch (error) {
            console.error('다음 단계 이동 중 에러 발생:', error);
        }
    };

    // 로딩 중일 때 보여줄 화면
    if (loading) {
        return (
            <div style={{ textAlign: 'center', padding: '80px', color: '#666', fontSize: '15px' }}>
                평가지 문항을 불러오는 중입니다...
            </div>
        );
    }

    return (
        <div className="container" style={{ padding: '20px', boxSizing: 'border-box', width: '100%', background: '#f4f6f8', minHeight: '100vh', fontFamily: 'Inter, sans-serif' }}>

            {/* 상단 타이틀 및 우측 액션 버튼 그룹 섹션 */}
            <div className="top-header-section" style={{ border: '1px solid #d0d7de', padding: '16px 20px', marginBottom: '16px', borderRadius: '8px', background: '#fff', display: 'flex', justifyContent: 'space-between', alignItems: 'center', boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}>
                <div>
                    <h3 style={{ margin: '0 0 4px 0', color: '#1f2328', fontSize: '16px' }}>친환경 패키지 평가 설문</h3>
                    <p style={{ margin: 0, fontSize: '12px', color: '#57606a' }}>
                        포장 차수: {packLevel}차 / 적용 소재: {appliedMaterial || '전체'}
                    </p>
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '6px', flexShrink: 0 }}>
                    <button
                        onClick={handleCancel}
                        style={{ padding: '4px 12px', border: '1px solid #d0d7de', borderRadius: '4px', background: '#fff', cursor: 'pointer', width: '75px', fontSize: '12px' }}
                    >
                        취소
                    </button>
                    <button
                        onClick={handleSave}
                        style={{ padding: '4px 12px', border: 'none', borderRadius: '4px', background: '#0969da', color: '#fff', cursor: 'pointer', width: '75px', fontSize: '12px' }}
                    >
                        저장
                    </button>
                    <button
                        onClick={handleNext}
                        style={{ padding: '4px 12px', border: 'none', borderRadius: '4px', background: '#1f883d', color: '#fff', cursor: 'pointer', width: '75px', fontSize: '12px' }}
                    >
                        다음
                    </button>
                </div>
            </div>

            {/* 테이블형 평가지 레이아웃 컨테이너 */}
            <div style={{
                background: '#fff',
                border: '1px solid #d0d7de',
                borderRadius: '8px',
                overflow: 'hidden',
                boxShadow: '0 1px 3px rgba(0,0,0,0.05)'
            }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px', tableLayout: 'fixed' }}>
                    <thead>
                        <tr style={{ background: '#24292f', color: '#fff', height: '34px' }}>
                            <th style={{ padding: '0 8px', width: '90px', borderRight: '1px solid #383f46' }}>포장차수</th>
                            <th style={{ padding: '0 8px', width: '100px', borderRight: '1px solid #383f46' }}>영역코드</th>
                            <th style={{ padding: '0 8px', width: '140px', borderRight: '1px solid #383f46' }}>질문대분류</th>
                            <th style={{ padding: '0 8px' }}>질문 및 답항 계층 구조</th>
                        </tr>
                    </thead>
                    <tbody>
                        {evalData && evalData.questions && evalData.questions.length > 0 ? (
                            evalData.questions.map((question, qIndex) => {
                                const selectedOptionId = answers[question.questionId];
                                const selectedOption = question.options?.find(opt => opt.optionId === selectedOptionId);
                                const hasSubQuestions = selectedOption && selectedOption.subQuestions && selectedOption.subQuestions.length > 0;

                                return (
                                    <tr key={question.questionId || qIndex} style={{ borderBottom: '1px solid #d0d7de', background: qIndex % 2 === 0 ? '#fff' : '#f8f9fa' }}>
                                        {/* 포장차수 열 */}
                                        <td style={{ padding: '6px 8px', fontWeight: 'bold', color: '#24292f', verticalAlign: 'top', borderRight: '1px solid #e1e4e8' }}>
                                            판매({packLevel}차)
                                        </td>
                                        {/* 영역코드 열 */}
                                        <td style={{ padding: '6px 8px', fontWeight: 'bold', color: '#24292f', verticalAlign: 'top', borderRight: '1px solid #e1e4e8' }}>
                                            {question.areaCode}
                                        </td>
                                        {/* 질문대분류 열 */}
                                        <td style={{ padding: '6px 8px', color: '#24292f', verticalAlign: 'top', borderRight: '1px solid #e1e4e8' }}>
                                            {question.areaName}
                                        </td>
                                        {/* 질문 및 답항 계층 구조 열 */}
                                        <td style={{ padding: '6px 8px', verticalAlign: 'top' }}>

                                            {/* 1단계 질문 및 옵션 */}
                                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '12px', width: '100%', minHeight: '24px' }}>
                                                <div style={{ color: '#1f2328', lineHeight: '1.3', flex: '1 1 auto', minWidth: '0' }}>
                                                    <strong>Q.</strong> {question.questionTitle}
                                                </div>

                                                <div style={{ display: 'flex', gap: '6px', flexShrink: 0, flexWrap: 'wrap', justifyContent: 'flex-end' }}>
                                                    {question.options && question.options.map((option) => {
                                                        const isChecked = selectedOptionId === option.optionId;
                                                        return (
                                                            <label
                                                                key={option.optionId}
                                                                style={{
                                                                    display: 'inline-flex',
                                                                    alignItems: 'center',
                                                                    cursor: 'pointer',
                                                                    background: '#fff',
                                                                    padding: '1px 5px',
                                                                    border: '1px solid #d0d7de',
                                                                    borderRadius: '4px',
                                                                    whiteSpace: 'nowrap',
                                                                    fontSize: '12px'
                                                                }}
                                                            >
                                                                <input
                                                                    type="radio"
                                                                    name={`question_${question.questionId}`}
                                                                    value={option.optionId}
                                                                    checked={isChecked}
                                                                    onChange={() => handleOptionChange(question.questionId, option.optionId)}
                                                                    style={{ marginRight: '3px', cursor: 'pointer' }}
                                                                />
                                                                <span style={{ marginRight: '3px' }}>{option.optionText}</span>
                                                                {option.score !== undefined && (
                                                                    <span style={{ fontSize: '11px', fontWeight: 'bold', color: Number(option.score) > 0 ? '#1f883d' : '#cf222e' }}>
                                                                        {option.score}
                                                                    </span>
                                                                )}
                                                            </label>
                                                        );
                                                    })}
                                                </div>
                                            </div>

                                            {/* 2단계 하위 문항 */}
                                            {hasSubQuestions && (
                                                <div style={{ marginTop: '5px', padding: '6px 8px', background: '#f1f3f5', borderRadius: '6px', borderLeft: '3px solid #0969da', border: '1px solid #d8dee4' }}>
                                                    <div style={{ fontSize: '11px', color: '#57606a', marginBottom: '4px', fontWeight: 'bold' }}>
                                                        → [{selectedOption.optionText}] 선택 시 (2차 문항)
                                                    </div>
                                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
                                                        {selectedOption.subQuestions.map((subQ) => {
                                                            const selectedSubOptId = answers[subQ.subQuestionId];
                                                            const selectedSubOpt = subQ.options?.find(subOpt => subOpt.optionId === selectedSubOptId);
                                                            const hasDeepSubQuestions = selectedSubOpt && selectedSubOpt.subQuestions && selectedSubOpt.subQuestions.length > 0;

                                                            return (
                                                                <div key={subQ.subQuestionId} style={{ display: 'flex', flexDirection: 'column', gap: '6px', background: '#fff', padding: '6px 8px', borderRadius: '4px', border: '1px solid #d0d7de' }}>

                                                                    <div style={{ fontSize: '12px', color: '#1f2328', lineHeight: '1.4', fontWeight: '500', width: '100%' }}>
                                                                        • {subQ.subQuestionTitle}
                                                                    </div>

                                                                    <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', justifyContent: 'flex-start' }}>
                                                                        {subQ.options.map((subOpt) => {
                                                                            const isSubChecked = selectedSubOptId === subOpt.optionId;
                                                                            return (
                                                                                <label
                                                                                    key={subOpt.optionId}
                                                                                    style={{
                                                                                        display: 'inline-flex',
                                                                                        alignItems: 'center',
                                                                                        cursor: 'pointer',
                                                                                        fontSize: '11px',
                                                                                        background: '#fff',
                                                                                        padding: '2px 6px',
                                                                                        border: '1px solid #d0d7de',
                                                                                        borderRadius: '4px',
                                                                                        whiteSpace: 'nowrap'
                                                                                    }}
                                                                                >
                                                                                    <input
                                                                                        type="radio"
                                                                                        name={`sub_question_${subQ.subQuestionId}`}
                                                                                        value={subOpt.optionId}
                                                                                        checked={isSubChecked}
                                                                                        onChange={() => handleOptionChange(subQ.subQuestionId, subOpt.optionId)}
                                                                                        style={{ marginRight: '4px', cursor: 'pointer' }}
                                                                                    />
                                                                                    <span style={{ marginRight: '4px' }}>{subOpt.optionText}</span>
                                                                                    {subOpt.score !== undefined && (
                                                                                        <span style={{ fontSize: '10px', fontWeight: 'bold', color: Number(subOpt.score) > 0 ? '#1f883d' : '#cf222e' }}>
                                                                                            {subOpt.score}
                                                                                        </span>
                                                                                    )}
                                                                                </label>
                                                                            );
                                                                        })}
                                                                    </div>

                                                                    {/* 3단계 하위 문항 */}
                                                                    {hasDeepSubQuestions && (
                                                                        <div style={{ marginTop: '5px', padding: '6px 8px', background: '#eef2f5', borderRadius: '4px', borderLeft: '3px solid #1f883d', border: '1px solid #cbd5e1' }}>
                                                                            <div style={{ fontSize: '11px', color: '#475569', marginBottom: '4px', fontWeight: 'bold' }}>
                                                                                ↳ [{selectedSubOpt.optionText}] 선택 시 (3차 문항)
                                                                            </div>
                                                                            <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
                                                                                {selectedSubOpt.subQuestions.map((deepQ) => (
                                                                                    <div key={deepQ.subQuestionId} style={{ display: 'flex', flexDirection: 'column', gap: '6px', background: '#fff', padding: '6px 8px', borderRadius: '4px', border: '1px solid #d0d7de' }}>

                                                                                        <div style={{ fontSize: '11px', color: '#1f2328', fontWeight: '500', width: '100%', lineHeight: '1.4' }}>
                                                                                            - {deepQ.subQuestionTitle}
                                                                                        </div>

                                                                                        <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', justifyContent: 'flex-start' }}>
                                                                                            {deepQ.options.map((deepOpt) => {
                                                                                                const isDeepChecked = answers[deepQ.subQuestionId] === deepOpt.optionId;
                                                                                                return (
                                                                                                    <label
                                                                                                        key={deepOpt.optionId}
                                                                                                        style={{
                                                                                                            display: 'inline-flex',
                                                                                                            alignItems: 'center',
                                                                                                            cursor: 'pointer',
                                                                                                            fontSize: '11px',
                                                                                                            background: '#fff',
                                                                                                            padding: '2px 6px',
                                                                                                            border: '1px solid #d0d7de',
                                                                                                            borderRadius: '4px',
                                                                                                            whiteSpace: 'nowrap'
                                                                                                        }}
                                                                                                    >
                                                                                                        <input
                                                                                                            type="radio"
                                                                                                            name={`deep_question_${deepQ.subQuestionId}`}
                                                                                                            value={deepOpt.optionId}
                                                                                                            checked={isDeepChecked}
                                                                                                            onChange={() => handleOptionChange(deepQ.subQuestionId, deepOpt.optionId)}
                                                                                                            style={{ marginRight: '4px', cursor: 'pointer' }}
                                                                                                        />
                                                                                                        <span style={{ marginRight: '4px' }}>{deepOpt.optionText}</span>
                                                                                                        {deepOpt.score !== undefined && (
                                                                                                            <span style={{ fontSize: '10px', fontWeight: 'bold', color: Number(deepOpt.score) > 0 ? '#1f883d' : '#cf222e' }}>
                                                                                                                {deepOpt.score}
                                                                                                            </span>
                                                                                                        )}
                                                                                                    </label>
                                                                                                );
                                                                                            })}
                                                                                        </div>

                                                                                    </div>
                                                                                ))}
                                                                            </div>
                                                                        </div>
                                                                    )}

                                                                </div>
                                                            );
                                                        })}
                                                    </div>
                                                </div>
                                            )}
                                        </td>
                                    </tr>
                                );
                            })
                        ) : (
                            <tr>
                                <td colSpan="4" style={{ textAlign: 'center', padding: '30px 0', color: '#666' }}>
                                    등록된 평가 문항이 없습니다.
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default PrjevalPage;