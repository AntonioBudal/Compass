import type { AutocompleteProvider, AutocompleteContext, Suggestion } from '../types';
import { TrieIndex } from '../../trieIndex';

interface HistoryNode {
  signature: string;
  title: string;
  itemType: string;
  projectName: string | null;
  energy: number;
  duration: number;
  frequency: number;
  lastUsedMs: number;
  baseScore: number;
}

export class HistoryProvider implements AutocompleteProvider {
  public triggerChar = null; 
  private trie: TrieIndex;
  private historyMap: Map<string, HistoryNode> = new Map();

  constructor() {
    this.trie = new TrieIndex();
  }

  public syncData(commitments: any[]): void {
    this.trie.clear();
    this.historyMap.clear();

    const now = Date.now();
    const MS_IN_24H = 86400000;
    const MS_IN_7D = 604800000;

    for (const item of commitments) {
      // UPGRADE: Agora aceitamos Tarefas, Hábitos e Eventos de Agenda!
      if (!['TASK', 'HABIT', 'EVENT'].includes(item.type) || !item.title) continue;

      // UPGRADE: Injetamos o item.type na assinatura para não misturar Tarefas com Hábitos de mesmo nome
      const signature = `${item.title.trim().toLowerCase()}|${item.type}|${item.projectName || ''}|${item.energyRequired}|${item.estimatedDurationMinutes}`;
      const itemTime = item.createdAt ? new Date(item.createdAt).getTime() : now;

      const existing = this.historyMap.get(signature);

      if (existing) {
        existing.frequency += 1;
        if (itemTime > existing.lastUsedMs) existing.lastUsedMs = itemTime;
      } else {
        this.historyMap.set(signature, {
          signature,
          title: item.title.trim(),
          itemType: item.type,
          projectName: item.projectName || null,
          energy: item.energyRequired || 2,
          duration: item.estimatedDurationMinutes || 30,
          frequency: 1,
          lastUsedMs: itemTime,
          baseScore: 0
        });
      }
    }

    for (const node of this.historyMap.values()) {
      let score = 0;
      const diff = now - node.lastUsedMs;
      
      if (diff <= MS_IN_24H) score += 50;
      else if (diff <= MS_IN_7D) score += 30;
      else score += 10;

      score += Math.min(node.frequency * 2, 20);
      node.baseScore = score;

      this.trie.insertMultiWord(node.title, { id: node.signature, title: node.title });
    }
  }

  public getSuggestions(query: string, context: AutocompleteContext, limit: number): Suggestion[] {
    if (query.length < 2) return []; 

    const rawResults = this.trie.searchPrefix(query, limit * 3);
    if (rawResults.length === 0) return [];

    const lowerQuery = query.toLowerCase();

    const scoredResults = rawResults.map(payload => {
      const node = this.historyMap.get(payload.id);
      if (!node) return null;

      let dynamicScore = node.baseScore;

      if (context.currentProject && node.projectName && context.currentProject.toLowerCase() === node.projectName.toLowerCase()) {
        dynamicScore += 15;
      }

      if (context.currentEnergy && context.currentEnergy === node.energy) {
        dynamicScore += 5;
      }

      if (node.title.toLowerCase().startsWith(lowerQuery)) {
        dynamicScore += 10;
      }

      // UPGRADE: Descobre a flag final baseada no arquétipo original do banco
      let typeFlag = '/t';
      if (node.itemType === 'HABIT') typeFlag = '/h';
      else if (node.itemType === 'EVENT') typeFlag = '/e';

      // Montagem inteligente do Ghost Text
      let template = node.title;
      if (node.projectName) template += ` #${node.projectName.toLowerCase()}`;
      if (node.duration) template += ` @${node.duration}m`;
      if (node.energy) template += ` !${node.energy}`;
      template += ` ${typeFlag}`;

      return {
        id: node.signature,
        label: template,
        insertText: template,
        type: 'history' as const,
        score: dynamicScore,
        htmlHighlight: this.generateHighlight(template, query)
      };
    }).filter(Boolean) as Suggestion[];

    return scoredResults.sort((a, b) => b.score - a.score).slice(0, limit);
  }

  private generateHighlight(label: string, query: string): string {
    const safeQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(`(${safeQuery})`, 'gi');
    return label.replace(regex, '<b>$1</b>');
  }
}

export const GlobalHistoryProvider = new HistoryProvider();