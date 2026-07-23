export interface ParsedNLP {
  cleanTitle: string;
  durationMinutes?: number;
  energy?: number;
  project?: string;
}

export function parseCommitmentNLP(input: string): ParsedNLP {
  let cleanTitle = input;
  let durationMinutes: number | undefined;
  let energy: number | undefined;
  let project: string | undefined;

  // Extrai duração (ex: @30m ou @1h)
  const timeMatch = input.match(/@(\d+)(m|h)/i);
  if (timeMatch) {
    const val = parseInt(timeMatch[1], 10);
    durationMinutes = timeMatch[2].toLowerCase() === 'h' ? val * 60 : val;
    cleanTitle = cleanTitle.replace(timeMatch[0], '').trim();
  }

  // Extrai energia (ex: !1, !2, !3)
  const energyMatch = input.match(/!([1-3])/);
  if (energyMatch) {
    energy = parseInt(energyMatch[1], 10);
    cleanTitle = cleanTitle.replace(energyMatch[0], '').trim();
  }

  // Extrai projeto (ex: #dev, #design)
  const projectMatch = input.match(/#(\w+)/);
  if (projectMatch) {
    project = projectMatch[1];
    cleanTitle = cleanTitle.replace(projectMatch[0], '').trim();
  }

  // Limpa espaços duplos remanescentes
  cleanTitle = cleanTitle.replace(/\s+/g, ' ').trim();

  return { cleanTitle, durationMinutes, energy, project };
}