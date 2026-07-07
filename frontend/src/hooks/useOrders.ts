import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderApi } from '../api/orderApi';
import type { Order, UpdateOrderStatusRequest } from '../types/order';

// Ключи для кеширования
export const orderKeys = {
    all: ['orders'] as const,
    lists: () => [...orderKeys.all, 'list'] as const,
    list: () => [...orderKeys.lists()] as const,
    details: () => [...orderKeys.all, 'detail'] as const,
    detail: (id: string) => [...orderKeys.details(), id] as const,
};

// Хук для получения всех заказов
export const useOrders = () => {
    return useQuery({
        queryKey: orderKeys.list(),
        queryFn: orderApi.getOrders,
        staleTime: 30000,
    });
};

// Хук для получения заказа по ID
export const useOrder = (id: string) => {
    return useQuery({
        queryKey: orderKeys.detail(id),
        queryFn: () => orderApi.getOrderById(id),
        enabled: !!id,
        staleTime: 30000,
    });
};

// Хук для создания заказа
export const useCreateOrder = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: orderApi.createOrder,
        onSuccess: (newOrder) => {
            // Оптимистичное обновление списка
            queryClient.setQueryData<Order[]>(orderKeys.list(), (old = []) => {
                return [newOrder, ...old];
            });
            // Инвалидируем для синхронизации
            queryClient.invalidateQueries({ queryKey: orderKeys.list() });
        },
    });
};

// Хук для обновления статуса заказа
export const useUpdateOrderStatus = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, status }: { id: string; status: UpdateOrderStatusRequest }) =>
            orderApi.updateOrderStatus(id, status),

        onMutate: async ({ id, status }) => {
            // Отменяем текущие запросы
            await queryClient.cancelQueries({ queryKey: orderKeys.detail(id) });

            // Сохраняем предыдущее состояние
            const previousOrder = queryClient.getQueryData<Order>(orderKeys.detail(id));

            // Оптимистично обновляем
            queryClient.setQueryData<Order>(orderKeys.detail(id), (old) => {
                if (!old) return old;
                return { ...old, status: status.status };
            });

            // Обновляем в списке
            queryClient.setQueryData<Order[]>(orderKeys.list(), (old = []) => {
                return old.map(order =>
                    order.id === id ? { ...order, status: status.status } : order
                );
            });

            return { previousOrder };
        },

        onError: (_err, variables, context) => {
            // Откат при ошибке
            if (context?.previousOrder) {
                queryClient.setQueryData(
                    orderKeys.detail(variables.id),
                    context.previousOrder
                );
            }
        },

        onSettled: (_data, _error, variables) => {
            // Инвалидация для синхронизации
            queryClient.invalidateQueries({ queryKey: orderKeys.detail(variables.id) });
            queryClient.invalidateQueries({ queryKey: orderKeys.list() });
        },
    });
};