import React from 'react';
import { OrderList } from '../components/orders/OrderList';

export const OrdersPage: React.FC = () => {
    return (
        <div className="container mx-auto px-4 py-8">
            <OrderList />
        </div>
    );
};