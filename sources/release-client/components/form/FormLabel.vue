<template>
  <label class="block tracking-wide text-gray-700 text-xs font-bold px-2" :for="relatedField">
    {{ title }}<template v-if="required"><sup>*</sup></template>
  </label>
</template>

<script>
export default {
  props: {
    title: {
      type: String,
      required: true
    },
    relatedField: {
      type: String,
      required: true
    },
    required: {
      type: Boolean,
      required: false
    }
  }
};
</script>
