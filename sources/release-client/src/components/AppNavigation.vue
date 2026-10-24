<template>
  <nav class="bg-gray-800 container mx-auto rounded-sm shadow-xl">
    <div class="flex flex-row">
      <div class="flex flex-row justify-start">
        <RouterLink v-for="link in links" :key="link.to" :to="link.to" class="text-white px-4 py-3">
          {{ link.title }}
        </RouterLink>
      </div>
      <RouterLink v-if="!isLoggedIn" to="/login" class="text-white px-4 py-3 ml-auto">
        Login
      </RouterLink>
      <button v-else type="button" class="text-white px-4 py-3 ml-auto" :title="`Logged in as ${username}`" @click="logout()">
        Logout
      </button>
    </div>
  </nav>
</template>

<script setup>
import { logout } from '../api.js';
import { isLoggedIn, username } from '../auth.js';

const links = [
  { to: '/', title: 'Start' },
  { to: '/products', title: 'Explore' },
  { to: '/maintain', title: 'Maintain' },
  { to: '/upload', title: 'Upload' }
];
</script>
