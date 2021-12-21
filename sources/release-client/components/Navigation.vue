<template>
  <nav class="bg-gray-800 container mx-auto rounded shadow-xl">
    <div class="flex flex-row">
      <div class="flex flex-row justify-start">
        <NuxtLink
          to="/"
          class="text-white px-4 py-3"
        >
          Start
        </NuxtLink>
        <NuxtLink
          to="/products"
          class="text-white px-4 py-3"
        >
          Explore
        </NuxtLink>
        <NuxtLink
          to="/maintain"
          class="text-white px-4 py-3"
        >
          Maintain
        </NuxtLink>
        <NuxtLink
          to="/upload"
          class="text-white px-4 py-3"
        >
          Upload
        </NuxtLink>
      </div>
      <NuxtLink
        v-if="!isLoggedIn"
        to="/login"
        class="text-white px-4 py-3 ml-auto"
      >
        Login
      </NuxtLink>
      <button
        v-if="isLoggedIn"
        class="text-white px-4 py-3 ml-auto"
        @click="logout()"
      >
        Logout
      </button>
    </div>
  </nav>
</template>

<script>
export default {
  computed: {
    isLoggedIn () {
      return this.$store.state.auth.isLoggedIn;
    }
  },
  methods: {
    logout () {
      this.$api.logout();
      this.$router.push('/login');
    }
  }
};
</script>
