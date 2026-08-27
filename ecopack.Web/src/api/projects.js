import axios from 'axios';

/**
 * 1. 프로젝트 목록 조회 함수
 * - 서버(백엔드)의 GET /api/projects 주소로 요청을 보냅니다.
 * - DB에 저장되어 있는 전체 프로젝트 목록을 가져올 때 사용합니다.
 * - response.data: 서버가 보내준 진짜 데이터만 쏙 뽑아서 리턴합니다.
 */
export async function getProjects() {
    const response = await axios.get('/api/projects');
    return response.data;
}

/**
 * 2. 신규 프로젝트 등록 함수
 * - 서버(백엔드)의 POST /api/projects 주소로 데이터(dto)를 보냅니다.
 * - 사용자가 입력한 프로젝트 정보와 차수 정보를 담은 상자(dto)를 서버에 전달합니다.
 * - response.data: 서버가 저장을 끝낸 후 보내주는 결과(성공 여부, 생성된 prjId 등)를 리턴합니다.
 */
export async function createProject(dto) {
    const response = await axios.post('/api/projects', dto);
    return response.data;
}