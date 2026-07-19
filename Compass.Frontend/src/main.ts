import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import './style.css'
import App from './App.vue'

const app = createApp(App)

// Registro arquitetural do Estado e do Roteamento
app.use(createPinia())
app.use(router)

app.mount('#app')
