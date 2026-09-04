/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - processChart API 클라이언트 (공정도)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 라이브러리 > 공정도 화면이 쓰는 서버 통신 묶음입니다. 목록은 if003, 상세는 if003a 입니다.
 *    - 기존 도메인 코드(projects.js)와 같이 axios 로 직접 호출합니다.
 * 
 * 2. 제공 함수
 *    - GetProcessChartFilters : 조회조건 셀렉트(적용소재)를 받아옵니다.
 *    - GetProcessChartList    : 템플릿명·적용소재로 걸러진 목록을 받아옵니다.
 *    - GetProcessChartDetail  : 공정도 이미지를 받아옵니다. 용량이 크므로 행을 펼칠 때만 부릅니다.
 * 
 * 3. 공통 규칙
 *    - 조회조건 중 빈 값은 '전체' 로 보고 서버가 그 조건을 건너뜁니다.
 *    - 목록 조회에서 pageSize 를 0 으로 보내면 전체를 받아옵니다(엑셀 내려받기용).
 * ==============================================================================
 */
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
