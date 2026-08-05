export type Rol = 'Admin' | 'Agente' | 'Solicitante'

export interface Usuario {
  id: string
  nombre: string
  email: string
  rol: Rol
  tenantId: string
  tenantNombre: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: Usuario
}