<template>
  <Page title="Maintain" back-link="/">
    <div class="flex flex-col gap-4">

      <UiCard title="Statistics" v-if="statistics">
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
                  {{ statistics.disk.totalSize }} MB
                </td>
              </tr>
              <tr>
                <td class="font-semibold">
                  Used space:
                </td>
                <td>
                  {{ statistics.disk.usedDiskSpace }} MB
                </td>
              </tr>
              <tr>
                <td class="font-semibold">
                  Avail. space:
                </td>
                <td>
                  {{ statistics.disk.availableFreeSpace }} MB
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
        title="Backup"
      >
        <div
          class="flex flex-wrap gap-2 justify-around"
        >
          <button
            class="btn btn-green w-48"
            @click="openCreateBackupDialog"
          >
            Create Backup
          </button>
          <button
            class="btn btn-green-outline w-48"
            @click="openRestoreDialog"
          >
            Restore Backup
          </button>
          <CreateBackupDialog
            :showing="showCreateBackupDialog"
            @close="closeCreateBackupDialog"
          />
          <RestoreBackupDialog
            :showing="showRestoreDialog"
            @close="closeRestoreDialog"
          />
        </div>
      </UiCard>
    </div>
  </Page>
</template>

<script>
export default {
  data () {
    return {
      showRestoreDialog: false,
      showCreateBackupDialog: false,
    };
  },
  async asyncData ({ $api }) {
    const statistics = await $api.getStatistics();
    return { statistics };
  },
  methods: {
    openRestoreDialog () {
      this.showRestoreDialog = true;
    },
    openCreateBackupDialog () {
      this.showCreateBackupDialog = true;
    },
    closeRestoreDialog () {
      this.showRestoreDialog = false;
    },
    closeCreateBackupDialog () {
      this.showCreateBackupDialog = false;
    }
  }
};
</script>
