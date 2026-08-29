# Compass V2 - Project State

## Visão Atual do Produto
O Compass é um sistema modular para gerenciamento de calendário, planejamento diário estruturado e execução guiada de atividades, projetado como um Monólito Modular com forte governança temporal e arquitetural.

---

## Stack Tecnológica
- **Backend**: .NET 10, C# 13, ASP.NET Core Minimal APIs, Entity Framework Core, PostgreSQL (schema `calendar`), UUIDv7 (`Guid.CreateVersion7()`).
- **Testes Backend**: xUnit, FluentAssertions, Moq, Testcontainers PostgreSQL, `Microsoft.AspNetCore.Mvc.Testing`.
- **Frontend**: Vue 3 (Composition API / `<script setup>`), TypeScript estrito, Vite, Vue Router, `@tanstack/vue-query`, tokens CSS neutros.
- **Testes Frontend**: Vitest, `@vue/test-utils`, `happy-dom`.

---

## Arquitetura e Fronteiras
- **Monólito Modular**: Três módulos conceituais previstos: `Calendar`, `Planning` e `Execution`.
- **Host**: Apenas composition root (`src/Host/Compass.Host`), registro de DI e mapeamento de endpoints HTTP.
- **Isolamento Modular**: Módulos não acessam diretamente DbContext, tabelas, domínio ou infraestrutura de outros módulos. Comunicação síncrona exclusivamente via projetos `Contracts`.
- **Calendar**: Único proprietário de timezone, DST, data civil, disponibilidade e conversão UTC.

---

## Nomenclatura Oficial
- **ScheduleProfile**: Agregado raiz que define fuso IANA e grade semanal de disponibilidade.
- **DayAvailabilityRule**: Entidade de disponibilidade associada a um dia da semana (`DayOfWeek`).
- **TimeWindow**: Value Object de intervalo contínuo diário (`StartTime < EndTime`).
- **Commands / Queries**: Padrão CQRS puro com sufixo `CommandHandler` / `QueryHandler` (sem sufixos `UseCase` ou `CommandService`).

---

## Features Concluídas
- `001-initial-onboarding`: Onboarding inicial em 5 etapas, configuração de timezone IANA, disponibilidade semanal padrão, persistência no PostgreSQL, restauração pós-F5 e recuperação automática de 404.

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
- **Host**:
  - `src/Host/Compass.Host`

---

## Contratos Importantes
- `ICalendarModule`: Interface cross-module para consulta de `ScheduleProfileDto`.
- `ScheduleProfileDto`, `DayAvailabilityDto`, `TimeWindowDto`, `TimeZoneItemDto`: DTOs imutáveis normalizados com marcas temporais em UTC.

---

## Migrations Atuais
- Schema `calendar`:
  - `calendar.schedule_profiles` (`"Id"` uuid PK, `time_zone_id` varchar(100), `created_at` timestamptz, `updated_at` timestamptz)
  - `calendar.day_availability_rules` (`"Id"` uuid PK, `schedule_profile_id` uuid FK, `day_of_week` integer, `windows` jsonb)

---

## Telas e Rotas Atuais
- `/`: Redirecionamento condicional baseado na presença de perfil ativo.
- `/onboarding`: Assistente em 5 etapas (`Apresentação`, `Fuso Horário`, `Disponibilidade`, `Confirmação`, `Redirecionamento`).
- `/today`: Painel inicial exibindo fuso ativo, data civil do perfil e blocos de disponibilidade do dia e da semana.

---

## Estado dos Testes
- **Backend (.NET)**: 28/28 testes aprovados (16 Domínio, 4 Aplicação, 1 Integração Testcontainers, 7 API WebApplicationFactory).
- **Frontend (Vitest)**: 14/14 testes aprovados (4 suites: `OnboardingWizard`, `StepAvailabilityValidation`, `authGuard`, `TodayPage`).
- **Build**: Compilação .NET e `npm run build` (`vue-tsc -b && vite build`) concluídos com 0 erros.

---

## Bloqueadores
*Nenhum bloqueador ativo.*

---

## Débitos Aceitos
- Amarração de perfil único baseada em `localStorage` (`compass_active_profile_id`), adequada para fase inicial desktop/single-user. Suporte a múltiplos perfis/usuários será tratado em feature futura.

---

## Próximo Passo Exato
Executar `/speckit-specify` para planejar a próxima feature do módulo `Calendar` (gestão e visualização contínua de disponibilidade e datas civis) ou iniciar a fundação do módulo `Planning`.
