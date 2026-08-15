# Banco de Dados (PostgreSQL)

O Compass utiliza PostgreSQL como mecanismo relacional de persistência. Para sustentar a arquitetura de Monólito Modular, o banco de dados impõe regras estritas de isolamento físico e lógico, garantindo que os domínios não fiquem acoplados por relacionamentos de chave estrangeira (FK) engessados.

## 1. Princípios de Persistência

### Schemas Separados
As tabelas não residem no schema padrão (`public`). Cada módulo de negócio possui seu próprio schema explícito (`planning`, `calendar`, `execution`). Cada `DbContext` no Entity Framework Core mapeia exclusivamente para o seu schema.

### Relacionamentos e Soft-Links
*   **FKs Internas:** Chaves estrangeiras (Foreign Keys) reais só são permitidas **dentro do mesmo schema** (ex: `tasks.project_id` referenciando `projects.id`).
*   **Soft-Links Externos:** O relacionamento entre schemas diferentes ocorre via **Soft-Links**. Armazenamos o UUID da entidade remota, mas sem criar uma Constraint de FK no banco. A integridade referencial cruzada é garantida via código (regras de negócio e *Soft Delete*).

### Deleção Lógica (Soft Delete)
Nenhuma entidade de planejamento (`Task`, `Project`) sofre `DELETE` físico. Elas recebem um timestamp na coluna `deleted_at` para serem ocultadas. Isso garante que o `ExecutionLog` (que possui um Soft-Link para a Tarefa) nunca aponte para um ID inexistente, protegendo as métricas históricas.

### Concorrência Otimista (Optimistic Concurrency)
Entidades mutáveis e críticas (como `Task`) possuem uma coluna explícita `row_version`. O EF Core utiliza este *Concurrency Token* para rejeitar atualizações simultâneas (ex: vindas do mobile e do web no mesmo milissegundo), evitando dependência direta da engine interna do PostgreSQL (`xmin`).

---

## 2. Estrutura de Tabelas

As chaves primárias padrão do sistema utilizam o tipo `UUID`. Datas e horários absolutos utilizam `TIMESTAMP WITH TIME ZONE`.

### Schema: `planning`
Contém as intenções do usuário. É o schema que mais sofre mutações (edições).

*   **Tabela `projects`**
    *   `id` (UUID, PK)
    *   `title` (VARCHAR)
    *   `status` (VARCHAR) — Enum como string (`Planning`, `Active`, `Paused`, `Completed`, `Archived`)
    *   `target_completion_date` (TIMESTAMP WITH TIME ZONE, Nullable)
    *   `strategic_weight` (DECIMAL, default 1.0)
    *   `deleted_at` (TIMESTAMP WITH TIME ZONE, Nullable)

*   **Tabela `tasks`**
    *   `id` (UUID, PK)
    *   `project_id` (UUID, Nullable) — FK para `planning.projects(id)`
    *   `title` (VARCHAR)
    *   `estimated_duration` (INTEGER, Nullable) — Em minutos.
    *   `hard_deadline` (TIMESTAMP WITH TIME ZONE, Nullable)
    *   `status` (VARCHAR) — Enum como string (`Draft`, `Ready`, `InProgress`, `Completed`, `Archived`)
    *   `row_version` (VARCHAR/BYTEA) — Token de concorrência explícito
    *   `deleted_at` (TIMESTAMP WITH TIME ZONE, Nullable)

*   **Tabela `habits`**
    *   **OPEN QUESTION:** Estrutura de colunas não detalhada. Será definida quando as regras de recorrência e instanciação do domínio de hábitos forem modeladas no detalhe.

### Schema: `calendar`
Lida com as regras e restrições de tempo absoluto.

*   **Tabela `schedule_profiles`**
    *   `id` (UUID, PK)
    *   `profile_data` (**JSONB**) — Armazena a rotina de janelas por dia da semana e o *timezone* IANA como um documento. Evita modelagem relacional excessiva e ineficiente.

*   **Tabela `commitments`**
    *   `id` (UUID, PK)
    *   `title` (VARCHAR)
    *   `start_time` (TIMESTAMP WITH TIME ZONE)
    *   `end_time` (TIMESTAMP WITH TIME ZONE)
    *   `is_external_sync` (BOOLEAN)
    *   `external_id` (VARCHAR, Nullable)
    *   *Constraint:* `CHECK (end_time > start_time)` — Validação primária a nível de banco.

### Schema: `execution`
Lida com a intersecção do planejamento e tempo, gerando o histórico. Este schema é predominantemente *append-only* (somente inserções).

*   **Tabela `daily_cycles`**
    *   `user_id` (UUID) — Parte da PK Composta
    *   `date_id` (DATE) — Parte da PK Composta (Ex: '2026-08-13')
    *   *Constraint:* `PRIMARY KEY (user_id, date_id)` — Garante escalabilidade multiusuário.
    *   `capacity_deficit` (INTEGER, Default 0) — Tempo devido/estourado.
    *   `closed_at` (TIMESTAMP WITH TIME ZONE, Nullable)

*   **Tabela `execution_logs`**
    *   `id` (UUID, PK)
    *   `cycle_user_id` (UUID) — FK para `daily_cycles(user_id)`
    *   `cycle_date_id` (DATE) — FK para `daily_cycles(date_id)`
    *   `task_id` (UUID) — **Soft-Link** (sem FK constraint) apontando logicamente para `planning.tasks(id)`.
    *   `task_snapshot_title` (VARCHAR) — Cópia (Snapshot) do título da tarefa no exato momento da execução. Blinda o histórico (*Journal*) contra alterações posteriores no nome da intenção.
    *   `duration_spent` (INTEGER) — Minutos consumidos.
    *   `logged_at` (TIMESTAMP WITH TIME ZONE)
    *   `is_completion_event` (BOOLEAN) — Identifica se este registro específico ocasionou a finalização da tarefa.
    *   *Regra de Negócio Física:* É uma tabela *append-only*. Registros não devem sofrer `UPDATE`.

---

*As regras de negócio que controlam as tabelas e transações citadas acima estão definidas nos [DEVELOPMENT_RULES.md](./DEVELOPMENT_RULES.md).*
*Para consultar os fundamentos das decisões arquiteturais adotadas, acesse [DECISIONS.md](./DECISIONS.md).*