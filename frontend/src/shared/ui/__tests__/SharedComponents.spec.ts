import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import AppButton from '../AppButton.vue'
import AppInput from '../AppInput.vue'
import AppSelect from '../AppSelect.vue'
import AppBadge from '../AppBadge.vue'
import EmptyState from '../EmptyState.vue'

describe('AppButton.vue', () => {
  it('renders correctly with slot content and primary variant', () => {
    const wrapper = mount(AppButton, {
      slots: { default: 'Salvar' }
    })
    expect(wrapper.text()).toBe('Salvar')
    expect(wrapper.classes()).toContain('app-btn--primary')
  })

  it('renders spinner and disables button when loading is true', () => {
    const wrapper = mount(AppButton, {
      props: { loading: true },
      slots: { default: 'Processando' }
    })
    expect(wrapper.find('.spinner').exists()).toBe(true)
    expect(wrapper.attributes('disabled')).toBeDefined()
  })
})

describe('AppInput.vue', () => {
  it('renders label, input and emits update:modelValue on input', async () => {
    const wrapper = mount(AppInput, {
      props: {
        modelValue: 'Texto inicial',
        label: 'Título'
      }
    })
    expect(wrapper.find('label').text()).toContain('Título')
    const input = wrapper.find('input')
    expect(input.element.value).toBe('Texto inicial')

    await input.setValue('Novo texto')
    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['Novo texto'])
  })

  it('renders error message and applies error class', () => {
    const wrapper = mount(AppInput, {
      props: {
        modelValue: '',
        error: 'Campo obrigatório'
      }
    })
    expect(wrapper.find('.app-input--error').exists()).toBe(true)
    expect(wrapper.find('.app-input-error').text()).toBe('Campo obrigatório')
  })
})

describe('AppSelect.vue', () => {
  it('renders options and emits update:modelValue on selection', async () => {
    const wrapper = mount(AppSelect, {
      props: {
        modelValue: 'America/Sao_Paulo',
        options: [
          { value: 'America/Sao_Paulo', label: 'Brasília (GMT-3)' },
          { value: 'UTC', label: 'UTC' }
        ]
      }
    })
    const select = wrapper.find('select')
    expect(select.element.value).toBe('America/Sao_Paulo')

    await select.setValue('UTC')
    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['UTC'])
  })
})

describe('AppBadge.vue', () => {
  it('renders with correct variant and text', () => {
    const wrapper = mount(AppBadge, {
      props: { variant: 'success' },
      slots: { default: 'Concluído' }
    })
    expect(wrapper.text()).toBe('Concluído')
    expect(wrapper.classes()).toContain('app-badge--success')
  })
})

describe('EmptyState.vue', () => {
  it('renders title and description without emojis', () => {
    const wrapper = mount(EmptyState, {
      props: {
        title: 'Nenhuma tarefa encontrada',
        description: 'Capture novas tarefas na barra acima.'
      }
    })
    expect(wrapper.text()).toContain('Nenhuma tarefa encontrada')
    expect(wrapper.text()).toContain('Capture novas tarefas na barra acima.')
    expect(wrapper.text()).not.toMatch(/[\u{1F300}-\u{1FAFF}]/u)
  })
})
