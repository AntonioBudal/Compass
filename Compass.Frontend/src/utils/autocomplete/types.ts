export type SuggestionType = 'history' | 'project' | 'goal' | 'command' | 'dictionary';

export interface Suggestion {
  id?: string;
  label: string;           
  insertText: string;      
  type: SuggestionType;    
  score: number;           
  confidence?: number;     // NOVO: Grau de certeza de 0.0 a 1.0
  htmlHighlight?: string;  
  commitCharacters?: string[]; // NOVO: Teclas que engatilham a predição (Ex: ['Tab', 'ArrowRight'])
}

export interface AutocompleteContext {
  currentProject: string | null;
  currentEnergy: number | null;
  currentTime: string | null;
  currentDate: string | null;
  currentType: string | null;
}

export interface AutocompleteProvider {
  triggerChar: string | null; 
  getSuggestions(query: string, context: AutocompleteContext, limit: number): Suggestion[];
}

export interface GhostPrediction {
  suggestion: Suggestion;
  ghostSuffix: string; // Exatamente o que a UI precisa renderizar em cinza
}