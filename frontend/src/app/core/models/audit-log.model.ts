export interface AuditLogDto {
  id: string;
  eventType: string;
  aggregateId: string;
  batchNumber: string;
  previousStatus: string | null;
  newStatus: string | null;
  reason: string | null;
  createdAt: string;
}

export interface ActivityResponse {
  items: AuditLogDto[];
  totalCount: number;
}
