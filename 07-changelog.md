# Developer Changelog

## 2026-07-16

### Arquitetura

* Estruturação da solução em `Compass.Domain`, `Compass.Application`, `Compass.Infrastructure` e `Compass.Api`.
* Configuração do Entity Framework Core com PostgreSQL.
* Implementação das estratégias de herança TPH e TPT.

### Banco de Dados

* Mapeamento de enums do PostgreSQL.
* Utilização de UUID como chave primária.
* Adição de CHECK Constraints e índices.
* Criação da migration `InitialProductionSchema`.

---

## 2026-07-17

### Backend

* Implementação do `ScoringEngine`.
* Implementação do `TimeWindowCalculator`.
* Modelagem de `CommitmentAttribute`.
* Modelagem de `DecisionSnapshot`.
* Implementação dos repositórios de Commitments, Projects e DecisionSnapshots.

### Banco de Dados

* Criação da tabela `decision_snapshots`.
* Configuração de relacionamentos e índices.
* Criação da migration `AddDecisionSnapshot`.

---

## 2026-07-18

### Backend

* Implementação dos DTOs.
* Configuração do FluentValidation.
* Implementação de `DecisionService`.
* Implementação de `CommitmentService`.

### API

* Criação dos controllers REST.
* Implementação do `GlobalExceptionHandler`.
* Suporte ao header `X-User-Id`.

---

## 2026-07-19

### Frontend

* Inicialização do projeto com Vue 3, Vite, TypeScript e Tailwind CSS.
* Configuração do Axios.
* Configuração do Pinia.
* Implementação do Design System.
* Implementação do App Shell.
* Implementação de atalhos globais de teclado.
* Suporte a `prefers-reduced-motion`.

### Stores

* `useCommitmentsStore`.
* `useDecisionStore`.

### Componentes

* `CommitmentCard`.
* `TopFocusCard`.
* `CommandBarModal`.
* `QuickCaptureModal`.

### Views

* `NowEngineView`.
* `AgendaView`.
* `ProjectsView`.
* `GoalsView`.
* `HabitsView`.

---

## 2026-07-20

### Frontend

* Compatibilidade entre Node.js, Vue-TSC e TypeScript.
* Padronização das animações no Tailwind CSS.
* Implementação de utilitários para aceleração por GPU.
* Adição de microinterações.

### Stores

* `toastStore`.
* `journalStore`.
* `settingsStore`.

### Componentes

* Tooltip de explicabilidade no `TopFocusCard`.
* Remoção de elementos visuais coloridos e emojis.
* Padronização da interface monocromática.

### Views

* `SettingsView`.
* `JournalView`.

### Modais

* `EditCommitmentModal`.
* `DailyShutdownModal`.

# Developer Changelog

## 2026-07-21

### Backend & API
- Adicionado endpoint `/api/v1/health` para monitoramento da API.
- Implementado seeder de usuário padrão para ambiente de desenvolvimento.
- Padronizada a propagação de erros utilizando RFC 7807 (Problem Details).
- Adicionado suporte a Correlation ID (`X-Correlation-Id`) nas requisições.

### Banco de Dados
- Corrigido o mapeamento TPH para propriedades específicas de subtipos (`CronExpression`, `StartTime` e `EndTime`).
- Criada a migration `FixTphNullability`.

### Frontend
- Implementado parser do Quick Capture baseado em tokens (`@`, `!`, `#`, `/`).
- Adicionado monitoramento de Long Tasks utilizando `PerformanceObserver`.
- Implementado tratamento global de erros da aplicação Vue.
- Adicionado suporte a fila de sincronização offline.
- Implementado rastreamento de requisições via Correlation ID.

### Stores
- Criada `useDevStore`.
- Criada `useOfflineStore`.

### Componentes
- Criado `ErrorBoundary`.
- Criado `DeveloperConsole`.
- Atualizada `StatusBar` com informações de sincronização, telemetria e requisições pendentes.

### Modais
- Refatorado `QuickCaptureModal` para entrada única baseada em comandos.
- Adicionada herança de contexto conforme a tela atual.

# Developer Changelog

## 2026-07-22

### Arquitetura & DX (Orquestrador de Ambiente)
* Implementação do orquestrador de desenvolvimento unificado (`scripts/dev-orchestrator.mjs`), permitindo iniciar todo o ecossistema com o comando único `npm run dev`.
* Implementação de escudo de segurança **Anti-Loop (Recursion Guard)** com a variável `COMPASS_ORCHESTRATOR_ACTIVE`, impedindo chamadas recursivas infinitas do NPM no Windows (Fork Bomb).
* Implementação do **Caçador de Zumbis (Zombie Killer)** interplataforma (`taskkill` no Windows / `pkill` no Unix) para limpar processos presas do `Compass.Api.exe` na memória antes do boot da API.
* Substituição de chamadas intermediárias do NPM pela execução direta dos binários via `npx vite --port 5173`.
* Adição da verificação de saúde TCP via socket (Porta `5432`) bloqueando a inicialização caso o PostgreSQL esteja indisponível.
* Padronização do `package.json` do frontend para suporte nativo a ECMAScript Modules (`"type": "module"`).

