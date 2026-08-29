# Feature Specification: Fundação Visual Consistente e Migração de Interface

**Feature Branch**: `003-frontend-visual-foundation`

**Created**: 2026-08-28

**Status**: Draft

**Input**: User description: "Criar a fundação visual consistente do frontend do Compass e migrar todas as telas existentes para ela. As telas atuais de Onboarding, Hoje e Planning foram construídas em features diferentes e precisam compartilhar a mesma linguagem visual. A experiência deve combinar: a organização, densidade e clareza estrutural do GitHub; o espaçamento, legibilidade e simplicidade do Notion; uma identidade própria do Compass baseada em branco e tons neutros. A feature deve: 1. auditar todas as páginas e componentes frontend existentes; 2. identificar cores, espaçamentos, tipografia, bordas e estados inconsistentes; 3. criar uma escala compartilhada de cores neutras; 4. criar tokens semânticos para superfícies, textos, bordas, controles, ações e foco; 5. criar escalas consistentes de espaçamento, tipografia, radius e transições; 6. preparar os tokens para temas Claro, Escuro e Sistema; 7. manter inicialmente o tema visual aprovado como padrão; 8. remover cores hardcoded dos componentes; 9. remover emojis da interface; 10. substituir emojis por texto ou ícones de um único conjunto consistente; 11. eliminar gradientes, glow, glassmorphism, sombras excessivas e cores decorativas; 12. reduzir o excesso de cards; 13. usar divisores e superfícies para separar setores; 14. uniformizar botões, inputs, selects, modais, abas, banners e empty states; 15. criar um App Shell consistente para as rotas existentes; 16. padronizar loading, erro, vazio, disabled, hover, active e focus; 17. corrigir a responsividade das telas existentes; 18. garantir navegação integral por teclado; 19. garantir foco visível e contraste adequado; 20. documentar a fundação visual para as próximas features. Telas obrigatoriamente migradas: /onboarding; /today; /planning; página de rota não encontrada; navegação principal; modais e formulários já existentes. Documentação obrigatória: Criar docs/design/FRONTEND_DESIGN_SYSTEM.md."

## Clarifications

### Session 2026-08-28
- Q: Qual estratégia de conjunto de ícones consistente deve ser adotada para substituir os emojis na interface? → A: Componentes SVG inline utilitários minimalistas baseados em traço (stroke 1.5px/2px estilo Feather/Lucide/GitHub Icons), mantendo o bundle leve, com zero dependências externas e controle total via tokens CSS de cor.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Tokens Semânticos Centrais e App Shell Unificado (Priority: P1) 🎯 MVP

Como um usuário navegando pelo Compass, quero uma estrutura de página coesa, estável e previsível entre todas as telas da aplicação, para que a transição entre Hoje, Planning e Onboarding seja natural e livre de choques visuais ou desalinhamentos.

**Why this priority**: É a fundação arquitetural de todo o frontend. Sem um sistema centralizado de tokens semânticos e um App Shell compartilhado, qualquer melhoria visual pontual em telas individuais continuará gerando inconsistências e duplicações.

**Independent Test**: Navegar entre `/today` e `/planning` e constatar que o cabeçalho, navegação, tipografia, divisores, superfícies e largura máxima do layout compartilham exatamente os mesmos tokens e o mesmo componente App Shell, mantendo estabilidade visual ao recarregar a página (F5).

**Acceptance Scenarios**:

1. **Given** a aplicação executando em modo padrão, **When** qualquer página é renderizada, **Then** as cores de fundo, texto, bordas e controles são derivadas exclusivamente dos tokens CSS semânticos em `tokens.css`.
2. **Given** que o usuário navega entre as rotas principais (`/today`, `/planning`), **When** o App Shell é renderizado, **Then** o cabeçalho de navegação exibe uma identidade visual sóbria com links ativos/inativos consistentes, sem emojis e com divisores finos.
3. **Given** a folha de estilos central `tokens.css`, **When** inspecionada, **Then** ela possui estrutura declarativa preparada para os modos Claro, Escuro e Sistema através de variáveis semânticas, sem que componentes locais dependam de classes ou seletores de tema hardcoded.

