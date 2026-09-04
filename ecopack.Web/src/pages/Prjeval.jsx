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
// 서버와 통신하기 위해 필요한 API 함수들을 불러와요! (기존 답변을 가져오는 함수도 함께 추가했어요)
import { getLatestEvalQuestions, getSavedEvalResults, saveEvalResults } from '../api/projects';

function PrjevalPage({ onSelectItem }) {
    // [상태 통장 만들기]
    // evalData: 화면에 보여줄 예쁜 시험지 뼈대 데이터
    // answers: 내가 어떤 문제에 어떤 보기를 골랐는지 적어두는 장바구니 (예: { 질문ID: 보기ID })
    // loading: 지금 데이터를 열심히 들고 오는 중인지 알려주는 신호등 (true면 로딩 중)
    const [evalData, setEvalData] = useState(null);
    const [answers, setAnswers] = useState({});
    const [loading, setLoading] = useState(true);

    // [세션 스토리지에서 금쪽같은 정보들 꺼내오기]
    // 프로젝트 번호, 포장 차수, 적용 소재, 사용자 아이디를 기억해 둡니다.
    const prjId = sessionStorage.getItem('currentPrjId') || '';
    const packLevel = sessionStorage.getItem('currentPackLevel') || '1';
    const appliedMaterial = sessionStorage.getItem('currentMaterial') || '';
    //const prjUserId = sessionStorage.getItem('currentUserId') || 'system_user';
    
    const sessionUser = JSON.parse(sessionStorage.getItem('prjuserid') || '{}');
    const prjUserId = sessionUser.repCustId || '';

    // [잠금 장치(Lock) 만들기]
    // 데이터가 중복으로 두 번 불러와지는 것을 막아주는 든든한 문고리예요.
    const isFetchingRef = useRef(false);

    /**
     * ==============================================================================
     * [데이터 변환 함수: 흩어진 시험지 조각들을 3단계 트리 구조로 조립하기]
     * 서버에서 오는 데이터는 한 줄씩 흩어져서 오기 때문에, 이를 1단계 -> 2단계 -> 3단계 
     * 계층 구조로 착착 조립해 주는 똑똑한 마법 상자 같은 함수입니다.
     * ==============================================================================
     */
    const transformEvalData = (rawData) => {
        // 만약 데이터가 이상한 모양이라면 빈 상자를 돌려주어 에러를 방지해요.
        if (!Array.isArray(rawData)) return { questions: [] };

        const questionsList = []; // 최종 완성될 3단계 질문 꾸러미
        const questionMap = new Map(); // 질문이 중복으로 들어오지 않게 빠르게 찾아주는 장부

        // 보기(선택지) 데이터를 보기 좋게 다듬어주는 작은 도우미 함수
        const parseOptionData = (row) => ({
            optionId: row.asmtQstItemId,         // 보기 고유 번호
            optionText: row.asmtQstItemNm,       // 보기 이름 (글자)
            score: row.scoringCriteria ?? row.score ?? 0, // 보기 점수 (없으면 0점)
            natRglAls: row.natRglAls || '',      // 국가 규제 이야기
            dsgnRecmImp: row.dsgnRecmImp || '',  // 디자인 개선 추천 이야기
            subQuestions: []                     // 이 보기를 고르면 나타날 하위 문항 주머니
        });

        // [1단계 작업] 가장 대장인 최상위 질문들을 먼저 찾아서 목록에 꽂아넣습니다.
        rawData.forEach(row => {
            // 부모 보기 ID가 없다는 것은 가장 맨 위에 있는 1단계 대장 질문이라는 뜻이에요!
            if (!row.prtAsmtQstItemId) {

                // 아직 장부에 등록되지 않은 새로운 질문이라면 새로 만들어 줍니다.
                if (!questionMap.has(row.asmtQstId)) {
                    const rawAreaCode = row.ecoPackLarType || row.ecopacklartype || row.ecoPackArea || row.ecopackarea || row.asmtAreaCode || '';
                    const rawMaterial = row.appliedMaterial || row.appliedmaterial || row.material || row.packageMaterial || appliedMaterial || '';
                    const rawAreaName = row.ecoPackAreaNm || row.ecopackareaynm || row.ecoPackLarTypeNm || row.ecoPackLarTypeNm || row.ecoPackArea || row.ecoPackArea || row.areaName || row.asmtAreaNm || '';

                    const newQuestion = {
                        questionId: row.asmtQstId,       // 질문 번호
                        questionTitle: row.asmtQstNm,     // 질문 제목
                        areaCode: rawAreaCode,           // 영역 코드
                        material: rawMaterial,           // 소재 이름
                        areaName: rawAreaName,           // 영역 이름
                        options: []                      // 보기 목록 주머니
                    };
                    questionMap.set(row.asmtQstId, newQuestion);
                    questionsList.push(newQuestion);
                }

                // 1단계 질문에 딸려 있는 보기(선택지)들을 쏙쏙 집어넣어요.
                if (row.asmtQstItemId) {
                    const currentQuestion = questionMap.get(row.asmtQstId);
                    const existsOption = currentQuestion.options.some(opt => opt.optionId === row.asmtQstItemId);
                    if (!existsOption) {
                        currentQuestion.options.push(parseOptionData(row));
                    }
                }
            }
        });

        // [2단계 및 3단계 작업] 부모가 있는 하위 문항들을 알맞은 부모 밑에 쏙쏙 찾아 끼워넣습니다.
        rawData.forEach(row => {
            if (row.prtAsmtQstItemId) { // 부모가 있으므로 하위 문항이에요!

                // 시나리오 A: 2단계 문항 찾기 (1단계 보기 밑에 쏙 들어가는 경우)
                for (let question of questionsList) {
                    const parentOption = question.options.find(opt => opt.optionId === row.prtAsmtQstItemId);

                    if (parentOption) {
                        let subQ = parentOption.subQuestions.find(sq => sq.subQuestionId === row.asmtQstId);

                        if (!subQ) {
                            subQ = {
                                subQuestionId: row.asmtQstId,
                                subQuestionTitle: row.asmtQstNm,
                                options: []
                            };
                            parentOption.subQuestions.push(subQ);
                        }

                        if (row.asmtQstItemId) {
                            const existsSubOpt = subQ.options.some(opt => opt.optionId === row.asmtQstItemId);
                            if (!existsSubOpt) {
                                subQ.options.push(parseOptionData(row));
                            }
                        }
                        return;
                    }
                }

                // 시나리오 B: 3단계 심층 문항 찾기 (2단계 보기 밑에 더 깊숙이 들어가는 경우)
                for (let question of questionsList) {
                    for (let option of question.options) {
                        for (let subQ of option.subQuestions || []) {
                            const parentSubOpt = subQ.options.find(subOpt => subOpt.optionId === row.prtAsmtQstItemId);

                            if (parentSubOpt) {
                                if (!parentSubOpt.subQuestions) {
                                    parentSubOpt.subQuestions = [];
                                }

                                let deepSubQ = parentSubOpt.subQuestions.find(dsq => dsq.subQuestionId === row.asmtQstId);

                                if (!deepSubQ) {
                                    deepSubQ = {
                                        subQuestionId: row.asmtQstId,
                                        subQuestionTitle: row.asmtQstNm,
                                        options: []
                                    };
                                    parentSubOpt.subQuestions.push(deepSubQ);
                                }

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
                                return;
                            }
                        }
                    }
                }
            }
        });

        // 조립이 모두 끝난 예쁜 트리 구조 데이터를 돌려줍니다.
        return { questions: questionsList };
    };

    /**
     * ==============================================================================
     * [초보자도 이해할 수 있는 아주 쉬운 복원 프로세스 설명]
     * 
     * 1. 화면이 처음 켜질 때 (useEffect)
     *    - 서버에 가서 두 가지 물건을 한꺼번에 받아와요.
     *      ① 원본 시험지 문제들 (getLatestEvalQuestions)
     *      ② 내가 예전에 체크해 뒀던 답변 기록들 (getSavedEvalResults)
     * 
     * 2. 예전 답변 정리하기 (restoredAnswers 만들기)
     *    - 서버에서 받아온 기록 중에서 "어떤 질문에 어떤 보기를 골랐었지?" 하고 
     *      짝을 지어서 { 질문ID: 보기ID } 형태의 장바구니(객체)를 만들어 줍니다.
     * 
     * 3. 상태에 쏙 집어넣기 (setAnswers)
     *    - 장바구니에 담긴 답들을 answers 상태에 저장해요.
     *    - 리액트는 이 상태가 바뀌면 화면을 자동으로 새로고침(리렌더링)합니다.
     * 
     * 4. 라디오 버튼에 마법처럼 체크되기!
     *    - 화면을 다시 그릴 때, 라디오 버튼들이 "아, 내 질문 ID에 이 보기 ID가 저장되어 있네?" 하고 
     *      스스로 체크(checked)되면서 사용자가 예전에 체크했던 그 모양 그대로 되살아납니다!
     * ==============================================================================
     */
    useEffect(() => {
        const fetchEvalDataAndSavedAnswers = async () => {
            // 만약 데이터를 이미 가져오는 중이라면 중복으로 실행되지 않도록 문고리를 꽉 잠급니다!
            if (isFetchingRef.current) return;
            isFetchingRef.current = true;

            try {
                // 화면에 "로딩 중..." 판넬을 띄우기 위해 신호를 true로 바꿔요.
                setLoading(true);
                //  전송하는 파라미터 값들이 정확한지 먼저 콘솔로 확인
                console.log("🚀 서버에 데이터 조회 요청 파라미터:", {
                    prjId,
                    prjUserId,
                    packLevel                    
                });
                //  [1단계] 서버에 동시에 요청해서 "시험지 원본"과 "내가 예전에 저장했던 답"을 가져와요!
                const [questionData, savedAnswersData] = await Promise.all([
                    getLatestEvalQuestions(packLevel, appliedMaterial),
                    prjId ? getSavedEvalResults(prjId, prjUserId, packLevel).catch(() => []) : Promise.resolve([])
                ]);
                console.log("서버에서 받아온 기존 저장 내역 원본:", savedAnswersData);

                // 🧩 [2단계] 가져온 시험지 원본을 화면에 예쁘게 그릴 수 있도록 3단계 트리 구조로 뚝딱 조립해요.
                const formattedData = transformEvalData(questionData);
                setEvalData(formattedData);

                // ✨ [3단계] 예전에 저장한 답변이 있다면, { 질문ID: 보기ID } 모양의 장바구니로 예쁘게 정리해요.
                if (Array.isArray(savedAnswersData) && savedAnswersData.length > 0) {
                    const restoredAnswers = {};

                    savedAnswersData.forEach(item => {
                        // 질문 ID와 보기 ID가 둘 다 있을 때만 장바구니에 쏙 담습니다.
                        if (item.asmtQstId && item.asmtQstItemId) {
                            restoredAnswers[item.asmtQstId] = item.asmtQstItemId;
                        }
                    });

                    // 정리된 장바구니를 answers 상태에 척 하니 넣어줍니다! 
                    // 이 순간 리액트가 화면을 다시 그리면서 라디오 버튼들이 알아서 척척 체크돼요.
                    setAnswers(restoredAnswers);
                    console.log("마법처럼 복원된 기존 답변들:", restoredAnswers);
                }

            } catch (error) {
                console.error('데이터를 불러오다가 문제가 생겼어요:', error);
                alert('데이터를 가져오지 못했습니다. 다시 시도해 주세요.');
            } finally {
                // 일이 끝났으니 로딩창을 끄고, 잠금 문고리를 다시 스르륵 풀어줍니다.
                setLoading(false);
                isFetchingRef.current = false;
            }
        };

        // 컴포넌트가 처음 화면에 나타날 때 이 마법의 함수를 실행해요!
        fetchEvalDataAndSavedAnswers();
    }, [packLevel, appliedMaterial, prjId]);

    // 사용자가 라디오 버튼을 콕 누를 때마다 장바구니(answers)에 어떤 걸 골랐는지 예쁘게 업데이트해주는 함수
    const handleOptionChange = (questionId, optionId) => {
        setAnswers(prev => ({
            ...prev,
            [questionId]: optionId
        }));
    };

    // 취소 버튼을 눌렀을 때 정말 뒤로 갈 것인지 물어보고 이동하는 함수
    const handleCancel = () => {
        if (window.confirm('작성 중인 내용이 저장되지 않을 수 있습니다. 이전 단계로 이동하시겠습니까?')) {
            if (typeof onSelectItem === 'function') {
                onSelectItem('prjtemplate');
            }
        }
    };

    /**
     * ==============================================================================
     * [저장용 데이터 생성 함수]
     * 화면에 체크된 모든 답들을 모아서, 2단계 3단계 하위 점수까지 전부 더한 뒤 
     * 서버에 보낼 수 있는 예쁜 택배 상자(배열)로 포장해 주는 함수입니다.
     * ==============================================================================
     */
    const createEvalSaveList = () => {
        const evalSaveList = [];

        if (!evalData || !evalData.questions) return evalSaveList;

        // 재귀적으로 하위 문항들을 순회하며 답변과 다음 질문 ID를 찾아 리스트에 담는 함수
        const processQuestionRecursive = (qstId, qstTitle, areaCode, areaName, options, shtHdrId = '', nextQstId = null) => {
            const selectedOptId = answers[qstId];
            if (!selectedOptId || !options) return;

            const option = options.find(opt => opt.optionId === selectedOptId);
            if (!option) return;

            // 선택된 항목과 함께 nextAsmtQstId를 포함한 객체를 생성합니다.
            evalSaveList.push({
                prjid: prjId,                        // 프로젝트 번호
                prjuserid: prjUserId,                // 사용자 번호
                packLevel: packLevel,                // 포장 차수
                packLevelnm: `판매(${packLevel}차)`, // 포장 차수 이름
                appliedMaterial: appliedMaterial,    // 포장 소재
                ecoPackLarType: areaCode || 'DEFAULT', // 영역 코드
                ecoPackAreaNm: areaName || '',         // 영역 이름
                asmtShtHdrId: shtHdrId || '',          // 헤더 ID
                asmtQstId: qstId,                      // 현재 질문 ID
                nextAsmtQstId: nextQstId || '',        // 다음 질문 ID 매핑
                asmtQstItemId: option.optionId,      // 고른 보기 ID
                prtAsmtQstItemId: null,              // 부모 보기 ID
                asmtQstNm: qstTitle,                   // 질문 제목
                asmtQstItemNm: option.optionText,    // 보기 이름
                scoringCriteria: String(option.score || 0), // 점수 기준
                asmtpoint: String(option.score || '0'),     // 획득 점수
                natRglAls: option.natRglAls || '',      // 규제 분석 내용
                dsgn_recm_imp: option.dsgnRecmImp || ''  // 디자인 개선 내용
            });

            // 하위 문항(subQuestions)이 존재할 경우 순회 진행
            if (option.subQuestions && option.subQuestions.length > 0) {
                option.subQuestions.forEach((subQ, index, arr) => {
                    const nextSubQId = (index + 1 < arr.length) ? arr[index + 1].subQuestionId : null;

                    processQuestionRecursive(
                        subQ.subQuestionId,
                        subQ.subQuestionTitle,
                        areaCode,
                        areaName,
                        subQ.options,
                        shtHdrId,
                        nextSubQId
                    );
                });
            }
        };

        // 최상위 질문들을 순회하며 시작
        evalData.questions.forEach((question, index, arr) => {
            const nextQuestionId = (index + 1 < arr.length) ? arr[index + 1].questionId : null;

            processQuestionRecursive(
                question.questionId,
                question.questionTitle,
                question.areaCode,
                question.areaName,
                question.options,
                question.asmtShtHdrId,
                nextQuestionId
            );
        });

        return evalSaveList;
    };

    // [임시 저장 버튼을 누를 때 실행되는 함수]
    const handleSave = async () => {
        if (!prjId) {
            alert('프로젝트 ID(prjId)를 찾을 수 없습니다.');
            return;
        }

        try {
            const savelist = createEvalSaveList();
            console.log("서버로 보낼 평가 리스트 데이터:", savelist);

            if (savelist.length === 0) {
                alert('저장할 평가 항목이 없습니다. 문항을 선택해 주세요.');
                return;
            }

            // 서버에 택배 상자를 전송해요!
            const result = await saveEvalResults(savelist);

            if (result && result.success) {
                alert(`성공적으로 임시 저장되었습니다. (총 ${result.count || savelist.length}개 항목)`);
            } else {
                alert('저장에 실패했습니다.');
            }
        } catch (error) {
            console.error('저장 중 에러 발생:', error);
            alert('저장 중 오류가 발생했습니다.');
        }
    };

    // [다음 단계 버튼을 누를 때 실행되는 함수]
    const handleNext = () => {
        try {
            alert('다음 단계로 이동합니다.');
            if (typeof onSelectItem === 'function') {
                onSelectItem('nextStepKey');
            }
        } catch (error) {
            console.error('다음 단계 이동 중 에러 발생:', error);
        }
    };

    // 데이터가 아직 안 와서 로딩 중일 때 화면에 보여주는 친절한 안내 문구
    if (loading) {
        return (
            <div style={{ textAlign: 'center', padding: '80px', color: '#666', fontSize: '15px' }}>
                평가지 문항과 이전 작성 내용을 불러오는 중입니다... 잠시만 기다려 주세요!
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