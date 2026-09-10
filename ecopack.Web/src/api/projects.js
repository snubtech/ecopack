import axios from 'axios';
import { getCurrentCustomerId } from '../utils/memberProfile';

/**
 * 1. 프로젝트 목록 조회 함수
 * - 백엔드의 [HttpGet("GetProjects")]에 맞춰 /api/Projects/GetProjects로 호출합니다.
 */
export async function getProjects() {
    // 본인이 만든 프로젝트만 받아온다
    const response = await axios.get('/api/Projects/GetProjects', {
        params: { repCustId: getCurrentCustomerId() },
    });
    return response.data;
}

/**
 * 2. 신규 프로젝트 등록 함수
 * - 백엔드의 [HttpPost("CreateProject")]에 맞춰 /api/Projects/CreateProject로 호출합니다.
 */
export async function createProject(dto) {
    const response = await axios.post('/api/Projects/CreateProject', dto);
    return response.data;
}

/**
 * 2-1. 프로젝트 삭제 함수
 * - 백엔드의 [HttpDelete("DeleteProject")]에 맞춰 /api/Projects/DeleteProject로 호출합니다.
 * - 포장차수(packLevel) 단위로 지웁니다. 2차만 넘기면 2차 프로젝트와 2차 기술문서/적합성선언서만 삭제됩니다.
 */
export async function deleteProject(prjId, packLevel) {
    const response = await axios.delete('/api/Projects/DeleteProject', {
        params: { prjId, packLevel, repCustId: getCurrentCustomerId() },
    });
    return response.data;
}

/**
 * 3. 프로젝트 템플릿 정보 조회 함수
 */
export async function GetProjecttemplate(params) {
    const response = await axios.get('/api/Projects/template', { params });
    return response.data;
}

/**
 * 4. 프로젝트 상세 정보 저장 함수
 */
export async function SaveProjectDetail(dto) {
    // 본인이 만든 프로젝트만 저장할 수 있으므로 로그인한 고객 ID를 함께 보낸다
    const response = await axios.post('/api/Projects/detail', dto, {
        params: { repCustId: getCurrentCustomerId() },
    });
    return response.data;
}

/**
 * 5. 프로젝트 상세 정보 조회 함수
 */
export async function GetProjectDetail(prjId, packLevel) {
    const response = await axios.get('/api/Projects/Getdetail', {
        // 본인이 만든 프로젝트만 조회할 수 있으므로 로그인한 고객 ID를 함께 보낸다
        params: { prjId: prjId, packLevel: packLevel, repCustId: getCurrentCustomerId() }
    });
    return response.data;
}

/**
 * 6. 프로젝트 상세 정보 저장 함수 (템플릿 수정)
 */
export async function templateUpdate(dto) {
    const response = await axios.post('/api/Projects/templateUpdate', dto);
    return response.data;
}

/**
 * 7. 최신 평가지 문항 조회 함수 (ProjectevalController 연동)
 * - 백엔드의 [HttpGet("GetLatestEvalQuestions")]에 맞춰 호출합니다.
 */
export async function getLatestEvalQuestions(packLevel, appliedMaterial) {
    const response = await axios.get('/api/Projecteval/GetLatestEvalQuestions', {
        params: { packLevel, appliedMaterial }
    });
    return response.data;
}

/**
 * 8. 최신 평가지 문항 저장 함수 (ProjectevalController 연동)
 * - 백엔드의 [HttpPost("SaveEvalResults")]에 맞춰 호출합니다.
 */
export async function saveEvalResults(saveDtos) {
    const response = await axios.post('/api/Projecteval/SaveEvalResults', saveDtos);
    return response.data;
}

/**
 * 9. 저장된 평가 결과 조회 함수 (ProjectevalController 연동)
 * - 백엔드의 [HttpGet("GetSavedEvalResults")]에 맞춰 호출합니다.
 */
export const getSavedEvalResults = async (prjId, prjUserId, packLevel) => {
    try {
        const response = await axios.get('/api/projecteval/GetSavedEvalResults', {
            params: { prjId, prjUserId, packLevel }
        });
        return response.data;
    } catch (error) {
        console.error('저장된 평가 결과 조회 실패:', error);
        return [];
    }
};