---

### User Story 2 - Padronização dos Componentes Base e Eliminação de Emojis (Priority: P1)

Como um usuário interagindo com botões, campos de texto, seletores, abas e modais, quero que todos os controles tenham feedback tátil e visual padronizado, alto contraste, foco visível e ausência de emojis decorativos, para obter uma experiência profissional e acessível.

**Why this priority**: Os componentes atômicos em `shared/ui` são reutilizados em todas as telas. Garantir que estejam 100% aderentes aos princípios constitucionais de design resolve problemas visuais de forma sistêmica.

**Independent Test**: Inspecionar os componentes `AppButton`, `AppInput`, `AppSelect`, `TimeRangePicker`, `EmptyState` e `TaskFilterTabs` e verificar a ausência total de emojis, ausência de cores hardcoded, presença de anéis de foco visíveis (`focus-visible`) e suporte total à navegação por teclado.

**Acceptance Scenarios**:

1. **Given** qualquer componente de interface, **When** renderizado na tela, **Then** nenhum emoji é exibido em títulos, botões, badges, abas ou estados vazios, sendo substituídos por texto explicativo claro ou ícones consistentes de mesmo traço.
2. **Given** qualquer elemento interativo (botão, input, link, aba), **When** recebe foco via teclado (Tab), **Then** um indicador de foco (`focus-visible`) de alto contraste e bem demarcado é exibido.
3. **Given** qualquer controle interativo, **When** passa pelos estados `:hover`, `:active`, `:disabled` ou `loading`, **Then** o feedback visual segue rigorosamente a escala de tons neutros e tokens de estado da fundação.

---

### User Story 3 - Migração Completa das Telas Existentes e Página 404 (Priority: P2)

Como um usuário utilizando as funcionalidades do Compass, quero que as telas de Onboarding (`/onboarding`), Hoje (`/today`), Planning (`/planning`) e a página de erro 404 utilizem a nova linguagem visual sóbria baseada em superfícies e divisores, para desfrutar de uma interface limpa, sem excesso de cards flutuantes ou gradientes artificiais.

**Why this priority**: Entrega a modernização visual real em todas as superfícies de contato com o usuário construídas até o momento, eliminando débitos de design de features anteriores.

**Independent Test**: Percorrer o fluxo completo do Onboarding, acessar o painel Hoje, gerenciar tarefas na Planning Inbox e acessar uma rota inexistente (`/rota-invalida`), confirmando que 100% das telas compartilham a mesma tipografia, paleta neutra e ergonomia.

**Acceptance Scenarios**:

1. **Given** a tela de Onboarding (`/onboarding`), **When** o usuário avança pelas etapas do assistente, **Then** as seções utilizam superfícies neutras limpas, sem emojis no cabeçalho ou botões e com feedback de formulário padronizado.
2. **Given** o painel Hoje (`/today`), **When** o usuário visualiza sua disponibilidade e fuso horário, **Then** as informações são organizadas com divisores sutis e hierarquia tipográfica inspirada no Notion, sem múltiplos cards sobrepostos ou cores saturadas.
3. **Given** a Planning Inbox (`/planning`), **When** o usuário interage com captura rápida, abas e tarefas, **Then** os cards de tarefas possuem bordas finas neutras, badges semânticos discretos e o modal de edição consome os componentes padronizados.
4. **Given** o usuário navegando para uma URL inexistente, **When** a rota é resolvida, **Then** a aplicação renderiza uma página `NotFoundPage.vue` sóbria e integrada ao App Shell, com opção clara para retornar à tela principal.

---

### User Story 4 - Responsividade Mobile e Documentação do Design System (Priority: P2)

