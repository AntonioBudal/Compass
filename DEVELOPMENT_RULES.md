# Regras Arquiteturais e de Desenvolvimento

Este documento estabelece as **Fitness Functions** (Regras Arquiteturais Verificáveis) do Compass. Estas não são meras sugestões ou guias de estilo, são mandamentos técnicos inegociáveis. 

A arquitetura do Compass baseia-se em separação estrita de domínios (Monólito Modular) e alta previsibilidade. Sempre que possível, estas regras devem ser fiscalizadas automaticamente na esteira de CI/CD utilizando ferramentas como **NetArchTest** (para o backend C#), **ESLint** (com regras de *dependency cruiser* no frontend) e análises estáticas de dependência.

---

## REGRAS DE BACKEND (MONÓLITO MODULAR)

### RULE-B01 — Isolamento de Infraestrutura
**Regra:** Nenhuma classe fora dos namespaces `Compass.Modules.[Módulo].Infrastructure` pode fazer referência aos pacotes `Microsoft.EntityFrameworkCore` ou `Npgsql`.
**Motivação:** O vazamento de classes do ORM (como `DbSet`, `IQueryable` ou `DbContext`) para as camadas de Aplicação ou Domínio gera forte acoplamento com o banco de dados.
**Exemplo de violação:** Retornar um `IQueryable<Task>` diretamente de um Use Case para o Controller.
**Forma correta:** O repositório na Infraestrutura executa a *query*, materializa o resultado e devolve uma lista de DTOs ou Entidades puras (ex: `IReadOnlyList<TaskReadModel>`).

### RULE-B02 — Isolamento Físico de Estado (Soft-Links)
**Regra:** Um módulo não pode possuir `DbSet` ou chaves estrangeiras físicas apontando para tabelas de outros schemas. O relacionamento inter-módulos ocorre apenas via armazenamento do UUID (Soft-Link).
**Motivação:** Se o `Execution` tiver uma *Foreign Key* dura para o `Planning`, torna-se impossível escalar ou extrair esses módulos no futuro sem quebrar o banco inteiro.
**Exemplo de violação:** O `ExecutionDbContext` possuir uma propriedade de navegação `public Planning.Task Task { get; set; }`.
**Forma correta:** A tabela `execution_logs` possui a coluna `task_id` do tipo UUID e o desenvolvedor realiza buscas sob demanda via contratos se precisar de dados estendidos.

### RULE-B03 — Comunicação Estrita Inter-Módulos
**Regra:** Módulos só podem se comunicar chamando interfaces públicas localizadas na pasta `Contracts`.
**Motivação:** Respeitar os limites do monólito modular. O uso do modificador `internal` protege as implementações, e a reflexão não pode ser usada para quebrar essa proteção.
**Exemplo de violação:** O módulo `Execution` instanciar diretamente o `PlanningDbContext` ou o `TaskRepository` usando reflexão ou *service locator* bypassando o container público.
**Forma correta:** O `Execution` injeta `IPlanningIntegrationService` (uma interface pública definida em `Contracts`), sem saber como a implementação resolve os dados.

### RULE-B04 — Motor Determinístico e Puro
**Regra:** O Serviço de Domínio `DecisionEngine` e seus métodos de *Scoring* não podem depender de I/O, instâncias de repositórios, chamadas HTTP ou leitura de relógios globais ocultos.
**Motivação:** O motor deve ser 100% reproduzível e testável apenas passando entradas e coletando saídas.
**Exemplo de violação:** O método do motor fazer internamente `_taskRepository.GetReadyTasks()`.
**Forma correta:** O método de *scoring* recebe a lista em memória `IEnumerable<TaskReadModel>` e o instante `DateTimeOffset now` como parâmetros da função.

### RULE-B05 — Pureza da Camada de Domínio
**Regra:** O namespace `Compass.Modules.[Módulo].Domain` não pode possuir dependências de frameworks externos de banco, rede, ou lógicas de serialização JSON. 
**Motivação:** O domínio deve modelar exclusivamente as regras do negócio do Compass.
**Exemplo de violação:** Usar `[Table("tasks")]` ou `[JsonPropertyName("title")]` em classes do Domínio.
**Forma correta:** O domínio é C# puro. Bibliotecas externas são permitidas **apenas** se forem pequenas, indispensáveis e de caráter estritamente matemático/lógico (como cálculo e conversão técnica de fuso horário IANA).

### RULE-B06 — Endpoints Focados e Magros
**Regra:** Classes de apresentação (`Endpoints` ou `Controllers`) estão estritamente proibidas de injetar `DbContext`.
**Motivação:** A API (Presentation) é apenas uma porta de entrada HTTP. A responsabilidade por orquestrar a operação é da camada `Application`.
**Exemplo de violação:** Um Controller recebendo um request JSON e fazendo `_dbContext.Tasks.Add(task); _dbContext.SaveChanges();`.
**Forma correta:** O Controller recebe o JSON HTTP, converte em um *Command* (ex: `CreateTaskCommand`), despacha para a camada de *Application* e retorna o respectivo Status Code.

---

## REGRAS DE FRONTEND (FEATURE-SLICED DESIGN)

### RULE-F01 — Pureza das Camadas de Apresentação (Entities)
**Regra:** Componentes dentro da pasta `src/entities/` não podem possuir instâncias de `Axios`, chamadas de *Vue Query* (mutações/queries) ou lógicas complexas de *Optimistic Update*.
**Motivação:** Componentes na camada `entities` são apenas os "tijolos" de apresentação (ex: `TaskCard.vue`). Eles devem ser reutilizáveis e fáceis de testar em isolamento.
**Exemplo de violação:** Um botão dentro de `entities/task/TaskCard.vue` fazendo `axios.post('/log-work')`.
**Forma correta:** O `TaskCard.vue` emite um evento (ex: `@complete="onComplete"`). O componente pai que reside na camada `features/` intercepta esse evento e realiza a chamada da API (Vue Query).

### RULE-F02 — Fluxo Direcional de Dependência FSD
**Regra:** O fluxo de importação (ES6 `import`) só pode ocorrer de cima para baixo na hierarquia FSD.
**Motivação:** Evitar ciclos de dependência (acoplamento espaguete), garantindo que infraestrutura não dependa de lógicas específicas de negócio.
**Exemplo de violação:** O arquivo `shared/api/axios.ts` importar o componente `features/work-logger/WorkLoggerModal.vue`.
**Forma correta:** 
*   `app` $\rightarrow$ importa de `widgets`
*   `widgets` $\rightarrow$ importa de `features`
*   `features` $\rightarrow$ importa de `entities`
*   `entities` $\rightarrow$ importa de `shared`.

### RULE-F03 — Isolamento Visual e Cores Semânticas
**Regra:** Nenhum arquivo Vue pode utilizar as classes hardcoded de paleta de cor do framework utilitário (ex: Tailwind) diretamente para cor.
**Motivação:** Permitir a troca instantânea de tema e evitar a proliferação visual descontrolada ("IA-slop").
**Exemplo de violação:** `<div class="bg-indigo-500 text-red-500">`
**Forma correta:** `<div class="bg-canvas text-danger">` (onde `canvas` e `danger` apontam para as variáveis CSS semânticas customizadas do sistema, ex: `var(--color-canvas)`).

### RULE-F04 — Propriedade Estrita do Server-State
**Regra:** Nenhuma cópia integral de listas do servidor (ex: "Inventário de Projetos") deve ser mantida ou atualizada manualmente na *Store* global local (Pinia). 
**Motivação:** Evitar duas fontes da verdade entrando em conflito no Front-end.
**Exemplo de violação:** Pinia Store com a propriedade `state: () => ({ tasks: [] })`, sendo abastecida manualmente após um Axios GET.
**Forma correta:** O *Server-State* é gerenciado unicamente pelo `Vue Query`. O Pinia é reservado **exclusivamente** para estados efêmeros da interface (ex: `isSidebarOpen`, `currentUserTheme`).

---

## REGRAS GERAIS DE SISTEMA E TEMPO

### RULE-S01 — Single Source of Truth do Motor de Decisão
**Regra:** A matemática de ranqueamento, filtros de viabilidade física e chunking (*Decision Engine*) residem **apenas** no Backend C#. 
**Motivação:** Prevenir a divergência de regras entre o C# e o TypeScript.
**Exemplo de violação:** O Front-end tentar calcular o `TargetDuration` localmente ou reordenar as recomendações offline com base em uma nova matemática implementada em JS.
**Forma correta:** O Front-end em modo Offline exibe as recomendações e cálculos que já estavam cacheados (podendo registrar a execução/mutação nelas). Para recalcular o ranking geral mediante mudanças, ele exige sincronicidade com o servidor.

### RULE-S02 — A Origem do "Agora"
**Regra:** Nenhum módulo no backend usará `DateTime.Now` ou `DateTime.UtcNow` diretamente no código de domínio ou aplicação. 
**Motivação:** Garantir testabilidade e controle preciso durante os *Assertions* de testes.
**Exemplo de violação:** `var isOverdue = task.HardDeadline < DateTime.UtcNow;`
**Forma correta:** Utilizar a abstração nativa do .NET 8+: `var isOverdue = task.HardDeadline < _timeProvider.GetUtcNow();`.

---
*A implementação destas regras orienta a topologia definida no [ARCHITECTURE.md](./ARCHITECTURE.md).*
*Para a base conceitual que originou estas restrições de comunicação, consulte o [DECISIONS.md](./DECISIONS.md).*