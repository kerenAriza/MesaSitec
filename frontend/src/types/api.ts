export interface ErrorApi {
  type: string
  title: string
  status: number
  detail: string
  codigo: string
  errores?: Record<string, string[]>
}