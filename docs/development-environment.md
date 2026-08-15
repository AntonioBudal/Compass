# Ambiente de Desenvolvimento

Este documento descreve como preparar e executar o ambiente local do projeto Compass.

## Pré-requisitos
*   **.NET SDK:** >= 8.0 (Recomendado 10.0+)
*   **Node.js:** >= 20.x LTS
*   **Podman:** (Substitui o Docker Desktop).
*   **Git**

## Configuração Inicial

1. **Variáveis de Ambiente:**
   Copie o arquivo `.env.example` na raiz para um novo arquivo local e configure as strings se necessário.
   * `backend`: Use a string de conexão no `appsettings.Development.json`.
   * `frontend`: Crie um `.env.local` na pasta `src/frontend` definindo `VITE_API_URL`.

2. **Iniciar o Banco de Dados (PostgreSQL):**
   O projeto utiliza um banco de dados PostgreSQL rodando em container.
   ```bash
   # Criar o volume (se for a primeira vez)
   podman volume create compass_pgdata

   # Iniciar o container
   podman run -d --name compass-postgres -e POSTGRES_USER=compass_dev -e POSTGRES_PASSWORD=compass_password -e POSTGRES_DB=compass_db -p 5432:5432 -v compass_pgdata:/var/lib/postgresql/data docker.io/library/postgres:16-alpine