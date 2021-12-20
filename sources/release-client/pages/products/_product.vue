<template>
  <Page :title="product.identifier" backLink="/products">
    <div class="flex gap-2">
      <div class="flex flex-col gap-2 w-1/3">
        <UiCard
          v-for="release in product.releases"
          :key="release.version"
          :title="release.version"
          class="hover:bg-gray-700 cursor-pointer"
          :class="[
            isSelected(release) ? 'bg-gray-700' : ''
          ]"
          @click.native="select(release)"
        >
          {{ new Date(release.releaseDate).toLocaleDateString() }}
        </UiCard>
      </div>
      <div class="flex flex-col gap-2 w-2/3">
        <UiPane class="bg-white" v-if="hasSelection">
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
  methods: {
    select (release) {
      console.log('select');
      this.selectedRelease = release;
      console.log(this.selectedRelease);
    },
    isSelected (release) {
      return release === this.selectedRelease;
    }
  },
  computed: {
    hasSelection () {
      return this.selectedRelease !== null;
    }
  }
};
</script>
