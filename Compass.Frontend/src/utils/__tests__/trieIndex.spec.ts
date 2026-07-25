import { describe, it, expect, beforeEach } from 'vitest';
import { TrieIndex, type TriePayload } from '../trieIndex';

describe('TrieIndex Engine — Indexação Prefixada Ultrarrápida in-RAM', () => {
  let trie: TrieIndex;

  const sampleProjects: TriePayload[] = [
    { id: '1', title: 'Compass Core Engine', lastUsedAtUtc: '2026-07-24T10:00:00Z' },
    { id: '2', title: 'Compass UI Refactor', lastUsedAtUtc: '2026-07-25T12:00:00Z' },
    { id: '3', title: 'Engenharia de Software (SWEBOK)', lastUsedAtUtc: '2026-07-20T08:00:00Z' },
    { id: '4', title: 'Infraestrutura PostgreSQL', lastUsedAtUtc: '2026-07-22T15:00:00Z' },
    { id: '5', title: 'Compiladores & LLVM', lastUsedAtUtc: null }
  ];

  beforeEach(() => {
    trie = new TrieIndex();
    sampleProjects.forEach(p => trie.insertMultiWord(p.title, p));
  });

  it('deve encontrar itens por prefixo exato e case-insensitive', () => {
    const results = trie.searchPrefix('comp');
    
    // Deve encontrar Compass Core, Compass UI e Compiladores
    expect(results.length).toBe(3);
    expect(results.map(r => r.id)).toContain('1');
    expect(results.map(r => r.id)).toContain('2');
    expect(results.map(r => r.id)).toContain('5');
  });

  it('deve encontrar itens por palavras internas (Multi-Word Indexing)', () => {
    // Digitou "#soft" deve encontrar "Engenharia de Software"
    const results = trie.searchPrefix('soft');
    expect(results.length).toBe(1);
    expect(results[0].title).toBe('Engenharia de Software (SWEBOK)');
  });

  it('deve respeitar a ordenação LRU (Mais recente primeiro)', () => {
    const results = trie.searchPrefix('compass');
    expect(results.length).toBe(2);
    // O ID '2' tem carimbo do dia 25, o ID '1' do dia 24. O ID '2' deve vir em 1º!
    expect(results[0].id).toBe('2');
    expect(results[1].id).toBe('1');
  });

  it('deve retornar array vazio para prefixo inexistente ou em branco', () => {
    expect(trie.searchPrefix('xyz')).toEqual([]);
    expect(trie.searchPrefix('   ')).toEqual([]);
  });

  it('deve executar buscas em menos de 1 milissegundo (< 1ms)', () => {
    // Injeta 1.000 projetos sintéticos para teste de estresse
    for (let i = 100; i < 1100; i++) {
      trie.insert(`Projeto de Teste Carga ${i}`, { id: `id-${i}`, title: `Projeto de Teste Carga ${i}` });
    }

    const start = performance.now();
    const results = trie.searchPrefix('proj');
    const end = performance.now();
    const durationMs = end - start;

    expect(results.length).toBeLessThanOrEqual(8); // Limite padrão de 8 itens
    expect(durationMs).toBeLessThan(1.0); // Garante SLA sub-milissegundo!
  });
});