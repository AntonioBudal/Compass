# Estratégia de Testes (Compass)

A estratégia de testes do Compass é pautada em um princípio simples: **garantir que o sistema nunca minta para o usuário**. 

A métrica de "100% de cobertura de código" baseada em testes fúteis de *getters e setters* é explicitamente rejeitada. A energia de testes é direcionada exclusivamente para as áreas onde uma falha sistêmica seria catastrófica para a confiabilidade do motor de execução: as regras de negócio puras (determinismo) e a persistência de estado (concorrência e histórico).

A pirâmide de testes é dividida nas seguintes frentes:

---

## 1. Testes de Unidade (Lógica Pura e Invariantes)

Focados nos Domínios e Serviços de Domínio. Estes testes rodam instantaneamente, em memória, sem acesso a banco de dados ou rede.
*   **Ferramenta Base:** `xUnit`.
*   **Gestão de Tempo:** O tempo é controlado utilizando instâncias *falsas/fixas* nativas (ex: `Microsoft.Extensions.Time.Testing.FakeTimeProvider`), ou os instantes de tempo são passados diretamente como parâmetros para funções puras.

### O que deve ser testado:

**A. Decision Engine (Módulo: Execution)**
O serviço de domínio matemático deve ser testado através de *Data-Driven Tests* (`[Theory]`).
*   **Determinismo e Elegibilidade:** Injetar tarefas em estado `Draft` ou pertencentes a projetos `Paused` e assegurar que são ignoradas no ranqueamento.
*   **Scoring:** Fixar o relógio, injetar duas tarefas com prazos diferentes e afirmar que a matemática exponencial de urgência funciona corretamente, priorizando a mais próxima do deadline.
*   **Chunking (Execução Parcial):** Passar uma Tarefa de 60 minutos em uma janela `Execution Window` de 20 minutos livres. Assegurar que a `Recommendation` gerada limite o `TargetDuration` em 20 minutos.

**B. Geometria Temporal (Módulo: Calendar)**
*   **Sobreposição Reversa:** Dois `Commitments` conflitantes inseridos no mesmo horário. Garantir que a união funde os intervalos, não subtraindo o tempo em duplicidade.
*   **Buffers de Transição:** Injetar o *Transition Buffer* configurado (ex: 15 min) antes e depois de um evento rígido e afirmar que as janelas livres são devidamente espremidas/fragmentadas.

**C. Invariantes de Estado (Módulo: Planning)**
*   **Transições:** Forçar a promoção de uma `Task` para `Ready` sem preencher a `EstimatedDuration`, assegurando o retorno de um Erro de Domínio / Padrão Result de falha previsível.

---

## 2. Testes de Integração (Estado Real e Banco)

Testam a infraestrutura e a transação. O *Entity Framework In-Memory* é estritamente proibido, pois não valida Constraints reais do banco.
*   **Ferramenta Base:** `xUnit` + `Testcontainers` (sobem uma instância limpa de PostgreSQL em um contêiner Docker para cada suíte, garantindo isolamento total).

### O que deve ser testado:

*   **Concorrência Otimista (`row_version`):**
    *   *Caso:* Duas instâncias de Use Case simultâneas tentam dar "Log Work" e concluir a mesma tarefa.
    *   *Assert:* A primeira passa. A segunda deve lançar exceção de concorrência do EF Core, e a API deve traduzir para HTTP `409 Conflict`.
*   **Capacidade Negativa (Déficit):**
    *   *Caso:* Trabalhar 45 minutos em um `DailyCycle` que só possuía 30 minutos livres restantes.
    *   *Assert:* O `capacity_deficit` da tabela deve ser incrementado para 15 minutos, garantindo que o tempo real nunca seja descartado.
*   **Snapshot Histórico (Execution Log):**
    *   *Caso:* Uma tarefa é completada, gravando seu título no Log. Posteriormente, o título da entidade original na tabela `tasks` é alterado.
    *   *Assert:* O `task_snapshot_title` inserido no `execution_logs` deve permanecer intacto, validando o Journal *append-only*.
*   **Resiliência de Soft-Links:**
    *   *Caso:* Arquivar uma tarefa (ocultá-la via *Soft Delete* no módulo `Planning`).
    *   *Assert:* O `execution_logs` (que aponta para ela via UUID sem Foreign Key física) deve continuar podendo ser consultado sem gerar `NullReferenceException` nas queries ou quebrar os relatórios.

---

## 3. Testes de Frontend (Vue 3 + Vue Query)

Testam os contratos e o comportamento reativo local da UI, sem depender do backend rodando simultaneamente.
*   **Ferramenta Base:** `Vitest` + `Vue Test Utils`.

### O que deve ser testado:

*   **Mutações e Optimistic Updates:**
    *   *Caso (Caminho Feliz/Offline):* Mockar a camada de rede (interceptando chamadas) e simular que a rede caiu. Disparar a ação de concluir tarefa.
    *   *Assert:* A interface deve assumir o sucesso da mutação (retirar a tarefa da tela instantaneamente) e enfileirar a ação na *Offline Queue*, **sem fazer rollback**.
    *   *Caso (Caminho de Falha Lógica):* Mockar a camada de rede retornando `409 Conflict`.
    *   *Assert:* O *Optimistic Update* deve sofrer **rollback**, a tarefa deve voltar à tela, o cache ser invalidado e a notificação de erro deve ser emitida.
*   **Apresentação e UX:** Garantir que o componente `RecommendationList.vue` formate e exiba corretamente o *Reasoning* em tela, e reaja aos indicadores de *Overdue* alterando as variáveis de cor para `--color-danger`.

---

## 4. Testes Ponta-a-Ponta (E2E)

Validam os fluxos críticos integrando Frontend e Backend através do navegador. Utilizados de forma comedida para evitar extrema lentidão no pipeline de CI.
*   **Ferramenta Base:** `Playwright`.

### Fluxo Crítico Mínimo:
O caminho feliz inegociável que deve estar 100% testado em E2E:
1.  *Planning:* O usuário cria uma Tarefa estimando o tempo.
2.  *Daily Cycle:* O usuário clica em "Iniciar Dia".
3.  *Decision Engine:* A UI altera para o Modo Execução e o sistema apresenta a Tarefa recém-criada no topo (visto que o dia está vazio).
4.  *Log Work:* O usuário clica em "Concluir", a latência reage, a tarefa some, e o teste verifica se o backend registrou a mutação do tempo e a finalização com sucesso.

---

*Para a definição exata das regras inegociáveis arquiteturais referenciadas nestes testes, consulte [DEVELOPMENT_RULES.md](./DEVELOPMENT_RULES.md).*
*Para detalhes estruturais do backend testado, veja [DOMAIN.md](./DOMAIN.md) e [DATABASE.md](./DATABASE.md).*