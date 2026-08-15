```markdown
# Modelo de Domínio do Compass

Este documento descreve o modelo conceitual de domínio do Compass. O sistema é dividido em três contextos lógicos (`Planning`, `Calendar`, `Execution`) que colaboram sem compartilhar o mesmo espaço restrito no banco de dados.

## 1. Hierarquia de Planejamento

O planejamento no Compass ocorre de cima para baixo, partindo da intenção estratégica até a ação atômica.

A estrutura base é:

**Goal (Objetivo Macro) → Project (Agrupamento Estratégico) → Task (Unidade de Ação)**

### Conceitos Paralelos (Fora da Hierarquia Direta)

Além da hierarquia principal, o domínio lida com os seguintes conceitos fundamentais:

- **Habit:** Intenções recorrentes, modeladas relacionalmente (base, frequência e exceções) para responder rapidamente o que deve ser feito hoje.
- **Commitment:** Blocos de tempo fixos, inegociáveis e já ocupados na agenda real do usuário.
- **Schedule Profile:** Configuração base de quando o usuário pretende estar operando (janelas de tempo).
- **Daily Cycle:** A instância operacional e matemática de um dia específico.
- **Execution Log (Journal):** O registro histórico de tempo gasto. É um *Aggregate Root* independente.
- **Recommendation:** O artefato de saída do motor de decisão. Não é persistido.

## 2. Distinções Fundamentais

A arquitetura do Compass é estritamente pragmática quanto aos conceitos e terminologias para evitar *anti-patterns* de modelagem.

### 2.1 Entidade Persistida vs. Resultado Efêmero

- **Task:** É a entidade salva no banco de dados, pertencente ao módulo `Planning`.
- **Recommendation:** É um objeto gerado *em memória* pelo `DecisionEngine` no momento da requisição. Nunca é salvo no banco de dados.

### 2.2 Estado vs. Condição Derivada

Para evitar a explosão da máquina de estados (ex: criar status combinados infinitos), o Compass separa "o que a entidade é" (Estado) de "o que concluímos sobre ela" (Condição).

- **Estado:** O estágio formal do ciclo de vida da tarefa (`Ready`, `InProgress`, etc.). Fica salvo no banco.
- **Condição Derivada:** Uma característica momentânea resultante da matemática e do tempo.
  - *Exemplo Central:* **Overdue (Atrasada) não é um estado.** É uma condição matemática onde o estado da tarefa é `Ready` e seu `HardDeadline < Now`.

## 3. Entidade: Task (Tarefa)

A `Task` é a unidade de intenção atômica pertencente ao módulo de `Planning`.

### Ciclo de Vida (Lifecycle)

O fluxo de estado garante que apenas intenções com parâmetros definidos alcancem o motor de decisão.

**Draft → Ready → InProgress → Completed | Archived**

### Atributos Críticos

- **EstimatedDuration:** O tempo (em minutos) estimado para a ação. É **obrigatório** para que a tarefa saia do estado `Draft` e avance para `Ready`.
- **HardDeadline:** O prazo final inegociável. É o principal fator impulsionador do cálculo de urgência do motor.
- **Project:** Referência para a entidade agregadora pai, da qual herda relevância estratégica.
- **Soft Delete:** Ocultamento lógico da tarefa. Tarefas nunca são deletadas fisicamente, garantindo que os apontamentos históricos no *Execution Log* nunca fiquem órfãos.

## 4. Entidade: Project (Projeto)

O `Project` agrupa tarefas, fornece contexto estratégico e atua como uma alavanca macro no fluxo de execução.

### Características

- **Relação com Goal:** Descende de um `Goal`, de onde pode herdar um "peso estratégico base".
- **Relevância Estratégica:** A relevância do projeto impacta o Score final de todas as suas tarefas filhas.
- **Estados:** `Planning`, `Active`, `Paused`, `Completed`, `Archived`.
- **Pausa Estratégica:** Se o status de um projeto for alterado para `Paused`, todas as `Tasks` filhas perdem elegibilidade no *Decision Engine*, sumindo das recomendações instantaneamente.
- **Conclusão Desacoplada:** A conclusão da última tarefa de um projeto **não** aciona automaticamente o status `Completed` do projeto em si. O encerramento de um projeto exige uma ação consciente (intenção de encerrar).

## 5. Entidade: Habit (Hábito)

Diferente do cronograma de calendário, hábitos são intenções recorrentes pertencentes ao `Planning`.

### Modelagem Relacional

Para evitar carregar documentos JSON complexos na memória apenas para descobrir as tarefas de hoje, o Hábito é fragmentado e normalizado no banco:

- **Habit Base:** Dados fundamentais (Nome, Duração Estimada).
- **Habit Frequencies:** Regras de recorrência (ex: "Todas as terças e quintas" ou "A cada 3 dias").
- **Habit Exceptions:** Pausas programadas (ex: "Suspenso durante férias").

Essa estrutura permite que o banco responda de forma ultra-rápida (via query direta) se o hábito deve gerar uma instância de execução para o dia atual.

## 6. Contexto Temporal e Disponibilidade

O modelo lida com a realidade física do tempo através de geometria de intervalos e exclusão.

O fluxo de disponibilidade para um determinado dia ocorre na seguinte ordem:

1. **Schedule Profile:** O usuário define intenções macro de disponibilidade (ex: "Nas segundas, estou disponível das 10h às 18h").
2. **Intended Windows:** O perfil gera os blocos em potencial para o dia atual.
3. **Commitments + Buffers:** Eventos fixos da agenda real (ex: consulta médica) recebem uma margem de tempo (*Transition Buffer*) antes e depois para absorver deslocamentos.
4. **Execution Windows:** As *Intended Windows* sofrem recorte geométrico (subtração) pelos *Commitments* expandidos. O resultado (janelas contínuas fragmentadas) é o tempo efetivamente consumível pelo motor.

> **Nota:** A disponibilidade é configurável e varia por usuário, dia da semana e exceções. Não existe conceito global de "horário comercial".

## 7. Agregados de Execução (Execution Log e Daily Cycle)

O módulo de `Execution` foi projetado para evitar gargalos de concorrência e preservar o registro contínuo.

### Execution Log (O Histórico Imutável)

- **Aggregate Root Independente:** O log de execução é inserido de forma totalmente isolada. O sistema não precisa bloquear ou carregar o "Dia Inteiro" (`DailyCycle`) para registrar 15 minutos trabalhados.
- **Resiliência:** Utiliza *Soft-Links* apontando para a Tarefa e para o Dia. Possui uma cópia em texto (*Snapshot*) do título da tarefa no momento exato em que o trabalho foi feito.

### Daily Cycle (A Bússola do Dia)

- **Modelo Híbrido de Capacidade:** Enquanto o dia está aberto e ocorrendo, a capacidade livre e o déficit de tempo são dinâmicos (calculados *on-the-fly* abatendo a soma dos logs inseridos das janelas operacionais).
- **Fechamento Congelado:** Ao encerrar o ciclo, o sistema calcula o saldo exato e preenche permanentemente a propriedade `closed_capacity_deficit`. O passado fica congelado e matematicamente estanque para relatórios.

## 8. Serviço de Domínio: Decision Engine e Recommendation

O **Decision Engine** é o cérebro matemático. Ele consome o contexto de `Execution Windows` (Tempo) e a lista de `Tasks/Habits` elegíveis (Intenções), cruzando os dados e aplicando heurísticas para devolver recomendações.

### O DTO Recommendation

A recomendação não é uma cópia da tarefa, é uma instrução efêmera para o usuário. Componentes:

- `TaskId`: Identificador do alvo da ação.
- `Título` e `Projeto`: Contexto rápido.
- `Score`: Valor numérico da prioridade matemática (para ordenação técnica).
- `Reasoning`: Justificativa em texto legível (ex: "Atrasada" ou "Alta contribuição estratégica"). Proporciona explicabilidade e ganho de confiança.
- `TargetDuration` e `Indicação de Execução Parcial (Chunking)`: Se a tarefa exige 60 minutos, mas a janela de tempo livre atual possui apenas 20 minutos, o motor ajusta o `TargetDuration` para 20 minutos, recomendando avanço parcial sem alterar a estimativa total da entidade base.
```
