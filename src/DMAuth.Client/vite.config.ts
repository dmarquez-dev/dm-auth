import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7259',
        changeOrigin: true,
        secure: false,
      },
      '/connect': {
        target: 'https://localhost:7259',
        changeOrigin: true,
        secure: false,
      },
      '/.well-known': {
        target: 'https://localhost:7259',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
