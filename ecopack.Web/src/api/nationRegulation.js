import axios from 'axios';

/**
 * 라이브러리 > 환경규제 API 클라이언트
 * - 백엔드 라우트: api/NationRegulation (대상 테이블 if004 국가규제정보)
 */

/** 1. 조회조건 셀렉트 목록 (포장차수 / 적용소재 / 국가) */
export async function GetNationRegulationFilters() {
    const response = await axios.get('/api/NationRegulation/GetFilters');
    return response.data;
}

/**
 * 2. 환경규제 목록 조회
 * @param {object} params
 *   packLevel, appliedMaterial, countryCode : 코드값 (빈 값이면 전체)
 *   relatedReg, regItem : 텍스트 부분일치 검색어
 *   page, pageSize : 페이징 (pageSize 를 0 으로 보내면 전체 — 엑셀 내려받기용)
 */
export async function GetNationRegulationList(params) {
    const response = await axios.get('/api/NationRegulation/GetList', { params });
    return response.data;
}
