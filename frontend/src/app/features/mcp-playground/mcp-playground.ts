import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { McpService, McpToolResult } from '../../core/services/mcp.service';

type TabId = 'pretty' | 'raw' | 'meta';
type Shape = 'workOrders' | 'batches' | 'createdBatch' | 'raw';

interface ToolCall extends McpToolResult {
  toolName: string;
  args: Record<string, any>;
  timestamp: string;
  uniqueId: string;
  activeTab: TabId;
}

@Component({
  selector: 'app-mcp-playground',
  imports: [CommonModule, FormsModule],
  templateUrl: './mcp-playground.html',
  styleUrl: './mcp-playground.css'
})
export class McpPlayground {
  private mcp = inject(McpService);
  private sanitizer = inject(DomSanitizer);

  batchStatus = 'InProduction';
  executing = signal<string | null>(null);
  calls = signal<ToolCall[]>([]);
  copiedId = signal<string | null>(null);

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
    const call: ToolCall = {
      toolName: name,
      args,
      timestamp: new Date().toLocaleTimeString(),
      uniqueId: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
      activeTab: 'pretty',
      ...result
    };
    call.activeTab = this.getShape(call) === 'raw' ? 'raw' : 'pretty';
    this.calls.update(arr => [call, ...arr].slice(0, 10));
    this.executing.set(null);
  }

  clearCalls(): void {
    this.calls.set([]);
  }

  prettyJson(v: any): string {
    if (v === undefined || v === null) return '';
    return typeof v === 'string' ? v : JSON.stringify(v, null, 2);
  }

  field(obj: any, ...keys: string[]): any {
    if (!obj) return undefined;
    for (const k of keys) if (obj[k] !== undefined) return obj[k];
    return undefined;
  }

  formatDate(v: any): string {
    if (!v) return '—';
    try {
      return new Date(v).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
    } catch {
      return String(v);
    }
  }

  formatNumber(v: any): string {
    if (v === undefined || v === null) return '—';
    const n = Number(v);
    return isNaN(n) ? String(v) : n.toLocaleString();
  }

  formatBytes(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1024 / 1024).toFixed(2) + ' MB';
  }

  responseSize(call: ToolCall): number {
    try {
      return new Blob([this.prettyJson(call.data)]).size;
    } catch {
      return 0;
    }
  }

  getShape(call: ToolCall): Shape {
    const d: any = call.data;
    if (!d || typeof d !== 'object') return 'raw';

    if (Array.isArray(d.workOrders) && d.workOrders.length) return 'workOrders';

    if (Array.isArray(d) && d.length) {
      const first = d[0];
      if (this.field(first, 'batchNumber', 'BatchNumber')) return 'batches';
      if (this.field(first, 'OrderNumber', 'orderNumber')) return 'workOrders';
      return 'raw';
    }

    const arr = d.batches ?? d.items ?? d.results;
    if (Array.isArray(arr) && arr.length && this.field(arr[0], 'batchNumber', 'BatchNumber')) return 'batches';

    if (this.field(d, 'batchNumber', 'BatchNumber') !== undefined && this.field(d, 'id', 'Id') !== undefined) return 'createdBatch';

    return 'raw';
  }

  getWorkOrders(call: ToolCall): any[] {
    const d: any = call.data;
    if (!d) return [];
    if (Array.isArray(d)) return d.filter((x: any) => this.field(x, 'OrderNumber', 'orderNumber'));
    return d.workOrders ?? [];
  }

  getBatches(call: ToolCall): any[] {
    const d: any = call.data;
    if (!d) return [];
    if (Array.isArray(d)) return d.filter((x: any) => this.field(x, 'batchNumber', 'BatchNumber'));
    return d.batches ?? d.items ?? d.results ?? [];
  }

  getTotal(call: ToolCall): any {
    const t = this.field(call.data, 'total', 'Total', 'count', 'Count');
    if (t !== undefined) return t;
    return this.getBatches(call).length || this.getWorkOrders(call).length;
  }

  setTab(call: ToolCall, tab: TabId): void {
    this.calls.update(arr => arr.map(c => c.uniqueId === call.uniqueId ? { ...c, activeTab: tab } : c));
  }

  highlight(call: ToolCall): SafeHtml {
    const json = this.prettyJson(call.data);
    const escaped = json
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');
    const html = escaped.replace(
      /("(\\u[a-fA-F0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,
      (match) => {
        let style = 'color:#D97706';
        if (/^"/.test(match)) {
          style = /:$/.test(match) ? 'color:#0891B2;font-weight:600' : 'color:#0F766E';
        } else if (/true|false/.test(match)) {
          style = 'color:#7C3AED;font-weight:600';
        } else if (/null/.test(match)) {
          style = 'color:#94A3B8;font-style:italic';
        }
        return '<span style="' + style + '">' + match + '</span>';
      }
    );
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  async copyCall(call: ToolCall): Promise<void> {
    const text = this.prettyJson(call.data);
    try {
      await navigator.clipboard.writeText(text);
      this.copiedId.set(call.uniqueId);
      setTimeout(() => {
        if (this.copiedId() === call.uniqueId) this.copiedId.set(null);
      }, 1500);
    } catch {}
  }

  downloadCall(call: ToolCall): void {
    const json = this.prettyJson(call.data);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = call.toolName + '-' + Date.now() + '.json';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
}
