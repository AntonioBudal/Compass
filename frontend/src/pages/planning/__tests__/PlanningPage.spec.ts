import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import PlanningPage from '../PlanningPage.vue'

// Mock router
vi.mock('vue-router', async (importOriginal) => {
  const actual = (await importOriginal()) as any
  return {
    ...actual,
    useRouter: () => ({
      push: vi.fn()
    }),
    useRoute: () => ({
      path: '/planning'
    })
  }
})

// Mock profile query
vi.mock('@/entities/schedule-profile/model/useScheduleProfileQuery', () => ({
  useScheduleProfileQuery: () => ({
    data: ref({ id: '123', timeZoneId: 'America/Sao_Paulo' })
  })
}))

// Mock task queries and mutations
const mockTasks = ref([
  {
    id: '01918a30-0000-7000-8000-000000000001',
    title: 'Tarefa Draft Teste',
    description: null,
    durationMinutes: null,
    deadline: null,
    status: 'Draft',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    completedAt: null
  },
  {
    id: '01918a30-0000-7000-8000-000000000002',
    title: 'Tarefa Ready Teste',
    description: null,
    durationMinutes: 30,
    deadline: null,
    status: 'Ready',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    completedAt: null
  }
])

vi.mock('@/entities/task/model/useTasksQuery', () => ({
  useTasksQuery: () => ({
    data: mockTasks,
    isLoading: ref(false),
    error: ref(null)
  })
}))

vi.mock('@/entities/task/model/useCreateTaskMutation', () => ({
  useCreateTaskMutation: () => ({
    mutateAsync: vi.fn(),
    isPending: ref(false)
  })
}))

vi.mock('@/entities/task/model/useUpdateTaskMutation', () => ({
  useUpdateTaskMutation: () => ({
    mutateAsync: vi.fn(),
    isPending: ref(false)
  })
}))

vi.mock('@/entities/task/model/useStartTaskMutation', () => ({
  useStartTaskMutation: () => ({
    mutateAsync: vi.fn(),
    isPending: ref(false)
  })
}))

vi.mock('@/entities/task/model/useCompleteTaskMutation', () => ({
  useCompleteTaskMutation: () => ({
    mutateAsync: vi.fn(),
    isPending: ref(false)
  })
}))

describe('PlanningPage.vue', () => {
  it('renders page heading, quick capture, tabs and tasks list', () => {
    const wrapper = mount(PlanningPage, {
      global: {
        stubs: {
          RouterLink: { template: '<a><slot /></a>' }
        }
      }
    })

    expect(wrapper.text()).toContain('Planning Inbox')
    expect(wrapper.find('input[placeholder*="Capturar nova tarefa"]').exists()).toBe(true)
    expect(wrapper.findAll('.task-card').length).toBe(2)
  })

  it('filters tasks when tab is clicked', async () => {
    const wrapper = mount(PlanningPage, {
      global: {
        stubs: {
          RouterLink: { template: '<a><slot /></a>' }
        }
      }
    })

    const tabs = wrapper.findAll('button[role="tab"]')
    // Click Draft tab (index 1)
    await tabs[1].trigger('click')

    expect(wrapper.findAll('.task-card').length).toBe(1)
    expect(wrapper.text()).toContain('Tarefa Draft Teste')
  })
})
