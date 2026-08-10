import type { AutocompleteContext, AutocompleteProvider, Suggestion, GhostPrediction } from './types';
import { DynamicTrieProvider } from './providers/DynamicTrieProvider';
import { GlobalHistoryProvider } from './providers/HistoryProvider';

// ============================================================================
// 1. CLASSE BASE DOS PROVIDERS ESTÁTICOS
// ============================================================================
class StaticCommandProvider implements AutocompleteProvider {
  constructor(
    public triggerChar: string,
    private items: Omit<Suggestion, 'score' | 'htmlHighlight' | 'type'>[],
    private baseScore: number = 50
  ) {}

  public getSuggestions(query: string, context: AutocompleteContext, limit: number): Suggestion[] {
    const lowerQuery = query.toLowerCase();
    
    return this.items
      .filter(item => 
        item.insertText.toLowerCase().includes(lowerQuery) || 
        item.label.toLowerCase().includes(lowerQuery)
      )
      .map(item => {
        let score = this.baseScore;
        if (item.insertText.toLowerCase().startsWith(lowerQuery)) score += 20;

        return {
          ...item,
          type: 'command',
          score,
          htmlHighlight: this.generateHighlight(item.label, query)
        };
      });
  }

  private generateHighlight(label: string, query: string): string {
    if (!query) return label;
    const safeQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(`(${safeQuery})`, 'gi');
    return label.replace(regex, '<b>$1</b>');
  }
}

// ============================================================================
// 2. ORQUESTrador CORE (AUTOCOMPLETE ENGINE)
// ============================================================================
export class AutocompleteEngine {
  private providers: Map<string | null, AutocompleteProvider> = new Map();
  private maxResults: number;

  constructor(maxResults = 8) {
    this.maxResults = maxResults;
  }

  public registerProvider(provider: AutocompleteProvider): void {
    this.providers.set(provider.triggerChar, provider);
  }

