// ============================================================================
// COMPASS — NLP PARSER v2.0 (QUICK CAPTURE ENGINE)
// Tipagem Estrita, Roteamento de Arquétipos e Matemática Temporal Determinística
// ============================================================================

export type CommitmentType = 'TASK' | 'HABIT' | 'EVENT' | 'NOTE';

export interface ParsedNLP {
  title: string;
  type: CommitmentType;
  estimatedDurationMinutes: number;
  energyRequired: number;
  projectQuery: string | null;
  deadlineIso: string | null; // Formato UTC ISO-8601 preservando o limite 23:59:59 local
  rawTokens: {
    type: string | null;
    time: string | null;
    energy: string | null;
    project: string | null;
    date: string | null;
  };
}

/**
 * Resolve tokens temporais iniciados por '^' para o limite do dia (23:59:59.999 local)
 * e exporta o timestamp em formato UTC ISO-8601 perfeito para a API .NET 10.
 */
function resolveTemporalToken(token: string): string | null {
  const clean = token.toLowerCase().trim();
  const now = new Date();
  
  // Helper para cravar 23:59:59.999 no fuso local e retornar ISO UTC
  const toLocalEndOfDayIso = (targetDate: Date): string => {
    targetDate.setHours(23, 59, 59, 999);
    return targetDate.toISOString();
  };

  // 1. Relativos Imediatos
  if (clean === '^hoje' || clean === '^today' || clean === '^hj') {
    return toLocalEndOfDayIso(now);
  }
  if (clean === '^amanha' || clean === '^tomorrow' || clean === '^am') {
    const tomorrow = new Date(now);
    tomorrow.setDate(tomorrow.getDate() + 1);
    return toLocalEndOfDayIso(tomorrow);
  }

  // 2. Dias da Semana (^seg, ^ter, ^qua, ^qui, ^sex, ^sab, ^dom)
  const daysMap: Record<string, number> = {
    '^dom': 0, '^seg': 1, '^ter': 2, '^qua': 3, '^qui': 4, '^sex': 5, '^sab': 6
  };
  
  if (clean in daysMap) {
    const targetDayOfWeek = daysMap[clean];
    const currentDayOfWeek = now.getDay();
    let daysToAdd = targetDayOfWeek - currentDayOfWeek;
    
    // Se o dia alvo já passou nesta semana (ou é hoje), projeta para a próxima semana (+7 dias)
    // Ex: Digitar ^sex em uma sexta-feira projeta para a sexta que vem (se quiser hoje, usa ^hoje)
    if (daysToAdd <= 0) {
      daysToAdd += 7;
    }
    
    const targetDate = new Date(now);
    targetDate.setDate(targetDate.getDate() + daysToAdd);
    return toLocalEndOfDayIso(targetDate);
  }

  // 3. Relativos Numéricos (^3d, ^1w)
  const relativeMatch = clean.match(/^\^(\d+)([dw])$/);
  if (relativeMatch) {
    const amount = parseInt(relativeMatch[1], 10);
    const unit = relativeMatch[2];
    const targetDate = new Date(now);
    
    if (unit === 'd') targetDate.setDate(targetDate.getDate() + amount);
    if (unit === 'w') targetDate.setDate(targetDate.getDate() + (amount * 7));
    
    return toLocalEndOfDayIso(targetDate);
  }

  // 4. Datas Absolutas (^YYYY-MM-DD ou ^DD/MM)
  const isoMatch = clean.match(/^\^(\d{4})-(\d{2})-(\d{2})$/);
  if (isoMatch) {
    const year = parseInt(isoMatch[1], 10);
    const monthIndex = parseInt(isoMatch[2], 10) - 1; // JS months são 0-indexed
    const day = parseInt(isoMatch[3], 10);
    
    // Construtor nativo (ano, mes, dia, hora, min, sec, ms) no fuso LOCAL
    const absoluteDate = new Date(year, monthIndex, day, 23, 59, 59, 999);
    return absoluteDate.toISOString();
  }

  const shortMatch = clean.match(/^\^(\d{2})\/(\d{2})$/);
  if (shortMatch) {
    const day = parseInt(shortMatch[1], 10);
    const monthIndex = parseInt(shortMatch[2], 10) - 1;
    const year = now.getFullYear();
    
    const absoluteDate = new Date(year, monthIndex, day, 23, 59, 59, 999);
    // Se a data curta já passou no ano corrente, projeta para o ano que vem
    if (absoluteDate.getTime() < now.getTime()) {
      absoluteDate.setFullYear(year + 1);
    }
    return absoluteDate.toISOString();
  }

  return null;
}

