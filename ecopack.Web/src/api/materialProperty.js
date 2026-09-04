/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - materialProperty API 클라이언트 (소재물성)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 라이브러리 > 소재물성 화면이 쓰는 서버 통신 묶음입니다. 대상 테이블은 if001 입니다.
 *    - 기존 도메인 코드(projects.js)와 같이 axios 로 직접 호출합니다.
 * 
 * 2. 제공 함수
 *    - GetMaterialPropertyFilters : 조회조건 셀렉트 7종을 한 번에 받아옵니다.
 *    - GetMaterialPropertyList    : 조건과 키워드로 걸러진 목록을 페이지 단위로 받아옵니다.
 * 
 * 3. 공통 규칙
 *    - 조회조건 중 빈 값은 '전체' 로 보고 서버가 그 조건을 건너뜁니다.
 *    - 목록 조회에서 pageSize 를 0 으로 보내면 전체를 받아옵니다(엑셀 내려받기용).
 * ==============================================================================
 */
import axios from 'axios';

/**
 * 라이브러리 > 소재물성 API 클라이언트
 * - 백엔드 라우트: api/MaterialProperty (대상 테이블 if001)
 * - 기존 도메인 코드와 동일하게 axios 직접 호출 방식을 사용합니다.
 */

/**
 * 1. 조회조건 셀렉트 목록 조회
 * - 포장차수/적용소재/사용환경/포장재구분/소재의구성/성능항목/단위 7종을 한 번에 받아옵니다.
 */
export async function GetMaterialPropertyFilters() {
    const response = await axios.get('/api/MaterialProperty/GetFilters');
    return response.data;
}

/**
 * 2. 소재물성 목록 조회
 * @param {object} params 조회조건
 *   packLevel, appliedMaterial, matUse, matType, matForm, item, unit : 코드값 (빈 값이면 전체)
 *   keywords : 텍스트 부분일치 검색어
 *   page, pageSize : 페이징 (pageSize 를 0 으로 보내면 전체 — 엑셀 내려받기용)
 */
export async function GetMaterialPropertyList(params) {
    const response = await axios.get('/api/MaterialProperty/GetList', { params });
    return response.data;
}
