const ACTIVE_PROFILE_STORAGE_KEY = 'compass_active_profile_id'

export const profileStorage = {
  getActiveProfileId(): string | null {
    return localStorage.getItem(ACTIVE_PROFILE_STORAGE_KEY)
  },

  setActiveProfileId(id: string): void {
    if (id && id.trim()) {
      localStorage.setItem(ACTIVE_PROFILE_STORAGE_KEY, id.trim())
    }
  },

  clearActiveProfileId(): void {
    localStorage.removeItem(ACTIVE_PROFILE_STORAGE_KEY)
  },

  hasActiveProfile(): boolean {
    const id = this.getActiveProfileId()
    return !!id && id.trim().length > 0
  }
}
