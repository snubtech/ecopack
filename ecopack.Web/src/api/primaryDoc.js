import axios from 'axios';

/**
 * 1차포장 적합성선언서(primary_doc) API 클라이언트
 * - 백엔드 라우트: api/PrimaryDoc
 * - 기존 도메인 코드(projects.js, primaryTd.js)와 동일하게 axios 직접 호출 방식을 사용합니다.
 */

/**
 * 1. 적합성선언서 조회
 * - 아직 작성 전이면 isNew=true 와 빈 데이터를 돌려줍니다.
 *   (이때 기술문서가 이미 있으면 pkg1TechDocId가 미리 채워집니다)
 * @param {string} prjId 프로젝트 ID
 */
export async function GetPrimaryDoc(prjId) {
    const response = await axios.get('/api/PrimaryDoc/Get', { params: { prjId } });
    return response.data;
}

/**
 * 2. 적합성선언서 저장 (신규/수정 통합 Upsert)
 * - 저장 시 서버가 lastWrtDt(발행일)를 현재 날짜로 갱신합니다.
 * - 신규일 경우 pkg1DocId 를 DOC-1-{타임스탬프} 규칙으로 채번합니다.
 * @param {object} dto 화면 입력값 전체
 */
export async function SavePrimaryDoc(dto) {
    const response = await axios.post('/api/PrimaryDoc/Save', dto);
    return response.data;
}

/**
 * 3. 근거문서(부속서) 업로드
 * - 파일은 서버 wwwroot/uploads/doc/{prjId}/ 에 저장되고
 *   evdDocUrl{slot} 에 경로가, evdDocNm{slot} 에 확장자 포함 원본 파일명이 반영됩니다.
 * @param {string} prjId 프로젝트 ID
 * @param {number} slot 부속서 슬롯 번호 (1~8 = 부속서 A~H)
 * @param {File} file 업로드할 파일
 */
export async function UploadEvdDoc(prjId, slot, file) {
    const formData = new FormData();
    formData.append('prjId', prjId);
    formData.append('slot', String(slot));
    formData.append('file', file);

    const response = await axios.post('/api/PrimaryDoc/UploadEvdDoc', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
}

/**
 * 4. 근거문서(부속서) 삭제
 * - 해당 슬롯의 URL/문서명을 비우고 서버의 실제 파일도 지웁니다.
 */
export async function DeleteEvdDoc(prjId, slot) {
    const response = await axios.delete('/api/PrimaryDoc/DeleteEvdDoc', {
        params: { prjId, slot },
    });
    return response.data;
}
