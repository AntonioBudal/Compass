# Research: Planning Inbox Inicial

**Feature**: `002-planning-inbox` | **Date**: 2026-08-28

---

## 1. Módulo Planning no Monólito Modular

### Decisão
Criar o módulo `Planning` em `src/Modules/Planning/` respeitando a separação em 5 projetos de responsabilidade única:
- `Compass.Modules.Planning.Contracts`: DTOs públicos imutáveis e interfaces cross-module (`IPlanningModule`).
- `Compass.Modules.Planning.Domain`: Agregado raiz `Task`, enum de status `TaskStatus`, exceções de domínio e interface de repositório `ITaskRepository`.
- `Compass.Modules.Planning.Application`: Commands, Queries e Handlers CQRS puros (`CreateTaskCommand`, `UpdateTaskCommand`, `StartTaskCommand`, `CompleteTaskCommand`, `GetTasksQuery`, `GetTaskByIdQuery`).
- `Compass.Modules.Planning.Infrastructure`: `PlanningDbContext` com schema isolado `planning`, configurações EF Core e repositório `TaskRepository`.
- `Compass.Modules.Planning.Presentation`: Endpoints Minimal API (`PlanningEndpoints`) mapeados sob `/api/planning`.

### Rationale
Garante estrita aderência aos princípios constitucionais 1 a 6 (Monólito Modular, sem acesso direto a DbContext/Domain de outros módulos, comunicação cross-module apenas via `Contracts`, sem HTTP interno e com CQRS padrão).

### Alternativas Consideradas
- *Adicionar tarefas dentro do módulo Calendar*: Rejeitado porque tarefas e planejamento de backlog pertencem semanticamente ao domínio de `Planning`, violando o Princípio 1.
- *Usar controllers ASP.NET tradicionais*: Rejeitado para manter paridade e simplicidade com Minimal APIs adotadas no Host.

---

## 2. Máquina de Estados e Ciclo de Vida da Tarefa

### Decisão
O agregado `Task` encapsula completamente seu ciclo de vida através de métodos de domínio:
- `Task.Create(string title, int? durationMinutes, DateTimeOffset? deadline)`:
  - Se `durationMinutes == null`, status = `Draft`.
  - Se `durationMinutes > 0`, status = `Ready`.
- `task.SetEstimate(int? durationMinutes)`:
  - Se `durationMinutes == null`, rebaixa para `Draft`.
  - Se `durationMinutes > 0`, promove para `Ready` (se estava `Draft`).
- `task.Start()`:
  - Válido apenas de `Ready` -> `InProgress`.
  - Rejeita `Draft` com `PlanningDomainException("A task must have a duration estimate and be in Ready status before starting.")`.
  - Rejeita `Done` com `PlanningDomainException("Completed tasks cannot be restarted directly.")`.
- `task.Complete()`:
  - Válido a partir de `Ready` ou `InProgress` -> `Done`.
  - Registra `CompletedAt = DateTimeOffset.UtcNow`.

### Rationale
Centraliza as invariantes de negócio no agregado raiz de domínio, garantindo que o backend seja a única fonte da verdade e impedindo que o frontend ou chamadas de API corrompam o estado da tarefa.

---

## 3. Persistência PostgreSQL e Schema `planning`

### Decisão
- Utilizar schema dedicado `planning` no PostgreSQL (`planning.tasks`).
- Chaves primárias sequenciais geradas no backend via UUIDv7 (`Guid.CreateVersion7()`).
- Prazos e marcas temporais persistidos em `timestamp with time zone` (UTC).

### Rationale
Mantém isolamento de dados entre módulos no PostgreSQL e garante ordenação natural e performance de indexação B-Tree com UUIDv7.

---

## 4. Frontend: Inbox com FSD Pragmático e TanStack Vue Query

### Decisão
- Rota `/planning` renderizada por `frontend/src/pages/planning/PlanningPage.vue`.
- Componentes em `frontend/src/features/planning-inbox/`:
  - `QuickTaskCapture.vue`: Formulário de captura rápida por título com submissão via Enter/botão.
  - `TaskFilterTabs.vue`: Alternador de abas/visualização (`All`, `Draft`, `Ready`, `InProgress`, `Done`).
  - `TaskCard.vue`: Card de exibição da tarefa com título, badge de status, estimativa em minutos, deadline e botões de ação (Estimativa rápida, Iniciar, Concluir, Editar).
  - `TaskEditModal.vue`: Modal acessível para edição completa de título, estimativa e prazo.
- Entidade `frontend/src/entities/task/`:
  - `types.ts`, `taskApi.ts`, composables de consulta e mutação com invalidação automática da query key `['tasks']`.

### Rationale
Respeita a arquitetura FSD sem pastas vazias, garantindo estado remoto via Vue Query e reatividade rápida com zero dados mockados.
