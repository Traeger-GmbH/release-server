<template>
  <div class="flex flex-col flex-grow justify-center items-center gap-4">
    <h1 v-if="error.statusCode === 404">Page not found</h1>
    <h1 v-else-if="error.statusCode === 504">The Release Server is not available.</h1>
    <h1 v-else>An error occurred {{ error }}</h1>
    <NuxtLink
      to="/"
      class="btn btn-green text-center"
    >
      Start
    </NuxtLink>
  </div>
</template>

<script>
export default {
  props: ['error'],
  layout: 'error', // you can set a custom layout for the error page
  methods: {
    reloadPage () {
      console.log('Reload page');
      // this.$forceUpdate();
      this.$router.app.refresh();
    }
  }
};
</script>
