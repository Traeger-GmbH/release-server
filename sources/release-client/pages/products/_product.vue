<template>
  <Page :title="product.identifier" back-link="/products">
    <!-- <template v-slot:header>
      <button
        class="btn btn-red-outline ml-auto"
      >
        delete product
      </button>
    </template> -->
    <div class="flex gap-2">
      <UiPane class="flex flex-col gap-2 w-1/3 overflow-y-auto">
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
      </UiPane>
      <div class="flex flex-col gap-2 w-2/3">
        <UiPane
          v-if="hasSelection"
          class="bg-white"
        >
          <ReleaseInformation
            :release="selectedRelease"
            :productIdentifier="product.identifier"
            @deleted="removeSelectedRelease()"
          />
        </UiPane>
      </div>
    </div>
  </Page>
</template>

<script>
export default {
  async asyncData ({ params, $api }) {
    const product = await $api.getProductInfo(params.product);
    return { product };
  },
  data () {
    return {
      selectedRelease: null
    };
  },
  computed: {
    hasSelection () {
      return this.selectedRelease !== null;
    }
  },
  beforeMount () {
    this.select(this.product.releases[0]);
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
