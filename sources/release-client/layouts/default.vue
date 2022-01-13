<template>
  <div class="h-screen mx-2 flex flex-col">
    <Navigation class="flex-grow-0 flex-shrink" />
    <div class="container mx-auto flex-grow flex-shrink flex flex-col min-h-0">
      <Nuxt />
    </div>
  </div>
</template>

<script>
export default {
  middleware: 'authentication',
};
</script>
