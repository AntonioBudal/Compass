import { z } from 'zod';

// Esquema Zod focado apenas no payload de edição, não na exportação global
export const DraftCommitmentSchema = z.object({
  id: z.string().uuid(),
  title: z.string().trim().min(1, 'O título não pode ficar em branco.'),
  type: z.enum(['TASK', 'HABIT', 'EVENT', 'NOTE']),
  estimatedDurationMinutes: z.number().int().nonnegative().optional(),
  energyRequired: z.number().int().min(1).max(3).optional(),
  projectId: z.string().uuid().nullable().optional(),
  // Validações específicas por tipo
  cronExpression: z.string().nullable().optional(),
  startTime: z.string().nullable().optional(),
  endTime: z.string().nullable().optional()
}).superRefine((data, ctx) => {
  // UX DEFENSIVA: Validação de Eventos - Não pode terminar antes de começar
  if (data.type === 'EVENT' && data.startTime && data.endTime) {
    if (new Date(data.endTime).getTime() <= new Date(data.startTime).getTime()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'O término do evento não pode ser anterior ao seu início.',
        path: ['endTime']
      });
    }
  }
});