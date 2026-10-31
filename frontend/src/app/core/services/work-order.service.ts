import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  WorkOrder,
  WorkOrdersListResponse,
  CreateWorkOrderRequest,
  CreateWorkOrderResponse
} from '../models/work-order.model';

@Injectable({ providedIn: 'root' })
export class WorkOrderService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/work-orders`;

  getList(page = 1, pageSize = 20): Observable<WorkOrdersListResponse> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<WorkOrdersListResponse>(this.baseUrl, { params });
  }

  create(request: CreateWorkOrderRequest): Observable<CreateWorkOrderResponse> {
    return this.http.post<CreateWorkOrderResponse>(this.baseUrl, request);
  }
}
