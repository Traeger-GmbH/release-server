<template>
  <div>
    <div class="flex flex-row w-full items-center max-w-full">
      <div class="px-2 text-2xl font-semibold tabular-nums whitespace-nowrap">
        v{{ release.version }} ({{ new Date(release.releaseDate).toLocaleDateString() }})
      </div>
      <div
        v-if="release.isPreviewRelease"
        class="bg-blue-500 rounded-full py-1 px-2 font-semibold text-white text-xs text-center"
      >
        preview
      </div>
      <div
        v-if="release.isSecurityPatch"
        class="bg-red-500 rounded-full py-1 px-2 font-semibold text-white text-xs text-center"
      >
        security patch
      </div>
    </div>
    <div class="flex flex-row flex-wrap w-full mt-4 gap-2 justify-between">
      <button
        class="btn btn-green"
        @click="openDownloadDialog"
      >
        download
      </button>
      <button
        class="btn btn-red-outline"
        @click="openDeleteDialog"
      >
        delete release
      </button>
    </div>
    <div class="w-full">
      <div class="p-2 my-1 text-xl">
        Platforms
      </div>
      <div class="p-2 my-1 flex flex-wrap gap-1 justify-start">
        <div
          v-for="platform in release.platforms"
          :key="platform"
          class="bg-gray-700 text-white py-1 px-2 rounded-full text-center text-sm"
        >
          {{ platform }}
        </div>
      </div>
    </div>
    <div class="w-full">
      <div class="p-2 my-1 text-xl">
        Changes
      </div>
      <div class="p-2 my-1 space-y-5">
        <UiPane
          v-for="(changeSet, index) in release.changes.en"
          :key="index"
          class="bg-gray-300"
        >
          <div class="flex">
            Affected Platforms
            <div class="flex ml-2">
              <div v-if="changeSet.platforms">
                <div
                  v-for="platform in changeSet.platforms"
                  :key="platform"
                  class="bg-gray-700 text-white py-1 px-2 rounded-full text-center text-sm"
                >
                  {{ platform }}
                </div>
              </div>
              <div v-else>
                <div
                  class="bg-gray-700 text-white py-1 px-2 rounded-full text-center text-sm"
                >
                  all
                </div>
              </div>
            </div>
          </div>
            Added:
            <ul class="list-disc list-inside">
              <li
                v-for="element in changeSet.added"
                :key="element"
              >
                {{ element }}
              </li>
            </ul>
          <div>
            Fixed:
            <ul class="list-disc list-inside">
              <li
                v-for="element in changeSet.fixed"
                :key="element"
              >
                {{ element }}
              </li>
            </ul>
          </div>
          <div>
            Breaking:
            <ul class="list-disc list-inside">
              <li
                v-for="element in changeSet.breaking"
                :key="element"
              >
                {{ element }}
              </li>
            </ul>
          </div>
          <div>
            Deprecated:
            <ul class="list-disc list-inside">
              <li
                v-for="element in changeSet.deprecated"
                :key="element"
              >
                {{ element }}
              </li>
            </ul>
          </div>
        </UiPane>
      </div>
    </div>
    <DeleteReleaseDialog
      :product-identifier="productIdentifier"
      :version="release.version"
      :showing="showDeleteDialog"
      @close="closeDeleteDialog"
      @deleted="onDeleted"
    />
    <DownloadReleaseDialog
      :product-identifier="productIdentifier"
      :release="release"
      :showing="showDownloadDialog"
      @close="closeDownloadDialog"
    />
  </div>
</template>

<script>
export default {
  props: {
    release: {
      type: Object,
      required: true
    },
    productIdentifier: {
      type: String,
      required: true
    }
  },
  data () {
    return {
      confirmVersionName: null,
      showDeleteDialog: false,
      showDownloadDialog: false
    };
  },
  methods: {
    openDeleteDialog () {
      this.showDeleteDialog = true;
    },
    openDownloadDialog () {
      this.showDownloadDialog = true;
    },
    closeDeleteDialog () {
      this.showDeleteDialog = false;
    },
    closeDownloadDialog () {
      this.showDownloadDialog = false;
    },
    onDeleted () {
      this.$emit('deleted');
    }
  }
};
</script>
