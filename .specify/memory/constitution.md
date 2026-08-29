<!--
Sync Impact Report:
- Version change: none -> 1.0.0 (Initial Ratification)
- Added principles: 25 mandatory principles established covering Modular Monolith architecture, Host boundaries, Cross-module contracts, CQRS Application patterns, Calendar/Temporal authority, Domain semantics, Backend truth, Vue Query & Vue 3 FSD frontend, Database migrations, Data integrity, Vertical features, and Strict Quality/Acceptance gates.
- Added sections: Core Principles (1-25), Architectural & Technical Constraints, Quality Gates & Delivery Lifecycle, Governance.
- Removed sections: Placeholder template slots.
- Follow-up TODOs: None.
-->

# Compass V2 Constitution

## Core Principles

### 1. Monólito Modular (Planning, Calendar, Execution)
O Compass MUST ser arquitetado e mantido como um Monólito Modular composto exatamente por três módulos de negócio principais: `Planning`, `Calendar` e `Execution`. Nenhum outro módulo de negócio ou bounded context pode ser introduzido sem emenda constitucional prévia. Cada módulo possui autonomia sobre sua lógica e armazenamento.
- **Rationale**: Garante coesão e separação clara de responsabilidades funcionais, mantendo a simplicidade de implantação, depuração e operação em um único processo.

### 2. Host como Composition Root Estrito
O projeto `Host` MUST atuar exclusivamente como composition root, ponto de entrada HTTP, orquestrador de middlewares, configuração de injeção de dependências (DI) e pipeline de inicialização da aplicação. O `Host` MUST NOT conter regras de negócio, lógica de domínio, acesso direto a bancos de dados ou orquestração de casos de uso.
- **Rationale**: Impede o acoplamento indevido no ponto de entrada e preserva a autonomia, modularidade e testabilidade de cada módulo.

### 3. Isolamento Estrito de Módulos e Dados
Um módulo MUST NOT acessar diretamente o `DbContext`, tabelas de banco de dados, entidades de `Domain` ou classes de `Infrastructure` de outro módulo. O encapsulamento de dados, esquemas e persistência é absoluto e inviolável para cada módulo.
- **Rationale**: Evita acoplamento acidental em nível de dados e classes internas, garantindo que alterações estruturais em um módulo não provoquem regressões em outros.

### 4. Comunicação Cross-Module via Contracts e DI
Qualquer integração ou comunicação síncrona entre módulos MUST ocorrer unicamente por meio de interfaces e DTOs definidos nos projetos `Contracts` do módulo chamado, resolvidos em tempo de execução via injeção de dependências (DI).
- **Rationale**: Mantém contratos explícitos, fortemente tipados e versionáveis, permitindo substituição simples e execução de testes isolados através de stubs ou mocks.

### 5. Proibição de HTTP Interno entre Módulos
A comunicação interna entre módulos dentro do monólito MUST NOT utilizar chamadas de rede ou clientes HTTP internos. Toda invocação cross-module no mesmo processo deve ser realizada in-memory através das abstrações de `Contracts`.
- **Rationale**: Elimina sobrecarga desnecessária de serialização, latência de rede, falhas parciais de transporte e complexidade de resiliência desnecessária dentro do mesmo processo.

### 6. Padrão CQRS na Camada Application (Command/Query + Handler)
A camada `Application` de cada módulo MUST estruturar suas operações estritamente no padrão Command/Query + Handler (ex.: `CreateDailyPlanCommand` e `CreateDailyPlanCommandHandler`, `GetScheduleQuery` e `GetScheduleQueryHandler`). Os sufixos `UseCase`, `CommandService`, `AppService` ou equivalentes MUST NOT ser utilizados.
- **Rationale**: Padroniza o fluxo de orquestração de aplicação, respeita a separação explícita de intenções (leitura vs. mutação) e garante previsibilidade em toda a base de código.

