# Compass Frontend Design System

Este documento estabelece as diretrizes de design, tokens semânticos, componentes compartilhados e boas práticas de acessibilidade e responsividade da interface web do **Compass**.

---

## 1. Princípios Norteadores de Design

1. **Sóbrio, Funcional e Orientado à Informação**: A interface prioriza clareza cognitiva, leitura sem distrações e rapidez operacional.
2. **GitHub como Referência Estrutural**:
   - Organização espacial em cabeçalho superior unificado (`AppHeader`) com 56px de altura e borda inferior de 1px.
   - Navegação por abas com indicador ativo e contadores em pílulas (`TaskFilterTabs`).
   - Densidade de dados compacta, com formulários alinhados e botões de ação contextuais.
3. **Notion como Referência Tipográfica e Espaçamento**:
   - Ritmo vertical fluido com separadores sutis de 1px (`<hr class="section-divider" />`).
   - Títulos hierárquicos discretos com peso semibold/bold e entrelinhamento legível.
   - Superfícies claras e ausência de ruído decorativo.
4. **Tolerância Zero a Emojis na Interface**:
   - Proibido o uso de emojis em títulos, botões, abas, badges, tooltips, modais, mensagens de erro e empty states.
   - Substituição obrigatória por texto descritivo claro ou ícones SVG monocromáticos (`currentColor`).
5. **Paleta Neutra Controlada**:
   - Fundo base neutro (`--color-bg-app: #ffffff`), superfícies (`--color-surface: #ffffff`), superfícies sutis (`--color-surface-subtle: #f6f8fa`) e bordas neutras (`--color-border: #d0d7de`).
   - Proibido o uso de gradientes decorativos, sombras flutuantes pesadas, glow, glassmorphism e cores típicas de IA generativa.
6. **Cores Funcionais Estritas**:
   - O azul (`--color-accent: #0969da`) é reservado para ações principais, links e foco.
   - Verde (`--color-success: #1a7f37`), amarelo/âmbar (`--color-warning: #9a6700`) e vermelho (`--color-danger: #cf222e`) são usados exclusivamente para status de domínio e feedback.

---

## 2. Tabela de Tokens CSS Semânticos (`tokens.css`)

Todos os componentes consomem variáveis CSS definidas em `src/app/styles/tokens.css`.

### 2.1 Cores e Superfícies (Tema Claro Padrão)

| Variável CSS | Valor Claro | Descrição / Uso |
|---|---|---|
| `--color-bg-app` | `#ffffff` | Fundo principal da página |
| `--color-surface` | `#ffffff` | Fundo de cartões, formulários e dropdowns |
| `--color-surface-subtle` | `#f6f8fa` | Fundo secundário sutil, pílulas e hover |
| `--color-surface-hover` | `#f3f4f6` | Estado de hover em itens e botões secundários |
| `--color-surface-active` | `#eaedf1` | Estado ativo/pressionado |
| `--color-border` | `#d0d7de` | Bordas principais de 1px em inputs, cards e divisores |
| `--color-border-subtle` | `#e1e4e8` | Bordas secundárias e divisões internas |
| `--color-text-primary` | `#1f2328` | Texto principal com alto contraste (WCAG AAA) |
| `--color-text-secondary` | `#656d76` | Texto secundário, descrições e labels |
| `--color-text-muted` | `#8c959f` | Placeholders e textos desativados |
| `--color-accent` | `#0969da` | Cor primária para botões principais e foco |
| `--color-accent-hover` | `#085cc0` | Hover do botão primário |
| `--color-accent-subtle` | `#ddf4ff` | Fundo de destaque suave para badges |
| `--color-accent-text` | `#ffffff` | Texto em superfícies accent |
| `--color-success` | `#1a7f37` | Status concluído / validação positiva |
| `--color-success-subtle`| `#dafbe1` | Fundo de badge de sucesso |
| `--color-warning` | `#9a6700` | Status em andamento / alerta |
| `--color-warning-subtle`| `#fff8c5` | Fundo de badge de aviso |
| `--color-danger` | `#cf222e` | Status de erro / perigo / prazo expirado |
| `--color-danger-subtle` | `#ffebe9` | Fundo de alertas e badges de erro |

