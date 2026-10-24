<template>
  <div class="bg-white rounded-sm shadow-xl border border-gray-300">
    <div class="py-3 px-4 w-full h-full">
      <div class="w-full text-xl mb-3 pb-2 border-b-2 border-green-500 text-green-500 font-semibold">
        {{ title }}
      </div>
      <div class="w-full text-gray-700">
        <slot />
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true }
});
</script>
