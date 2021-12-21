<template>
  <div class="h-screen mx-2">
    <Navigation class="mb-10" />
    <div class="container mx-auto">
      <Nuxt />
    </div>
  </div>
</template>

<script>
export default {
  middleware: 'authentication',
};
</script>
