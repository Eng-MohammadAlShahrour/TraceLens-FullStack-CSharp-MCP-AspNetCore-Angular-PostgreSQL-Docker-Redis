import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BatchDetail,
  BatchesListResponse,
  CreateBatchRequest,
  CreateBatchResponse
} from '../models/batch.model';

@Injectable({ providedIn: 'root' })
export class BatchService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/batches`;

  getList(page = 1, pageSize = 20): Observable<BatchesListResponse> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<BatchesListResponse>(this.baseUrl, { params });
  }

  getById(id: string): Observable<BatchDetail> {
    return this.http.get<BatchDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateBatchRequest): Observable<CreateBatchResponse> {
    return this.http.post<CreateBatchResponse>(this.baseUrl, request);
  }

  postAction(path: string, body?: any): Observable<BatchDetail> {
    return this.http.post<BatchDetail>(`${this.baseUrl}/${path}`, body ?? {});
  }
}
