<template>
  <Page :title="productIdentifier" back-link="/products">
    <ProductView :product-identifier="productIdentifier" />
  </Page>
</template>

<script>
export default {
  asyncData ({ params }) {
    return { productIdentifier: params.product };
  }
};
</script>
