import * as signalR from '@microsoft/signalr';
import type { OrderStatusChangedEvent, NewOrderCreatedEvent } from '../types/order';

// Use the Vite proxy in development (relative URL), fall back to direct connection
const HUB_URL = import.meta.env.VITE_HUB_URL || '/hubs/order';
console.log('[SignalR] Hub URL:', HUB_URL);

export class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private isConnected = false;
    private startPromise: Promise<void> | null = null;
    private stopped = false;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext) => {
                    if (retryContext.elapsedMilliseconds < 60000) {
                        return 1000;
                    }
                    return 5000;
                },
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.setupEventHandlers();
    }

    private setupEventHandlers() {
        if (!this.connection) return;

        this.connection.onclose((error) => {
            this.isConnected = false;
            this.startPromise = null;
            console.log('SignalR connection closed:', error);
        });

        this.connection.onreconnecting((error) => {
            console.log('SignalR reconnecting:', error);
        });

        this.connection.onreconnected((connectionId) => {
            this.isConnected = true;
            console.log('SignalR reconnected:', connectionId);
        });
    }

    async start(): Promise<void> {
        // If already connected, nothing to do
        if (this.isConnected) return;

        // If a start is already in progress, return the existing promise
        // This prevents duplicate starts when React StrictMode double-invokes effects
        if (this.startPromise) return this.startPromise;

        // If the connection is in the process of disconnecting, wait for it to finish
        if (this.connection && this.connection.state === signalR.HubConnectionState.Disconnecting) {
            console.log(`SignalR connection is in '${this.connection.state}' state, waiting for disconnect...`);
            try {
                await this.connection.stop();
            } catch {
                // Ignore stop errors
            }
        }

        try {
            this.stopped = false;
            this.startPromise = this.connection!.start();
            await this.startPromise;
            // If stop() was called while start() was in progress, disconnect immediately
            if (this.stopped) {
                await this.connection!.stop().catch(() => {});
                this.isConnected = false;
                this.startPromise = null;
                return;
            }
            this.isConnected = true;
            console.log('SignalR connected');
        } catch (error) {
            this.isConnected = false;
            this.startPromise = null;
            console.error('SignalR connection failed:', error);
            throw error;
        }
    }

    async stop(): Promise<void> {
        this.stopped = true;
        this.startPromise = null;

        if (!this.connection || this.connection.state === signalR.HubConnectionState.Disconnected) {
            this.isConnected = false;
            return;
        }

        try {
            await this.connection.stop();
            this.isConnected = false;
            console.log('SignalR stopped');
        } catch (error) {
            console.error('SignalR stop failed:', error);
            throw error;
        }
    }

    // Подписка на события
    onOrderStatusChanged(callback: (event: OrderStatusChangedEvent) => void): void {
        this.connection?.on('OrderStatusChanged', callback);
    }

    onNewOrderCreated(callback: (event: NewOrderCreatedEvent) => void): void {
        this.connection?.on('NewOrderCreated', callback);
    }

    onConnected(callback: (data: any) => void): void {
        this.connection?.on('Connected', callback);
    }

    // Отписка от событий
    offOrderStatusChanged(): void {
        this.connection?.off('OrderStatusChanged');
    }

    offNewOrderCreated(): void {
        this.connection?.off('NewOrderCreated');
    }

    // Вызов методов хаба
    async subscribeToOrder(orderId: string): Promise<void> {
        if (!this.isConnected) throw new Error('SignalR not connected');
        await this.connection?.invoke('SubscribeToOrder', orderId);
    }

    async unsubscribeFromOrder(orderId: string): Promise<void> {
        if (!this.isConnected) throw new Error('SignalR not connected');
        await this.connection?.invoke('UnsubscribeFromOrder', orderId);
    }

    async sendTestNotification(message: string): Promise<void> {
        if (!this.isConnected) throw new Error('SignalR not connected');
        await this.connection?.invoke('SendTestNotification', message);
    }
}

// Создаем singleton экземпляр
export const signalRService = new SignalRService();