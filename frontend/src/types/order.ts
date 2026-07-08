export type OrderStatus = 'Created' | 'Sent' | 'Delivered' | 'Cancelled';

export interface Order {
    id: string;
    orderNumber: string;
    description: string;
    status: OrderStatus;
    createdAt: string;
    updatedAt: string;
}

export interface CreateOrderRequest {
    description: string;
}

export interface UpdateOrderStatusRequest {
    status: OrderStatus;
}

// Событие от SignalR: изменение статуса заказа
// Бэкенд отправляет { OrderId, OrderNumber, OldStatus, NewStatus, UpdatedAt }
export interface OrderStatusChangedEvent {
    orderId: string;
    orderNumber: string;
    oldStatus: OrderStatus;
    newStatus: OrderStatus;
    updatedAt: string;
}

// Событие от SignalR: создание нового заказа
// Бэкенд отправляет { OrderId, OrderNumber, Description, Status, CreatedAt }
export interface NewOrderCreatedEvent {
    orderId: string;
    orderNumber: string;
    description: string;
    status: OrderStatus;
    createdAt: string;
}