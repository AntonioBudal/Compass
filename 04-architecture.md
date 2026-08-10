# Arquitetura do Compass — Frontend

O frontend do Compass utiliza uma arquitetura baseada em **Feature-Sliced Design (FSD)**, adaptada às necessidades do projeto.

O principal objetivo é manter **alta coesão**, **baixo acoplamento** e uma separação clara entre infraestrutura, componentes compartilhados e regras de negócio.

Em vez de organizar o código por tecnologia (`views`, `stores`, `components`), a estrutura é organizada principalmente por **domínio de negócio**. Dessa forma, funcionalidades que evoluem juntas permanecem próximas.

## Estrutura do projeto

A pasta `src/` é dividida em três camadas principais:

```text
Compass.Frontend/
└── src/
    ├── app/        # Inicialização e configuração da aplicação
    ├── shared/     # Infraestrutura e componentes reutilizáveis
    └── modules/    # Domínios e regras de negócio
```

A responsabilidade de cada camada é definida abaixo.

---

## 1. `app/`

Responsável pela inicialização e configuração global da aplicação.

Essa camada não deve conter regras específicas de negócio.

### `App.vue`

Atua como **bootstrapper da aplicação**.

É responsável por inicializar o estado global e garantir que os dados essenciais sejam carregados antes da renderização das telas.

O carregamento inicial inclui dados como:

* Metas
* Projetos
* Compromissos
* Configurações necessárias para a aplicação

Essa abordagem evita que as Views realizem múltiplos carregamentos independentes e reduz problemas de sincronização durante o reload da aplicação.

### `router/index.ts`

Responsável por:

* Definição das rotas
* Navegação
* Guards de rota
* Controle de acesso

### `styles/`

Contém estilos globais e tokens utilizados pela aplicação.

### `globalErrorHandler.ts`

Centraliza o tratamento de erros não capturados da aplicação.

---

## 2. `shared/`

Contém código reutilizável e infraestrutura que não depende de regras específicas de negócio.

O código dessa camada não deve conhecer conceitos como `Goal`, `Project`, `Habit` ou `Commitment`.

### `api/`

Responsável pela infraestrutura de comunicação HTTP.

```text
shared/api/
└── client.ts
```

O `client.ts` centraliza:

* Configuração do Axios
* Interceptors
* Headers
* Tratamento de autenticação
* Comportamentos relacionados à rede
* Suporte à estratégia Offline-First

As APIs específicas de cada domínio ficam dentro dos respectivos módulos.

### `ui/`

Contém componentes visuais reutilizáveis e independentes de domínio.

Exemplos:

* `OmniInput`
* `ToastContainer`
* `ErrorBoundary`
* Modais globais
* Layouts estruturais

### `composables/`

Contém composables reutilizáveis que não pertencem a um domínio específico.

### `utils/`

Contém funções utilitárias, como:

* Formatação de datas
* Funções matemáticas
* Utilitários de teclado
* Funções auxiliares

O `nlpParser` utilizado pelo Quick Capture também pertence a essa camada enquanto permanecer agnóstico de domínio.

### `stores/`

Contém apenas stores relacionados ao estado global da aplicação.

Exemplos:

* `toastStore`
* `offlineStore`
* `themeStore`

Stores relacionados a regras de negócio devem permanecer dentro dos respectivos módulos.

---

# 3. `modules/`

Essa é a camada responsável pelas regras e funcionalidades de negócio do Compass.

Cada módulo representa um domínio funcional e mantém seus próprios:

* Components
* Views
* Stores
* APIs
* Composables
* Tipos
* Regras específicas do domínio

Estrutura conceitual:

```text
modules/
├── tactical/
├── strategy/
├── execution/
├── analytics/
├── onboarding/
└── settings/
```

A remoção ou alteração de um módulo deve causar o mínimo possível de impacto nos demais módulos.

---

## `modules/tactical/`

Responsável pela execução das atividades do dia a dia.

### Domínios

* Hábitos
* Tarefas
* Eventos
* Notas
* Compromissos

### Views

* `AgendaView`
* `HabitsView`
* `DatabaseView`

### Responsabilidades

O módulo concentra funcionalidades relacionadas à execução direta das atividades, incluindo:

* Agenda
* Drag & Drop
* Resize
* Cálculos de posicionamento
* Gerenciamento de compromissos

As chamadas relacionadas a compromissos são realizadas através de `commitments.api.ts`.

---

