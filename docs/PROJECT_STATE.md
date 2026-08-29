# Compass V2 - Project State

## Visão Atual do Produto
O Compass é um sistema modular para gerenciamento de calendário, planejamento diário estruturado e execução guiada de atividades, projetado como um Monólito Modular com forte governança temporal, arquitetural e visual.

---

## Stack Tecnológica
- **Backend**: .NET 10, C# 13, ASP.NET Core Minimal APIs, Entity Framework Core, PostgreSQL (schemas `calendar` e `planning`), UUIDv7 (`Guid.CreateVersion7()`).
- **Testes Backend**: xUnit, FluentAssertions, Moq, Testcontainers PostgreSQL, `Microsoft.AspNetCore.Mvc.Testing`.
- **Frontend**: Vue 3 (Composition API / `<script setup>`), TypeScript estrito, Vite, Vue Router, `@tanstack/vue-query`, tokens CSS neutros e semânticos (`tokens.css`).
- **Testes Frontend**: Vitest, `@vue/test-utils`, `happy-dom`.

---

## Arquitetura e Fronteiras
- **Monólito Modular**: Três módulos conceituais: `Calendar`, `Planning` e `Execution`.
- **Host**: Apenas composition root (`src/Host/Compass.Host`), registro de DI e mapeamento de endpoints HTTP.
- **Isolamento Modular**: Módulos não acessam diretamente DbContext, tabelas, domínio ou infraestrutura de outros módulos. Comunicação síncrona exclusivamente via projetos `Contracts` e DI.
- **Calendar**: Único proprietário de timezone, DST, data civil, disponibilidade e conversão UTC.
- **Planning**: Proprietário do ciclo de vida de tarefas, estimativas, deadlines e futuro motor de geração de DailyPlan.
- **Design System & UI Governance**: Interface sóbria, densa e funcional inspirada na estrutura do GitHub e no ritmo do Notion, com tolerância zero a emojis, 0 cores hardcoded, contraste WCAG AA e App Shell compartilhado.

---

## Nomenclatura Oficial
- **ScheduleProfile**: Agregado raiz que define fuso IANA e grade semanal de disponibilidade.
- **DayAvailabilityRule**: Entidade de disponibilidade associada a um dia da semana (`DayOfWeek`).
- **TimeWindow**: Value Object de intervalo contínuo diário (`StartTime < EndTime`).
- **Task**: Agregado raiz de planejamento com status `Draft`, `Ready`, `InProgress`, `Done`.
- **Commands / Queries**: Padrão CQRS puro com sufixo `CommandHandler` / `QueryHandler` (sem sufixos `UseCase` ou `CommandService`).

---

## Features Concluídas
- `001-initial-onboarding`: Onboarding inicial em 5 etapas, configuração de timezone IANA, disponibilidade semanal padrão, persistência no PostgreSQL, restauração pós-F5 e recuperação de 404.
- `002-planning-inbox`: Planning Inbox inicial, captura rápida de tarefas (nascendo como `Draft`), filtros por status (`Draft`, `Ready`, `InProgress`, `Done`), refinamento de estimativa em minutos (com promoção para `Ready`), edição de detalhes, ciclo de vida e persistência no schema PostgreSQL `planning`.
- `003-frontend-visual-foundation`: Fundação visual unificada e migração completa de todas as telas (Onboarding, Hoje, Planning, NotFound 404). Estabelece `tokens.css`, `AppShell`, `AppHeader`, componentes base em `shared/ui`, eliminação de 100% dos emojis, foco acessível (`:focus-visible`), responsividade mobile (320px+) e documentação canônica em `docs/design/FRONTEND_DESIGN_SYSTEM.md`.

---

## Feature Ativa
*Nenhuma no momento* (Aguardando definição da próxima feature).

---

## Módulos Existentes
- **Calendar**:
  - `src/Modules/Calendar/Compass.Modules.Calendar.Contracts`
  - `src/Modules/Calendar/Compass.Modules.Calendar.Domain`
  - `src/Modules/Calendar/Compass.Modules.Calendar.Application`
  - `src/Modules/Calendar/Compass.Modules.Calendar.Infrastructure`
  - `src/Modules/Calendar/Compass.Modules.Calendar.Presentation`
