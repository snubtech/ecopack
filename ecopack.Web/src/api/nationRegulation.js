/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - nationRegulation API 클라이언트 (환경규제)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 라이브러리 > 환경규제 화면이 쓰는 서버 통신 묶음입니다. 대상 테이블은 if004 입니다.
 *    - 기존 도메인 코드(projects.js)와 같이 axios 로 직접 호출합니다.
 * 
 * 2. 제공 함수
 *    - GetNationRegulationFilters : 조회조건 셀렉트 3종을 받아옵니다.
 *    - GetNationRegulationList    : 셀렉트 3종과 관련규정·규제항목 검색어로 걸러진 목록을 받아옵니다.
 * 
 * 3. 공통 규칙
 *    - 조회조건 중 빈 값은 '전체' 로 보고 서버가 그 조건을 건너뜁니다.
 *    - 목록 조회에서 pageSize 를 0 으로 보내면 전체를 받아옵니다(엑셀 내려받기용).
 * ==============================================================================
 */
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
