<template>
  <div class="flex flex-col grow shrink min-h-0">
    <div class="flex flex-row items-center grow-0 shrink py-5">
      <div class="grow flex flex-row items-center">
        <h1 class="mr-5 break-all">{{ title }}</h1>
        <RouterLink v-if="backLink" :to="backLink" class="btn btn-green flex justify-center grow-0">
          go back
        </RouterLink>
      </div>
      <div class="flex grow">
        <slot name="header" />
      </div>
    </div>
    <div class="grow shrink min-h-0 flex flex-col pb-2">
      <slot />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  backLink: { type: String, default: null }
});
</script>