/**
 * Analisa a string bruta de entrada, extrai tokens táticos e retorna o payload limpo.
 * Exemplo: "Revisar PR do motor tático @45m !3 #core ^amanha /t"
 */
export function parseQuickCapture(input: string): ParsedNLP {
  let workingText = ` ${input} `; // Padding para simplificar regex de borda de palavra

  // --- 1. Extração de Arquétipo (/t, /h, /e, /n) ---
  let type: CommitmentType = 'TASK';
  let rawType: string | null = null;
  const typeRegex = /\s\/([thentaskhabitoeventonota]+)\b/i;
  const typeMatch = workingText.match(typeRegex);
  
  if (typeMatch) {
    rawType = typeMatch[0].trim();
    const flag = typeMatch[1].toLowerCase();
    
    if (flag.startsWith('h')) type = 'HABIT';
    else if (flag.startsWith('e')) type = 'EVENT';
    else if (flag.startsWith('n')) type = 'NOTE';
    else type = 'TASK';

    workingText = workingText.replace(typeMatch[0], ' ');
  }

  // --- 2. Extração de Duração (@30m, @1h, @90) ---
  let duration = 30; // Fallback padrão de 30 minutos
  let rawTime: string | null = null;
  const timeRegex = /\s@(\d+)(m|h|min|minutos|horas)?\b/i;
  const timeMatch = workingText.match(timeRegex);

  if (timeMatch) {
    rawTime = timeMatch[0].trim();
    const val = parseInt(timeMatch[1], 10);
    const unit = timeMatch[2]?.toLowerCase();

    if (unit === 'h' || unit === 'horas') duration = val * 60;
    else duration = val;

    workingText = workingText.replace(timeMatch[0], ' ');
  }

  // --- 3. Extração de Energia (!1, !2, !3) ---
  let energy = 2; // Fallback padrão operacional (!2)
  let rawEnergy: string | null = null;
  const energyRegex = /\s!([123])\b/;
  const energyMatch = workingText.match(energyRegex);

  if (energyMatch) {
    rawEnergy = energyMatch[0].trim();
    energy = parseInt(energyMatch[1], 10);
    workingText = workingText.replace(energyMatch[0], ' ');
  }

  // --- 4. Extração de Projeto (#nome do projeto composto) ---
  let projectQuery: string | null = null;
  let rawProject: string | null = null;
  
  // 🔥 ARQ: Regex Gananciosa. Lê o '#' e captura TUDO até encontrar outro gatilho (@, !, ^, /) ou o fim da linha.
  // Isso permite nomes com espaços, acentos, hifens e números (Ex: #Integração C# e Vue.js).
  const projectRegex = /\s#([^@!\^\/]+)/;
  const projectMatch = workingText.match(projectRegex);

  if (projectMatch) {
    rawProject = projectMatch[0];
    projectQuery = projectMatch[1].trim(); // Remove espaços residuais do final
    workingText = workingText.replace(projectMatch[0], ' ');
  }

  // --- 5. Extração de Data/Prazo (^hoje, ^amanha, ^YYYY-MM-DD) ---
  let deadlineIso: string | null = null;
  let rawDate: string | null = null;
  const dateRegex = /\s\^([a-zA-Z0-9_-]+|\d{2}\/\d{2})\b/;
  const dateMatch = workingText.match(dateRegex);

  if (dateMatch) {
    rawDate = dateMatch[0].trim();
    deadlineIso = resolveTemporalToken(rawDate);
    workingText = workingText.replace(dateMatch[0], ' ');
  }

  // --- 6. Limpeza da String Restante (Título Puro) ---
  const cleanTitle = workingText
    .replace(/\s+/g, ' ') // Remove espaços duplos
    .trim();

  return {
    title: cleanTitle || 'Nova Captura Sem Título',
    type,
    estimatedDurationMinutes: duration,
    energyRequired: energy,
    projectQuery,
    deadlineIso,
    rawTokens: {
      type: rawType,
      time: rawTime,
      energy: rawEnergy,
      project: rawProject,
      date: rawDate
    }
  };
}