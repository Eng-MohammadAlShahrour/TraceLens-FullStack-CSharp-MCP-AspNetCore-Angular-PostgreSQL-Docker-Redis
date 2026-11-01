import { Injectable } from '@angular/core';

export interface McpTool {
  name: string;
  description: string;
}

export interface McpToolResult {
  success: boolean;
  data: any;
  error?: string;
  durationMs: number;
}

@Injectable({ providedIn: 'root' })
export class McpService {
  private mcpUrl = 'http://localhost:5268/mcp';

  async callTool(name: string, args: Record<string, any>): Promise<McpToolResult> {
    const start = Date.now();
    try {
      const response = await this.call({
        jsonrpc: '2.0',
        id: Date.now(),
        method: 'tools/call',
        params: { name, arguments: args }
      });
      const durationMs = Date.now() - start;

      if (response.error) {
        return { success: false, error: response.error.message ?? 'MCP error', data: null, durationMs };
      }

      const content = response.result?.content ?? [];
      const textContent = content.find((c: any) => c.type === 'text')?.text;
      let data = textContent;
      try { data = JSON.parse(textContent); } catch { /* keep string */ }

      return { success: true, data, durationMs };
    } catch (e: any) {
      return { success: false, error: e?.message ?? 'Request failed', data: null, durationMs: Date.now() - start };
    }
  }

  private async call(body: any): Promise<any> {
    const response = await fetch(this.mcpUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json, text/event-stream'
      },
      body: JSON.stringify(body)
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }

    const contentType = response.headers.get('content-type') ?? '';

    if (contentType.includes('text/event-stream')) {
      const reader = response.body?.getReader();
      if (!reader) throw new Error('No response body');
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });

        const lines = buffer.split('\n');
        buffer = lines.pop() ?? '';

        for (const line of lines) {
          if (line.startsWith('data: ')) {
            const json = line.substring(6).trim();
            if (json) return JSON.parse(json);
          }
        }
      }
      throw new Error('No data in SSE response');
    }

    return await response.json();
  }
}
