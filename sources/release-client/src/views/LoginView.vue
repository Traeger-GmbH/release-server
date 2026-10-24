<template>
  <div class="h-screen flex items-center">
    <form class="w-full max-w-sm mx-auto mt-5" @submit.prevent="submit">
      <h3 class="mb-8 text-center">Release Server</h3>
      <FormInput id="username" v-model="usernameInput" name="username" title="Username" autocomplete="username" required />
      <FormInput id="password" v-model="password" name="password" title="Password" type="password" autocomplete="current-password" required />
      <ErrorMessage :error="error" class="mb-4" />
      <div class="w-full flex justify-end">
        <button class="btn btn-green" type="submit" :disabled="isLoading || !usernameInput || !password">
          {{ isLoading ? 'logging in...' : 'login' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { ApiError, login } from '../api.js';
import ErrorMessage from '../components/ErrorMessage.vue';
import FormInput from '../components/FormInput.vue';

const route = useRoute();
const router = useRouter();

const usernameInput = ref('');
const password = ref('');
const isLoading = ref(false);
const error = ref(null);

function targetAfterLogin () {
  // Only redirect to paths of this application
  const target = route.query.afterLogin;
  return typeof target === 'string' && target.startsWith('/') && !target.startsWith('//') ? target : '/';
}

async function submit () {
  isLoading.value = true;
  error.value = null;
  try {
    await login(usernameInput.value, password.value);
    password.value = '';
    await router.push(targetAfterLogin());
  } catch (e) {
    error.value = e instanceof ApiError && e.status === 401 ? 'Wrong username or password.' : e;
  } finally {
    isLoading.value = false;
  }
}
</script>
