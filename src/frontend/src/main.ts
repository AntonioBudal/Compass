import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import App from './App.vue'

// Importa nossa folha de estilos utilitária monocromática globalmente
import './style.css'

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: true, 
      retry: 1, 
    },
  },
})

app.use(VueQueryPlugin, { queryClient })

app.mount('#app')