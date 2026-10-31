export interface WorkOrder {
  id: string;
  orderNumber: string;
  productId: string;
  plannedQuantity: number;
  dueDate: string;
  isClosed: boolean;
  createdAt: string;
}

export interface WorkOrdersListResponse {
  items: WorkOrder[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateWorkOrderRequest {
  orderNumber: string;
  productId: string;
  plannedQuantity: number;
  dueDate: string;
}

export interface CreateWorkOrderResponse {
  id: string;
  orderNumber: string;
  plannedQuantity: number;
  dueDate: string;
  isClosed: boolean;
  createdAt: string;
}
