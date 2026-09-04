/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - carbonEmission API 클라이언트 (탄소배출량)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 라이브러리 > 탄소배출량 화면이 쓰는 서버 통신 묶음입니다. 대상 테이블은 if005 입니다.
 *    - 기존 도메인 코드(projects.js)와 같이 axios 로 직접 호출합니다.
 * 
 * 2. 제공 함수
 *    - GetCarbonEmissionFilters : 조회조건 셀렉트 3종을 받아옵니다.
 *    - GetCarbonEmissionList    : 포장차수·적용소재·소재의 구성으로 걸러진 목록을 받아옵니다.
 * 
 * 3. 공통 규칙
 *    - 조회조건 중 빈 값은 '전체' 로 보고 서버가 그 조건을 건너뜁니다.
 *    - 목록 조회에서 pageSize 를 0 으로 보내면 전체를 받아옵니다(엑셀 내려받기용).
 * ==============================================================================
 */
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
