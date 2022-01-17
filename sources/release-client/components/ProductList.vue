<template>
  <div class="flex flex-col gap-2 min-h-0 flex-grow">
    <div
      v-if="products && products.length > 0"
      class="flex-shrink flex-grow flex flex-row min-h-0 gap-2 h-full"
    >
      <div class="flex flex-col w-1/5 max-h-full">
        <UiPane class="min-h-0 gap-2 flex flex-col flex-grow">
          <div class="rounded flex flex-row items-center gap-2 font-semibold -mt-4 -mx-4 px-4 py-4 bg-gray-500 mb-2 relative">
            <input
              id="filter"
              class="rounded px-3 py-2 flex-grow focus:outline-none focus:ring ring-green-500 font-medium"
              type="text"
              v-model="filter"
              placeholder="Filter"
              @keyup.esc="filter = null"
              autocomplete="off"
            >
            <button
              v-if="filter && filter.length > 0"
              class="absolute top-6 right-8 text-gray-500 font-semibold"
              @click="filter = null"
            >
              x
            </button>
          </div>
          <div class="overflow-y-scroll flex flex-col gap-2 -mx-4 px-4 h-full -my-4 py-4">
            <button
              v-for="product in filteredProducts"
              :key="product"
              :title="product"
              @click="selectedProduct = product"
              class="bg-gray-100 rounded px-3 py-2 font-semibold hover:bg-gray-700 hover:text-white text-left"
              :class="[
                selectedProduct === product ? 'bg-gray-700 text-white' : ''
              ]"
            >
              {{ product }}
            </button>
          </div>
        </UiPane>
      </div>
      <div class="flex-grow min-h-0 h-full flex">
        <ProductView :product-identifier="selectedProduct" />
      </div>
    </div>
    <div
      v-else
      class="flex justify-center items-center flex-grow"
    >
      <LoadingSpinner
        message="Loading products..."
        :show="isLoading"
      />
      <template
        v-if="!isLoading && error"
      >
        <div
          v-if="!isLoading && error.statusCode === 404"
          class="text-xl"
        >
          No products found.
        </div>
        <div
          v-else-if="!isLoading"
          class="flex flex-col gap-4"
        >
          <span class="text-xl">{{ error }}</span>
          <button
            class="btn btn-green self-center"
            @click="$fetch()"
          >
            retry
          </button>
        </div>
      </template>
    </div>
  </div>
</template>

<script>
export default {
  data () {
    return {
      selectedProduct: null,
      products: null,
      isLoading: false,
      filter: null,
      error: null
    };
  },
  async fetch () {
    try {
      this.isLoading = true;
      this.products = (await this.$api.getProductList()).sort();
    } catch (error) {
      this.error = error;
    } finally {
      this.isLoading = false;
    }
  },
  computed: {
    hasSelection () {
      return this.selectedProduct !== null;
    },
    filteredProducts () {
      if (this.filter !== null) {
        return this.products.filter(product => product.includes(this.filter));
      } else {
        return this.products;
      }
    }
  },
  methods: {
    select (product) {
      this.selectedProduct = product;
    },
    isSelected (product) {
      return product === this.selectedProduct;
    }
  }
};
</script>

<style>

</style>
