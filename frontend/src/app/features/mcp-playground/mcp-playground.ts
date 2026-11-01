import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { McpService, McpToolResult } from '../../core/services/mcp.service';

interface ToolCall extends McpToolResult {
  toolName: string;
  args: Record<string, any>;
  timestamp: string;
}

@Component({
  selector: 'app-mcp-playground',
  imports: [CommonModule, FormsModule],
  templateUrl: './mcp-playground.html',
  styleUrl: './mcp-playground.css'
})
export class McpPlayground {
  private mcp = inject(McpService);

  batchStatus = 'InProduction';
  executing = signal<string | null>(null);
  calls = signal<ToolCall[]>([]);

  newBatch = {
    batchNumber: 'B-2026-100',
    workOrderId: '11111111-1111-1111-1111-111111111111',
    productId: '22222222-2222-2222-2222-222222222222',
    producedQuantity: 100,
    producedQuantityUnit: 'pcs'
  };

  statusOptions = ['Created', 'InProduction', 'QCPassed', 'QCFailed', 'Quarantined', 'Shipped'];

  async runGetBatchesByStatus(): Promise<void> {
    await this.run('get_batches_by_status', { status: this.batchStatus });
  }

  async runGetWorkOrders(): Promise<void> {
    await this.run('get_work_orders', {});
  }

  async runCreateBatch(): Promise<void> {
    await this.run('create_batch', {
      batchNumber: this.newBatch.batchNumber,
      workOrderId: this.newBatch.workOrderId,
      productId: this.newBatch.productId,
      producedQuantity: Number(this.newBatch.producedQuantity),
      producedQuantityUnit: this.newBatch.producedQuantityUnit
    });
  }

  private async run(name: string, args: Record<string, any>): Promise<void> {
    this.executing.set(name);
    const result = await this.mcp.callTool(name, args);
    this.calls.update(arr => [{
      toolName: name,
      args,
      timestamp: new Date().toLocaleTimeString(),
      ...result
    }, ...arr].slice(0, 10));
    this.executing.set(null);
  }

  clearCalls(): void {
    this.calls.set([]);
  }

  prettyJson(v: any): string {
    return typeof v === 'string' ? v : JSON.stringify(v, null, 2);
  }
}
