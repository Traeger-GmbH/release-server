<template>
  <div class="flex flex-col flex-grow flex-shrink min-h-0">
    <div class="flex items-center flex-grow-0 flex-shrink py-5">
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
    <div class="flex-grow flex-shrink min-h-0 flex flex-col">
      <slot />
    </div>
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
