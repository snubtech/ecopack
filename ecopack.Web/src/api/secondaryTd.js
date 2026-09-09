/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - primaryTd API 클라이언트 (2차 기술문서)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 2차 기술문서 화면이 쓰는 서버 통신 묶음입니다. 대상 테이블은 secondary_td 입니다.
 *    - 기존 도메인 코드(projects.js)와 같이 axios 로 직접 호출합니다.
 * 
 * 2. 제공 함수
 *    - GetSecondaryTd        : 프로젝트 한 건의 2차 기술문서를 받아옵니다. 작성 전이면 빈 문서가 옵니다.
 *    - SaveSecondaryTd       : 신규/수정을 한 번에 처리합니다(Upsert).
 *    - UploadAtchDoc       : 첨부문서를 올립니다. 문서명은 확장자 포함 원본 파일명으로 기록됩니다.
 *    - DeleteAtchDoc       : 첨부문서 슬롯을 비우고 서버의 실제 파일도 지웁니다.
 * 
 * 3. 소유자 확인
 *    - 문서는 그 프로젝트를 만든 회원만 열고 저장할 수 있습니다.
 *    - 그래서 모든 요청에 로그인한 고객 ID(repCustId)를 함께 보내고,
 *      서버가 본인 프로젝트가 맞는지 확인한 뒤 처리합니다. 남의 것이면 403 이 옵니다.
 * 
 * 4. 알아둘 점
 *    - 문서 ID 채번(TD-2-{타임스탬프})과 작성일시 갱신은 서버가 처리합니다.
 * ==============================================================================
 */
import axios from 'axios';
import { getCurrentCustomerId } from '../utils/memberProfile';

/**
 * 1차포장 2차 기술문서(secondary_td) API 클라이언트
 * - 백엔드 라우트: api/SecondaryTd
 * - 기존 도메인 코드(projects.js)와 동일하게 axios 직접 호출 방식을 사용합니다.
 */

/**
 * 1. 2차 기술문서 조회
 * - 해당 프로젝트의 2차 기술문서를 가져옵니다.
 * - 아직 작성 전이면 isNew=true 와 빈 데이터를 돌려줍니다.
 * @param {string} prjId 프로젝트 ID
 * @returns {Promise<{success:boolean, isNew:boolean, data:object}>}
 */
export async function GetSecondaryTd(prjId) {
    // 로그인한 고객 ID를 함께 보내 본인 프로젝트의 문서인지 서버가 확인하게 한다
    const response = await axios.get('/api/SecondaryTd/Get', { params: { prjId, repCustId: getCurrentCustomerId() } });
    return response.data;
}

/**
 * 2. 2차 기술문서 저장 (신규/수정 통합 Upsert)
 * - 저장 시 서버가 lastWrtDtm 을 현재 타임스탬프로 갱신합니다.
 * - 신규일 경우 pkg1TechDocId 를 TD-2-{타임스탬프} 규칙으로 채번합니다.
 * @param {object} dto 화면 입력값 전체
 */
export async function SaveSecondaryTd(dto) {
    // 저장도 본인 프로젝트인지 확인받는다
    const response = await axios.post('/api/SecondaryTd/Save', dto, { params: { repCustId: getCurrentCustomerId() } });
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

    const response = await axios.post('/api/SecondaryTd/UploadAtchDoc', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
        // 파일 업로드도 본인 프로젝트인지 확인받는다
        params: { repCustId: getCurrentCustomerId() },
    });
    return response.data;
}

/**
 * 4. 첨부문서 삭제
 * - 해당 슬롯의 URL/문서명을 비우고 서버의 실제 파일도 지웁니다.
 */
export async function DeleteAtchDoc(prjId, slot) {
    const response = await axios.delete('/api/SecondaryTd/DeleteAtchDoc', {
        // 파일 삭제도 본인 프로젝트인지 확인받는다
        params: { prjId, slot, repCustId: getCurrentCustomerId() },
    });
    return response.data;
}

/**
 * 5. 첨부문서 다운로드 링크 만들기
 * - 예전엔 서버가 돌려준 경로를 그대로 <a href>에 써서 로그인 없이도 내려받혔다.
 *   지금은 첨부파일이 wwwroot 밖에 있어 URL만으로는 못 받고, 이 API(소유자 확인 통과)를
 *   거쳐야만 내려받을 수 있다. 로그인한 고객 ID를 함께 실어 보낸다.
 * @param {string} prjId 프로젝트 ID
 * @param {number} slot 첨부 슬롯 번호 (1~8)
 * @returns {string} <a href>에 그대로 쓸 수 있는 다운로드 URL
 */
export function getAtchDocDownloadUrl(prjId, slot) {
    const params = new URLSearchParams({ prjId, slot, repCustId: getCurrentCustomerId() });
    return `/api/SecondaryTd/DownloadAtchDoc?${params.toString()}`;
}

/**
 * 6. 제조 도면 업로드
 * - 이미지 파일(png/jpg/jpeg/svg)만 허용한다. 슬롯이 아니라 한 장만 유지하므로
 *   다시 올리면 서버가 이전 이미지를 지우고 새로 바꾼다.
 * @param {string} prjId 프로젝트 ID
 * @param {File} file 업로드할 이미지 파일
 * @returns {Promise<{success:boolean, imageDataUri?:string, message:string}>}
 */
export async function UploadMfrDrw(prjId, file) {
    const formData = new FormData();
    formData.append('prjId', prjId);
    formData.append('file', file);

    const response = await axios.post('/api/SecondaryTd/UploadMfrDrw', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
        params: { repCustId: getCurrentCustomerId() },
    });
    return response.data;
}

/**
 * 7. 제조 도면 삭제
 */
export async function DeleteMfrDrw(prjId) {
    const response = await axios.delete('/api/SecondaryTd/DeleteMfrDrw', {
        params: { prjId, repCustId: getCurrentCustomerId() },
    });
    return response.data;
}
