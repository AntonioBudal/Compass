# 01. Overview

**Versão:** 1.0 (Especificação Determinística)  
**Stack Alvo:** Vue.js 3 | ASP.NET Core (.NET 8+) | PostgreSQL 16+

---

## 1. Missão

> "Reduzir a carga cognitiva do gerenciamento pessoal, transformando um sistema tradicional de organização passiva em um mecanismo ativo de priorização e execução."

O Compass não tem como objetivo apenas armazenar tarefas. Seu propósito é responder continuamente:

**"Qual é a ação mais relevante e viável para executar neste momento?"**

A aplicação atua como um sistema de decisão, considerando contexto, disponibilidade de tempo, energia e objetivos ativos do usuário.

---

## 2. O Problema e a Solução

### 2.1. O Problema (Limitação dos Sistemas Atuais)

A maioria das ferramentas atuais de produtividade assume que o usuário possui capacidade ilimitada de organizar, classificar e priorizar suas próprias atividades.

Na prática, elas funcionam como **repositórios passivos de informações**:

* O usuário precisa criar estruturas manualmente através de etiquetas, categorias, projetos e prioridades artificiais.
* A manutenção do sistema se torna uma tarefa adicional, aumentando a carga operacional.
* Ao abrir o aplicativo em um intervalo curto de disponibilidade, o usuário encontra dezenas de tarefas sem contexto suficiente para decidir qual executar.
* O excesso de opções gera **paralisia de decisão** e reduz a taxa de execução.

---

### 2.2. A Solução (Modelo Compass)

O Compass transfere parte da responsabilidade de organização para o próprio sistema, operando como um **motor determinístico de decisão**.

Principais características:

* Captura rápida de intenções através de **Arquétipos Visuais**, reduzindo a necessidade de preenchimento manual extenso.
* Cálculo automático da disponibilidade real do usuário, considerando compromissos fixos da agenda.
* Análise do estado atual de energia e contexto operacional.
* Geração dinâmica das **Top 3 Ações Viáveis**, priorizadas através de um algoritmo de pontuação determinístico.

O sistema não substitui a intenção do usuário; ele reduz o custo de transformar intenção em execução.

---

# 3. Análise Competitiva

| Ferramenta | Filosofia Central | Limitação Principal | Abordagem COMPASS |
| :--- | :--- | :--- | :--- |
| **Notion** | Sistema flexível baseado em páginas, blocos e bancos de dados. | O usuário precisa projetar, manter e evoluir sua própria estrutura organizacional. | **Modelo Estruturado.** O Compass fornece uma experiência orientada a decisão sem exigir configuração constante. |
| **Todoist / TickTick** | Gerenciamento baseado em listas, projetos e prioridades manuais. | Grande volume de tarefas exige revisão constante e classificação manual. | **Priorização Contextual.** O sistema considera tempo disponível, energia e contexto antes de apresentar tarefas. |
| **Sistemas baseados em IA (LLMs)** | Interação através de linguagem natural e processamento semântico. | Dependência externa, latência, custos operacionais e possibilidade de inconsistência nas respostas. | **Motor Determinístico.** Decisões baseadas em regras explícitas, cálculos previsíveis e dados estruturados. |

---

# 4. Princípios Operacionais

## 4.1. O que o usuário nunca precisará fazer:

1. Não precisará administrar estruturas complexas de organização.
2. Não precisará ordenar manualmente tarefas na tela principal.
3. Não precisará reorganizar constantemente tarefas atrasadas.
4. Não precisará preencher formulários extensos para registrar uma atividade.
5. Não precisará estimar sozinho se uma tarefa é compatível com sua disponibilidade atual.

---

## 4.2. O que o sistema sempre fará automaticamente:

1. Sempre considerará **Blocos Rígidos (*Hard Blockers*)** para calcular disponibilidade real.
2. Sempre filtrará tarefas incompatíveis com o contexto atual.
3. Sempre calculará a relevância das tarefas através do algoritmo de pontuação.
4. Sempre manterá consistência dos dados através de uma única fonte de verdade.
5. Sempre evitará decisões baseadas em informações duplicadas ou conflitantes.

---

# 5. Os 5 Pilares Arquiteturais

## 1. Captura em 4 Segundos (Fricção Zero)

Interface otimizada para registro rápido utilizando:

* Arquétipos visuais.
* Atalhos de teclado.
* Fluxos mínimos de interação.

O objetivo é reduzir o intervalo entre intenção e registro.

---

## 2. O "Agora" como Unidade Principal de Execução

O painel principal não representa uma lista completa de tarefas.

Ele representa o contexto atual de execução:

* O que deve ser feito agora.
* O que pode ser feito em seguida.
* O que deve permanecer fora do foco.

---

## 3. Decisão Matemática e Transparente

As recomendações devem ser explicáveis.

O sistema deve demonstrar os fatores utilizados na decisão:

Exemplo:

> "Recomendado porque possui prioridade alta, cabe nos seus 40 minutos disponíveis e está alinhado ao objetivo atual."

---

## 4. Uma Única Fonte de Verdade

O domínio utiliza um modelo consistente de dados:

* Entidades centralizadas.
* Regras de negócio no domínio.
* Persistência relacional sem duplicação de informação.
* Polimorfismo de domínio quando necessário.

---

## 5. Velocidade como Recurso (Local-First Feel)

A experiência deve transmitir resposta imediata:

* Processamento previsível.
* Baixa latência.
* Pouca dependência de serviços externos.
* Sensação de aplicação local mesmo utilizando arquitetura distribuída.