Como um desenvolvedor ou usuário móvel do Compass, quero que todas as páginas sejam responsivas a partir de 320px de largura e que todas as diretrizes de design estejam formalmente documentadas em `docs/design/FRONTEND_DESIGN_SYSTEM.md`, para garantir manutenibilidade e consistência nas próximas features.

**Why this priority**: Garante que o Compass seja plenamente operável em dispositivos móveis e cria o guia definitivo para os futuros desenvolvimentos do produto.

**Independent Test**: Redimensionar a viewport do navegador para 320px e 375px nas telas `/onboarding`, `/today` e `/planning`, constatando ausência de quebras de layout, rolagem horizontal indesejada ou textos truncados; verificar o arquivo `docs/design/FRONTEND_DESIGN_SYSTEM.md`.

**Acceptance Scenarios**:

1. **Given** uma tela com largura de 320px a 480px (mobile), **When** qualquer rota existente é acessada, **Then** o layout se reorganiza verticalmente com padding adequado, formulários confortáveis para toque e sem scroll horizontal.
2. **Given** o arquivo `docs/design/FRONTEND_DESIGN_SYSTEM.md`, **When** consultado por desenvolvedores, **Then** ele descreve detalhadamente os princípios de design, paleta de tokens neutros e semânticos, tipografia, espaçamento, padrões de componentes, estados e regras de não-utilização de emojis/gradientes.

---

### Edge Cases

- **Telas com densidade extrema de dados**: Tabelas e listas em telas móveis devem colapsar ou empilhar informações mantendo legibilidade sem comprimir fontes abaixo de 12px.
- **Campos desabilitados ou em estado de carregamento**: Devem manter legibilidade de texto suficiente sem parecerem quebrados ou com contraste excessivamente baixo.
- **Navegação com leitor de tela e teclado**: Modais devem prender o foco (focus trap) e liberar com tecla Escape; abas de filtro devem possuir semântica `role="tablist"` e `role="tab"`.
- **Recarregamento de página (F5)**: Nenhuma oscilação ou flash de estilo incorreto (FOUC) ocorre durante o carregamento de CSS e fontes de sistema.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST centralizar todos os estilos e variáveis de design no arquivo `frontend/src/app/styles/tokens.css`.
- **FR-002**: O sistema MUST definir uma paleta de cores primária baseada em branco/preto e uma escala controlada de quatro a cinco tons neutros (cinzas neutros/ardósia: background, surface, surface-subtle, border, border-subtle, text-primary, text-secondary, text-muted).
- **FR-003**: O sistema MUST definir tokens semânticos para estados e ações funcionais (success, error, warning, info, focus-ring), proibindo cores hardcoded nos componentes.
- **FR-004**: O sistema MUST estruturar os tokens semânticos de modo a viabilizar os modos Claro, Escuro e Sistema via atributos no elemento raiz (`:root`, `[data-theme="dark"]`), mantendo o tema padrão sóbrio ativo.
- **FR-005**: Esta feature MUST NOT implementar o seletor visual de temas em tempo de execução (switcher) nem depender dele.
- **FR-006**: Todos os componentes e páginas MUST NOT conter emojis em títulos, botões, mensagens, abas, badges, empty states ou notificações.
- **FR-007**: Todo ícone na interface MUST ser substituído por texto claro ou por ícones SVG pertencentes a um único conjunto visual consistente.
- **FR-008**: O sistema MUST eliminar gradientes decorativos, efeitos neon/glow, glassmorphism, sombras volumosas e cards flutuantes de todo o frontend.
- **FR-009**: A interface MUST priorizar o uso de divisores sutis, superfícies diferenciadas e ritmo de espaçamento vertical/horizontal antes de criar containers em formato de card.
- **FR-010**: O sistema MUST implementar um componente compartilhado de layout `AppShell` (ou `AppHeader` e `AppNavigation`) reutilizado nas rotas `/today`, `/planning` e rotas de erro.
- **FR-011**: O sistema MUST padronizar os componentes base (`AppButton`, `AppInput`, `AppSelect`, `TimeRangePicker`, `EmptyState`, `TaskFilterTabs`, modais) para garantir estados consistentes (`hover`, `focus-visible`, `active`, `disabled`, `loading`, `error`, `empty`).
- **FR-012**: O sistema MUST migrar integralmente as páginas `/onboarding`, `/today`, `/planning` e criar a página `NotFoundPage.vue` para rotas não mapeadas.
- **FR-013**: Todos os elementos interativos MUST possuir anel de foco visível (`focus-visible`) e atender aos critérios de contraste WCAG AA.
- **FR-014**: Todos os layouts MUST ser totalmente operáveis e responsivos em resoluções desktop (1280px+) e mobile (a partir de 320px).
- **FR-015**: O sistema MUST criar a documentação oficial da fundação visual em `docs/design/FRONTEND_DESIGN_SYSTEM.md`.
- **FR-016**: Esta feature MUST NOT alterar contratos de backend, endpoints HTTP, migrações de banco de dados ou regras de negócio existentes.

