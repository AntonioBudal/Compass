# Developer Changelog

## 2026-07-16

### Arquitetura

* Estruturação da solução em `Compass.Domain`, `Compass.Application`, `Compass.Infrastructure` e `Compass.Api`.
* Configuração do Entity Framework Core com PostgreSQL.
* Implementação das estratégias de herança TPH e TPT.

### Banco de Dados

* Mapeamento de enums do PostgreSQL.
* Utilização de UUID como chave primária.
* Adição de CHECK Constraints e índices.
* Criação da migration `InitialProductionSchema`.

---

## 2026-07-17

### Backend

* Implementação do `ScoringEngine`.
* Implementação do `TimeWindowCalculator`.
* Modelagem de `CommitmentAttribute`.
* Modelagem de `DecisionSnapshot`.
* Implementação dos repositórios de Commitments, Projects e DecisionSnapshots.

### Banco de Dados

* Criação da tabela `decision_snapshots`.
* Configuração de relacionamentos e índices.
* Criação da migration `AddDecisionSnapshot`.

---

## 2026-07-18

### Backend

* Implementação dos DTOs.
* Configuração do FluentValidation.
* Implementação de `DecisionService`.
* Implementação de `CommitmentService`.

### API

* Criação dos controllers REST.
* Implementação do `GlobalExceptionHandler`.
* Suporte ao header `X-User-Id`.

---

## 2026-07-19

### Frontend

* Inicialização do projeto com Vue 3, Vite, TypeScript e Tailwind CSS.
* Configuração do Axios.
* Configuração do Pinia.
* Implementação do Design System.
* Implementação do App Shell.
* Implementação de atalhos globais de teclado.
* Suporte a `prefers-reduced-motion`.

### Stores

* `useCommitmentsStore`.
* `useDecisionStore`.

### Componentes

* `CommitmentCard`.
* `TopFocusCard`.
* `CommandBarModal`.
* `QuickCaptureModal`.

### Views

* `NowEngineView`.
* `AgendaView`.
* `ProjectsView`.
* `GoalsView`.
* `HabitsView`.

---

## 2026-07-20

### Frontend

* Compatibilidade entre Node.js, Vue-TSC e TypeScript.
* Padronização das animações no Tailwind CSS.
* Implementação de utilitários para aceleração por GPU.
* Adição de microinterações.

### Stores

* `toastStore`.
* `journalStore`.
* `settingsStore`.

### Componentes

* Tooltip de explicabilidade no `TopFocusCard`.
* Remoção de elementos visuais coloridos e emojis.
* Padronização da interface monocromática.

### Views

* `SettingsView`.
* `JournalView`.

### Modais

* `EditCommitmentModal`.
* `DailyShutdownModal`.

# Developer Changelog

## 2026-07-21

### Backend & API
- Adicionado endpoint `/api/v1/health` para monitoramento da API.
- Implementado seeder de usuário padrão para ambiente de desenvolvimento.
- Padronizada a propagação de erros utilizando RFC 7807 (Problem Details).
- Adicionado suporte a Correlation ID (`X-Correlation-Id`) nas requisições.

### Banco de Dados
- Corrigido o mapeamento TPH para propriedades específicas de subtipos (`CronExpression`, `StartTime` e `EndTime`).
- Criada a migration `FixTphNullability`.

### Frontend
- Implementado parser do Quick Capture baseado em tokens (`@`, `!`, `#`, `/`).
- Adicionado monitoramento de Long Tasks utilizando `PerformanceObserver`.
- Implementado tratamento global de erros da aplicação Vue.
- Adicionado suporte a fila de sincronização offline.
- Implementado rastreamento de requisições via Correlation ID.

### Stores
- Criada `useDevStore`.
- Criada `useOfflineStore`.

### Componentes
- Criado `ErrorBoundary`.
- Criado `DeveloperConsole`.
- Atualizada `StatusBar` com informações de sincronização, telemetria e requisições pendentes.

### Modais
- Refatorado `QuickCaptureModal` para entrada única baseada em comandos.
- Adicionada herança de contexto conforme a tela atual.

# Developer Changelog

# 2026-07-22

## Arquitetura & DX

- Implementação do orquestrador de desenvolvimento (`dev-orchestrator.mjs`).
- Adição de proteção contra execuções recursivas no Windows.
- Implementação da limpeza automática de processos da API antes da inicialização.
- Adição da verificação de disponibilidade do PostgreSQL via TCP.
- Padronização do `package.json` para suporte a ECMAScript Modules (`type: module`).

---

## Frontend

- Substituição de cores literais por Design Tokens utilizando CSS Custom Properties.
- Padronização da interface monocromática.
- Implementação do sistema de temas com persistência no `localStorage`.
- Adição de proteção contra FOUC (`Flash of Unstyled Content`) no `index.html`.
- Criação de 8 temas visuais.

---

## Stores

- `useThemeStore`

---

## Views

- Refatoração da `SettingsView.vue` para seleção de temas e visualização dos tokens em tempo real.

---

# 2026-07-23

## Backend & API

- Adição do `TraceIdentifier` nas respostas de erro (`RFC 7807`) para rastreabilidade.
- Configuração do `EnableRetryOnFailure` no Entity Framework Core.
- Implementação do endpoint `/api/v1/healthz` para monitoramento da API e do banco de dados.

