# Handoff: 001-initial-onboarding (Onboarding Inicial do Compass)

- **ID da Spec**: `001-initial-onboarding`
- **Nome da Feature**: Onboarding Inicial do Compass
- **Data**: 2026-08-28
- **Branch / Worktree**: `master` (branch de feature correspondente: `001-initial-onboarding`)
- **Status**: Concluída

---

## 1. Problema Resolvido

Um usuário que iniciava uma instalação sem perfil configurado não possuía um ponto de entrada para definir seu fuso horário IANA e sua disponibilidade semanal padrão. Sem essas informações temporais fundamentadas pelo módulo `Calendar`, o sistema não conseguia calcular datas civis nem ancorar janelas de tempo sem inventar disponibilidade artificial.

---

## 2. Comportamento Entregue ao Usuário

- **Primeiro Acesso**: Ao abrir a aplicação sem sessão, o usuário é automaticamente direcionado ao assistente de onboarding em 5 etapas:
  1. *Apresentação*: Introdução curta às capacidades do Compass.
  2. *Fuso Horário*: Seleção de fuso horário IANA (com pré-seleção baseada no fuso do navegador e filtro de busca).
  3. *Disponibilidade Semanal*: Configuração dia a dia (Segunda a Domingo) com habilitação de dias e adição/remoção de intervalos de horário com validação inline.
  4. *Confirmação*: Revisão clara dos dados antes de submeter ao servidor.
  5. *Redirecionamento*: Criação do perfil no backend, armazenamento exclusivo do identificador no `localStorage` do cliente e redirecionamento para a tela `Hoje` (`/today`).
- **Persistência pós-F5**: Ao recarregar a página ou revisitar a aplicação, o frontend recupera fuso horário e disponibilidade reais do backend a partir do identificador armazenado.
- **Recuperação de Erro (404)**: Se o identificador local for inválido ou não existir no banco de dados, o armazenamento local é limpo e o usuário retorna ao onboarding.
- **Acessibilidade e Responsividade**: Toda a navegação funciona por teclado e o layout se adapta a telas móveis (320px+).

---

## 3. Critérios de Aceitação Atendidos

- [x] **US1 / AC1-AC4**: Exibição do onboarding em 5 etapas no primeiro acesso, seleção de fuso IANA, validação de intervalos válidos, criação do `ScheduleProfile` no backend com UUIDv7 e redirecionamento para `/today`.
- [x] **US2 / AC1-AC2**: Restauração automática de sessão a partir do identificador local após F5, recuperando timezone e disponibilidade reais do backend.
- [x] **US3 / AC1-AC2**: Limpeza de identificador inexistente/inválido com retorno seguro ao onboarding; proteção de rotas privadas via router guard.
- [x] **US4 / AC1-AC3**: Rejeição de timezone inválido (400 Bad Request), bloqueio de janelas onde `StartTime >= EndTime`, e unificação determinística de intervalos sobrepostos ou adjacentes no mesmo dia.
- [x] **SC-001 a SC-005**: 100% dos critérios de sucesso atingidos.

### Critérios Não Atendidos
*Nenhum*. Todos os critérios especificados em `specs/001-initial-onboarding/spec.md` foram 100% atendidos.

---

## 4. Decisões de Domínio

1. **`TimeZoneId`**: Encapsulado como Value Object com validação estrita contra a base IANA (`TimeZoneInfo.FindSystemTimeZoneById`).
2. **`TimeWindow`**: Value Object representando intervalo contínuo diário (`TimeOnly StartTime`, `TimeOnly EndTime`), com invariante imutável `StartTime < EndTime`.
3. **Unificação Determinística**: Método `TimeWindow.Normalize(IEnumerable<TimeWindow>)` ordena intervalos por horário inicial e funde qualquer sobreposição ou contiguidade (`w1.EndTime >= w2.StartTime`).
4. **Agregado `ScheduleProfile`**: Raiz de agregação contendo `TimeZoneId`, `WeeklyAvailability` (`IReadOnlyList<DayAvailabilityRule>`), `CreatedAt` e `UpdatedAt` em UTC. Identificador gerado via UUIDv7 (`Guid.CreateVersion7()`).

