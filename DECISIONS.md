# Registro de Decisões Arquiteturais (ADR / DECISIONS)

Este documento consolida todas as decisões arquiteturais tomadas durante o planejamento estrutural do Compass. As decisões aqui registradas formam a base inegociável do sistema.

---

## DEC-001 — Atraso (Overdue) não é um estado persistido

### Contexto
O modelo precisava definir como tratar tarefas que ultrapassaram o seu prazo (`HardDeadline`). Criar um estado explícito como "Atrasado" causaria uma explosão da máquina de estados (ex: `InProgressOverdue`).
### Decisão
`Overdue` não é um estado persistido. É uma condição momentânea (derivada) calculada dinamicamente: a tarefa é considerada atrasada se o `deadline` está no passado e o `status` não é `Completed` ou `Cancelled`.
### Consequências
Mantém a máquina de estados simples (`Ready`, `InProgress`, etc.) e desloca a lógica de tempo e urgência puramente para as leituras do motor.

---

## DEC-002 — Tarefa atrasada não força execução irreal

### Contexto
O Decision Engine poderia tentar impor a execução imediata de uma tarefa atrasada, sugerindo ações inviáveis.
### Decisão
Uma tarefa atrasada (`Overdue`) permanece elegível, porém seu atraso apenas *aumenta a urgência* no ranqueamento. Ela ainda deve obrigatoriamente passar pelos filtros de restrição física (viabilidade de tempo e capacidade).
### Consequências
O sistema mantém a confiança do usuário, nunca sugerindo algo que não cabe na realidade momentânea.

---

## DEC-003 — Remoção da prioridade manual na Tarefa

### Contexto
Usuários tendem a burlar sistemas de "Prioridade" (Alta, Média, Baixa), marcando tudo como Alta para forçar a visibilidade, destruindo a confiabilidade do indicador.
### Decisão
Não existe campo `Priority` manual inserido pelo usuário na entidade `Task`.
### Consequências
A prioridade passa a ser calculada estritamente pelas variáveis matemáticas: prazo limite, tempo estimado e relevância hierárquica.

---

## DEC-004 — Prioridade operacional é propriedade derivada

### Contexto
Se não há prioridade manual, de onde vem a prioridade real da ação?
### Decisão
A prioridade operacional de uma tarefa é uma propriedade emergente, resultante do algoritmo do *Decision Engine*. 
### Consequências
A "prioridade" só existe no momento da avaliação e pode mudar a cada minuto conforme os prazos se esgotam.

---

## DEC-005 — O Score do Decision Engine é efêmero

### Contexto
Poderíamos salvar o *Score* calculado na tabela da Tarefa sempre que ele mudasse.
### Decisão
O *Score* não é persistido como uma verdade do domínio. Ele é recalculado em memória a partir do contexto e das regras atuais sempre que as recomendações são solicitadas.
### Consequências
Mantém o *Decision Engine* como uma função pura (stateless) e elimina atualizações massivas de banco de dados a cada ciclo.

---

## DEC-006 — Tempo Ocupado é união geométrica, não soma

### Contexto
Se o usuário tem dois compromissos sobrepostos (ex: 09h às 10h e 09h30 às 11h), somar suas durações resultaria num bloqueio irreal de 150 minutos.
### Decisão
O cálculo de `CommittedTime` representa a **união dos intervalos temporais** (geometria de sobreposição), resultando no bloqueio físico real (120 minutos no exemplo).
### Consequências
Garante precisão milimétrica na definição do tempo disponível, evitando falsos déficits de capacidade.

---

## DEC-007 — Importância Estratégica é transformacional

### Contexto
Projetos e Objetivos possuem pesos estratégicos. Como isso chega até a Tarefa?
### Decisão
A importância estratégica não é um valor copiado para a linha da Tarefa no banco. O *Decision Engine* calcula a contribuição final a partir da estrutura hierárquica lida no momento do cálculo.
### Consequências
Evita anomalias de atualização em cascata caso o peso de um projeto pai mude.

---

## DEC-008 — Separação entre Capacidade Restante e Déficit

### Contexto
Se o usuário trabalhar mais minutos do que a agenda previa, a capacidade iria para negativo, possivelmente quebrando o motor se não tratada.
### Decisão
O domínio adota dois conceitos separados: `CapacityRemaining` (que nunca é menor que 0) e `CapacityDeficit` (uma dívida de tempo acumulada). 
### Consequências
A matemática de alocação de tempo não quebra, e o sistema preserva historicamente o esforço extra do usuário sem corromper a semântica da disponibilidade.

---

## DEC-009 — Heurística de Chunking (Execução Parcial)

