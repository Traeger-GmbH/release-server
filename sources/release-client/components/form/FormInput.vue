<template>
  <FormField :title="title" :field-name="name" :required="required">
    <input
      :id="id"
      :name="name"
      class="
        appearance-none
        block
        w-full
        bg-transparent
        text-gray-700
        border-b
        border-b-2
        border-blue-500
        py-2
        px-2
        leading-tight
        focus:outline-none
        focus:bg-white
        focus:border-orange"
      :type="type"
      :disabled="disabled"
      :value="value"
      :placeholder="placeholder"
      :required="required"
      :autocomplete="autocomplete"
      @input="updateValue($event.target.value)"
    />
  </FormField>
</template>

<script>
import FormField from './FormField.vue';
export default {
  components: {
    FormField
  },
  props: {
    title: {
      type: String,
      required: true
    },
    id: {
      type: String,
      required: true
    },
    type: {
      type: String,
      required: true
    },
    required: {
      type: Boolean,
      required: false
    },
    disabled: {
      type: Boolean,
      required: false
    },
    value: {
      type: String,
      default: null
    },
    name: {
      type: String,
      required: true
    },
    placeholder: {
      type: String,
      default: null
    },
    autocomplete: {
      type: String,
      default: 'on'
    }
  },
  methods: {
    updateValue (value) {
      this.$emit('input', value);
    }
  }
};
</script>
