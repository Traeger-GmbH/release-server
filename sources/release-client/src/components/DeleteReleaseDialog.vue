<template>
  <CardModal :showing="showing" class="text-center" @close="close">
    <h4 class="font-light mb-2">Delete release</h4>
    <div class="font-light text-sm text-gray-500 mb-4">
      Deletes the release on all its platforms ({{ release.platforms.join(', ') }}).
    </div>
    <template v-if="state === 'confirm'">
      <label class="block mb-2" :for="inputId">Confirm by typing the name of the release:</label>
      <div class="font-semibold p-1 rounded-sm bg-green-200 mb-2">{{ releaseName }}</div>
      <input :id="inputId" v-model="confirmation" class="border border-gray-300 w-full p-2 rounded-sm text-center font-semibold" type="text" autocomplete="off">
      <div class="flex justify-around mt-6">
        <button type="button" class="btn btn-green-outline" @click="close">Cancel</button>
        <button type="button" class="btn btn-green" :disabled="confirmation !== releaseName" @click="deleteNow">Delete</button>
      </div>
    </template>
    <template v-else>
      <div class="flex justify-center my-4">
        <LoadingSpinner v-if="state === 'running'" message="Deleting release..." small />
        <ErrorMessage v-else-if="error" :error="error" />
        <span v-else class="text-green-500 font-semibold">Successfully deleted "{{ releaseName }}".</span>
      </div>
      <div class="flex justify-around">
        <button v-if="state === 'done' && error" type="button" class="btn btn-green-outline" @click="state = 'confirm'">Back</button>
        <button type="button" class="btn btn-green" :disabled="state === 'running'" @click="close">Finish</button>
      </div>
    </template>
  </CardModal>
</template>

<script setup>
import { computed, ref, useId } from 'vue';

import { deleteRelease } from '../api.js';
import CardModal from './CardModal.vue';
import ErrorMessage from './ErrorMessage.vue';
import LoadingSpinner from './LoadingSpinner.vue';

const props = defineProps({
  productIdentifier: { type: String, required: true },
  release: { type: Object, required: true },
  showing: { type: Boolean, required: true }
});
const emit = defineEmits(['close', 'deleted']);

const inputId = useId();
const state = ref('confirm');
const confirmation = ref('');
const error = ref(null);

const releaseName = computed(() => `${props.productIdentifier}-v${props.release.version}`);

async function deleteNow () {
  state.value = 'running';
  error.value = null;
  try {
    await deleteRelease(props.productIdentifier, props.release.version, props.release.platforms);
  } catch (e) {
    error.value = e;
  } finally {
    state.value = 'done';
  }
}

function close () {
  const deleted = state.value === 'done' && !error.value;
  state.value = 'confirm';
  confirmation.value = '';
  error.value = null;
  emit('close');
  if (deleted) {
    emit('deleted');
  }
}
</script>
