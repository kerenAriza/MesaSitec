<script setup lang="ts">
import { useAuthStore } from './stores/auth'
import { useToastStore } from './stores/toast'
import AppNav from './components/AppNav.vue'

const authStore = useAuthStore()
const toastStore = useToastStore()
</script>

<template>
  <AppNav v-if="authStore.estaAutenticado" />

  <router-view />

  <div v-if="toastStore.visible" data-testid="toast-mensaje">
    {{ toastStore.mensaje }}
  </div>
</template>

<style scoped>
</style>