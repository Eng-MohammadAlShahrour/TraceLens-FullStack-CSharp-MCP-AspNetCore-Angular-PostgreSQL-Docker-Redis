import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { WorkOrderService } from '../../core/services/work-order.service';
import { WorkOrder } from '../../core/models/work-order.model';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const ORDER_NUMBER_PATTERN = /^WO-\d{4}-\d{3}$/;

@Component({
  selector: 'app-work-orders',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './work-orders.html',
  styleUrl: './work-orders.css'
})
export class WorkOrders implements OnInit {
  private woService = inject(WorkOrderService);
  private fb = inject(FormBuilder);

  private _workOrders = signal<WorkOrder[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  totalCount = signal(0);

  showCreateModal = signal(false);
  submitting = signal(false);
  createError = signal<string | null>(null);

  createForm = this.fb.group({
    orderNumber: ['', [Validators.required, Validators.pattern(ORDER_NUMBER_PATTERN)]],
    productId: ['', [Validators.required, Validators.pattern(GUID_PATTERN)]],
    plannedQuantity: [null as number | null, [Validators.required, Validators.min(1)]],
    dueDate: ['', Validators.required]
  });

  get workOrderList(): WorkOrder[] {
    return this._workOrders();
  }

  ngOnInit(): void {
    this.loadWorkOrders();
  }

  loadWorkOrders(): void {
    this.loading.set(true);
    this.error.set(null);
    this.woService.getList().subscribe({
      next: (res) => {
        this._workOrders.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load work orders.');
        this.loading.set(false);
      }
    });
  }

  openCreateModal(): void {
    this.submitting.set(false);
    this.createError.set(null);
    this.createForm.reset({
      orderNumber: '',
      productId: '',
      plannedQuantity: null,
      dueDate: ''
    });
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
    this.submitting.set(false);
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.createError.set(null);

    const v = this.createForm.value;
    this.woService.create({
      orderNumber: v.orderNumber!.trim().toUpperCase(),
      productId: v.productId!,
      plannedQuantity: Number(v.plannedQuantity),
      dueDate: new Date(v.dueDate!).toISOString()
    }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.showCreateModal.set(false);
        this.loadWorkOrders();
      },
      error: (err) => {
        this.submitting.set(false);
        const apiErrors = err?.error?.errors;
        if (apiErrors) {
          const details = Object.entries(apiErrors)
            .map(([field, msgs]) => `${field}: ${Array.isArray(msgs) ? msgs.join(', ') : msgs}`)
            .join(' | ');
          this.createError.set(details);
        } else {
          this.createError.set(err?.error?.title || 'Failed to create.');
        }
      }
    });
  }

  isInvalid(field: string): boolean {
    const c = this.createForm.get(field);
    return !!c && c.invalid && (c.dirty || c.touched);
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString();
  }
}
