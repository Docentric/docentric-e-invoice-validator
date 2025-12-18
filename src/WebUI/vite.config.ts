import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { componentTagger } from "lovable-tagger";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  server: {
    host: "::",
    port: 53365,
    strictPort: true,
    proxy: {
      "/api": {
        target: 'https://localhost:57456',
        changeOrigin: true,
        secure: false,      // if you use a self-signed dev cert
      },
      "/openapi/v1.json": {
        target: 'https://localhost:57456',
        changeOrigin: true,
        secure: false,      // if you use a self-signed dev cert
      }
    },
  },
  plugins: [react(), mode === "development" && componentTagger()].filter(Boolean),
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
}));
