<template>
  <div
    class="flex items-center justify-center w-full text-center"
    id="app"
  >
    <div
      class="p-12 bg-white h-36 text-center rounded w-full border-2 border-dashed border-gray-300"
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
        multiple
      />

      <label for="assetsFieldHandle" class="block cursor-pointer">
        <div>
          Drop your files here
          or <span class="underline">click here</span>
          to open the file browser.
        </div>
      </label>
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
      files: [],
    };
  },
  // watch: {
  //   value (value) {
  //     this.files = value;
  //   }
  // },
  methods: {
    onChange () {
      this.files = this.$refs.file.files;
      console.log(this.files);
      for (let i = 0; i < this.files.length; i++) {
        this.$emit('add', this.files[i]);
      }
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
      console.log('drop');
      console.log(event.dataTransfer);
      this.$refs.file.files = event.dataTransfer.files;
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
