<template>
  <div class="">
    <h1 class="text-2xl capitalize mb-20 text-center">
      {{ identifier }}
    </h1>
    <div class="flex w-full">
      <div class="w-1/6 bg-gray-100">
        <div
          v-for="version in versions"
          :key="version"
          class="tabular-nums p-2 my-1 rounded cursor-pointer ml-2"
          @click="select(version)"
          :class="[
            selected === version ? 'bg-green-500' : 'hover:bg-blue-100'
          ]"
        >
          {{ version }}
        </div>
      </div>
      <div v-if="selected" class="w-5/6">
        <ReleaseInformation :release="selectedReleaseInfo" />
        {{ selectedReleaseInfo }}
      </div>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    info: {
      type: Object,
      required: true
    }
  },
  data () {
    return {
      selected: null
    };
  },
  computed: {
    identifier () {
      return this.info.identifier;
    },
    releases () {
      return this.info.releases;
    },
    versions () {
      return this.releases.map(release => release.version);
    },
    selectedReleaseInfo () {
      if (this.selected) {
        return this.releases.find(element => element.version === this.selected);
      } else {
        return null;
      }
    }
  },
  methods: {
    select (version) {
      this.selected = version;
    }
  }
};
</script>

<style>

</style>
