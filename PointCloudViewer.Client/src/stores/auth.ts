import { defineStore } from 'pinia';
import axios from 'axios';
import { jwtDecode } from 'jwt-decode';

interface AuthState {
  token: string;
  axiosInterceptor: number | null;
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => {
    return {
      token: localStorage.getItem('token') || '',
      axiosInterceptor: null as number | null,
    };
  },
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

        if (this.axiosInterceptor == null) {
          this.axiosInterceptor = axios.interceptors.request.use((config) => {
            if (this.token) {
              config.headers.Authorization = `Bearer ${this.token}`;
            }
            return config;
          });
        }
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

        if (this.axiosInterceptor !== null) {
          axios.interceptors.request.eject(this.axiosInterceptor);
          this.axiosInterceptor = null;
        }
      }
    },
    async refresh() {
      try {
        const response = await axios.post('/api/auth/refresh', {});
        this.token = response.data.token;
        localStorage.setItem('token', this.token);
      } catch (error) {
        console.log(error);
        throw new Error('Failed to refresh token');
      }
    }
  },
  getters: {
    isAuthenticated(state: AuthState): boolean {
      if (!!state.token) {
        const decodedToken = jwtDecode(state.token);
        const expiryTime = decodedToken.exp * 1000;
        const currentTime = Date.now();
        if (expiryTime > currentTime) {
          return true;
        }
      }

      return false;
    },
  },
});