---

## 5. Decisões Arquiteturais

1. **Monólito Modular**: Implementado apenas o módulo `Calendar` e a composition root `Compass.Host`, sem adiantar `Planning` e `Execution`.
2. **Isolamento de Camadas**: `Domain` puro sem dependências externas; `Application` com abstrações CQRS (`ICommand`, `ICommandHandler`, `IQuery`, `IQueryHandler`); `Infrastructure` com `CalendarDbContext` e repositórios; `Presentation` com Minimal APIs.
3. **Persistência PostgreSQL**: Schema isolado `calendar`. Tabelas `schedule_profiles` e `day_availability_rules` (janelas persistidas em coluna `jsonb` com `ValueComparer` dedicado no EF Core).
4. **Frontend FSD Pragmático**: Separação clara entre `shared/ui`, `entities/schedule-profile`, `features/onboarding`, `pages/onboarding` e `pages/today`. Gerenciamento remoto via `@tanstack/vue-query` sem necessidade de Pinia.

---

## 6. Entidades e Invariantes

- `ScheduleProfile`: Agregado raiz (`Id: Guid` UUIDv7, `TimeZone: TimeZoneId`, `WeeklyAvailability: List<DayAvailabilityRule>`, `CreatedAt: DateTimeOffset`, `UpdatedAt: DateTimeOffset`).
- `DayAvailabilityRule`: Entidade interna (`Id: Guid`, `ScheduleProfileId: Guid`, `DayOfWeek: DayOfWeek`, `Windows: IReadOnlyList<TimeWindow>`).
- Invariantes:
  - `TimeZoneId` não nulo e pertencente à base IANA válida.
  - Em cada `TimeWindow`, `StartTime < EndTime`.
  - Janelas do mesmo dia da semana são unificadas sem sobreposições.

---

## 7. Commands, Queries e Handlers

- `CreateScheduleProfileCommand(string TimeZoneId, IReadOnlyList<DayAvailabilityDto>? WeeklyAvailability)` -> `CreateScheduleProfileCommandHandler` : `ICommandHandler<CreateScheduleProfileCommand, ScheduleProfileDto>`
- `GetScheduleProfileByIdQuery(Guid Id)` -> `GetScheduleProfileByIdQueryHandler` : `IQueryHandler<GetScheduleProfileByIdQuery, ScheduleProfileDto?>`

---

## 8. Contratos Cross-Module

- `ICalendarModule`:
  - `Task<ScheduleProfileDto?> GetProfileByIdAsync(Guid profileId, CancellationToken cancellationToken = default)`
- DTOs exportados: `ScheduleProfileDto`, `DayAvailabilityDto`, `TimeWindowDto`, `TimeZoneItemDto`.

---

## 9. Endpoints HTTP

| Método | Rota | Request Body | Response Body | Status Codes |
|---|---|---|---|---|
| `POST` | `/api/calendar/schedule-profiles` | `CreateScheduleProfileCommand` | `ScheduleProfileDto` | `201 Created` (`Location: /api/calendar/schedule-profiles/{id}`), `400 Bad Request` |
| `GET` | `/api/calendar/schedule-profiles/{id:guid}` | *(None)* | `ScheduleProfileDto` | `200 OK`, `404 Not Found` |
| `GET` | `/api/calendar/timezones` | *(None)* | `IReadOnlyList<TimeZoneItemDto>` | `200 OK` |

---

## 10. Tabelas e Migrations

- **Schema**: `calendar`
- **Tabelas**:
  - `calendar.schedule_profiles` (`"Id"` uuid PK, `time_zone_id` varchar(100), `created_at` timestamptz, `updated_at` timestamptz)
  - `calendar.day_availability_rules` (`"Id"` uuid PK, `schedule_profile_id` uuid FK -> `schedule_profiles.Id`, `day_of_week` integer, `windows` jsonb)

