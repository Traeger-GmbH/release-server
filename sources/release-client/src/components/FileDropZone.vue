<template>
  <div
    class="p-12 bg-white h-36 text-center rounded-sm w-full border-2 border-dashed border-gray-300"
    :class="{ 'bg-green-300': dragging }"
    @dragover.prevent="dragging = true"
    @dragleave="dragging = false"
    @drop.prevent="onDrop"
  >
    <input
      :id="inputId"
      ref="input"
      type="file"
      class="w-px h-px opacity-0 overflow-hidden absolute"
      :accept="accept"
      :multiple="multiple"
      @change="onChange"
    >
    <label v-if="multiple || !modelValue" :for="inputId" class="block cursor-pointer">
      Drop your {{ multiple ? 'files' : 'file' }} here or <span class="underline">click here</span> to open the file browser.
    </label>
    <div v-else>
      {{ modelValue.name }}
      <button type="button" class="bg-green-500 text-white px-2 py-1 inline-block rounded-sm" title="Remove file" @click="select([])">
        ×
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, useId } from 'vue';

const props = defineProps({
  accept: { type: String, default: '' },
  multiple: { type: Boolean, default: false },
  modelValue: { type: File, default: null }
});
// "update:modelValue" for a single file, "add" for each file in multiple mode
const emit = defineEmits(['update:modelValue', 'add']);

const inputId = useId();
const input = ref(null);
const dragging = ref(false);

function select (files) {
  if (props.multiple) {
    files.forEach(file => emit('add', file));
  } else {
    emit('update:modelValue', files[0] ?? null);
  }
  // Allow selecting the same file again
  if (input.value) {
    input.value.value = '';
  }
}

function onChange (event) {
  select(Array.from(event.target.files));
}

function onDrop (event) {
  dragging.value = false;
  select(Array.from(event.dataTransfer.files));
}
</script>
