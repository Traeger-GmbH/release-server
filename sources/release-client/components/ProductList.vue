<template>
  <div class="flex flex-col gap-2 min-h-0">
    <div
      v-if="products && products.length > 0"
      class="flex-shrink flex-grow flex flex-row min-h-0"
    >
      <div class="flex-shrink-0 flex-grow-0 overflow-auto min-h-0 gap-2 flex flex-col">
        <LinkCard
          v-for="product in products"
          :key="product"
          :title="product"
          :link="'/products/' + product"
        />
      </div>
    </div>
    <div
      v-else
    >
      <div
        v-if="isLoading"
      >
        <VueSpinner />
        loading products...
      </div>
      <div
        v-else
      >
        No products found.
      </div>
    </div>
  </div>
</template>

<script>
import VueSpinner from 'vue-simple-spinner';
function sleep (milliseconds) {
  return new Promise(resolve => setTimeout(resolve, milliseconds));
}

export default {
  components: {
    VueSpinner
  },
  data () {
    return {
      selectedProduct: null,
      products: null,
      isLoading: false
    };
  },
  async fetch () {
    console.log('fetch()...');
    try {
      this.isLoading = true;
      while (true) {
        try {
          console.log('fetching products...');
          this.products = await this.$api.getProductList();
          return;
        } catch (error) {
          console.log('catching error...');
          console.log(error.response.status);
          if (error.response.status === 504) {
            console.log('waiting to retry...');
            sleep(2000);
            console.log('retrying...');
          } else {
            throw error;
          }
        }
      }
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
