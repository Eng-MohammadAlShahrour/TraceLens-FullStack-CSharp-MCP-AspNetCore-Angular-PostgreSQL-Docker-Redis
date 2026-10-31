import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BatchService } from '../../core/services/batch.service';
import { BatchListItem } from '../../core/models/batch.model';

@Component({
  selector: 'app-batches',
  imports: [CommonModule, RouterLink],
  templateUrl: './batches.html',
  styleUrl: './batches.css'
})
export class Batches implements OnInit {
  private batchService = inject(BatchService);

  private _batches = signal<BatchListItem[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  totalCount = signal(0);
  page = signal(1);
  pageSize = 20;

  get batchList(): BatchListItem[] {
    return this._batches();
  }

  ngOnInit(): void {
    this.loadBatches();
  }

  loadBatches(): void {
    this.loading.set(true);
    this.error.set(null);

    this.batchService.getList(this.page(), this.pageSize).subscribe({
      next: (res) => {
        this._batches.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load batches', err);
        this.error.set('Could not load batches. Is the API running?');
        this.loading.set(false);
      }
    });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'QCPassed': return 'badge badge-success';
      case 'InProduction': return 'badge badge-info';
      case 'QCFailed':
      case 'Quarantined': return 'badge badge-danger';
      case 'Shipped': return 'badge badge-success';
      default: return 'badge badge-neutral';
    }
  }

  statusLabel(status: string): string {
    switch (status) {
      case 'InProduction': return 'In Production';
      case 'QCPassed': return 'QC Passed';
      case 'QCFailed': return 'QC Failed';
      default: return status;
    }
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    const diff = Date.now() - d.getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins} min ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs} hour${hrs > 1 ? 's' : ''} ago`;
    return d.toLocaleDateString();
  }
}