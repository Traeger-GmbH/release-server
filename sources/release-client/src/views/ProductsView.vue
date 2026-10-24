<template>
  <PageLayout title="Explore" back-link="/">
    <ProductList :selected-product="product ?? null" />
  </PageLayout>
</template>

<script setup>
import PageLayout from '../components/PageLayout.vue';
import ProductList from '../components/ProductList.vue';

defineProps({
  product: { type: String, default: null }
});
</script>
