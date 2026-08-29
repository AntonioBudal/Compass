# Research & Technical Decisions: Onboarding Inicial do Compass

**Feature**: `001-initial-onboarding` | **Status**: Approved

## 1. Backend Architecture & Modular Monolith Layout

### Decision
Adotar a estrutura canônica de Monólito Modular em .NET 10 para o módulo `Calendar`, mantendo o `Host` estritamente como composition root e ponto de partida HTTP:
- `src/Host/Compass.Host`: Inicialização, middlewares, DI de módulos, endpoints raiz e Swagger/OpenAPI.
- `src/Modules/Calendar/Compass.Modules.Calendar.Domain`: Agregados (`ScheduleProfile`), Entidades (`DayAvailabilityRule`), Value Objects (`TimeWindow`, `TimeZoneId`), invariantes de negócio e validações puras.
- `src/Modules/Calendar/Compass.Modules.Calendar.Application`: Commands/Queries (`CreateScheduleProfileCommand`, `GetScheduleProfileByIdQuery`), DTOs internos e Handlers (`CreateScheduleProfileCommandHandler`, `GetScheduleProfileByIdQueryHandler`).
- `src/Modules/Calendar/Compass.Modules.Calendar.Infrastructure`: `CalendarDbContext`, configurações de mapeamento EF Core, migrações no schema `calendar`, repositórios de persistência.
- `src/Modules/Calendar/Compass.Modules.Calendar.Presentation`: Registro de endpoints HTTP via ASP.NET Core Minimal APIs (`CalendarEndpoints.MapCalendarEndpoints(...)`).
- `src/Modules/Calendar/Compass.Modules.Calendar.Contracts`: Interfaces e DTOs exportáveis para comunicação síncrona cross-module futura (ex.: `ICalendarModule`).

### Rationale
- Atende integralmente aos Princípios 1, 2, 3, 4, 5 e 6 da Constituição do Compass V2.
- Evita a criação antecipada dos módulos `Planning` e `Execution`, focando na entrega vertical da fundação de `Calendar`.

### Alternatives Considered
- *Criar todos os projetos de Planning e Execution agora*: Rejeitado para evitar sobrecarga de código ocioso e manter o princípio de fatiamento vertical e foco na feature ativa.
- *Usar controladores tradicionais (MVC Controllers)*: Rejeitado em favor de Minimal APIs com métodos de extensão na camada Presentation do módulo, garantindo endpoints leves e coesos.

---

## 2. Command / Query Handler Pattern

