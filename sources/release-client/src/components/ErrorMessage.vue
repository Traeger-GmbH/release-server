<template>
  <div v-if="error" class="text-red-500 font-semibold break-words" role="alert">
    {{ text }}
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  error: { type: [Error, String, Object], default: null }
});

const text = computed(() => {
  if (!props.error) {
    return '';
  }
  return typeof props.error === 'string' ? props.error : (props.error.message || String(props.error));
});
</script>
