<template>
  <Page title="Upload" back-link="/">
    <UiCard title="Upload package" class="flex flex-row w-full gap-2">
      <div class="flex w-full flex-col gap-2 max-w-2xl mx-auto">
        <FileSelector v-model="packageFile" accept=".zip" />
        <div class="w-full flex items-center">
          <div class="flex flex-row items-center gap-2">
            <input type="checkbox" id="forceOverwrite" v-model="forceOverwrite" />
            <label for="forceOverwrite">
              overwrite existing package
            </label>
          </div>
          <button
            class="btn btn-green ml-auto"
            type="button"
            title="Upload package"
            :disabled="isUploading || this.packageFile == null"
            @click="upload()"
          >
            upload
          </button>
        </div>
        <div
          v-if="successMessage"
          class="w-full flex justify-center text-green-500 font-bold"
        >
          {{ successMessage }}
        </div>
        <div
          v-if="error"
          class="w-full flex justify-center text-red-500 font-bold"
        >
          {{ error }}
        </div>
      </div>
    </UiCard>
  </Page>
</template>

<script>
export default {
  data () {
    return {
      isUploading: false,
      packageFile: null,
      error: null,
      successMessage: null,
      forceOverwrite: false
    };
  },
  methods: {
    async upload () {
      this.successMessage = null;
      if (!this.packageFile) {
        this.error = 'Please select a package file you want to upload!';
        return;
      }
      this.error = null;
      try {
        this.isUploading = true;
        await this.$api.uploadPackage(this.packageFile, this.forceOverwrite);
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
