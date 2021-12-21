<template>
  <Page title="Login" back-link="/">
    <div class="max-w-sm mx-auto mt-5">
      <FormInput
        id="username"
        v-model="username"
        title="Username"
        name="username"
        type="text"
      />
      <FormInput
        id="password"
        v-model="password"
        title="Password"
        name="username"
        type="password"
      />
      <div class="w-full flex justify-end">
        <button
          class="btn btn-green"
          type="button"
          title="Login"
          @click="login()"
        >
          login
        </button>
      </div>
    </div>
  </Page>
</template>

<script>
export default {
  data () {
    return {
      username: 'TestUser',
      password: 'SomePassword'
    };
  },
  methods: {
    async login () {
      await this.$api.login(this.username, this.password);
      const path = this.$route.query.afterLogin ?? '/';
      console.log('new path');
      console.log(path);
      this.$router.push({ path });
    }
  }
};
</script>

<style>

</style>
