<template>
  <div>
    <h2>Register</h2>
    <form @submit.prevent="registerNewUser">
      <input type="text" v-model="username" placeholder="Username" />
      <input type="text" v-model="email" placeholder="Email" />
      <input type="password" v-model="password" placeholder="Password" />
      <button type="submit">Sign up</button>
    </form>
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref } from 'vue';
  import { useAuthStore } from '../stores/auth';

  export default defineComponent({
    name: 'UserRegistration',
    setup() {
      const username = ref('');
      const email = ref('');
      const password = ref('');
      const error = ref('');
      const authStore = useAuthStore();

      const registerNewUser = async () => {
        try {
          await authStore.registerNewUser(username.value, email.value, password.value);
          error.value = '';
          this.$router.push('/');
        } catch (err) {
          error.value = err.message;
        }
      };

      return {
        username,
        email,
        password,
        error,
        registerNewUser,
      };
    },
  });
</script>
