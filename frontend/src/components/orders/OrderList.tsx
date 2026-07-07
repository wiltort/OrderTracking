import React from 'react';
import { useOrders } from '../../hooks/useOrders';
import { LoadingSpinner } from '../common/LoadingSpinner';
import { ErrorMessage } from '../common/ErrorMessage';
import { OrderStatusBadge } from './OrderStatusBadge';
import { Link } from 'react-router-dom';

export const OrderList: React.FC = () => {
    const { data: orders, isLoading, error, refetch } = useOrders();

    if (isLoading) return <LoadingSpinner />;
    if (error) return <ErrorMessage error={error} onRetry={refetch} />;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold">Заказы</h2>
                <button
                    onClick={() => refetch()}
                    className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                >
                    Обновить
                </button>
            </div>

            <div className="grid gap-4">
                {orders?.map((order) => (
                    <div
                        key={order.id}
                        className="border rounded-lg p-4 shadow-sm hover:shadow-md transition-shadow"
                    >
                        <div className="flex justify-between items-start">
                            <div className="flex-1">
                                <div className="flex items-center gap-3 mb-2">
                                    <h3 className="font-semibold text-lg">
                                        Заказ #{order.orderNumber}
                                    </h3>
                                    <OrderStatusBadge status={order.status} />
                                </div>
                                <p className="text-gray-600">{order.description}</p>
                                <div className="flex gap-4 mt-2 text-sm text-gray-400">
                                    <span>Создан: {new Date(order.createdAt).toLocaleString()}</span>
                                    <span>Обновлен: {new Date(order.updatedAt).toLocaleString()}</span>
                                </div>
                            </div>
                            <Link
                                to={`/orders/${order.id}`}
                                className="ml-4 px-4 py-2 text-blue-500 hover:text-blue-700 border border-blue-500 rounded hover:bg-blue-50 transition-colors"
                            >
                                Подробнее →
                            </Link>
                        </div>
                    </div>
                ))}
            </div>

            {orders?.length === 0 && (
                <div className="text-center py-12 text-gray-500">
                    <p className="text-lg">Нет заказов</p>
                    <p className="text-sm">Создайте первый заказ!</p>
                </div>
            )}
        </div>
    );
};