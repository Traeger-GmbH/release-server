<template>
  <div v-show="show" class="flex flex-col items-center gap-3" role="status">
    <div
      class="animate-spin rounded-full border-green-500 border-t-transparent"
      :class="small ? 'h-6 w-6 border-4' : 'h-20 w-20 border-8'"
    />
    <span v-if="message" class="text-xl">{{ message }}</span>
  </div>
</template>

<script setup>
defineProps({
  message: { type: String, default: '' },
  show: { type: Boolean, default: true },
  small: { type: Boolean, default: false }
});
</script>
