export default function ({ store, redirect, route }) {
  // If the user is not authenticated, redirect to login page.
  console.log('authentication middleware');
  console.log(route.path);
  if (store.state.auth.isLoggedIn !== true) {
    return redirect({
      path: '/login',
      query: { afterLogin: route.path }
    });
  }
}
