# 02. Domain

**Agregado Raiz Principal:** `Commitment` (Compromisso)  
**Mecanismo de Desacoplamento:** Domain Events (MediatR)

---

# 1. Mapa de Contextos Delimitados (Bounded Contexts)

O domínio do Compass é dividido nos seguintes Bounded Contexts:

| Contexto | Responsabilidade |
|----------|------------------|
| **Core (Commitments)** | Gerencia Objetivos, Projetos, Tarefas, Hábitos e Eventos. |
| **Decision Engine** | Responsável pelo histórico de decisões, cálculo de pontuação (Scoring) e atributos dinâmicos utilizados pelo motor de decisão. |
| **Knowledge** | Armazena notas, referências e informações que não possuem comportamento temporal nem participam do processo de decisão. |
| **Scheduling** | Gerencia disponibilidade do usuário, horários úteis e lembretes. |
| **Analytics & Telemetry** | Registra histórico de execução, sessões de foco e métricas utilizadas para análise posterior. |
| **Identity** | Gerencia usuários, autenticação e configurações pessoais. |

---

# 2. Modelagem de Entidades por Contexto

## 2.1. Contexto: Core (`Commitments`)

### Commitment (Classe Abstrata Base)

Representa qualquer compromisso executável do sistema.

**Implementação**

- Classe abstrata
- Persistência via TPH (Table Per Hierarchy)

**Atributos**

- `Id`
- `UserId`
- `Title`
- `Status`
- `CreatedAt`

---

### Task : Commitment

Representa uma unidade executável de trabalho.

**Atributos**

- `Deadline`
- `EstimatedDuration` (30 minutos por padrão)
- `ProjectId?`
- `ParentTaskId?`

---

### Project : Commitment

Representa um conjunto organizado de tarefas.

**Atributos**

- `GoalId?`
- `Deadline`

**Observação**

O progresso do projeto é calculado dinamicamente durante a leitura, não sendo persistido no banco.

---

### Goal : Commitment

Representa um objetivo estratégico de longo prazo.

**Atributos**

- `TargetDate`
- `WhyDescription`

**Observação**

Objetivos nunca aparecem diretamente na tela "Agora". Eles apenas influenciam a pontuação dos projetos relacionados.

---

### Habit : Commitment

Representa uma atividade recorrente.

**Atributos**

- `CronExpression`
- `CurrentStreak`
- `BestStreak`

---

### Event : Commitment

Representa um compromisso fixo na agenda.

**Atributos**

- `StartTime`
- `EndTime`
- `Location`

**Observação**

Eventos funcionam como bloqueadores rígidos de tempo para o Motor de Decisão.

---

## 2.2. Contexto: Decision Engine

### CommitmentAttribute

Estrutura flexível para armazenar atributos utilizados pelo algoritmo de decisão.

**Exemplo**

Key   = "MentalEnergy"
Value = "3"

---

### DecisionSnapshot

Registra o contexto completo de uma decisão tomada pelo sistema.

**Atributos**

- `AvailableWindowMinutes`
- `UserEnergyContext`
- `Top1Id`
- `Top2Id`
- `Top3Id`
- `ChosenCommitmentId`
- `WasIgnored`

**Objetivo**

Permitir auditoria, análise comportamental e evolução futura do algoritmo.

---

## 2.3. Contexto: Knowledge

### Note

Representa uma nota ou referência.

**Atributos**

- `Id`
- `Title`
- `Content`
- `ConvertedToCommitmentId?`

---

## 2.4. Contexto: Scheduling

### AvailabilityBlock

Define intervalos de disponibilidade do usuário.

**Atributos**

- Dia da semana
- `StartTime`
- `EndTime`

---

## 2.5. Contexto: Analytics & Telemetry

### CompletionHistory

Histórico de alterações relevantes.

**Atributos**

- `Action`
    - Completed
    - Reopened
    - Archived

---

### FocusSession

Registra sessões reais de execução realizadas pelo usuário.

---

# 3. Invariantes do Domínio (Regras Inegociáveis)

| Entidade | Regra de Negócio | Impacto |
|-----------|------------------|----------|
| **Task** | Dependência Estrita: uma tarefa não pode ser iniciada ou sugerida caso a tarefa pai ainda não tenha sido concluída. | O Motor de Decisão atribui pontuação zero para tarefas bloqueadas. |
| **Task** | Toda tarefa executável deve possuir duração estimada maior ou igual a 5 minutos. | Evita registros inválidos que comprometam o algoritmo de planejamento. |
| **Project** | Um projeto não pode ser concluído manualmente enquanto possuir tarefas pendentes. | Garante consistência entre execução e progresso. |
| **Goal** | O progresso é sempre calculado pelo backend a partir dos projetos relacionados. | Elimina atualização manual de progresso. |
| **Event** | Dois eventos nunca podem ocupar o mesmo intervalo de tempo. | O sistema lança `EventOverlapException` (HTTP 409). |
| **Habit** | Caso o ciclo termine sem conclusão, a sequência (`CurrentStreak`) é reiniciada automaticamente. | Mantém a gamificação totalmente determinística. |

---

# 4. Máquina de Estados (Lifecycle State Machine)

## Estados

1. Criado
2. Pendente
3. Em Andamento
4. Concluído
5. Bloqueado
6. Arquivado

---

## Fluxo Principal

Criado
    ↓
Pendente
    ├── Iniciar foco ─────────────► Em Andamento
    ├── Bloqueado ────────────────► Bloqueado
    └── Arquivar ─────────────────► Arquivado

Em Andamento
    ├── Concluir ─────────────────► Concluído
    ├── Bloquear ─────────────────► Bloqueado
    └── Arquivar ─────────────────► Arquivado

Bloqueado
    └── Desbloquear ──────────────► Pendente

Concluído
    ├── Reabrir (Task) ───────────► Pendente
    └── Arquivar ─────────────────► Arquivado

Arquivado
    └── (Estado final)


### Regras Críticas

- Apenas `Task` pode ser reaberta após conclusão.
- Reabrir uma tarefa dispara os eventos necessários para recalcular projetos e objetivos.
- Um item arquivado não pode sofrer alterações.
- Para modificar um item arquivado é obrigatório restaurá-lo primeiro.

---

# 5. Fluxo de Domain Events (CQRS + MediatR)

## Cenário: Conclusão de uma tarefa

POST /tasks/{id}/complete
        │
        ▼
TaskService
Altera o Status para Completed
        │
        ▼
TaskCompletedEvent
        │
        ├────────► CompletionHistoryHandler
        │           • Registra o histórico da conclusão
        │
        ├────────► DecisionFeedbackHandler
        │           • Atualiza estatísticas do Motor de Decisão
        │           • Marca a recomendação como executada
        │
        └────────► ProjectCascadeHandler
                    • Verifica se ainda existem tarefas pendentes
                    • Caso não existam:
                        → Conclui automaticamente o Project
                        → Dispara os eventos responsáveis pela atualização do Goal
```