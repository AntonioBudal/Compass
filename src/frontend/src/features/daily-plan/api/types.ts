// Espelha o DailyPlanSuggestionDto do backend
export interface DailyPlanSuggestion {
  referenceId: string;
  type: string;
  title: string;
  start: string;
  end: string;
}

// Espelha o DailyPlanResponseDto do backend (Retorno do GET)
export interface DailyPlanPreview {
  date: string;
  suggestions: DailyPlanSuggestion[];
}

export interface RecordExecutionRequest {
  referenceId: string | null
  start: string
  end: string
  type: string
}

// Corpo da requisição para aceitar o plano (POST)
export interface AcceptDailyPlanRequest {
  profileId: string;
  date: string;
}

// Resposta esperada de sucesso ao aceitar o plano
export interface AcceptDailyPlanResponse {
  dailyPlanId: string;
}