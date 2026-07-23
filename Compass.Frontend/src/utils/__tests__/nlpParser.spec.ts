import { describe, it, expect } from 'vitest';
import { parseCommitmentNLP } from '@/utils/nlpParser';

describe('NLP Parser — Extração Tática de Tokens', () => {
  it('deve extrair tempo em minutos, energia e projeto em uma única string contínua', () => {
    const input = 'Refatorar módulo de relatórios @45m !3 #backend';
    const result = parseCommitmentNLP(input);

    expect(result.cleanTitle).toBe('Refatorar módulo de relatórios');
    expect(result.durationMinutes).toBe(45);
    expect(result.energy).toBe(3);
    expect(result.project).toBe('backend');
  });

  it('deve converter horas para minutos corretamente com o token @2h', () => {
    const input = 'Reunião de Arquitetura @2h !2';
    const result = parseCommitmentNLP(input);

    expect(result.cleanTitle).toBe('Reunião de Arquitetura');
    expect(result.durationMinutes).toBe(120); // 2 horas = 120 min
    expect(result.energy).toBe(2);
    expect(result.project).toBeUndefined();
  });

  it('deve manter o texto intacto se nenhum token tático for encontrado', () => {
    const input = 'Lembrete simples sem modificadores';
    const result = parseCommitmentNLP(input);

    expect(result.cleanTitle).toBe('Lembrete simples sem modificadores');
    expect(result.durationMinutes).toBeUndefined();
    expect(result.energy).toBeUndefined();
  });
});