### 7. Autoridade Exclusiva do Módulo Calendar sobre Tempo Civil e Fusos
O módulo `Calendar` MUST ser o único e exclusivo proprietário de toda a lógica relacionada a timezones, horário de verão (DST), cálculo e interpretação de data civil, disponibilidade/janelas de tempo e conversões bidirecionais de/para UTC. Nenhum outro módulo pode implementar ou duplicar regras de fuso horário ou janelas de disponibilidade.
- **Rationale**: Centraliza a complexidade de regras temporais em um único ponto confiável, eliminando discrepâncias sutis e bugs de fuso horário distribuídos.

### 8. Normalização Temporal em UTC (DateTimeOffset)
Todos os DTOs, interfaces e contratos expostos em projetos `Contracts` que transportem instantes ou pontos no tempo MUST utilizar `DateTimeOffset` normalizado em UTC (`Offset == TimeSpan.Zero`).
- **Rationale**: Elimina ambiguidades temporais e inconsistências de conversão em fronteiras de módulos e integrações de sistema.

### 9. Representação de Dia Civil com DateOnly
O tipo `DateOnly` MUST ser utilizado exclusivamente para representar o dia civil de calendário no contexto do perfil/timezone do usuário, sem qualquer componente ou distorção de horário.
- **Rationale**: Previne bugs clássicos de troca de data causados por offsets de timezone quando o conceito de negócio é estritamente um dia de calendário.

### 10. Distinção Semântica entre Sugestão, DailyPlan Aceito e ExecutionLog
O sistema MUST modelar `Sugestão` (proposta de planejamento calculada), `DailyPlan aceito` (plano de trabalho formalmente aprovado) e `ExecutionLog` (registro real e imutável de execução/rastreamento) como entidades de domínio rigorosamente distintas em tipos, ciclos de vida, regras de transição e invariantes. Essas entidades MUST NOT ser fundidas ou representadas por uma única tabela ou modelo genérico.
- **Rationale**: Cada conceito possui garantias, mutabilidade, requisitos de auditoria e significado operacional radicalmente diferentes no ciclo de planejamento e execução.

### 11. Isolamento de Intervalos (Break)
Entidades e blocos de intervalo (`Break`) gerenciados no módulo `Execution` MUST NOT possuir `ReferenceId` associado a itens de planejamento e MUST NOT publicar progresso ou eventos de avanço de tarefas para o módulo `Planning`.
- **Rationale**: Intervalos constituem pausas operacionais/fisiológicas no fluxo contínuo de tempo e não representam progresso ou execução de tarefas planejadas.

### 12. Backend como Única Fonte da Verdade
O backend .NET MUST ser a fonte exclusiva e definitiva da verdade para todas as validações, regras de negócio, cálculos de domínio e transições de estado. O frontend MUST NOT duplicar ou tentar emular regras de negócio, restringindo-se a validações visuais básicas de formulário.
- **Rationale**: Evita divergência de regras entre cliente e servidor, brechas de integridade e custos duplicados de manutenção.

### 13. Gerenciamento de Estado no Frontend (Vue Query vs. Estado Local)
Todo o estado de dados remotos no frontend MUST ser gerenciado através do Vue Query (TanStack Query), incluindo caching, refetching e invalidação automática. O uso de estado reativo local (`ref`, `reactive`, Pinia) MUST ser restrito exclusivamente a comportamentos visuais e efêmeros da interface (ex.: visibilidade de modais, aba selecionada, rascunho de input).
- **Rationale**: Previne problemas de dessincronização de cache, dados obsoletos na UI e duplicação descontrolada do estado do servidor na memória do cliente.

### 14. Geração de IDs de Agregados pelo Backend
Identificadores de agregados e entidades MUST ser gerados e atribuídos pelo backend (ex.: GUID v7 ou estratégia sequencial definida), salvo quando uma especificação técnica aprovada determinar explicitamente o contrário para atender a um cenário de negócio justificado.
- **Rationale**: Assegura integridade referencial, controle transacional e proteção contra colisões de identificadores gerados no cliente.

### 15. Migrations de Banco de Dados Aditivas e Validadas
Todas as migrações de banco de dados MUST ser aditivas (evitando remoções ou modificações destrutivas imediatas de colunas/tabelas) e MUST ser testadas e comprovadas tanto na inicialização a partir de banco vazio (fresh install) quanto na aplicação sequencial de upgrade sobre bases pré-existentes.
- **Rationale**: Garante compatibilidade contínua, capacidade de rollback seguro e previsibilidade de implantação em todos os ambientes.

