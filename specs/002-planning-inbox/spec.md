# Feature Specification: Planning Inbox Inicial

**Feature Branch**: `002-planning-inbox`

**Created**: 2026-08-28

**Status**: Draft

**Input**: User description: "Criar a Planning Inbox inicial do Compass. O usuário deve conseguir capturar uma Task informando inicialmente apenas um título. A Task nasce como Draft enquanto não possuir estimativa de duração. Na Inbox, o usuário deve conseguir: 1. visualizar Tasks persistidas; 2. separar Draft, Ready, InProgress e Done; 3. adicionar uma estimativa em minutos a uma Task Draft; 4. tornar a Task Ready quando receber uma estimativa válida; 5. editar título, estimativa e deadline; 6. iniciar e concluir uma Task conforme seu lifecycle; 7. recarregar a página sem perder os dados; 8. receber feedback claro para estados vazios, loading, erros e transições inválidas. Regras: Draft não pode ser candidato ao planejamento diário; Ready pode ser consumida futuramente pelo motor de planejamento; estimativa deve ser positiva; transições de estado pertencem ao backend; o frontend não deve inferir nem forçar estados; nenhuma Task mockada; o backend é a fonte da verdade; esta feature não deve implementar ainda Projects, Habits, Calendar avançado ou DailyPlan. A entrega deve incluir backend, persistência PostgreSQL, endpoints, tela /planning, testes de domínio, integração, API e frontend."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Captura Rápida e Visualização na Inbox (Priority: P1) 🎯 MVP

Como um usuário gerenciando minhas demandas, quero capturar rapidamente uma nova tarefa informando apenas o título, visualizá-la na minha Inbox na categoria "Draft" e consultar todas as minhas tarefas organizadas por status, para nunca perder uma ideia ou pendência.

**Why this priority**: É o ponto de entrada primário do módulo de planejamento. Sem a capacidade de capturar itens como rascunho de forma rápida e visualizá-los persistentemente, nenhuma etapa posterior de refinamento ou execução pode ocorrer.

**Independent Test**: Acessar a tela `/planning`, digitar o título de uma tarefa no formulário de captura rápida, submeter e verificar que a tarefa surge imediatamente na coluna/aba "Draft" e permanece lá após recarregar a página (F5).

**Acceptance Scenarios**:

1. **Given** que o usuário está na tela `/planning`, **When** insere o título "Comprar passagens" e clica em capturar (ou pressiona Enter), **Then** o backend cria a tarefa com status `Draft` (sem estimativa de duração) e a tarefa é exibida na Inbox na seção de rascunhos.
2. **Given** que existem tarefas cadastradas no sistema com diferentes status, **When** o usuário acessa a Inbox, **Then** as tarefas são apresentadas agrupadas/filtradas claramente pelos status `Draft`, `Ready`, `InProgress` e `Done`.
3. **Given** que o usuário cadastrou tarefas na Inbox, **When** a página é recarregada (F5), **Then** todas as tarefas são recuperadas do backend sem perda de dados e sem depender de dados mockados.

---

### User Story 2 - Estimativa de Duração e Promoção para Ready (Priority: P1)

Como um usuário que capturou ideias em rascunho, quero adicionar uma estimativa de duração em minutos a uma tarefa `Draft` para que ela seja promovida ao status `Ready` e se torne elegível para futuros planejamentos diários.

**Why this priority**: O motor de planejamento diário exige que tarefas possuam duração estimada conhecida para poder alocá-las na disponibilidade de tempo. A transição de `Draft` para `Ready` estabelece a fronteira entre ideias brutas e trabalho planejável.

**Independent Test**: Selecionar uma tarefa em estado `Draft`, informar uma estimativa válida (ex.: `45` minutos), salvar e verificar que o backend atualiza o status para `Ready`, movendo a tarefa para a seção de tarefas prontas.

**Acceptance Scenarios**:

1. **Given** uma tarefa com status `Draft`, **When** o usuário informa uma estimativa de duração positiva (ex.: `30` minutos) e salva, **Then** o backend define a estimativa e transiciona a tarefa para o status `Ready`.
2. **Given** uma tarefa com status `Draft`, **When** o usuário tenta informar uma estimativa menor ou igual a zero (ex.: `0` ou `-15` minutos), **Then** o sistema rejeita a alteração com erro de validação claro e mantém a tarefa como `Draft`.
3. **Given** uma tarefa com status `Draft`, **When** o sistema inspeciona se ela pode ser consumida pelo planejamento, **Then** a tarefa `Draft` é explicitamente inelegível para alocação diária.

