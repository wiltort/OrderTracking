import { apiClient } from './client';
import type { Order, CreateOrderRequest, UpdateOrderStatusRequest } from '../types/order';

export const orderApi = {
    // Получение всех заказов
    getOrders: async (): Promise<Order[]> => {
        const response = await apiClient.get('/orders');
        return response.data;
    },

    // Получение заказа по ID
    getOrderById: async (id: string): Promise<Order> => {
        const response = await apiClient.get(`/orders/${id}`);
        return response.data;
    },

    // Создание заказа
    createOrder: async (data: CreateOrderRequest): Promise<Order> => {
        const response = await apiClient.post('/orders', data);
        return response.data;
    },

    // Обновление статуса заказа
    updateOrderStatus: async (id: string, data: UpdateOrderStatusRequest): Promise<Order> => {
        const response = await apiClient.patch(`/orders/${id}/status`, data);
        return response.data;
    },
};