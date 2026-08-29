import { describe, it, expect } from 'vitest'
import {
  createInitialOnboardingState,
  validateAvailability,
  toWeeklyAvailability
} from '../model/onboardingState'

describe('StepAvailability Detailed Validation', () => {
  it('detects inverted time ranges across multiple windows', () => {
    const state = createInitialOnboardingState()
    // Add multiple windows on Tuesday, one of them invalid
    state.days[1].windows = [
      { startTime: '09:00', endTime: '12:00' },
      { startTime: '17:00', endTime: '13:00' } // Inverted
    ]

    const error = validateAvailability(state.days)
    expect(error).not.toBeNull()
    expect(error).toContain('Terça-feira')
    expect(error).toContain('deve ser anterior ao término')
  })

  it('detects empty start or end time strings', () => {
    const state = createInitialOnboardingState()
    state.days[0].windows = [{ startTime: '', endTime: '18:00' }]

    const error = validateAvailability(state.days)
    expect(error).not.toBeNull()
    expect(error).toContain('Preencha os horários')
  })

  it('accepts valid multi-window configuration for active days', () => {
    const state = createInitialOnboardingState()
    state.days[0].windows = [
      { startTime: '08:30', endTime: '12:00' },
      { startTime: '13:30', endTime: '18:00' }
    ]

    const error = validateAvailability(state.days)
    expect(error).toBeNull()

    const weekly = toWeeklyAvailability(state.days)
    expect(weekly.find((d) => d.dayOfWeek === 1)?.windows).toHaveLength(2)
  })
})
