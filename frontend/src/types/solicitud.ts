export type EstadoSolicitud =
  | 'Nueva'
  | 'Asignada'
  | 'EnProceso'
  | 'Resuelta'
  | 'Cerrada'
  | 'Cancelada'

export type PrioridadSolicitud = 'Baja' | 'Media' | 'Alta' | 'Critica'

export interface CategoriaResumen {
  id: string
  nombre: string
}

export interface AgenteResumen {
  id: string
  nombre: string
}

export interface UsuarioResumen {
  id: string
  nombre: string
}

export interface SolicitudListItem {
  id: string
  codigo: string
  titulo: string
  estado: EstadoSolicitud
  prioridad: PrioridadSolicitud
  categoria: CategoriaResumen
  agente: AgenteResumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudListResponse {
  items: SolicitudListItem[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface SolicitudDetalle {
  id: string
  codigo: string
  titulo: string
  descripcion: string
  estado: EstadoSolicitud
  prioridad: PrioridadSolicitud
  categoria: CategoriaResumen
  agente: AgenteResumen | null
  solicitante: UsuarioResumen
  fechaCreacion: string
  fechaLimiteSla: string
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
  vencida: boolean
}

export interface Categoria {
  id: string
  nombre: string
  slaHoras: number
}

export interface CrearSolicitudRequest {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: PrioridadSolicitud
}

export interface EditarSolicitudRequest {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: PrioridadSolicitud
}

export interface TransicionRequest {
  accion: 'asignar' | 'iniciar' | 'resolver' | 'cerrar' | 'reabrir' | 'cancelar'
  agenteId?: string
  motivo?: string
}

export interface FiltrosSolicitudes {
  estado?: EstadoSolicitud
  prioridad?: PrioridadSolicitud
  categoriaId?: string
  agenteId?: string
  q?: string
  vencidas?: boolean
  page?: number
  pageSize?: number
  sort?: string
}