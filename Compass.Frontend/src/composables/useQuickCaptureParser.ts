import { type CommitmentType } from '@/types/index';

export interface ParsedCaptureResult {
  cleanTitle: string;
  type: CommitmentType;
  energyRequired: number; // 1 | 2 | 3
  estimatedDurationMinutes: number;
  projectToken: string | null;
  explicitType: boolean;
}

export function useQuickCaptureParser() {
  const parseInput = (
    rawInput: string, 
    fallbackType: CommitmentType = 'TASK',
    fallbackDuration: number = 30,
    fallbackEnergy: number = 2
  ): ParsedCaptureResult => {
    let text = rawInput;
    let type: CommitmentType = fallbackType;
    let energyRequired = fallbackEnergy;
    let estimatedDurationMinutes = fallbackDuration;
    let projectToken: string | null = null;
    let explicitType = false;

    // 1. Extração de Arquétipo (/t, /h, /e, /n)
    const typeMatch = text.match(/(?:^|\s)\/([then])(?:\s|$)/i);
    if (typeMatch) {
      explicitType = true;
      const char = typeMatch[1].toLowerCase();
      if (char === 't') type = 'TASK';
      else if (char === 'h') type = 'HABIT';
      else if (char === 'e') type = 'EVENT';
      else if (char === 'n') type = 'NOTE';
      text = text.replace(typeMatch[0], ' ');
    }

    // 2. Extração de Energia (!1, !2, !3 ou ⚡)
    const energyMatch = text.match(/(?:^|\s)!(1|2|3)(?:\s|$)/);
    if (energyMatch) {
      energyRequired = parseInt(energyMatch[1], 10);
      text = text.replace(energyMatch[0], ' ');
    }

    // 3. Extração de Duração (@15m, @1h, @90m)
    const durationMatch = text.match(/(?:^|\s)@(\d+)(m|h)(?:\s|$)/i);
    if (durationMatch) {
      const val = parseInt(durationMatch[1], 10);
      const unit = durationMatch[2].toLowerCase();
      estimatedDurationMinutes = unit === 'h' ? val * 60 : val;
      text = text.replace(durationMatch[0], ' ');
    }

    // 4. Extração de Projeto (#nome)
    const projectMatch = text.match(/(?:^|\s)#([a-zA-Z0-9_-]+)(?:\s|$)/);
    if (projectMatch) {
      projectToken = projectMatch[1];
      text = text.replace(projectMatch[0], ' ');
    }

    // Limpeza final de espaços redundantes gerados pela substituição
    const cleanTitle = text.replace(/\s+/g, ' ').trim();

    return {
      cleanTitle: cleanTitle || 'Nova Ação Sem Título',
      type,
      energyRequired,
      estimatedDurationMinutes,
      projectToken,
      explicitType
    };
  };

  return {
    parseInput
  };
}