import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { merge, debounceTime } from 'rxjs';
import { ActivityService } from '../../core/services/activity.service';
import { SignalRService } from '../../core/services/signalr.service';
import { AuditLogDto } from '../../core/models/audit-log.model';

@Component({
  selector: 'app-activity',
  imports: [CommonModule],
  templateUrl: './activity.html',
  styleUrl: './activity.css'
})
export class Activity implements OnInit {
  private activityService = inject(ActivityService);
  private signalr = inject(SignalRService);

  logs = signal<AuditLogDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  totalCount = signal(0);
  readonly signalrState = this.signalr.state;

  readonly hasData = computed(() => this.logs().length > 0);

  constructor() {
    // Worker writes to audit_logs AFTER API SignalR fires.
    // Poll 3 times to catch the write regardless of timing.
    merge(this.signalr.batchCreated$, this.signalr.batchStatusChanged$)
      .pipe(
        debounceTime(200),
        takeUntilDestroyed()
      )
      .subscribe(() => this.scheduleActivityRefresh());
  }

  ngOnInit(): void {
    this.loadActivity();
  }

  private scheduleActivityRefresh(): void {
    // 400ms, 1000ms, 2000ms — covers fast Worker + slow Worker
    [400, 1000, 2000].forEach(ms =>
      setTimeout(() => this.loadActivity(), ms)
    );
  }

  loadActivity(): void {
    this.error.set(null);
    this.activityService.getRecent(100).subscribe({
      next: (res) => {
        this.logs.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load activity', err);
        this.error.set('Could not load activity log. Is the API running?');
        this.loading.set(false);
      }
    });
  }

  eventIcon(eventType: string): string {
    if (eventType === 'BatchCreatedIntegrationEvent') return '✓';
    if (eventType === 'BatchStatusChangedIntegrationEvent') return '↻';
    return '•';
  }

  eventLabel(eventType: string): string {
    if (eventType === 'BatchCreatedIntegrationEvent') return 'Batch Created';
    if (eventType === 'BatchStatusChangedIntegrationEvent') return 'Status Changed';
    return eventType;
  }

  statusBadge(status: string | null): string {
    if (!status) return 'badge-neutral';
    const map: Record<string, string> = {
      'Created': 'badge-neutral',
      'InProduction': 'badge-info',
      'QCPending': 'badge-warning',
      'QCPassed': 'badge-success',
      'QCFailed': 'badge-danger',
      'Quarantined': 'badge-danger',
      'Shipped': 'badge-success'
    };
    return map[status] ?? 'badge-neutral';
  }

  timeAgo(isoDate: string): string {
    if (!isoDate) return '—';
    const date = new Date(isoDate).getTime();
    if (isNaN(date)) return '—';
    const diff = Date.now() - date;
    const seconds = Math.floor(diff / 1000);
    if (seconds < 60) return `${seconds}s ago`;
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;
    const days = Math.floor(hours / 24);
    return `${days}d ago`;
  }
}
