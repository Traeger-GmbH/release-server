<template>
  <Page title="Upload" back-link="/">
    <div class="flex flex-row w-full gap-2">
      <div class="flex w-full flex-col gap-2">
        <div
          v-if="!packageFile"
          class="p-12 bg-white h-36 text-center border-2 border-dashed border-gray-300 rounded"
          @dragover="dragover"
          @dragleave="dragleave"
          @drop="drop"
        >
          <label for="assetsFieldHandle" class="block cursor-pointer">
            <div>
              Drop your package file here
              or <span class="underline">click here</span> to open the file browser.
            </div>
          </label>
        </div>
        <UiPane
          v-if="packageFile"
          class="bg-white h-36 flex flex-col justify-center items-center gap-2"
        >
          <div>
            {{ packageFile.name }}
          </div>
          <button
            class="btn btn-green"
            type="button"
            title="Remove file"
            @click="packageFile = null"
          >
            remove
          </button>
        </UiPane>
        <UiPane
          v-if="error"
          class="w-full flex justify-center text-red-500 font-bold"
        >
          {{ error }}
        </UiPane>
        <div class="w-full flex items-center">
          <div
            v-if="successMessage"
            class="w-full flex justify-center text-green-500 font-bold"
          >
            {{ successMessage }}
          </div>
          <button
            class="btn btn-green ml-auto"
            type="button"
            title="Upload package"
            :disabled="isUploading || this.file == null"
            @click="upload()"
          >
            upload
          </button>
        </div>
      </div>
    </div>
  </Page>
</template>

<script>
export default {
  data () {
    return {
      isUploading: false,
      packageFile: null,
      error: null,
      successMessage: null
    };
  },
  methods: {
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
      this.packageFile = event.dataTransfer.files[0];
      // Clean up
      event.currentTarget.classList.add('bg-gray-100');
      event.currentTarget.classList.remove('bg-green-300');
    },
    async upload () {
      this.successMessage = null;
      if (!this.packageFile) {
        this.error = 'Please select a package file you want to upload!';
        return;
      }
      this.error = null;
      try {
        this.isUploading = true;
        await this.$api.uploadPackage(this.packageFile);
        this.successMessage = `Successfully uploaded "${this.packageFile.name}".`;
        this.packageFile = null;
      } catch (error) {
        const responseData = error.response.data;
        // build error message:
        let message = responseData.title;
        if (responseData.detail) {
          message += `: ${responseData.detail}`;
        }
        this.error = message;
      } finally {
        this.isUploading = false;
      }
    }
  }
};
</script>

<style>

</style>
