<template>
  <div class="mb-6">
    <slot />
    <FormLabel
      :title="title"
      :related-field="fieldName"
      :required="required"
      class="mt-1"
    />
  </div>
</template>

<script>
import FormLabel from './FormLabel.vue';
export default {
  components: {
    FormLabel
  },
  props: {
    title: {
      type: String,
      required: true
    },
    fieldName: {
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
