import axios from 'axios';

/**
 * 1차포장 기술문서(primary_td) API 클라이언트
 * - 백엔드 라우트: api/PrimaryTd
 * - 기존 도메인 코드(projects.js)와 동일하게 axios 직접 호출 방식을 사용합니다.
 */

/**
 * 1. 기술문서 조회
 * - 해당 프로젝트의 기술문서를 가져옵니다.
 * - 아직 작성 전이면 isNew=true 와 빈 데이터를 돌려줍니다.
 * @param {string} prjId 프로젝트 ID
 * @returns {Promise<{success:boolean, isNew:boolean, data:object}>}
 */
export async function GetPrimaryTd(prjId) {
    const response = await axios.get('/api/PrimaryTd/Get', { params: { prjId } });
    return response.data;
}

/**
 * 2. 기술문서 저장 (신규/수정 통합 Upsert)
 * - 저장 시 서버가 lastWrtDtm 을 현재 타임스탬프로 갱신합니다.
 * - 신규일 경우 pkg1TechDocId 를 TD-1-{타임스탬프} 규칙으로 채번합니다.
 * @param {object} dto 화면 입력값 전체
 */
export async function SavePrimaryTd(dto) {
    const response = await axios.post('/api/PrimaryTd/Save', dto);
    return response.data;
}

/**
 * 3. 첨부문서 업로드
 * - 파일은 서버 wwwroot/uploads/td/{prjId}/ 에 저장되고
 *   atchDocUrl{slot} 에 경로가, atchDocNm{slot} 에 확장자 포함 원본 파일명이 반영됩니다.
 * @param {string} prjId 프로젝트 ID
 * @param {number} slot 첨부 슬롯 번호 (1~8)
 * @param {File} file 업로드할 파일
 */
export async function UploadAtchDoc(prjId, slot, file) {
    const formData = new FormData();
    formData.append('prjId', prjId);
    formData.append('slot', String(slot));
    formData.append('file', file);

    const response = await axios.post('/api/PrimaryTd/UploadAtchDoc', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
}

/**
 * 4. 첨부문서 삭제
 * - 해당 슬롯의 URL/문서명을 비우고 서버의 실제 파일도 지웁니다.
 */
export async function DeleteAtchDoc(prjId, slot) {
    const response = await axios.delete('/api/PrimaryTd/DeleteAtchDoc', {
        params: { prjId, slot },
    });
    return response.data;
}
