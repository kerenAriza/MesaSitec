<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { obtenerSolicitud, crearSolicitud, editarSolicitud, listarCategorias } from '../api/solicitudes'
import { useToastStore } from '../stores/toast'
import type { Categoria, PrioridadSolicitud } from '../types/solicitud'

const route = useRoute()
const router = useRouter()
const toastStore = useToastStore()

const id = route.params.id as string | undefined
const esEdicion = computed(() => id !== undefined)

const titulo = ref('')
const descripcion = ref('')
const categoriaId = ref('')
const prioridad = ref<PrioridadSolicitud>('Media')

const categorias = ref<Categoria[]>([])

const errorTitulo = ref('')
const errorDescripcion = ref('')
const errorCategoria = ref('')

const cargando = ref(false)
const guardando = ref(false)

function validar(): boolean {
  errorTitulo.value = ''
  errorDescripcion.value = ''
  errorCategoria.value = ''
  let esValido = true

  if (titulo.value.length < 5 || titulo.value.length > 120) {
    errorTitulo.value = 'El título debe tener entre 5 y 120 caracteres.'
    esValido = false
  }

  if (descripcion.value.length < 10 || descripcion.value.length > 4000) {
    errorDescripcion.value = 'La descripción debe tener entre 10 y 4000 caracteres.'
    esValido = false
  }

  if (!categoriaId.value) {
    errorCategoria.value = 'Selecciona una categoría.'
    esValido = false
  }

  return esValido
}

async function manejarSubmit() {
  if (!validar()) {
    return
  }

  guardando.value = true

  try {
    const datos = {
      titulo: titulo.value,
      descripcion: descripcion.value,
      categoriaId: categoriaId.value,
      prioridad: prioridad.value,
    }

    if (esEdicion.value && id) {
      await editarSolicitud(id, datos)
      toastStore.mostrar('Solicitud actualizada con éxito.')
    } else {
      await crearSolicitud(datos)
      toastStore.mostrar('Solicitud creada con éxito.')
    }

    router.push({ name: 'solicitudes' })
  } catch {
    toastStore.mostrar('No se pudo guardar la solicitud. Revisa los datos.')
  } finally {
    guardando.value = false
  }
}

function cancelar() {
  router.push({ name: 'solicitudes' })
}

onMounted(async () => {
  cargando.value = true

  try {
    categorias.value = await listarCategorias()

    if (esEdicion.value && id) {
      const solicitud = await obtenerSolicitud(id)
      titulo.value = solicitud.titulo
      descripcion.value = solicitud.descripcion
      categoriaId.value = solicitud.categoria.id
      prioridad.value = solicitud.prioridad
    }
  } finally {
    cargando.value = false
  }
})
</script>

<template>
  <div>
    <h1>{{ esEdicion ? 'Editar solicitud' : 'Nueva solicitud' }}</h1>

    <div v-if="cargando">Cargando...</div>

    <form v-else @submit.prevent="manejarSubmit">
      <div>
        <label for="titulo">Título</label>
        <input id="titulo" data-testid="form-titulo" v-model="titulo" type="text" />
        <p v-if="errorTitulo" data-testid="error-titulo">{{ errorTitulo }}</p>
      </div>

      <div>
        <label for="descripcion">Descripción</label>
        <textarea id="descripcion" data-testid="form-descripcion" v-model="descripcion"></textarea>
        <p v-if="errorDescripcion" data-testid="error-descripcion">{{ errorDescripcion }}</p>
      </div>

      <div>
        <label for="categoria">Categoría</label>
        <select id="categoria" data-testid="form-categoria" v-model="categoriaId">
          <option value="">Selecciona una categoría</option>
          <option v-for="categoria in categorias" :key="categoria.id" :value="categoria.id">
            {{ categoria.nombre }}
          </option>
        </select>
        <p v-if="errorCategoria" data-testid="error-categoria">{{ errorCategoria }}</p>
      </div>

      <div>
        <label for="prioridad">Prioridad</label>
        <select id="prioridad" data-testid="form-prioridad" v-model="prioridad">
          <option value="Baja">Baja</option>
          <option value="Media">Media</option>
          <option value="Alta">Alta</option>
          <option value="Critica">Crítica</option>
        </select>
      </div>

      <button data-testid="form-submit" type="submit" :disabled="guardando">
        {{ guardando ? 'Guardando...' : 'Guardar' }}
      </button>

      <button data-testid="form-cancelar" type="button" @click="cancelar">
        Cancelar
      </button>
    </form>
  </div>
</template>

<style scoped>
</style>