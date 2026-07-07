import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useOrder, useUpdateOrderStatus } from '../hooks/useOrders';
import { OrderStatusBadge } from '../components/orders/OrderStatusBadge';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { ErrorMessage } from '../components/common/ErrorMessage';
import type { OrderStatus } from '../types/order';

const statusOptions: { value: OrderStatus; label: string }[] = [
    { value: 'Created', label: 'Создан' },
    { value: 'Sent', label: 'Отправлен' },
    { value: 'Delivered', label: 'Доставлен' },
    { value: 'Cancelled', label: 'Отменен' },
];

export const OrderDetailPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { data: order, isLoading, error, refetch } = useOrder(id!);
    const updateStatus = useUpdateOrderStatus();
    const [selectedStatus, setSelectedStatus] = useState<OrderStatus | ''>('');

    const handleStatusUpdate = async () => {
        if (!id || !selectedStatus) return;
        try {
            await updateStatus.mutateAsync({ id, status: { status: selectedStatus } });
            setSelectedStatus('');
        } catch (err) {
            console.error('Failed to update status:', err);
        }
    };

    if (isLoading) return <LoadingSpinner />;
    if (error) return <ErrorMessage error={error} onRetry={refetch} />;
    if (!order) return <div className="text-center py-12 text-gray-500">Заказ не найден</div>;

    return (
        <div className="container mx-auto px-4 py-8 max-w-2xl">
            <button
                onClick={() => navigate('/orders')}
                className="mb-4 text-blue-500 hover:text-blue-700 transition-colors"
            >
                ← Назад к списку
            </button>

            <div className="bg-white rounded-lg shadow-md p-6">
                <div className="flex justify-between items-start mb-6">
                    <div>
                        <h1 className="text-2xl font-bold">Заказ #{order.orderNumber}</h1>
                        <p className="text-sm text-gray-500 mt-1">
                            ID: {order.id}
                        </p>
                    </div>
                    <OrderStatusBadge status={order.status} />
                </div>

                <div className="border-t pt-4 space-y-3">
                    <div>
                        <span className="text-sm text-gray-500">Описание:</span>
                        <p className="mt-1 text-gray-800">{order.description}</p>
                    </div>
                    <div className="flex gap-6 text-sm text-gray-500">
                        <span>Создан: {new Date(order.createdAt).toLocaleString()}</span>
                        <span>Обновлен: {new Date(order.updatedAt).toLocaleString()}</span>
                    </div>
                </div>

                <div className="border-t mt-6 pt-6">
                    <h2 className="text-lg font-semibold mb-3">Изменить статус</h2>
                    <div className="flex gap-3">
                        <select
                            value={selectedStatus}
                            onChange={(e) => setSelectedStatus(e.target.value as OrderStatus)}
                            className="flex-1 border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            <option value="">Выберите статус...</option>
                            {statusOptions.map((opt) => (
                                <option key={opt.value} value={opt.value}>
                                    {opt.label}
                                </option>
                            ))}
                        </select>
                        <button
                            onClick={handleStatusUpdate}
                            disabled={!selectedStatus || updateStatus.isPending}
                            className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors disabled:opacity-50"
                        >
                            {updateStatus.isPending ? 'Обновление...' : 'Обновить'}
                        </button>
                    </div>
                    {updateStatus.isError && (
                        <p className="mt-2 text-sm text-red-500">
                            Ошибка при обновлении статуса: {updateStatus.error?.message}
                        </p>
                    )}
                </div>
            </div>
        </div>
    );
};