---

## Frontend

- Implementação de Code Splitting e Lazy Loading para rotas e componentes.
- Otimização da geração de chunks no `vite.config.ts`.
- Implementação do composable `useFocusTrap.ts`.
- Adição de atributos de acessibilidade (`role="dialog"` e `aria-modal="true"`).

---

## Stores

- `useOnboardingStore`

---

## Componentes

- `OnboardingSteps`
- `SpotlightOverlay`

---

## Testes

- Configuração do Vitest com ambiente `jsdom`.
- Testes unitários para `offlineStore`.
- Testes unitários para o parser NLP (`nlpParser.spec.ts`).

---

# 2026-07-24

## Banco de Dados

- Criação de índices analíticos no PostgreSQL.
- Correção da leitura de entidades na estratégia TPH.
- Criação da migration `AddAnalyticsPartialIndexes`.

---

## Backend & API

- Implementação dos DTOs de telemetria.
- Implementação do `ProgressService`.
- Criação do `ProgressController`.
- Implementação de cache em memória e suporte a ETag (`304 Not Modified`).

---

## Frontend

- Implementação da sincronização entre dados históricos da API e dados do dia armazenados em memória.

---

## Stores

- `useProgressStore`

---

## Componentes

- `ProgressKpiGrid`
- `EstimationAccuracyChart`
- `FrictionHeatmapGrid`
- `ExecutionChronology`

---

## Views

- Refatoração da `JournalView.vue` com painel de evolução e filtro por período.

---

## Testes

- Testes unitários para `ProgressService`.
- Testes unitários para `progressStore`.

---

# 2026-07-25

## Banco de Dados

- Adição da coluna `last_used_at` na entidade `Project`.
- Criação de índice composto para catálogo LRU.
- Criação da migration `AddProjectLastUsedAt`.

---

## Backend & API

- Implementação do `ProjectCatalogDto`.
- Implementação da consulta `GetActiveCatalogAsync`.
- Criação do endpoint `GET /api/v1/projects/catalog`.
- Configuração de cache para respostas do catálogo.

---

## Frontend

- Refatoração do parser NLP (`nlpParser.ts`) com tipagem mais segura e suporte aos comandos `/t`, `/h`, `/e` e `/n`.
- Implementação de suporte a datas relativas (`^hoje`, `^amanha`, `^seg`).
- Implementação da estrutura `TrieIndex` para busca por prefixo.
- Implementação do composable `useKeyboardNavigation.ts`.

---

## Stores

- `useProjectsStore`

---

## Componentes

- `AutoCompleteDropdown`
- Refatoração do `QuickCaptureModal` com auto-complete em tempo real.
- Correção do recorte visual do dropdown removendo `overflow-hidden` do container principal.

---

## Testes

- Testes unitários para o parser NLP.
- Testes de desempenho e precisão do `TrieIndex`.
- Testes do `useKeyboardNavigation`.

# 2026-07-26

## Banco de Dados

- Criação da tabela `user_scoring_profiles` com suporte a controle de concorrência utilizando `xmin` (`IsRowVersion()`).
- Criação de índice parcial `idx_decision_snapshots_user_recent_analytics` para otimizar consultas de histórico analítico.
- Correção do filtro do índice `idx_projects_user_catalog_lru`, ajustando os valores do enum para minúsculas (`completed` e `archived`) para compatibilidade com o PostgreSQL.
- Criação e aplicação da migration `AddAdaptiveScoringProfiles`.

---

## Backend

- Refatoração do `ScoringEngine`, adicionando pesos adaptativos, limites de segurança para os cálculos e calibração baseada no Índice de Acurácia de Estimativa (EAI).
- Implementação do padrão **Null Object** (`UserScoringProfile.Default`).
- Implementação da regra mínima de amostragem para ativação da calibração (`SampleCount >= 10`).
- Implementação do `UserBehaviorProfilerService`, responsável por analisar o histórico do usuário, calcular o perfil comportamental e tratar valores extremos.
- Criação do `BehavioralCalibrationWorker`, executando a recalibração automática em segundo plano e atualizando os perfis por meio de UPSERT no PostgreSQL.

---

## API

- Atualização dos DTOs `DecisionResponseDto` e `ScoredActionDto`, adicionando informações do perfil adaptativo (`AdaptiveProfileDto`).
- Refatoração do `DecisionsController` para eliminar problemas de concorrência durante o acesso ao `DbContext`.

---

## Frontend

- Atualização da `useDecisionStore` com suporte ao armazenamento local do perfil adaptativo e funcionamento em modo offline.
- Adição do alias `fetchNow` para manter compatibilidade com versões anteriores da aplicação.
- Inclusão do indicador de transparência algorítmica nos componentes `TopFocusCard.vue` e `CommitmentCard.vue`.
- Criação do componente `ScoreBreakdownPanel.vue` para exibir a composição detalhada da pontuação calculada pelo motor de decisão.

---

## Testes

- Criação de testes unitários para `ScoringEngine` e `UserBehaviorProfilerService`, cobrindo cálculos, regras de calibração e tratamento de casos extremos.
- Criação de testes para `decisionStore`, validando hidratação da API, funcionamento offline e uso do cache em memória.