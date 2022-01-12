<template>
  <div class="">
    <div class="flex items-center mb-5">
      <h1 class="mx-5">{{title}}</h1>
      <NuxtLink
        v-if="backLink"
        :to="backLink"
        class="btn btn-green flex justify-center"
      >
        go back
      </NuxtLink>
      <div class="flex w-full">
        <slot name="header"></slot>
      </div>
    </div>
    <slot />
  </div>
</template>

<script>
export default {
  props: {
    title: {
      type: String,
      required: true
    },
    backLink: {
      type: String,
      required: false,
      default: null
    }
  }
};
</script>
