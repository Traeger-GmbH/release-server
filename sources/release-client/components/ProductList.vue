<template>
  <div class="flex flex-col gap-2">
    <LinkCard
      v-for="product in products"
      :key="product"
      :title="product"
      :link="'/products/' + product"
    />
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
