<template>
  <Page title="Explore" backLink="/">
    <ProductList
      class="mx-auto"
      :products="products"
    />
  </Page>
</template>

<script>
export default {
  name: 'ProductsOverviewPage',
  async asyncData ({ $api }) {
    const products = await $api.getProductList();
    return { products };
  }
};
</script>
