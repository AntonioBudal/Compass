export type CommitmentType = 'TASK' | 'EVENT' | 'HABIT' | 'NOTE';
export type CommitmentStatus = 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' | 'ARCHIVED' | 'BLOCKED';

export interface CommitmentDto {
  id: string;
  title: string;
  type: CommitmentType;
  status: CommitmentStatus;
  estimatedDurationMinutes: number;
  energyRequired: number; // 1 | 2 | 3
  deadline: string | null;
  startTime: string | null;
  endTime: string | null;
  locationOrLink: string | null;
  cronExpression: string | null;
  currentStreak: number;
  bestStreak: number;
  postponedCount: number;
  content: string | null;
  projectId: string | null;
  projectName: string | null;
}

export interface ActiveHardBlockerDto {
  title: string;
  startsAt: string;
}

export interface DecisionContextDto {
  effectiveEnergy: number;
  availableWindowMinutes: number;
  activeHardBlocker: ActiveHardBlockerDto | null;
}

export interface ScoredActionDto {
  id: string;
  title: string;
  type: CommitmentType;
  estimatedDurationMinutes: number;
  energyRequired: number;
  projectName: string | null;
  scorePercentage: number;
  reason: string;
}

export interface DecisionResponseDto {
  generatedAt: string;
  context: DecisionContextDto;
  topFocus: ScoredActionDto | null;
  alternatives: ScoredActionDto[];
}

export interface CreateCommitmentDto {
  title: string;
  type: CommitmentType;
  projectId?: string | null;
  estimatedDurationMinutes?: number | null;
  energyRequired?: number | null;
  deadline?: string | null;
  startTime?: string | null;
  endTime?: string | null;
  locationOrLink?: string | null;
  cronExpression?: string | null;
  content?: string | null;
}

export interface UpdateCommitmentDto {
  title?: string;
  estimatedDurationMinutes?: number;
  energyRequired?: number;
  deadline?: string | null;
  content?: string | null;
  projectId?: string | null;
}

export interface UpdateStatusDto {
  newStatus: CommitmentStatus;
}

export interface CascadedDomainEventDto {
  eventType: string;
  entityId: string;
  message: string;
  newProgressPercentage?: number | null;
}

export interface StatusTransitionResponseDto {
  commitmentId: string;
  previousStatus: CommitmentStatus;
  currentStatus: CommitmentStatus;
  timestamp: string;
  cascadedDomainEvents: CascadedDomainEventDto[];
}