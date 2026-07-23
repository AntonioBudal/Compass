import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';

// 1. ASYNC VIEW ROUTING (Lazy Loading)
// O Vite criará um chunk .js isolado para cada tela, baixado apenas no clique do menu!
const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/now'
  },
  {
    path: '/now',
    name: 'now',
    component: () => import('@/views/NowEngineView.vue'),
    meta: { title: 'Motor de Decisão (Now Engine)' }
  },
  {
    path: '/agenda',
    name: 'agenda',
    component: () => import('@/views/AgendaView.vue'),
    meta: { title: 'Agenda & Hard Blockers' }
  },
  {
    path: '/projects',
    name: 'projects',
    component: () => import('@/views/ProjectsView.vue'),
    meta: { title: 'Projetos Ativos' }
  },
  {
    path: '/goals',
    name: 'goals',
    component: () => import('@/views/GoalsView.vue'),
    meta: { title: 'Metas Estratégicas' }
  },
  {
    path: '/habits',
    name: 'habits',
    component: () => import('@/views/HabitsView.vue'),
    meta: { title: 'Hábitos Diários' }
  },
  {
    path: '/journal',
    name: 'journal',
    component: () => import('@/views/JournalView.vue'),
    meta: { title: 'Auditoria & Fechamento' }
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('@/views/SettingsView.vue'),
    meta: { title: 'Configurações & Portabilidade' }
  },
  {
    path: '/sandbox',
    name: 'sandbox',
    component: () => import('@/views/OnboardingView.vue'),
    meta: { title: 'RAM Sandbox (Modo Treinamento)', requiresAuth: false, isSandbox: true }
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

router.beforeEach((to, from, next) => {
  // Atualiza dinamicamente o título da aba do navegador para imersão tática
  if (to.meta.title) {
    document.title = `${to.meta.title} | Compass`;
  } else {
    document.title = 'Compass Local-First';
  }
  next();
});

export default router;