---

## 11. Rotas e Telas Frontend

- `/` -> Redirecionamento condicional (`hasActiveProfile()` ? `/today` : `/onboarding`)
- `/onboarding` (`OnboardingPage.vue`): Assistente de 5 etapas.
- `/today` (`TodayPage.vue`): Painel com fuso ativo, data civil local, disponibilidade diária e grade semanal.
- **Router Guard**: Intercepta rotas privadas e força redirecionamento caso não haja perfil configurado.

---

## 12. Query Keys e Invalidações

- `['schedule-profile', activeId]`: Chave do TanStack Vue Query para busca e cache do perfil ativo.
- `useCreateScheduleProfileMutation`: Popula `queryClient.setQueryData(['schedule-profile', profile.id], profile)` no sucesso.

---

## 13. Arquivos Criados, Modificados e Removidos

### Backend Criado:
- `Compass.slnx`
- `src/Host/Compass.Host/Program.cs`, `Compass.Host.csproj`
- `src/Modules/Calendar/Compass.Modules.Calendar.Contracts/DTOs/ScheduleProfileDto.cs`, `ICalendarModule.cs`, `Compass.Modules.Calendar.Contracts.csproj`
- `src/Modules/Calendar/Compass.Modules.Calendar.Domain/Exceptions/CalendarDomainException.cs`, `Model/TimeZoneId.cs`, `Model/TimeWindow.cs`, `Model/DayAvailabilityRule.cs`, `Model/ScheduleProfile.cs`, `Repositories/IScheduleProfileRepository.cs`, `Compass.Modules.Calendar.Domain.csproj`
- `src/Modules/Calendar/Compass.Modules.Calendar.Application/Abstractions/ICommand.cs`, `IQuery.cs`, `Commands/CreateScheduleProfileCommand.cs`, `Commands/CreateScheduleProfileCommandHandler.cs`, `Queries/GetScheduleProfileByIdQuery.cs`, `Queries/GetScheduleProfileByIdQueryHandler.cs`, `CalendarApplicationExtensions.cs`, `Compass.Modules.Calendar.Application.csproj`
- `src/Modules/Calendar/Compass.Modules.Calendar.Infrastructure/Persistence/CalendarDbContext.cs`, `Persistence/Configurations/ScheduleProfileConfiguration.cs`, `Persistence/Configurations/DayAvailabilityRuleConfiguration.cs`, `Persistence/Repositories/ScheduleProfileRepository.cs`, `CalendarInfrastructureExtensions.cs`, `Compass.Modules.Calendar.Infrastructure.csproj`
- `src/Modules/Calendar/Compass.Modules.Calendar.Presentation/Endpoints/CalendarEndpoints.cs`, `Extensions/CalendarModuleExtensions.cs`, `Compass.Modules.Calendar.Presentation.csproj`

### Backend Testes Criado:
- `tests/Compass.Modules.Calendar.Domain.UnitTests/ScheduleProfileTests.cs`, `TimeWindowTests.cs`, `TimeZoneIdTests.cs`, `Compass.Modules.Calendar.Domain.UnitTests.csproj`
- `tests/Compass.Modules.Calendar.Application.UnitTests/CreateScheduleProfileCommandHandlerTests.cs`, `GetScheduleProfileByIdQueryHandlerTests.cs`, `Compass.Modules.Calendar.Application.UnitTests.csproj`
- `tests/Compass.Modules.Calendar.IntegrationTests/TestDatabaseFixture.cs`, `ScheduleProfilePersistenceTests.cs`, `Compass.Modules.Calendar.IntegrationTests.csproj`
- `tests/Compass.Host.IntegrationTests/CustomWebApplicationFactory.cs`, `CreateScheduleProfileApiTests.cs`, `GetScheduleProfileApiTests.cs`, `ScheduleProfileValidationApiTests.cs`, `Compass.Host.IntegrationTests.csproj`

