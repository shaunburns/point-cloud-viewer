<template>
  <div id="app">
    <Login v-if="!isAuthenticated" />
    <div v-else>
      <h1>Welcome to the Point Cloud Viewer</h1>
      <button @click="logout">Logout</button>
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent, computed } from 'vue';
  import { useAuthStore } from './stores/auth';
  import Login from './components/TheLogin.vue';

  export default defineComponent({
    name: 'App',
    components: {
      Login,
    },
    setup() {
      const authStore = useAuthStore();
      const isAuthenticated = computed(() => authStore.isAuthenticated);

      const logout = () => {
        authStore.logout();
      };

      return {
        isAuthenticated,
        logout,
      };
    },
  });
</script>

<style>
  #app {
    text-align: center;
  }
</style>
