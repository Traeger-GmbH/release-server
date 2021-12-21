export const state = () => ({
  username: null,
  password: null,
  isLoggedIn: false
});

export const mutations = {
  login (state, { username, password }) {
    state.username = username;
    state.password = password;
    state.isLoggedIn = true;
  },
  logout (state) {
    state.username = null;
    state.password = null;
    state.isLoggedIn = false;
  },
};
