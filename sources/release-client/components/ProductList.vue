<template>
  <div class="flex flex-col gap-2 min-h-0 flex-grow">
    <div
      v-if="products && products.length > 0"
      class="flex-shrink flex-grow flex flex-col min-h-0"
    >
      <div class="flex flex-col w-1/3 flex-grow">
        <div class="pb-2 flex flex-row items-center gap-2 font-semibold">
          <input
            id="filter"
            class="bg-gray-100 rounded px-3 py-2 flex-grow"
            type="text"
            v-model="filter"
            placeholder="Filter products"
          />
        </div>
        <UiPane class="overflow-auto min-h-0 gap-2 flex flex-col flex-grow">
          <NuxtLink
            v-for="product in filteredProducts"
            :key="product"
            :title="product"
            :to="'/products/' + product"
            class="bg-gray-100 rounded px-3 py-2 font-semibold hover:bg-gray-700 hover:text-white"
          >
            {{ product }}
          </NuxtLink>
        </UiPane>
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
      <div
        v-if="!isLoading"
        class="text-xl"
      >
        No products found.
      </div>
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
      filter: null
    };
  },
  async fetch () {
    try {
      this.isLoading = true;
      this.products = await this.$api.getProductList();
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
