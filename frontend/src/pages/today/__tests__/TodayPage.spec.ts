import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import TodayPage from '../TodayPage.vue'
import * as queryHook from '@/entities/schedule-profile/model/useScheduleProfileQuery'
import { ref } from 'vue'

describe('TodayPage.vue', () => {
  it('renders loading state when isLoading is true', () => {
    vi.spyOn(queryHook, 'useScheduleProfileQuery').mockReturnValue({
      data: ref(null),
      isLoading: ref(true)
    } as any)

    const wrapper = mount(TodayPage)
    expect(wrapper.find('.loading-state').exists()).toBe(true)
    expect(wrapper.text()).toContain('Carregando perfil')
  })

  it('renders profile data and availability when loaded', () => {
    vi.spyOn(queryHook, 'useScheduleProfileQuery').mockReturnValue({
      data: ref({
        id: '01918a22-38b2-7000-8000-000000000001',
        timeZoneId: 'America/Sao_Paulo',
        weeklyAvailability: [
          {
            dayOfWeek: 1, // Monday
            windows: [{ startTime: '09:00:00', endTime: '18:00:00' }]
          }
        ],
        createdAt: '2026-08-28T22:30:00Z',
        updatedAt: '2026-08-28T22:30:00Z'
      }),
      isLoading: ref(false)
    } as any)

    const wrapper = mount(TodayPage)
    expect(wrapper.find('.dashboard-content').exists()).toBe(true)
    expect(wrapper.text()).toContain('America/Sao_Paulo')
    expect(wrapper.text()).toContain('Grade de Disponibilidade Semanal')
  })
})
