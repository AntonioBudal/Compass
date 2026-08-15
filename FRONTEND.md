````markdown
# Frontend (Vue 3 + TypeScript)

O frontend do Compass não é um mero exibidor de JSONs. Ele é uma interface de execução tática, de baixíssima latência e foco absoluto, projetada para se comportar como uma ferramenta utilitária e industrial.

Construído com **Vue 3 (Composition API) e TypeScript**, a arquitetura do cliente baseia-se em um **Feature-Sliced Design (FSD) Adaptado**, focando na separação rigorosa entre estado remoto, estado local e apresentação.

## 1. Feature-Sliced Design (FSD) Adaptado

A estrutura de diretórios rompe com o clássico modelo "components/views" para agrupar o código por escopo e nível de responsabilidade.

```text
src/
├── app/          # Inicialização, estilos globais e roteamento (Router, App.vue)
├── widgets/      # Composição complexa de features (Ex: Modo de Execução completo)
├── features/     # Lógica de negócio, interações e mutações (API/Vue Query)
├── entities/     # Modelos (DTOs de leitura) e componentes visuais de apresentação puros
└── shared/       # Ferramentas independentes, basekit UI genérico e infraestrutura (Axios)
````

### Regra de Dependência (Fluxo de Importação)

As importações só podem fluir **de cima para baixo** no diagrama de camadas.

* `app` pode importar `widgets`, `features`, `entities`, `shared`.
* `widgets` pode importar `features`, `entities`, `shared`.
* `features` pode importar `entities`, `shared`.
* `entities` pode importar apenas `shared`.
* **Violação:** Uma camada inferior tentar importar algo de uma camada superior (ex: `entities` importando de `features`).

### Responsabilidades Específicas

* **`entities/` (Modelos + Apresentação):** Não possuem estado de servidor (*stores*) e não realizam chamadas HTTP. Contêm apenas as interfaces TypeScript (refletindo os contratos da API) e componentes "burros" (ex: `TaskCard.vue`) que reagem a `props` e emitem `emits`.
* **`features/` (Comportamento / Interação):** Onde os dados ganham vida. Um componente como `RecommendationList.vue` invoca os *composables* do Vue Query para buscar os dados e despacha comandos (ex: "Log Work").
* **`widgets/` (Composição):** Agrupam múltiplas features para formar painéis de alto nível, como o `ExecutionModeBoard` ou o `PlanningBoard`.
* **`shared/` (Infra e Genéricos):** Componentes universais (Botões, Inputs, Command Palette abstrata), formatação de datas, cliente Axios e configuração do Vue Query.

## 2. Gerenciamento de Estado

O Compass diferencia estritamente o estado que pertence ao servidor do estado que pertence à tela do usuário.

### Vue Query (Server-State)

**É o dono absoluto do estado do backend.** Não replicamos o banco de dados em *stores* globais gigantescas.

* O Vue Query gerencia o *fetching*, *caching*, invalidação e *refetch* automático (ex: ao focar na janela do navegador).
* Se a lista de projetos precisa ser acessada em múltiplas telas, o Vue Query serve a resposta em cache instantaneamente, mantendo a responsabilidade sincronizada com a rede.

### Pinia (UI / Local-State)

Restrito exclusivamente para **estado global puramente local**.

* Controla preferências de sessão, *Dark Mode*, ou estados de interface que cruzam componentes (ex: estado de abertura de um Command Palette global ou painéis de navegação).
* **Proibido:** Armazenar DTOs de `Tasks` ou `Recommendations` no Pinia.

## 3. Resiliência Offline e Optimistic Updates

Para garantir a sensação de latência zero (resposta imediata), o Compass utiliza mutações otimistas para ações críticas, como completar uma tarefa.

1. **Optimistic Update:** Ao clicar em "Concluir", a camada `features` intercepta a ação e atualiza o cache local do Vue Query instantaneamente. O item some da tela em ~1ms.
2. **Offline Queue (Fila de Sincronização):** A intenção de mutação é enviada para uma fila em *background* gerenciada pelo Vue Query.
3. **Processamento e Rede:**

   * **Sucesso:** O servidor confirma (`204 No Content`). Nenhuma mudança visual extra ocorre.
   * **Offline:** Se não houver internet, a UI permanece atualizada (otimista), e a fila agenda um *retry* passivo para quando a conexão retornar.
   * **Rejeição (Conflito):** Se o servidor rejeitar (ex: `409 Conflict` ou erro de regra de negócio), o Vue Query faz o **rollback** automático. O item volta para a tela, o cache é invalidado e uma notificação sutil avisa o usuário do conflito.

*(Consulte o [DECISIONS.md](./DECISIONS.md) para ver como o Motor de Decisão é tratado no cenário offline).*

## 4. Filosofia Visual e Sistema de Cores (CSS Variables)

A interface é utilitária, discreta e focada em produtividade calma. **Fica proibido o uso de paletas decorativas, "IA-slop" (ex: gradientes brilhantes genéricos), ilustrações infantis ou gamificação supérflua.**

### Modo Escuro e Tipografia

* O sistema é desenhado em *Dark Mode* por padrão (alto contraste, tipografia legível, bordas de 1px sutis).
* Inspiração: Ferramentas de alta densidade técnica (Linear, Raycast, Cursor).

### Variáveis CSS Semânticas

Não utilizamos classes de cor absolutas *hardcoded* (ex: `bg-red-500` do Tailwind). Toda cor referenciará as variáveis globais, permitindo alteração de tema (Light Mode ou Temas de Alto Contraste) instantaneamente.

* `--color-canvas`: O fundo absoluto do app.
* `--color-surface-1`, `2`, `3`: Elevações hierárquicas (painéis, hover, modais).
* `--color-text-primary`, `secondary`, `muted`: Hierarquia de leitura.
* `--color-border`: Delimitações e separadores estruturais.
* `--color-accent`: O tom unificado de foco / ação principal.
* `--color-danger`: Sinais de atraso (`Overdue`) ou ações destrutivas (Soft-Delete).

## 5. Os Dois Modos da Interface

Para proteger o usuário da fadiga de decisão, a UI impõe uma barreira física e visual entre organizar e executar.

### Modo Planning (A Estratégia)

* **Foco:** Alta densidade de informação. Tabelas, listas completas, hierarquia de projetos.
* **Permissões:** O usuário pode criar, editar durações, repriorizar, arrastar e soltar projetos e tarefas livremente.
* **Interação:** Pesado uso de navegação por teclado e edição em lote.

### Modo Execution (O Motor Ativo)

* **Foco:** Isolamento absoluto. Oculta menus laterais e o restante do backlog.
* **Visualização:** Renderiza estritamente as recomendações fornecidas pelo `DecisionEngine` do backend.
* **Explicabilidade:** Exibe claramente o *Reasoning* (motivo da escolha, ex: "Urgência por Prazo") para manter a transparência, mas oculta o *Score* matemático técnico da visualização primária.
* **Permissões:** Estritamente *Read-Only* para alterações estruturais. O usuário não pode repriorizar projetos aqui. Ele só tem dois caminhos: **Agir** (iniciar timer, logar trabalho) ou **Descartar/Pausar** momentaneamente a ação recomendada.

*As restrições de código relativas a importações do FSD estão listadas no [DEVELOPMENT_RULES.md](./DEVELOPMENT_RULES.md). Os endpoints consumidos pelo frontend estão descritos no [API.md](./API.md).*

```
```
