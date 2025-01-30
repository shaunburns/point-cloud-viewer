import { defineStore } from 'pinia';
import axios from 'axios';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || '',
  }),
  actions: {
    async login(username: string, password: string) {
      try {
        const response = await axios.post('/api/auth/login', { username, password });
        this.token = response.data.token;
        localStorage.setItem('token', this.token);
      } catch (error) {
        console.log(error);
        throw new Error('Invalid username or password');
      }
    },
    logout() {
      this.token = '';
      localStorage.removeItem('token');
    },
  },
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
});
