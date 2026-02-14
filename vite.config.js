import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    fable({
      fsproj: "src/App.fsproj",
      jsx: "automatic"
    }),
    react({
      include: /\.(fs|js|jsx|ts|tsx)$/
    })
  ],
  base: '/CalTwo/',
  server: {
    port: 5173,
    strictPort: true,
  },
  build: {
    sourcemap: true
  }
});
