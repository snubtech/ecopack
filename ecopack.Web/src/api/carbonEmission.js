import axios from 'axios';

/**
 * 라이브러리 > 탄소배출량 API 클라이언트
 * - 백엔드 라우트: api/CarbonEmission (대상 테이블 if005 환경영향평가정보)
 */

/** 1. 조회조건 셀렉트 목록 (포장차수 / 적용소재 / 소재의 구성) */
export async function GetCarbonEmissionFilters() {
    const response = await axios.get('/api/CarbonEmission/GetFilters');
    return response.data;
}

/**
 * 2. 탄소배출량 목록 조회
 * @param {object} params packLevel, appliedMaterial, matForm (코드값, 빈 값이면 전체), page, pageSize
 *   pageSize 를 0 으로 보내면 전체 — 엑셀 내려받기용
 */
export async function GetCarbonEmissionList(params) {
    const response = await axios.get('/api/CarbonEmission/GetList', { params });
    return response.data;
}
