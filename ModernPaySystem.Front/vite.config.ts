import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from "path"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  optimizeDeps: {
    exclude: ['cytoscape'],
    include: ['jspdf', 'html2canvas'],
  },
  build: {
    outDir: '../ModernPaySystem/wwwroot',
    emptyOutDir: true,
  }
})
