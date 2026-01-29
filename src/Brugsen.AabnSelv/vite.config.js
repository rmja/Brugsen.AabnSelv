import { defineConfig } from "vite";
import aurelia from "@aurelia/vite-plugin";
import { analyzer } from "vite-bundle-analyzer";

export default defineConfig({
  plugins: [
    aurelia(),
    analyzer({ analyzerMode: "static", openAnalyzer: false }),
  ],
  base: "./",
  build: {
    target: "esnext",
    sourcemap: true,
    outDir: "./wwwroot",
    rollupOptions: {
      output: {
        manualChunks: {
          aurelia: ["aurelia", "@aurelia/router", "@aurelia/i18n"],
          bootstrap: [
            "bootstrap",
            "bootstrap/dist/css/bootstrap.css",
            "bootstrap/dist/js/bootstrap",
            "@popperjs/core",
          ],
          "intl-tel-input": ["intl-tel-input", "intl-tel-input/utils"],
        },
      },
    },
  },
  server: {
    proxy: {
      "/api": "http://localhost:60900/api",
    },
  },
});
