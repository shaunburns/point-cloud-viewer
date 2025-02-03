<template>
  <div class="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
    <div class="sm:mx-auto sm:w-full sm:max-w-sm">
      <img class="mx-auto h-10 w-auto" src="../assets/logo.svg" alt="Your Company" />
      <h2 class="mt-10 text-center text-2xl/9 font-bold tracking-tight text-gray-900">Sign in to your account</h2>
    </div>

    <div class="mt-10 sm:mx-auto sm:w-full sm:max-w-sm">
      <form class="space-y-6" @submit.prevent="login">
        <div>
          <label for="username" class="block text-sm/6 font-medium text-gray-900 text-left">Username</label>
          <div class="mt-2">
            <input type="text" name="username" id="username" autocomplete="username" v-model="username" required class="block w-full rounded-md bg-white px-3 py-1.5 text-base text-gray-900 outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600 sm:text-sm/6" />
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between">
            <label for="password" class="block text-sm/6 font-medium text-gray-900">Password</label>
            <div class="text-sm">
              <a href="#" class="font-semibold text-indigo-600 hover:text-indigo-500" tabindex="-1">Forgot password?</a>
            </div>
          </div>
          <div class="mt-2">
            <input type="password" name="password" id="password" autocomplete="current-password" v-model="password" required class="block w-full rounded-md bg-white px-3 py-1.5 text-base text-gray-900 outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600 sm:text-sm/6" />
          </div>
        </div>

        <div>
          <button type="submit" class="flex w-full justify-center rounded-md bg-indigo-600 px-3 py-1.5 text-sm/6 font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600">Sign in</button>
        </div>
      </form>

      <p v-if="error" class="text-red-500 mt-4">{{ error }}</p>

      <p class="mt-10 text-center text-sm/6 text-gray-500">
        Don't have an account?
        {{ ' ' }}
        <a href="#" class="font-semibold text-indigo-600 hover:text-indigo-500">Create a new account now</a>
      </p>
    </div>
  </div>
</template>


<script lang="ts">
  import { defineComponent, ref } from 'vue';
  import { useRouter } from 'vue-router';
  import { useAuthStore } from '../stores/auth';

  export default defineComponent({
    name: 'UserLogin',
    setup() {
      const username = ref('');
      const password = ref('');
      const error = ref('');
      const authStore = useAuthStore();
      const router = useRouter();

      const login = async () => {
        try {
          await authStore.login(username.value, password.value);
          error.value = '';
          router.push('/');
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