## `modules/strategy/`

Responsável pelo planejamento de médio e longo prazo.

### Domínios

* Projetos
* Metas

### Views

* `ProjectsView`
* `GoalsView`
* `LibraryView`

### Responsabilidades

O módulo trabalha com:

* Definição de objetivos
* Planejamento de projetos
* Acompanhamento de progresso
* Relação entre tarefas, projetos e metas

O progresso dos projetos pode ser calculado a partir dos dados provenientes do módulo `tactical`, como tarefas concluídas e tempo registrado.

---

## `modules/execution/`

Responsável pela execução orientada e tomada de decisão durante uma sessão de trabalho.

### Domínios

* Sistema de decisão
* Scoring
* Diário
* Ciclo diário

### Views

* `NowEngineView`
* `JournalView`

### Responsabilidades

O módulo utiliza dados de decisão provenientes de `decisions.api.ts` para determinar os focos prioritários da sessão.

Entre os fatores considerados estão:

* Energia disponível
* Janela de tempo
* Prioridade
* Contexto
* Estado atual das atividades

---

# Módulos de apoio

## `modules/analytics/`

Responsável pela análise e visualização dos dados operacionais.

Inclui:

* `progressStore`
* KPIs
* Heatmaps
* Gráficos
* Métricas de produtividade

---

## `modules/onboarding/`

Responsável pelos fluxos de inicialização e experimentação do usuário.

Inclui:

* `SandboxView`
* Fluxos de onboarding
* Integração com o `GlassBox`

A `SandboxView` permite testar determinadas funcionalidades sem depender diretamente do estado persistido da aplicação.

---

## `modules/settings/`

Responsável pelas configurações da aplicação.

Inclui:

* Preferências de interface
* Importação e exportação de dados
* Configurações relacionadas ao armazenamento local
* Operações relacionadas ao SQLite

---

# Regras de arquitetura

## 1. Views não devem realizar carregamento inicial de dados

As Views não devem realizar chamadas diretas à API durante o `onMounted`.

```ts
// Evitar
onMounted(async () => {
    const response = await api.get(...)
})
```

O carregamento inicial é responsabilidade do `App.vue` e das respectivas camadas de estado.

As Views devem consumir os dados já disponíveis através das Stores e apenas cuidar da apresentação e interação com o usuário.

---

## 2. Componentes devem possuir responsabilidades claras

Componentes genéricos e reutilizáveis devem permanecer em `shared/ui`.

Exemplo:

```text
shared/ui/
└── Button.vue
```

Componentes que possuem regras específicas de um domínio devem permanecer dentro do módulo correspondente.

Exemplo:

```text
modules/strategy/
└── components/
    └── ProjectProgressCard.vue
```

A regra geral é:

* **Componente genérico:** `shared/ui`
* **Componente específico de domínio:** `modules/<domain>/components`

---

## 3. Módulos devem evitar dependências diretas entre si

Um módulo não deve alterar diretamente o estado interno de outro módulo.

Por exemplo, `execution` pode consumir dados relacionados a tarefas do módulo `tactical`, mas não deve modificar diretamente suas Stores ou estruturas internas.

A comunicação deve ocorrer através das interfaces e estados públicos definidos pela aplicação.

```text
execution
    │
    │ consome dados
    ▼
tactical
```

O objetivo é manter os módulos desacoplados e permitir que cada domínio evolua de forma independente.

---

## 4. APIs devem permanecer próximas ao domínio

A infraestrutura HTTP permanece em:

```text
shared/api/client.ts
```

Enquanto as operações específicas de cada domínio permanecem dentro do módulo correspondente:

```text
modules/
├── tactical/
│   └── api/
│       └── commitments.api.ts
│
├── strategy/
│   └── api/
│       ├── goals.api.ts
│       └── projects.api.ts
│
└── execution/
    └── api/
        └── decisions.api.ts
```

Essa separação evita a criação de um único arquivo central contendo todas as operações da API.

---

# Objetivo da arquitetura

A arquitetura busca manter três responsabilidades claramente separadas:

```text
app/
└── Inicialização da aplicação

shared/
└── Infraestrutura e recursos reutilizáveis

modules/
└── Regras e funcionalidades de negócio
```

Com essa organização, novas funcionalidades devem ser adicionadas preferencialmente ao módulo de domínio correspondente, evitando o crescimento de arquivos centralizados e reduzindo o acoplamento entre diferentes partes do sistema.