### Frontend Criado:
- `frontend/package.json`, `tsconfig.json`, `vite.config.ts`, `index.html`
- `frontend/src/app/App.vue`, `main.ts`, `styles/tokens.css`, `router/index.ts`, `router/__tests__/authGuard.spec.ts`
- `frontend/src/shared/ui/AppButton.vue`, `AppInput.vue`, `AppSelect.vue`, `TimeRangePicker.vue`
- `frontend/src/entities/schedule-profile/api/types.ts`, `api/scheduleProfileApi.ts`, `model/profileStorage.ts`, `model/useCreateScheduleProfileMutation.ts`, `model/useScheduleProfileQuery.ts`
- `frontend/src/features/onboarding/model/onboardingState.ts`, `components/StepPresentation.vue`, `components/StepTimeZone.vue`, `components/StepAvailability.vue`, `components/StepConfirmation.vue`, `__tests__/OnboardingWizard.spec.ts`, `__tests__/StepAvailabilityValidation.spec.ts`
- `frontend/src/pages/onboarding/OnboardingPage.vue`
- `frontend/src/pages/today/TodayPage.vue`, `pages/today/__tests__/TodayPage.spec.ts`

### Arquivos Modificados / Removidos:
- Nenhum arquivo de produção foi removido.

---

## 14. Testes Adicionados e Resultados dos Quality Gates

### Backend (.NET 10 / xUnit / Testcontainers PostgreSQL)
- **Comando**: `dotnet test Compass.slnx`
- **Resultado**:
  - `Compass.Modules.Calendar.Domain.UnitTests`: 16 aprovados (0 falhas)
  - `Compass.Modules.Calendar.Application.UnitTests`: 4 aprovados (0 falhas)
  - `Compass.Modules.Calendar.IntegrationTests`: 1 aprovado (0 falhas)
  - `Compass.Host.IntegrationTests`: 7 aprovados (0 falhas)
  - **Total Backend**: 28 aprovados, 0 falhas.

### Frontend (Vitest / TypeScript Estrito / Vite)
- **Comando de Testes**: `npm test -- --run`
- **Resultado**: 4 arquivos de teste, 14 aprovados, 0 falhas.
- **Comando de Build**: `npm run build` (`vue-tsc -b && vite build`)
- **Resultado**: 0 erros de tipagem, build gerado com sucesso em `dist/`.

---

## 15. Divergências entre Spec, Plan, Tasks e Implementação

Nenhuma divergência identificada. Todas as 46 tarefas de `specs/001-initial-onboarding/tasks.md` foram implementadas e validadas conforme `spec.md` e `plan.md`.

---

## 16. Débitos Técnicos e Riscos Conhecidos

- **Débito Aceito**: Por ser uma instalação local/single-user neste momento, a amarração da sessão é baseada na chave local `compass_active_profile_id`. A introdução de múltiplos perfis ou autenticação multi-tenant será abordada em features futuras quando houver especificação explícita.
- **Risco Conhecido**: Ambientes sem Docker para execução de Testcontainers dependem de configuração prévia de Docker Desktop / daemon do Docker para a execução da suíte de integração no pipeline de CI.

---

## 17. Instruções para Executar a Feature

1. **Subir Backend**:
   ```bash
   dotnet run --project src/Host/Compass.Host
   ```
2. **Subir Frontend**:
   ```bash
   cd frontend
   npm run dev
   ```
3. **Acessar**: `http://localhost:5173/` (será direcionado para `/onboarding` no primeiro acesso).

---

## 18. Próximo Passo Recomendado

Com a fundação do módulo `Calendar` e o onboarding do usuário concluídos, o próximo passo recomendado é a especificação da feature de visualização/gerenciamento do calendário e disponibilidade (`/speckit-specify`), preparando a base temporal para o módulo `Planning`.
