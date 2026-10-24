<template>
  <PageLayout title="Upload" back-link="/">
    <UiCard title="Package upload" class="w-full">
      <div class="flex w-full flex-col max-w-6xl mx-auto gap-4">
        <FileDropZone class="max-w-2xl mx-auto" accept=".zip" multiple @add="addPackage" />
        <table v-if="packages.length > 0" class="table-fixed">
          <thead>
            <tr>
              <th class="text-left">File</th>
              <th>
                <label class="flex flex-row items-center gap-2 justify-center">
                  <input v-model="forceAll" type="checkbox" :disabled="isUploading">
                  overwrite existing version
                </label>
              </th>
              <th>Status</th>
              <th>Remove</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="pkg in packages" :key="pkg.id">
              <td class="break-all">{{ pkg.file.name }}</td>
              <td class="text-center">
                <input v-model="pkg.force" type="checkbox" :aria-label="`Overwrite existing version of ${pkg.file.name}`" :disabled="isUploading || pkg.status === 'done'">
              </td>
              <td class="text-center">
                <div class="flex justify-center">
                  <LoadingSpinner v-if="pkg.status === 'uploading'" small />
                  <div v-else-if="pkg.status === 'done'" class="px-2 py-1 bg-green-500 rounded-full text-white w-36">done</div>
                  <button v-else-if="pkg.status === 'error'" type="button" class="px-2 py-1 bg-red-500 rounded-full text-white w-36" @click="shownError = pkg.error">
                    {{ pkg.error.title || 'error' }}
                  </button>
                  <div v-else-if="isUploading" class="px-2 py-1 bg-yellow-500 rounded-full text-white w-36">queued</div>
                </div>
              </td>
              <td class="text-center">
                <button type="button" class="bg-gray-400 text-white px-2 py-1 rounded-sm" title="Remove file" :disabled="isUploading" @click="removePackage(pkg)">×</button>
              </td>
            </tr>
          </tbody>
        </table>
        <div class="w-full flex items-center justify-end gap-2">
          <button v-if="isUploading" type="button" class="btn btn-red-outline" :disabled="isCancelling" @click="isCancelling = true">cancel</button>
          <button type="button" class="btn btn-green" :disabled="isUploading || pendingPackages.length === 0" @click="uploadPackages()">
            {{ isUploading ? 'uploading...' : 'upload' }}
          </button>
        </div>
      </div>
    </UiCard>
    <CardModal :showing="shownError !== null" @close="shownError = null">
      <h4 class="font-light mb-2">{{ shownError?.title }}</h4>
      <div class="break-words">{{ shownError?.detail }}</div>
    </CardModal>
  </PageLayout>
</template>

<script setup>
import { computed, ref } from 'vue';

import { uploadPackage } from '../api.js';
import CardModal from '../components/CardModal.vue';
import FileDropZone from '../components/FileDropZone.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import PageLayout from '../components/PageLayout.vue';
import UiCard from '../components/UiCard.vue';

const packages = ref([]);
const isUploading = ref(false);
const isCancelling = ref(false);
const shownError = ref(null);
let nextId = 1;

// Packages that are not uploaded yet (new or failed)
const pendingPackages = computed(() => packages.value.filter(pkg => pkg.status !== 'done'));

const forceAll = computed({
  get: () => pendingPackages.value.length > 0 && pendingPackages.value.every(pkg => pkg.force),
  set: (value) => pendingPackages.value.forEach((pkg) => { pkg.force = value; })
});

function addPackage (file) {
  const exists = packages.value.some(pkg => pkg.file.name === file.name && pkg.file.size === file.size &&
    pkg.file.lastModified === file.lastModified);
  if (!exists) {
    packages.value.push({ id: nextId++, file, force: false, status: 'pending', error: null });
  }
}

function removePackage (pkg) {
  packages.value = packages.value.filter(item => item !== pkg);
}

async function uploadPackages () {
  isUploading.value = true;
  try {
    for (const pkg of pendingPackages.value) {
      if (isCancelling.value) {
        break;
      }
      pkg.status = 'uploading';
      pkg.error = null;
      try {
        await uploadPackage(pkg.file, pkg.force);
        pkg.status = 'done';
      } catch (error) {
        pkg.status = 'error';
        pkg.error = { title: error.title || 'Upload failed', detail: error.detail || error.message };
      }
    }
  } finally {
    isUploading.value = false;
    isCancelling.value = false;
  }
}
</script>