---

### Key Entities / Design System Assets *(include if feature involves data)*

- **CSS Semantic Tokens (`tokens.css`)**:
  - *Neutrals*: `--color-bg-app`, `--color-surface`, `--color-surface-subtle`, `--color-surface-hover`, `--color-border`, `--color-border-subtle`, `--color-text-primary`, `--color-text-secondary`, `--color-text-muted`.
  - *Semantics*: `--color-accent`, `--color-success`, `--color-warning`, `--color-danger`, `--color-focus-ring`.
  - *Typography*: `--font-sans` (System UI font stack: `-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif`), `--font-size-xs` a `--font-size-2xl`, `--font-weight-regular`, `--font-weight-medium`, `--font-weight-semibold`, `--font-weight-bold`.
  - *Spacing*: `--space-1` (4px) a `--space-12` (48px).
  - *Radius*: `--radius-sm` (4px), `--radius-md` (6px), `--radius-lg` (8px), `--radius-full` (9999px).
  - *Borders & Dividers*: `--border-width-thin` (1px), `--border-color`.
- **Componentes do Design System (`shared/ui`)**:
  - `AppButton.vue`, `AppInput.vue`, `AppSelect.vue`, `TimeRangePicker.vue`, `EmptyState.vue`, `AppModal.vue`, `AppBadge.vue`, `AppHeader.vue`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos emojis removidos de todas as telas, botões, títulos e mensagens da aplicação (contagem de emojis na UI = 0).
- **SC-002**: 100% dos componentes de frontend consom exclusivamente variáveis semânticas de `tokens.css`, com zero cores literais hardcoded em arquivos `.vue`.
- **SC-003**: 100% dos testes unitários e de integração existentes no backend (62 testes) e frontend (27 testes) continuam passando sem regressões.
- **SC-004**: 100% dos fluxos de Onboarding, Hoje, Planning e navegação principal são operáveis exclusivamente via teclado com foco visível contínuo.
- **SC-005**: 100% das telas renderizam sem rolagem horizontal ou sobreposição em viewports móveis de 320px de largura.
- **SC-006**: Documento `docs/design/FRONTEND_DESIGN_SYSTEM.md` criado e validado como guia canônico para próximas features.

---

## Assumptions

- O design system utilizará fontes de sistema limpas nativas (`system-ui`, `-apple-system`, `Segoe UI`, `Roboto`), eliminando requisições a fontes externas na web para garantir privacidade, leveza e renderização instantânea.
- O tema padrão adotado nesta etapa é a paleta neutra e sóbria ratificada na Constituição v1.1.0, com variáveis preparadas para Dark Mode no futuro.
- A migração visual preserva rigorosamente todos os identificadores de teste (`aria-label`, inputs, roles e eventos) para assegurar integridade dos testes automatizados.
