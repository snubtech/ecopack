import axiosInstance from './axiosInstance';

/**
 * 소재 속성 목록을 조회하는 공용 함수
 */
export const getMaterialProperty = async () => {
    try {
        //  백엔드의 [HttpGet("material")] 경로와 글자 하나 안 틀리고 일치해야 합니다!
        const response = await axiosInstance.get('/common/material');
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
export const getMattypes = async () => {
    const response = await axiosInstance.get('/common/mattype');
    return response.data;
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