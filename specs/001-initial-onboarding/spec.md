# Feature Specification: Onboarding Inicial do Compass

**Feature Branch**: `001-initial-onboarding`

**Created**: 2026-08-28

**Status**: Draft

**Input**: User description: "Criar o onboarding inicial do Compass. Um usuário abrindo uma instalação sem perfil deve configurar seu timezone IANA e sua disponibilidade semanal padrão. O backend cria o ScheduleProfile e devolve seu identificador. O frontend armazena apenas esse identificador para selecionar o perfil, enquanto timezone e disponibilidade continuam sendo recuperados do servidor. Ao recarregar a página, o perfil existente deve ser restaurado. Um identificador local inexistente ou inválido deve retornar o usuário ao onboarding. Não deve existir ProfileId fixo, timezone fixo, disponibilidade mockada ou necessidade de digitar GUID. O onboarding deve possuir: 1. apresentação curta; 2. seleção de timezone; 3. configuração dos dias e horários disponíveis; 4. confirmação; 5. redirecionamento para a tela Hoje. Critérios principais: timezone inválido é rejeitado; horário inicial deve ser anterior ao final; janelas sobrepostas são rejeitadas ou unificadas conforme regra explícita; dados persistem após F5; ausência de perfil não gera disponibilidade artificial; interface funciona por teclado e em tela móvel."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Primeiro Acesso e Configuração de Perfil Inicial (Priority: P1)

Como um novo usuário abrindo o Compass pela primeira vez em um ambiente sem perfil configurado, quero ser guiado por um assistente de onboarding simples e claro para definir meu fuso horário e minha disponibilidade semanal de trabalho, sendo redirecionado para a tela principal ("Hoje") com meu perfil salvo e ativo.

**Why this priority**: É a jornada essencial de entrada e pré-requisito absoluto para qualquer operação do Compass. Sem um perfil com fuso horário e disponibilidade configurados, nenhuma sugestão ou plano diário pode ser gerado corretamente.

**Independent Test**: Abrir a aplicação com armazenamento local limpo, preencher as etapas de apresentação, timezone e disponibilidade semanal, confirmar os dados e validar que o perfil foi criado pelo backend e que o usuário é redirecionado para a tela "Hoje".

**Acceptance Scenarios**:

1. **Given** que o usuário abre a aplicação sem nenhum identificador de perfil armazenado, **When** a página carrega, **Then** o sistema exibe a etapa 1 de boas-vindas do assistente de onboarding.
2. **Given** que o usuário está na etapa 2 (Seleção de Timezone), **When** seleciona um fuso horário IANA válido (ex.: `America/Sao_Paulo`), **Then** o sistema permite avançar para a etapa 3.
3. **Given** que o usuário está na etapa 3 (Disponibilidade Semanal), **When** seleciona os dias ativos (ex.: Segunda a Sexta) e define intervalos de horário válidos (ex.: `09:00` às `12:00` e `13:00` às `18:00`), **Then** o sistema valida que todos os horários iniciais são anteriores aos finais e permite avançar.
4. **Given** que o usuário está na etapa 4 (Confirmação), **When** revisa o resumo de timezone e disponibilidade e clica em "Confirmar e Concluir", **Then** o backend cria o `ScheduleProfile`, gera um novo identificador único, persiste os dados, o frontend salva o identificador no armazenamento local e redireciona o usuário para a tela "Hoje".

---

### User Story 2 - Restauração Automática de Sessão com Perfil Existente / F5 (Priority: P1)

Como um usuário que já configurou seu perfil no Compass, quero que minhas visitas subsequentes ou recarregamentos de página (F5) restaurem automaticamente meu perfil a partir do identificador armazenado, carregando timezone e disponibilidade diretamente do servidor sem reexibir o onboarding.

**Why this priority**: Garante continuidade de uso e respeita o princípio de que o backend é a fonte da verdade, permitindo que a aplicação reconheça o usuário sem exigir reconfiguração a cada acesso.

**Independent Test**: Concluir o onboarding, recarregar a página (F5) no navegador e constatar que a aplicação abre diretamente na tela "Hoje" com o fuso e disponibilidade recuperados do backend.

**Acceptance Scenarios**:

1. **Given** que o usuário possui um identificador de perfil válido armazenado localmente, **When** a aplicação é carregada ou recarregada (F5), **Then** o sistema consulta o backend utilizando o identificador, carrega os dados do `ScheduleProfile` e apresenta a tela "Hoje".
2. **Given** que a aplicação restaurou o perfil ativo, **When** o usuário inspeciona o estado da aplicação, **Then** nenhuma disponibilidade simulada ou mockada é utilizada; todos os dados refletem o estado persistido no backend.

