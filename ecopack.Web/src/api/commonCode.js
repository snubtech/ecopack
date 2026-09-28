import axiosInstance from './axiosInstance';

/**
 * 소재 속성 목록을 조회하는 공용 함수
 */
export const getMaterialProperty = async (packLevel) => {
    try {
        // 백엔드의 [HttpGet("material")] 경로와 일치하며, packLevel을 쿼리 파라미터로 전달합니다.
        const response = await axiosInstance.get('/common/material', {
            params: { packLevel } // 👈 packLevel 인자 추가
        });
        return response.data;
    } catch (error) {
        console.error('소재 정보 조회 실패:', error);
        return [];
    }
};
export const getPackLevels = async () => {
    const response = await axiosInstance.get('/common/packlevels');
    return response.data;
};
// packLevel 인자 추가)
export const getMattypes = async (packLevel) => {
    try {
        const response = await axiosInstance.get('/common/mattype', {
            params: { packLevel } // 이제 정의된 packLevel 변수를 정상적으로 참조합니다.
        });
        return response.data;
    } catch (error) {
        console.error('포장재 종류 조회 실패:', error);
        return [];
    }
};

export const getMatForms = async (packLevel, appliedMaterial, matType) => {
    try {
        const response = await axiosInstance.get('/common/matforms', {
            params: { packLevel, appliedMaterial, matType }
        });
        return response.data;
    } catch (error) {
        console.error('소재 형태 목록 조회 실패:', error);
        return [];
    }
};