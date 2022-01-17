<template>
  <CardModal
    :showing="showing"
    @close="onClose"
    class="text-center"
  >
    <form-wizard
      ref="createBackupDialog"
      title="Create backup"
      subtitle="Create a backup of the current server data."
      color="#10b981"
    >
      <tab-content
        title="Start backup"
      >
      </tab-content>
      <tab-content title="Run backup">
        <div
          class="flex justify-center"
        >
          <template
            v-if="isLoading"
          >
            <div>
              <VueSpinner size="medium" line-fg-color="#10b981" />
              Creating backup...
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
            @click="onClose()"
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
            @click="createBackup() && props.nextTab()"
            class="btn btn-green"
          >
            Create backup
          </button>
          <button
            v-if="props.isLastStep"
            @click="onClose()"
            class="btn btn-green"
            :disabled="isLoading || error"
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
      successMessage: null,
      error: null,
      isLoading: false
    };
  },
  computed: {
    isValid () {
      return this.confirmReleaseName === this.releaseName;
    },
    releaseName () {
      return `${this.productIdentifier}-v${this.version}`;
    }
  },
  methods: {
    onClose () {
      this.confirmReleaseName = null;
      this.successMessage = null;
      this.isLoading = false;
      this.error = null;
      this.$emit('close');
    },
    forceFileDownload (blob, filename) {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', filename);
      document.body.appendChild(link);
      link.click();
    },
    async createBackup () {
      this.isLoading = true;
      try {
        const { blob, filename } = await this.$api.createBackup();
        this.successMessage = 'Successfully created backup.';
        this.forceFileDownload(blob, filename);
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
