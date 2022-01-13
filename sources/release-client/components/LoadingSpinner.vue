<template>
  <div
    class=""
    v-show="show"
  >
    <VueSpinner
      size="80"
      speed="1.5"
      spacing="25"
      line-size="12"
      line-fg-color="#10b981"
      :message="message"
      font-size="20"
    />
  </div>
</template>

<script>
import VueSpinner from 'vue-simple-spinner';

export default {
  components: {
    VueSpinner
  },
  props: {
    message: {
      type: String,
      required: false,
      default: ''
    },
    show: {
      type: Boolean,
      required: false,
      default: true
    }
  }
};
</script>

<style>

</style>
