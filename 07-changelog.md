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

# 2026-07-29

## Laboratório Interativo & "Glass-Box" Pipeline

- Criação do componente `GlassBoxPipeline.vue`, atuando como um **Raio-X** da inteligência do sistema.
- Exposição visual, em tempo real, do fluxo completo de processamento:
  - **Input → Parser NLP → Validação Defensiva → Motor de Decisão → Preview → Efeito Borboleta**.
- Reestruturação da `OnboardingView.vue` para uma jornada híbrida:
  - Introdução teórica rápida.
  - Transição automática para o laboratório interativo em **Split View**.

---

- Criação da tela `InteractiveLabView.vue`, substituindo o onboarding estático por **10 desafios pedagógicos interativos**.
- Implementação de uma máquina de estados integrada ao `nlpParser`, avaliando os comandos digitados em tempo real.
- Desenvolvimento das fases de aprendizagem dos arquétipos:
  - **Task:** criação guiada com cálculo dinâmico de Score.
  - **Event:** demonstração de bloqueio da Agenda e conflitos de horário.
  - **Habit:** explicação da consistência diária e sistema de *Streaks*.
  - **Note:** utilização do *Brain Dump* para captura rápida de ideias.

### Simuladores Interativos

- Implementação de um simulador do **Now Engine**, utilizando controles de Energia e Tempo para demonstrar o reordenamento dinâmico da fila de execução.
- Implementação de uma simulação visual do **Estimation Accuracy Index (EAI)**, mostrando como o sistema aprende e recalibra estimativas de duração.

### Pedagogia Baseada em Erros

- Inclusão de cenários com falhas intencionais (como sobreposição de horários e criação de tarefas fora do turno) para demonstrar o funcionamento da **UX Defensiva**.
- Explicação contextual do motivo de cada erro e das possíveis formas de resolução diretamente durante o laboratório.

### Missão Final

- Desenvolvimento de um desafio final em formato de "Boss Fight", exigindo o uso correto da gramática do Terminal Prompt para validar todo o conhecimento adquirido durante o treinamento.

---

## Desbloqueio Progressivo (Boot Sequence)

- Reengenharia do `AppLayout.vue`, ocultando **Sidebar**, **Header** e **StatusBar** para usuários que ainda não concluíram o onboarding (`compass_onboarded === false`).
- Criação do evento SPA `compass:boot-sequence` no `onboardingStore.ts`, permitindo que a interface seja construída gradualmente ao redor do usuário por meio de animações CSS, sem necessidade de recarregar a página.
- Desenvolvimento do componente `PilotChecklistWidget.vue`, exibindo um checklist flutuante e reativo para orientar os primeiros passos no banco de dados real, incluindo:
  - Criação do primeiro Projeto.
  - Criação da primeira Task.
  - Registro do primeiro Habit.
  - Acesso à Agenda.
  - Realização da primeira Daily Review.

# 2026-07-30

# Universal Entity Inspector & UX Foundation

- Início da substituição do `EditCommitmentModal.vue` pelo novo `UniversalEntityInspector.vue`, estabelecendo uma arquitetura única para edição de todas as entidades do sistema.

- Criação da infraestrutura do `inspectorStore`, utilizando um **Draft isolado** para impedir mutações diretas nas Stores durante a edição.

- Estruturação do **Shell do Inspetor Universal** com carregamento dinâmico  para formulários especializados de Tasks, Habits, Events, Projects, Goals e Notes.

- Implementação do **Slide-over Inspector**, com atalhos de teclado (`ESC`, `Ctrl+S`), indicador de Auto-Save e base para persistência desacoplada das Stores de domínio.

# Interface Adaptativa (View Density)

- Planejamento da infraestrutura de **View Density**, permitindo alternar entre modo detalhado e modo compacto em diferentes telas do Compass.

- Definição da arquitetura para reutilização da densidade visual em `NowEngineView`, `ProjectsView` e futuras páginas do sistema.

- Preparação dos componentes (`CommitmentCard` e `TopFocusCard`) para suportarem múltiplos níveis de informação sem duplicação de código.

# UX Defensiva

- Planejamento da próxima camada de **UX Defensiva**, expandindo o conceito para todos os modais e fluxos de edição do sistema.

- Definição dos principais cenários de intervenção ao usuário para evitar ações silenciosas, conflitos e perda de dados durante operações críticas.

# 2026-07-31

# Autocomplete Engine

- Implementação da infraestrutura inicial do Autocomplete Engine baseada em Providers independentes.

- Criação da primeira suíte de testes automatizados com Vitest para validar o funcionamento do motor de autocomplete.

