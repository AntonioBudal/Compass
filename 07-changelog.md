# Developer Changelog

## 2026-07-16

### Arquitetura

* Estruturação da solução em `Compass.Domain`, `Compass.Application`, `Compass.Infrastructure` e `Compass.Api`.
* Configuração do Entity Framework Core com PostgreSQL.
* Implementação das estratégias de herança TPH e TPT.

### Banco de Dados

* Mapeamento de enums do PostgreSQL.
* Utilização de UUID como chave primária.
* Adição de CHECK Constraints e índices.
* Criação da migration `InitialProductionSchema`.

---

## 2026-07-17

### Backend

* Implementação do `ScoringEngine`.
* Implementação do `TimeWindowCalculator`.
* Modelagem de `CommitmentAttribute`.
* Modelagem de `DecisionSnapshot`.
* Implementação dos repositórios de Commitments, Projects e DecisionSnapshots.

### Banco de Dados

* Criação da tabela `decision_snapshots`.
* Configuração de relacionamentos e índices.
* Criação da migration `AddDecisionSnapshot`.

---

## 2026-07-18

### Backend

* Implementação dos DTOs.
* Configuração do FluentValidation.
* Implementação de `DecisionService`.
* Implementação de `CommitmentService`.

### API

* Criação dos controllers REST.
* Implementação do `GlobalExceptionHandler`.
* Suporte ao header `X-User-Id`.

---

## 2026-07-19

### Frontend

* Inicialização do projeto com Vue 3, Vite, TypeScript e Tailwind CSS.
* Configuração do Axios.
* Configuração do Pinia.
* Implementação do Design System.
* Implementação do App Shell.
* Implementação de atalhos globais de teclado.
* Suporte a `prefers-reduced-motion`.

### Stores

* `useCommitmentsStore`.
* `useDecisionStore`.

### Componentes

* `CommitmentCard`.
* `TopFocusCard`.
* `CommandBarModal`.
* `QuickCaptureModal`.

### Views

* `NowEngineView`.
* `AgendaView`.
* `ProjectsView`.
* `GoalsView`.
* `HabitsView`.

---

## 2026-07-20

### Frontend

* Compatibilidade entre Node.js, Vue-TSC e TypeScript.
* Padronização das animações no Tailwind CSS.
* Implementação de utilitários para aceleração por GPU.
* Adição de microinterações.

### Stores

* `toastStore`.
* `journalStore`.
* `settingsStore`.

### Componentes

* Tooltip de explicabilidade no `TopFocusCard`.
* Remoção de elementos visuais coloridos e emojis.
* Padronização da interface monocromática.

### Views

* `SettingsView`.
* `JournalView`.

### Modais

* `EditCommitmentModal`.
* `DailyShutdownModal`.
