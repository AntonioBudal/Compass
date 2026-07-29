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

# 2026-07-27

## Banco de Dados e Infraestrutura (.NET 10)

- Reconciliação da entidade `Setting`, forçando o mapeamento para tipos nativos (`jsonb`, `time without time zone`) e chave primária por `user_id`.
- Modelagem da tabela `daily_reviews` para armazenamento das métricas de encerramento diário e notas analíticas.
- Aplicação de índice composto único (`idx_daily_reviews_user_date`) para impedir duplicidade de encerramentos por turno.
- Implementação de controle transacional (`IDbContextTransaction`) e instruções SQL atômicas (`ON CONFLICT DO UPDATE`) para importação sem retenção de locks ou deadlocks no PostgreSQL.
- Geração e aplicação da migration `AddDailyReviewsAndSettingsReconciliation`.

---

## Backend e Domínio

- Criação da entidade `DailyReview.cs` com validações de invariantes anti-negatividade para tempo de foco e contagem de entregas.
- Criação do serviço `DataPortabilityService.cs`, executando exportações de leitura limpa (`AsNoTracking`) em $< 50\text{ms}$ e importações com política *Last-Write-Wins* baseada em timestamp UTC.
- Criação do serviço `DailyCycleService.cs` para gerenciar rituais diários:
  - **Morning Briefing:** Cálculo de carga horária líquida pendente e identificação de tarefas atrasadas.
  - **Daily Shutdown:** Registro de fechamento com injeção automática de tags de divergência algorítmica (`#underestimated`, `#flow`, `#interrupted`, `#low-energy`).

---

## API e Contratos

- Exposição do endpoint `GET /api/v1/portability/export`, anexando o cabeçalho `Content-Disposition` para download autossuficiente do pacote de backup (`v4.0.0-tactical`).
- Exposição do endpoint `POST /api/v1/portability/import` para ingestão transacional do bundle JSON.
- Exposição dos endpoints `GET /api/v1/daily-cycle/morning-briefing` e `POST /api/v1/daily-cycle/shutdown` para consumo na interface de rituais.

---

## Frontend e UX

- Separação arquitetural no menu lateral (`Sidebar.vue`) entre duas modalidades de teste em memória:
  - **`[RAM SANDBOX]` (Simulador E2E):** Injeta um ecossistema completo (projetos, tarefas, hábitos em streak, eventos e EAI calibrado em $1.4\text{x}$) diretamente na tela Agora (`/now`).
  - **`[TUTORIAL]` (Guia Pedagógico):** Limpa a memória e abre o fluxo passo a passo de aprendizagem em `/sandbox`.
- Reestruturação do componente `OnboardingSteps.vue` em 5 etapas pedagógicas explicativas (Task, Event, Habit, Note), permitindo a simulação interativa de cada tipo na RAM via botões práticos.
- Criação do host visual `SandboxView.vue`, renderizando em tempo real os itens gerados durante o tutorial no fundo da tela.
- Acoplamento das ações de importação, exportação e reset de banco de dados na tela `SettingsView.vue`, com leitura assíncrona de arquivos via `FileReader`.

---

## Segurança e Soberania de Dados (Local-First)

- Implementação do escudo de validação `portabilitySchema.ts` via biblioteca **Zod**, interceptando arquivos JSON corrompidos ou malformados em milissegundos no cliente antes do envio à API.
- Implementação de sincronização reativa multi-aba em `settingsStore.ts` e `dailyCycleStore.ts` utilizando a API nativa `BroadcastChannel` e ouvintes do evento `storage`.

---

## Testes e Homologação E2E

- Criação de testes unitários no xUnit (`DataPortabilityServiceTests.cs` e `DailyCycleServiceTests.cs`), 
- Criação da suíte mestre no Vitest (`e2e-ecosystem.spec.ts`) com mock estruturado de instâncias

# 2026-07-28

## Arquitetura de UX Defensiva e Visibilidade

- Criação dos contratos `DefensiveIntervention` e `DefensiveAction` em `types/index.ts`.
- Evolução da `toastStore.ts` e do `ToastContainer.vue` para suportar notificações de intervenção com ações imediatas.
- Criação do composable `useVisibilityTracker.ts` para detectar quando um item recém-criado não está visível na view atual ou foi roteado para outra tela.
- Implementação de validações defensivas na `commitmentsStore.ts`:
  - Alerta para criação de tarefas fora do turno útil.
  - Aviso ao criar tarefas sem projeto vinculado.
  - Bloqueio de hábitos duplicados no mesmo dia com feedback de streak.
  - Inserção automática da expressão CRON (`0 8 * * *`) na criação de hábitos.

---

## Horizonte Tático e Navegação Temporal

- Criação do componente `TacticalHorizonBar.vue`.
- Navegação entre **Hoje**, **Amanhã**, **Próximos 3 Dias** e **Próxima Semana**.
- Suporte aos atalhos `Shift + 1..4`.
- Integração da navegação temporal nas views:
  - `NowEngineView.vue`
  - `HabitsView.vue`
  - `AgendaView.vue`

---

## Evolução de Views e Componentes

- Criação do componente `DefensiveEmptyState.vue` para estados vazios com explicação e ações de recuperação.

### Projetos

- Refatoração da `ProjectsView.vue`.
- Organização em abas (**Em Andamento** e **Concluídos**).
- Exibição do progresso do turno.
- Adição do botão **+ Turno**, abrindo o Quick Capture com o projeto pré-selecionado.

### Metas

- Refatoração da `GoalsView.vue`.
- Estrutura em Accordion (Strategic Tree).
- Edição inline de títulos.
- Ajuste reativo de progresso por slider.
- Criação da `goalsStore.ts` com persistência em `localStorage`.

---

## Rede, Offline e Portabilidade

- Padronização das portas da aplicação:
  - API (.NET 10): `http://localhost:5000`
  - Frontend (Vite): `http://localhost:5173`
- Criação do `SandboxTopBanner.vue`, exibindo um aviso permanente durante o modo Sandbox.
- Implementação de proteção contra fechamento da aba no `StatusBar.vue` quando houver sincronizações offline pendentes.
- Melhoria das mensagens de erro na importação de arquivos `.json`, indicando exatamente quais campos falharam na validação do Zod.

---

## Testes e Homologação

- Atualização da suíte `e2e-ecosystem.spec.ts`.
- Cobertura para:
  - Injeção automática de CRON em hábitos.
  - Bloqueio de duplicidade diária.
  - Alertas para tarefas fora do turno e sem projeto.
  - Validação de arquivos corrompidos via Zod.
  - Teste de estresse com **500 compromissos**.
  - Verificação de latência reativa em memória inferior a **16 ms**.
