<template>
  <div class="flex flex-grow flex-shrink gap-2 min-h-0">
    <UiPane class="flex flex-col gap-2 w-2/6 overflow-y-auto">
      <template v-if="productIdentifier">
        <template v-if="product">
          <div class="rounded flex flex-row items-center gap-2 font-semibold -mt-4 -mx-4 px-4 py-4 bg-gray-500 mb-2 relative">
            <input
              id="filter"
              class="rounded px-3 py-2 flex-grow focus:outline-none focus:ring ring-green-500 font-medium"
              type="text"
              v-model="filter"
              placeholder="Filter"
              @keyup.esc="filter = null"
              autocomplete="off"
            >
            <button
              v-if="filter && filter.length > 0"
              class="absolute top-6 right-8 text-gray-500 font-semibold"
              @click="filter = null"
            >
              x
            </button>
          </div>
          <div class="overflow-y-scroll flex flex-col gap-2 -mx-4 px-4 h-full -my-4 py-4">
            <div
              v-for="release in filteredReleases"
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
          </div>
        </template>
        <template v-else>
          <div class="flex-grow flex items-center justify-center">
            <LoadingSpinner
              :message="`Loading releases...`"
              :show="isLoading"
            />
            <template
              v-if="!isLoading && error"
            >
              <div
                v-if="!isLoading && error.statusCode === 404"
                class="text-xl"
              >
                No releases found for this product.
              </div>
              <div
                v-else-if="!isLoading"
                class="flex flex-col gap-4"
              >
                <span class="text-xl">{{ error }}</span>
                <button
                  class="btn btn-green self-center"
                  @click="$fetch()"
                >
                  retry
                </button>
              </div>
            </template>
          </div>
        </template>
      </template>
      <template v-else>
        <div class="h-full w-full flex justify-center items-center text-lg font-medium">
          No product selected.
        </div>
      </template>
    </UiPane>
    <UiPane
      v-if="hasSelection"
      class="bg-white flex flex-col flex-grow overflow-y-scroll gap-2 w-2/3"
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
      type: String
    }
  },
  data () {
    return {
      isLoading: false,
      product: null,
      selectedRelease: null,
      filter: null,
      error: null
    };
  },
  async fetch () {
    if (this.productIdentifier !== null) {
      try {
        this.isLoading = true;
        this.product = await this.$api.getProductInfo(this.productIdentifier);
        this.select(this.product.releases[0]);
      } catch (error) {
        this.error = error;
      } finally {
        this.isLoading = false;
      }
    }
  },
  watch: {
    productIdentifier () {
      this.$fetch();
    }
  },
  computed: {
    hasSelection () {
      return this.selectedRelease !== null;
    },
    filteredReleases () {
      if (this.filter !== null) {
        return this.product.releases.filter(release => release.version.startsWith(this.trimmedFilter));
      } else {
        return this.product.releases;
      }
    },
    trimmedFilter () {
      if (this.filter !== null) {
        const splitFilter = this.filter.split('v');
        if (splitFilter.length > 1) {
          return splitFilter[1];
        } else {
          return splitFilter[0];
        }
      } else {
        return this.filter;
      }
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
