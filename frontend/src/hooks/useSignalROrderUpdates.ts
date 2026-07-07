import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { signalRService } from '../api/signalRService';
import { orderKeys } from './useOrders';
import type { Order, OrderStatusChangedEvent, NewOrderCreatedEvent } from '../types/order';

/**
 * Хук для подписки на real-time обновления заказов через SignalR.
 * Автоматически обновляет кэш React Query при получении событий:
 * - OrderStatusChanged — обновляет статус в списке и в детальной карточке
 * - NewOrderCreated — добавляет новый заказ в список
 *
 * Использовать один раз на страницах OrdersPage и OrderDetailPage.
 */
export const useSignalROrderUpdates = () => {
    const queryClient = useQueryClient();

    useEffect(() => {
        // Подписка на изменение статуса заказа
        signalRService.onOrderStatusChanged((event: OrderStatusChangedEvent) => {
            console.log('[SignalR] Order status changed:', event);

            const { orderId, newStatus, updatedAt } = event;

            // 1. Обновляем заказ в списке (orders list)
            queryClient.setQueryData<Order[]>(orderKeys.list(), (oldOrders) => {
                if (!oldOrders) return oldOrders;
                return oldOrders.map((order) =>
                    order.id === orderId
                        ? { ...order, status: newStatus, updatedAt }
                        : order
                );
            });

            // 2. Обновляем детальную карточку заказа (order detail)
            queryClient.setQueryData<Order>(orderKeys.detail(orderId), (oldOrder) => {
                if (!oldOrder) return oldOrder;
                return { ...oldOrder, status: newStatus, updatedAt };
            });
        });

        // Подписка на создание нового заказа
        signalRService.onNewOrderCreated((event: NewOrderCreatedEvent) => {
            console.log('[SignalR] New order created:', event);

            const { orderId, orderNumber, status, createdAt } = event;

            // Создаём объект заказа из события
            const newOrder: Order = {
                id: orderId,
                orderNumber,
                description: '', // описание не приходит в событии — подтянется при следующем refetch
                status,
                createdAt,
                updatedAt: createdAt,
            };

            // Добавляем новый заказ в начало списка
            queryClient.setQueryData<Order[]>(orderKeys.list(), (oldOrders) => {
                if (!oldOrders) return [newOrder];
                // Проверяем, нет ли уже такого заказа (чтобы избежать дубликатов)
                const exists = oldOrders.some((o) => o.id === orderId);
                if (exists) {
                    return oldOrders.map((o) =>
                        o.id === orderId ? { ...o, status } : o
                    );
                }
                return [newOrder, ...oldOrders];
            });
        });

        // Отписка при размонтировании
        return () => {
            signalRService.offOrderStatusChanged();
            signalRService.offNewOrderCreated();
        };
    }, [queryClient]);
};