---

### User Story 3 - Recuperação de Identificador Local Inválido ou Ausente (Priority: P2)

Como um usuário cuja referência local de perfil foi corrompida, excluída ou aponta para um identificador que não existe no backend, quero ser direcionado de forma segura e transparente de volta ao assistente de onboarding para configurar um perfil válido.

**Why this priority**: Evita estados quebrados, telas em branco ou bloqueios irrecuperáveis na interface quando a referência local estiver dessincronizada do backend.

**Independent Test**: Inserir manualmente um identificador inexistente no armazenamento local e recarregar a página, verificando que a aplicação limpa o identificador inválido e apresenta a tela de onboarding.

**Acceptance Scenarios**:

1. **Given** que o armazenamento local contém um identificador que não existe no backend, **When** a aplicação inicializa e a busca pelo perfil retorna "não encontrado" (404), **Then** o frontend remove a referência inválida e exibe a tela de onboarding inicial.
2. **Given** que o usuário nunca configurou um perfil, **When** tenta acessar diretamente qualquer rota interna da aplicação, **Then** o sistema intercepta a navegação e redireciona para o onboarding.

---

### User Story 4 - Validação e Normalização de Janelas de Disponibilidade e Timezone (Priority: P2)

Como um usuário configurando minha disponibilidade semanal, quero que o sistema me impeça de submeter horários inconsistentes (ex.: horário final antes do inicial) e unifique automaticamente intervalos sobrepostos no mesmo dia, garantindo integridade dos dados.

**Why this priority**: Protege a integridade das regras do domínio temporal do módulo `Calendar` e previne criação de janelas de tempo impossíveis ou ambíguas.

**Independent Test**: Tentar submeter fusos inválidos ou janelas com `StartTime >= EndTime` e verificar as mensagens de erro; configurar janelas sobrepostas (ex.: 08:00-12:00 e 10:00-14:00) e verificar que o sistema as unifica em 08:00-14:00.

**Acceptance Scenarios**:

1. **Given** que o usuário tenta submeter um fuso horário não reconhecido ou adulterado, **When** a validação é executada, **Then** o sistema rejeita a operação com mensagem de erro clara e impede a criação do perfil.
2. **Given** que o usuário define um intervalo com horário de término anterior ou igual ao de início (ex.: início `17:00`, término `09:00`), **When** tenta avançar, **Then** o formulário sinaliza o campo com erro e bloqueia o avanço.
3. **Given** que o usuário define intervalos sobrepostos ou adjacentes para o mesmo dia (ex.: `09:00` às `12:00` e `11:30` às `15:00`), **When** o perfil é salvo, **Then** o backend unifica os intervalos em uma única janela contínua (`09:00` às `15:00`).

---

### Edge Cases

- **Timezone do navegador não identificado**: Se o fuso do navegador não puder ser detectado automaticamente via API do navegador, o seletor apresenta uma lista completa de fusos IANA ordenada por região/alfabética com campo de busca/filtro, sem selecionar um valor padrão inválido.
- **Dia sem nenhuma janela de disponibilidade**: Dias da semana desmarcados pelo usuário são tratados como dias sem disponibilidade (folga/descanso), sem criação de janelas artificiais.
- **Nenhum dia selecionado na semana**: Se o usuário tentar avançar sem selecionar nenhum dia ou sem definir pelo menos uma janela de disponibilidade válida em toda a semana, o sistema exibe aviso de que ao menos um período de disponibilidade semanal deve ser configurado.
- **Navegação com teclado**: Todo o assistente de onboarding pode ser percorrido utilizando apenas o teclado (Tab, Shift+Tab, Enter, Espaço, Setas e Escape para dropdowns), com foco visível e acessibilidade ARIA.
- **Dispositivos móveis e telas estreitas**: O layout do assistente adapta-se responsivamente sem cortes de conteúdo ou rolagem horizontal em telas a partir de 320px de largura.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST verificar na inicialização da aplicação a existência de um identificador de perfil ativo no armazenamento local do cliente.
- **FR-002**: O sistema MUST exibir o assistente de onboarding inicial caso nenhum identificador de perfil esteja configurado ou caso o identificador existente seja inválido no backend.
- **FR-003**: O assistente de onboarding MUST ser composto por 5 etapas sequenciais claras:
  1. *Apresentação*: Breve introdução de boas-vindas explicando o objetivo do Compass.
  2. *Fuso Horário*: Seletor pesquisável de fuso horário padrão IANA, com sugestão inicial baseada no fuso do navegador do usuário.
  3. *Disponibilidade Semanal*: Configuração dia a dia (Segunda a Domingo) permitindo habilitar/desabilitar cada dia e adicionar uma ou mais janelas de horário com início e término.
  4. *Confirmação*: Painel de revisão sumarizando o fuso selecionado e a grade semanal configurada antes do envio.
  5. *Conclusão e Redirecionamento*: Criação do perfil no backend e redirecionamento automático para a tela "Hoje".
