const getFileNameFromHeaders = function (headers) {
  const contentDisposition = headers['content-disposition'];
  const values = contentDisposition.replace(/\s+/g, '').split(';');
  const filenameEntry = values.find((element) => {
    const keyValuePair = element.split('=');
    if (keyValuePair.length === 2 && keyValuePair[0] === 'filename') {
      return true;
    } else {
      return false;
    }
  });
  return filenameEntry.split('=')[1];
};

class Api {
  constructor ($axios, store) {
    this.axios = $axios.create({
      baseURL: '/artifacts/'
    });

    this.getProductList = async function () {
      const data = (await this.axios.get()).data;
      console.log(data);
      return data.products;
    };

    this.getProductInfo = async function (productName) {
      const data = (await this.axios.get(`${productName}/info`)).data;
      return data;
    };

    this.getStatistics = async function () {
      console.log(this.axios.defaults);
      const data = (await this.axios.get('/statistics', {
        auth: store.state.auth
      })).data;
      return data;
    };

    this.uploadPackage = async function (packageToUpload, force = false) {
      const formData = new FormData();
      formData.append(
        'package',
        packageToUpload,
        { type: 'application/json' },
      );
      await this.axios.put(
        '/',
        formData,
        {
          auth: store.state.auth,
          params: { force }
        }
      );
    };

    this.deleteSpecificArtifact = async function (product, platform, version) {
      await this.axios.delete(
        `/${product}/${platform}/${version}`,
        { auth: store.state.auth }
      );
    };

    this.downloadSpecificArtifact = async function (product, platform, version) {
      const response = await this.axios.get(
        `/${product}/${platform}/${version}`
      );
      const blob = new Blob([response.data]);
      const filename = getFileNameFromHeaders(response.headers);
      return { blob, filename };
    };

    this.deleteSpecificRelease = async function (product, version) {
      const data = (await this.axios.get(`${product}/info`, { params: { version } })).data;
      const platforms = data.releases[0].platforms;
      for (const platform of platforms) {
        await this.deleteSpecificArtifact(product, platform, version);
      }
    };

    this.restoreBackup = async function (backupToRestore) {
      const formData = new FormData();
      formData.append(
        'backupFile',
        backupToRestore,
        { type: 'application/octet-stream' }
      );
      await this.axios.put(
        '/restore',
        formData,
        { auth: store.state.auth }
      );
    };

    this.login = async function (username, password) {
      try {
        await this.axios.get('/statistics', {
          auth: {
            username,
            password
          }
        });
        store.commit('auth/login', { username, password });
        this.axios.defaults.auth = {
          username,
          password
        };
      } catch (error) {
        console.log(error);
        throw error;
      }
    };

    this.logout = function () {
      store.commit('auth/logout');
      this.axios.defaults.auth = null;
    };

    this.createBackup = async function () {
      const response = await this.axios.get(
        '/backup',
        {
          auth: store.state.auth,
          responseType: 'blob'
        }
      );
      const blob = new Blob([response.data]);
      const filename = getFileNameFromHeaders(response.headers);
      return { blob, filename };
    };
  }
}

export default ({ $axios, store }, inject) => {
  inject('api', new Api($axios, store));
};
