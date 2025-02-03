import { createRouter, createWebHistory } from 'vue-router';
import UserLogin from './components/UserLogin.vue';
import UserRegistration from './components/UserRegistration.vue';
import TheWelcome from './components/TheWelcome.vue';
import { useAuthStore } from './stores/auth';

const routes = [
  { name: 'login', path: '/login', component: UserLogin },
  { name: 'register', path: '/register', component: UserRegistration },
  { name: 'home', path: '/', component: TheWelcome, meta: { requiresAuth: true } }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach(async (to) => {
  const authStore = useAuthStore();
  if (!authStore.isAuthenticated && to.name !== 'login') {
    return { name: 'login' }
  }
})

export default router;
