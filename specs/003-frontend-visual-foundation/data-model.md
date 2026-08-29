# Data Model & Design Tokens: Fundação Visual Consistente

**Feature**: `003-frontend-visual-foundation` | **Date**: 2026-08-28

## 1. Estrutura de Design Tokens (`tokens.css`)

### Neutrals (Light Default & Dark Prepared)

| Token Semântico | Light (Default) | Dark (Prepared) | Finalidade / Aplicação |
|---|---|---|---|
| `--color-bg-app` | `#ffffff` | `#0d1117` | Fundo geral da aplicação e body |
| `--color-surface` | `#ffffff` | `#161b22` | Superfície principal de containers e modais |
| `--color-surface-subtle` | `#f6f8fa` | `#21262d` | Fundo de cabeçalhos, barras e seções secundárias |
| `--color-surface-hover` | `#f3f4f6` | `#30363d` | Estado `:hover` em itens de lista e botões secundários |
| `--color-border` | `#d0d7de` | `#30363d` | Divisores de seção, bordas de inputs e cards |
| `--color-border-subtle` | `#e1e4e8` | `#21262d` | Linhas divisórias muito suaves |
| `--color-text-primary` | `#1f2328` | `#f0f6fc` | Títulos, textos principais e valores de formulário |
| `--color-text-secondary` | `#656d76` | `#8b949e` | Rótulos, descrições secundárias e cabeçalhos de tabela |
| `--color-text-muted` | `#8c959f` | `#6e7681` | Placeholders, metadados e textos desabilitados |

### Semantics (Functional & State Accents)

| Token Semântico | Valor Light | Valor Dark | Finalidade / Aplicação |
|---|---|---|---|
| `--color-accent` | `#0969da` | `#2f81f7` | Links ativos, borda de seleção e botões primários |
| `--color-accent-subtle` | `#ddf4ff` | `#0c2d6b` | Fundo de destaque suave (badges, abas ativas) |
| `--color-success` | `#1a7f37` | `#238636` | Confirmações, status concluído (`Done`) |
| `--color-success-subtle` | `#dafbe1` | `#173d1f` | Fundo suave de badge/alerta de sucesso |
| `--color-warning` | `#9a6700` | `#d29922` | Alertas de prazo, status em andamento (`InProgress`) |
| `--color-warning-subtle` | `#fff8c5` | `#3d2e00` | Fundo suave de badge de atenção/alerta |
| `--color-danger` | `#cf222e` | `#f85149` | Erros de validação, ações destrutivas |
| `--color-danger-subtle` | `#ffebe9` | `#490202` | Fundo suave de banner de erro |
| `--color-focus-ring` | `0 0 0 2px #ffffff, 0 0 0 4px #0969da` | `0 0 0 2px #0d1117, 0 0 0 4px #2f81f7` | Indicador de foco por teclado (`:focus-visible`) |

---

## 2. Tipografia e Espaçamento

### Escala Tipográfica (System Font Stack)
- **Família**: `-apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif`
- **Tamanhos**:
  - `--font-size-xs`: `0.75rem` (12px) - badges e metadados
  - `--font-size-sm`: `0.875rem` (14px) - corpo de texto padrão, inputs, botões
  - `--font-size-base`: `1rem` (16px) - parágrafos destacados e subtítulos
  - `--font-size-lg`: `1.125rem` (18px) - títulos de seção
  - `--font-size-xl`: `1.25rem` (20px) - títulos de página
  - `--font-size-2xl`: `1.5rem` (24px) - cabeçalhos principais
- **Pesos**:
  - `--font-weight-regular`: `400`
  - `--font-weight-medium`: `500`
  - `--font-weight-semibold`: `600`

### Escala de Espaçamento e Dimensões
- `--space-1`: `4px`
- `--space-2`: `8px`
- `--space-3`: `12px`
- `--space-4`: `16px`
- `--space-5`: `20px`
- `--space-6`: `24px`
- `--space-8`: `32px`
- `--space-10`: `40px`
- `--space-12`: `48px`
- `--radius-sm`: `4px`
- `--radius-md`: `6px`
- `--radius-full`: `9999px`

---

## 3. Catálogo de Componentes do Design System (`shared/ui`)

1. **`AppButton.vue`**:
   - Variantes: `primary` (fundo accent escuro/neutro, texto branco), `secondary` (fundo sutil, borda fina, texto primário), `danger` (fundo sutil vermelho, texto danger), `ghost` (sem borda/fundo até hover).
   - Tamanhos: `sm`, `md`.
   - Estados: `:hover`, `:focus-visible`, `:active`, `disabled`, `loading` (spinner inline SVG).
2. **`AppInput.vue`**:
   - Variantes: com/sem label, com/sem mensagem de erro.
   - Estados: normal, foco com anel semântico, erro com borda `--color-danger`, desabilitado.
3. **`AppSelect.vue`**:
   - Estilização de dropdown nativo com seta SVG customizada e foco de alto contraste.
4. **`TimeRangePicker.vue`**:
   - Seleção inline de horários (início e fim) com validação visual e botão de remoção limpo.
5. **`EmptyState.vue`**:
   - Container centralizado com ícone utilitário sutil, título objetivo, mensagem de orientação e slot para ação (sem emojis).
6. **`AppModal.vue`**:
   - Modal com backdrop neutro sutil (sem glassmorphism/blur pesado), container com borda fina de 1px, cabeçalho com título, botão de fechar e suporte a Escape/focus-trap.
7. **`AppBadge.vue`**:
   - Etiqueta de status compacta com cores semânticas suaves (`default`, `accent`, `success`, `warning`, `danger`).
8. **`AppShell.vue` / `AppHeader.vue`**:
   - Barra superior sóbria com logotipo tipográfico do Compass, links de navegação (`Hoje`, `Planning`), divisores finos e área de status.
