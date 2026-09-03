import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [react()],
    server: {
        port: 5173,
        proxy: {
            '/api': {
                target: 'http://localhost:5258',
                changeOrigin: true,
            },
            // 기술문서 첨부파일(wwwroot/uploads) 다운로드용
            '/uploads': {
                target: 'http://localhost:5258',
                changeOrigin: true,
            },
        },
    },
});