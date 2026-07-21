import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import './style.css'; // estilos Tailwind

import { setupGlobalErrorHandler } from './plugins/globalErrorHandler';

const app = createApp(App);

const pinia = createPinia();
app.use(pinia);
app.use(router);

// Instala a rede de arrastão de erros global
setupGlobalErrorHandler(app);

app.mount('#app');