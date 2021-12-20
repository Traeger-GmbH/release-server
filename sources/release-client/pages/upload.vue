<template>
  <Page title="Upload" back-link="/">
    <div class="flex flex-row w-full gap-2">
      <div class="flex w-full flex-col gap-2">
        <div
          v-if="!file"
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
          class="bg-white h-36 flex flex-col justify-center items-center gap-2"
          v-if="file"
        >
          <div>
            {{ file.name }}
          </div>
          <button
            class="p-1 px-3 bg-green-500 text-white font-bold rounded hover:bg-green-700"
            type="button"
            title="Remove file"
            @click="file = null"
          >
            remove
          </button>
        </UiPane>
        <UiPane class="bg-white ">
          <div class="max-w-sm mx-auto">
            <p class="my-5">
              Please enter your credentials:
            </p>
            <FormInput
              title="Username"
              id="username"
              name="username"
              type="text"
              v-model="username"
            />
            <FormInput
              title="Password"
              id="password"
              name="username"
              type="password"
              v-model="password"
            />
          </div>
        </UiPane>
        <UiPane
          v-if="error"
          class="w-full flex justify-center text-red-500 font-bold"
        >
          {{ error }}
        </UiPane>
        <UiPane
          v-if="successMessage"
          class="w-full flex justify-center text-green-500 font-bold"
        >
          {{ successMessage }}
        </UiPane>
        <div class="w-full flex justify-end">
          <button
            class="py-1 px-2 bg-green-500 text-white hover:bg-green-700 rounded"
            type="button"
            title="Upload package"
            @click="upload()"
            :disable="isUploading"
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
      username: 'TestUser',
      password: 'SomePassword',
      isUploading: false,
      file: null,
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
      this.file = event.dataTransfer.files[0];
      // Clean up
      event.currentTarget.classList.add('bg-gray-100');
      event.currentTarget.classList.remove('bg-green-300');
    },
    async upload () {
      this.successMessage = null;
      if (!this.file) {
        this.error = 'Please select a package file you want to upload!';
        return;
      }
      if (!this.username) {
        this.error = 'Please enter a username!';
        return;
      }
      if (!this.password) {
        this.error = 'Please enter a password!';
        return;
      }
      this.error = null;
      try {
        this.isUploading = true;
        await this.$api.uploadPackage(
          this.file,
          { username: this.username, password: this.password }
        );
        this.successMessage = `Successfully uploaded "${this.file.name}".`;
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
