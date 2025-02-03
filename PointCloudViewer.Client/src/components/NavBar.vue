<template>
  <MenuBar :model="items">
    <template #item="{ item, props, hasSubmenu }">
      <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
        <a :href="href" v-bind="props.action" @click="navigate">
          <span :class="item.icon" />
          <span>{{ item.label }}</span>
        </a>
      </router-link>
      <a v-else :href="item.url" :target="item.target" v-bind="props.action">
        <span :class="item.icon" />
        <span>{{ item.label }}</span>
        <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down" />
      </a>
    </template>

    <template #end>
      <div class="card flex justify-center">
        <Button type="button" @click="toggleProfileMenu" aria-haspopup="true" aria-controls="overlay_menu" variant="link">
          <Avatar image="https://primefaces.org/cdn/primevue/images/avatar/amyelsner.png" shape="circle" />
        </Button>
        <Menu ref="profileMenu" id="overlay_menu" :model="profileItems" :popup="true" />
      </div>
    </template>
  </MenuBar>
</template>

<script lang="ts">
  import { defineComponent, computed, ref } from 'vue';
  import { useAuthStore } from '../stores/auth';
  import MenuBar from 'primevue/menubar';
  import Avatar from 'primevue/avatar';
  import Menu from 'primevue/menu';
  import Button from 'primevue/button';
  import { useRouter } from 'vue-router';

  export default defineComponent({
    name: 'NavBar',
    components: {
      MenuBar,
      Avatar,
      Menu,
      Button,
    },
    setup() {
      const router = useRouter();
      const authStore = useAuthStore();
      const isAuthenticated = computed(() => authStore.isAuthenticated);
      const profileMenu = ref(null);

      const logout = async () => {
        await authStore.logout();
        router.push('/login');
      };

      const toggleProfileMenu = (event) => {
        profileMenu.value.toggle(event);
      };

      const items = [
        { label: 'Home', icon: '', route: '/', },
        { label: 'Test', icon: '', route: '/login', },
      ]

      const profileItems = [
        {
          label: 'Logout',
          icon: 'pi pi-sign-out',
          command: logout,
        },
      ];

      return {
        isAuthenticated,
        logout,
        items,
        profileItems,
        profileMenu,
        toggleProfileMenu,
      };
    },
  });
</script>
