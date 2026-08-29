# Handoff: 002-planning-inbox (Planning Inbox Inicial)

- **ID da Spec**: `002-planning-inbox`
- **Nome da Feature**: Planning Inbox Inicial
- **Data**: 2026-08-28
- **Branch / Worktree**: `master` (branch de feature correspondente: `002-planning-inbox`)
- **Status**: Concluída

---

## 1. Problema Resolvido

O usuário necessitava de um local centralizado para capturar rapidamente tarefas e ideias no Compass sem atrito, associando inicialmente apenas um título. Sem a separação de rascunhos (`Draft`) e tarefas refinadas com estimativa (`Ready`), o motor de planejamento diário não teria critérios claros para discernir demandas planejáveis daquelas ainda indefinidas.

---

## 2. Comportamento Entregue ao Usuário

- **Captura Rápida na Inbox**:
  - Campo de entrada no topo da tela `/planning` permitindo digitar o título e pressionar Enter ou clicar em "Capturar".
  - A tarefa nasce imediatamente no backend como `Draft` (sem estimativa de duração).
- **Organização Visual em Abas**:
  - Filtros acessíveis por abas: `Todas`, `Draft`, `Ready`, `Em Andamento` (`InProgress`) e `Concluídas` (`Done`), com contadores de itens em tempo real.
- **Definição de Estimativa e Promoção para Ready**:
  - Em tarefas `Draft`, um controle inline permite informar a estimativa de tempo em minutos (deve ser `> 0`).
  - Ao salvar uma estimativa válida, o backend atualiza a duração e promove automaticamente a tarefa para o status `Ready`.
- **Edição Completa e Modal de Detalhes**:
  - O usuário pode abrir um modal acessível para alterar título, notas/descrição, estimativa em minutos e prazo (`deadline`), com validação e suporte à tecla Escape.
- **Ciclo de Vida Controlado pelo Backend**:
  - Tarefas `Ready` possuem botões de ação para "Iniciar" (passando para `InProgress`) e "Concluir" (passando para `Done`).
  - Tarefas `InProgress` podem ser concluídas diretamente, gravando o timestamp UTC `completedAt`.
  - Tentativas de avançar tarefas `Draft` sem estimativa são rejeitadas pelo backend com mensagem clara.
- **Persistência Real pós-F5**:
  - Todos os dados são recuperados do PostgreSQL via TanStack Vue Query. Recarregar a página (F5) mantém 100% do estado sem perdas e sem dados mockados.
- **Feedback Visual e Resiliência**:
  - Componentes de Empty State dedicados e amigáveis para cada aba vazia.
  - Indicadores visuais de loading não-bloqueantes e banners de alerta para erros de requisição ou validação.

---

## 3. Critérios de Aceitação Atendidos

- [x] **US1 / AC1-AC3**: Captura rápida por título iniciando como `Draft`, visualização e filtros por status na Inbox e persistência completa pós-recarregamento (F5).
- [x] **US2 / AC1-AC3**: Atribuição de estimativa positiva em minutos promovendo para `Ready`, rejeição de estimativas `<= 0` com erro de validação e inelegibilidade explícita de `Draft` para o planejamento.
- [x] **US3 / AC1-AC4**: Edição de título, estimativa e deadline, avanço de ciclo de vida (`Ready` -> `InProgress` -> `Done`), e rejeição de início em tarefas `Draft` ou modificações em tarefas `Done`.
- [x] **US4 / AC1-AC3**: Mensagens acolhedoras de estado vazio por categoria, loading states e tratamento de erros do servidor.
- [x] **SC-001 a SC-005**: 100% dos critérios mensuráveis atingidos.

### Critérios Não Atendidos
*Nenhum*. Todos os requisitos funcionais e critérios de aceitação foram 100% atendidos.

---

## 4. Decisões de Domínio

1. **Agregado Raiz `Task`**: Criado em `Compass.Modules.Planning.Domain.Model.Task` encapsulando `Id` (UUIDv7), `Title`, `Description`, `DurationMinutes`, `Deadline`, `Status`, `CreatedAt`, `UpdatedAt` e `CompletedAt`.
2. **Invariante de Estimativa**: `DurationMinutes` só pode ser nulo ou inteiro estritamente positivo (`> 0`). Valores `<= 0` disparam `PlanningDomainException`.
3. **Máquina de Estados Interna**:
   - `Create(title, description, durationMinutes, deadline)`: Se `durationMinutes == null` -> `Draft`; se `durationMinutes > 0` -> `Ready`.
   - `SetEstimate(durationMinutes)`: Se `null` -> rebaixa para `Draft`; se `> 0` -> promove para `Ready`.
   - `Start()`: Apenas permitido de `Ready` para `InProgress`. Rejeita `Draft` e `Done`.
   - `Complete()`: Transiciona de `Ready` ou `InProgress` para `Done` e define `CompletedAt = DateTimeOffset.UtcNow`.