### Decision
Utilizar interfaces explícitas e leves de Command/Query + Handler:
```csharp
public interface ICommand<out TResult> { }
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IQuery<out TResult> { }
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

### Rationale
- Respeita o Princípio 6 da Constituição (Command/Query + Handler sem sufixos `UseCase` ou `CommandService`).
- Oferece total clareza e controle de injeção de dependências sem necessidade de adicionar dependências pesadas de bibliotecas de terceiros para mediação.

---

## 3. Timezone Validation & IANA Authority

### Decision
O módulo `Calendar` valida e manipula os fusos horários utilizando a biblioteca padrão `TimeZoneInfo` do .NET 10 (que possui suporte nativo integral a identificadores IANA como `America/Sao_Paulo`, `UTC`, `Europe/London` em todas as plataformas via ICU).
- Validação no domínio: `TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)` ou `TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz)`.
- Se o fuso for nulo, vazio ou inválido, o domínio lança exceção de validação específica (`InvalidTimeZoneException` ou `DomainValidationException`).

### Rationale
- Respeita os Princípios 7 e 8 da Constituição: `Calendar` é o proprietário único da interpretação e validação de fusos e disponibilidade.

---

## 4. Availability Windows Unification & Invariants

### Decision
1. **Invariante de Janela**: Cada `TimeWindow` possui `StartTime` e `EndTime` do tipo `TimeOnly`. A invariante exige obrigatoriamente `StartTime < EndTime`.
2. **Algoritmo de Normalização/Unificação**: Para qualquer lista de janelas de um mesmo dia da semana:
   - Ordenar as janelas por `StartTime`.
   - Iterar sobre a lista: se a janela atual sobrepõe ou é contígua à anterior (`current.StartTime <= previous.EndTime`), fundir em uma única janela `[previous.StartTime, max(previous.EndTime, current.EndTime)]`.
   - Caso contrário, adicionar como uma nova janela separada.
3. Se um dia não tiver janelas ativas, ele é registrado com lista vazia de disponibilidade.

### Rationale
- Garante representação determinística, elimina redundâncias e sobreposições, e atende com precisão ao critério da especificação.

---

## 5. PostgreSQL Persistence & Schema Isolation

### Decision
- Banco de Dados: PostgreSQL 16+.
- Schema privativo: `calendar`.
- Tabelas:
  - `calendar.schedule_profiles`: `id` (uuid v7, PK), `time_zone_id` (varchar(100)), `created_at` (timestamptz), `updated_at` (timestamptz).
  - `calendar.availability_windows`: `id` (uuid, PK), `profile_id` (uuid, FK), `day_of_week` (int / smallint), `start_time` (time), `end_time` (time).
- Migrações: Geradas pelo EF Core (`CalendarDbContextModelSnapshot` e migrations aditivas).

### Rationale
- Respeita os Princípios 3, 14 e 15 da Constituição (isolamento estrito de módulo, ID gerado pelo backend e migrations aditivas).

---

## 6. Frontend Architecture (Vue 3 + FSD Pragmático + Vue Query)

### Decision
- **Camadas FSD** (somente pastas necessárias):
  - `frontend/src/app`: Configuração de plugins (Router, TanStack Query), estilos globais, design tokens e `App.vue`.
  - `frontend/src/pages/onboarding`: `OnboardingPage.vue` (orquestrador de fluxo das 5 etapas).
  - `frontend/src/pages/today`: `TodayPage.vue` (tela de destino com dados reais).
  - `frontend/src/features/onboarding`: Componentes de cada etapa (`StepPresentation.vue`, `StepTimeZone.vue`, `StepAvailability.vue`, `StepConfirmation.vue`), máquina de estado de navegação e validações visuais.
  - `frontend/src/entities/schedule-profile`: Tipos TypeScript (`ScheduleProfile`, `DayAvailability`), API client (`api.ts`), Vue Query composables (`useScheduleProfileQuery`, `useCreateScheduleProfileMutation`), e serviço local `profileStorage.ts` (para salvar/ler apenas o identificador no `localStorage`).
  - `frontend/src/shared/ui`: Componentes acessíveis reutilizáveis (`AppButton.vue`, `AppInput.vue`, `AppSelect.vue`, `TimeRangeInput.vue`, etc.) estilizados com tokens CSS puros.
- **Gerenciamento de Estado**:
  - `Vue Query` gerencia a consulta remota do perfil (`/api/calendar/schedule-profiles/{id}`) e mutação (`POST /api/calendar/schedule-profiles`).
  - `ref`/`reactive` gerenciam apenas o rascunho de dados do formulário nas etapas do onboarding.
  - Sem uso de Pinia.

### Rationale
- Respeita os Princípios 12, 13, 24 e 25 da Constituição (backend como fonte da verdade, estado remoto em Vue Query, sem pastas vazias, acessibilidade e tokens CSS).

---

## 7. Testing Strategy

### Decision
1. **Domain Tests** (`Compass.Modules.Calendar.Domain.UnitTests`):
   - Invariantes de `TimeWindow` (`StartTime < EndTime`).
   - Algoritmo de unificação de janelas sobrepostas e adjacentes.
   - Validação de `TimeZoneId` e criação do `ScheduleProfile`.
2. **Application Tests** (`Compass.Modules.Calendar.Application.UnitTests`):
   - Execução de `CreateScheduleProfileCommandHandler` e `GetScheduleProfileByIdQueryHandler`.
   - Tratamento de perfil não encontrado e validação de comandos.
3. **Integration Tests com Testcontainers** (`Compass.Modules.Calendar.IntegrationTests`):
   - Execução real sobre container PostgreSQL (`PostgreSqlBuilder`).
   - Aplicação de migrations e persistência/leitura de `ScheduleProfile` e janelas de disponibilidade via `CalendarDbContext`.
4. **Host API Tests** (`Compass.Host.IntegrationTests`):
   - Teste de ponta a ponta com `WebApplicationFactory<Program>` validando contratos HTTP, códigos de status (201 Created, 200 OK, 400 Bad Request, 404 Not Found) e cabeçalho `Location`.
5. **Frontend Vitest Tests** (`frontend/src/**/*.spec.ts`):
   - Renderização das 5 etapas do onboarding.
   - Navegação e validação de horários.
   - Comportamento de restauração de sessão com identificador válido vs. identificador inválido.