- **Planning**:
  - `src/Modules/Planning/Compass.Modules.Planning.Contracts`
  - `src/Modules/Planning/Compass.Modules.Planning.Domain`
  - `src/Modules/Planning/Compass.Modules.Planning.Application`
  - `src/Modules/Planning/Compass.Modules.Planning.Infrastructure`
  - `src/Modules/Planning/Compass.Modules.Planning.Presentation`
- **Host**:
  - `src/Host/Compass.Host`

---

## Contratos Importantes
- `ICalendarModule`: Interface cross-module para consulta de `ScheduleProfileDto`.
- `IPlanningModule`: Interface cross-module para consulta de `TaskDto` e tarefas em status `Ready`.
- DTOs exportados: `ScheduleProfileDto`, `DayAvailabilityDto`, `TimeWindowDto`, `TimeZoneItemDto`, `TaskDto`, `CreateTaskRequest`, `UpdateTaskRequest`.

---

## Migrations Atuais
- Schema `calendar`:
  - `calendar.schedule_profiles` (`"Id"` uuid PK, `time_zone_id` varchar(100), `created_at` timestamptz, `updated_at` timestamptz)
  - `calendar.day_availability_rules` (`"Id"` uuid PK, `schedule_profile_id` uuid FK, `day_of_week` integer, `windows` jsonb)
- Schema `planning`:
  - `planning.tasks` (`"Id"` uuid PK, `title` varchar(255), `description` text, `duration_minutes` integer, `deadline` timestamptz, `status` varchar(50), `created_at` timestamptz, `updated_at` timestamptz, `completed_at` timestamptz)

---

## Telas e Rotas Atuais
- `/`: Redirecionamento condicional baseado na presença de perfil ativo.
- `/onboarding`: Assistente em 5 etapas com layout sóbrio (`Apresentação`, `Fuso Horário`, `Disponibilidade`, `Confirmação`, `Redirecionamento`).
- `/today`: Painel com `AppShell`, fuso ativo, data civil, disponibilidade diária e grade semanal padronizada.
- `/planning`: Planning Inbox com `AppShell`, captura rápida, abas de status com contadores, estimativas rápidas, ações de ciclo de vida e `AppModal` de edição.
- `/:pathMatch(.*)*`: Rota 404 (`NotFoundPage.vue`) integrada ao `AppShell`.

---

## Estado dos Testes
- **Backend (.NET)**: **62/62 testes aprovados** (36 Domínio, 10 Aplicação, 2 Integração Testcontainers PostgreSQL, 14 API WebApplicationFactory).
- **Frontend (Vitest)**: **38/38 testes aprovados** em 12 arquivos (`AppShell`, `NotFoundPage`, `TodayPage`, `SharedComponents`, `TaskLifecycle`, `PlanningPage`, `OnboardingWizard`, `PlanningInbox`, `authGuard`, `TaskEstimate`, `StepAvailabilityValidation`, `PlanningFeedback`).
- **Auditorias Estáticas**: **0 emojis** e **0 cores hex hardcoded** em componentes `.vue`.
- **Build**: Compilação .NET e `npm run build` (`vue-tsc -b && vite build`) concluídos com **0 erros**.

---

## Bloqueadores
*Nenhum bloqueador ativo.*

---

## Débitos Aceitos
- Amarração de perfil único baseada em `localStorage` (`compass_active_profile_id`), adequada para fase inicial desktop/single-user. Suporte a múltiplos perfis/usuários será tratado em feature futura.
- Ordenação de tarefas na Inbox utiliza data de criação; drag-and-drop manual será avaliado em incrementos futuros.
- Seletor manual em tempo de execução para alternância Claro/Escuro não implementado no frontend (tokens preparados para ativação futura).

---

## Próximo Passo Exato
Executar `/speckit-specify` para planejar a integração entre os módulos `Calendar` e `Planning`: **geração de sugestões e criação do DailyPlan diário**.
