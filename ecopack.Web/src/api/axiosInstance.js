import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: '/api', // vite.config.js의 프록시 설정 활용
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// 요청 인터셉터 (필요한 경우 인증 토큰 등 주입)
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// 응답 인터셉터 (에러 공통 처리 등)
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    // 예: 401 Unauthorized 발생 시 로그인 페이지로 리다이렉트 등
    return Promise.reject(error);
  }
);

export default axiosInstance;