import React from 'react';
import type { OrderStatus } from '../../types/order';

interface OrderStatusBadgeProps {
    status: OrderStatus;
}

const statusColors: Record<OrderStatus, string> = {
    Created: 'bg-blue-100 text-blue-800',
    Sent: 'bg-yellow-100 text-yellow-800',
    Delivered: 'bg-green-100 text-green-800',
    Cancelled: 'bg-red-100 text-red-800',
};

const statusLabels: Record<OrderStatus, string> = {
    Created: 'Создан',
    Sent: 'Отправлен',
    Delivered: 'Доставлен',
    Cancelled: 'Отменен',
};

export const OrderStatusBadge: React.FC<OrderStatusBadgeProps> = ({ status }) => {
    return (
        <span className={`px-2 py-1 rounded-full text-xs font-medium ${statusColors[status]}`}>
            {statusLabels[status]}
        </span>
    );
};