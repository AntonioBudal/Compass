# Interface Contracts: Design Tokens e Componentes Base

**Feature**: `003-frontend-visual-foundation` | **Date**: 2026-08-28

## 1. Contrato de Variáveis CSS Semânticas

Todo arquivo `.vue` ou `.css` no projeto deve respeitar estritamente o seguinte contrato de tokens CSS exportados por `tokens.css`:

```css
:root {
  /* Surfaces & Canvas */
  --color-bg-app: #ffffff;
  --color-surface: #ffffff;
  --color-surface-subtle: #f6f8fa;
  --color-surface-hover: #f3f4f6;

  /* Borders & Dividers */
  --color-border: #d0d7de;
  --color-border-subtle: #e1e4e8;

  /* Typography Colors */
  --color-text-primary: #1f2328;
  --color-text-secondary: #656d76;
  --color-text-muted: #8c959f;

  /* Functional Accents */
  --color-accent: #0969da;
  --color-accent-subtle: #ddf4ff;
  --color-success: #1a7f37;
  --color-success-subtle: #dafbe1;
  --color-warning: #9a6700;
  --color-warning-subtle: #fff8c5;
  --color-danger: #cf222e;
  --color-danger-subtle: #ffebe9;

  /* Focus & Elevation */
  --focus-ring: 0 0 0 2px #ffffff, 0 0 0 4px #0969da;
  --shadow-subtle: 0 1px 2px rgba(0, 0, 0, 0.05);

  /* Typography Scales */
  --font-family-sans: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif;
  --font-size-xs: 0.75rem;
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  --font-size-lg: 1.125rem;
  --font-size-xl: 1.25rem;
  --font-size-2xl: 1.5rem;
  --font-weight-regular: 400;
  --font-weight-medium: 500;
  --font-weight-semibold: 600;

  /* Spacing Scale */
  --space-1: 0.25rem;
  --space-2: 0.5rem;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --space-5: 1.25rem;
  --space-6: 1.5rem;
  --space-8: 2rem;
  --space-10: 2.5rem;
  --space-12: 3rem;

  /* Radius Scale */
  --radius-sm: 4px;
  --radius-md: 6px;
  --radius-full: 9999px;
}
```

---

## 2. Contratos de Componentes Compartilhados (`shared/ui`)

### `AppButton.vue`
```typescript
interface AppButtonProps {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
  size?: 'sm' | 'md'
  type?: 'button' | 'submit' | 'reset'
  disabled?: boolean
  loading?: boolean
}
```

### `AppInput.vue`
```typescript
interface AppInputProps {
  modelValue: string | number
  label?: string
  id?: string
  placeholder?: string
  type?: string
  error?: string
  disabled?: boolean
  required?: boolean
}
```

### `AppBadge.vue`
```typescript
interface AppBadgeProps {
  variant?: 'default' | 'accent' | 'success' | 'warning' | 'danger'
  size?: 'sm' | 'md'
}
```

### `EmptyState.vue`
```typescript
interface EmptyStateProps {
  title: string
  description?: string
}
```

### `AppShell.vue`
```typescript
// Componente de casca da aplicação (Layout Root)
// Renderiza: <AppHeader /> + <main class="app-main"><slot /></main>
```
