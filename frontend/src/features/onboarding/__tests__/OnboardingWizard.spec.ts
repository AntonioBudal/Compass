import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import StepPresentation from '../components/StepPresentation.vue'
import StepTimeZone from '../components/StepTimeZone.vue'
import StepAvailability from '../components/StepAvailability.vue'
import StepConfirmation from '../components/StepConfirmation.vue'
import {
  createInitialOnboardingState,
  toWeeklyAvailability,
  validateAvailability
} from '../model/onboardingState'

describe('Onboarding Wizard Components & Model', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [
        { id: 'America/Sao_Paulo', displayName: '(UTC-03:00) Brasília Time', baseUtcOffset: '-03:00:00' },
        { id: 'UTC', displayName: '(UTC) Coordinated Universal Time', baseUtcOffset: '00:00:00' }
      ]
    }))
  })

  it('StepPresentation emits next when button clicked', async () => {
    const wrapper = mount(StepPresentation)
    const button = wrapper.find('button')
    expect(button.exists()).toBe(true)

    await button.trigger('click')
    expect(wrapper.emitted('next')).toHaveLength(1)
  })

  it('StepTimeZone emits updated modelValue and next', async () => {
    const wrapper = mount(StepTimeZone, {
      props: {
        modelValue: 'America/Sao_Paulo'
      }
    })

    const select = wrapper.find('select')
    await select.setValue('UTC')

    const nextBtn = wrapper.findAll('button').find((b) => b.text().includes('Avançar'))
    expect(nextBtn).toBeDefined()
    await nextBtn!.trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['UTC'])
    expect(wrapper.emitted('next')).toHaveLength(1)
  })

  it('StepAvailability validates invalid time ranges before emitting next', async () => {
    const state = createInitialOnboardingState()
    // Set invalid time range on Monday: 18:00 to 09:00
    state.days[0].windows[0] = { startTime: '18:00', endTime: '09:00' }

    const wrapper = mount(StepAvailability, {
      props: {
        days: state.days
      }
    })

    const nextBtn = wrapper.findAll('button').find((b) => b.text().includes('Avançar'))
    await nextBtn!.trigger('click')

    // Should NOT emit next because validation failed
    expect(wrapper.emitted('next')).toBeUndefined()
    expect(wrapper.find('.validation-alert').exists()).toBe(true)
    expect(wrapper.find('.validation-alert').text()).toContain('deve ser anterior ao término')
  })

  it('StepConfirmation displays selected timezone and emits confirm', async () => {
    const state = createInitialOnboardingState()

    const wrapper = mount(StepConfirmation, {
      props: {
        timeZoneId: 'America/Sao_Paulo',
        days: state.days,
        loading: false
      }
    })

    expect(wrapper.text()).toContain('America/Sao_Paulo')
    expect(wrapper.text()).toContain('Segunda-feira')

    const confirmBtn = wrapper.findAll('button').find((b) => b.text().includes('Confirmar'))
    expect(confirmBtn).toBeDefined()
    await confirmBtn!.trigger('click')

    expect(wrapper.emitted('confirm')).toHaveLength(1)
  })

  it('toWeeklyAvailability formats enabled days and times with seconds', () => {
    const state = createInitialOnboardingState()
    const result = toWeeklyAvailability(state.days)

    expect(result.length).toBeGreaterThan(0)
    expect(result[0].windows[0].startTime).toMatch(/^\d{2}:\d{2}:\d{2}$/)
    expect(result[0].windows[0].endTime).toMatch(/^\d{2}:\d{2}:\d{2}$/)
  })

  it('validateAvailability rejects if all days are disabled', () => {
    const state = createInitialOnboardingState()
    state.days.forEach((d) => (d.enabled = false))

    const error = validateAvailability(state.days)
    expect(error).toContain('pelo menos um dia')
  })
})
