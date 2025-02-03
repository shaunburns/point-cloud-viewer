import { defineStore } from 'pinia';
import axios from 'axios';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || '',
  }),
  actions: {
    async registerNewUser(username: string, email: string, password: string) {
      try {
        await axios.post('/api/auth/register', { username, email, password });
      } catch (error) {
        console.log(error);
        throw new Error('Failed to register new user');
      }
    },
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
    async logout() {
      try {
        await axios.post('/api/auth/logout', {});
      } catch (error) {
        console.log(error);
      }
      finally {
        this.token = '';
        localStorage.removeItem('token');
      }
    },
  },
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
});
