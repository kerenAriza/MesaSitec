import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Usuario } from '../types/auth'
import { login as loginApi } from '../api/auth'
import type { LoginRequest } from '../types/auth'

export const useAuthStore = defineStore('auth', () => {
  const usuarioGuardado = localStorage.getItem('usuario')

  const usuario = ref<Usuario | null>(
    usuarioGuardado ? (JSON.parse(usuarioGuardado) as Usuario) : null,
  )
  const token = ref<string | null>(localStorage.getItem('accessToken'))

  const estaAutenticado = computed(() => token.value !== null)

  async function iniciarSesion(credenciales: LoginRequest): Promise<void> {
    const respuesta = await loginApi(credenciales)

    token.value = respuesta.accessToken
    usuario.value = respuesta.usuario

    localStorage.setItem('accessToken', respuesta.accessToken)
    localStorage.setItem('usuario', JSON.stringify(respuesta.usuario))
  }

  function cerrarSesion(): void {
    token.value = null
    usuario.value = null

    localStorage.removeItem('accessToken')
    localStorage.removeItem('usuario')
  }

  return {
    usuario,
    token,
    estaAutenticado,
    iniciarSesion,
    cerrarSesion,
  }
})