### Contexto
O que o motor faz se a tarefa mais importante leva 60 minutos, mas a janela de tempo atual só tem 15 minutos livres?
### Decisão
O motor sugere a tarefa com um `TargetDuration` ajustado para o tempo disponível (15 minutos) na recomendação final, sem alterar a duração total persistida na entidade original.
### Consequências
Evita telas vazias, fomenta o progresso incremental e protege a integridade do planejamento.

---

## DEC-010 — Avaliação contra Janelas de Execução

### Contexto
Ter 1 hora livre espalhada em intervalos de 5 minutos é inútil para trabalhos de alto foco.
### Decisão
A viabilidade não é avaliada contra o total geral de horas do dia, mas sim contra a geometria contínua da `Execution Window` vigente no exato momento da solicitação.
### Consequências
Sufoca sugestões absurdas de alocação onde a tarefa fisicamente não cabe de forma contínua.

---

## DEC-011 — Buffers de Transição de Compromisso

### Contexto
Humanos precisam se deslocar e fazer pausas entre eventos fixos.
### Decisão
O Compass injetará matematicamente uma constante temporal (*Transition Buffer*) ao redor dos compromissos rígidos do calendário, expandindo suas áreas de bloqueio físico.
### Consequências
As janelas de execução geradas serão mais realistas e humanas.

---

## DEC-012 — Calendário Externo é Read-Only (One-Way Sync)

### Contexto
Integração com Google Calendar pode ser fonte constante de conflitos e quebra de estado.
### Decisão
O Compass apenas consome calendários externos como leitura, importando eventos como restrições temporais (`Commitments`). O Compass nunca escreve ou empurra Tarefas de volta para o calendário externo.
### Consequências
Blinda o Monólito contra os complexos tratamentos de conflito bidirecional.

---

## DEC-013 — Horário Operacional Configurável por Perfil

### Contexto
Dias "comerciais" fixos (08h às 18h) quebram rotinas reais e variadas.
### Decisão
A disponibilidade é configurável através de um `ScheduleProfile` (por dia, com regras customizadas). O `DailyCycle` orquestra as regras daquele dia específico.
### Consequências
O sistema permite pausas intencionais, turnos da noite ou dias atípicos.

---

## DEC-014 — Monólito Modular: Fronteiras de Comunicação

### Contexto
Como impedir que o backend vire código espaguete sem usar microsserviços?
### Decisão
Comunicação restrita: acesso direto a tabelas de outros domínios é **proibido**. Módulos só conversam de forma síncrona através de Contratos (Interfaces públicas internas) e de forma assíncrona via *Domain Events* na memória (ex: MediatR).
### Consequências
Total isolamento lógico sem latência de rede.

---

## DEC-015 — Desacoplamento da Entidade Task no Motor

### Contexto
O Módulo Execution precisa processar Tasks. Usar a própria entidade quebraria o isolamento.
### Decisão
O Execution opera sobre uma visualização de leitura (`TaskReadModel`), exposta pela camada de contratos do Planning.
### Consequências
Reforça o princípio *Domain Model != API Contract != Frontend Model*.

---

## DEC-016 — A Posse da Transição de Estado

### Contexto
Quem é o responsável por concluir uma tarefa: quem executa ou quem planejou?
### Decisão
O Execution aponta que um trabalho ocorreu registrando o tempo (emite *WorkLoggedIntegrationEvent*). O Planning consome o evento e é o único que decide alterar o status da Task para `Completed`.
### Consequências
Módulos continuam como donos estritos do ciclo de vida de suas entidades.

---

## DEC-017 — Soft-Links no Banco de Dados

### Contexto
O PostgreSQL suporta chaves estrangeiras, mas referências inter-schema em um monólito modular impedem a extração limpa de módulos.
### Decisão
A conexão entre, por exemplo, o Log de Execução e a Tarefa ocorrerá por UUID (Soft-Link), sem constraint física de Foreign Key. A integridade será defendida por arquitetura de software (*Soft Delete*).
### Consequências
Permite evolução ou migração física independente para cada banco de módulo, se necessário no futuro.

---

## DEC-018 — Concorrência Otimista Implícita (PostgreSQL xmin)
*Status: Superseded*
*Superseded by: DEC-020*
**(Motivo: Forte acoplamento com a engine específica do PG).**

---

## DEC-019 — JSONB para Perfil de Calendário

### Contexto
Criar tabelas relacionais para os horários operacionais do usuário seria exagerado e ineficiente para as consultas (reads).
### Decisão
O `ScheduleProfile` será persistido como um documento integral em coluna `JSONB`.
### Consequências
Leitura e persistência ocorrem em um único passe na tabela principal.

---

## DEC-020 — Concorrência Otimista Explícita

