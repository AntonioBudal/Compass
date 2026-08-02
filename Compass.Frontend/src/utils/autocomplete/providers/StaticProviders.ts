import type { AutocompleteProvider, AutocompleteContext, Suggestion } from '../types';
import { omniEngine } from '../AutocompleteEngine';

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
        // Bônus de Prefixo Exato (+20)
        if (item.insertText.toLowerCase().startsWith(lowerQuery)) score += 20;

        return {
          ...item,
          type: 'command',
          score,
          htmlHighlight: this.generateHighlight(item.label, query)
        };
      });
  }

  /**
   * Envelopa a substring correspondente em tags <b> para a renderização da UI
   */
  private generateHighlight(label: string, query: string): string {
    if (!query) return label;
    const regex = new RegExp(`(${query})`, 'gi');
    return label.replace(regex, '<b>$1</b>');
  }
}

// ============================================================================
// REGISTRO DE PROVIDERS ESTÁTICOS DE ALTA PERFORMANCE
// ============================================================================

const TypeProvider = new StaticCommandProvider('/', [
  { label: '/t — Tarefa Operacional', insertText: 't' },
  { label: '/h — Hábito ou Rotina', insertText: 'h' },
  { label: '/e — Evento ou Reunião', insertText: 'e' },
  { label: '/n — Nota / Captura Rápida', insertText: 'n' }
]);

const TimeProvider = new StaticCommandProvider('@', [
  { label: '@15m — Sprint Curta (15 min)', insertText: '15m' },
  { label: '@30m — Turno Padrão (30 min)', insertText: '30m' },
  { label: '@45m — Foco Intenso (45 min)', insertText: '45m' },
  { label: '@1h — Bloco Profundo (60 min)', insertText: '1h' },
  { label: '@2h — Imersão Total (120 min)', insertText: '2h' }
]);

const DateProvider = new StaticCommandProvider('^', [
  { label: '^hoje — Limite às 23:59 de hoje', insertText: 'hoje' },
  { label: '^amanha — Limite às 23:59 de amanhã', insertText: 'amanha' },
  { label: '^seg — Próxima Segunda-feira', insertText: 'seg' },
  { label: '^sex — Próxima Sexta-feira', insertText: 'sex' }
]);

const EnergyProvider = new StaticCommandProvider('!', [
  { label: '!1 — Manutenção (Baixa Cognição)', insertText: '1' },
  { label: '!2 — Operacional (Média Cognição)', insertText: '2' },
  { label: '!3 — Deep Work (Alta Cognição)', insertText: '3' }
]);

// Injeção de Dependência no Singleton
omniEngine.registerProvider(TypeProvider);
omniEngine.registerProvider(TimeProvider);
omniEngine.registerProvider(DateProvider);
omniEngine.registerProvider(EnergyProvider);