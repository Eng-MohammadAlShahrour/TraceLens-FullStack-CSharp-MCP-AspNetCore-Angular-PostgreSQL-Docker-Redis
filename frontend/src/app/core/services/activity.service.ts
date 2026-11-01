import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ActivityResponse } from '../models/audit-log.model';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/activity`;

  getRecent(limit = 100): Observable<ActivityResponse> {
    return this.http.get<ActivityResponse>(`${this.apiUrl}?limit=${limit}`);
  }
}
