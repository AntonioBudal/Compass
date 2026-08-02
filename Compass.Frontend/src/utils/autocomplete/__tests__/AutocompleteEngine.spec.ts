import { describe, it, expect, beforeEach, vi } from 'vitest';
import { AutocompleteEngine } from '../AutocompleteEngine';
import type { AutocompleteProvider, Suggestion, AutocompleteContext } from '../types';

// ============================================================================
// HELPER: Mock Provider para injeção de dependência estrita nos testes
// ============================================================================
class MockProvider implements AutocompleteProvider {
  public triggerChar: string | null;
  public mockSuggestions: Suggestion[];
  
  // Usamos o vi.fn() para poder espiar (spy) quais argumentos a Engine enviou
  public getSuggestions = vi.fn((query: string, context: AutocompleteContext, limit: number) => {
    return this.mockSuggestions;
  });

  constructor(triggerChar: string | null, suggestions: Suggestion[] = []) {
    this.triggerChar = triggerChar;
    this.mockSuggestions = suggestions;
  }
}

describe('AutocompleteEngine', () => {
  let engine: AutocompleteEngine;

  // Garante um motor virgem e livre de estado global a cada teste
  beforeEach(() => {
    engine = new AutocompleteEngine(3); // Limitamos maxResults a 3 para facilitar asserts
  });

  // ============================================================================
  // 1. LEXER: Testes de Resolução de Cursor e Gatilhos (resolveActiveQuery)
  // ============================================================================
  describe('Lexer (resolveActiveQuery)', () => {
    it('deve extrair trigger de projeto corretamente', () => {
      const result = engine.resolveActiveQuery('Revisar #comp', 13);
      expect(result.isActive).toBe(true);
      expect(result.trigger).toBe('#');
      expect(result.query).toBe('comp');
      expect(result.wordStart).toBe(8); // Começa depois do espaço
    });

    it('deve extrair texto livre (History) quando não há gatilho', () => {
      const result = engine.resolveActiveQuery('Estudar Arquitetura', 7);
      expect(result.isActive).toBe(true);
      expect(result.trigger).toBe(null);
      expect(result.query).toBe('Estudar');
    });

    it('deve extrair parâmetros no meio do texto baseando-se no cursor', () => {
      // O usuário voltou o cursor para o meio do tempo "@45"
      const text = 'Revisar PR @45m !3 /t';
      const result = engine.resolveActiveQuery(text, 14); 
      expect(result.trigger).toBe('@');
      expect(result.query).toBe('45');
    });

    it('deve tratar strings vazias ou só com espaços', () => {
      expect(engine.resolveActiveQuery('', 0).isActive).toBe(false);
      expect(engine.resolveActiveQuery('   ', 3).isActive).toBe(false);
    });

    it('deve capturar corretamente no início absoluto da string', () => {
      const result = engine.resolveActiveQuery('#back', 5);
      expect(result.trigger).toBe('#');
      expect(result.query).toBe('back');
      expect(result.wordStart).toBe(0);
    });
  });

  // ============================================================================
  // 2. PARSER DE CONTEXTO: Testes de AST Leve (parseContext)
  // ============================================================================
  describe('Parser de Contexto (parseContext)', () => {
    it('deve extrair corretamente todos os parâmetros da string', () => {
      const context = engine.parseContext('Lançamento #v2-core !3 @120m ^amanha /e');
      expect(context.currentProject).toBe('v2-core');
      expect(context.currentEnergy).toBe(3);
      expect(context.currentTime).toBe('120m');
      expect(context.currentDate).toBe('amanha');
      expect(context.currentType).toBe('e');
    });

    it('deve retornar null para parâmetros não informados', () => {
      const context = engine.parseContext('Estudar inglês');
      expect(context.currentProject).toBeNull();
      expect(context.currentEnergy).toBeNull();
      expect(context.currentTime).toBeNull();
    });

    it('deve ignorar símbolos soltos que não formem um padrão válido', () => {
      const context = engine.parseContext('Comprar pão # ! @ ^ /');
      // Esperamos null porque as regex exigem ao menos caracteres logo após o gatilho
      expect(context.currentProject).toBeNull();
      expect(context.currentTime).toBeNull();
    });
  });

  // ============================================================================
  // 3. ROTEAMENTO E ORDENAÇÃO: Testes de Providers e Resultados
  // ============================================================================
  describe('Seleção do Provider e Ordenação', () => {
    let mockProjectProvider: MockProvider;
    let mockHistoryProvider: MockProvider;

    beforeEach(() => {
      mockProjectProvider = new MockProvider('#', [
        { label: '#Backend', insertText: 'Backend', type: 'project', score: 10 },
        { label: '#BancoDados', insertText: 'BancoDados', type: 'project', score: 50 },
        { label: '#Bugs', insertText: 'Bugs', type: 'project', score: 30 }
      ]);
      mockHistoryProvider = new MockProvider(null);

      engine.registerProvider(mockProjectProvider);
      engine.registerProvider(mockHistoryProvider);
    });

    it('deve rotear gatilho "#" para o ProjectProvider', () => {
      engine.getSuggestions('Revisar #B', 10);
      expect(mockProjectProvider.getSuggestions).toHaveBeenCalled();
      expect(mockHistoryProvider.getSuggestions).not.toHaveBeenCalled();
    });

    it('deve rotear texto sem gatilho para o HistoryProvider', () => {
      engine.getSuggestions('Estu', 4);
      expect(mockHistoryProvider.getSuggestions).toHaveBeenCalled();
      expect(mockProjectProvider.getSuggestions).not.toHaveBeenCalled();
    });

    it('deve retornar vazio se não houver provider registrado para o gatilho', () => {
      const results = engine.getSuggestions('Teste @45', 9); // Provider '@' não foi registrado neste teste
      expect(results).toEqual([]);
    });

    it('deve ordenar resultados pelo score e respeitar maxResults', () => {
      const results = engine.getSuggestions('Revisar #B', 10);
      
      expect(results.length).toBe(3); // Configuramos maxResults=3 no beforeEach
      expect(results[0].insertText).toBe('BancoDados'); // Score 50
      expect(results[1].insertText).toBe('Bugs');       // Score 30
      expect(results[2].insertText).toBe('Backend');    // Score 10
    });
  });

  // ============================================================================
  // 4. PREDIÇÃO FANTASMA: Testes da Engine de Confidence (Ghost Text)
  // ============================================================================
  describe('Ghost Prediction Engine', () => {
    let mockProvider: MockProvider;

    beforeEach(() => {
      // Registramos um único provider de texto livre para testar as regras de Confidence
      mockProvider = new MockProvider(null);
      engine.registerProvider(mockProvider);
    });

    it('deve exibir Ghost Text quando a confiança for alta (Prefixo exato + Tamanho + Score)', () => {
      mockProvider.mockSuggestions = [{
        label: 'Estudar Arquitetura', insertText: 'Estudar Arquitetura', type: 'history', score: 60
      }];

      // Usuário digitou "Est". 
      // Tamanho >= 2 (+0.1), Match Exato (+0.8), Score >= 50 (+0.1) => Confidence 1.0 (>= 0.85)
      const prediction = engine.getGhostPrediction('Est', 3);
      
      expect(prediction).not.toBeNull();
      expect(prediction?.ghostSuffix).toBe('udar Arquitetura');
      expect(prediction?.suggestion.confidence).toBeGreaterThanOrEqual(0.85);
    });

    it('deve rejeitar (retornar null) quando a confiança for baixa (Apenas 1 letra digitada e score baixo)', () => {
      mockProvider.mockSuggestions = [{
        label: 'Estudar Arquitetura', insertText: 'Estudar Arquitetura', type: 'history', score: 10
      }];

      // Usuário digitou apenas "E". 
      // Tamanho < 2 (sem bônus), Match Exato (+0.8), Score < 50 (sem bônus) => Confidence 0.80 (< 0.85 limite)
      const prediction = engine.getGhostPrediction('E', 1);
      
      expect(prediction).toBeNull();
    });

    it('deve rejeitar (retornar null) em match fuzzy (quando não começa com a string exata)', () => {
      mockProvider.mockSuggestions = [{
        label: 'Estudar Arquitetura', insertText: 'Estudar Arquitetura', type: 'history', score: 100
      }];

      // Usuário digitou "Arquitetura"
      // "Estudar Arquitetura" não COMEÇA com "Arquitetura", então o Ghost Text ficaria desalinhado visualmente.
      const prediction = engine.getGhostPrediction('Arquitetura', 11);
      
      expect(prediction).toBeNull();
    });

    it('deve retornar null silenciosamente se o provider não retornar sugestões', () => {
      mockProvider.mockSuggestions = [];
      const prediction = engine.getGhostPrediction('Est', 3);
      expect(prediction).toBeNull();
    });
  });

  // ============================================================================
  // 5. MUTAÇÃO: Teste do Mutador de String (applySuggestion)
  // ============================================================================
  describe('Aplicação de Sugestões (applySuggestion)', () => {
    it('deve substituir o token atual pela inserção completa mantendo o resto da frase intacto', () => {
      const fullText = 'Revisar PR #ba e subir';
      const cursor = 14; // Logo após o "#ba"
      const suggestion: Suggestion = {
        label: '#Backend', insertText: 'Backend', type: 'project', score: 50
      };

      const result = engine.applySuggestion(fullText, cursor, suggestion);

      expect(result.newText).toBe('Revisar PR #Backend  e subir');
      // Cursor deve parar logo após o espaço adicionado pela inserção
      expect(result.newCursorPosition).toBe(20); 
    });

    it('deve lidar corretamente com textos livres da HistoryProvider (sem trigger explícito)', () => {
      const fullText = 'Estu';
      const cursor = 4;
      const suggestion: Suggestion = {
        label: 'Estudar Vue 3', insertText: 'Estudar Vue 3 @60m', type: 'history', score: 90
      };

      const result = engine.applySuggestion(fullText, cursor, suggestion);

      // Substitui "Estu" por "Estudar Vue 3 @60m "
      expect(result.newText).toBe('Estudar Vue 3 @60m ');
    });
  });
});