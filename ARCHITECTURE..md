# Visão Arquitetural do Compass

## 1. Missão do Sistema

### O problema que o Compass resolve

O Compass resolve a fadiga de decisão (*decision fatigue*), a sobrecarga cognitiva gerada pela manutenção de backlogs infinitos e o abismo entre o planejamento (o que o usuário quer fazer) e a execução (o que o usuário consegue fazer no momento atual).

Ele elimina a fricção de olhar para uma lista de dezenas de itens e ter que calcular mentalmente o que cabe na janela de tempo disponível.

### O problema que o Compass NÃO resolve

- Não é um sistema de Gestão de Conhecimento (*Second Brain* / *Zettelkasten*).
- Não é um editor de textos complexos (como o Notion).
- Não é uma ferramenta de colaboração em equipe (como o Jira).
- Não é uma agenda de *Time Blocking* rígida que quebra ao primeiro imprevisto do dia.

### Responsabilidade Central

Filtrar, avaliar e ranquear o inventário de intenções do usuário contra as restrições da realidade (tempo livre, horários fixos) para produzir recomendações determinísticas de ação.

### Diferencial

A **separação estrita entre Planejamento e Execução**.

Um gerenciador de tarefas tradicional é um repositório passivo onde o usuário busca, filtra e escolhe. O Compass atua como um motor ativo: o usuário informa seu contexto atual e o sistema assume a carga cognitiva de decidir e apresentar a melhor ação viável.

### Definição Operacional

> O Compass é um motor determinístico de execução pessoal que cruza intenções cadastradas com restrições de contexto em tempo real para responder continuamente qual é a próxima ação mais viável e relevante.

---

## 2. Princípios Arquiteturais

A arquitetura do Compass é guiada pelos seguintes princípios inegociáveis:

1. **Separação entre Planning e Execution:** O que é planejado é mutável; o que é executado é histórico imutável. A interface e os domínios refletem essa separação fisicamente.
2. **Decision Engine Determinístico:** Para as mesmas entradas (tarefas, tempo, contexto), o sistema produzirá invariavelmente a mesma saída.
3. **Multi-Tenancy Lógico (Segurança by Design):** O sistema é arquitetado desde o Dia 1 para múltiplos usuários, isolando dados através de *Global Query Filters* e índices compostos obrigatórios.
4. **Comunicação Intermodular Pragmática (O Paradoxo do JOIN):** Privilegia-se a simplicidade inicial (Fase 1: *In-memory JOINs* via *SharedKernel*) com um plano de fuga arquitetural explícito (Fase 2: *Outbox Pattern + Read Models*) ativado apenas mediante gargalos reais de latência.
5. **Evolução Segura (Day 2 Operations):** Migrações de banco de dados adotam estritamente o padrão *Expand/Contract* (sem *Drop/Rename* diretos). Testes de integração utilizam PostgreSQL real (*Testcontainers*).
6. **Ausência de IA Generativa no Núcleo de Decisão:** Modelos probabilísticos (LLMs) criam alucinações e corroem a confiança. O núcleo de ranqueamento é puramente matemático.

---

## 3. Arquitetura Geral

O sistema adota uma topologia de **Monólito Modular** no backend e **Feature-Sliced Design (FSD)** no frontend, comunicando-se através de contratos HTTP baseados em intenção.

```mermaid
flowchart TD
    subgraph Frontend [Frontend - Vue 3 + TypeScript]
        UI[Interface / Widgets]
        State[Vue Query / Server-State]
        UI <--> State
    end

    subgraph Backend [Backend - Monólito Modular .NET 8+]
        API[API Presentation / Controllers]

        subgraph Modules [Módulos de Negócio]
            Planning[Planning Module]
            Calendar[Calendar Module]
            Execution[Execution Module]
        end

        Shared[SharedKernel / Interfaces de Integração]
    end

    State <-->|Contratos HTTP/JSON| API
    API --> Modules

    Planning -.->|Implementa Contratos| Shared
    Calendar -.->|Implementa Contratos| Shared

    Execution -.->|Consome via DI em Memória| Shared