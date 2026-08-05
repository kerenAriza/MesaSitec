<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const errorMensaje = ref('')
const cargando = ref(false)

async function manejarSubmit() {
  errorMensaje.value = ''
  cargando.value = true

  try {
    await authStore.iniciarSesion({ email: email.value, password: password.value })
    router.push({ name: 'solicitudes' })
  } catch (error: unknown) {
    errorMensaje.value = 'Email o contraseña incorrectos.'
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div>
    <h1>Iniciar sesión</h1>

    <form @submit.prevent="manejarSubmit">
      <div>
        <label for="email">Email</label>
        <input
          id="email"
          data-testid="login-email"
          v-model="email"
          type="email"
          required
        />
      </div>

      <div>
        <label for="password">Contraseña</label>
        <input
          id="password"
          data-testid="login-password"
          v-model="password"
          type="password"
          required
        />
      </div>

      <button data-testid="login-submit" type="submit" :disabled="cargando">
        {{ cargando ? 'Ingresando...' : 'Ingresar' }}
      </button>

      <p v-if="errorMensaje" data-testid="login-error">
        {{ errorMensaje }}
      </p>
    </form>
  </div>
</template>

<style scoped>
</style>