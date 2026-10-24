<template>
  <div class="rounded-sm flex flex-row items-center gap-2 font-semibold px-4 py-4 bg-gray-500 relative">
    <input
      class="rounded-sm px-3 py-2 grow bg-white focus:outline-hidden focus:ring-3 ring-green-500 font-medium"
      type="text"
      :value="modelValue"
      placeholder="Filter"
      autocomplete="off"
      @input="$emit('update:modelValue', $event.target.value)"
      @keyup.esc="$emit('update:modelValue', '')"
    >
    <button
      v-if="modelValue"
      type="button"
      class="absolute right-8 text-gray-500 font-semibold"
      title="Clear filter"
      @click="$emit('update:modelValue', '')"
    >
      ×
    </button>
  </div>
</template>

<script setup>
defineProps({
  modelValue: { type: String, default: '' }
});
defineEmits(['update:modelValue']);
</script>
