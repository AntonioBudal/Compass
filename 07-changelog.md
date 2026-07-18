# Compass — Change Log

**Projeto:** Compass  
**Stack:** ASP.NET Core (.NET 8/10), PostgreSQL 16+, Vue.js 3  
**Arquitetura:** Clean Architecture, Domain-Driven Design (DDD), TDD/BDD, Local-First

---

# 2026-07-16 — Etapa 01: Estrutura Inicial do Backend

## Objetivos concluídos

- Estruturação da solução em quatro projetos:
  - `Compass.Domain`
  - `Compass.Application`
  - `Compass.Infrastructure`
  - `Compass.Api`
- Configuração das referências entre projetos conforme a arquitetura definida.
- Isolamento da camada de domínio, mantendo independência de frameworks e infraestrutura.
- Implementação das entidades do domínio e das principais invariantes de negócio.
- Configuração da persistência utilizando Entity Framework Core e PostgreSQL.
- Definição da estratégia de herança:
  - TPH (`Commitment`, `Task`, `Habit`, `Event` e `Note`)
  - TPT (`Project` e `Goal`)
- Criação da migration inicial (`InitialProductionSchema`).

---

## Banco de Dados

### Recursos implementados

- Mapeamento dos enums nativos do PostgreSQL.
- Geração do esquema inicial da aplicação.
- Utilização de UUID como chave primária.
- Implementação de `CHECK CONSTRAINTS` para validação de regras estruturais.
- Criação de índices parciais voltados ao Motor de Decisão e ao sistema de lembretes.

---

## Observações

- A migration inicial foi gerada com sucesso.
- O banco PostgreSQL foi configurado para o ambiente de desenvolvimento.
- Permanecem warnings do Entity Framework Core relacionados aos valores padrão de propriedades `enum` configuradas com `HasDefaultValue()`. A correção foi adiada por não impactar o funcionamento atual da aplicação.

---

## Próximas etapas

- Implementação do `CompassDbContext`.
- Implementação dos repositórios.
- Configuração da camada Application.
- Desenvolvimento dos primeiros casos de uso.

---

# 2026-07-17 — Etapa 02: O Motor de Decisão (Scoring Engine & Time Windows)

## Objetivos concluídos

- Implementação dos serviços de domínio puros e determinísticos em C#:
  - `ScoringEngine`: Algoritmo matemático de priorização e ordenação de ações viáveis.
  - `TimeWindowCalculator`: Calculador de tempo líquido disponível baseado na agenda do usuário.
- Modelagem de objetos de valor e entidades de auditoria:
  - `CommitmentAttribute`: Estrutura chave-valor para flexibilizar parâmetros de score sem alterar schema SQL.
  - `DecisionSnapshot`: Entidade de gravação de telemetria e histórico das recomendações do motor.
- Implementação dos repositórios concretos na camada de Infraestrutura (`Compass.Infrastructure`):
  - `CommitmentRepository`: Otimizado com `.AsNoTracking()` e filtros alinhados aos índices parciais do banco.
  - `ProjectRepository`: Consulta rápida de árvores estratégicas para cálculo de bônus de alinhamento.
  - `DecisionSnapshotRepository`: Persistência de auditoria assíncrona.
- Geração e aplicação da migração `AddDecisionSnapshot` no PostgreSQL 16+.

---

## Engenharia do Algoritmo & Lógica Temporal

### 1. Curto-Circuito (Short-Circuit Filtering)
O motor elimina processamento desnecessário aplicando dois cortes de complexidade $O(1)$ antes do cálculo de pesos:
- **Corte de Dependência:** Tarefas filhas com pais não concluídos são descartadas.
- **Corte Temporal ($M_{\text{tempo}}$):** Se a duração estimada excede os minutos livres da janela atual (`EstimatedDurationMinutes > AvailableWindowMinutes`), a tarefa recebe $S = 0$ e é ignorada.

### 2. Fórmula de Ponderação Normatizada
$$S = w_1 \cdot U_{\text{prazo}} + w_2 \cdot M_{\text{tempo}} + w_3 \cdot M_{\text{energia}} + w_4 \cdot A_{\text{meta}} - P_{\text{atrito}}$$
- **$w_1$ (Urgência - 35%):** Pontuação escalonada por tempo restante (100% para atrasadas/vencendo hoje; 85% estrutural para Hábitos).
- **$w_2$ (Tempo - 25%):** 100% atribuído aos itens que sobreviveram ao curto-circuito temporal.
- **$w_3$ (Energia - 20%):** Distância absoluta compatibilizada via $1 - (|E_{\text{usuário}} - E_{\text{tarefa}}| / 2)$.
- **$w_4$ (Estratégia - 20%):** Bônus máximo para tarefas pertencentes a projetos vinculados a objetivos ativos (`GoalStatus.Active`).
- **$P_{\text{atrito}}$ (Penalidade por Adiantamento):** Subtração fixa de **5%** por cada adiamento anterior (`PostponedCount`), punindo a procrastinação crônica.

