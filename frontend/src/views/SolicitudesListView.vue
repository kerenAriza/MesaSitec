<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { listarSolicitudes } from '../api/solicitudes'
import { listarCategorias } from '../api/solicitudes'
import type { SolicitudListItem, Categoria } from '../types/solicitud'

const router = useRouter()

const items = ref<SolicitudListItem[]>([])
const categorias = ref<Categoria[]>([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const totalPaginas = ref(0)

const filtroEstado = ref('')
const filtroPrioridad = ref('')
const filtroCategoria = ref('')
const filtroVencidas = ref(false)
const filtroBusqueda = ref('')

const cargando = ref(false)
const errorMensaje = ref('')

async function cargarSolicitudes() {
  cargando.value = true
  errorMensaje.value = ''

  try {
    const respuesta = await listarSolicitudes({
      estado: (filtroEstado.value || undefined) as SolicitudListItem['estado'] | undefined,
      prioridad: (filtroPrioridad.value || undefined) as SolicitudListItem['prioridad'] | undefined,
      categoriaId: filtroCategoria.value || undefined,
      vencidas: filtroVencidas.value ? true : undefined,
      q: filtroBusqueda.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
    })

    items.value = respuesta.items
    total.value = respuesta.total
    totalPaginas.value = respuesta.totalPaginas
  } catch {
    errorMensaje.value = 'No se pudieron cargar las solicitudes. Intenta de nuevo.'
  } finally {
    cargando.value = false
  }
}

async function cargarCategorias() {
  categorias.value = await listarCategorias()
}

function limpiarFiltros() {
  filtroEstado.value = ''
  filtroPrioridad.value = ''
  filtroCategoria.value = ''
  filtroVencidas.value = false
  filtroBusqueda.value = ''
  page.value = 1
}

function irADetalle(id: string) {
  router.push({ name: 'solicitudes-detalle', params: { id } })
}

function paginaAnterior() {
  if (page.value > 1) {
    page.value -= 1
  }
}

function paginaSiguiente() {
  if (page.value < totalPaginas.value) {
    page.value += 1
  }
}

watch([filtroEstado, filtroPrioridad, filtroCategoria, filtroVencidas, filtroBusqueda], () => {
  page.value = 1
  cargarSolicitudes()
})

watch(page, () => {
  cargarSolicitudes()
})

onMounted(() => {
  cargarCategorias()
  cargarSolicitudes()
})
</script>

<template>
  <div>
    <button data-testid="btn-nueva-solicitud" @click="router.push({ name: 'solicitudes-nueva' })">
      Nueva solicitud
    </button>

    <div>
      <select data-testid="filtro-estado" v-model="filtroEstado">
        <option value="">Todos los estados</option>
        <option value="Nueva">Nueva</option>
        <option value="Asignada">Asignada</option>
        <option value="EnProceso">En proceso</option>
        <option value="Resuelta">Resuelta</option>
        <option value="Cerrada">Cerrada</option>
        <option value="Cancelada">Cancelada</option>
      </select>

      <select data-testid="filtro-prioridad" v-model="filtroPrioridad">
        <option value="">Todas las prioridades</option>
        <option value="Baja">Baja</option>
        <option value="Media">Media</option>
        <option value="Alta">Alta</option>
        <option value="Critica">Crítica</option>
      </select>

      <select data-testid="filtro-categoria" v-model="filtroCategoria">
        <option value="">Todas las categorías</option>
        <option v-for="categoria in categorias" :key="categoria.id" :value="categoria.id">
          {{ categoria.nombre }}
        </option>
      </select>

      <label>
        <input data-testid="filtro-vencidas" type="checkbox" v-model="filtroVencidas" />
        Solo vencidas
      </label>

      <input
        data-testid="filtro-busqueda"
        type="text"
        v-model="filtroBusqueda"
        placeholder="Buscar por título, descripción o código"
      />

      <button data-testid="btn-limpiar-filtros" @click="limpiarFiltros">
        Limpiar filtros
      </button>
    </div>

    <div v-if="cargando" data-testid="listado-cargando">
      Cargando solicitudes...
    </div>

    <div v-else-if="errorMensaje">
      {{ errorMensaje }}
    </div>

    <div v-else-if="items.length === 0" data-testid="listado-vacio">
      No hay solicitudes que coincidan con los filtros.
    </div>

    <table v-else data-testid="tabla-solicitudes">
      <thead>
        <tr>
          <th>Código</th>
          <th>Título</th>
          <th>Estado</th>
          <th>Prioridad</th>
          <th>SLA</th>
          <th>Vencida</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="item in items"
          :key="item.id"
          data-testid="fila-solicitud"
          :data-codigo="item.codigo"
          @click="irADetalle(item.id)"
        >
          <td data-testid="celda-codigo">{{ item.codigo }}</td>
          <td>{{ item.titulo }}</td>
          <td data-testid="celda-estado">{{ item.estado }}</td>
          <td data-testid="celda-prioridad">{{ item.prioridad }}</td>
          <td data-testid="celda-sla">{{ item.fechaLimiteSla }}</td>
          <td>
            <span v-if="item.vencida" data-testid="badge-vencida">Vencida</span>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="!cargando && !errorMensaje && items.length > 0">
      <button data-testid="paginacion-anterior" :disabled="page <= 1" @click="paginaAnterior">
        Anterior
      </button>

      <span data-testid="paginacion-info">
        Página {{ page }} de {{ totalPaginas }} — {{ total }} resultados
      </span>

      <button
        data-testid="paginacion-siguiente"
        :disabled="page >= totalPaginas"
        @click="paginaSiguiente"
      >
        Siguiente
      </button>
    </div>
  </div>
</template>

<style scoped>
</style>