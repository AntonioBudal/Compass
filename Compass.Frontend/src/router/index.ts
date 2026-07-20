import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import NowEngineView from '@/views/NowEngineView.vue';
import AgendaView from '@/views/AgendaView.vue';
import ProjectsView from '@/views/ProjectsView.vue';
import GoalsView from '@/views/GoalsView.vue';
import HabitsView from '@/views/HabitsView.vue';
import SettingsView from '@/views/SettingsView.vue';
import JournalView from '@/views/JournalView.vue';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/now'
  },
  {
    path: '/now',
    name: 'now',
    component: NowEngineView,
    meta: { title: 'Motor de Decisão (Now Engine)' }
  },
  {
    path: '/agenda',
    name: 'agenda',
    component: AgendaView,
    meta: { title: 'Agenda & Hard Blockers' }
  },
  {
    path: '/projects',
    name: 'projects',
    component: ProjectsView,
    meta: { title: 'Projetos Ativos' }
  },
  {
    path: '/goals',
    name: 'goals',
    component: GoalsView,
    meta: { title: 'Metas Estratégicas' }
  },
  {
    path: '/habits',
    name: 'habits',
    component: HabitsView,
    meta: { title: 'Hábitos Diários' }
  },
  {
    path: '/journal',
    name: 'journal',
    component: JournalView,
    meta: { title: 'Auditoria & Fechamento' }
  },
  {
    path: '/settings',
    name: 'settings',
    component: SettingsView,
    meta: { title: 'Configurações & Portabilidade' }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;