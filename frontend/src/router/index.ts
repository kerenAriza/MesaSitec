import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { requiereAuth: false },
    },
    {
      path: '/',
      redirect: '/solicitudes',
    },
    {
      path: '/solicitudes',
      name: 'solicitudes',
      component: () => import('../views/SolicitudesListView.vue'),
      meta: { requiereAuth: true },
    },
    {
      path: '/solicitudes/nueva',
      name: 'solicitudes-nueva',
      component: () => import('../views/SolicitudFormView.vue'),
      meta: { requiereAuth: true },
    },
    {
      path: '/solicitudes/:id',
      name: 'solicitudes-detalle',
      component: () => import('../views/SolicitudDetalleView.vue'),
      meta: { requiereAuth: true },
    },
    {
      path: '/solicitudes/:id/editar',
      name: 'solicitudes-editar',
      component: () => import('../views/SolicitudFormView.vue'),
      meta: { requiereAuth: true },
    },
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  const requiereAuth = to.meta.requiereAuth !== false

  if (requiereAuth && !authStore.estaAutenticado) {
    return { name: 'login' }
  }

  if (to.name === 'login' && authStore.estaAutenticado) {
    return { name: 'solicitudes' }
  }

  return true
})

export default router