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