### 16. Proibição de Dados Fictícios em Fallbacks de Produção
Mecanismos de fallback, tratamento de exceções ou componentes de UI/Backend MUST NOT criar, embutir ou exibir dados fictícios/sintéticos quando operando em cenários de produção. Na ocorrência de falha ou ausência de dados, o sistema MUST apresentar estados explícitos de erro, vazio ou indisponibilidade.
- **Rationale**: Impede que decisões de negócio ou usuários sejam induzidos ao erro por dados inventados e assegura que falhas de infraestrutura sejam visíveis e corrigidas.

### 17. Cenários de Aceitação Verificáveis Pré-Implementação
Cada funcionalidade (feature) MUST conter cenários de aceitação explícitos, objetivos e verificáveis (formato Given/When/Then ou critérios inequívocos de sucesso) aprovados no artefato de especificação (`spec.md`) antes de qualquer esforço de implementação.
- **Rationale**: Estabelece critérios claros e consensuais de "sucesso", alinhando o comportamento esperado entre especificação, desenvolvimento e testes.

### 18. Fatiamento Vertical de Features (Full-Slice)
A entrega de uma funcionalidade MUST abranger verticalmente todas as camadas necessárias para sua operação completa: `Domain`, `Application`, `Infrastructure`, endpoints `HTTP`, componentes de `Frontend` e seus respectivos testes automatizados, quando aplicável.
- **Rationale**: Evita a criação de camadas órfãs ou código parcial não integrado, garantindo entrega de valor ponta a ponta a cada incremento.

### 19. Gate de Planejamento Obrigatório (Spec -> Plan -> Tasks -> Approval)
Nenhuma implementação de código de produção pode ser iniciada antes que a sequência de artefatos `spec.md`, `plan.md` e `tasks.md` tenha sido gerada, revisada e formalmente aprovada.
- **Rationale**: Garante maturidade no design técnico, evita retrabalho por suposições incorretas e dá visibilidade clara sobre o plano de trabalho.

### 20. Análise de Consistência Pré-Execução e Convergência Pós-Execução
Antes de iniciar a implementação das tarefas, MUST ser executada a análise de consistência cross-artifact (`speckit-analyze`). Após a conclusão das tarefas, MUST ser executada a convergência (`speckit-converge`) para comprovar que todo o escopo especificado foi plenamente construído e que não restam pendências.
- **Rationale**: Garante rastreabilidade estrita entre o que foi planejado e o que foi implementado, prevenindo dívidas técnicas ocultas.

### 21. Foco Estrito no Escopo da Feature (Sem Refatorações Oportunistas)
O trabalho de implementação de uma feature ativa MUST concentrar-se estritamente nas tarefas aprovadas no seu plano. Refatorações, limpezas ou alterações em módulos e arquivos não relacionados ao escopo da feature ativa são terminantemente proibidas.
- **Rationale**: Mantém os diffs enxutos e auditáveis, simplifica revisões de código e minimiza o risco de regressões colaterais.

### 22. Compilação Não É Critério de Conclusão
O fato de o código compilar ou o build passar com sucesso MUST NOT ser interpretado como evidência suficiente de que uma feature ou tarefa está concluída.
- **Rationale**: A compilação atesta apenas conformidade sintática e estática de tipos, não validando a correção das regras de negócio ou o comportamento em tempo de execução.

### 23. Critérios Obrigatórios de Conclusão (Definition of Done)
A conclusão de qualquer feature exige obrigatoriamente: (a) testes unitários e de integração relevantes passando; (b) build limpo e sem warnings impeditivos; (c) diff de código limpo e estritamente aderente ao plano; e (d) evidência verificável de cumprimento dos cenários de aceitação.
- **Rationale**: Estabelece uma barra de qualidade intransigente para a entrega contínua de software confiável.

