export interface BatchListItem {
  id: string;
  batchNumber: string;
  status: string;
  producedQuantity: number;
  unit: string;
  createdAt: string;
}

export interface BatchDetail {
  id: string;
  batchNumber: string;
  status: string;
  producedQuantity: number;
  producedQuantityUnit: string;
  workOrderId: string;
  productId: string;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  notes: string | null;
}

export interface BatchesListResponse {
  items: BatchListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateBatchRequest {
  batchNumber: string;
  workOrderId: string;
  productId: string;
  producedQuantity: number;
  producedQuantityUnit: string;
}

export interface CreateBatchResponse {
  id: string;
  batchNumber: string;
  status: string;
  createdAt: string;
}