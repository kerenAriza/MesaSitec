import { http } from './http'
import type { LoginRequest, LoginResponse, Usuario } from '../types/auth'

export async function login(datos: LoginRequest): Promise<LoginResponse> {
  const respuesta = await http.post<LoginResponse>('/auth/login', datos)
  return respuesta.data
}

export async function obtenerPerfil(): Promise<Usuario> {
  const respuesta = await http.get<Usuario>('/me')
  return respuesta.data
}