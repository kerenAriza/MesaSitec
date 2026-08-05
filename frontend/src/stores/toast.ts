import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useToastStore = defineStore('toast', () => {
  const mensaje = ref('')
  const visible = ref(false)

  function mostrar(texto: string) {
    mensaje.value = texto
    visible.value = true

    setTimeout(() => {
      visible.value = false
    }, 4000)
  }

  return { mensaje, visible, mostrar }
})
