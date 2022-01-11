<template>
  <div
    class="flex items-center justify-center w-full text-center"
    id="app"
  >
    <div
      class="p-12 bg-gray-100 border border-gray-300 w-full"
      @dragover="dragover"
      @dragleave="dragleave"
      @drop="drop"
    >
      <input
        type="file"
        name="file"
        id="assetsFieldHandle"
        class="w-px h-px opacity-0 overflow-hidden absolute"
        @change="onChange()"
        ref="file"
        :accept="accept"
      />

      <label v-if="!file" for="assetsFieldHandle" class="block cursor-pointer">
        <div>
          Drop your backup file here
          or <span class="underline">click here</span>
          to open the file browser.
        </div>
      </label>
      <div v-if="file">
        {{ file.name }} <button
          type="button"
          @click="remove"
          title="Remove file"
        >
          x
        </button>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    accept: {
      type: String,
      required: false,
      default: ''
    }
  },
  data () {
    return {
      file: null,
    };
  },
  methods: {
    onChange () {
      this.file = this.$refs.file.files[0];
      this.$emit('input', this.file);
    },
    remove () {
      this.file = null;
    },
    dragover (event) {
      event.preventDefault();
      // Add some visual fluff to show the user can drop its files
      if (!event.currentTarget.classList.contains('bg-green-300')) {
        event.currentTarget.classList.remove('bg-gray-100');
        event.currentTarget.classList.add('bg-green-300');
      }
    },
    dragleave (event) {
      // Clean up
      event.currentTarget.classList.add('bg-gray-100');
      event.currentTarget.classList.remove('bg-green-300');
    },
    drop (event) {
      event.preventDefault();
      this.$refs.file.files = [];
      this.$refs.file.files[0] = event.dataTransfer.files[0];
      this.onChange(); // Trigger the onChange event manually
      // Clean up
      event.currentTarget.classList.add('bg-gray-100');
      event.currentTarget.classList.remove('bg-green-300');
    },
  },
};
</script>

<style scoped>
[v-cloak] {
  display: none;
}
</style>
