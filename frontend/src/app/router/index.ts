import { createRouter, createWebHistory } from 'vue-router'
import { profileStorage } from '@/entities/schedule-profile/model/profileStorage'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: () => {
        return profileStorage.hasActiveProfile() ? '/today' : '/onboarding'
      }
    },
    {
      path: '/onboarding',
      name: 'onboarding',
      component: () => import('@/pages/onboarding/OnboardingPage.vue')
    },
    {
      path: '/today',
      name: 'today',
      component: () => import('@/pages/today/TodayPage.vue')
    },
    {
      path: '/planning',
      name: 'planning',
      component: () => import('@/pages/planning/PlanningPage.vue')
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/pages/not-found/NotFoundPage.vue')
    }
  ]
})

router.beforeEach((to, _from, next) => {
  const hasProfile = profileStorage.hasActiveProfile()
  if (to.name !== 'onboarding' && to.name !== 'not-found' && !hasProfile) {
    next({ name: 'onboarding' })
  } else {
    next()
  }
})