### 24. Arquitetura de Frontend (Vue 3, TypeScript Estrito, FSD Pragmático)
O frontend MUST utilizar Vue 3 com Composition API (`<script setup>`), TypeScript em modo estrito e a metodologia Feature-Sliced Design (FSD) aplicada de forma pragmática, criando exclusivamente as camadas e fatias necessárias para as necessidades reais, sem a criação de pastas vazias ou estruturas puramente organizacionais.
- **Rationale**: Promove escalabilidade, modularidade e forte segurança de tipos no cliente, evitando complexidade arquitetural desnecessária ou diretórios ociosos.

### 25. Acessibilidade, Semântica e Design System Neutro
A interface do usuário MUST ser construída com HTML semântico, suporte completo a navegação e atalhos por teclado (a11y), atributos ARIA adequados e estilização baseada em tokens CSS neutros e consistentes.
- **Rationale**: Assegura padrões de acessibilidade universal, usabilidade intuitiva e um design system limpo e de fácil manutenção.

## Architectural & Technical Constraints

### Limites Modulares e Injeção de Dependência
- Módulos (`Planning`, `Calendar`, `Execution`) são pastas ou projetos isolados dentro da mesma solution .NET.
- Cada módulo expõe um assembly `Module.Contracts` contendo exclusivamente DTOs, interfaces de serviços públicos e tipos de eventos.
- Implementações de serviços de contratos residem em `Module.Application` ou `Module.Infrastructure` e são registradas no contêiner de DI através de métodos de extensão (ex.: `AddPlanningModule(...)`).
- Acesso a dados interno é restrito ao `DbContext` próprio do módulo, mapeado para schemas ou tabelas privativas.

### Padrões de Tempo e Fuso Horário
- Todo cálculo que envolva horário de trabalho, compromissos ou janelas de disponibilidade deve delegar a responsabilidade para o módulo `Calendar`.
- O cliente/frontend sempre envia e recebe horários formatados com base nos contratos UTC (`DateTimeOffset`) e datas civis (`DateOnly`).

### Frontend e Estado Remoto
- O frontend é organizado sob camadas FSD (`app`, `pages`, `widgets`, `features`, `entities`, `shared`). Camadas ou slices só são criadas quando contêm arquivos efetivos.
- Queries e Mutations remotas devem ser encapsuladas em composables usando Vue Query.

## Quality Gates & Delivery Lifecycle

### Fluxo de Especificação e Execução (Spec-Driven Development)
1. **Especificação (`spec.md`)**: Define requisitos do usuário, regras de negócio e cenários de aceitação verificáveis.
2. **Planejamento (`plan.md`)**: Desenha a arquitetura técnica, modelo de dados, contratos de API e estratégia de testes.
3. **Tarefas (`tasks.md`)**: Lista de tarefas granulares, ordenadas por dependência, cobrindo o slice vertical completo.
4. **Análise de Consistência (`speckit-analyze`)**: Validação cruzada sem modificações entre spec, plan e tasks antes da execução.
5. **Implementação (`speckit-implement`)**: Execução estrita das tarefas planejadas.
6. **Convergência (`speckit-converge`)**: Auditoria final de cobertura para certificar que nenhum requisito da spec foi omitido.

## Governance

### Processo de Emenda
- A presente Constituição é a autoridade máxima sobre a arquitetura e governança do projeto Compass V2.
- Qualquer alteração nos 25 Princípios Fundamentais ou nas seções de restrições exige proposta formal de emenda, justificativa técnica documentada e aprovação explícita.

### Política de Versionamento
- **MAJOR**: Remoção, inversão ou redefinição incompatível de princípios constitucionais ou quebra de fronteiras arquiteturais.
- **MINOR**: Adição de novos princípios, seções de diretrizes ou expansão material de regras existentes.
- **PATCH**: Correções tipográficas, ajustes redacionais e refinamentos não semânticos.

### Revisão de Conformidade
- Todos os planos de implementação, PRs e revisões de código MUST verificar e certificar a conformidade com as regras desta Constituição.
- Qualquer complexidade adicional introduzida no sistema MUST ser justificada tecnicamente contra os princípios de simplicidade e modularidade aqui ratificados.

**Version**: 1.0.0 | **Ratified**: 2026-08-28 | **Last Amended**: 2026-08-28
