<template>
  <div class="transition-all ease-out duration-500 rounded max-w-sm h-32 bg-white hover:bg-gray-700 group shadow-xl border border-gray-300">
    <NuxtLink :to="link" class="w-full h-full block">
      <div class="py-3 px-4 w-full h-full">
        <div class="w-full text-xl mb-3 pb-2 border-b-2 border-gray-700 border-green-500 text-green-500 font-semibold">
          {{ title }}
        </div>
        <div class="w-full text-normal text-gray-700 group-hover:text-gray-400">
          {{ description }}
        </div>
      </div>
    </NuxtLink>
  </div>
</template>

<script>
export default {
  props: {
    title: {
      type: String,
      required: true
    },
    link: {
      type: String,
      required: true
    },
    description: {
      type: String,
      required: false,
      default: ''
    }
  }
};
</script>
