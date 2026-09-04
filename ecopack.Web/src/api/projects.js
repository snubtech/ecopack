import axios from 'axios';

/**
 * 1. 프로젝트 목록 조회 함수
 * - 백엔드의 [HttpGet("GetProjects")]에 맞춰 /api/Projects/GetProjects로 호출합니다.
 */
export async function getProjects() {
    const response = await axios.get('/api/Projects/GetProjects');
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
    const response = await axios.post('/api/Projects/detail', dto);
    return response.data;
}

/**
 * 5. 프로젝트 상세 정보 조회 함수
 */
export async function GetProjectDetail(prjId, packLevel) {
    const response = await axios.get('/api/Projects/Getdetail', {
        params: { prjId: prjId, packLevel: packLevel }
    });
    return response.data;
}

/**
* 6. 프로젝트 상세 정보 저장 함수
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