module.exports = {
  root: true,
  env: {
    browser: true,
    node: true
  },
  parserOptions: {
    parser: '@babel/eslint-parser',
    requireConfigFile: false
  },
  extends: [
    '@nuxtjs',
    'plugin:nuxt/recommended'
  ],
  plugins: [
  ],
  // add your custom rules here
  rules: {
    semi: [2, "always"],
    "comma-dangle": "off",
    "vue/no-template-shadow": "off",
    "vue/multi-word-component-names": "off"
  },
}
