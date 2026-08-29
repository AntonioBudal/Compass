# Quickstart & Visual Validation Guide: Fundação Visual do Compass

**Feature**: `003-frontend-visual-foundation` | **Date**: 2026-08-28

## 1. Pré-requisitos e Inicialização

1. **Subir Backend**:
   ```bash
   dotnet run --project src/Host/Compass.Host
   ```
2. **Subir Frontend**:
   ```bash
   cd frontend
   npm run dev
   ```
3. **Acessar**: `http://localhost:5173/`

---

## 2. Roteiro de Validação Visual Ponta a Ponta

### Cenário 1: App Shell e Navegação entre Rotas
1. Acesse `http://localhost:5173/today`.
2. Verifique o cabeçalho superior unificado:
   - Logotipo textual `Compass` sóbrio;
   - Abas de navegação `Hoje` e `Planning` com indicador ativo fino;
   - Ausência total de emojis no cabeçalho.
3. Clique em `Planning` e observe a transição suave para `http://localhost:5173/planning` mantendo exatamente a mesma estrutura de cabeçalho e alinhamento.

### Cenário 2: Planning Inbox com Novos Tokens
1. Na tela `/planning`, inspecione:
   - Formulário de captura rápida: campo de texto limpo com botão `Capturar`;
   - Abas de filtro (`Todas`, `Draft`, `Ready`, `Em Andamento`, `Concluídas`) com badges numéricos neutros;
   - Cards de tarefas com bordas finas (`1px solid var(--color-border)`), sem sombras pesadas ou cantos excessivamente arredondados;
   - Badges semânticos sutis de status e duração;
   - Botões de ação (`Iniciar`, `Concluir`, `Editar`).
2. Abra o modal de edição de tarefa:
   - Verifique que o modal possui backdrop suave, campos alinhados e botões `Cancelar` e `Salvar`;
   - Pressione `Escape` e verifique que o modal fecha adequadamente.

### Cenário 3: Painel Hoje (`/today`) com Hierarquia Estilo Notion
1. Acesse `/today`.
2. Verifique a disposição vertical baseada em superfícies e divisores:
   - Título e data civil com tipografia limpa;
   - Seção de disponibilidade diária e semanal organizada por divisores, sem cards sobrepostos;
   - Ausência de emojis em todas as legendas.

### Cenário 4: Assistente de Onboarding (`/onboarding`)
1. Limpe o `localStorage` no DevTools e recarregue a página.
2. Percorra as 5 etapas do assistente:
   - Etapa 1 (Apresentação), Etapa 2 (Fuso Horário), Etapa 3 (Disponibilidade), Etapa 4 (Confirmação);
   - Verifique que os botões de avanço, selects de timezone e intervalos de horário usam os novos componentes de `shared/ui`;
   - Conclua e confirme o redirecionamento para `/today`.

### Cenário 5: Página 404 (`/rota-inexistente`)
1. Digite `http://localhost:5173/rota-inexistente`.
2. Verifique a exibição de mensagem 404 sóbria integrada ao App Shell, com botão funcional para retornar para a página inicial.

### Cenário 6: Responsividade Mobile e Acessibilidade por Teclado
1. Abra o DevTools e ative a emulação de dispositivos móveis com largura de **320px** e **375px**.
2. Navegue por todas as páginas e confirme:
   - Zero rolagem horizontal indesejada;
   - Textos e botões perfeitamente legíveis e tocáveis;
   - Navegação por teclado com tecla `Tab` exibindo anel de foco visível em todos os controles.

---

## 3. Comandos de Validação Automatizada

```bash
# Testes do Frontend
cd frontend
npm test -- --run

# Verificação de Tipos e Build de Produção
npm run build

# Testes de Regressão do Backend
cd ..
dotnet test Compass.slnx
```
