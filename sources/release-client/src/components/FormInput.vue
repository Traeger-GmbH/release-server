<template>
  <div class="mb-6">
    <input
      :id="id"
      :name="name"
      class="appearance-none block w-full bg-transparent text-gray-700 border-b-2 border-blue-500 py-2 px-2 leading-tight focus:outline-hidden focus:bg-white focus:border-orange-500"
      :type="type"
      :value="modelValue"
      :autocomplete="autocomplete"
      :required="required"
      @input="$emit('update:modelValue', $event.target.value)"
    >
    <label class="block tracking-wide text-gray-700 text-xs font-bold px-2 mt-1" :for="id">
      {{ title }}<sup v-if="required">*</sup>
    </label>
  </div>
</template>

<script setup>
defineProps({
  id: { type: String, required: true },
  name: { type: String, required: true },
  title: { type: String, required: true },
  type: { type: String, default: 'text' },
  modelValue: { type: String, default: '' },
  autocomplete: { type: String, default: 'on' },
  required: { type: Boolean, default: false }
});
defineEmits(['update:modelValue']);
</script>