4. **Backend como Única Fonte da Verdade**: Todas as transições de status e regras de validação residem no agregado de domínio; o frontend apenas dispara commands.

---

## 5. Decisões Arquiteturais

1. **Fundação do Módulo `Planning`**: Estruturado nos 5 projetos padrão do Monólito Modular (`Contracts`, `Domain`, `Application`, `Infrastructure`, `Presentation`).
2. **CQRS Puro**: Handlers implementando `ICommandHandler<TCommand, TResponse>` e `IQueryHandler<TQuery, TResponse>` sem sufixos proibidos (`UseCase`, `CommandService`).
3. **Persistência PostgreSQL Isolada**: Schema dedicado `planning` (tabela `planning.tasks`), mapeado no `PlanningDbContext` e isolado do schema `calendar`.
4. **Minimal APIs**: Endpoints mapeados sob `/api/planning/tasks` na camada de Presentation.
5. **Frontend FSD**: Entidade `entities/task`, componentes de feature `features/planning-inbox` e tela `pages/planning/PlanningPage.vue` consumindo dados via `@tanstack/vue-query`.

---

## 6. Entidades e Invariantes

- `Task`: Agregado raiz
  - `Id`: `Guid` gerado via `Guid.CreateVersion7()`.
  - `Title`: `string` obrigatória (1..255 caracteres, sem espaços em branco puros).
  - `Description`: `string?` opcional.
  - `DurationMinutes`: `int?` (`> 0` se preenchido).
  - `Deadline`: `DateTimeOffset?` normalizado em UTC.
  - `Status`: `TaskStatus` (`Draft`, `Ready`, `InProgress`, `Done`).
  - `CreatedAt` / `UpdatedAt`: `DateTimeOffset` em UTC.
  - `CompletedAt`: `DateTimeOffset?` em UTC.

---

## 7. Commands, Queries e Handlers

- `CreateTaskCommand(string Title, string? Description, int? DurationMinutes, DateTimeOffset? Deadline)` -> `CreateTaskCommandHandler`
- `SetTaskEstimateCommand(Guid Id, int? DurationMinutes)` -> `SetTaskEstimateCommandHandler`
- `UpdateTaskDetailsCommand(Guid Id, string Title, string? Description, int? DurationMinutes, DateTimeOffset? Deadline)` -> `UpdateTaskDetailsCommandHandler`
- `StartTaskCommand(Guid Id)` -> `StartTaskCommandHandler`
- `CompleteTaskCommand(Guid Id)` -> `CompleteTaskCommandHandler`
- `GetTasksQuery(TaskStatus? Status)` -> `GetTasksQueryHandler`
- `GetTaskByIdQuery(Guid Id)` -> `GetTaskByIdQueryHandler`

---

## 8. Contratos Cross-Module

- `IPlanningModule`:
  - `Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)`
  - `Task<IReadOnlyList<TaskDto>> GetReadyTasksAsync(CancellationToken cancellationToken = default)`
- DTOs públicos: `TaskDto`, `CreateTaskRequest`, `UpdateTaskRequest`, `SetTaskEstimateRequest`.

---

## 9. Endpoints HTTP

| Método | Rota | Request Body | Response Body | Status Codes |
|---|---|---|---|---|
| `POST` | `/api/planning/tasks` | `CreateTaskRequest` | `TaskDto` | `201 Created` (`Location: /api/planning/tasks/{id}`), `400 Bad Request` |
| `GET` | `/api/planning/tasks` | *(Query: `?status=Draft`)* | `IReadOnlyList<TaskDto>` | `200 OK` |
| `GET` | `/api/planning/tasks/{id:guid}` | *(None)* | `TaskDto` | `200 OK`, `404 Not Found` |
| `PATCH` | `/api/planning/tasks/{id:guid}` | `UpdateTaskRequest` | `TaskDto` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `POST` | `/api/planning/tasks/{id:guid}/start` | *(None)* | `TaskDto` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `POST` | `/api/planning/tasks/{id:guid}/complete` | *(None)* | `TaskDto` | `200 OK`, `400 Bad Request`, `404 Not Found` |

