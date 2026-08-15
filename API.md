````markdown
# Contratos de API (Compass)

A API do Compass rejeita o padrão *RESTful CRUD anêmico* (como expor um genérico `PUT /api/tasks/{id}`). O design adota **CQS (Command Query Separation)** e **Task-Based UI**, focando na intenção da ação.

## 1. Padrões Transversais da API

### 1.1 Respostas de Erro (Problem Details)

O sistema implementa a RFC 7807 (`ProblemDetails`). Nenhuma exceção vaza stack trace, e falhas de domínio nunca retornam `500 Internal Server Error`.

```json
{
  "type": "https://compass.local/errors/concurrency",
  "title": "Registro Obsoleto",
  "status": 409,
  "detail": "A tarefa que você tentou alterar já foi modificada por outra sessão.",
  "instance": "/api/execution/log-work",
  "errorCode": "TASK_ALREADY_MODIFIED"
}
````

O *frontend* lida com os erros primariamente analisando o campo `errorCode`.

### 1.2 Status HTTP Padrão

* `200 OK`: Consultas realizadas com sucesso.
* `201 Created`: Criação de recurso (retornando header `Location`).
* `204 No Content`: Comandos intencionais executados com sucesso, sem retorno de estado.
* `404 Not Found`: Recurso inexistente ou arquivado (*Soft-Delete*).
* `409 Conflict`: Rejeição por controle de concorrência (`row_version`).
* `422 Unprocessable Entity`: Falha na regra de negócio (ex: logar tempo em ciclo fechado).

### 1.3 Idempotência

Ações sensíveis ou com risco de retentativa por instabilidade de rede requerem o envio do cabeçalho `Idempotency-Key` com um UUID gerado pelo cliente.

### 1.4 Paginação e Enums

* **Enums:** Trafegam estritamente como *Strings* (ex: `"Active"`, `"Paused"`) e nunca numéricos.
* **Paginação:** Endpoints de listagem de grandes volumes (como o inventário de Planning) utilizam paginação por cursor (`?after={uuid}&limit=50`). O motor de recomendações **não** possui paginação, pois retorna apenas o Top N viável.

## 2. Módulo: Planning

Endpoints que gerenciam o planejamento, intenções e estratégia.

### Criar Projeto

* **Método / Rota:** `POST /api/planning/projects`
* **Intenção:** Cria um contêiner estratégico de tarefas.
* **Request:** `{ "title": "Aprender Vue 3", "targetCompletionDate": "2026-12-31T23:59:59Z" }`
* **Response:** `201 Created`

### Criar Tarefa

* **Método / Rota:** `POST /api/planning/tasks`
* **Intenção:** Insere uma nova intenção no sistema.
* **Request:** `{ "projectId": "uuid-opcional", "title": "Ler documentação", "estimatedDurationMinutes": 60, "hardDeadline": null }`
* **Response:** `201 Created`
* **Regra:** Se `estimatedDurationMinutes` for nulo, a tarefa nasce em status `Draft`. Caso contrário, nasce como `Ready`.

### Definir Estimativa de Tempo

* **Método / Rota:** `POST /api/planning/tasks/{id}/estimate`
* **Intenção:** Atualiza a estimativa temporal (o que promove de `Draft` para `Ready` e altera o `Score`).
* **Request:** `{ "estimatedDurationMinutes": 90 }`
* **Response:** `204 No Content`
* **Erros:** `404 Not Found`, `409 Conflict`.

### Pausar Projeto

* **Método / Rota:** `POST /api/planning/projects/{id}/pause`
* **Intenção:** Suspende o projeto. Tarefas filhas somem do motor de decisão.
* **Request:** Vazio.
* **Response:** `204 No Content`

### Listar Inventário (Exemplo de Paginação)

* **Método / Rota:** `GET /api/planning/tasks`
* **Intenção:** Retorna o inventário estratégico para o front-end (Modo Planning).
* **Query Params:** `?status=Ready&after=uuid&limit=50`
* **Response:** `200 OK` (Array paginado de `TaskReadModel`).

## 3. Módulo: Calendar

Endpoints que definem restrições físicas (quando a execução não pode ocorrer).

### Configurar Perfil de Disponibilidade

* **Método / Rota:** `PUT /api/calendar/schedule-profile`
* **Intenção:** Sobrescreve o documento de rotina semanal.
* **Request:**

```json
{
  "timezoneIana": "America/Sao_Paulo",
  "days": {
    "Monday": [
      {
        "start": "08:00",
        "end": "12:00"
      },
      {
        "start": "14:00",
        "end": "18:00"
      }
    ]
  }
}
```

* **Response:** `204 No Content`

### Registrar Compromisso Fixo

* **Método / Rota:** `POST /api/calendar/commitments`
* **Intenção:** Injeta um bloqueio no calendário.
* **Request:** `{ "title": "Reunião de Alinhamento", "startTime": "2026-08-14T10:00:00Z", "endTime": "2026-08-14T11:00:00Z" }`
* **Response:** `201 Created`

## 4. Módulo: Execution

A API central de operações diárias do Compass.

### Iniciar Ciclo Diário

* **Método / Rota:** `POST /api/execution/cycle/start`
* **Intenção:** Congela a capacidade do dia baseada no perfil e nos compromissos.
* **Request:** `{ "targetDate": "2026-08-13" }`
* **Headers:** `Idempotency-Key` (obrigatório).
* **Response:** `201 Created`
* **Erros:** `422 Unprocessable Entity` (se o ciclo já foi iniciado).

### Solicitar Próximas Ações (Decision Engine)

* **Método / Rota:** `GET /api/execution/recommendations`
* **Intenção:** Aciona o ranqueamento determinístico em memória. **(Endpoint core do front-end no Modo Execução).**
* **Request:** Vazio.
* **Response:** `200 OK`

```json
[
  {
    "taskId": "uuid",
    "title": "Ler documentação do Vue Query",
    "projectTitle": "Aprender Vue 3",
    "targetDurationMinutes": 15,
    "decisionReasoning": [
      "Forte contribuição estratégica",
      "Janela apertada (Chunking)"
    ],
    "score": 87.5
  }
]
```

### Registrar Trabalho (Log Work)

* **Método / Rota:** `POST /api/execution/log-work`
* **Intenção:** Abate o tempo da capacidade atual, cria um histórico imutável e (se solicitado) aciona a transição da tarefa para concluída.
* **Request:**

```json
{
  "taskId": "uuid",
  "durationSpentMinutes": 30,
  "isCompleted": true,
  "rowVersion": "string-base64-para-concorrencia"
}
```

* **Headers:** `Idempotency-Key` (Fortemente recomendado pela fila Offline do Front-end).
* **Response:** `204 No Content`
* **Erros:**

  * `409 Conflict`: Se o `rowVersion` estiver desatualizado (ação repetida ou concorrente).
  * `422 Unprocessable Entity`: Se não houver ciclo aberto no dia.

### Consultar Ciclo Atual

* **Método / Rota:** `GET /api/execution/cycle/current`
* **Intenção:** Retorna o resumo do dia operacional.
* **Request:** Vazio.
* **Response:** `200 OK`

```json
{
  "date": "2026-08-13",
  "capacityRemainingMinutes": 120,
  "capacityDeficitMinutes": 0,
  "executionWindows": [
    {
      "start": "2026-08-13T14:00:00Z",
      "end": "2026-08-13T16:00:00Z"
    }
  ]
}
```

A comunicação entre Frontend e Backend está detalhada arquiteturalmente no [FRONTEND.md](./FRONTEND.md).

Para entender as estruturas de persistência atreladas aos fluxos desta API, consulte [DATABASE.md](./DATABASE.md).

```
```
