<template>
  <Page :title="product.identifier" back-link="/products">
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
          <span>
            v{{ release.version }}
          </span>
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
          <ReleaseInformation :release="selectedRelease" />
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
    }
  },
};
</script>
