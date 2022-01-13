<template>
  <div class="flex flex-col gap-2 min-h-0 flex-grow">
    <div
      v-if="products && products.length > 0"
      class="flex-shrink flex-grow flex flex-row min-h-0"
    >
      <UiPane class="overflow-auto min-h-0 gap-2 flex flex-col w-1/3">
        <LinkCard
          v-for="product in products"
          :key="product"
          :title="product"
          :link="'/products/' + product"
        />
      </UiPane>
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
      isLoading: false
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
