import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import tailwindcss from '@tailwindcss/vite';

// During development the API requests are forwarded to a local release server
// (in production nginx forwards them, see nginx/default.conf.template).
const releaseServerUrl = process.env.RELEASE_SERVER_URL || 'http://localhost:5000';

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    port: 3000,
    proxy: {
      '/artifacts': {
        target: releaseServerUrl,
        changeOrigin: true
      }
    }
  },
  test: {
    environment: 'happy-dom',
    include: ['tests/**/*.test.js'],
    restoreMocks: true
  }
});
