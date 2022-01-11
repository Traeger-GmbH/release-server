<template>
  <CardModal
    :showing="showing"
    @close="onClose"
    class="text-center"
  >
    <form-wizard
      ref="deleteWizard"
      title="Delete release"
      subtitle="Delete the selected release of the product."
      color="#10b981"
    >
      <tab-content
        title="Confirm"
      >
        <div class="w-full flex flex-wrap justify-center items-center gap-2">
          <label class="text-center" for="confirmReleaseName">
            Confirm by typing the name of the release:
          </label>
          <div class="text-center font-semibold p-1 rounded bg-green-200 grow-0">{{ releaseName }}</div>
          <input id="confirmReleaseName" class="border border-gray-300 w-full p-2 rounded text-center font-semibold" type="text" v-model="confirmReleaseName" />
        </div>
      </tab-content>
      <tab-content title="Delete">
        <div
          class="flex justify-center"
        >
          <template
            v-if="isDeleting"
          >
            <div>
              <VueSpinner size="medium" line-fg-color="#10b981" />
              Deleting release...
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
            @click="deleteRelease() && props.nextTab()"
            class="btn btn-green"
            :disabled="!isValid"
          >
            Delete
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
    productIdentifier: {
      type: String,
      required: true
    },
    version: {
      type: String,
      required: true
    },
    showing: {
      type: Boolean,
      required: true
    }
  },
  data () {
    return {
      confirmReleaseName: null,
      successMessage: null,
      error: null,
      isDeleting: false
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
      this.error = null;
      this.$emit('close');
    },
    async deleteRelease () {
      this.isDeleting = true;
      try {
        await this.$api.deleteSpecificRelease(this.productIdentifier, this.version);
        this.successMessage = `Successfully deleted "${this.releaseName}".`;
        this.restoreFile = null;
        this.$emit('deleted');
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
        this.isDeleting = false;
      }
    }
  }
};
</script>
