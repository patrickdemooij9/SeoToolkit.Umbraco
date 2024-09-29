import { defineConfig } from "vite";

export default defineConfig({
    build: {
        lib: {
            entry: "src/index.ts", // your web component source file
            formats: ["es"],
            fileName: "robotstxt"
        },
        outDir: "../wwwroot", 
        emptyOutDir: false,
        sourcemap: true,
        rollupOptions: {
            external: [/^@umbraco/],
            onwarn: () => { },
        },
    },
});