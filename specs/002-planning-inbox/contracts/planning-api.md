# API Contracts: Planning Module (Tasks & Inbox)

**Feature**: `002-planning-inbox` | **Status**: Approved

## Base Path: `/api/planning`

---

### 1. Create / Capture Task

- **Method / Path**: `POST /api/planning/tasks`
- **Description**: Cria uma nova tarefa na Inbox. Se `durationMinutes` for omitido, a tarefa nasce como `Draft`. Se for informado (`> 0`), nasce como `Ready`.

#### Request Body
```json
{
  "title": "Escrever documentação técnica",
  "description": "Detalhar os novos endpoints",
  "durationMinutes": 45,
  "deadline": "2026-08-30T21:00:00Z"
}
```
*(Nota: `description`, `durationMinutes` e `deadline` são opcionais)*

#### Responses

- **`201 Created`**
  - **Headers**: `Location: /api/planning/tasks/01918a30-0000-7000-8000-000000000001`
  - **Body**:
    ```json
    {
      "id": "01918a30-0000-7000-8000-000000000001",
      "title": "Escrever documentação técnica",
      "description": "Detalhar os novos endpoints",
      "durationMinutes": 45,
      "deadline": "2026-08-30T21:00:00Z",
      "status": "Ready",
      "createdAt": "2026-08-28T23:10:00Z",
      "updatedAt": "2026-08-28T23:10:00Z",
      "completedAt": null
    }
    ```

- **`400 Bad Request`**
  - **Body**: `ProblemDetails` com erro de validação (ex.: título vazio, estimativa <= 0).

---

### 2. List Tasks (Inbox)

- **Method / Path**: `GET /api/planning/tasks`
- **Query Parameters**:
  - `status` (opcional): `Draft`, `Ready`, `InProgress`, `Done`. Se omitido, retorna todas as tarefas.
- **Description**: Lista as tarefas cadastradas na Inbox ordenadas por data de criação.

#### Responses

- **`200 OK`**
  - **Body**:
    ```json
    [
      {
        "id": "01918a30-0000-7000-8000-000000000001",
        "title": "Escrever documentação técnica",
        "description": "Detalhar os novos endpoints",
        "durationMinutes": 45,
        "deadline": "2026-08-30T21:00:00Z",
        "status": "Ready",
        "createdAt": "2026-08-28T23:10:00Z",
        "updatedAt": "2026-08-28T23:10:00Z",
        "completedAt": null
      }
    ]
    ```

---

### 3. Get Task By ID

- **Method / Path**: `GET /api/planning/tasks/{id:guid}`
- **Description**: Recupera os detalhes de uma tarefa específica.

#### Responses

- **`200 OK`**: Retorna o payload `TaskDto`.
- **`404 Not Found`**: ProblemDetails indicando que a tarefa não existe.

---

### 4. Update Task Details

- **Method / Path**: `PATCH /api/planning/tasks/{id:guid}`
- **Description**: Atualiza título, descrição, estimativa ou deadline da tarefa.

#### Request Body
```json
{
  "title": "Escrever documentação técnica revisada",
  "description": "Atualizado com novos exemplos",
  "durationMinutes": 60,
  "deadline": "2026-08-31T18:00:00Z"
}
```

#### Responses

- **`200 OK`**: Retorna o `TaskDto` atualizado.
- **`400 Bad Request`**: ProblemDetails com erro de validação.
- **`404 Not Found`**: ProblemDetails indicando tarefa não encontrada.

---

### 5. Start Task (Lifecycle)

- **Method / Path**: `POST /api/planning/tasks/{id:guid}/start`
- **Description**: Inicia uma tarefa que esteja com status `Ready`, transicionando-a para `InProgress`.

#### Responses

- **`200 OK`**: Retorna o `TaskDto` com `status: "InProgress"`.
- **`400 Bad Request`**: ProblemDetails caso a tarefa não esteja em `Ready` (ex.: se for `Draft` ou já `Done`).
- **`404 Not Found`**: ProblemDetails indicando tarefa não encontrada.

---

### 6. Complete Task (Lifecycle)

- **Method / Path**: `POST /api/planning/tasks/{id:guid}/complete`
- **Description**: Conclui uma tarefa que esteja em `Ready` ou `InProgress`, transicionando-a para `Done` e registrando `completedAt`.

#### Responses

- **`200 OK`**: Retorna o `TaskDto` com `status: "Done"` e `completedAt` preenchido.
- **`400 Bad Request`**: ProblemDetails caso a transição seja ilegal.
- **`404 Not Found`**: ProblemDetails indicando tarefa não encontrada.
