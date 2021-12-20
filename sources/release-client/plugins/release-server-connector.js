import axios from 'axios';

class Api {
  constructor ($axios) {
    this.axios = axios.create({
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
  }
}

export default (context, inject) => {
  inject('api', new Api(context.$axios));
};
