# 03. Use-Cases
**Objetivo:** Garantia funcional do sistema. O MVP será considerado completo quando os 50 cenários passarem nos testes automatizados.

## 1. Captura e Criação Rápida (Core - Commitments)
* **UC-01 | Tarefa Simples de Foco Rápido:** Título + Tempo (30m) + Energia (Média) -> Cria `Task` pendente.
* **UC-02 | Criação de Projeto:** Título + Prazo -> Cria `Project` pendente (tempo estimado = 0 até receber tarefas).
* **UC-03 | Tarefa Vinculada a Projeto:** Ao criar tarefa com `ProjectId`, soma tempo estimado ao projeto pai.
* **UC-04 | Tarefa com Dependência:** Cria tarefa com `ParentTaskId`. Nasce com status bloqueado ($S = 0$).
* **UC-05 | Bloco Rígido (Evento):** Título + Data/Hora -> Cria `Event`, registrando um *Hard Blocker* na agenda.
* **UC-06 | Evento com Horário Inválido:** Início maior que Fim -> Backend rejeita (`InvalidTimeRangeException`).
* **UC-07 | Objetivo de Longo Prazo:** Cria `Goal`. Fica invisível no "Agora", agindo como multiplicador.
* **UC-08 | Hábito Recorrente Diário:** Cria `Habit` com CRON diário e streak zero.
* **UC-09 | Hábito em Dias Alternados:** Cria `Habit` (Seg/Qua/Sex). Gerador só cria instâncias nesses dias.
* **UC-10 | Título Curto (< 3 caracteres):** Rejeição instantânea por invariante de validação.

## 2. O Motor de Decisão ("O que fazer agora?")
* **UC-11 | Consulta com Tempo Amplo/Energia Alta:** Retorna tarefas complexas e longas no Top 1.
* **UC-12 | Consulta Antes de Compromisso Fixo:** Se há evento em 20m, elimina tarefas longas ($S = 0$) e sugere vitórias rápidas.
* **UC-13 | Consulta com Energia Baixa:** Rebaixa tarefas de energia alta e eleva tarefas organizacionais leves.
* **UC-14 | Ocultação por Dependência:** Tarefas com dependências pendentes nunca aparecem nas sugestões.
* **UC-15 | Desbloqueio Instantâneo:** Ao concluir a dependência, a tarefa filha é desbloqueada no mesmo milissegundo.
* **UC-16 | Penalidade de Atrito:** Tarefas adiadas repetidamente (`PostponedCount > 0`) perdem pontuação para não travarem o topo.
* **UC-17 | Bônus de Alinhamento a Metas:** Tarefas vinculadas a um `Goal` ganham bônus contra tarefas avulsas.
* **UC-18 | Filtragem Rígida por Tag:** Filtro `#celular` ativo zera a nota de qualquer tarefa que exija `#computador`.
* **UC-19 | Decisão Ignorada:** Fechar o app sem interagir gera um `DecisionSnapshot` com flag `WasIgnored = true`.
* **UC-20 | Decisão Escolhida:** Clicar na opção Top 2 grava telemetria de sucesso para calibração.

## 3. Conclusões e Efeitos em Cascata
* **UC-21 | Conclusão de Tarefa Simples:** Status `Completed`, grava histórico e atualiza a tela do "Agora".
* **UC-22 | Conclusão em Cascata (Projeto):** Concluir a última tarefa pendente encerra o projeto automaticamente.
* **UC-23 | Recálculo de Progresso (Goal):** O encerramento do projeto recalcula a porcentagem da meta pai.
* **UC-24 | Conclusão Manual Proibida:** Tentar concluir projeto com tarefas pendentes retorna erro HTTP 422.
* **UC-25 | Reabertura de Tarefa:** Reabrir tarefa reverte o status do projeto concluído para `Em Andamento`.
* **UC-26 | Adição a Projeto Concluído:** Tentar adicionar tarefa a projeto concluído é bloqueado pela invariante.
* **UC-27 | Arquivamento Automático:** Tarefas concluídas há > 7 dias vão para `Archived` (limpeza de índices SQL).
* **UC-28 | Alteração em Item Arquivado:** Proibido alterar título/energia de item arquivado sem desarquivar antes.
* **UC-29 | Exclusão de Tarefa com Filhas:** Excluir tarefa pai aciona exclusão/arquivamento em cascata nas sub-tarefas.
* **UC-30 | Exclusão de Tarefa com Lembrete:** Remove automaticamente o lembrete pendente na tabela `reminders`.

## 4. Agenda, Conflitos e Bloqueios (Scheduling)
* **UC-31 | Subtração de Janela por Bloco Rígido:** Evento às 11h faz o app às 10h calcular tempo livre de 60m (não o dia todo).
* **UC-32 | Conflito de Agenda:** Agendar evento em horário já ocupado retorna HTTP 409 Conflict.
* **UC-33 | Eventos Adjacentes:** Permitido evento das 14h-16h e outro das 16h-17h (sem colisão).
* **UC-34 | Fim do Turno Útil:** Se o turno acaba às 18h e são 17h40, o app só sugere tarefas de até 20m.
* **UC-35 | Disparo de Lembrete:** Job em background notifica no horário exato e marca `IsSent = true`.
* **UC-36 | Cancelamento de Lembrete:** Concluir tarefa cancela seu lembrete pendente automaticamente.
* **UC-37 | Incremento Noturno de Adiantamento:** Job da meia-noite soma `+1` ao `PostponedCount` de tarefas pendentes de ontem.
* **UC-38 | Evento Passado:** Eventos encerrados deixam de bloquear o "Agora", permanecendo fixos no histórico.

## 5. Hábitos e Recorrência Determinística
* **UC-39 | Geração Noturna de Hábito:** Meia-noite, gera nova ocorrência pendente para hábitos diários ativos.
* **UC-40 | Conclusão de Hábito (Streak +1):** Marca ocorrência concluída, soma `+1` ao `CurrentStreak` e atualiza `BestStreak`.
* **UC-41 | Quebra de Sequência:** Virada de dia sem conclusão zera o `CurrentStreak` no banco.
* **UC-42 | Hábito Fora do Dia:** Hábitos de Seg/Qua/Sex não geram ocorrências nem cobranças às Terças.
* **UC-43 | Pausa de Hábito:** Status `Pausado` congela o streak e impede geração noturna.
* **UC-44 | Reativação de Hábito:** Voltar para `Ativo` recomeça o ciclo sem zerar o histórico anterior.

## 6. Conhecimento, Telemetria e Edge Cases
* **UC-45 | Captura de Nota (`Note`):** Salva texto sem prazo ou energia; não concorre no "Agora".
* **UC-46 | Conversão de Nota em Projeto:** 1 clique converte nota em projeto e marca nota como convertida.
* **UC-47 | Registro de Sessão de Foco:** Cronômetro real salva `FocusSession` com tempo gasto exato.
* **UC-48 | Alerta de Desvio:** Se tarefa de 20m levou 60m, o módulo de Analytics registra desvio (+200%).
* **UC-49 | Sincronização Local-First:** Offline no Vue (Dexie.js), UI responde em 0ms. Ao reconectar, sincroniza lote via API.
* **UC-50 | Virada de Dia com App Aberto:** 00h00, o Vue.js consulta o motor silenciosamente e atualiza a tela sem refresh.