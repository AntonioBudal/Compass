import { describe, it, expect, beforeEach } from 'vitest'
import { profileStorage } from '@/entities/schedule-profile/model/profileStorage'

describe('Router Auth & Profile Guard Logic', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('hasActiveProfile returns false when localStorage is empty', () => {
    expect(profileStorage.hasActiveProfile()).toBe(false)
    expect(profileStorage.getActiveProfileId()).toBeNull()
  })

  it('hasActiveProfile returns true when valid id stored', () => {
    profileStorage.setActiveProfileId('01918a22-38b2-7000-8000-000000000001')
    expect(profileStorage.hasActiveProfile()).toBe(true)
    expect(profileStorage.getActiveProfileId()).toBe('01918a22-38b2-7000-8000-000000000001')
  })

  it('clearActiveProfileId removes key from storage', () => {
    profileStorage.setActiveProfileId('01918a22-38b2-7000-8000-000000000001')
    profileStorage.clearActiveProfileId()
    expect(profileStorage.hasActiveProfile()).toBe(false)
  })
})
