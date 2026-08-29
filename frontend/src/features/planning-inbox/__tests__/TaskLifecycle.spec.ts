import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import TaskCard from '../components/TaskCard.vue'
import TaskEditModal from '../components/TaskEditModal.vue'
import type { TaskDto } from '@/entities/task/types'

describe('TaskLifecycle (US3)', () => {
  const readyTask: TaskDto = {
    id: '01918a30-0000-7000-8000-000000000001',
    title: 'Tarefa Pronta',
    description: 'Notas',
    durationMinutes: 30,
    deadline: '2026-08-30T18:00:00Z',
    status: 'Ready',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    completedAt: null
  }

  it('renders Start and Complete actions on Ready task', async () => {
    const wrapper = mount(TaskCard, {
      props: { task: readyTask }
    })

    expect(wrapper.find('.btn-start').exists()).toBe(true)
    expect(wrapper.find('.btn-complete').exists()).toBe(true)

    await wrapper.find('.btn-start').trigger('click')
    expect(wrapper.emitted('start')![0]).toEqual([readyTask.id])

    await wrapper.find('.btn-complete').trigger('click')
    expect(wrapper.emitted('complete')![0]).toEqual([readyTask.id])
  })

  it('renders Complete action and no Start action on InProgress task', async () => {
    const inProgressTask: TaskDto = {
      ...readyTask,
      status: 'InProgress'
    }

    const wrapper = mount(TaskCard, {
      props: { task: inProgressTask }
    })

    expect(wrapper.find('.btn-start').exists()).toBe(false)
    expect(wrapper.find('.btn-complete').exists()).toBe(true)
  })

  it('emits save in TaskEditModal with updated fields', async () => {
    const wrapper = mount(TaskEditModal, {
      props: {
        task: readyTask,
        isOpen: true
      }
    })

    const titleInput = wrapper.find('input[placeholder*="Ex.: Criar"]')
    await titleInput.setValue('Título Alterado')

    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('save')).toBeTruthy()
    expect(wrapper.emitted('save')![0][0]).toBe(readyTask.id)
    expect((wrapper.emitted('save')![0][1] as any).title).toBe('Título Alterado')
  })
})
