<template>
  <div class="h-screen mx-2 flex flex-col">
    <AppNavigation v-if="!route.meta.public || isLoggedIn" class="grow-0 shrink mt-1" />
    <div class="container mx-auto grow shrink flex flex-col min-h-0">
      <RouterView v-slot="{ Component }">
        <Transition name="page" mode="out-in">
          <component :is="Component" />
        </Transition>
      </RouterView>
    </div>
  </div>
</template>

<script setup>
import { useRoute } from 'vue-router';

import { isLoggedIn } from './auth.js';
import AppNavigation from './components/AppNavigation.vue';

const route = useRoute();
</script>
