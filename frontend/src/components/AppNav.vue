<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

function manejarLogout() {
  authStore.cerrarSesion()
  router.push({ name: 'login' })
}
</script>

<template>
  <nav data-testid="app-nav">
    <span data-testid="nav-usuario-nombre">{{ authStore.usuario?.nombre }}</span>
    <span data-testid="nav-usuario-rol">{{ authStore.usuario?.rol }}</span>
    <button data-testid="btn-logout" @click="manejarLogout">Cerrar sesión</button>
  </nav>
</template>

<style scoped>
</style>