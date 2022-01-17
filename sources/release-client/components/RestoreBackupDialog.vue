<template>
  <CardModal
    :showing="showing"
    @close="onClose"
  >
    <form-wizard
      ref="restoreWizard"
      title="Restore backup"
      subtitle="Restore a previously created backup."
      color="#10b981"
    >
      <tab-content
        title="Choose backup file"
      >
        <FileSelector v-model="restoreFile"  accept=".zip" />
      </tab-content>
      <tab-content title="Restore">
        <div
          class="flex justify-center"
        >
          <template
            v-if="isLoading"
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
              {{ successMessage }}
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
            @click="onClose()"
            class="btn btn-green"
            :disabled="error"
          >
            Finish
          </button>
        </div>
      </template>
    </form-wizard>
  </CardModal>
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
  props: {
    showing: {
      type: Boolean,
      required: true
    }
  },
  data () {
    return {
      restoreFile: null,
      successMessage: null,
      error: null,
      isLoading: false
    };
  },
  methods: {
    onClose () {
      this.confirmReleaseName = null;
      this.successMessage = null;
      this.isLoading = false;
      this.error = null;
      this.$emit('close');
    },
    resetRestoreFile () {
      this.restoreFile = null;
    },
    async restoreBackup () {
      this.successMessage = null;
      if (!this.restoreFile) {
        this.error = 'Please select a backup file you want to restore!';
        return;
      }
      this.error = null;
      try {
        this.isLoading = true;
        await this.$api.restoreBackup(this.restoreFile);
        this.successMessage = `Successfully restored "${this.restoreFile.name}".`;
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
        this.isLoading = false;
      }
    }
  }
};
</script>
