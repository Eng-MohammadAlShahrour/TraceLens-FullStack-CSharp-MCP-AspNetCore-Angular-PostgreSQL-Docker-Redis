import { Injectable, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface BatchCreatedEvent {
  id: string;
  batchNumber: string;
  status: string;
}

export interface BatchStatusChangedEvent {
  id: string;
  batchNumber: string;
  status: string;
}

export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly hubUrl = 'http://localhost:5268/hubs/batches';
  private connection?: HubConnection;

  private batchCreatedSubject = new Subject<BatchCreatedEvent>();
  private batchStatusChangedSubject = new Subject<BatchStatusChangedEvent>();
  private stateSignal = signal<ConnectionState>('disconnected');

  readonly batchCreated$ = this.batchCreatedSubject.asObservable();
  readonly batchStatusChanged$ = this.batchStatusChangedSubject.asObservable();
  readonly state = this.stateSignal.asReadonly();

  async start(): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected) return;

    this.stateSignal.set('connecting');

    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('BatchCreated', (e: BatchCreatedEvent) =>
      this.batchCreatedSubject.next(e)
    );

    this.connection.on('BatchStatusChanged', (e: BatchStatusChangedEvent) =>
      this.batchStatusChangedSubject.next(e)
    );

    this.connection.onreconnecting(() => this.stateSignal.set('reconnecting'));
    this.connection.onreconnected(() => this.stateSignal.set('connected'));
    this.connection.onclose(() => this.stateSignal.set('disconnected'));

    try {
      await this.connection.start();
      this.stateSignal.set('connected');
      console.log('[SignalR] Connected →', this.hubUrl);
    } catch (err) {
      this.stateSignal.set('disconnected');
      console.error('[SignalR] Connect failed — retry in 5s', err);
      setTimeout(() => {
        this.connection = undefined;
        this.start();
      }, 5000);
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = undefined;
    }
  }
}
