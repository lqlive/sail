import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:8100',
        changeOrigin: true,
        secure: false,
        // Don't rewrite the path - keep /api prefix
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: true
  }
})