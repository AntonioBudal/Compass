# Implementation Plan: Fundação Visual Consistente e Migração de Interface

**Branch**: `003-frontend-visual-foundation` | **Date**: 2026-08-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/003-frontend-visual-foundation/spec.md`

## Summary

Esta feature constrói a fundação visual definitiva e unificada do Compass, combinando a organização, densidade e bordas sutis do GitHub com o respiro e a hierarquia tipográfica do Notion em uma identidade própria baseada em branco e tons neutros. Realiza a auditoria, limpeza e migração completa de todas as telas existentes (`/onboarding`, `/today`, `/planning`, 404), eliminando 100% dos emojis, cores hardcoded, gradientes e sombras excessivas, padronizando os componentes base de `shared/ui`, introduzindo um App Shell comum e gerando a documentação oficial em `docs/design/FRONTEND_DESIGN_SYSTEM.md`.

---

## Technical Context

**Language/Version**: TypeScript 5.7+ (estrito), Vue 3.5+ (Composition API com `<script setup>`), CSS3 Moderno (Custom Properties).

**Primary Dependencies**: Vue Router 4.5+, `@tanstack/vue-query` 5+, Vite 6+, `@vue/test-utils` 2+, Vitest 3+.

**Storage**: `localStorage` no cliente para `compass_active_profile_id`; Nenhuma alteração no backend/PostgreSQL.

**Testing**: Vitest (`npm test -- --run`), Vue Test Utils, `vue-tsc -b && vite build`. Testes de regressão do backend via `dotnet test Compass.slnx`.

**Target Platform**: Navegadores modernos (Desktop 1280px+ e Mobile 320px+).

**Project Type**: Frontend Web Application (FSD Pragmático).

**Performance Goals**: Renderização inicial instantânea (FOUC = 0ms), zero dependências externas de fontes pesadas, bundle de produção otimizado (< 150KB gzip).

**Constraints**:
- Conformidade total com os Princípios Constitucionais 26 a 45 (Constituição v1.1.0).
- Proibição absoluta de emojis em toda a interface.
- Proibição de cores hardcoded dentro de componentes `.vue`.
- Zero alterações em contratos de backend ou endpoints HTTP.
- Preparação arquitetural de tokens para Claro, Escuro e Sistema, sem implementar o switcher de tema no runtime.

**Scale/Scope**: 4 telas principais (`/onboarding`, `/today`, `/planning`, `NotFoundPage`), 8 componentes compartilhados em `shared/ui`, 1 folha central de tokens (`tokens.css`), 1 documento canônico de design system (`FRONTEND_DESIGN_SYSTEM.md`).

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Princípio 24 (FSD Pragmático)**: Camadas `app`, `pages`, `features`, `entities`, `shared` preservadas e respeitadas.
- **Princípio 25 (Acessibilidade e Semântica)**: Foco visível (`:focus-visible`), contraste WCAG AA e navegação por teclado garantidos.
- **Princípios 26 a 45 (Design & UI Principles)**:
  - *26 & 34*: Interface sóbria com branco e escala de 4 a 5 tons neutros. (PASS)
  - *27 & 28*: GitHub como referência estrutural; Notion como referência tipográfica. (PASS)
  - *30*: Proibição absoluta de emojis em toda a aplicação. (PASS)
  - *31*: Conjunto unificado de ícones SVG minimalistas ou texto puro. (PASS)
  - *32 & 33*: Proibição de gradientes, glow, glassmorphism e cores saturadas padrão. (PASS)
  - *36 & 37*: Estrutura em divisores e superfícies antes de cards; cards restritos a unidades atômicas. (PASS)
  - *38*: Proibição de cores hardcoded; consumo obrigatório de tokens CSS semânticos. (PASS)
  - *39 & 40*: Tokens preparados para Claro/Escuro/Sistema sem depender de switcher de temas na UI. (PASS)
  - *41*: Legibilidade e densidade confortáveis em 1280px+ e 320px+. (PASS)
  - *42*: Consistência de estados (`hover`, `focus-visible`, `active`, `disabled`, `loading`, `error`, `empty`). (PASS)
  - *44*: Reutilização estrita de `shared/ui`. (PASS)
  - *45*: Validação visual em desktop e mobile. (PASS)

*Resultado da Avaliação*: **100% Aprovado (0 violações).**

---

## Project Structure

### Documentation (this feature)

```text
specs/003-frontend-visual-foundation/
├── plan.md              # Este arquivo
├── research.md          # Diagnóstico da auditoria e decisões de design
├── data-model.md        # Especificação dos tokens semânticos e escala de tipografia/espaçamento
├── quickstart.md        # Roteiro de validação visual e execução de testes
├── contracts/
│   └── design-tokens.md # Contrato de variáveis CSS e interfaces de componentes compartilhados
└── checklists/
    └── requirements.md  # Checklist de qualidade da especificação
