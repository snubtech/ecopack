import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [react()],
    server: {
        host: true, // 👈 이 줄을 추가해주세요! (모든 외부 IP 접속 허용)
        port: 5173,
        proxy: {
            '/api': {
                target: 'http://localhost:5258',               
                changeOrigin: true,
            },
        },
    },
});