import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    fable({
      project: "./src/App.fsproj",
      jsx: "automatic"
    }),
    react({
      include: /\.(fs|js|jsx|ts|tsx)$/
    })
  ],
  build: {
    sourcemap: true
  }
});
