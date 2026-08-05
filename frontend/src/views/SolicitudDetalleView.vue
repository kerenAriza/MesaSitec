<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { obtenerSolicitud, ejecutarTransicion,  } from '../api/solicitudes'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import type { SolicitudDetalle, EstadoSolicitud } from '../types/solicitud'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const solicitud = ref<SolicitudDetalle | null>(null)
const cargando = ref(false)
const errorMensaje = ref('')

const id = route.params.id as string

async function cargar() {
  cargando.value = true
  errorMensaje.value = ''

  try {
    solicitud.value = await obtenerSolicitud(id)
  } catch {
    errorMensaje.value = 'No se pudo cargar la solicitud.'
  } finally {
    cargando.value = false
  }
}

onMounted(cargar)

// --- Reglas de visibilidad de botones (RN-02 + RN-03, sección 7.5) ---

const rol = computed(() => authStore.usuario?.rol)
const usuarioId = computed(() => authStore.usuario?.id)

const esElSolicitante = computed(
  () => solicitud.value?.solicitante.id === usuarioId.value,
)

const transicionesPorEstado: Record<EstadoSolicitud, string[]> = {
  Nueva: ['asignar', 'cancelar'],
  Asignada: ['iniciar', 'asignar', 'cancelar'],
  EnProceso: ['resolver', 'asignar', 'cancelar'],
  Resuelta: ['cerrar', 'reabrir'],
  Cerrada: [],
  Cancelada: [],
}

function accionPermitidaPorRol(accion: string): boolean {
  if (rol.value === 'Admin') return true
  if (rol.value === 'Agente') return accion !== 'cancelar'
  if (rol.value === 'Solicitante') {
    return accion === 'cerrar' && esElSolicitante.value
  }
  return false
}

const accionesVisibles = computed(() => {
  if (!solicitud.value) return []

  const accionesDelEstado = transicionesPorEstado[solicitud.value.estado]
  return accionesDelEstado.filter((accion) => accionPermitidaPorRol(accion))
})

const puedeEditar = computed(() => {
  if (!solicitud.value) return false
  if (rol.value === 'Admin' || rol.value === 'Agente') return true
  if (rol.value === 'Solicitante') {
    return esElSolicitante.value && solicitud.value.estado === 'Nueva'
  }
  return false
})

// --- Modal de acción ---

const modalAbierto = ref(false)
const accionActual = ref('')
const agenteSeleccionado = ref('')
const motivoTexto = ref('')
const modalErrorMensaje = ref('')

function abrirModal(accion: string) {
  accionActual.value = accion
  agenteSeleccionado.value = ''
  motivoTexto.value = ''
  modalErrorMensaje.value = ''
  modalAbierto.value = true
}

function cerrarModal() {
  modalAbierto.value = false
}

async function confirmarAccion() {
  modalErrorMensaje.value = ''

  try {
    await ejecutarTransicion(id, {
      accion: accionActual.value as never,
      agenteId: agenteSeleccionado.value || undefined,
      motivo: motivoTexto.value || undefined,
    })

    modalAbierto.value = false
    toastStore.mostrar('Acción ejecutada con éxito.')
    await cargar()
  } catch (error: unknown) {
    modalErrorMensaje.value = 'No se pudo completar la acción. Verifica los datos.'
  }
}
</script>

<template>
  <div>
    <div v-if="cargando">Cargando...</div>
    <div v-else-if="errorMensaje">{{ errorMensaje }}</div>

    <div v-else-if="solicitud">
      <p data-testid="detalle-codigo">{{ solicitud.codigo }}</p>
      <h1 data-testid="detalle-titulo">{{ solicitud.titulo }}</h1>
      <p data-testid="detalle-descripcion">{{ solicitud.descripcion }}</p>
      <p data-testid="detalle-estado">{{ solicitud.estado }}</p>
      <p data-testid="detalle-prioridad">{{ solicitud.prioridad }}</p>
      <p data-testid="detalle-categoria">{{ solicitud.categoria.nombre }}</p>
      <p data-testid="detalle-agente">{{ solicitud.agente?.nombre ?? 'Sin asignar' }}</p>
      <p data-testid="detalle-fecha-creacion">{{ solicitud.fechaCreacion }}</p>
      <p data-testid="detalle-fecha-limite">{{ solicitud.fechaLimiteSla }}</p>
      <p v-if="solicitud.vencida" data-testid="detalle-vencida">Vencida</p>
      <p v-if="solicitud.motivoResolucion" data-testid="detalle-motivo">
        {{ solicitud.motivoResolucion }}
      </p>
      <p v-else-if="solicitud.motivoCancelacion" data-testid="detalle-motivo">
        {{ solicitud.motivoCancelacion }}
      </p>

      <button
        v-if="puedeEditar"
        data-testid="btn-editar"
        @click="router.push({ name: 'solicitudes-editar', params: { id } })"
      >
        Editar
      </button>

      <button
        v-if="accionesVisibles.includes('asignar')"
        data-testid="btn-accion-asignar"
        @click="abrirModal('asignar')"
      >
        Asignar
      </button>

      <button
        v-if="accionesVisibles.includes('iniciar')"
        data-testid="btn-accion-iniciar"
        @click="abrirModal('iniciar')"
      >
        Iniciar
      </button>

      <button
        v-if="accionesVisibles.includes('resolver')"
        data-testid="btn-accion-resolver"
        @click="abrirModal('resolver')"
      >
        Resolver
      </button>

      <button
        v-if="accionesVisibles.includes('cerrar')"
        data-testid="btn-accion-cerrar"
        @click="abrirModal('cerrar')"
      >
        Cerrar
      </button>

      <button
        v-if="accionesVisibles.includes('reabrir')"
        data-testid="btn-accion-reabrir"
        @click="abrirModal('reabrir')"
      >
        Reabrir
      </button>

      <button
        v-if="accionesVisibles.includes('cancelar')"
        data-testid="btn-accion-cancelar"
        @click="abrirModal('cancelar')"
      >
        Cancelar
      </button>
    </div>

    <div v-if="modalAbierto" data-testid="modal-accion">
      <p>Acción: {{ accionActual }}</p>

      <select
        v-if="accionActual === 'asignar'"
        data-testid="modal-select-agente"
        v-model="agenteSeleccionado"
      >
        <option value="">Selecciona un agente</option>
      </select>

      <textarea
        v-if="accionActual === 'resolver' || accionActual === 'cancelar'"
        data-testid="modal-motivo"
        v-model="motivoTexto"
        placeholder="Motivo"
      ></textarea>

      <p v-if="modalErrorMensaje" data-testid="modal-error">{{ modalErrorMensaje }}</p>

      <button data-testid="modal-confirmar" @click="confirmarAccion">Confirmar</button>
      <button data-testid="modal-cancelar" @click="cerrarModal">Cancelar</button>
    </div>
  </div>
</template>

<style scoped>
</style>