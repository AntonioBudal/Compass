# Quickstart & Validation Guide: Onboarding Inicial do Compass

**Feature**: `001-initial-onboarding` | **Status**: Approved

Este guia descreve os passos para inicializar e validar a implementação do onboarding ponta a ponta.

---

## 1. Pré-Requisitos

- **.NET 10 SDK** instalado.
- **Node.js 20+** e **npm** ou **pnpm** instalados.
- **Docker** em execução (necessário para os testes de integração com Testcontainers PostgreSQL).

---

## 2. Executando os Testes Automatizados

### Backend (Testes de Unidade, Integração e API)
```bash
# Executar todos os testes da solução
dotnet test Compass.sln --logger "console;verbosity=normal"
```

### Frontend (Testes de Componentes e Páginas)
```bash
# Navegar para o diretório do frontend
cd frontend

# Executar a suíte Vitest
npm test -- --run
```

---

## 3. Execução Manual Ponta a Ponta

### 1. Iniciar o Backend
```bash
dotnet run --project src/Host/Compass.Host
```
O backend estará acessível em `http://localhost:5000` (ou porta configurada), com OpenAPI/Swagger disponível em `/swagger` ou `/openapi`.

### 2. Iniciar o Frontend
```bash
cd frontend
npm install
npm run dev
```
O frontend estará acessível em `http://localhost:5173`.

---

## 4. Roteiro de Validação Manual

### Cenário 1: Novo Usuário (Primeiro Acesso)
1. Abrir o navegador em janela anônima e acessar `http://localhost:5173`.
2. Verificar que o sistema redireciona automaticamente para `/onboarding` na etapa 1 (Apresentação).
3. Clicar em "Avançar" para ir à etapa 2 (Timezone).
4. Verificar que o fuso do navegador é sugerido e selecionar um fuso IANA (ex.: `America/Sao_Paulo`).
5. Clicar em "Avançar" para ir à etapa 3 (Disponibilidade Semanal).
6. Configurar dias e horários (ex.: Segunda a Sexta, das `09:00` às `18:00`).
7. Clicar em "Avançar" para ir à etapa 4 (Confirmação) e revisar o resumo.
8. Clicar em "Confirmar e Concluir".
9. Validar que o backend responde com `201 Created` e que o usuário é redirecionado para a tela `/today` exibindo seu fuso e disponibilidade reais.

### Cenário 2: Persistência após Recarregamento (F5)
1. Estando na tela `/today`, pressionar `F5` ou recarregar a página.
2. Verificar que o sistema restaura o perfil a partir do `compass_active_profile_id` no `localStorage`, buscando os dados do backend e permanecendo em `/today` sem reexibir o onboarding.

### Cenário 3: Identificador Inválido / Corrompido
1. Abrir o DevTools (`F12`), ir em `Application > Local Storage` e alterar `compass_active_profile_id` para um GUID inexistente (ex.: `00000000-0000-0000-0000-000000000000`).
2. Pressionar `F5`.
3. Validar que a aplicação remove o identificador corrompido e exibe novamente a tela de onboarding inicial.

### Cenário 4: Validação de Horários Inconsistentes
1. No onboarding, na etapa 3, tentar definir início `18:00` e término `09:00`.
2. Verificar que o sistema bloqueia o avanço e sinaliza o erro visualmente.
