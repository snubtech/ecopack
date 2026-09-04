import axios from 'axios';

/**
 * 라이브러리 > 공정도 API 클라이언트
 * - 목록: api/ProcessChart (if003), 상세: 같은 컨트롤러의 GetDetail (if003a)
 */

/** 1. 조회조건 셀렉트 목록 (적용소재) */
export async function GetProcessChartFilters() {
    const response = await axios.get('/api/ProcessChart/GetFilters');
    return response.data;
}

/**
 * 2. 공정도 목록 조회
 * @param {object} params subject(템플릿명 부분일치), appliedMaterial(코드), page, pageSize
 *   pageSize 를 0 으로 보내면 전체 — 엑셀 내려받기용
 */
export async function GetProcessChartList(params) {
    const response = await axios.get('/api/ProcessChart/GetList', { params });
    return response.data;
}

/**
 * 3. 공정도 상세 (공정도 이미지)
 * - 응답에 이미지가 실리므로 행을 펼칠 때만 호출합니다.
 */
export async function GetProcessChartDetail(packMmftProcId) {
    const response = await axios.get('/api/ProcessChart/GetDetail', { params: { packMmftProcId } });
    return response.data;
}
