<template>
  <UiPane class="bg-white flex flex-col gap-2">
    <h2 class="mb-2 capitalize">{{ product }}</h2>
    <div
      :title="product"
      v-for="product in products"
      :key="product"
      class="p-2 rounded capitalize bg-gray-300 text-black hover:text-white cursor-pointer hover:bg-green-500"
      :class="[
        isSelected(product) ? 'bg-green-500 text-white' : ''
      ]"
      @click="select(product)"
    >
      {{ product }}
    </div>
  </UiPane>
</template>

<script>
export default {
  props: {
    product: {
      type: String,
      required: true
    }
  },
  async fetch () {
  }
};
</script>

<style>

</style>
