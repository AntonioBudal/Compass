import { z } from 'zod';

export const SettingExportSchema = z.object({
  defaultEnergyLevel: z.number().int().min(1).max(3),
  theme: z.string().min(1),
  autoPostponeEnabled: z.boolean(),
  dailyReviewTime: z.string().regex(/^\d{2}:\d{2}$/),
  preferencesJson: z.string()
});

export const ScoringProfileExportSchema = z.object({
  sampleCount: z.number().int().nonnegative(),
  eaiMultiplier: z.number().min(0.5).max(3.0),
  morningBias: z.number().min(0.1).max(3.0),
  afternoonBias: z.number().min(0.1).max(3.0),
  eveningBias: z.number().min(0.1).max(3.0),
  nightBias: z.number().min(0.1).max(3.0)
});

export const ProjectExportSchema = z.object({
  id: z.string().uuid(),
  title: z.string().min(1),
  status: z.string(),
  totalEstimatedMinutes: z.number().int().nonnegative(),
  lastUsedAt: z.string().nullable().optional()
});

export const CommitmentExportSchema = z.object({
  id: z.string().uuid(),
  title: z.string().min(1),
  type: z.string(),
  status: z.string(),
  estimatedMinutes: z.number().int().nonnegative(),
  energyRequired: z.number().int().min(1).max(3),
  postponedCount: z.number().int().nonnegative(),
  deadline: z.string().nullable().optional(),
  createdAt: z.string(),
  completedAt: z.string().nullable().optional(),
  projectId: z.string().uuid().nullable().optional()
});

export const FocusSessionExportSchema = z.object({
  id: z.string().uuid(),
  commitmentId: z.string().uuid(),
  startTimeUtc: z.string(),
  endTimeUtc: z.string().nullable().optional(),
  actualMinutes: z.number().int().nonnegative()
});

export const DailyReviewExportSchema = z.object({
  id: z.string().uuid(),
  reviewDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
  completedCount: z.number().int().nonnegative(),
  postponedCount: z.number().int().nonnegative(),
  totalFocusMinutes: z.number().int().nonnegative(),
  notes: z.string()
});

// Schema Raiz Autossuficiente
export const PortabilityBundleSchema = z.object({
  exportedAtUtc: z.string(),
  schemaVersion: z.string().min(1),
  userId: z.string().uuid(),
  settings: SettingExportSchema.nullable().optional(),
  adaptiveProfile: ScoringProfileExportSchema.nullable().optional(),
  projects: z.array(ProjectExportSchema),
  commitments: z.array(CommitmentExportSchema),
  focusSessions: z.array(FocusSessionExportSchema),
  dailyReviews: z.array(DailyReviewExportSchema)
});

export type ValidatedPortabilityBundle = z.infer<typeof PortabilityBundleSchema>;