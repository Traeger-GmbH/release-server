<template>
  <Teleport to="body">
    <Transition name="fade">
      <div
        v-if="showing"
        class="fixed inset-0 z-10 flex items-center justify-center bg-black/75"
        role="dialog"
        aria-modal="true"
        @click.self="close"
        @keydown.esc="close"
      >
        <div class="relative mx-10 w-full max-w-xl bg-white shadow-lg rounded-sm p-8">
          <button type="button" aria-label="close" class="absolute top-0 right-0 text-xl text-gray-500 my-2 mx-4" @click.prevent="close">
            ×
          </button>
          <slot />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { onBeforeUnmount, watch } from 'vue';

const props = defineProps({
  showing: { type: Boolean, required: true }
});
const emit = defineEmits(['close']);

function close () {
  emit('close');
}

// Prevent scrolling of the page behind the dialog
watch(() => props.showing, (showing) => {
  document.body.classList.toggle('overflow-hidden', showing);
});
onBeforeUnmount(() => document.body.classList.remove('overflow-hidden'));
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
