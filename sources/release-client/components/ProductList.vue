<template>
  <div class="flex flex-col gap-2">
    <!-- <UiPane class="bg-white w-1/4 flex flex-col gap-2"> -->
      <!-- <h2 class="mb-2">Products</h2> -->
      <LinkCard
        :title="product"
        :link="'/products/' + product"
        v-for="product in products"
        :key="product"
      >
        {{ product }}
      </LinkCard>
    <!-- </UiPane> -->
  </div>
</template>

<script>
export default {
  props: {
    products: {
      type: Array,
      required: true
    }
  },
  data () {
    return {
      selectedProduct: null
    };
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