---

### User Story 3 - Edição e Ciclo de Vida da Tarefa (InProgress e Done) (Priority: P2)

Como um usuário executando suas atividades, quero poder atualizar detalhes da tarefa (título, estimativa, deadline) e avançar seu ciclo de vida iniciando o trabalho (`InProgress`) ou concluindo a tarefa (`Done`), garantindo que apenas transições válidas sejam aceitas pelo backend.

**Why this priority**: Permite o acompanhamento e a conclusão do ciclo de vida das tarefas diretamente na Inbox, preparando o terreno para a posterior integração com o módulo de execução.

**Independent Test**: Pegar uma tarefa `Ready`, clicar em "Iniciar", verificar a transição para `InProgress`, e em seguida clicar em "Concluir", constatando a transição para `Done` com auditoria e bloqueio de reversões ilegais.

**Acceptance Scenarios**:

1. **Given** uma tarefa existente, **When** o usuário edita seu título, ajusta a estimativa em minutos ou define/altera uma data limite (`deadline`), **Then** o backend persiste as alterações e reflete os dados atualizados na Inbox.
2. **Given** uma tarefa no status `Ready`, **When** o usuário aciona a ação de iniciar, **Then** o backend valida a transição e altera o status para `InProgress`.
3. **Given** uma tarefa no status `InProgress` ou `Ready`, **When** o usuário aciona a ação de concluir, **Then** o backend altera o status para `Done` e registra a conclusão.
4. **Given** uma tarefa no status `Draft` (sem estimativa), **When** uma tentativa de transição direta para `InProgress` for enviada, **Then** o backend rejeita a operação com erro de regra de negócio (`400 Bad Request`), proibindo o avanço sem estimativa prévia.

---

### User Story 4 - Resiliência, Feedback Visual e Estados Vazios (Priority: P2)

Como um usuário navegando pela Inbox, quero receber feedback visual imediato durante o carregamento de dados, mensagens informativas em seções vazias e avisos claros de erro caso uma operação falhe, garantindo uma experiência estável e confiável.

**Why this priority**: Evita incertezas na interface, melhora a usabilidade e garante que falhas de rede ou validação sejam comunicadas de forma transparente sem quebrar a aplicação.

**Independent Test**: Acessar a Inbox sem tarefas cadastradas e verificar que cada coluna/seção exibe uma mensagem de estado vazio convidativa; tentar realizar uma operação offline ou inválida e verificar a exibição de toast/alerta de erro.

**Acceptance Scenarios**:

1. **Given** que não há tarefas cadastradas para um determinado status, **When** a Inbox é renderizada, **Then** a seção correspondente exibe uma mensagem explicativa de estado vazio amigável.
2. **Given** que uma requisição de busca ou mutação está em andamento, **When** o usuário visualiza a interface, **Then** indicadores de carregamento não-bloqueantes são exibidos.
3. **Given** que o backend retorna um erro de validação ou conflito de transição, **When** a resposta chega ao frontend, **Then** a mensagem de erro fornecida pelo backend é apresentada em destaque sem recarregar nem quebrar o estado da tela.

---

### Edge Cases

- **Título em branco ou apenas espaços**: A tentativa de criar ou renomear uma tarefa com título vazio ou composto apenas por espaços é rejeitada na validação com mensagem clara.
- **Deadline no passado**: Definir um deadline anterior à data atual é permitido (representa tarefa vencida/atrasada), mas deve ser sinalizado visualmente como atrasado quando não estiver `Done`.
- **Remoção de estimativa de uma tarefa `Ready`**: Caso a estimativa de uma tarefa `Ready` seja explicitamente removida (definida como nula), o backend rebaixa seu status de volta para `Draft`.
- **Tentativa de alterar tarefa já concluída (`Done`)**: O backend bloqueia edições de título, estimativa ou reabertura não autorizada em tarefas finalizadas sem fluxo explícito de reativação.
- **Navegação com teclado na Inbox**: O formulário de criação rápida e os cards de tarefas são totalmente navegáveis por teclado (Tab, Enter para submissão/ações, Espaço para seleção).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir a criação rápida de uma tarefa fornecendo apenas o título.
- **FR-002**: Toda tarefa criada sem estimativa de duração MUST ser inicializada com o status `Draft`.
- **FR-003**: O sistema MUST validar que qualquer estimativa de duração informada seja um número inteiro positivo de minutos (`DurationMinutes > 0`).
- **FR-004**: O backend MUST transicionar automaticamente uma tarefa do status `Draft` para `Ready` quando uma estimativa de duração positiva for atribuída a ela.
- **FR-005**: O backend MUST transicionar uma tarefa do status `Ready` para `Draft` caso sua estimativa de duração seja removida (definida como ausente).
- **FR-006**: O sistema MUST impedir que tarefas no status `Draft` sejam consideradas candidatas para planejamento diário.
- **FR-007**: O sistema MUST permitir a edição de título, estimativa de duração em minutos e data limite (`deadline` opcional).
- **FR-008**: O backend MUST suportar as transições de ciclo de vida:
  - `Draft` -> `Ready` (via adição de estimativa)
  - `Ready` -> `Draft` (via remoção de estimativa)
  - `Ready` -> `InProgress` (via comando de início)
  - `InProgress` -> `Done` (via comando de conclusão)
  - `Ready` -> `Done` (via comando direto de conclusão rápida)
