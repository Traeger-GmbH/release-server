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
        title="Backup"
      >
        <div
          class="flex flex-wrap gap-2 justify-start"
        >
          <button
            class="btn btn-green w-48"
            @click="createBackup()"
          >
            Create Backup
          </button>
          <button
            class="btn btn-green-outline w-48"
            @click="openRestoreDialog()"
          >
            Restore Backup
          </button>
          <a
            class="hidden"
            id="downloadBackup"
            ref="download"
          />
          <CardModal
            :showing="showRestoreDialog"
            @close="closeRestoreDialog()"
          >
            <div>
              <form-wizard
                ref="restoreWizard"
                title="Restore backup"
                subtitle="Restore a previously created backup."
                color="#10b981"
              >
                <tab-content
                  title="Choose backup file"
                >
                  <div
                    v-if="!restoreFile"
                    class="p-12 bg-white h-36 text-center border-2 border-dashed border-gray-300 rounded"
                    @dragover="dragover"
                    @dragleave="dragleave"
                    @drop="drop"
                  >
                    <label for="assetsFieldHandle" class="block cursor-pointer">
                      <div>
                        Drop your backup file here
                        or <span class="underline">click here</span> to open the file browser.
                      </div>
                    </label>
                  </div>
                  <div
                    v-if="restoreFile"
                    class="bg-white h-36 flex flex-col justify-center items-center gap-2"
                  >
                    <div>
                      {{ restoreFile.name }}
                    </div>
                    <button
                      class="btn btn-green-outline"
                      type="button"
                      title="Remove file"
                      @click="restoreFile = null"
                    >
                      remove
                    </button>
                  </div>
                </tab-content>
                <tab-content title="Restore">
                  <div
                    class="flex justify-center"
                  >
                    <template
                      v-if="isUploading"
                    >
                      <div>
                        <VueSpinner size="medium" line-fg-color="#10b981" />
                        Restoring...
                      </div>
                    </template>
                    <template
                      v-else-if="error"
                    >
                      <span
                        class="text-red-500 font-semibold"
                      >
                        {{ error }}
                      </span>
                    </template>
                    <template
                      v-else
                    >
                      <span
                        class="text-green-500 font-semibold"
                      >
                        {{ restoreSuccessMessage }}
                      </span>
                    </template>
                  </div>
                </tab-content>
                <template slot="footer" slot-scope="props">
                  <div class="flex justify-around">
                    <button
                      v-if="!props.isLastStep"
                      @click="closeRestoreDialog()"
                      class="btn btn-green-outline"
                    >
                      Cancel
                    </button>
                    <button
                      v-else-if="error"
                      @click="props.prevTab()"
                      class="btn btn-green-outline"
                    >
                      Back
                    </button>
                    <button
                      v-if="!props.isLastStep"
                      @click="restoreBackup() && props.nextTab()"
                      class="btn btn-green"
                      :disabled="!restoreFile"
                    >
                      Restore
                    </button>
                    <button
                      v-if="props.isLastStep"
                      @click="closeRestoreDialog()"
                      class="btn btn-green"
                      :disabled="error"
                    >
                      Finish
                    </button>
                  </div>
                </template>
              </form-wizard>
            </div>
          </CardModal>
        </div>
      </UiCard>
    </div>
  </Page>
</template>

<script>
import { FormWizard, TabContent } from 'vue-form-wizard';
import 'vue-form-wizard/dist/vue-form-wizard.min.css';
import VueSpinner from 'vue-simple-spinner';

export default {
  components: {
    FormWizard,
    TabContent,
    VueSpinner
  },
  data () {
    return {
      showRestoreDialog: false,
      restoreFile: null,
      isUploading: false,
      restoreSuccessMessage: null,
      error: null,
      steps: [
        {
          name: 'choose',
          title: 'Backup file',
          subtitle: 'Choose the backup file to restore'
        },
        {
          name: 'upload',
          title: 'Backup file',
          subtitle: 'Choose the backup file to restore'
        },
      ]
    };
  },
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
    },
    openRestoreDialog () {
      this.showRestoreDialog = true;
    },
    closeRestoreDialog () {
      this.showRestoreDialog = false;
      this.resetRestoreFile();
    },
    resetRestoreFile () {
      this.restoreFile = null;
    },
    async restoreBackup () {
      this.restoreSuccessMessage = null;
      if (!this.restoreFile) {
        this.error = 'Please select a backup file you want to restore!';
        return;
      }
      this.error = null;
      try {
        this.isUploading = true;
        await this.$api.restoreBackup(this.restoreFile);
        this.restoreSuccessMessage = `Successfully restored "${this.restoreFile.name}".`;
        this.restoreFile = null;
      } catch (error) {
        let message;
        if (error.response && error.response.data) {
          const responseData = error.response.data;
          // build error message:
          if (responseData.title) {
            message = responseData.title;
          }
          if (responseData.detail) {
            message += `: ${responseData.detail}`;
          }
        }
        if (!message) {
          message = error;
        }
        this.error = message;
      } finally {
        this.isUploading = false;
      }
    },
    dragover (event) {
      event.preventDefault();
      // Add some visual fluff to show the user can drop its files
      if (!event.currentTarget.classList.contains('bg-green-300')) {
        event.currentTarget.classList.remove('bg-gray-100');
        event.currentTarget.classList.add('bg-green-300');
      }
    },
    dragleave (event) {
      // Clean up
      event.currentTarget.classList.add('bg-gray-100');
      event.currentTarget.classList.remove('bg-green-300');
    },
    drop (event) {
      event.preventDefault();
      this.restoreFile = event.dataTransfer.files[0];
      // Clean up
      event.currentTarget.classList.add('bg-gray-100');
      event.currentTarget.classList.remove('bg-green-300');
    }
  }
};
</script>

<style>

</style>
