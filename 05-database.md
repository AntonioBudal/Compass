# 05. Database(PostgreSQL)
**Versão do Schema:** 1.0 (Production-Ready)  
**Estratégia:** 3NF + TPT para Camada Estratégica (`goals`/`projects`) + TPH para Camada Operacional (`commitments`)  

## 1. Tipos Customizados (Enums)
```sql
CREATE TYPE commitment_type AS ENUM ('TASK', 'EVENT', 'HABIT', 'NOTE');
CREATE TYPE commitment_status AS ENUM ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'ARCHIVED', 'BLOCKED');
CREATE TYPE goal_status AS ENUM ('ACTIVE', 'COMPLETED', 'ON_HOLD', 'CANCELLED');

2. DDL Definitivo (Tabelas)
SQL

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    time_zone VARCHAR(50) NOT NULL DEFAULT 'America/Sao_Paulo',
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE settings (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    default_energy_level SMALLINT NOT NULL DEFAULT 2 CHECK (default_energy_level BETWEEN 1 AND 3),
    theme VARCHAR(20) NOT NULL DEFAULT 'dark',
    auto_postpone_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    daily_review_time TIME NOT NULL DEFAULT '20:00:00',
    preferences_json JSONB NOT NULL DEFAULT '{}'::jsonb,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE goals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(200) NOT NULL CHECK (char_length(title) >= 3),
    why_description TEXT,
    target_date DATE,
    status goal_status NOT NULL DEFAULT 'ACTIVE',
    progress_percentage NUMERIC(5, 2) NOT NULL DEFAULT 0.00 CHECK (progress_percentage BETWEEN 0.00 AND 100.00),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE projects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    goal_id UUID REFERENCES goals(id) ON DELETE SET NULL,
    title VARCHAR(200) NOT NULL CHECK (char_length(title) >= 3),
    deadline TIMESTAMPTZ,
    status commitment_status NOT NULL DEFAULT 'PENDING',
    total_estimated_duration_minutes INTEGER NOT NULL DEFAULT 0 CHECK (total_estimated_duration_minutes >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE commitments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    project_id UUID REFERENCES projects(id) ON DELETE SET NULL,
    title VARCHAR(255) NOT NULL CHECK (char_length(title) >= 3),
    type commitment_type NOT NULL,
    status commitment_status NOT NULL DEFAULT 'PENDING',
    estimated_duration_minutes INTEGER NOT NULL DEFAULT 30 CHECK (estimated_duration_minutes >= 0),
    energy_required SMALLINT NOT NULL DEFAULT 2 CHECK (energy_required BETWEEN 1 AND 3),
    deadline TIMESTAMPTZ,
    start_time TIMESTAMPTZ,
    end_time TIMESTAMPTZ,
    location_or_link VARCHAR(500),
    cron_expression VARCHAR(100),
    current_streak INTEGER NOT NULL DEFAULT 0 CHECK (current_streak >= 0),
    best_streak INTEGER NOT NULL DEFAULT 0 CHECK (best_streak >= 0),
    postponed_count INTEGER NOT NULL DEFAULT 0 CHECK (postponed_count >= 0),
    content TEXT,
    converted_to_commitment_id UUID REFERENCES commitments(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMPTZ,
    CONSTRAINT chk_event_time_validity CHECK (
        (type != 'EVENT') OR (start_time IS NOT NULL AND end_time IS NOT NULL AND end_time > start_time)
    )
);

CREATE TABLE dependencies (
    parent_commitment_id UUID NOT NULL REFERENCES commitments(id) ON DELETE CASCADE,
    child_commitment_id UUID NOT NULL REFERENCES commitments(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (parent_commitment_id, child_commitment_id),
    CONSTRAINT chk_no_self_dependency CHECK (parent_commitment_id != child_commitment_id)
);

CREATE TABLE schedules (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    day_of_week SMALLINT NOT NULL CHECK (day_of_week BETWEEN 0 AND 6),
    work_start TIME NOT NULL,
    work_end TIME NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT chk_schedule_time_order CHECK (work_end > work_start)
);

CREATE TABLE tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL CHECK (char_length(name) >= 2),
    color_hex VARCHAR(7) NOT NULL DEFAULT '#6366F1' CHECK (color_hex ~* '^#[a-f0-9]{6}$'),
    UNIQUE (user_id, name)
);

CREATE TABLE commitment_tags (
    commitment_id UUID NOT NULL REFERENCES commitments(id) ON DELETE CASCADE,
    tag_id UUID NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    PRIMARY KEY (commitment_id, tag_id)
);

CREATE TABLE reminders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    commitment_id UUID NOT NULL REFERENCES commitments(id) ON DELETE CASCADE,
    trigger_time TIMESTAMPTZ NOT NULL,
    is_sent BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE focus_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    commitment_id UUID NOT NULL REFERENCES commitments(id) ON DELETE CASCADE,
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    actual_duration_minutes INTEGER NOT NULL CHECK (actual_duration_minutes > 0),
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_focus_time_validity CHECK (end_time > start_time)
);

3. Índices Parciais de Alta Performance
SQL

-- 1. Motor de Decisão: Busca instantânea por itens ativos
CREATE INDEX idx_commitments_now_engine 
ON commitments (user_id, status, type, estimated_duration_minutes, energy_required)
WHERE status IN ('PENDING', 'IN_PROGRESS') AND type IN ('TASK', 'HABIT');

-- 2. Busca por Eventos/Bloqueadores de agenda
CREATE INDEX idx_commitments_events_lookup 
ON commitments (user_id, start_time, end_time)
WHERE type = 'EVENT' AND status != 'ARCHIVED';

-- 3. Resolução de Grafo de Dependências
CREATE INDEX idx_dependencies_child_lookup ON dependencies (child_commitment_id);

-- 4. Disparo de Lembretes (Quartz.NET)
CREATE INDEX idx_reminders_unsent_trigger ON reminders (trigger_time) WHERE is_sent = FALSE;

-- 5. Telemetria e Projetos
CREATE INDEX idx_focus_sessions_user_history ON focus_sessions (user_id, start_time DESC);
CREATE INDEX idx_projects_user_status ON projects (user_id, status);
CREATE INDEX idx_commitments_project_id ON commitments (project_id) WHERE project_id IS NOT NULL