/**
 * 10. 모의평가 최종 결과 요약 및 산출 조회 함수 (ProjectevalController 연동)
 * - 저장된 답안을 바탕으로 서버에서 합계 및 영역별 평가 결과 데이터를 연산하여 가져옵니다.
 */
export const getEvalSummary = async (prjId, prjUserId, packLevel) => {
    try {
        const { data } = await axios.get('/api/projecteval/GetEvalSummary', {
            params: { prjId, prjUserId, packLevel }
        });
        return data;
    } catch (error) {
        console.error('평가 결과 요약 조회 실패:', error);
        return null;
    }
};

// ─────────────────────────────────────────────────────────────
// 💡 추가된 4가지 분석 조회 API 함수 (백엔드 경로 매칭)
// ─────────────────────────────────────────────────────────────

/**
 * 11. 물성 정보 조회 함수
 * - 백엔드의 [HttpGet("Getmaterial")]에 맞춰 호출합니다.
 */
export async function getMaterial(packLevel, appliedMaterial, matType) {
    const response = await axios.get('/api/Projects/Getmaterial', {
        params: { packLevel, appliedMaterial, matType }
    });
    return response.data;
}

/**
 * 12. 환경규제 정보 조회 함수
 * - 백엔드의 [HttpGet("Getenvironment")]에 맞춰 호출합니다.
 */
export async function getEnvironment(packLevel, appliedMaterial, exportCountry) {
    const response = await axios.get('/api/Projects/Getenvironment', {
        params: { packLevel, appliedMaterial, exportCountry }
    });
    return response.data;
}

/**
 * 13. 공정도 정보 조회 함수
 * - 백엔드의 [HttpGet("Getprocessflow")]에 맞춰 호출합니다.
 */
export async function getProcessFlow(appliedMaterial, matType) {
    const response = await axios.get('/api/Projects/Getprocessflow', {
        params: { appliedMaterial, matType }
    });
    return response.data;
}

/**
 * 14. 탄소배출량 정보 조회 함수
 * - 백엔드의 [HttpGet("Getcarconinfo")]에 맞춰 호출합니다.
 */
export async function getCarconInfo(packLevel, appliedMaterial, matform) {
    const response = await axios.get('/api/Projects/Getcarconinfo', {
        params: { packLevel, appliedMaterial, matform }
    });
    return response.data;
}

/**
 * 15. 프로젝트 상세 리포트 정보 저장 함수 (Upsert)
 * - 백엔드의 [HttpPost("detailreport")]에 맞춰 호출합니다.
 */
export async function SaveProjectDetailReport(dto) {
    // 본인이 만든 프로젝트만 저장할 수 있으므로 로그인한 고객 ID를 함께 보낸다
    const response = await axios.post('/api/Projects/detailreport', dto, {
        params: { repCustId: getCurrentCustomerId() },
    });
    return response.data;
}

/**
 * 16. 프로젝트 상세 리포트 정보 조회 함수
 */
export async function GetProjectDetailReport(prjId, packLevel) {
    const response = await axios.get('/api/Projects/Getdetailreport', {
        params: { prjId: prjId, packLevel: packLevel, repCustId: getCurrentCustomerId() }
    });
    return response.data;
}

/**
 * 17. 디자인 템플릿 데이터 조회 함수 (파라미터 전달 방식)
 *   return { prjId, packLevel, appliedMaterial, prjUserId };
 */
// 수정 후: axios.post를 사용하고 데이터를 객체(Body)로 전달
// 1. 디자인 템플릿 조회 (POST 방식으로 Body 전달)
export const getDesignTemplate = async (prjId, packLevel) => {
    try {
        const response = await axios.post('/api/ProjectAiimage/GetDesignTemplate', {
            prjId: prjId,
            packLevel: packLevel
        });
        return response.data;
    } catch (error) {
        console.error('디자인 템플릿 조회 실패:', error);
        throw error;
    }
};

// 2. 2D AI 이미지 생성 요청 및 저장 함수
export async function generate2DImage(payload) {
    try {
        const response = await axios.post('/api/ProjectAiimage/Generate2DImage', payload);
        return response.data;
    } catch (error) {
        console.error('2D AI 이미지 생성 요청 실패:', error);
        throw error;
    }
}