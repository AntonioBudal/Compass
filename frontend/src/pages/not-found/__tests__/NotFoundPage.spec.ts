import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import NotFoundPage from '../NotFoundPage.vue'
import { profileStorage } from '@/entities/schedule-profile/model/profileStorage'

const pushMock = vi.fn()
vi.mock('vue-router', async (importOriginal) => {
  const actual = (await importOriginal()) as any
  return {
    ...actual,
    useRouter: () => ({
      push: pushMock
    }),
    useRoute: () => ({
      path: '/unknown'
    })
  }
})

describe('NotFoundPage.vue', () => {
  it('renders 404 status and message without emojis', () => {
    const wrapper = mount(NotFoundPage, {
      global: {
        stubs: {
          RouterLink: { template: '<a><slot /></a>' }
        }
      }
    })

    expect(wrapper.text()).toContain('404')
    expect(wrapper.text()).toContain('Página não encontrada')
    expect(wrapper.text()).not.toMatch(/[\u{1F300}-\u{1FAFF}]/u)
  })

  it('redirects to /today if active profile exists when clicking goHome', async () => {
    vi.spyOn(profileStorage, 'hasActiveProfile').mockReturnValue(true)
    const wrapper = mount(NotFoundPage, {
      global: {
        stubs: {
          RouterLink: { template: '<a><slot /></a>' }
        }
      }
    })

    const btn = wrapper.find('button')
    await btn.trigger('click')
    expect(pushMock).toHaveBeenCalledWith('/today')
  })
})