  public parseContext(fullText: string): AutocompleteContext {
    return {
      currentProject: fullText.match(/(?:^|\s)#([a-zA-Z0-9_-]+)/)?.[1] || null,
      currentEnergy: parseInt(fullText.match(/(?:^|\s)!([123])/)?.[1] || '0', 10) || null,
      currentTime: fullText.match(/(?:^|\s)@([a-zA-Z0-9_-]+)/)?.[1] || null,
      currentDate: fullText.match(/(?:^|\s)\^([a-zA-Z0-9_\-\/]+)/)?.[1] || null,
      currentType: fullText.match(/(?:^|\s)\/([a-zA-Z]+)/)?.[1] || null,
    };
  }

  public resolveActiveQuery(fullText: string, cursorPosition: number) {
    const textBeforeCursor = fullText.slice(0, cursorPosition);
    const match = textBeforeCursor.match(/(?:^|\s)([\#\/@\^\!])?([a-zA-Z0-9_\-\/]*)$/);

    // FIX DA AUDITORIA: Adicionamos a checagem se não é um monte de espaço vazio
    if (!match || (!match[1] && match[2].trim() === '')) {
      return { isActive: false, trigger: null, query: '', wordStart: 0, wordEnd: cursorPosition };
    }

    const trigger = match[1] || null;
    const query = match[2] || '';
    
    const matchIndex = match.index === undefined ? 0 : match.index;
    const wordStart = match[0].startsWith(' ') ? matchIndex + 1 : matchIndex;

    return { isActive: true, trigger, query, wordStart, wordEnd: cursorPosition };
  }

  public getSuggestions(fullText: string, cursorPosition: number): Suggestion[] {
    const activeState = this.resolveActiveQuery(fullText, cursorPosition);
    if (!activeState.isActive) return [];

    const provider = this.providers.get(activeState.trigger);
    if (!provider) return [];

    const context = this.parseContext(fullText);
    const results = provider.getSuggestions(activeState.query, context, this.maxResults);
    
    return results.sort((a, b) => b.score - a.score).slice(0, this.maxResults);
  }

  public getGhostPrediction(fullText: string, cursorPosition: number): GhostPrediction | null {
    const activeState = this.resolveActiveQuery(fullText, cursorPosition);
    if (!activeState.isActive) return null;

    const provider = this.providers.get(activeState.trigger);
    if (!provider) return null;

    const context = this.parseContext(fullText);
    const results = provider.getSuggestions(activeState.query, context, 1);
    
    if (results.length === 0) return null;

    const best = results[0];
    const queryLower = activeState.query.toLowerCase();
    const insertLower = best.insertText.toLowerCase();

    let confidence = 0.0;

    // A regra fundamental do Ghost Text: A inserção DEVE começar com o que o usuário digitou.
    if (insertLower.startsWith(queryLower)) {
      // 1. Confiança Base (Match Exato de Prefixo)
      confidence = 0.80; 

      // 2. Redução de Entropia (Se já digitou 2+ letras, a intenção é muito mais clara)
      if (queryLower.length >= 2) confidence += 0.10; 
      
      // 3. Intenção Explícita (Se digitou um gatilho como '@' ou '/', a intenção já é óbvia mesmo sem letras)
      if (activeState.trigger !== null && queryLower.length === 0) confidence += 0.15; 

      // 4. Bônus de Relevância / Recência da Base de Dados
      if (best.score >= 50) confidence += 0.10; 
    }

    // Trava matemática de segurança
    best.confidence = Math.min(confidence, 1.0);

    // Limiar estrito: Só mostra Ghost Text se o cálculo atingiu 85% ou mais de certeza
    if (best.confidence < 0.85) return null;

    return {
      suggestion: best,
      ghostSuffix: best.insertText.substring(queryLower.length)
    };
  }

  public applySuggestion(fullText: string, cursorPosition: number, suggestion: Suggestion) {
    const activeState = this.resolveActiveQuery(fullText, cursorPosition);
    
    const textBefore = fullText.slice(0, activeState.wordStart);
    const textAfter = fullText.slice(activeState.wordEnd);
    const triggerStr = activeState.trigger || '';
    
    const newText = `${textBefore}${triggerStr}${suggestion.insertText} ${textAfter}`;
    const newCursorPosition = textBefore.length + triggerStr.length + suggestion.insertText.length + 1;

    return { newText, newCursorPosition };
  }
}

// ============================================================================
// 3. REGISTRO E INICIALIZAÇÃO DO SINGLETON
// ============================================================================
export const omniEngine = new AutocompleteEngine();

// Registra Provedores Estáticos (Comandos)
omniEngine.registerProvider(new StaticCommandProvider('/', [
  { label: '/t — Tarefa Operacional', insertText: 't' },
  { label: '/h — Hábito ou Rotina', insertText: 'h' },
  { label: '/e — Evento ou Reunião', insertText: 'e' },
  { label: '/n — Nota / Captura Rápida', insertText: 'n' }
]));

omniEngine.registerProvider(new StaticCommandProvider('@', [
  { label: '@15m — Sprint Curta (15 min)', insertText: '15m' },
  { label: '@30m — Turno Padrão (30 min)', insertText: '30m' },
  { label: '@45m — Foco Intenso (45 min)', insertText: '45m' },
  { label: '@1h — Bloco Profundo (60 min)', insertText: '1h' },
  { label: '@2h — Imersão Total (120 min)', insertText: '2h' }
]));

omniEngine.registerProvider(new StaticCommandProvider('^', [
  { label: '^hoje — Limite às 23:59 de hoje', insertText: 'hoje' },
  { label: '^amanha — Limite às 23:59 de amanhã', insertText: 'amanha' },
  { label: '^seg — Próxima Segunda-feira', insertText: 'seg' },
  { label: '^sex — Próxima Sexta-feira', insertText: 'sex' }
]));

omniEngine.registerProvider(new StaticCommandProvider('!', [
  { label: '!1 — Manutenção (Baixa Cognição)', insertText: '1' },
  { label: '!2 — Operacional (Média Cognição)', insertText: '2' },
  { label: '!3 — Deep Work (Alta Cognição)', insertText: '3' }
]));

// Registra Provedores Dinâmicos (Tries e Histórico)
export const ProjectProvider = new DynamicTrieProvider('#', 'project');
export const GoalProvider = new DynamicTrieProvider('+', 'goal');

omniEngine.registerProvider(ProjectProvider);
omniEngine.registerProvider(GoalProvider);
omniEngine.registerProvider(GlobalHistoryProvider);