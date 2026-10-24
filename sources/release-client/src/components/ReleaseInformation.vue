<template>
  <div>
    <div class="flex flex-row w-full items-center gap-2 max-w-full">
      <div class="px-2 text-2xl font-semibold tabular-nums whitespace-nowrap">
        v{{ release.version }} ({{ formatDate(release.releaseDate) }})
      </div>
      <div v-if="release.isPreviewRelease" class="bg-blue-500 rounded-full py-1 px-2 font-semibold text-white text-xs text-center">
        preview
      </div>
      <div v-if="release.isSecurityPatch" class="bg-red-500 rounded-full py-1 px-2 font-semibold text-white text-xs text-center">
        security patch
      </div>
      <button type="button" class="btn btn-red-outline ml-auto" @click="showDeleteDialog = true">
        delete release
      </button>
    </div>

    <div class="w-full">
      <div class="p-2 my-1 text-xl">Download</div>
      <div class="p-2 my-1 flex flex-wrap gap-2 justify-start">
        <a
          v-for="platform in release.platforms"
          :key="platform"
          :href="artifactUrl(productIdentifier, platform, release.version)"
          class="badge hover:bg-green-700"
          :title="`Download ${productIdentifier} ${release.version} for ${platform}`"
          download
        >
          {{ platform }}
        </a>
      </div>
    </div>

    <div class="w-full">
      <div class="p-2 my-1 text-xl flex items-center gap-4">
        Changes
        <div v-if="languages.length > 1" class="flex gap-1 text-sm" role="tablist">
          <button
            v-for="language in languages"
            :key="language"
            type="button"
            role="tab"
            class="px-2 py-1 rounded-sm border border-gray-300"
            :class="language === selectedLanguage ? 'bg-gray-700 text-white' : ''"
            :aria-selected="language === selectedLanguage"
            @click="selectedLanguage = language"
          >
            {{ language }}
          </button>
        </div>
      </div>
      <div class="p-2 my-1 space-y-5">
        <div v-if="changeSets.length === 0" class="text-gray-500">No changes listed.</div>
        <UiPane v-for="(changeSet, index) in changeSets" :key="index" class="bg-gray-100">
          <div class="flex items-center gap-2 mb-2">
            Affected platforms
            <span v-for="platform in changeSet.platforms || ['all']" :key="platform" class="badge">{{ platform }}</span>
          </div>
          <template v-for="category in categories" :key="category.key">
            <div v-if="changeSet[category.key] && changeSet[category.key].length > 0">
              {{ category.title }}:
              <ul class="list-disc list-inside">
                <li v-for="(entry, entryIndex) in changeSet[category.key]" :key="entryIndex">{{ entry }}</li>
              </ul>
            </div>
          </template>
        </UiPane>
      </div>
    </div>

    <DeleteReleaseDialog
      :product-identifier="productIdentifier"
      :release="release"
      :showing="showDeleteDialog"
      @close="showDeleteDialog = false"
      @deleted="$emit('deleted')"
    />
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue';

import { artifactUrl } from '../api.js';
import { formatDate } from '../format.js';
import DeleteReleaseDialog from './DeleteReleaseDialog.vue';
import UiPane from './UiPane.vue';

const props = defineProps({
  release: { type: Object, required: true },
  productIdentifier: { type: String, required: true }
});
defineEmits(['deleted']);

const categories = [
  { key: 'added', title: 'Added' },
  { key: 'fixed', title: 'Fixed' },
  { key: 'breaking', title: 'Breaking' },
  { key: 'deprecated', title: 'Deprecated' }
];

const showDeleteDialog = ref(false);
const selectedLanguage = ref(null);

// Languages with at least one change set; many releases only have German change logs
const languages = computed(() => Object.entries(props.release.changes || {})
  .filter(([, changeSets]) => Array.isArray(changeSets) && changeSets.length > 0)
  .map(([language]) => language));

const changeSets = computed(() => (props.release.changes || {})[selectedLanguage.value] || []);

watch(languages, (available) => {
  if (!available.includes(selectedLanguage.value)) {
    selectedLanguage.value = available.includes('en') ? 'en' : (available[0] ?? null);
  }
}, { immediate: true });
</script>
