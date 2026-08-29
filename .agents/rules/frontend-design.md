---
description: "Regras obrigatórias de design de interface, acessibilidade, tipografia e tokenização para o frontend do Compass V2."
globs:
  - "frontend/src/**/*.vue"
  - "frontend/src/**/*.css"
  - "frontend/src/**/*.ts"
---

# Compass Frontend Design & UI Governance

@/.specify/memory/constitution.md

Ao criar, editar ou inspecionar qualquer arquivo de frontend (`.vue`, `.css`, `.ts`), as seguintes regras de design e interface são **estritas e não-negociáveis**:

---

## 1. Referência Constitucional e Filosofia Visual
- A interface do Compass MUST ser **sóbria, utilitária, altamente legível e orientada à informação** (Princípios Constitucionais 26 a 45).
- **GitHub** (Prime Web / GitHub Web) é a referência oficial para:
  - Estrutura de layout e navegação;
  - Densidade de dados, listas e tabelas;
  - Bordas sutis, divisores e controles interativos (botões, inputs, selects, tabs).
- **Notion** é a referência oficial para:
  - Respiro e ritmo vertical de espaçamento;
  - Legibilidade do fluxo de texto e hierarquia tipográfica;
  - Redução sistemática de ruído visual.
- As referências de GitHub e Notion servem como **direção visual e ergonômica**; não devem ser copiadas literalmente.

---

## 2. Proibição Absoluta de Emojis e Ícones Inconsistentes
- **Emojis são terminantemente proibidos** em títulos, cabeçalhos, botões, mensagens, empty states, navegação, badges, notificações e qualquer conteúdo estrutural da interface.
- Ícones na interface MUST pertencer a um **único conjunto visual consistente** (mesmo peso de traço, proporção e estilo) ou MUST ser substituídos por texto explicativo claro.

---

## 3. Paleta Neutra Controlada e Proibição de Estética de IA
- A paleta principal da aplicação MUST ser construída exclusivamente em **branco/preto e uma escala controlada de quatro a cinco tons neutros** (cinzas neutros / slate) para fundo, superfícies, bordas e textos.
- **Proibição de Estética de IA Genérica**: É expressamente proibido o uso de:
  - Gradientes decorativos ou multicoloridos;
  - Efeitos de *glow*, luzes neon ou reflexos;
  - *Glassmorphism* (fundos translúcidos excessivos);
  - Sombras decorativas pesadas ou volumosas;
  - Efeitos de cards "flutuantes".
- É proibido usar **esmeralda, roxo ou azul saturado/elétrico** como destaque decorativo padrão ou cor de fundo genérica.
- **Cores adicionais** (sucesso, erro, alerta, info) MUST possuir **significado funcional estrito e explícito** e nunca podem ser o único meio de transmitir uma informação (exigindo suporte por texto legível ou atributo ARIA).

---

## 4. Estrutura de Layout: Divisores antes de Cards
- A estrutura das páginas MUST priorizar **divisores sutis, alinhamento rigoroso, alternância de superfícies e espaçamento** antes da criação de containers em formato de card.
- **Cards** só devem existir quando representarem uma **unidade de informação realmente independente, atômica e destacável** (ex.: card individual de tarefa em lista/grid). Evitar o excesso de bordas arredondadas e encaixotamentos desnecessários ("carditis").

---

## 5. Tokenização CSS Semântica e Preparação de Temas
- **Cores hardcoded são proibidas** dentro de componentes `.vue` ou classes CSS locais.
- Toda cor, espaçamento, raio de borda, tipografia e sombra MUST consumir exclusivamente **tokens CSS semânticos** definidos centralmente (`tokens.css`).
- Os tokens semânticos MUST estruturar e preparar a aplicação para os modos **Claro (Light)**, **Escuro (Dark)** e **Preferência do Sistema (System)**.
- **Não implementar o seletor de temas**: Esta fundação prepara as variáveis CSS mas **não deve implementar antecipadamente** o seletor visual de temas em tempo de execução enquanto ele não for formalmente priorizado em uma feature dedicada.

---

## 6. Estados de Interação, Acessibilidade e Responsividade
- **Consistência Sistemática de Estados**: Todos os controles interativos (botões, links, inputs, abas, itens clicáveis) MUST implementar estilos claros e consistentes para:
  - `normal`, `:hover`, `:focus-visible`, `:active`, `:disabled`, `loading`, `error` e `empty`.
- **Acessibilidade Obrigatória (WCAG AA)**:
  - Navegação completa por teclado (Tab, Shift+Tab, Enter, Espaço, Escape);
  - Anéis de foco visíveis (`focus-visible`) em todos os elementos focáveis;
  - Contraste mínimo de cores garantido para textos e controles essenciais;
  - HTML semântico e atributos ARIA corretos (`role="tab"`, `role="status"`, `aria-label`, etc.).
- **Responsividade Adaptativa**:
  - Todos os componentes e layouts MUST permanecer funcionais e confortavelmente legíveis em **desktop (1280px+)** e **mobile (a partir de 320px)**.
- **Reutilização Obrigatória**: Novas features e telas MUST reutilizar prioritariamente os componentes de `frontend/src/shared/ui/` e os tokens existentes antes de criar novos padrões.
