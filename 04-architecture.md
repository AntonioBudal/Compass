# 04. Architecture

> **Padrão Arquitetural:** MVC Moderno (API-First) + Arquitetura em Camadas Tradicional (N-Tier)

**Frontend:** Vue.js 3 (SPA)  
**Backend:** ASP.NET Core (.NET 8+)  
**Banco de Dados:** PostgreSQL 16+

---

# 1. Visão Geral da Arquitetura (MVC Desacoplado / API-First)

A arquitetura do **Compass** é estruturada em quatro camadas técnicas superpostas e bem definidas, operando em um fluxo unidirecional de dados (**Top-Down** para comandos e **Bottom-Up** para leitura).

Em vez de um MVC monolítico onde o servidor renderiza HTML, adotamos uma abordagem **API-First**, onde o **Vue.js** atua exclusivamente como a camada **View**, enquanto o **ASP.NET Core** gerencia os **Controllers**, **Services** e **Models**.

---

## 1.1 Camada View (Frontend — Vue.js 3)

Responsável por capturar a intenção do usuário em sub-segundos, gerenciar o estado local e garantir funcionamento offline. Não possui regras de negócio, apenas reatividade visual.

### Responsabilidades

- **Componentes UI (Arquétipos Rápidos)**
  - Interface focada no uso por teclado.
  - Botões predestinados (Tarefa, Projeto, Evento e Hábito).
  - Tela principal **"Agora"**.

- **State Cache (Pinia Store)**
  - Contexto atual do usuário.
  - Energia disponível.
  - Tempo disponível.
  - Cache local do **Top 3**.

- **Persistência Offline (Dexie.js + IndexedDB)**
  - Armazena ações sem conexão.
  - Mantém uma fila de sincronização (*Sync Queue*).

- **Comunicação**
  - HTTP GET para consultas.
  - HTTP POST / PUT / PATCH para mutações.

---

## 1.2 Camada Controller (Backend — ASP.NET Core API)

A porta de entrada pública da aplicação.

Segue o conceito de **Thin Controller**, cuja responsabilidade limita-se a:

- Receber requisições HTTP.
- Validar entrada.
- Delegar processamento.

### Componentes

- **API Controllers**
  - DecisionsController
  - CommitmentsController

- **Autenticação**
  - JWT

- **Validação**
  - FluentValidation

- **Injeção de Dependência**
  - ICommitmentService
  - IDecisionService

- **Middlewares**
  - Tratamento global de exceções.
  - Respostas RFC 7807.

---

## 1.3 Camada Application & Domain

O coração da aplicação.

Concentra todas as regras de negócio mantendo isolamento completo de HTTP e banco de dados.

### Services

- CommitmentService
- DecisionService

Responsabilidades:

- Orquestrar casos de uso.
- Aplicar validações.
- Persistir dados.
- Executar atualizações em cascata.

### Entidades

- Commitment
- Task
- Project
- Goal

As entidades encapsulam comportamentos e garantem invariantes do domínio.

### Motor de Decisão

**ScoringEngine**

- Implementado em C#
- Determinístico
- Sem dependências externas
- Baixa latência

---

## 1.4 Camada de Infraestrutura

Responsável pela persistência física dos dados.

### Repositórios

- CommitmentRepository
- ProjectRepository

### ORM

- EF Core 8/9
- Npgsql

### Banco

- PostgreSQL 16+

Recursos utilizados:

- JSONB
- Partial Indexes
- Check Constraints

### Background Jobs

Quartz.NET executa:

- geração diária de hábitos;
- reset de streaks;
- outras tarefas agendadas.

---

## 1.5 Fluxo de Execução

```text
Usuário
    │
    ▼
Vue.js (View)
    │
    ▼
Pinia
    │
    ▼
HTTP
    │
    ▼
Controller
    │
    ▼
Application Service
    │
    ▼
Domain
    │
    ▼
Repository
    │
    ▼
Entity Framework Core
    │
    ▼
PostgreSQL
```

---

# 2. Ecossistema de Tecnologias

## 2.1 Backend (.NET)

