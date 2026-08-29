import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import TaskCard from '../components/TaskCard.vue'
import type { TaskDto } from '@/entities/task/types'

describe('TaskEstimate (US2)', () => {
  const draftTask: TaskDto = {
    id: '01918a30-0000-7000-8000-000000000001',
    title: 'Tarefa Draft',
    description: null,
    durationMinutes: null,
    deadline: null,
    status: 'Draft',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    completedAt: null
  }

  it('opens inline estimate form and emits set-estimate with positive number', async () => {
    const wrapper = mount(TaskCard, {
      props: { task: draftTask }
    })

    // Click on "Definir Estimativa"
    await wrapper.find('.btn-estimate').trigger('click')
    expect(wrapper.find('.inline-estimate-box').exists()).toBe(true)

    // Fill in 45 minutes and save
    const input = wrapper.find('.estimate-input')
    await input.setValue(45)
    await wrapper.find('.estimate-controls button').trigger('click')

    expect(wrapper.emitted('set-estimate')).toBeTruthy()
    expect(wrapper.emitted('set-estimate')![0]).toEqual([draftTask.id, 45])
  })

  it('validates non-positive estimate in inline form', async () => {
    const wrapper = mount(TaskCard, {
      props: { task: draftTask }
    })

    await wrapper.find('.btn-estimate').trigger('click')
    const input = wrapper.find('.estimate-input')
    await input.setValue(0)
    await wrapper.find('.estimate-controls button').trigger('click')

    expect(wrapper.emitted('set-estimate')).toBeFalsy()
    expect(wrapper.find('.inline-error').text()).toContain('número positivo')
  })

  it('displays duration badge for Ready task', () => {
    const readyTask: TaskDto = {
      ...draftTask,
      status: 'Ready',
      durationMinutes: 45
    }

    const wrapper = mount(TaskCard, {
      props: { task: readyTask }
    })

    expect(wrapper.find('.duration-badge').text()).toContain('45m')
    expect(wrapper.find('.badge-ready').exists()).toBe(true)
  })
})
