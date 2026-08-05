import { http } from './http'
import type {
  SolicitudListResponse,
  SolicitudDetalle,
  Categoria,
  FiltrosSolicitudes,
  CrearSolicitudRequest,
  EditarSolicitudRequest,
  TransicionRequest,
} from '../types/solicitud'

export async function listarSolicitudes(
  filtros: FiltrosSolicitudes,
): Promise<SolicitudListResponse> {
  const respuesta = await http.get<SolicitudListResponse>('/solicitudes', {
    params: filtros,
  })
  return respuesta.data
}

export async function obtenerSolicitud(id: string): Promise<SolicitudDetalle> {
  const respuesta = await http.get<SolicitudDetalle>(`/solicitudes/${id}`)
  return respuesta.data
}

export async function crearSolicitud(
  datos: CrearSolicitudRequest,
): Promise<SolicitudDetalle> {
  const respuesta = await http.post<SolicitudDetalle>('/solicitudes', datos)
  return respuesta.data
}

export async function editarSolicitud(
  id: string,
  datos: EditarSolicitudRequest,
): Promise<SolicitudDetalle> {
  const respuesta = await http.put<SolicitudDetalle>(`/solicitudes/${id}`, datos)
  return respuesta.data
}

export async function ejecutarTransicion(
  id: string,
  datos: TransicionRequest,
): Promise<SolicitudDetalle> {
  const respuesta = await http.post<SolicitudDetalle>(
    `/solicitudes/${id}/transiciones`,
    datos,
  )
  return respuesta.data
}

export async function listarCategorias(): Promise<Categoria[]> {
  const respuesta = await http.get<Categoria[]>('/categorias')
  return respuesta.data
}
