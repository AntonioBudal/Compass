# Quickstart: Validação da Planning Inbox

**Feature**: `002-planning-inbox` | **Status**: Approved

---

## 1. Pré-Requisitos

1. PostgreSQL em execução (localmente ou via Docker/Testcontainers).
2. .NET 10 SDK instalado.
3. Node.js v20+ e npm instalados.

---

## 2. Executando o Ambiente

### Backend
```powershell
dotnet run --project src/Host/Compass.Host
```
A API estará acessível em `http://localhost:5000` (ou porta configurada).

### Frontend
```powershell
cd frontend
npm run dev
```
Acesse a aplicação em `http://localhost:5173/planning`.

---

## 3. Roteiro de Validação Ponta a Ponta

### Cenário 1: Captura Rápida de Tarefa Draft (MVP)
1. Acesse `http://localhost:5173/planning`.
2. No campo de captura rápida, digite `"Preparar apresentação de release"` e pressione Enter.
3. **Resultado esperado**:
   - A tarefa é criada imediatamente no backend com status `Draft`.
   - O card surge na coluna/aba `Draft` com badge informativo.
   - Ao recarregar a página (F5), a tarefa permanece visível com status `Draft`.

### Cenário 2: Estimativa de Duração e Promoção para Ready
1. No card da tarefa `"Preparar apresentação de release"` (Draft), clique no botão de definir estimativa.
2. Informe `45` minutos e confirme.
3. **Resultado esperado**:
   - O backend valida a estimativa positiva e transiciona a tarefa para `Ready`.
   - O card move-se para a aba `Ready`.
   - Ao recarregar a página (F5), a tarefa permanece em `Ready` com duração de 45 min.

### Cenário 3: Validação de Estimativa Inválida
1. Crie uma nova tarefa Draft `"Comprar café"`.
2. Tente definir uma estimativa de `0` ou `-10` minutos.
3. **Resultado esperado**:
   - O formulário bloqueia ou a API retorna `400 Bad Request`.
   - A tarefa permanece inalterada como `Draft`.

### Cenário 4: Iniciar e Concluir Tarefa (Lifecycle)
1. No card da tarefa `Ready`, clique em `"Iniciar"`.
2. **Resultado esperado**:
   - Status transiciona para `InProgress`.
3. Clique em `"Concluir"`.
4. **Resultado esperado**:
   - Status transiciona para `Done` com timestamp `completedAt` preenchido.
   - Tarefa é exibida na seção `Done` com estilo visual de concluída.

---

## 4. Comandos Automatizados de Teste

```powershell
# Testes do Backend
dotnet test Compass.slnx

# Testes e Build do Frontend
cd frontend
npm test -- --run
npm run build
```
