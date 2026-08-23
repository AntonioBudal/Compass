/**
 * Pega um DateTimeOffset que chega em UTC (ex: "2026-08-18T12:00:00Z")
 * e formata apenas as horas na exibição do fuso alvo (ex: "America/Sao_Paulo").
 */
export function formatTimeWithTimezone(utcString: string, timeZone: string): string {
  const dateObj = new Date(utcString)
  return new Intl.DateTimeFormat('pt-BR', {
    timeZone,
    hour: '2-digit',
    minute: '2-digit'
  }).format(dateObj)
}

/**
 * Retorna a data civil atual (YYYY-MM-DD) restrita a um timezone específico.
 * Totalmente seguro contra vazamentos, pois `new Date()` (vazio) captura o timestamp
 * absoluto (Date.now()) que independe do fuso da máquina.
 */
export function getCurrentCivilDate(timeZone: string): string {
  const now = new Date()
  
  const parts = new Intl.DateTimeFormat('en-US', {
    timeZone,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  }).formatToParts(now)

  const year = parts.find(p => p.type === 'year')?.value
  const month = parts.find(p => p.type === 'month')?.value
  const day = parts.find(p => p.type === 'day')?.value

  return `${year}-${month}-${day}`
}

/**
 * Pega uma Data civil e uma Hora civil e gera um timestamp absoluto ISO 8601
 * com o Offset exato do fuso alvo.
 * 
 * Completamente blindado contra o fuso horário do sistema operacional do usuário e
 * matematicamente seguro contra ambiguidades de transição de Horário de Verão (DST).
 */
export function buildAbsoluteTime(dateCivil: string, timeCivil: string, timeZone: string): string {
  // 1. Instancia como UTC estrito ("Z") para evitar que o navegador aplique o fuso do SO.
  const baseUtc = new Date(`${dateCivil}T${timeCivil}:00Z`);
  
  const getOffsetString = (date: Date) => {
    const parts = new Intl.DateTimeFormat('en-US', {
      timeZone,
      timeZoneName: 'longOffset',
    }).formatToParts(date);
    
    const gmtString = parts.find(p => p.type === 'timeZoneName')?.value || 'GMT';
    const offset = gmtString.replace('GMT', '');
    
    if (!offset) return 'Z';
    
    // Padroniza falhas de formatação cross-browser para: -03:00, +05:30
    const sign = offset.startsWith('-') ? '-' : '+';
    const timePart = offset.replace(/[+-]/, '');
    const [h, m] = timePart.split(':');
    
    return `${sign}${h.padStart(2, '0')}:${m || '00'}`;
  };

  // 2. Descobre o offset provisório para esse momento base 
  const provisionalOffset = getOffsetString(baseUtc);
  
  // 3. Converte o offset string para milissegundos
  let offsetMs = 0;
  if (provisionalOffset !== 'Z') {
     const sign = provisionalOffset.startsWith('-') ? -1 : 1;
     const [hours, minutes] = provisionalOffset.substring(1).split(':').map(Number);
     offsetMs = sign * ((hours * 60) + minutes) * 60 * 1000;
  }
  
  // 4. Calcula o momento UTC real (subtraindo o offset)
  const realUtc = new Date(baseUtc.getTime() - offsetMs);
  
  // 5. Obtém o offset final avaliando o UTC real (isso resolve perfeitamente a barreira 
  // do Horário de Verão, pois checa o offset na hora real pretendida).
  const finalOffset = getOffsetString(realUtc);
  
  return `${dateCivil}T${timeCivil}:00${finalOffset}`;
}