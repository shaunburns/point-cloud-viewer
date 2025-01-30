<template>
  <div class="login">
    <h2>Login</h2>
    <form @submit.prevent="login">
      <div>
        <label for="username">Username:</label>
        <input type="text" v-model="username" required />
      </div>
      <div>
        <label for="password">Password:</label>
        <input type="password" v-model="password" required />
      </div>
      <button type="submit">Login</button>
    </form>
    <p v-if="error">{{ error }}</p>
  </div>
</template>

<script lang="ts">import { defineComponent, ref } from 'vue';
import { useAuthStore } from '../stores/auth';

export default defineComponent({
  name: 'TheLogin',
  setup() {
    const username = ref('');
    const password = ref('');
    const error = ref('');
    const authStore = useAuthStore();

    const login = async () => {
      try {
        await authStore.login(username.value, password.value);
        error.value = '';
      } catch (err) {
        error.value = err.message;
      }
    };

    return {
      username,
      password,
      error,
      login,
    };
  },
});</script>

<style scoped>
  .login {
    max-width: 300px;
    margin: auto;
  }
</style>
