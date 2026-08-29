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
    }
  ]
})

router.beforeEach((to, _from, next) => {
  const hasProfile = profileStorage.hasActiveProfile()
  if (to.name !== 'onboarding' && !hasProfile) {
    next({ name: 'onboarding' })
  } else {
    next()
  }
})
