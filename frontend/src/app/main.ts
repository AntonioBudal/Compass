import { createApp } from 'vue'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import App from './App.vue'
import { router } from './router'
import './styles/tokens.css'

const app = createApp(App)

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 1000 * 60 * 5 // 5 minutes
    }
  }
})

app.use(VueQueryPlugin, { queryClient })
app.use(router)

app.mount('#app')
