import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BatchService } from '../../../core/services/batch.service';
import { BatchDetail } from '../../../core/models/batch.model';

@Component({
  selector: 'app-batch-details',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './batch-details.html',
  styleUrl: './batch-details.css'
})
export class BatchDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private batchService = inject(BatchService);

  batch = signal<BatchDetail | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  acting = signal(false);

  showQuarantineModal = signal(false);
  quarantineReason = signal('');
  quarantineError = signal<string | null>(null);

  private id = '';

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.id) {
      this.error.set('Invalid batch ID');
      this.loading.set(false);
      return;
    }
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.batchService.getById(this.id).subscribe({
      next: (b) => { this.batch.set(b); this.loading.set(false); },
      error: () => { this.error.set('Could not load batch.'); this.loading.set(false); }
    });
  }

  startProduction(): void { this.action('start-production'); }
  passQC(): void { this.action('pass-qc'); }
  failQC(): void { this.action('fail-qc'); }
  ship(): void { this.action('ship'); }

  openQuarantine(): void {
    this.quarantineReason.set('');
    this.quarantineError.set(null);
    this.showQuarantineModal.set(true);
  }

  closeQuarantine(): void {
    this.showQuarantineModal.set(false);
  }

  confirmQuarantine(): void {
    const reason = this.quarantineReason().trim();
    if (!reason) {
      this.quarantineError.set('Reason is required.');
      return;
    }
    if (reason.length > 1000) {
      this.quarantineError.set('Reason too long (max 1000).');
      return;
    }
    this.action('quarantine', { reason });
  }

  private action(path: string, body?: any): void {
    this.acting.set(true);
    const url = `${this.id}/${path}`;
    this.batchService.postAction(url, body).subscribe({
      next: (b) => {
        this.batch.set(b);
        this.acting.set(false);
        this.showQuarantineModal.set(false);
      },
      error: () => {
        this.error.set('Action failed.');
        this.acting.set(false);
      }
    });
  }

  statusBadge(): string {
    const s = this.batch()?.status ?? '';
    switch (s) {
      case 'QCPassed':
      case 'Shipped': return 'badge badge-success';
      case 'InProduction': return 'badge badge-info';
      case 'QCFailed':
      case 'Quarantined': return 'badge badge-danger';
      default: return 'badge badge-neutral';
    }
  }

  statusLabel(): string {
    const s = this.batch()?.status ?? '';
    return s === 'InProduction' ? 'In Production'
      : s === 'QCPassed' ? 'QC Passed'
      : s === 'QCFailed' ? 'QC Failed'
      : s;
  }
}
