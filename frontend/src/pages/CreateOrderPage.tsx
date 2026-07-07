import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateOrder } from '../hooks/useOrders';

export const CreateOrderPage: React.FC = () => {
    const navigate = useNavigate();
    const [description, setDescription] = useState('');
    const createOrder = useCreateOrder();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!description.trim()) return;

        try {
            await createOrder.mutateAsync({ description });
            navigate('/orders');
        } catch (error) {
            console.error('Failed to create order:', error);
        }
    };

    return (
        <div className="container mx-auto px-4 py-8 max-w-2xl">
            <h1 className="text-3xl font-bold mb-6">Создать новый заказ</h1>

            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                        Описание заказа
                    </label>
                    <textarea
                        id="description"
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        className="w-full border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        rows={4}
                        placeholder="Введите описание заказа..."
                        required
                        disabled={createOrder.isPending}
                    />
                </div>

                <div className="flex gap-3">
                    <button
                        type="submit"
                        disabled={createOrder.isPending || !description.trim()}
                        className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors disabled:opacity-50"
                    >
                        {createOrder.isPending ? 'Создание...' : 'Создать заказ'}
                    </button>
                    <button
                        type="button"
                        onClick={() => navigate('/orders')}
                        className="px-6 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 transition-colors"
                    >
                        Отмена
                    </button>
                </div>
            </form>
        </div>
    );
};