<template>
  <CardModal :showing="showing" @close="close">
    <h4 class="font-light mb-2 text-center">Restore backup</h4>
    <div class="font-light text-sm text-gray-500 mb-4 text-center">
      Replaces all products by the content of the backup. The backup is checked first; if it is invalid,
      the existing data stays unchanged.
    </div>
    <template v-if="state === 'select'">
      <FileDropZone v-model="file" accept=".zip" />
      <div class="flex justify-around mt-6">
        <button type="button" class="btn btn-green-outline" @click="close">Cancel</button>
        <button type="button" class="btn btn-green" :disabled="!file" @click="restore">Restore</button>
      </div>
    </template>
    <template v-else>
      <div class="flex justify-center my-4 text-center">
        <LoadingSpinner v-if="state === 'running'" message="Restoring..." small />
        <ErrorMessage v-else-if="error" :error="error" />
        <span v-else class="text-green-500 font-semibold">Successfully restored "{{ restoredFileName }}".</span>
      </div>
      <div class="flex justify-around">
        <button v-if="state === 'done' && error" type="button" class="btn btn-green-outline" @click="state = 'select'">Back</button>
        <button type="button" class="btn btn-green" :disabled="state === 'running'" @click="close">Finish</button>
      </div>
    </template>
  </CardModal>
</template>

<script setup>
import { ref } from 'vue';

import { restoreBackup } from '../api.js';
import CardModal from './CardModal.vue';
import ErrorMessage from './ErrorMessage.vue';
import FileDropZone from './FileDropZone.vue';
import LoadingSpinner from './LoadingSpinner.vue';

defineProps({
  showing: { type: Boolean, required: true }
});
const emit = defineEmits(['close', 'restored']);

const state = ref('select');
const file = ref(null);
const error = ref(null);
const restoredFileName = ref('');

async function restore () {
  state.value = 'running';
  error.value = null;
  try {
    await restoreBackup(file.value);
    restoredFileName.value = file.value.name;
    file.value = null;
    emit('restored');
  } catch (e) {
    error.value = e;
  } finally {
    state.value = 'done';
  }
}

function close () {
  state.value = 'select';
  error.value = null;
  emit('close');
}
</script>
