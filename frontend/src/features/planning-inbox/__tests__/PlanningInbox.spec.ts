import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import QuickTaskCapture from '../components/QuickTaskCapture.vue'
import TaskFilterTabs from '../components/TaskFilterTabs.vue'
import TaskCard from '../components/TaskCard.vue'
import type { TaskDto } from '@/entities/task/types'

describe('QuickTaskCapture', () => {
  it('emits capture event when title is submitted', async () => {
    const wrapper = mount(QuickTaskCapture)

    const input = wrapper.find('input')
    await input.setValue('Nova Tarefa Teste')
    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('capture')).toBeTruthy()
    expect(wrapper.emitted('capture')![0]).toEqual(['Nova Tarefa Teste'])
  })

  it('shows error and does not emit if title is empty', async () => {
    const wrapper = mount(QuickTaskCapture)

    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('capture')).toBeFalsy()
    expect(wrapper.text()).toContain('Informe um título')
  })
})

describe('TaskFilterTabs', () => {
  it('renders all tab options and emits update when clicked', async () => {
    const wrapper = mount(TaskFilterTabs, {
      props: {
        modelValue: 'ALL',
        counts: { ALL: 3, Draft: 1, Ready: 1, InProgress: 1, Done: 0 }
      }
    })

    const buttons = wrapper.findAll('button[role="tab"]')
    expect(buttons.length).toBe(5)

    await buttons[1].trigger('click')
    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')![0]).toEqual(['Draft'])
  })
})

describe('TaskCard (US1)', () => {
  const mockTask: TaskDto = {
    id: '01918a30-0000-7000-8000-000000000001',
    title: 'Tarefa Draft Inicial',
    description: 'Descrição de teste',
    durationMinutes: null,
    deadline: null,
    status: 'Draft',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    completedAt: null
  }

  it('renders draft task with missing duration badge and estimate action', () => {
    const wrapper = mount(TaskCard, {
      props: { task: mockTask }
    })

    expect(wrapper.text()).toContain('Tarefa Draft Inicial')
    expect(wrapper.text()).toContain('Draft')
    expect(wrapper.text()).toContain('Sem estimativa')
    expect(wrapper.find('.btn-estimate').exists()).toBe(true)
  })
})