---

## 10. Tabelas e Migrations

- **Schema**: `planning`
- **Tabela**: `planning.tasks`
  - `"Id"` (`uuid`, PK)
  - `title` (`varchar(255)`, NOT NULL)
  - `description` (`text`, NULL)
  - `duration_minutes` (`integer`, NULL)
  - `deadline` (`timestamptz`, NULL)
  - `status` (`varchar(50)`, NOT NULL)
  - `created_at` (`timestamptz`, NOT NULL)
  - `updated_at` (`timestamptz`, NOT NULL)
  - `completed_at` (`timestamptz`, NULL)
  - Índices: `IX_tasks_status` em `status`, `IX_tasks_created_at` em `created_at`.

---

## 11. Rotas e Telas Frontend

- `/planning` (`PlanningPage.vue`): Tela da Inbox com formulário de captura rápida, abas de filtro, listagem de cards de tarefas, feedback de loading, empty states e modal de edição.
- `/today` (`TodayPage.vue`): Atualizada com barra de navegação superior permitindo alternar entre Hoje (`/today`) e Planning (`/planning`).
- `router/index.ts`: Rota `/planning` registrada e protegida pelo router guard de autenticação/perfil ativo.

---

## 12. Query Keys e Invalidações

- `['tasks', statusRef]`: Chave do TanStack Vue Query para listagem reativa de tarefas.
- Invalidações automáticas da chave raiz `['tasks']` ao executar:
  - `useCreateTaskMutation`
  - `useUpdateTaskMutation`
  - `useStartTaskMutation`
  - `useCompleteTaskMutation`

---

## 13. Arquivos Criados, Modificados e Removidos

### Backend Criado:
- `src/Modules/Planning/Compass.Modules.Planning.Contracts/DTOs/TaskDto.cs`, `IPlanningModule.cs`, `Compass.Modules.Planning.Contracts.csproj`
- `src/Modules/Planning/Compass.Modules.Planning.Domain/Exceptions/PlanningDomainException.cs`, `Model/TaskStatus.cs`, `Model/Task.cs`, `Repositories/ITaskRepository.cs`, `Compass.Modules.Planning.Domain.csproj`
- `src/Modules/Planning/Compass.Modules.Planning.Application/Abstractions/ICommand.cs`, `Commands/CreateTaskCommand.cs`, `Commands/SetTaskEstimateCommand.cs`, `Commands/UpdateTaskDetailsCommand.cs`, `Commands/StartTaskCommand.cs`, `Commands/CompleteTaskCommand.cs`, `Queries/GetTasksQuery.cs`, `PlanningApplicationExtensions.cs`, `Compass.Modules.Planning.Application.csproj`
- `src/Modules/Planning/Compass.Modules.Planning.Infrastructure/Persistence/PlanningDbContext.cs`, `Persistence/Configurations/TaskConfiguration.cs`, `Persistence/Repositories/TaskRepository.cs`, `PlanningInfrastructureExtensions.cs`, `Compass.Modules.Planning.Infrastructure.csproj`
- `src/Modules/Planning/Compass.Modules.Planning.Presentation/Endpoints/PlanningEndpoints.cs`, `Extensions/PlanningModuleExtensions.cs`, `Compass.Modules.Planning.Presentation.csproj`

### Backend Testes Criado:
- `tests/Compass.Modules.Planning.Domain.UnitTests/TaskTests.cs`, `TaskEstimateTests.cs`, `TaskLifecycleTests.cs`, `Compass.Modules.Planning.Domain.UnitTests.csproj`
- `tests/Compass.Modules.Planning.Application.UnitTests/TaskHandlerTests.cs`, `SetTaskEstimateCommandHandlerTests.cs`, `TaskLifecycleHandlerTests.cs`, `Compass.Modules.Planning.Application.UnitTests.csproj`
- `tests/Compass.Modules.Planning.IntegrationTests/PlanningTestDatabaseFixture.cs`, `TaskPersistenceTests.cs`, `Compass.Modules.Planning.IntegrationTests.csproj`
- `tests/Compass.Host.IntegrationTests/TaskApiTests.cs`, `TaskEstimateApiTests.cs`, `TaskLifecycleApiTests.cs`

