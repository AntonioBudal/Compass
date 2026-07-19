import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';

// Estendendo a tipagem dos metadados da rota (UI_SPECIFICATION.md - Cap. 4.3)
declare module 'vue-router' {
  interface RouteMeta {
    title: string;
    shortcut?: string;
    layout?: string;
  }
}

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/now'
  },
  {
    path: '/now',
    name: 'Engine.Now',
    component: () => import('@/views/NowEngineView.vue'),
    meta: { 
      title: 'Motor de Decisão (Agora)', 
      shortcut: 'G N',
      layout: 'AppLayout' 
    }
  },
  {
    path: '/agenda',
    name: 'Execution.Agenda',
    component: () => import('@/views/AgendaView.vue'),
    meta: { 
      title: 'Agenda & Hard Blockers', 
      shortcut: 'G A', 
      layout: 'AppLayout' 
    }
  },
  {
    path: '/projects',
    name: 'Strategy.Projects',
    component: () => import('@/views/ProjectsView.vue'),
    meta: { 
      title: 'Projetos Ativos', 
      shortcut: 'G P', 
      layout: 'AppLayout' 
    }
  },
  {
    path: '/goals',
    name: 'Strategy.Goals',
    component: () => import('@/views/GoalsView.vue'),
    meta: { 
      title: 'Metas Estratégicas', 
      shortcut: 'G G', 
      layout: 'AppLayout' 
    }
  },
  {
    path: '/habits',
    name: 'Execution.Habits',
    component: () => import('@/views/HabitsView.vue'),
    meta: { 
      title: 'Hábitos Diários', 
      shortcut: 'G H', 
      layout: 'AppLayout' 
    }
  }
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    // Como o App Shell é travado em 100vh (Cap. 4.1), o scroll é resetado no topo da view interna
    return { top: 0 };
  }
});

// Guard de navegação para atualizar o título do navegador (UX Industrial)
router.beforeEach((to, _from, next) => {
  const title = to.meta.title ? `Compass — ${to.meta.title}` : 'Compass';
  document.title = title;
  next();
});

export default router;