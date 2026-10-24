import { watch } from 'vue';
import { createRouter, createWebHistory } from 'vue-router';

import { isLoggedIn } from './auth.js';
import HomeView from './views/HomeView.vue';
import LoginView from './views/LoginView.vue';
import MaintainView from './views/MaintainView.vue';
import NotFoundView from './views/NotFoundView.vue';
import ProductsView from './views/ProductsView.vue';
import UploadView from './views/UploadView.vue';

export const routes = [
  { path: '/', component: HomeView },
  { path: '/login', component: LoginView, meta: { public: true } },
  { path: '/products', component: ProductsView },
  { path: '/products/:product', component: ProductsView, props: true },
  { path: '/maintain', component: MaintainView },
  { path: '/upload', component: UploadView },
  { path: '/:pathMatch(.*)*', component: NotFoundView, meta: { public: true } }
];

export function createAppRouter (history = createWebHistory()) {
  const router = createRouter({ history, routes });

  // The UI is meant for administrators only: every page except the login requires a login.
  router.beforeEach((to) => {
    if (!to.meta.public && !isLoggedIn.value) {
      return { path: '/login', query: { afterLogin: to.fullPath } };
    }
    return true;
  });

  // Leave protected pages when the login ends (logout or rejected credentials).
  watch(isLoggedIn, (loggedIn) => {
    const current = router.currentRoute.value;
    if (!loggedIn && !current.meta.public) {
      router.push({ path: '/login', query: { afterLogin: current.fullPath } });
    }
  });

  return router;
}

export const router = createAppRouter();