### Frontend Criado:
- `frontend/src/entities/task/types.ts`, `api/taskApi.ts`, `model/useTasksQuery.ts`, `model/useCreateTaskMutation.ts`, `model/useUpdateTaskMutation.ts`, `model/useStartTaskMutation.ts`, `model/useCompleteTaskMutation.ts`
- `frontend/src/shared/ui/EmptyState.vue`
- `frontend/src/features/planning-inbox/components/QuickTaskCapture.vue`, `components/TaskFilterTabs.vue`, `components/TaskCard.vue`, `components/TaskEditModal.vue`
- `frontend/src/features/planning-inbox/__tests__/PlanningInbox.spec.ts`, `TaskEstimate.spec.ts`, `TaskLifecycle.spec.ts`, `PlanningFeedback.spec.ts`
- `frontend/src/pages/planning/PlanningPage.vue`, `pages/planning/__tests__/PlanningPage.spec.ts`

### Arquivos Modificados:
- `Compass.slnx` (adicionados projetos de Planning e de testes)
- `src/Host/Compass.Host/Compass.Host.csproj`, `src/Host/Compass.Host/Program.cs` (registro do módulo Planning)
- `tests/Compass.Host.IntegrationTests/CustomWebApplicationFactory.cs` (suporte a múltiplos DbContexts e criação de schemas)
- `frontend/src/app/router/index.ts` (adicionada rota `/planning`)
- `frontend/src/pages/today/TodayPage.vue` (adicionada barra de navegação no cabeçalho)

---

## 14. Testes Adicionados e Resultados dos Quality Gates

### Backend (.NET 10 / xUnit / Testcontainers PostgreSQL)
- **Comando**: `dotnet test Compass.slnx --logger "console;verbosity=normal"`
- **Resultados**:
  - `Compass.Modules.Calendar.Domain.UnitTests`: 16 aprovados (0 falhas)
  - `Compass.Modules.Calendar.Application.UnitTests`: 4 aprovados (0 falhas)
  - `Compass.Modules.Calendar.IntegrationTests`: 1 aprovado (0 falhas)
  - `Compass.Modules.Planning.Domain.UnitTests`: 20 aprovados (0 falhas)
  - `Compass.Modules.Planning.Application.UnitTests`: 6 aprovados (0 falhas)
  - `Compass.Modules.Planning.IntegrationTests`: 1 aprovado (0 falhas)
  - `Compass.Host.IntegrationTests`: 14 aprovados (0 falhas)
  - **Total Backend**: **62 aprovados**, **0 falhas**.

### Frontend (Vitest / TypeScript Estrito / Vite)
- **Comando de Testes**: `npm test -- --run`
- **Resultados**: **27 testes aprovados em 9 arquivos**, **0 falhas**.
- **Comando de Build**: `npm run build` (`vue-tsc -b && vite build`)
- **Resultados**: **0 erros de tipagem**, bundle de produção gerado em `dist/`.

---

## 15. Divergências entre Spec, Plan, Tasks e Implementação

Nenhuma divergência identificada. Todas as 48 tarefas planejadas em `specs/002-planning-inbox/tasks.md` foram implementadas e validadas conforme especificado em `spec.md` e `plan.md`.

---

## 16. Débitos Técnicos e Riscos Conhecidos

- **Débito Aceito**: A ordenação atual da Inbox utiliza a data de criação (`created_at ASC/DESC`). Reordenação manual via drag-and-drop será especificada em feature futura.
- **Risco Conhecido**: Tarefas marcadas com deadline não disparam notificações ativas (push/email) nesta fase (o Compass atua como exibição e verificação no momento do carregamento).

---

## 17. Instruções para Executar a Feature

1. **Subir Backend (.NET 10)**:
   ```bash
   dotnet run --project src/Host/Compass.Host
   ```
2. **Subir Frontend (Vue 3)**:
   ```bash
   cd frontend
   npm run dev
   ```
3. **Navegar**: Acesse `http://localhost:5173/planning` para capturar e gerenciar tarefas na Inbox.

---

## 18. Próximo Passo Recomendado

Com a Inbox e o agregado `Task` do módulo `Planning` consolidados, o próximo passo no roteiro do Compass é o desenvolvimento do **motor de sugestão e geração do DailyPlan** (`/speckit-specify`), cruzando as tarefas `Ready` com a disponibilidade diária do módulo `Calendar`.
