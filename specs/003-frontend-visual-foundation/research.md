# Research: Fundação Visual Consistente e Migração de Interface

**Feature**: `003-frontend-visual-foundation` | **Date**: 2026-08-28

## 1. Auditoria do Estado Atual do Frontend

### Diagnóstico de Inconsistências
1. **Cores & Tokens**:
   - `tokens.css` atual continha valores escuros saturados (`#0f172a`, `#3b82f6`) sem suporte arquitetural a Light Mode e com sombras volumosas (`--shadow-lg`).
   - Alguns componentes continham cores literais hardcoded ou manipulações com `rgba(...)` diretamente em seus blocos `<style scoped>`.
2. **Uso Indevido de Emojis**:
   - Foram identificados emojis em títulos de assistentes (`✨`, `👋`), botões de navegação e abas (`📅`, `🕒`, `📥`, `⚡`, `✅`, `🧭`), empty states e mensagens de erro.
   - Violação do Princípio Constitucional 30 (proibição de emojis em toda a interface).
3. **Estrutura de Layout e Excesso de Cards ("Carditis")**:
   - A página Hoje (`TodayPage.vue`) e Onboarding (`OnboardingPage.vue`) abusavam de múltiplos cards encaixotados e sobrepostos com bordas arredondadas pesadas, em vez de superfícies contínuas e divisores sutis.
4. **Ausência de App Shell Central**:
   - A navegação entre Hoje e Planning era duplicada em cabeçalhos locais dentro das próprias páginas, sem um container comum.
5. **Acessibilidade e Foco**:
   - Variações nos indicadores de foco entre inputs, botões e abas.

---

## 2. Decisões de Design & Arquitetura Visual

### Decisão 1: Paleta Neutra Controlada (GitHub + Notion)
- **Decisão**: A base padrão do Compass adota um tema claro sóbrio e utilitário, inspirado na densidade do GitHub Primer e no respiro tipográfico do Notion.
- **Escala Neutra Controlada**:
  - `canvas/app-bg`: `#ffffff` / `#fcfcfc`
  - `surface`: `#ffffff`
  - `surface-subtle`: `#f6f8fa` (fundo de cabeçalhos, barras laterais e seções)
  - `surface-hover`: `#f3f4f6`
  - `border`: `#d0d7de` (divisores e contornos de 1px)
  - `border-subtle`: `#e1e4e8`
  - `text-primary`: `#1f2328` (contraste ótimo 14.5:1)
  - `text-secondary`: `#656d76` (contraste 5.5:1, compatível com WCAG AA)
  - `text-muted`: `#8c959f` (metadados e placeholders)
- **Alternativas consideradas**:
  - *Dark mode forçado*: Rejeitado para alinhar à identidade primária sóbria e legível em ambientes iluminados.
  - *Cores decorativas roxas/esmeralda estilo SaaS AI*: Rejeitado categoricamente pelo Princípio Constitucional 33.

### Decisão 2: Preparação para Temas Claro, Escuro e Sistema
- **Decisão**: Toda estilização consome variáveis semânticas CSS em `:root`. A folha `tokens.css` conterá a especificação para `[data-theme="dark"]` e `@media (prefers-color-scheme: dark)`, viabilizando alternância de tema no futuro sem necessidade de refatorar componentes.
- **Isolamento**: O seletor visual de temas em tempo de execução NÃO será implementado agora, mantendo o escopo estritamente na fundação.

### Decisão 3: Conjunto Minimalista de Ícones SVG e Texto Puro
- **Decisão**: Eliminação de 100% dos emojis. Substituição por texto claro descritivo ou ícones SVG utilitários inline com traço consistente (`stroke-width="1.5"` ou `"2"`), viewBox `24x24`, herdando `currentColor`.
- **Alternativas consideradas**:
  - *Instalar pacote pesado de ícones*: Rejeitado para evitar inchaço de bundle e complexidade externa desnecessária.

### Decisão 4: Estrutura Baseada em Divisores e App Shell
- **Decisão**: Criar o componente `AppShell.vue` (e `AppHeader.vue` / `AppNavigation.vue`) em `frontend/src/shared/ui/` ou `frontend/src/app/`.
- **Comportamento**: As rotas autenticadas (`/today`, `/planning`) compartilham o mesmo App Shell com navegação padronizada, sem duplicação de layout.
