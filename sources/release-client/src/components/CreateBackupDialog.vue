<template>
  <CardModal :showing="showing" class="text-center" @close="close">
    <h4 class="font-light mb-2">Create backup</h4>
    <div class="font-light text-sm text-gray-500 mb-4">
      Creates a backup of all products. The server keeps the newest backups as well.
    </div>
    <div class="flex justify-center my-4 min-h-8">
      <LoadingSpinner v-if="state === 'running'" message="Creating backup..." small />
      <ErrorMessage v-else-if="error" :error="error" />
      <span v-else-if="state === 'done'" class="text-green-500 font-semibold">Successfully created "{{ fileName }}".</span>
    </div>
    <div class="flex justify-around">
      <button v-if="state === 'start'" type="button" class="btn btn-green-outline" @click="close">Cancel</button>
      <button v-if="state === 'start'" type="button" class="btn btn-green" @click="create">Create backup</button>
      <button v-else type="button" class="btn btn-green" :disabled="state === 'running'" @click="close">Finish</button>
    </div>
  </CardModal>
</template>

<script setup>
import { ref } from 'vue';

import { createBackup } from '../api.js';
import { saveBlob } from '../download.js';
import CardModal from './CardModal.vue';
import ErrorMessage from './ErrorMessage.vue';
import LoadingSpinner from './LoadingSpinner.vue';

defineProps({
  showing: { type: Boolean, required: true }
});
const emit = defineEmits(['close']);

const state = ref('start');
const error = ref(null);
const fileName = ref('');

async function create () {
  state.value = 'running';
  error.value = null;
  try {
    const backup = await createBackup();
    fileName.value = backup.fileName;
    saveBlob(backup.blob, backup.fileName);
  } catch (e) {
    error.value = e;
  } finally {
    state.value = 'done';
  }
}

function close () {
  state.value = 'start';
  error.value = null;
  emit('close');
}
</script>