### Contexto
Garantir que a mesma transição crítica de tarefa não aconteça simultaneamente via web e mobile.
### Decisão
Substituir o `xmin` por uma coluna explícita `row_version` (Concurrency Token mapeado no EF Core) nas entidades críticas.
### Consequências
Torna o conflito de transações explícito no código e desacoplado de truques de SGBD.

---

## DEC-021 — Identidade do Daily Cycle

### Contexto
Como garantir o ciclo no futuro se múltiplos usuários usarem o software?
### Decisão
A chave primária (PK) do `DailyCycle` será composta por `UserId + Date_Id`.
### Consequências
Evita colisões num futuro cenário multi-tenant.

---

## DEC-022 — Separação de Server-State e Local-State no Frontend

### Contexto
Armazenar e atualizar manualmente tabelas grandes de projetos e tarefas na `store` global é propício a erros crônicos.
### Decisão
O Vue Query gerenciará 100% do *Server-State* (cache, fetching e mutations). O Pinia será estritamente utilizado para estado local de UI e sessão.
### Consequências
O código da UI foca em exibir os dados cacheados e despachar intenções, e não em atualizar reducers gigantes.

---

## DEC-023 — Pureza da Apresentação (FSD)

### Contexto
Onde os componentes do Frontend acessam o banco e disparam métodos HTTP?
### Decisão
Componentes na camada `entities/` não realizam chamadas Axios nem possuem estado global. A comunicação ocorre em camadas superiores (`features/` ou `widgets/`), que apenas passam os dados via props.
### Consequências
Componentes altamente testáveis, reaproveitáveis e blindados de regras de API.

---

## DEC-024 — Modo Execução como Interface Restrita

### Contexto
O usuário no meio da execução sente o impulso de organizar tarefas em outras pastas, dispersando seu foco.
### Decisão
O "Modo Execução" na interface visual é estritamente read-only em questões estruturais. O usuário pode apenas atuar na execução (logar tempo/concluir), e não reorganizar o planejamento.
### Consequências
Barreira cognitiva explícita entre a ação ("fazer") e o planejamento ("pensar").

---

## DEC-025 — Ausência de Motor Decision Engine Offline

### Contexto
Como calcular o score de ranqueamento complexo quando o celular perde conexão?
### Decisão
O *Decision Engine* só roda no servidor. O frontend continuará exibindo e operando sobre o Cache das Recomendações em modo Offline, mas não calculará *novas* recomendações de tarefas sem conexão.
### Consequências
Garante *Single Source of Truth* das regras matemáticas sem ter que escrever e testar todo o código duas vezes (no C# e no TypeScript).

---

## DEC-026 — LWW vs. Concorrência Crítica para Conflitos

### Contexto
Quando a fila offline restabelecer conexão, o que acontece com dois comandos conflitantes?
### Decisão
Mutações descritivas puras (ex: trocar título) usam Last-Write-Wins (quem chegar por último sobrescreve). Mutações de estado críticas (logar trabalho, finalizar tarefa) enfrentam checagem rígida no `row_version`, podendo sofrer rollback.
### Consequências
Equilibra simplicidade com proteção matemática onde importa.

---

## DEC-027 — Resiliência via Refetch passivo (Sem WebSockets)

### Contexto
Sincronizar a aba do PC em tempo real se a tarefa foi feita pelo celular.
### Decisão
Em vez de implementar conexões WebSockets persistentes (SignalR), o Vue Query cuidará disso invalidando agressivamente o cache com `refetchOnWindowFocus`.
### Consequências
Complexidade de infraestrutura reduzida sem quebrar a coerência percebida na UX.

---

## DEC-028 — Identificadores de Fuso Horário

### Contexto
Armazenar offsets (`-03:00`) quebra com horários de verão e mudanças políticas de governos locais.
### Decisão
Transições HTTP e persitências utilizam identificadores estritos **IANA** (ex: `America/Sao_Paulo`).
### Consequências
A biblioteca de tempo manipula saltos de horário corretamente.

---

## DEC-029 — Enums como Strings nos Contratos

### Contexto
Integrar APIs usando inteiros (0 = Activo, 1 = Pausado) esconde bugs graves quando os Enums mudam no backend e o frontend permanece desatualizado.
### Decisão
Todo tráfego de Enum entre Back e Front acontecerá em formato String Literal.
### Consequências
Contratos se tornam resilientes à reorganização interna de enumerações no código C#.

---
*Para ver as regras arquiteturais aplicadas ao desenvolvimento decorrentes dessas decisões, acesse [DEVELOPMENT_RULES.md](./DEVELOPMENT_RULES.md).*
*Consulte o registro cronológico de reflexões em [DEVLOG.md](./DEVLOG.md).*