- **FR-009**: O backend MUST rejeitar qualquer transição de ciclo de vida ilegal (ex.: `Draft` -> `InProgress`, ou qualquer transição a partir de `Done`) retornando erro `400 Bad Request` com ProblemDetails.
- **FR-010**: O backend MUST ser a única autoridade definidora do status e das transições de estado; o frontend MUST NOT calcular nem forçar transições localmente.
- **FR-011**: O backend MUST persistir as tarefas no banco de dados PostgreSQL utilizando o schema dedicado `planning`.
- **FR-012**: O backend MUST gerar identificadores únicos para as tarefas utilizando UUIDv7 sequencial no backend (`Guid.CreateVersion7()`).
- **FR-013**: O frontend MUST disponibilizar a rota `/planning` apresentando a Inbox com as tarefas organizadas e filtráveis pelos status `Draft`, `Ready`, `InProgress` e `Done`.
- **FR-014**: O frontend MUST recuperar todas as tarefas via consulta remota ao backend (utilizando TanStack Vue Query), garantindo persistência dos dados após recarregamentos (F5) sem dados mockados.
- **FR-015**: O frontend MUST exibir mensagens de estado vazio apropriadas quando não houver tarefas em uma categoria, indicadores visuais de loading e feedback descritivo em caso de erros.
- **FR-016**: Esta feature MUST NOT implementar antecipadamente entidades de `Projects`, `Habits`, `DailyPlan` ou integrações avançadas de calendário.

---

### Key Entities *(include if feature involves data)*

- **Task (Agregado Raiz no módulo Planning)**:
  - *Id*: Identificador único sequencial UUIDv7 gerado pelo backend.
  - *Title*: String obrigatória (1 a 255 caracteres).
  - *Description*: String opcional com notas adicionais.
  - *DurationMinutes*: Inteiro nulo ou positivo representando a estimativa de tempo em minutos.
  - *Deadline*: `DateTimeOffset?` em UTC indicando a data/hora limite opcional de conclusão.
  - *Status*: Enum com valores `Draft`, `Ready`, `InProgress`, `Done`.
  - *CreatedAt / UpdatedAt*: Marcas temporais de auditoria em UTC (`DateTimeOffset`).
  - *CompletedAt*: Marca temporal em UTC (`DateTimeOffset?`) registrada quando o status passa para `Done`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: O usuário consegue capturar uma nova tarefa de rascunho a partir da tela `/planning` em menos de 5 segundos.
- **SC-002**: 100% das tarefas persistidas são restauradas corretamente do backend após recarregamento de página (F5) sem perdas ou divergências de estado.
- **SC-003**: 100% das tentativas de atribuir estimativas não-positivas (<= 0) ou transições ilegais de ciclo de vida são bloqueadas pelo backend com mensagens de erro claras.
- **SC-004**: Ao adicionar uma estimativa válida a um `Draft`, a tarefa transiciona para `Ready` no backend e é atualizada na interface em menos de 1 segundo.
- **SC-005**: Toda a navegação básica e captura rápida na Inbox pode ser realizada integralmente via teclado (Tab, Enter, Espaço).

---

## Assumptions

- O usuário já possui um perfil inicial configurado no Compass (concluído na feature `001-initial-onboarding`), permitindo navegar entre as rotas da aplicação.
- A tela `/planning` será acessível na navegação principal do Compass ao lado da tela "Hoje" (`/today`).
- As marcas de tempo e datas limites (`deadline`) enviadas e recebidas pela API são normalizadas em UTC (`DateTimeOffset`).
- A Inbox agrupa inicialmente tarefas individuais simples sem dependências de sub-tarefas, projetos ou recorrências (que serão entregues em features futuras dedicadas).
