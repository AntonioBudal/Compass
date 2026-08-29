# Implementation Plan: Planning Inbox Inicial

**Branch**: `002-planning-inbox` | **Date**: 2026-08-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/002-planning-inbox/spec.md`

---

## Summary

Implementar a fundação do módulo **`Planning`** e a tela de **Planning Inbox** no Compass, permitindo a captura rápida de tarefas (iniciadas como `Draft`), organização por status (`Draft`, `Ready`, `InProgress`, `Done`), refinamento de estimativa em minutos (com promoção para `Ready`), edição de detalhes (título, estimativa, deadline) e transições controladas de ciclo de vida no backend com persistência no schema PostgreSQL `planning`.

---

## Technical Context

**Language/Version**: .NET 10 (C# 13), TypeScript 5.x / Vue 3 (Composition API / `<script setup>`)

**Primary Dependencies**:
- Backend: ASP.NET Core Minimal APIs, Entity Framework Core, Npgsql, FluentValidation, FluentAssertions, Testcontainers PostgreSql.
- Frontend: Vite, Vue Router, `@tanstack/vue-query`, Vitest, `@vue/test-utils`, `happy-dom`.

**Storage**: PostgreSQL 16 com schema dedicado `planning` (tabela `planning.tasks`).

**Testing**:
- Backend: xUnit para testes unitários de domínio/aplicação, Testcontainers para testes de integração com banco PostgreSQL real e `WebApplicationFactory` para testes E2E da API.
- Frontend: Vitest com testes unitários e de integração de componentes/composables.

**Target Platform**: Web application (Desktop e Mobile responsivo a partir de 320px).

**Project Type**: Monólito Modular (.NET 10 Web App + Vue 3 SPA).

**Performance Goals**:
- Captura rápida de tarefa na Inbox < 1s.
- Listagem e transição de status em tela < 1s.

**Constraints**:
- Transições de estado pertencem estritamente ao backend.
- O frontend não força nem simula estados localmente.
- Nenhuma tarefa mockada em produção.
- Sem implementação antecipada de `Projects`, `Habits`, `DailyPlan` ou `Calendar` avançado nesta feature.

**Scale/Scope**: Módulo `Planning` estruturado para receber posteriormente o motor de planejamento diário.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Verificação | Status |
|---|---|---|
| 1. Monólito Modular (Planning, Calendar, Execution) | Criação da fundação de `Planning` em 5 projetos isolados. | ✅ Aprovado |
| 2. Host é apenas composition root | Host registra apenas DI e mapeia Minimal APIs de Planning. | ✅ Aprovado |
| 3. Isolamento de DbContext / Schema | Schema dedicado `planning` no PostgreSQL; nenhum acesso a outro módulo. | ✅ Aprovado |
| 4. Comunicação síncrona cross-module via Contracts | `Compass.Modules.Planning.Contracts` e interface `IPlanningModule`. | ✅ Aprovado |
| 5. Sem HTTP interno | Comunicação entre módulos somente via DI e contratos. | ✅ Aprovado |
| 6. Application usa Command/Query + Handler | CQRS puro sem sufixos `UseCase` ou `CommandService`. | ✅ Aprovado |
| 12. Backend é a fonte da verdade | Validações e transições executadas exclusivamente no domínio do backend. | ✅ Aprovado |
| 13. Estado remoto via TanStack Vue Query | Gerenciamento de tarefas no frontend sem Pinia desnecessário. | ✅ Aprovado |
| 14. IDs de agregados via UUIDv7 no backend | `Guid.CreateVersion7()` em `Task.Create`. | ✅ Aprovado |
| 24. FSD pragmático no frontend | `shared`, `entities`, `features`, `pages` sem pastas vazias. | ✅ Aprovado |
| 25. HTML semântico e acessibilidade por teclado | Formulários e cards totalmente navegáveis por teclado. | ✅ Aprovado |

---

## Project Structure

### Documentation (this feature)

```text
specs/002-planning-inbox/
├── spec.md              # Feature specification
├── plan.md              # Implementation plan (this file)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── planning-api.md  # HTTP API contracts
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (/speckit-tasks)
```

### Source Code Layout

```text
Compass.slnx
src/
├── Host/
│   └── Compass.Host/                                 # Composition Root
└── Modules/
    ├── Calendar/                                     # Módulo Calendar (Feature 001)
    └── Planning/                                     # Módulo Planning (Feature 002)
        ├── Compass.Modules.Planning.Contracts/       # DTOs e IPlanningModule
        ├── Compass.Modules.Planning.Domain/          # Agregado Task, Enum TaskStatus, Repositories
        ├── Compass.Modules.Planning.Application/     # Commands, Queries, Handlers CQRS
        ├── Compass.Modules.Planning.Infrastructure/  # PlanningDbContext, Configurations, Repositories
        └── Compass.Modules.Planning.Presentation/    # Minimal APIs (PlanningEndpoints)

tests/
├── Compass.Modules.Planning.Domain.UnitTests/
├── Compass.Modules.Planning.Application.UnitTests/
├── Compass.Modules.Planning.IntegrationTests/
└── Compass.Host.IntegrationTests/

frontend/
└── src/
    ├── app/
    │   └── router/index.ts                           # Rota /planning
    ├── shared/
    │   └── ui/                                       # Componentes base
    ├── entities/
    │   └── task/                                     # Types, API, Composables Vue Query
    ├── features/
    │   └── planning-inbox/                           # QuickTaskCapture, TaskCard, TaskFilterTabs
    └── pages/
        └── planning/                                 # PlanningPage.vue
```

---

## Complexity Tracking

> Nenhuma violação constitucional detectada.
