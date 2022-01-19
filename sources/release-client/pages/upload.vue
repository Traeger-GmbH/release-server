<template>
  <Page title="Upload" back-link="/">
    <!-- <UiCard title="Upload package" class="flex flex-row w-full gap-2">
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
    </UiCard> -->
    <UiCard title="Multipackage upload" class="flex flex-row w-full gap-2">
      <div class="flex w-full flex-col gap-2 max-w-6xl mx-auto gap-4">
        <MultipleFileSelector
          class="max-w-2xl mx-auto"
          accept=".zip"
          @add="addPackage"
        />
        <table class="table-fixed">
          <thead>
            <tr>
              <th class="text-left">File</th>
              <th class="flex flex-row items-center gap-2 justify-center">
                <input
                  class=""
                  type="checkbox"
                  id="forceOverwrite"
                  v-model="forceOverwrite"
                  @change="onOverallForceOverwrite"
                  :disabled="isUploading"
                />
                overwrite existing
              </th>
              <th>status</th>
              <th>remove</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(pkg, index) in packages"
              :key="pkg.file.name"
              class=""
            >
              <td class="">
                {{ pkg.file.name }}
              </td>
              <td class="text-center">
                <input
                  type="checkbox"
                  id="forceOverwrite"
                  v-model="pkg.forceOverwrite"
                  @change="onPkgForceOverwrite"
                  :disabled="isUploading"
                />
              </td>
              <td class="text-center flex justify-center">
                <template
                  v-if="pkg.status.isLoading"
                >
                  <VueSpinner :size="25" line-fg-color="#10b981" />
                </template>
                <template
                  v-else-if="pkg.status.success"
                >
                  <div class="px-2 py-1 bg-green-500 rounded-full text-white w-36">
                    done
                  </div>
                </template>
                <template
                  v-else-if="pkg.status.error"
                >
                  <button
                    class="px-2 py-1 bg-red-500 rounded-full text-white w-36"
                    @click="pkg.status.showError = true"
                  >
                    {{ pkg.status.error.title }}
                  </button>
                  <CardModal
                    :showing="pkg.status.showError"
                    @close="pkg.status.showError = false"
                  >
                    <h4 class="font-light">{{ pkg.status.error.title }}</h4>
                    {{ pkg.status.error.detail }}
                  </CardModal>
                </template>
                <template v-else>
                  <div
                    v-if="isUploading"
                    class="px-2 py-1 bg-yellow-500 rounded-full text-white w-36">
                    queued
                  </div>
                </template>
              </td>
              <td class="text-center">
                <button
                  class="bg-gray-400 text-white px-2 py-1 inline-block rounded mx-auto"
                  type="button"
                  @click="removePackage(index)"
                  title="Remove file"
                >
                  ×
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <div class="w-full flex items-center justify-end gap-2">
            <button
              class="btn btn-red-outline"
              type="button"
              title="Cancel upload"
              v-if="isUploading"
              @click="cancelUpload"
              :disabled="!isUploading"
            >
              cancel
            </button>
            <button
              class="btn btn-green"
              type="button"
              title="Upload package"
              :disabled="isUploading || this.packages.length < 1"
              @click="uploadPackages()"
            >
              {{ !isUploading ? 'upload' : 'uploading...' }}
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
import VueSpinner from 'vue-simple-spinner';

export default {
  components: {
    VueSpinner
  },
  data () {
    return {
      isUploading: false,
      packageFile: null,
      error: null,
      successMessage: null,
      forceOverwrite: false,
      packages: [],
      isCancelling: false
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
        const title = responseData.title;
        const detail = responseData.detail;
        this.error = { detail, title };
      } finally {
        this.isUploading = false;
      }
    },
    async uploadPackages () {
      if (this.packages.length < 1) {
        this.error = 'Please select at least one package file you want to upload!';
        return;
      }

      try {
        this.isUploading = true;
        this.successMessage = null;

        for (let i = 0; i < this.packages.length && !this.isCancelling; i++) {
          const pkg = this.packages[i];
          try {
            pkg.status.isLoading = true;
            await this.$api.uploadPackage(pkg.file, pkg.forceOverwrite);
            pkg.status.success = true;
          } catch (error) {
            const responseData = error.response.data;
            const title = responseData.title;
            const detail = responseData.detail;
            pkg.status.error = { detail, title };
          } finally {
            pkg.status.isLoading = false;
          }
        }
      } finally {
        this.isUploading = false;
        this.isCancelling = false;
      }
    },
    cancelUpload () {
      this.isCancelling = true;
    },
    removePackage (index) {
      this.packages.splice(index, 1);
    },
    addPackage (file) {
      const exists = this.packages.some((pkg) => {
        return pkg.file.name === file.name &&
          pkg.file.type === file.type &&
          pkg.file.size === file.size &&
          pkg.file.lastModified === file.lastModified;
      });
      if (!exists) {
        this.packages.push({ file, forceOverwrite: false, status: { isLoading: false, success: false, error: null, showError: false } });
      }
    },
    onOverallForceOverwrite (event) {
      this.packages.forEach((pkg) => {
        console.log('before');
        console.log(pkg);
        pkg.forceOverwrite = event.srcElement.checked;
        console.log('after');
        console.log(pkg);
      });
    },
    onPkgForceOverwrite (event) {
      const value = event.srcElement.checked;
      if (!this.packages.some(pkg => pkg.forceOverwrite === value)) {
        this.forceOverwrite = value;
      } else {
        this.forceOverwrite = false;
      }
    }
  }
};
</script>

<style>

</style>
