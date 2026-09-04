/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - designTemplate API 클라이언트 (디자인 템플릿)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 라이브러리 > 디자인 템플릿 화면이 쓰는 서버 통신 묶음입니다. 목록은 if002, 상세는 if002a 입니다.
 *    - 기존 도메인 코드(projects.js)와 같이 axios 로 직접 호출합니다.
 * 
 * 2. 제공 함수
 *    - GetDesignTemplateFilters : 조회조건 셀렉트 3종을 받아옵니다.
 *    - GetDesignTemplateList    : 템플릿명과 셀렉트 3종으로 걸러진 목록을 받아옵니다.
 *    - GetDesignTemplateDetail  : 설명 3종과 이미지를 받아옵니다. 행을 펼칠 때만 부릅니다.
 * 
 * 3. 공통 규칙
 *    - 조회조건 중 빈 값은 '전체' 로 보고 서버가 그 조건을 건너뜁니다.
 *    - 목록 조회에서 pageSize 를 0 으로 보내면 전체를 받아옵니다(엑셀 내려받기용).
 * ==============================================================================
 */
import axios from 'axios';

/**
 * 라이브러리 > 디자인 템플릿 API 클라이언트
 * - 목록: api/DesignTemplate (if002), 상세: 같은 컨트롤러의 GetDetail (if002a)
 */

/** 1. 조회조건 셀렉트 목록 (포장차수 / 적용소재 / 포장재 구분) */
export async function GetDesignTemplateFilters() {
    const response = await axios.get('/api/DesignTemplate/GetFilters');
    return response.data;
}

/**
 * 2. 디자인 템플릿 목록 조회
 * @param {object} params
 *   subject : 템플릿명 부분일치
 *   packLevel, appliedMaterial, matType : 코드값 (빈 값이면 전체)
 *   page, pageSize : 페이징 (pageSize 를 0 으로 보내면 전체 — 엑셀 내려받기용)
 */
export async function GetDesignTemplateList(params) {
    const response = await axios.get('/api/DesignTemplate/GetList', { params });
    return response.data;
}

/**
 * 3. 디자인 템플릿 상세 (설명 3종 + 이미지)
 * - 응답에 이미지가 실리므로 행을 펼칠 때만 호출합니다.
 */
export async function GetDesignTemplateDetail(packDsgnTplId) {
    const response = await axios.get('/api/DesignTemplate/GetDetail', { params: { packDsgnTplId } });
    return response.data;
}
