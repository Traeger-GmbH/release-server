<template>
  <div class="flex flex-col flex-grow flex-shrink min-h-0">
    <div class="flex flex-row items-center flex-grow-0 flex-shrink py-5">
      <div class="flex-grow flex flex-row items-center">
        <h1 class="mr-5">{{title}}</h1>
        <NuxtLink
          v-if="backLink"
          :to="backLink"
          class="btn btn-green flex justify-center flex-grow-0"
        >
          go back
        </NuxtLink>
      </div>
      <div class="flex flex-grow">
        <slot name="header"></slot>
      </div>
    </div>
    <div class="flex-grow flex-shrink min-h-0 flex flex-col pb-2">
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