| Tecnologia | Finalidade |
|------------|------------|
| Npgsql.EntityFrameworkCore.PostgreSQL | ORM PostgreSQL |
| FluentValidation | Validação de DTOs |
| Quartz.NET | Background Jobs |
| Serilog | Logs estruturados |
| Swashbuckle | OpenAPI / Swagger |

---

## 2.2 Frontend (Vue)

| Tecnologia | Finalidade |
|------------|------------|
| Vue 3 (Composition API) | Interface |
| TypeScript | Tipagem |
| Pinia | Estado Global |
| Vue Query | Cache e sincronização |
| Tailwind CSS | Estilização |
| Radix Vue / Shadcn Vue | Componentes |
| Dexie.js | Offline First |

---

# 3. O Algoritmo de Pontuação

O algoritmo de pontuação é implementado no **ScoringEngine** em C# puro, garantindo:

- comportamento determinístico;
- previsibilidade;
- baixa latência.

Sua responsabilidade é calcular a relevância de cada compromisso elegível.

---

## 3.1 Fórmula

```text
S = w1 * U_prazo
  + w2 * M_tempo
  + w3 * M_energia
  + w4 * A_meta
  - P_atrito
```

Onde:

| Variável | Significado |
|----------|-------------|
| U_prazo | Urgência |
| M_tempo | Compatibilidade temporal |
| M_energia | Compatibilidade energética |
| A_meta | Alinhamento estratégico |
| P_atrito | Penalidade por adiamentos |

---

## 3.2 Componentes

### 1. Urgência (U_prazo)

Quanto menor o tempo restante até o prazo, maior sua influência na nota final.

---

### 2. Compatibilidade Temporal (M_tempo)

Verificação:

```csharp
EstimatedDuration > AvailableWindowMinutes
```

Caso verdadeiro:

```text
M_tempo = 0
```

A tarefa é eliminada do cálculo.

---

### 3. Compatibilidade Energética (M_energia)

```text
M_energia = 1 - |E_usuario - E_tarefa| / 2
```

Quanto maior a compatibilidade entre energia do usuário e energia exigida, maior a pontuação.

---

### 4. Alinhamento Estratégico (A_meta)

```text
Task
└── Project
    └── Goal (Ativo)
```

Projetos vinculados a objetivos ativos recebem prioridade estrutural.

---

### 5. Penalidade por Adiamento (P_atrito)

Baseada em:

```text
PostponedCount
```

Quanto maior:

- maior a penalidade;
- menor a chance de permanecer indefinidamente no Top 3.

---

## 3.3 Resultado Final

Após os cálculos, o motor:

1. Remove tarefas incompatíveis.
2. Calcula a nota S.
3. Ordena por relevância.
4. Retorna as três melhores ações.

---

# 4. Estrutura de Pastas

```text
Compass.Backend/
│
└── src/
    │
    ├── Compass.Api/
    │   ├── Controllers/
    │   ├── Middlewares/
    │   ├── Extensions/
    │   └── Program.cs
    │
    ├── Compass.Application/
    │   ├── DTOs/
    │   ├── Services/
    │   ├── Interfaces/
    │   └── Validators/
    │
    ├── Compass.Domain/
    │   ├── Entities/
    │   ├── Enums/
    │   ├── Exceptions/
    │   └── Services/
    │
    └── Compass.Infrastructure/
        ├── Persistence/
        ├── Repositories/
        └── Jobs/
```

---

# 5. Pipeline de Decisão (End-to-End)

## 1. Requisição HTTP

```text
GET /api/v1/decisions/now
```

↓

**DecisionsController**

↓

**IDecisionService.GetTop3ActionsAsync()**

---

## 2. Consulta ao Repositório

O serviço consulta o **ICommitmentRepository**, utilizando índices parciais do PostgreSQL para recuperar apenas:

- PENDING
- IN_PROGRESS

não arquivados.

---

## 3. Cruzamento de Agenda

O sistema:

- verifica eventos;
- calcula bloqueios;
- determina a janela de tempo realmente disponível.

---

## 4. Execução do ScoringEngine

O algoritmo:

- calcula a pontuação;
- remove tarefas incompatíveis;
- ordena por relevância.

---

## 5. Retorno

O serviço:

- registra um **DecisionSnapshot**;
- converte as entidades para DTO;
- retorna as **Top 3 Ações**.

**Latência esperada:** `< 15 ms`.