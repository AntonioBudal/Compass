// ============================================================================
// COMPASS — TRIE INDEX ENGINE v1.0
// Estrutura de Dados em RAM para Indexação Prefixada Ultrarrápida (O(L))
// ============================================================================

export interface TriePayload {
  id: string;
  title: string;
  lastUsedAtUtc?: string | null;
  [key: string]: any;
}

class TrieNode {
  children: Map<string, TrieNode> = new Map();
  isEndOfWord: boolean = false;
  payloads: TriePayload[] = [];
}

export class TrieIndex {
  private root: TrieNode;
  private totalWords: number;

  constructor() {
    this.root = new TrieNode();
    this.totalWords = 0;
  }

  /**
   * Limpa a árvore inteira para re-hidratação de catálogo.
   */
  public clear(): void {
    this.root = new TrieNode();
    this.totalWords = 0;
  }

  /**
   * Insere um item na árvore indexando por palavras ou termo completo.
   * Complexidade: O(L), onde L é o comprimento da chave digitada.
   */
  public insert(key: string, payload: TriePayload): void {
    if (!key || !key.trim()) return;

    const normalizedKey = key.toLowerCase().trim();
    let currentNode = this.root;

    for (let i = 0; i < normalizedKey.length; i++) {
      const char = normalizedKey[i];
      if (!currentNode.children.has(char)) {
        currentNode.children.set(char, new TrieNode());
      }
      currentNode = currentNode.children.get(char)!;
    }

    currentNode.isEndOfWord = true;
    // Evita duplicatas exatas de ID no mesmo prefixo folha
    if (!currentNode.payloads.some(p => p.id === payload.id)) {
      currentNode.payloads.push(payload);
      this.totalWords++;
    }
  }

  /**
   * Insere todas as palavras individuais de um título como prefixos válidos.
   * Ex: "Engenharia de Software" indexa em "engenharia", "de", "software".
   */
  public insertMultiWord(title: string, payload: TriePayload): void {
    this.insert(title, payload); // Insere o título completo
    
    const words = title.split(/\s+/);
    if (words.length > 1) {
      for (const word of words) {
        if (word.length >= 2) { // Ignora preposições de 1 caractere
          this.insert(word, payload);
        }
      }
    }
  }

  /**
   * Busca todas as cargas úteis que começam com o prefixo fornecido.
   * Complexidade de busca: O(L + K), onde K é o número de nós filhos coletados.
   */
  public searchPrefix(prefix: string, limit: number = 8): TriePayload[] {
    if (!prefix || !prefix.trim()) return [];

    const normalizedPrefix = prefix.toLowerCase().trim();
    let currentNode = this.root;

    // 1. Navega até o nó final do prefixo
    for (let i = 0; i < normalizedPrefix.length; i++) {
      const char = normalizedPrefix[i];
      if (!currentNode.children.has(char)) {
        return []; // Prefixo não existe na árvore
      }
      currentNode = currentNode.children.get(char)!;
    }

    // 2. Coleta em profundidade (DFS) todos os nós abaixo do prefixo
    const resultsMap = new Map<string, TriePayload>();
    this.collectPayloads(currentNode, resultsMap, limit);

    const results = Array.from(resultsMap.values());

    // 3. Ordenação temporal LRU (Mais recentes primeiro)
    return results.sort((a, b) => {
      const timeA = a.lastUsedAtUtc ? new Date(a.lastUsedAtUtc).getTime() : 0;
      const timeB = b.lastUsedAtUtc ? new Date(b.lastUsedAtUtc).getTime() : 0;
      return timeB - timeA;
    }).slice(0, limit);
  }

  private collectPayloads(node: TrieNode, resultsMap: Map<string, TriePayload>, limit: number): void {
    if (resultsMap.size >= limit * 2) return; // Limite de poda de busca para performance

    if (node.isEndOfWord) {
      for (const payload of node.payloads) {
        if (!resultsMap.has(payload.id)) {
          resultsMap.set(payload.id, payload);
        }
      }
    }

    for (const childNode of node.children.values()) {
      this.collectPayloads(childNode, resultsMap, limit);
    }
  }

  public size(): number {
    return this.totalWords;
  }
}