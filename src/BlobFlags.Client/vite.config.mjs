import { defineConfig } from "vite";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [tailwindcss()],
  build: {
    outDir: "../BlobFlags.Server/wwwroot",
    emptyOutDir: true,
  },
  server: {
    port: 8080,
    proxy: {
      "/api": "http://localhost:8085",
    },
  },
});