### Design System & Identidade Tática (Banimento de Cores)
* **Expurgo Definitivo de Cores:** Banimento arquitetural absoluto das paletas `emerald-*` e `indigo-*` em todo o ecossistema. Toda a identidade visual transitou para o **Monocromático Tático de Alta Densidade**.
* Erradicação de cores literais (*hardcoded*) e substituição integral de todos os arquivos `.vue` por **Design Tokens Semânticos** via CSS Custom Properties (`--bg-app`, `--bg-surface`, `--text-main`, `--border-base`, `--text-accent`, etc.).
* Mapeamento semântico de tokens diretamente no `tailwind.config.js` (`bg-surface`, `text-content`, `borderbase`, etc.).
* Manutenção rigorosa de aceleração por GPU (`.gpu-accelerated`) e diretivas de acessibilidade para animações reduzidas (`prefers-reduced-motion: reduce`).

### UI/UX & Motor de Temas
* Implementação do motor de gerenciamento de temas no Pinia (`useThemeStore`), com persistência instantânea no `localStorage` e latência de aplicação $< 16\text{ms}$.
* Injeção do **Escudo Anti-FOUC (Flash of Unstyled Content)** no cabeçalho do `index.html`, garantindo que o tema salvo seja aplicado ao DOM antes da hidratação do Vue e evitando piscadas na tela.
* Refatoração da tela de configurações (`SettingsView.vue`) com um grid de seleção visual monocromático e amostras cromáticas dos tokens em tempo real.
* Curadoria de suíte profissional com **8 Temas de Nível IDE e Editorial**:
  * `dark`: **Graphite Dark** (Padrão Zinc / Estilo Linear / GitHub Dark)
  * `oled`: **OLED Absolute** (Preto absoluto `#000000` para economia de bateria AMOLED)
  * `light`: **Light IDE** (Branco de alta precisão técnica)
  * `paper`: **Warm Paper** (Modo editorial claro em tom sépia suave, estilo Notion/E-reader)
  * `slate`: **Gunmetal Slate** (Azul-Grafite técnico, estilo DevOps / Vercel)
  * `nordic`: **Nordic Night** (Estética ártica escandinava Nord)
  * `solarized`: **Solarized Precision** (Contraste clássico de alta engenharia)
  * `matrix`: **Terminal Green** (Fósforo verde hacker estilo UNIX)

---
# 2026-07-23 

## Onboarding e Treinamento Zero-Mouse

* **Sandbox em Memória RAM:** Implementação de um ambiente de simulação isolado em memória (`onboardingStore`), permitindo que novos usuários testem funcionalidades como criação, adiamento e conclusão de tarefas sem salvar dados no banco PostgreSQL.

* **Treinamento Interativo Zero-Mouse:** Desenvolvimento do fluxo guiado de onboarding em 4 etapas (`OnboardingSteps.vue`), utilizando uma camada de destaque (`SpotlightOverlay.vue`). O segundo passo possui validação dinâmica que exige a utilização dos tokens NLP (`@tempo`, `!energia`, `#projeto`) antes de liberar a próxima etapa.

* **Navegação Livre no Sandbox:** Ajuste do fluxo de navegação para permitir acesso ao ambiente de treinamento diretamente pela barra lateral (`[RAM SANDBOX]`), possibilitando retornar ao modo de prática a qualquer momento.

---

## Acessibilidade e Controle de Foco

* Implementação do composable `useFocusTrap.ts`, responsável por controlar a navegação via teclado (`Tab` e `Shift+Tab`) dentro de modais ativos, evitando que o foco seja direcionado para elementos fora do contexto atual.

* Adição de atributos de acessibilidade seguindo o padrão WCAG 2.1 AA (`role="dialog"` e `aria-modal="true"`) nos componentes `QuickCaptureModal.vue` e `CommandBarModal.vue`.

---

## Performance e Otimização do Frontend

* **Code Splitting e Lazy Loading:** Migração de rotas e componentes mais pesados para carregamento sob demanda utilizando importações assíncronas (`() => import(...)` e `defineAsyncComponent`), reduzindo o carregamento inicial da aplicação.

* **Otimização do Build com Vite/Rollup:** Configuração do `vite.config.ts` para melhorar a organização dos arquivos gerados, separando dependências em diferentes chunks (`vendor-core`, `vendor-icons` e módulos individuais). O tempo de build foi reduzido para aproximadamente **1,86 segundos**, com melhor distribuição dos arquivos gerados.

---

## Melhorias no Backend (.NET 10) e Resiliência

* **Rastreabilidade de Erros (Correlation IDs):** Atualização do `GlobalExceptionHandler.cs` para adicionar o identificador da requisição (`TraceIdentifier`) nas respostas de erro utilizando o padrão RFC 7807 (`ProblemDetails`), facilitando a análise e resolução de problemas.

* **Maior Resiliência nas Conexões com Banco:** Configuração do `EnableRetryOnFailure` nas conexões Npgsql/EF Core, permitindo novas tentativas automáticas em casos de falhas temporárias de rede ou reinicializações do PostgreSQL.

* **Endpoint de Monitoramento:** Implementação do endpoint `/api/healthz` para verificar a disponibilidade da aplicação e medir a latência da comunicação com o banco de dados em tempo real.

---

## Homologação e Testes Automatizados

* Configuração do **Vitest** integrado ao ambiente Vite, com suporte ao ambiente simulado de navegador utilizando `jsdom`.

* Criação e validação de testes unitários:

  * `offlineStore.spec.ts`: valida o funcionamento da fila offline, incluindo armazenamento temporário em memória e persistência no `localStorage`.

  * `nlpParser.spec.ts`: valida a interpretação dos comandos de entrada, incluindo extração de tempo, nível de energia e identificação de projetos através dos tokens NLP.