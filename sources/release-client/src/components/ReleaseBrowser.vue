<template>
  <div class="flex grow shrink gap-2 min-h-0">
    <UiPane class="flex flex-col gap-2 w-2/6 overflow-y-auto">
      <template v-if="!productIdentifier">
        <div class="h-full w-full flex justify-center items-center text-lg font-medium">
          No product selected.
        </div>
      </template>
      <template v-else-if="product">
        <FilterInput v-model="filter" class="-mt-4 -mx-4 mb-2" />
        <div v-if="product.releases.length === 0" class="text-lg font-medium text-center">
          No releases found for this product.
        </div>
        <div class="overflow-y-auto flex flex-col gap-2 -mx-4 px-4 h-full -my-4 py-4">
          <button
            v-for="release in filteredReleases"
            :key="release.version"
            type="button"
            :title="release.version"
            class="rounded-sm px-3 py-2 hover:bg-gray-700 hover:text-white tabular-nums font-semibold flex justify-between"
            :class="release === selectedRelease ? 'bg-gray-700 text-white' : ''"
            @click="selectedRelease = release"
          >
            <span class="flex items-center">
              v{{ release.version }}
              <span v-if="release.isPreviewRelease" class="ml-2 w-2 h-2 rounded-xl bg-blue-500" title="preview" />
              <span v-if="release.isSecurityPatch" class="ml-2 w-2 h-2 rounded-xl bg-red-500" title="security patch" />
            </span>
            <span>[{{ formatDate(release.releaseDate) }}]</span>
          </button>
        </div>
      </template>
      <template v-else>
        <div class="grow flex items-center justify-center">
          <LoadingSpinner message="Loading releases..." :show="isLoading" />
          <div v-if="!isLoading && error && error.status === 404" class="text-xl">
            No releases found for this product.
          </div>
          <div v-else-if="!isLoading && error" class="flex flex-col gap-4 items-center">
            <ErrorMessage :error="error" class="text-xl" />
            <button type="button" class="btn btn-green" @click="load()">retry</button>
          </div>
        </div>
      </template>
    </UiPane>
    <UiPane v-if="selectedRelease" class="bg-white flex flex-col grow overflow-y-auto gap-2 w-2/3">
      <ReleaseInformation
        :release="selectedRelease"
        :product-identifier="product.identifier"
        @deleted="removeSelectedRelease()"
      />
    </UiPane>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue';

import { getProductInfo } from '../api.js';
import { formatDate } from '../format.js';
import ErrorMessage from './ErrorMessage.vue';
import FilterInput from './FilterInput.vue';
import LoadingSpinner from './LoadingSpinner.vue';
import ReleaseInformation from './ReleaseInformation.vue';
import UiPane from './UiPane.vue';

const props = defineProps({
  productIdentifier: { type: String, default: null }
});

const product = ref(null);
const selectedRelease = ref(null);
const isLoading = ref(false);
const error = ref(null);
const filter = ref('');

const filteredReleases = computed(() => {
  // A leading "v" is allowed in the filter ("v1.2" finds "1.2.0")
  const term = filter.value.trim().replace(/^v/i, '');
  return term ? product.value.releases.filter(release => release.version.startsWith(term)) : product.value.releases;
});

async function load () {
  product.value = null;
  selectedRelease.value = null;
  error.value = null;
  filter.value = '';
  if (!props.productIdentifier) {
    return;
  }

  const requested = props.productIdentifier;
  isLoading.value = true;
  try {
    const info = await getProductInfo(requested);
    // Ignore the response if another product was selected in the meantime
    if (requested === props.productIdentifier) {
      product.value = info;
      selectedRelease.value = info.releases[0] ?? null;
    }
  } catch (e) {
    if (requested === props.productIdentifier) {
      error.value = e;
    }
  } finally {
    isLoading.value = false;
  }
}

function removeSelectedRelease () {
  product.value.releases = product.value.releases.filter(release => release !== selectedRelease.value);
  selectedRelease.value = product.value.releases[0] ?? null;
}

watch(() => props.productIdentifier, load, { immediate: true });
</script>
