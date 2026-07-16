# 06. API (Endpoints REST & Payloads JSON)
**Versão:** v1 (`/api/v1`) | **Erros:** RFC 7807 (Problem Details) | **Auth:** Bearer Token JWT  

## 1. Mapa de Rotas Principais
| Módulo | Método | Rota Endpoint | Descrição Funcional |
| :--- | :--- | :--- | :--- |
| **Decision Engine** | `GET` | `/api/v1/decisions/now` | **[Coração]** Calcula e retorna o Top 3 para o minuto atual. |
| **Commitments** | `GET` | `/api/v1/commitments/today` | Lista a agenda e itens previstos para hoje. |
| **Commitments** | `POST` | `/api/v1/commitments` | Criação estruturada via botões de Arquétipo. |
| **Commitments** | `POST` | `/api/v1/commitments/quick-parse` | Parser determinístico de atalhos em string. |
| **Commitments** | `PATCH` | `/api/v1/commitments/{id}/status` | Transição de Máquina de Estados (dispara cascatas). |
| **Focus/Telemetry**| `POST` | `/api/v1/focus/start` / `/complete` | Gerencia cronômetro e registra telemetria de desvio. |
| **Strategy** | `GET` | `/api/v1/projects` / `/goals` | Lista árvores estratégicas com progresso dinâmico. |

## 2. Contratos JSON (Exemplos)
### 2.1. `GET /api/v1/decisions/now` (Resposta HTTP 200)
```json
{
  "generatedAt": "2026-07-15T22:58:54Z",
  "context": {
    "effectiveEnergy": 3,
    "availableWindowMinutes": 45,
    "activeHardBlocker": { "title": "Reunião", "startsAt": "2026-07-15T23:45:00Z" }
  },
  "topFocus": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Praticar Composition API no código",
    "type": "TASK",
    "estimatedDurationMinutes": 30,
    "energyRequired": 3,
    "projectName": "Aprender Vue.js 3",
    "scorePercentage": 98.5,
    "reason": "⚡ Alinhado ao seu objetivo de carreira e cabe nos seus 45m livres."
  },
  "alternatives": [
    {
      "id": "7bc92f11-8821-4321-a1bc-1a2b3c4d5e6f",
      "title": "Ler artigo sobre Banco de Dados",
      "type": "HABIT",
      "estimatedDurationMinutes": 15,
      "energyRequired": 2,
      "scorePercentage": 85.0,
      "reason": "🔄 Hábito diário pendente (Streak atual: 4 dias)."
    }
  ]
}

2.2. Exemplo de Erro RFC 7807 (HTTP 409 Conflict - Colisão de Horário)
JSON

{
  "type": "[https://compass.app/errors/domain/event-overlap](https://compass.app/errors/domain/event-overlap)",
  "title": "Conflito de Agenda Detectado",
  "status": 409,
  "detail": "Não é possível criar o evento 'Dentista' (14:00-15:00) pois colide com 'Aula de Cálculo' (14:00-16:00).",
  "instance": "/api/v1/commitments",
  "extensions": {
    "conflictingCommitmentId": "88888888-9999-0000-1111-222222222222",
    "suggestedNextFreeWindow": "2026-07-16T16:00:00Z"
  }
}

2.3. PATCH /api/v1/commitments/{id}/status (Efeitos em Cascata - HTTP 200)
JSON

{
  "commitmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "previousStatus": "IN_PROGRESS",
  "currentStatus": "COMPLETED",
  "timestamp": "2026-07-15T23:00:12Z",
  "cascadedDomainEvents": [
    {
      "eventType": "ProjectCompletedEvent",
      "entityId": "11111111-2222-3333-4444-555555555555",
      "message": "Projeto 'Aprender Vue.js 3' finalizado organicamente."
    },
    {
      "eventType": "GoalProgressUpdatedEvent",
      "entityId": "99999999-0000-0000-0000-000000000000",
      "newProgressPercentage": 50.0
    }
  ]
}