- **FR-004**: O backend MUST validar formalmente se o fuso horário submetido corresponde a um identificador IANA válido (ex.: `America/Sao_Paulo`, `Europe/London`, `UTC`) e rejeitar valores desconhecidos com erro de validação.
- **FR-005**: O sistema MUST exigir e validar que cada janela de horário possua horário de início estritamente anterior ao horário de término (`StartTime < EndTime`).
- **FR-006**: O backend MUST normalizar e unificar deterministicamente quaisquer intervalos sobrepostos ou imediatamente adjacentes configurados no mesmo dia da semana antes de persistir o perfil.
- **FR-007**: O backend MUST gerar e atribuir o identificador único do agregado `ScheduleProfile`, persistindo o fuso IANA e as janelas semanais de disponibilidade.
- **FR-008**: O backend MUST expor uma operação de consulta para recuperar os dados completos do `ScheduleProfile` (fuso horário e grade semanal de disponibilidade) a partir de seu identificador.
- **FR-009**: O frontend MUST armazenar apenas o identificador do perfil no armazenamento local do navegador e recuperar todos os dados de fuso e disponibilidade via consulta ao servidor.
- **FR-010**: O sistema MUST NOT utilizar identificadores fixos, fusos horários fixos "hardcoded" ou disponibilidades mockadas/fictícias no fluxo de produção.
- **FR-011**: O sistema MUST NOT exigir que o usuário digite ou manipule identificadores técnicos (ex.: GUID) manualmente na interface em nenhum momento.
- **FR-012**: A interface do onboarding MUST ser totalmente operável por teclado, contar com semântica HTML adequada e renderizar com layout responsivo em telas móveis e desktop.

### Key Entities *(include if feature involves data)*

- **ScheduleProfile**: Agregado raiz que representa o perfil de planejamento e disponibilidade do usuário.
  - *Identificador*: Identificador único gerado pelo backend.
  - *TimeZoneId*: String com identificador IANA válido (ex.: `America/Sao_Paulo`).
  - *WeeklyAvailability*: Coleção de regras de disponibilidade agrupadas por dia da semana.
  - *CreatedAt / UpdatedAt*: Marcas temporais de auditoria em UTC (`DateTimeOffset`).
- **DayAvailabilityRule**: Representa o conjunto de janelas de disponibilidade de um dia específico da semana.
  - *DayOfWeek*: Dia civil da semana (Segunda a Domingo).
  - *Windows*: Lista de intervalos de disponibilidade (`TimeWindow`) do dia.
- **TimeWindow**: Intervalo de tempo diário contínuo.
  - *StartTime*: Horário de início do período.
  - *EndTime*: Horário de término do período (estritamente posterior ao `StartTime`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Usuários novos sem perfil concluem todas as 5 etapas do onboarding e alcançam a tela "Hoje" em menos de 2 minutos.
- **SC-002**: Ao recarregar a página (F5) ou reabrir o navegador após o onboarding, o perfil existente é restaurado a partir do backend em menos de 1 segundo, sem exibir novamente o assistente de onboarding.
- **SC-003**: 100% das tentativas de submeter timezones inválidos ou janelas com horário inicial maior ou igual ao final são bloqueadas com mensagens de erro compreensíveis.
- **SC-004**: 100% dos intervalos sobrepostos configurados no mesmo dia são unificados corretamente em janelas contínuas no backend.
- **SC-005**: Todas as interações do assistente de onboarding podem ser realizadas via navegação por teclado e sem quebras de layout em visualização móvel (largura mínima de 320px).

## Assumptions

- O ambiente do navegador do usuário disponibiliza a API padrão `Intl.DateTimeFormat().resolvedOptions().timeZone` para a sugestão inteligente inicial do fuso horário.
- Para uma instalação de usuário único/cliente local, a persistência do identificador de perfil no `localStorage` do navegador é o mecanismo de associação da sessão do cliente ao seu perfil no backend.
- A tela de destino pós-onboarding ("Hoje") renderizará a data civil e a disponibilidade calculada a partir do perfil recém-criado.
- Janelas de disponibilidade representam períodos em que o usuário está apto a executar atividades no seu dia civil de trabalho.
