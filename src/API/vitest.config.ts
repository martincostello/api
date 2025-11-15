import { defineConfig } from 'vitest/config';

export default defineConfig({
    test: {
        environment: 'jsdom',
        globals: true,
        coverage: {
            provider: 'v8',
            reporter: ['text', 'json', 'html'],
            exclude: [
                'node_modules/',
                'webpack.config.cjs',
                'eslint.config.js',
                '**/*.config.*',
                'wwwroot/',
            ],
        },
    },
});
