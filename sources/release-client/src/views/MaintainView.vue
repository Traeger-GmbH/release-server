<template>
  <PageLayout title="Maintain" back-link="/">
    <div class="flex flex-col gap-4">
      <UiCard title="Statistics">
        <LoadingSpinner v-if="isLoading" small />
        <ErrorMessage :error="error" />
        <div v-if="statistics" class="flex flex-wrap gap-8">
          <div>
            <div class="text-xl font-semibold mb-2">Disk Usage</div>
            <table class="table-auto">
              <tbody>
                <tr><td class="pr-4">Total size:</td><td class="font-semibold">{{ statistics.disk.totalSize }} MB</td></tr>
                <tr><td class="pr-4">Used by artifacts:</td><td class="font-semibold">{{ statistics.disk.usedDiskSpace }} MB</td></tr>
                <tr>
                  <td class="pr-4">Available space:</td>
                  <td class="font-semibold">{{ statistics.disk.availableFreeSpace }} MB</td>
                  <td v-if="freePercent !== null" class="pl-2">
                    = <span class="text-white bg-green-500 rounded-sm px-2 py-1">{{ freePercent }} %</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <div>
            <div class="text-xl font-semibold mb-2">Artifacts</div>
            <table class="table-auto">
              <tbody>
                <tr><td class="pr-4">Number of products:</td><td class="font-semibold">{{ statistics.numberOfProducts }}</td></tr>
                <tr><td class="pr-4">Number of artifacts:</td><td class="font-semibold">{{ statistics.numberOfArtifacts }}</td></tr>
              </tbody>
            </table>
          </div>
        </div>
      </UiCard>
      <UiCard title="Backup">
        <div class="flex flex-wrap gap-2 justify-around">
          <button type="button" class="btn btn-green" @click="showCreateBackupDialog = true">Create Backup</button>
          <button type="button" class="btn btn-green-outline" @click="showRestoreDialog = true">Restore Backup</button>
        </div>
        <CreateBackupDialog :showing="showCreateBackupDialog" @close="showCreateBackupDialog = false" />
        <RestoreBackupDialog :showing="showRestoreDialog" @close="showRestoreDialog = false" @restored="load" />
      </UiCard>
    </div>
  </PageLayout>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';

import { getStatistics } from '../api.js';
import CreateBackupDialog from '../components/CreateBackupDialog.vue';
import ErrorMessage from '../components/ErrorMessage.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import PageLayout from '../components/PageLayout.vue';
import RestoreBackupDialog from '../components/RestoreBackupDialog.vue';
import UiCard from '../components/UiCard.vue';

const statistics = ref(null);
const isLoading = ref(false);
const error = ref(null);
const showCreateBackupDialog = ref(false);
const showRestoreDialog = ref(false);

const freePercent = computed(() => {
  const disk = statistics.value?.disk;
  return disk && disk.totalSize > 0 ? (disk.availableFreeSpace / disk.totalSize * 100).toFixed(2) : null;
});

async function load () {
  isLoading.value = true;
  error.value = null;
  try {
    statistics.value = await getStatistics();
  } catch (e) {
    error.value = e;
  } finally {
    isLoading.value = false;
  }
}

onMounted(load);
</script>
