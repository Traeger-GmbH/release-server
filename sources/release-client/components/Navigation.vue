<template>
  <nav class="bg-gray-800 container mx-auto rounded shadow-xl">
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
  </nav>
</template>

<script>
export default {
};
</script>
