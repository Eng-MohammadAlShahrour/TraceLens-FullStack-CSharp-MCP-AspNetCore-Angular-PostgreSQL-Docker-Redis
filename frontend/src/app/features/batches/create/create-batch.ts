import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BatchService } from '../../../core/services/batch.service';

@Component({
  selector: 'app-create-batch',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './create-batch.html',
  styleUrl: './create-batch.css'
})
export class CreateBatch {
  private fb = inject(FormBuilder);
  private batchService = inject(BatchService);
  private router = inject(Router);

  submitting = signal(false);
  error = signal<string | null>(null);

  form = this.fb.group({
    batchNumber: ['', [Validators.required, Validators.maxLength(50)]],
    workOrderId: ['11111111-1111-1111-1111-111111111111', Validators.required],
    productId: ['22222222-2222-2222-2222-222222222222', Validators.required],
    producedQuantity: [100, [Validators.required, Validators.min(0.01)]],
    producedQuantityUnit: ['pcs', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    const req = {
      batchNumber: this.form.value.batchNumber!,
      workOrderId: this.form.value.workOrderId!,
      productId: this.form.value.productId!,
      producedQuantity: Number(this.form.value.producedQuantity),
      producedQuantityUnit: this.form.value.producedQuantityUnit!
    };

    this.batchService.create(req).subscribe({
      next: () => this.router.navigate(['/batches']),
      error: (err) => {
        this.submitting.set(false);
        this.error.set(err?.error?.title || err?.error?.message || 'Failed to create batch. Is the API running?');
      }
    });
  }

  isInvalid(field: string): boolean {
    const c = this.form.get(field);
    return !!c && c.invalid && (c.dirty || c.touched);
  }
}