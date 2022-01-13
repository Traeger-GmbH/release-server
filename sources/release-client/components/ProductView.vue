<template>
  <div class="flex flex-grow flex-shrink gap-2 min-h-0">
    <UiPane class="flex flex-col gap-2 w-1/3 overflow-y-auto">
      <template v-if="product">
        <div
          v-for="release in product.releases"
          :key="release.version"
          :title="release.version"
          class="rounded px-3 py-2 hover:bg-gray-700 hover:text-white cursor-pointer tabular-nums font-semibold flex justify-between"
          :class="[
            isSelected(release) ? 'bg-gray-700 text-white' : ''
          ]"
          @click="select(release)"
        >
          <div class="flex items-center">
            v{{ release.version }}
            <div
              class="ml-2 w-2 h-2 rounded-xl"
              :class="release.isPreviewRelease ? 'bg-blue-500' : ''"
            />
            <div
              class="ml-2 w-2 h-2 rounded-xl"
              :class="release.isSecurityPatch ? 'bg-red-500' : ''"
            />
          </div>
          <span>
            [{{ new Date(release.releaseDate).toLocaleDateString() }}]
          </span>
        </div>
      </template>
      <template v-else>
        <div class="flex-grow flex items-center justify-center">
          <LoadingSpinner :message="`Loading product...`" :show="isLoading" />
          <div
            v-if="!isLoading"
            class="text-xl self-start"
          >
            No releases found for this product.
          </div>
        </div>
      </template>
    </UiPane>
    <UiPane
      v-if="hasSelection"
      class="bg-white flex flex-col overflow-y-auto gap-2 w-2/3"
    >
      <ReleaseInformation
        :release="selectedRelease"
        :product-identifier="product.identifier"
        @deleted="removeSelectedRelease()"
      />
    </UiPane>
  </div>
</template>

<script>
export default {
  props: {
    productIdentifier: {
      type: String,
      required: true
    }
  },
  data () {
    return {
      isLoading: false,
      product: null,
      selectedRelease: null
    };
  },
  async fetch () {
    try {
      this.isLoading = true;
      this.product = await this.$api.getProductInfo(this.productIdentifier);
      this.select(this.product.releases[0]);
    } finally {
      this.isLoading = false;
    }
  },
  computed: {
    hasSelection () {
      return this.selectedRelease !== null;
    }
  },
  methods: {
    select (release) {
      this.selectedRelease = release;
    },
    isSelected (release) {
      return release === this.selectedRelease;
    },
    removeSelectedRelease () {
      const index = this.product.releases.findIndex(element => element === this.selectedRelease);
      this.selectedRelease = null;
      this.product.releases.splice(index, 1);
      this.select(this.product.releases[0]);
    }
  },
};
</script>
