<template>
  <CardModal
    :showing="showing"
    @close="onClose"
    class="text-center"
  >
    <h4 class="font-light">Download
      <span class="font-normal">{{ productIdentifier }}-v{{ release.version }}</span>
    </h4>
    <div class="font-light text-sm text-gray-500">
      Please select the platform to download.
    </div>
    <div class="m-4">
      <button
        v-for="platform in release.platforms"
        :key="platform"
        :title="platform"
        @click="selectedPlatform = platform"
        class="bg-gray-100 rounded-full px-3 py-2 font-semibold hover:bg-gray-700 hover:text-white text-left m-2"
        :class="[
          selectedPlatform === platform ? 'bg-gray-700 text-white' : ''
        ]"
      >
        {{ platform }}
      </button>
    </div>
    <div
      v-if="error"
      class="text-red-500 font-bold"
    >
      {{ error }}
    </div>
    <button
      class="btn btn-green"
      :disabled="!this.selectedPlatform || isLoading"
      @click="download"
    >
      {{ isLoading ? 'downloading...' : 'download' }}
    </button>
  </CardModal>
</template>

<script>
export default {
  props: {
    productIdentifier: {
      type: String,
      required: true
    },
    release: {
      type: Object,
      required: true
    },
    showing: {
      type: Boolean,
      required: true
    }
  },
  data () {
    return {
      selectedPlatform: null,
      isLoading: false,
      error: null
    };
  },
  methods: {
    onClose () {
      this.$emit('close');
      this.selectedPlatform = null;
    },
    forceFileDownload (blob, filename) {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', filename);
      document.body.appendChild(link);
      link.click();
    },
    async download () {
      try {
        this.isLoading = true;
        const { blob, filename } = await this.$api.downloadSpecificArtifact(this.productIdentifier, this.selectedPlatform, this.release.version);
        this.onClose();
        this.forceFileDownload(blob, filename);
      } catch (error) {
        let message;
        if (error.response && error.response.data) {
          const responseData = error.response.data;
          // build error message:
          if (responseData.title) {
            message = responseData.title;
          }
          if (responseData.detail) {
            message += `: ${responseData.detail}`;
          }
        }
        if (!message) {
          message = error;
        }
        this.error = message;
      } finally {
        this.isLoading = false;
      }
    },
  }
};
</script>
