import { describe, it, expect } from 'vitest';
import { parseQuickCapture } from '../nlpParser';

describe('NLP Parser v2.0 — Quick Capture CLI Engine', () => {
  
  // ==========================================================================
  // BLOCO DA MANHÃ: Roteamento de Arquétipos (/t, /h, /e, /n) & Tipagem
  // ==========================================================================
  describe('Roteamento de Tipos e Arquétipos', () => {
    it('deve assumir TASK como fallback padrão quando nenhuma flag for fornecida', () => {
      const result = parseQuickCapture('Revisar backlog de bugs');
      expect(result.type).toBe('TASK');
      expect(result.title).toBe('Revisar backlog de bugs');
    });

    it('deve identificar corretamente os 4 arquétipos via flags curtas', () => {
      expect(parseQuickCapture('Beber 3L de água /h').type).toBe('HABIT');
      expect(parseQuickCapture('Reunião de Arquitetura /e').type).toBe('EVENT');
      expect(parseQuickCapture('Ideia sobre o cache LRU /n').type).toBe('NOTE');
      expect(parseQuickCapture('Implementar endpoint REST /t').type).toBe('TASK');
    });

    it('deve ignorar maiúsculas/minúsculas e aceitar palavras completas', () => {
      expect(parseQuickCapture('Correr na esteira /HABITO').type).toBe('HABIT');
      expect(parseQuickCapture('Almoço com investidor /evento').type).toBe('EVENT');
      expect(parseQuickCapture('Lembrete de leitura /Nota').type).toBe('NOTE');
    });
  });

  // ==========================================================================
  // BLOCO DA TARDE: Motor Temporal Determinístico (^token)
  // ==========================================================================
  describe('Resolução Temporal Determinística (^data)', () => {
    it('deve resolver ^hoje para 23:59:59.999 do dia atual sem salto de fuso', () => {
      const result = parseQuickCapture('Entregar relatório financeiro ^hoje');
      expect(result.deadlineIso).not.toBeNull();

      // Transforma o ISO UTC de volta para o Date local para checar a precisão
      const resolvedDate = new Date(result.deadlineIso!);
      const now = new Date();

      expect(resolvedDate.getDate()).toBe(now.getDate());
      expect(resolvedDate.getMonth()).toBe(now.getMonth());
      expect(resolvedDate.getHours()).toBe(23);
      expect(resolvedDate.getMinutes()).toBe(59);
      expect(resolvedDate.getSeconds()).toBe(59);
    });

    it('deve resolver ^amanha exatamente +1 dia às 23:59:59 local', () => {
      const result = parseQuickCapture('Deploy de produção ^amanha');
      const resolvedDate = new Date(result.deadlineIso!);
      
      const expectedTomorrow = new Date();
      expectedTomorrow.setDate(expectedTomorrow.getDate() + 1);

      expect(resolvedDate.getDate()).toBe(expectedTomorrow.getDate());
      expect(resolvedDate.getHours()).toBe(23);
    });

    it('deve resolver datas absolutas no formato ^YYYY-MM-DD perfeitamente', () => {
      const result = parseQuickCapture('Marco da Release v2.0 ^2026-12-31');
      expect(result.deadlineIso).not.toBeNull();

      const resolvedDate = new Date(result.deadlineIso!);
      expect(resolvedDate.getFullYear()).toBe(2026);
      expect(resolvedDate.getMonth()).toBe(11); // Dezembro é índice 11
      expect(resolvedDate.getDate()).toBe(31);
      expect(resolvedDate.getHours()).toBe(23);
    });
  });

  // ==========================================================================
  // HOMOLOGAÇÃO GERAL: Comando Completo Multi-Token
  // ==========================================================================
  describe('Comando Tático Completo (Multi-Token Extraction)', () => {
    it('deve extrair todos os tokens simultaneamente e deixar o título imaculado', () => {
      const input = 'Criar suíte de testes de integração @90m !3 #backend ^amanha /t';
      const result = parseQuickCapture(input);

      expect(result.title).toBe('Criar suíte de testes de integração');
      expect(result.type).toBe('TASK');
      expect(result.estimatedDurationMinutes).toBe(90);
      expect(result.energyRequired).toBe(3);
      expect(result.projectQuery).toBe('backend');
      expect(result.deadlineIso).not.toBeNull();
      
      // Checa se os rawTokens guardaram as strings exatas para o highlight no input
      expect(result.rawTokens).toEqual({
        type: '/t',
        time: '@90m',
        energy: '!3',
        project: '#backend',
        date: '^amanha'
      });
    });
  });
});