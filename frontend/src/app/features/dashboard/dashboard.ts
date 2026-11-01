import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, switchMap, tap, debounceTime } from 'rxjs';
import { BatchService } from '../../core/services/batch.service';
import { SignalRService } from '../../core/services/signalr.service';
import { BatchesListResponse } from '../../core/models/batch.model';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  private batchService = inject(BatchService);
  private signalr = inject(SignalRService);

  private _loaded = signal(false);
  readonly loading = computed(() => !this._loaded());

  private refresh$ = new BehaviorSubject<void>(undefined);

  private batchesResponse = toSignal(
    this.refresh$.pipe(
      
      debounceTime(100),
      switchMap(() => this.batchService.getList(1, 100)),
      tap(() => this._loaded.set(true))
    ),
    { initialValue: { items: [], totalCount: 0, page: 1, pageSize: 100 } as BatchesListResponse }
  );

  readonly batches = computed(() => this.batchesResponse().items);
  readonly totalBatches = computed(() => this.batchesResponse().totalCount);
  readonly inProduction = computed(() => this.batches().filter(b => b.status === 'InProduction').length);
  readonly qcPassed = computed(() => this.batches().filter(b => b.status === 'QCPassed').length);
  readonly quarantined = computed(() => this.batches().filter(b => b.status === 'Quarantined').length);
  readonly recentBatches = computed(() => this.batches().slice(0, 4));

  readonly qcSuccessRate = computed(() => {
    const items = this.batches();
    const passed = items.filter(b => b.status === 'QCPassed').length;
    const failed = items.filter(b => b.status === 'QCFailed').length;
    const total = passed + failed;
    return total === 0 ? 0 : Math.round((passed / total) * 100);
  });

  constructor() {
    // Real-time: refetch on BatchCreated (from MCP or REST)
    this.signalr.batchCreated$
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.refresh$.next());
  }

  statusBadge(status: string): string {
    const map: Record<string, string> = {
      'Created': 'badge-neutral',
      'InProduction': 'badge-info',
      'QCPending': 'badge-warning',
      'QCPassed': 'badge-success',
      'QCFailed': 'badge-danger',
      'Quarantined': 'badge-danger',
      'Shipped': 'badge-success',
      'Cancelled': 'badge-neutral'
    };
    return map[status] ?? 'badge-neutral';
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      'InProduction': 'In Production',
      'QCPending': 'QC Pending',
      'QCPassed': 'QC Passed',
      'QCFailed': 'QC Failed'
    };
    return map[status] ?? status;
  }

  timeAgo(isoDate: string): string {
    if (!isoDate) return '—';
    const date = new Date(isoDate).getTime();
    if (isNaN(date)) return '—';
    const diff = Date.now() - date;
    const minutes = Math.floor(diff / 60000);
    if (minutes < 1) return 'just now';
    if (minutes < 60) return `${minutes} min ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    const days = Math.floor(hours / 24);
    return `${days} day${days > 1 ? 's' : ''} ago`;
  }
}
