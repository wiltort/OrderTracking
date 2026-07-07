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

export interface OrderStatusChangedEvent {
    orderId: string;
    orderNumber: string;
    oldStatus: OrderStatus;
    newStatus: OrderStatus;
    updatedAt: string;
    message: string;
}

export interface NewOrderCreatedEvent {
    orderId: string;
    orderNumber: string;
    status: OrderStatus;
    createdAt: string;
    message: string;
}