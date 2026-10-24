<template>
  <div class="flex flex-col gap-2 min-h-0 grow">
    <div v-if="products && products.length > 0" class="shrink grow flex flex-row min-h-0 gap-2 h-full">
      <div class="flex flex-col w-1/5 max-h-full">
        <UiPane class="min-h-0 gap-2 flex flex-col grow">
          <FilterInput v-model="filter" class="-mt-4 -mx-4 mb-2" />
          <div class="overflow-y-auto flex flex-col gap-2 -mx-4 px-4 h-full -my-4 py-4">
            <RouterLink
              v-for="product in filteredProducts"
              :key="product"
              :to="`/products/${encodeURIComponent(product)}`"
              :title="product"
              class="rounded-sm px-3 py-2 font-semibold hover:bg-gray-700 hover:text-white text-left break-all"
              :class="selectedProduct === product ? 'bg-gray-700 text-white' : 'bg-gray-100'"
            >
              {{ product }}
            </RouterLink>
          </div>
        </UiPane>
      </div>
      <div class="grow min-h-0 h-full flex">
        <ReleaseBrowser :product-identifier="selectedProduct" />
      </div>
    </div>
    <div v-else class="flex justify-center items-center grow">
      <LoadingSpinner message="Loading products..." :show="isLoading" />
      <div v-if="!isLoading && products && products.length === 0" class="text-xl">
        No products found.
      </div>
      <div v-if="!isLoading && error" class="flex flex-col gap-4 items-center">
        <ErrorMessage :error="error" class="text-xl" />
        <button type="button" class="btn btn-green" @click="load()">retry</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';

import { getProducts } from '../api.js';
import ErrorMessage from './ErrorMessage.vue';
import FilterInput from './FilterInput.vue';
import LoadingSpinner from './LoadingSpinner.vue';
import ReleaseBrowser from './ReleaseBrowser.vue';
import UiPane from './UiPane.vue';

defineProps({
  selectedProduct: { type: String, default: null }
});

const products = ref(null);
const isLoading = ref(false);
const error = ref(null);
const filter = ref('');

const filteredProducts = computed(() => {
  const term = filter.value.trim().toLowerCase();
  return term ? products.value.filter(product => product.toLowerCase().includes(term)) : products.value;
});

async function load () {
  isLoading.value = true;
  error.value = null;
  try {
    products.value = (await getProducts()).sort((a, b) => a.localeCompare(b));
  } catch (e) {
    error.value = e;
  } finally {
    isLoading.value = false;
  }
}

onMounted(load);
</script>
