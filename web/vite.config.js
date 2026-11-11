import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // En desarrollo (npm run dev) redirige /api a la API local.
    proxy: {
      '/api': 'http://localhost:5127'
    }
  }
});
