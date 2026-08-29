import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import AppHeader from '../AppHeader.vue'
import AppShell from '../AppShell.vue'

vi.mock('vue-router', async (importOriginal) => {
  const actual = (await importOriginal()) as any
  return {
    ...actual,
    useRouter: () => ({
      push: vi.fn()
    }),
    useRoute: () => ({
      path: '/today'
    })
  }
})

describe('AppHeader.vue', () => {
  it('renders brand name and navigation links without emojis', () => {
    const wrapper = mount(AppHeader, {
      global: {
        stubs: {
          RouterLink: {
            template: '<a><slot /></a>'
          }
        }
      }
    })

    expect(wrapper.text()).toContain('Compass')
    expect(wrapper.text()).toContain('Hoje')
    expect(wrapper.text()).toContain('Planning')
    // Ensure no emojis exist in rendered text
    expect(wrapper.text()).not.toMatch(/[\u{1F300}-\u{1FAFF}]/u)
  })
})

describe('AppShell.vue', () => {
  it('renders header and slot content', () => {
    const wrapper = mount(AppShell, {
      slots: {
        default: '<div class="test-content">Conteúdo da página</div>'
      },
      global: {
        stubs: {
          RouterLink: {
            template: '<a><slot /></a>'
          }
        }
      }
    })

    expect(wrapper.find('header').exists()).toBe(true)
    expect(wrapper.find('.test-content').text()).toBe('Conteúdo da página')
  })
})