```

### Source Code (repository root)

```text
docs/
└── design/
    └── FRONTEND_DESIGN_SYSTEM.md  # [NOVO] Documentação oficial e canônica do Design System

frontend/
├── src/
│   ├── app/
│   │   ├── App.vue                    # [MODIFY] Integração com AppShell e tokens
│   │   ├── styles/
│   │   │   └── tokens.css             # [MODIFY] Paleta neutra sóbria, tokens semânticos e Light/Dark
│   │   └── router/
│   │       └── index.ts               # [MODIFY] Adição da rota 404 (NotFoundPage)
│   ├── shared/
│   │   └── ui/
│   │       ├── AppButton.vue          # [MODIFY] Refatoração com tokens, estados, loading spinner e sem emojis
│   │       ├── AppInput.vue           # [MODIFY] Refatoração com tokens e foco visível
│   │       ├── AppSelect.vue          # [MODIFY] Refatoração com tokens
│   │       ├── TimeRangePicker.vue    # [MODIFY] Refatoração com tokens e sem emojis
│   │       ├── EmptyState.vue         # [MODIFY] Refatoração com tokens, ícone SVG sutil e sem emojis
│   │       ├── AppBadge.vue           # [NEW] Badge semântico neutro e compacto
│   │       ├── AppModal.vue           # [NEW] Container de modal reutilizável e acessível
│   │       └── AppShell.vue           # [NEW] Layout root com cabeçalho de navegação compartilhado
│   ├── features/
│   │   ├── onboarding/
│   │   │   └── components/
│   │   │       ├── StepPresentation.vue  # [MODIFY] Remoção de emojis, visual limpo
│   │   │       ├── StepTimeZone.vue      # [MODIFY] Consumo dos novos componentes
│   │   │       ├── StepAvailability.vue  # [MODIFY] Refatoração com divisores
│   │   │       └── StepConfirmation.vue  # [MODIFY] Resumo limpo sem emojis
│   │   └── planning-inbox/
│   │       └── components/
│   │           ├── QuickTaskCapture.vue  # [MODIFY] Refatoração com novos tokens
│   │           ├── TaskFilterTabs.vue    # [MODIFY] Abas sóbrias estilo GitHub/Notion sem emojis
│   │           ├── TaskCard.vue          # [MODIFY] Card limpo com borda fina e badges sutis
│   │           └── TaskEditModal.vue     # [MODIFY] Migração para AppModal compartilhado
│   └── pages/
│       ├── onboarding/
│       │   └── OnboardingPage.vue        # [MODIFY] Layout limpo baseado em superfícies
│       ├── today/
│       │   └── TodayPage.vue             # [MODIFY] Hierarquia Notion, remoção de emojis e integração com AppShell
│       ├── planning/
│       │   └── PlanningPage.vue          # [MODIFY] Integração com AppShell e novos tokens
│       └── not-found/
│           └── NotFoundPage.vue          # [NEW] Página 404 integrada ao design system
└── tests/
    └── [testes de componentes e páginas atualizados para a nova semântica sem emojis]
```

**Structure Decision**: A organização segue estritamente a metodologia FSD pragmática, centralizando os tokens em `src/app/styles/tokens.css`, componentes compartilhados em `src/shared/ui/` e migrando as features e páginas existentes sem alterar rotas funcionais ou chamadas de API.

---

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| *Nenhuma* | Todas as decisões seguem estritamente a Constituição v1.1.0 | N/A |
