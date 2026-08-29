---
name: "feature-handoff"
description: "Finalize and document a converged feature by validating quality gates, generating docs/handoffs/<id>-<feature>.md, and updating docs/PROJECT_STATE.md without modifying production code or committing automatically."
compatibility: "Requires spec-kit project structure with .specify/ and docs/ directories"
metadata:
  author: "compass-sdd"
  source: "workflows/feature-handoff.md"
---

## User Input

```text
$ARGUMENTS
```

## Goal

Finalizar o ciclo de desenvolvimento de uma feature concluída e convergida, executando os quality gates previstos, registrando os resultados exatos e gerando a documentação técnica e funcional padronizada em `docs/handoffs/<id>-<feature>.md` e `docs/PROJECT_STATE.md`.

## Operating Constraints & Critical Rules

1. **NÃO ALTERAR CÓDIGO DE PRODUÇÃO**: O comando opera em modo estrito de documentação. É proibido alterar, criar ou deletar qualquer código em `src/`, `frontend/src/` ou qualquer outro diretório de aplicação.
2. **NÃO FAZER COMMIT AUTOMATICAMENTE**: O comando NÃO executa `git commit`, `git push` ou operações destrutivas no repositório.
3. **PARAR SE HOUVER FALHAS OU INFORMAÇÕES NÃO COMPROVADAS**: Se qualquer teste unitário, de integração, E2E ou build falhar, ou se houver informação não verificável no código/artefatos, o fluxo DEVE ser interrompido imediatamente com diagnóstico claro.
4. **NÃO INVENTAR RESULTADOS**: Nunca use frases genéricas como "tudo está perfeito". Registre sempre os números exatos e resultados reais comprovados pela execução dos comandos.
5. **CONSTITUIÇÃO É SOBERANA**: O documento `.specify/memory/constitution.md` dita a arquitetura, limites modulares e governança do projeto.

---

## Execution Steps

### 1. Confirmar Convergência da Feature
- Identificar a feature ativa executando `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks` (ou inspecionando `.specify/feature.json` e `specs/`).
- Ler `tasks.md` da feature ativa e verificar se 100% das tarefas estão marcadas como concluídas (`[X]`).
- Verificar se não há seções de convergência pendentes ou incompletas. Se houver tarefas abertas (`[ ]`), ABORTAR o comando informando as tarefas pendentes que exigem `/speckit-implement` ou `/speckit-converge`.

### 2. Inspecionar Git e Artefatos do Spec Kit
- Inspecionar o estado do Git (`git status -s`, `git branch --show-current`, `git diff`). Usar o Git como fonte das mudanças literais e os artefatos como explicação semântica.
- Ler e cruzar as informações de:
  - `.specify/memory/constitution.md`
  - `specs/<feature>/spec.md` (Requisitos FR-###, Success Criteria SC-###, User Stories, Edge Cases)
  - `specs/<feature>/plan.md` e `research.md` (Decisões arquiteturais e de domínio)
  - `specs/<feature>/contracts/` (Contratos HTTP e DTOs)
  - Migrations, schemas de banco e mapeamentos EF Core
  - Componentes, rotas e composables de frontend
  - Testes relacionados (backend e frontend)

### 3. Executar os Quality Gates Previstos
- Executar os testes automatizados do backend:
  ```powershell
  dotnet test Compass.slnx --logger "console;verbosity=normal"
  ```
- Executar os testes automatizados do frontend e verificação de tipos/build:
  ```powershell
  npm test -- --run (dentro de frontend/)
  npm run build (dentro de frontend/)
  ```
- Capturar a saída real e métricas exatas (total de testes, aprovados, falhos, duração e status do build). Se houver qualquer erro, PARAR imediatamente e reportar a falha.

### 4. Gerar o Handoff da Feature (`docs/handoffs/<id>-<feature>.md`)
Criar o arquivo `docs/handoffs/<id>-<feature>.md` estruturado obrigatoriamente com os seguintes tópicos:
- **ID e nome da spec**
- **Data** (ISO 8601 ou YYYY-MM-DD)
- **Branch ou worktree**
- **Status**: Concluída, Parcial ou Bloqueada
- **Problema resolvido**
- **Comportamento entregue ao usuário**
- **Critérios de aceitação atendidos** (rastreados por User Story / Acceptance Criteria)
- **Critérios não atendidos** (ou declaração explícita de 100% atendidos)
- **Decisões de domínio**
- **Decisões arquiteturais**
- **Entidades e invariantes criadas ou alteradas**
- **Commands, Queries e Handlers**
- **Contratos cross-module**
- **Endpoints HTTP** (tabela com Método, Rota, Request Body, Response Body, Status Codes)
- **Tabelas e migrations** (schema, colunas, chaves, tipos)
- **Rotas e telas frontend**
- **Query keys e invalidações** (Vue Query)
- **Arquivos criados**
- **Arquivos modificados**
- **Arquivos removidos**
- **Testes adicionados**
- **Comandos de validação executados**
- **Resultados exatos dos testes**
- **Divergências entre spec, plan, tasks e implementação**
- **Débitos técnicos**
- **Riscos conhecidos**
- **Instruções para executar a feature**
- **Próximo passo recomendado**

*Importante*: Não copie blocos gigantes de código. Use caminhos de arquivos e assinaturas relevantes.

### 5. Atualizar `docs/PROJECT_STATE.md`
Atualizar o arquivo `docs/PROJECT_STATE.md` mantendo-o conciso e restrito exclusivamente a:
- **Visão atual do produto**
- **Stack**
- **Arquitetura e fronteiras**
- **Nomenclatura oficial**
- **Features concluídas**
- **Feature ativa** (ou "Nenhuma no momento")
- **Módulos existentes**
- **Contratos importantes**
- **Migrations atuais**
- **Telas e rotas atuais**
- **Estado dos testes** (métricas reais consolidadas)
- **Bloqueadores**
- **Débitos aceitos**
- **Próximo passo exato**

### 6. Relatório Final de Handoff
Apresentar ao usuário o resumo das ações realizadas, links diretos para os arquivos em `docs/` e o próximo comando recomendado no fluxo SDD.
