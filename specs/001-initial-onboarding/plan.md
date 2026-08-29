# Implementation Plan: Onboarding Inicial do Compass

**Branch**: `001-initial-onboarding` | **Date**: 2026-08-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from [`specs/001-initial-onboarding/spec.md`](./spec.md)

## Summary

Implementar o fluxo de onboarding inicial e a fundação do módulo `Calendar` do Compass V2. O sistema permitirá a um novo usuário configurar seu fuso horário IANA e sua disponibilidade semanal padrão através de um assistente em 5 etapas no frontend Vue 3. O backend .NET 10 processa a criação do `ScheduleProfile`, valida os dados e persiste as regras no PostgreSQL. O frontend armazena apenas o identificador para recuperar o perfil em acessos subsequentes (F5), redirecionando o usuário para a tela "Hoje".

## Technical Context

**Language/Version**: .NET 10 (C# 14), TypeScript 5.x estrito, Node.js 20+

**Primary Dependencies**:
- Backend: ASP.NET Core Minimal APIs, EF Core 10 (Npgsql.EntityFrameworkCore.PostgreSQL), FluentValidation
- Frontend: Vue 3 (Composition API, `<script setup>`), Vite, Vue Router 4, `@tanstack/vue-query`

**Storage**: PostgreSQL 16+ (Schema dedicado `calendar`)

**Testing**:
- Backend Unit: xUnit, FluentAssertions
- Backend Integration: Testcontainers (`Testcontainers.PostgreSql`), `Microsoft.AspNetCore.Mvc.Testing`
- Frontend: Vitest, `@vue/test-utils`, `happy-dom`

**Target Platform**: Web application (Backend Linux/Windows Container, Frontend Web SPA)

**Project Type**: Monólito Modular (Backend Web API + Frontend Single Page Application)

**Performance Goals**:
- Criação e consulta de perfil < 100ms no backend.
- Restauração de sessão e renderização inicial no frontend < 500ms.

**Constraints**:
- Host é apenas composition root; sem regras de negócio no Host.
- Módulos isolados por schema de banco e projetos separados.
- Fuso e disponibilidade centralizados exclusivamente no módulo `Calendar`.
- Frontend não duplica regras de negócio e utiliza Vue Query para estado remoto.
- Sem mocks em produção e sem digitação manual de identificadores.

**Scale/Scope**: Instalação local/single-tenant inicial, expansível para múltiplos perfis e agendamentos.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Justificativa / Conformidade |
|--------------------------|--------|------------------------------|
| 1. Monólito Modular (Planning, Calendar, Execution) | PASS | Feature implementa o módulo `Calendar`. Módulos `Planning` e `Execution` não são criados antecipadamente. |
| 2. Host como Composition Root | PASS | `Compass.Host` apenas registra os módulos, middlewares e rotas via Presentation extensions. |
| 3. Isolamento de Dados e Módulos | PASS | `CalendarDbContext` opera exclusivamente no schema `calendar`. |
| 4. Comunicação Cross-Module via Contracts | PASS | `Compass.Modules.Calendar.Contracts` exporta interfaces e DTOs para integração futura. |
| 5. Sem HTTP Interno | PASS | Comunicação é in-memory via DI. |
| 6. Application usa Command/Query + Handler | PASS | Handlers dedicados (`CreateScheduleProfileCommandHandler`, `GetScheduleProfileByIdQueryHandler`), sem sufixos `UseCase`/`CommandService`. |
| 7. Calendar é proprietário único de tempo/fuso | PASS | Timezones IANA e janelas de disponibilidade pertencem e são validados exclusivamente em `Calendar`. |
| 8. DateTimeOffset em UTC em contratos | PASS | Marcas temporais exportadas com `DateTimeOffset` UTC (`Offset == TimeSpan.Zero`). |
| 9. DateOnly para dia civil | PASS | Dias civis e regras representados sem distorções de fuso. |
| 10. Distinção de entidades de domínio | PASS | `ScheduleProfile` modelado de forma isolada e expressiva. |
| 11. Break sem ReferenceId | PASS | N/A nesta feature. |
| 12. Backend é a fonte da verdade | PASS | Backend valida timezones e unifica janelas de disponibilidade; frontend não duplica regras. |
| 13. Vue Query para estado remoto | PASS | Consultas de perfil e mutações usam `@tanstack/vue-query`. Estado local restrito a rascunhos de UI. |
| 14. IDs gerados pelo backend | PASS | `ScheduleProfile.Id` gerado pelo backend (UUIDv7). |
| 15. Migrations aditivas e testadas | PASS | Migrations EF Core validadas em banco vazio via Testcontainers. |
| 16. Sem fallback de dados fictícios | PASS | Ausência de perfil redireciona ao onboarding; nenhum perfil mockado é injetado. |
| 17. Cenários de aceitação prévios | PASS | Cenários definidos formalmente em `spec.md`. |
| 18. Fatiamento vertical (Full-Slice) | PASS | Abrange Domain, Application, Infrastructure, Presentation, Frontend e Testes. |
| 19. Gate de planejamento aprovado | PASS | Artefatos `spec.md`, `plan.md`, `research.md`, `data-model.md` e `contracts/` gerados. |
| 20. Análise de consistência e convergência | PASS | Será executado `speckit-analyze` e `speckit-converge`. |
| 21. Sem refatorações fora de escopo | PASS | Foco exclusivo na entrega do onboarding inicial. |
| 22. Compilação não é critério de conclusão | PASS | Conclusão condicionada à aprovação dos testes de integração, API e frontend. |
| 23. Critérios de DoD estritos | PASS | Testes automatizados, build limpo e validação dos cenários de aceitação. |
| 24. Vue 3 + TypeScript Estrito + FSD | PASS | FSD pragmático sem pastas vazias, `<script setup>` e TypeScript estrito. |
| 25. HTML semântico, teclado e tokens CSS | PASS | Controles acessíveis por teclado, foco visível, tokens de tema CSS neutros. |

## Project Structure

### Documentation (this feature)

```text
specs/001-initial-onboarding/
├── spec.md              # Especificação de requisitos e cenários de aceitação
├── checklists/
│   └── requirements.md  # Checklist de qualidade da especificação
├── plan.md              # Este plano de implementação
├── research.md          # Decisões técnicas e arquiteturais (Fase 0)
├── data-model.md        # Modelo de domínio, entidades e esquema PostgreSQL (Fase 1)
├── quickstart.md        # Guia de validação e execução ponta a ponta (Fase 1)
└── contracts/
    └── calendar-api.md  # Contratos de endpoints HTTP e schemas (Fase 1)
```

### Source Code Layout

```text
compass-v2/
├── Compass.sln
├── src/
│   ├── Host/
│   │   └── Compass.Host/
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── Compass.Host.csproj
│   └── Modules/
│       └── Calendar/
│           ├── Compass.Modules.Calendar.Contracts/
│           │   ├── DTOs/
│           │   │   └── ScheduleProfileDto.cs
│           │   ├── ICalendarModule.cs
│           │   └── Compass.Modules.Calendar.Contracts.csproj
│           ├── Compass.Modules.Calendar.Domain/
│           │   ├── Model/
│           │   │   ├── ScheduleProfile.cs
│           │   │   ├── DayAvailabilityRule.cs
│           │   │   ├── TimeWindow.cs
│           │   │   └── TimeZoneId.cs
│           │   ├── Exceptions/
│           │   │   └── CalendarDomainException.cs
│           │   └── Compass.Modules.Calendar.Domain.csproj
│           ├── Compass.Modules.Calendar.Application/
│           │   ├── Abstractions/
│           │   │   ├── ICommand.cs
│           │   │   ├── ICommandHandler.cs
│           │   │   ├── IQuery.cs
│           │   │   └── IQueryHandler.cs
│           │   ├── Commands/
│           │   │   ├── CreateScheduleProfileCommand.cs
│           │   │   └── CreateScheduleProfileCommandHandler.cs
│           │   ├── Queries/
│           │   │   ├── GetScheduleProfileByIdQuery.cs
│           │   │   └── GetScheduleProfileByIdQueryHandler.cs
│           │   └── Compass.Modules.Calendar.Application.csproj
│           ├── Compass.Modules.Calendar.Infrastructure/
│           │   ├── Persistence/
│           │   │   ├── CalendarDbContext.cs
│           │   │   ├── Configurations/
│           │   │   │   └── ScheduleProfileConfiguration.cs
│           │   │   └── Repositories/
│           │   │       └── ScheduleProfileRepository.cs
│           │   ├── Migrations/
│           │   └── Compass.Modules.Calendar.Infrastructure.csproj
│           └── Compass.Modules.Calendar.Presentation/
│               ├── Endpoints/
│               │   └── CalendarEndpoints.cs
│               ├── Extensions/
│               │   └── CalendarModuleExtensions.cs
│               └── Compass.Modules.Calendar.Presentation.csproj
├── tests/
│   ├── Compass.Modules.Calendar.Domain.UnitTests/
│   │   ├── TimeWindowTests.cs
│   │   └── ScheduleProfileTests.cs
│   ├── Compass.Modules.Calendar.Application.UnitTests/
│   │   ├── CreateScheduleProfileCommandHandlerTests.cs
│   │   └── GetScheduleProfileByIdQueryHandlerTests.cs
│   ├── Compass.Modules.Calendar.IntegrationTests/
│   │   ├── CalendarDbContextTests.cs
│   │   └── TestDatabaseFixture.cs
│   └── Compass.Host.IntegrationTests/
│       ├── CalendarApiTests.cs
│       └── CustomWebApplicationFactory.cs
└── frontend/
    ├── package.json
    ├── tsconfig.json
    ├── vite.config.ts
    ├── index.html
    └── src/
        ├── app/
        │   ├── App.vue
        │   ├── main.ts
        │   ├── router/
        │   │   └── index.ts
        │   └── styles/
        │       └── tokens.css
        ├── pages/
        │   ├── onboarding/
        │   │   └── OnboardingPage.vue
        │   └── today/
        │       └── TodayPage.vue
        ├── features/
        │   └── onboarding/
        │       ├── components/
        │       │   ├── StepPresentation.vue
        │       │   ├── StepTimeZone.vue
        │       │   ├── StepAvailability.vue
        │       │   └── StepConfirmation.vue
        │       └── model/
        │           └── onboardingState.ts
        ├── entities/
        │   └── schedule-profile/
        │       ├── api/
        │       │   ├── scheduleProfileApi.ts
        │       │   └── types.ts
        │       ├── model/
        │       │   ├── useScheduleProfileQuery.ts
        │       │   ├── useCreateScheduleProfileMutation.ts
        │       │   └── profileStorage.ts
        └── shared/
            └── ui/
                ├── AppButton.vue
                ├── AppInput.vue
                ├── AppSelect.vue
                └── TimeRangePicker.vue
```

**Structure Decision**: Monólito modular com fatiamento vertical para o módulo `Calendar`. Estrutura de frontend FSD pragmática sem pastas vazias, contendo apenas o que é essencial para o onboarding e a tela "Hoje".

## Complexity Tracking

> Nenhuma violação constitucional detectada. Todos os padrões selecionados estão estritamente alinhados à Constituição do Compass V2.