- Diagnóstico da integração entre o motor e a Quick Capture, identificando que o algoritmo estava funcionando, mas ainda não conectado à interface.

- Definição da estratégia para utilizar o histórico de tarefas como principal fonte de sugestões inteligentes (Ghost Text).

# Universal Entity Inspector

- Finalização da auditoria do fluxo de edição do Universal Inspector.

- Confirmação de que o Frontend está enviando corretamente os payloads de atualização para o Backend.

- Investigação do problema de "Phantom Save", onde o Frontend indicava sucesso mesmo sem persistência dos dados.

# Auditoria do Backend

- Descoberta da ausência completa da infraestrutura de Update para Goals e Commitments.

- Identificação da falta de Controllers, DTOs, Validators, Services e métodos de domínio responsáveis pela edição das entidades.

- Planejamento da implementação completa do pipeline de atualização seguindo a arquitetura em camadas do projeto.

# Infraestrutura de Testes

- Planejamento da arquitetura de testes do Backend.

- Organização da estrutura para testes de Domain, Application, Infrastructure e API.

- Definição da base para testes unitários, integração, End-to-End e cobertura de código.

# 2026-08-01

# Correções de Backend e Sincronização

- Correção dos erros de validação do Backend (.NET) relacionados à duração mínima de tarefas e expressões CRON de hábitos.

- Implementação de sanitização de dados legados no Frontend para evitar envio de tarefas com duração inválida.

- Refatoração das Stores para preservar a reatividade utilizando `Object.assign`, corrigindo a atualização das telas após edições.

- Adição do controle de hidratação (`isLoaded`) para evitar requisições desnecessárias e problemas de cache.

# Universal Entity Inspector

- Finalização da integração do Auto-Save entre o Inspector e o Backend para Compromissos, Metas e Projetos.

- Migração dos formulários para o padrão `defineModel`, simplificando a comunicação entre o Inspector e os componentes de edição.

# Agenda

- Refatoração do `AllocationPickerModal`, permitindo alocação direta de tarefas e hábitos na Agenda com cálculo automático de horários.

- Ajustes no payload enviado ao endpoint `PUT` para compatibilidade com as validações do Backend.

# Biblioteca View

- Criação da `LibraryView.vue` reunindo documentação da arquitetura, sintaxe do Quick Capture, atalhos de teclado e conceitos do sistema.

- Integração da nova tela na Sidebar e no sistema de rotas.

# Database View

- Implementação da `DatabaseView.vue`, exibindo todos os registros persistidos em uma tabela de alta densidade.

- Adição de filtros por tipo, status e projeto, além de ações para abrir o Inspector e excluir registros.

# 2026-08-03

# Gerenciamento de Estado

- Refatoração das Stores para utilizar um modelo baseado em **Single Source of Truth (SSOT)**, adotando dicionários em memória para acesso O(1).

- Atualização das principais Views (`NowEngineView`, `DatabaseView`, `HabitsView` e `SettingsView`) para consumirem diretamente o estado global, eliminando cópias locais e reduzindo problemas de sincronização.

- Remoção de watchers profundos desnecessários no Pinia, reduzindo processamento reativo e melhorando a performance da interface.

# Persistência e Sincronização

- Refatoração da fila de operações offline, movendo a lógica de **Undo** para a `offlineStore` utilizando o padrão **Command Pattern**, garantindo persistência mesmo após recarregar a aplicação.

- Ajuste do interceptor do Axios para diferenciar corretamente falhas reais de rede de erros retornados pelo Backend, evitando falsos sucessos.

- Implementação de tratamento idempotente para operações de exclusão, considerando respostas `404 Not Found` como sucesso quando o recurso já não existe.

# Backend

- Implementação do método `DeleteAsync` no `CommitmentService`, centralizando a lógica de remoção com validações de domínio.

- Adição do endpoint `DELETE /api/v1/commitments/{id}` no `CommitmentsController`, integrando o fluxo de exclusão ao repositório e ao `SaveChangesAsync`.


# 2026-08-05

## Hierarquia Meta → Projeto → Tarefa

* Implementação da nova estrutura hierárquica do Compass, organizando o fluxo entre Metas, Projetos e Tarefas.

* Atualização da entidade `Project` e dos DTOs para suportarem o relacionamento opcional com `Goal` através de `GoalId`.

* Criação da infraestrutura de Metas no Backend, incluindo repositório, controller e operações completas de CRUD.

## Frontend

* Implementação do cálculo automático de progresso em cascata, onde tarefas atualizam projetos e projetos atualizam metas de forma reativa.

* Atualização do Quick Capture para reconhecer os comandos `/meta` e `/projeto`, permitindo criar entidades estratégicas diretamente pelo capturador.

