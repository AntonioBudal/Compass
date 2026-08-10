import type { AutocompleteProvider, AutocompleteContext, Suggestion, SuggestionType } from '../types';
import { TrieIndex, type TriePayload } from '../../trieIndex';

export class DynamicTrieProvider implements AutocompleteProvider {
  private trie: TrieIndex;

  constructor(
    public triggerChar: string,
    private suggestionType: SuggestionType,
    private baseScore: number = 40
  ) {
    this.trie = new TrieIndex();
  }

  /**
   * [SINK DE DADOS O(N)]
   * Chamado em background pela Store sempre que o catálogo sofrer mutação.
   * Reconstrói a árvore de prefixos (N-Gramas) para buscas instantâneas.
   */
  public syncData(items: { id: string; name: string; lastUsedAtUtc?: string | null }[]): void {
    this.trie.clear();
    for (const item of items) {
      this.trie.insertMultiWord(item.name, { 
        id: item.id, 
        title: item.name, 
        lastUsedAtUtc: item.lastUsedAtUtc 
      });
    }
  }

  /**
   * [PIPELINE DE BUSCA O(L)]
   * Onde L é o tamanho da string digitada.
   */
  public getSuggestions(query: string, context: AutocompleteContext, limit: number): Suggestion[] {
    // 1. Busca profunda na Trie (Limitamos x2 para sobrar margem pro Scoring)
    const rawResults = this.trie.searchPrefix(query, limit * 2);

    if (rawResults.length === 0) return [];

    const now = Date.now();
    const MS_IN_24H = 86400000;
    const MS_IN_7D = 604800000;

    // 2. Mapeamento, Scoring e Highlight
    return rawResults.map(payload => {
      let score = this.baseScore;

      // --- HEURÍSTICA DE PONTUAÇÃO ---
      // A. Bônus de Recência (LRU)
      if (payload.lastUsedAtUtc) {
        const lastUsedMs = new Date(payload.lastUsedAtUtc).getTime();
        const diff = now - lastUsedMs;
        
        if (diff <= MS_IN_24H) score += 30;      // Usado hoje: Super Bônus
        else if (diff <= MS_IN_7D) score += 15;  // Usado esta semana: Bônus
        else score += 5;                         // Usado em algum momento
      }

      // B. Bônus de Prefixo Exato (Ignora espaços)
      const isExactPrefix = payload.title.toLowerCase().startsWith(query.toLowerCase());
      if (isExactPrefix) score += 20;

      // 3. Empacotamento do Contrato
      return {
        id: payload.id,
        label: `${this.triggerChar}${payload.title}`, // Ex: "#Backend"
        insertText: payload.title,                    // Ex: "Backend"
        type: this.suggestionType,
        score,
        htmlHighlight: this.generateHighlight(`${this.triggerChar}${payload.title}`, query)
      };
    });
  }

  /**
   * Engine de Highlight Seguro
   * Escapa caracteres sensíveis da regex e embrulha o match em <b>
   */
  private generateHighlight(label: string, query: string): string {
    if (!query) return label;
    // Escapa a query para evitar que o usuário quebre a regex digitando "[.*+"
    const safeQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(`(${safeQuery})`, 'gi');
    return label.replace(regex, '<b>$1</b>');
  }
}