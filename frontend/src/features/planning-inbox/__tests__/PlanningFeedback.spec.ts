import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import EmptyState from '@/shared/ui/EmptyState.vue'

describe('PlanningFeedback (US4)', () => {
  it('renders EmptyState with title and description without emojis', () => {
    const wrapper = mount(EmptyState, {
      props: {
        title: 'Sua Inbox está vazia',
        description: 'Capture sua primeira tarefa.'
      }
    })

    expect(wrapper.text()).toContain('Sua Inbox está vazia')
    expect(wrapper.text()).toContain('Capture sua primeira tarefa.')
    expect(wrapper.text()).not.toMatch(/[\u{1F300}-\u{1FAFF}]/u)
  })
})
