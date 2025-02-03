<template>
  <NavBar />
  <main>
    <RouterView />
  </main>
</template>

<script lang="ts">
  import { defineComponent, computed } from 'vue';
  import { useAuthStore } from './stores/auth';
  import UserLogin from './components/UserLogin.vue';
  import NavBar from './components/NavBar.vue'

  export default defineComponent({
    name: 'App',
    components: {
      UserLogin,
      NavBar,
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