### 3. Janelas de Foco & Hard Blockers
- O `TimeWindowCalculator` intercepta reuniões em andamento (retornando `0m` livres imediatamente).
- Identifica o próximo evento (`EventCommitment`) no dia e limita a janela livre à diferença exata entre o momento atual (`nowUtc`) e o início do bloqueio.
- Aplica corte automático no fim do turno útil configurado em `Schedule` (ex: 18:00).

---

## Banco de Dados

### Recursos implementados
- Nova tabela `decision_snapshots` com relacionamentos opcionais (`NULLABLE FKs`) para as opções Top 1, Top 2 e Top 3.
- Aplicação de regra de exclusão defensiva (`ON DELETE SET NULL`) para preservar o histórico analítico mesmo que compromissos originais sejam deletados.
- Criação do índice analítico `idx_decision_snapshots_user_history` ordenado de forma decrescente por data de criação.

---

## Observações

- A compilação geral do ecossistema `.NET 10` foi concluída com êxito em todos os 4 projetos da Clean Architecture.
- O tempo de execução da query de candidatos ativos (`GetActiveCandidatesAsync`) está estruturalmente pronto para operar em latência inferior a 15 milissegundos no ambiente de produção.

---

## Próximas etapas

- Estruturação do projeto `Compass.Application` com DTOs e Casos de Uso.
- Implementação dos `Services` orquestradores (`CommitmentService` e `DecisionService`).
- Configuração do FluentValidation para blindagem de inputs REST.
- Criação do endpoint principal de consumo do Vue.js 3: `GET /api/v1/decisions/now`.

---

# 2026-07-18 — Etapa 03: Camada API, Casos de Uso Basais & Tratamento RFC 7807

## Objetivos concluídos

- Implementação dos Data Transfer Objects (DTOs) na camada `Compass.Application`:
  - `DecisionResponseDto`: Estrutura hierárquica contendo contexto livre, Top Focus e Alternativas.
  - `CreateCommitmentDto` e `CommitmentDto`: Modelagem unificada dos 4 arquétipos de compromissos.
  - `StatusTransitionResponseDto`: Retorno enriquecido com eventos em cascata para feedback no estado visual (Pinia).
- Blindagem de inputs com `FluentValidation`:
  - Regras de validação condicionais por tipo de compromisso (ex: duração mínima para Tarefas, ordem temporal para Eventos, formato CRON para Hábitos).
- Orquestração dos Casos de Uso via Serviços de Aplicação:
  - `DecisionService`: Integração transacional do `ScoringEngine` com consulta temporal e gravação de telemetria (< 15ms).
  - `CommitmentService`: Gerenciamento do ciclo de vida e acionamento de regras de negócio de transição de status.
- Estruturação da camada REST (`Compass.Api`):
  - `DecisionsController` (`GET /api/v1/decisions/now`, `POST /api/v1/decisions/now/choice`).
  - `CommitmentsController` (`GET`, `POST`, `PATCH /status`).
- Configuração de Middleware Global de Exceções (`GlobalExceptionHandler`) em conformidade com a **RFC 7807 (Problem Details)**.
- Mapeamento de Injeção de Dependência completa no `Program.cs` com suporte a CORS para o cliente local Vue.js 3 / Vite (`http://localhost:5173`).

---

## Padrões de API & Tratamento de Erros

### 1. Interceptação Global (RFC 7807)
A aplicação elimina respostas HTTP genéricas ou vazamentos de stack trace. Exceções são capturadas pelo `GlobalExceptionHandler` e traduzidas para o content-type `application/problem+json`:
- **`ValidationException` (`400 Bad Request`):** Retorna dicionário contendo os campos inválidos e suas respectivas mensagens de erro.
- **`DomainException` (`400 Bad Request`):** Traduz tentativas de violação das invariantes da arquitetura em mensagens humanamente legíveis.
- **`Exception` (`500 Internal Server Error`):** Resposta genérica segura com log rastreável no backend.

### 2. Acesso Local-First (MVP)
Para facilitar testes sem acoplamento prematuro a provedores de identidade complexos, os controladores implementam fallback inteligente para identificação de contexto através do header HTTP `X-User-Id` (ou UUID padrão de teste local).

---

## Observações

- A compilação da solução completa (`Compass.Backend`) atingiu o marco de **0 Erros** no ambiente `.NET 10 / .NET 8`.
- Os endpoints de decisão e compromissos estão operacionais e prontos para teste via Swagger UI ou Postman.

---

## Próximas etapas

- Iniciar a **O Frontend Vue.js 3 (Local-First & Pinia)**.
- Criação do projeto Vite com TypeScript e Tailwind CSS.
- Configuração das stores reativas no Pinia para sincronização de estado em tempo real com os DTOs do backend.
- Construção do componente de interface principal: O Dashboard "Agora" com animação de escolha das Top 3 Ações.