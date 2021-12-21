<template>
  <Page title="Maintain" back-link="/">
    <div class="flex flex-col gap-4">

    <UiCard title="Statistics">
      <div class="flex flex-wrap gap-4">
        <div>
          <div class="text-xl font-semibold mb-2">
            Disk Usage
          </div>
          <table class="table-auto">
            <tr>
              <td class="font-semibold">
                Total size:
              </td>
              <td>
                {{ statistics.disk.totalSize }}
              </td>
            </tr>
            <tr>
              <td class="font-semibold">
                Used space:
              </td>
              <td>
                {{ statistics.disk.usedDiskSpace }}
              </td>
            </tr>
            <tr>
              <td class="font-semibold">
                Avail. space:
              </td>
              <td>
                {{ statistics.disk.availableFreeSpace }}
              </td>
              <td>
                <span class="text-white bg-green-500 rounded px-2 py-1">{{ (statistics.disk.availableFreeSpace / statistics.disk.totalSize * 100).toFixed(2) }} %</span>
              </td>
            </tr>
          </table>
        </div>
        <div>
          <div class="text-xl font-semibold mb-2">
            Artifacts
          </div>
          <table class="table-auto">
            <tr>
              <td class="font-semibold">
                Number of stored products:
              </td>
              <td>
                {{ statistics.numberOfProducts }}
              </td>
            </tr>
            <tr>
              <td class="font-semibold">
                Number of stored artifacts:
              </td>
              <td>
                {{ statistics.numberOfArtifacts }}
              </td>
            </tr>
          </table>
        </div>
      </div>
    </UiCard>
    <UiCard
      class="max-w-sm h-32 cursor-pointer"
      title="Create Backup"
      @click.native="createBackup()"
    />
    <a
      class="hidden"
      id="downloadBackup"
      ref="download"
    />
    </div>
  </Page>
</template>

<script>
export default {
  async asyncData ({ $api }) {
    const statistics = await $api.getStatistics();
    return { statistics };
  },
  methods: {
    async createBackup () {
      const result = await this.$api.createBackup();
      const url = URL.createObjectURL(result.blob);
      const a = this.$refs.download;
      a.href = url;
      a.download = result.filename;
      a.click();
      URL.revokeObjectURL(url);
    }
  }
};
</script>

<style>

</style>
