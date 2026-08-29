# API Contracts: Calendar Module (Schedule Profiles)

**Feature**: `001-initial-onboarding` | **Status**: Approved

## Base Path: `/api/calendar`

---

### 1. Create Schedule Profile

- **Method / Path**: `POST /api/calendar/schedule-profiles`
- **Description**: Cria um novo perfil de calendário com fuso horário IANA e disponibilidade semanal.

#### Request Body
```json
{
  "timeZoneId": "America/Sao_Paulo",
  "weeklyAvailability": [
    {
      "dayOfWeek": 1,
      "windows": [
        {
          "startTime": "09:00:00",
          "endTime": "12:00:00"
        },
        {
          "startTime": "13:00:00",
          "endTime": "18:00:00"
        }
      ]
    },
    {
      "dayOfWeek": 2,
      "windows": [
        {
          "startTime": "09:00:00",
          "endTime": "18:00:00"
        }
      ]
    }
  ]
}
```

#### Responses

- **`201 Created`**
  - **Headers**: `Location: /api/calendar/schedule-profiles/01918a22-38b2-7000-8000-000000000001`
  - **Body**:
    ```json
    {
      "id": "01918a22-38b2-7000-8000-000000000001",
      "timeZoneId": "America/Sao_Paulo",
      "weeklyAvailability": [
        {
          "dayOfWeek": 1,
          "windows": [
            {
              "startTime": "09:00:00",
              "endTime": "12:00:00"
            },
            {
              "startTime": "13:00:00",
              "endTime": "18:00:00"
            }
          ]
        },
        {
          "dayOfWeek": 2,
          "windows": [
            {
              "startTime": "09:00:00",
              "endTime": "18:00:00"
            }
          ]
        }
      ],
      "createdAt": "2026-08-28T22:30:00Z",
      "updatedAt": "2026-08-28T22:30:00Z"
    }
    ```

- **`400 Bad Request`**
  - **Body**: `ProblemDetails` com erros de validação (ex.: timezone inválido, `startTime >= endTime`).
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      "title": "Validation Failed",
      "status": 400,
      "errors": {
        "timeZoneId": ["Invalid IANA time zone identifier 'Invalid/Zone'."],
        "weeklyAvailability[0].windows[0]": ["Start time (18:00) must be earlier than end time (09:00)."]
      }
    }
    ```

---

### 2. Get Schedule Profile By ID

- **Method / Path**: `GET /api/calendar/schedule-profiles/{id:guid}`
- **Description**: Retorna os detalhes do perfil de calendário associado ao identificador fornecido.

#### Responses

- **`200 OK`**
  - **Body**:
    ```json
    {
      "id": "01918a22-38b2-7000-8000-000000000001",
      "timeZoneId": "America/Sao_Paulo",
      "weeklyAvailability": [
        {
          "dayOfWeek": 1,
          "windows": [
            {
              "startTime": "09:00:00",
              "endTime": "12:00:00"
            },
            {
              "startTime": "13:00:00",
              "endTime": "18:00:00"
            }
          ]
        }
      ],
      "createdAt": "2026-08-28T22:30:00Z",
      "updatedAt": "2026-08-28T22:30:00Z"
    }
    ```

- **`404 Not Found`**
  - **Body**:
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
      "title": "Schedule Profile Not Found",
      "status": 404,
      "detail": "Schedule profile with id '01918a22-38b2-7000-8000-000000000000' was not found."
    }
    ```

---

### 3. List Supported Time Zones

- **Method / Path**: `GET /api/calendar/timezones`
- **Description**: Retorna a lista de fusos horários IANA suportados pelo sistema.

#### Responses

- **`200 OK`**
  - **Body**:
    ```json
    [
      {
        "id": "America/Sao_Paulo",
        "displayName": "(UTC-03:00) Brasília Time",
        "baseUtcOffset": "-03:00:00"
      },
      {
        "id": "UTC",
        "displayName": "(UTC) Coordinated Universal Time",
        "baseUtcOffset": "00:00:00"
      }
    ]
    ```
