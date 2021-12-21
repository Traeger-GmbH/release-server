<template>
  <div>
    <div class="flex w-full">
      <div class="px-2 text-2xl font-semibold tabular-nums">
        v{{ release.version }} ({{ new Date(release.releaseDate).toLocaleDateString() }})
      </div>
      <div v-if="release.isPreviewRelease" class="bg-blue-500 rounded-full py-1 px-2 font-semibold text-white text-xs">
        preview
      </div>
      <div v-if="release.isSecurityPatch" class="bg-red-500 rounded-full py-1 px-2 font-semibold text-white text-xs">
        security patch
      </div>
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
  </div>
</template>

<script>
export default {
  props: {
    release: {
      type: Object,
      required: true
    }
  }
};
</script>