* Simplificação da `ProjectsView` e `GoalsView`, removendo regras de negócio da interface e concentrando a lógica nas Stores.

* Evolução do Universal Entity Inspector com seleção dinâmica de relacionamentos utilizando listas (`select`) em vez de campos de texto livres.

* Adição de validações de UX para impedir a exclusão de projetos e metas que ainda possuam entidades vinculadas.

## Correções

* Correção da implementação dos repositórios garantindo suporte ao `SaveChangesAsync`.

* Correção do fluxo de criação para utilizar exclusivamente os identificadores retornados pelo Backend, eliminando IDs temporários e evitando erros de persistência.

* Ajustes gerais na integração entre Frontend e Backend para garantir a consistência dos relacionamentos entre Metas, Projetos e Tarefas.


# 2026-08-07

## Banco de Dados

- Migração da persistência principal do IndexedDB (Frontend) para um banco local em SQLite (Backend) via Entity Framework Core.
- Remoção de funções específicas do PostgreSQL nas configurações do EF Core para garantir a geração limpa das Migrations locais.

## Gerenciamento de Estado

- Expurgo do `localStorage` e IndexedDB nas Stores principais:
  - `commitmentsStore`
  - `projectsStore`
  - `goalsStore`
  - `progressStore`
- O Pinia agora atua como um espelho de estado puro da API (**State Mirror**), eliminando o armazenamento duplicado no navegador.

## Sincronização e Resiliência

- Correção de rota na `offlineStore` para garantir que a fila de comandos pendentes aponte para a porta correta do Backend (`5000`).
- Adição de limpeza de cache na `progressStore` em caso de falha de conexão, evitando a exibição de gráficos e métricas corrompidas.

## Interface e UX

- Atualização do `OmniInput` com novos placeholders sugestivos.
- Inclusão dos novos atalhos de criação rápida (`/meta` e `/projeto`) na documentação viva da `LibraryView`.

# 2026-08-08

# Refatoração da Agenda Tática

* Divisão da `AgendaView.vue` em componentes menores seguindo responsabilidade única.
* Criação dos componentes `AgendaHeader`, `AgendaBacklog`, `AgendaTimeGrid`, `AgendaRestBlocks`, `AgendaFreeBlocks`, `AgendaCommitmentBlock`, `AgendaGhostBlock` e `AgendaConflictModal`.
* `AgendaView.vue` passou a atuar principalmente como orquestradora do estado, cálculos temporais e eventos de Drag & Drop e Resize.
* Isolamento da renderização dos compromissos, grade de horários, blocos de descanso, espaços livres e pré-visualização do Drag.

# Estado e Sincronização

* Ajuste da atualização de `entities` na `commitmentsStore` para preservar a reatividade da Agenda após o Drop.
* Correção do fluxo de atualização local após o `PUT`, garantindo que o compromisso seja refletido na interface imediatamente.
* Implementação de proteção no Frontend para manter o estado visual quando a resposta da API não retorna corretamente os dados de horário.

# Drag & Drop

* Mantida a pré-visualização do compromisso durante o arraste.
* Integração do cálculo de horário com a atualização persistida via `PUT`.
* Agenda passa a atualizar visualmente o compromisso após a movimentação.

# Devlog: 2026-08-09

## Persistência e Herança TPH (Backend)

- Identificada a causa da perda de `StartTime` após o F5: conflito na herança TPH do Entity Framework Core.
- Movido `StartTime` para a classe base `Commitment`, unificando o horário de Tarefas, Hábitos e Eventos.
- Ajustados `UpdateAsync` e `MapToDto` em `CommitmentService.cs` para persistir corretamente o horário e permitir `startTime = null`.
- Corrigidos `TimeWindowCalculator.cs` e `DecisionService.cs` para suportar `DateTime?`.

## Hábitos e Inspetor

- Corrigido o erro `400 Bad Request` ao arrastar Hábitos para a Agenda.
- Atualizado o payload de Hábitos para enviar também `cronExpression`.
- Adicionado campo de horário ao `HabitEditorForm.vue`.
- Sincronização do horário do Hábito entre Inspetor, Store e Agenda.

## Agenda Tática

- Implementado Drag & Drop reverso: compromissos podem ser arrastados da Agenda de volta para o Backlog.
- Ao retornar ao Backlog, o `startTime` é removido e a tarefa volta para `unscheduled`.
- Adicionado aviso no carregamento da Agenda quando existem compromissos sem horário.

## Arquitetura

- Unificação da persistência de `StartTime` no modelo base reduz divergências entre os tipos de compromisso.
- Fluxo de horário agora segue uma única fonte de verdade entre Backend, Store, Inspetor e Agenda.
