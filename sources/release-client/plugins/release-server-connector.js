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
      const data = (await this.axios.get('/statistics')).data;
      return data;
    };

    this.uploadPackage = async function (packageToUpload, { username, password }) {
      const formData = new FormData();
      formData.append(
        'package',
        packageToUpload,
        { type: 'application/json' }
      );
      await this.axios.put(
        '/',
        formData,
        { auth: { username, password } }
      );
    };

    this.login = async function (username, password) {
      try {
        console.log('Logging in...');
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
  }
}

export default ({ $axios, store }, inject) => {
  inject('api', new Api($axios, store));
};
