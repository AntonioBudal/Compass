# Compass — Change Log

**Projeto:** Compass  
**Stack:** ASP.NET Core (.NET 8/10), PostgreSQL 16+, Vue.js 3  
**Arquitetura:** Clean Architecture, Domain-Driven Design (DDD), TDD/BDD, Local-First

---

# 2026-07-16 — Etapa 01: Estrutura Inicial do Backend

## Objetivos concluídos

- Estruturação da solução em quatro projetos:
  - `Compass.Domain`
  - `Compass.Application`
  - `Compass.Infrastructure`
  - `Compass.Api`
- Configuração das referências entre projetos conforme a arquitetura definida.
- Isolamento da camada de domínio, mantendo independência de frameworks e infraestrutura.
- Implementação das entidades do domínio e das principais invariantes de negócio.
- Configuração da persistência utilizando Entity Framework Core e PostgreSQL.
- Definição da estratégia de herança:
  - TPH (`Commitment`, `Task`, `Habit`, `Event` e `Note`)
  - TPT (`Project` e `Goal`)
- Criação da migration inicial (`InitialProductionSchema`).

---

## Banco de Dados

### Recursos implementados

- Mapeamento dos enums nativos do PostgreSQL.
- Geração do esquema inicial da aplicação.
- Utilização de UUID como chave primária.
- Implementação de `CHECK CONSTRAINTS` para validação de regras estruturais.
- Criação de índices parciais voltados ao Motor de Decisão e ao sistema de lembretes.

---

## Observações

- A migration inicial foi gerada com sucesso.
- O banco PostgreSQL foi configurado para o ambiente de desenvolvimento.
- Permanecem warnings do Entity Framework Core relacionados aos valores padrão de propriedades `enum` configuradas com `HasDefaultValue()`. A correção foi adiada por não impactar o funcionamento atual da aplicação.

---

## Próximas etapas

- Implementação do `CompassDbContext`.
- Implementação dos repositórios.
- Configuração da camada Application.
- Desenvolvimento dos primeiros casos de uso.