### 2.2 Foco e Sombras

| Variável CSS | Valor | Uso |
|---|---|---|
| `--focus-ring` | `0 0 0 2px #ffffff, 0 0 0 4px #0969da` | Anel de foco para `:focus-visible` (WCAG AA) |
| `--shadow-subtle` | `0 1px 2px rgba(0, 0, 0, 0.05)` | Elevação mínima |
| `--shadow-modal` | `0 8px 24px rgba(140, 149, 159, 0.2)` | Sombra sutil de modais |

### 2.3 Tipografia, Espaçamento e Raio

| Escala | Tokens | Valores |
|---|---|---|
| **Fonte** | `--font-family-sans` | `-apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif` |
| **Tamanhos** | `--font-size-xs`, `--font-size-sm`, `--font-size-base`, `--font-size-lg`, `--font-size-xl`, `--font-size-2xl` | `0.75rem` (12px), `0.875rem` (14px), `1rem` (16px), `1.125rem` (18px), `1.25rem` (20px), `1.5rem` (24px) |
| **Espaçamento** | `--space-1` até `--space-12` | `4px`, `8px`, `12px`, `16px`, `20px`, `24px`, `32px`, `40px`, `48px` |
| **Raio de Borda**| `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-full` | `4px`, `6px`, `8px`, `9999px` |

---

## 3. Preparação para Temas Escuro e Sistema

Os tokens suportam troca de tema automática via atributos ou media query:
1. **Light (Padrão)**: Definido no seletor `:root`.
2. **Dark**: Definido em `[data-theme="dark"]` e `@media (prefers-color-scheme: dark)` utilizando os tons GitHub Dark (`#0d1117`, `#161b22`, `#21262d`, `#30363d`, `#f0f6fc`).
3. **Sem seletor em tempo de execução**: O switcher manual de temas não foi ativado para evitar complexidade prematura no MVP.

---

## 4. Catálogo de Componentes Base (`shared/ui`)

### 4.1 `AppShell.vue`
- Estrutura base de página com `AppHeader` embutido e container centralizado (`max-width: 1200px`).
- Slots: `default` (conteúdo da página) e `header-actions` (ações à direita no cabeçalho).

### 4.2 `AppHeader.vue`
- Barra superior fixa de 56px.
- Exibe o logotipo textual `Compass` e os links de navegação (`Hoje` e `Planning`).

### 4.3 `AppButton.vue`
- Variantes: `primary`, `secondary`, `outline`, `ghost`, `danger`.
- Tamanhos: `sm` (compacto) e `md` (padrão).
- Suporta `loading` com indicador SVG animado em `currentColor`.

### 4.4 `AppInput.vue` & `AppSelect.vue`
- Controles de formulário com rótulo (`label`), indicador de obrigatoriedade (`required`), texto de ajuda (`hint`) e mensagens de erro acessíveis (`role="alert"`).
- O `AppSelect` possui chevron SVG customizado em substituição ao estilo nativo do SO.

### 4.5 `AppBadge.vue`
- Pílula compacta de status.
- Variantes: `default` (neutro), `accent` (azul), `success` (verde), `warning` (âmbar), `danger` (vermelho).

### 4.6 `AppModal.vue`
- Diálogo modal acessível com `role="dialog"`, `aria-modal="true"`, foco automático inicial, fechamento via tecla `Escape` e bloqueio do scroll do body.

### 4.7 `EmptyState.vue`
- Exibição de listas vazias com ícone sutil, título em destaque, descrição explicativa e slot opcional para ações primárias.

---

## 5. Diretrizes de Ícones e Acessibilidade

- **Ícones**: Devem ser sempre SVGs inline com `stroke-width="1.5"` ou `"2"`, `stroke="currentColor"`, `fill="none"` e `aria-hidden="true"`.
- **Navegação por Teclado**: Todos os elementos interativos possuem `:focus-visible` com anel duplo de 2px + 4px (`--focus-ring`).
- **Contraste**: Todos os textos atendem ao critério de contraste mínimo de 4.5:1 (WCAG AA) para texto normal e 3:1 para texto grande.
- **Responsividade (320px+)**: Todas as telas se adaptam fluidamente a partir de 320px de largura de tela sem overflow horizontal acidental.
