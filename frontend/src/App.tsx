import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import { QueryProvider } from './providers/QueryProvider';
import { OrdersPage } from './pages/OrdersPage';
import { CreateOrderPage } from './pages/CreateOrderPage';
import { OrderDetailPage } from './pages/OrderDetailPage';
import { useSignalR } from './hooks/useSignalR';
import './styles/App.css';

function AppContent() {
    const { isConnected } = useSignalR();

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Навигация */}
            <nav className="bg-white shadow-md">
                <div className="container mx-auto px-4 py-3 flex justify-between items-center">
                    <Link to="/" className="text-xl font-bold text-blue-600">
                        Order Tracking
                    </Link>
                    <div className="flex items-center gap-4">
                        <span className="text-xs text-gray-400">
                            {isConnected ? '🟢' : '🔴'}
                        </span>
                        <Link
                            to="/orders"
                            className="text-gray-600 hover:text-blue-600 transition-colors"
                        >
                            Заказы
                        </Link>
                        <Link
                            to="/orders/create"
                            className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
                        >
                            Новый заказ
                        </Link>
                    </div>
                </div>
            </nav>

            {/* Контент */}
            <main>
                <Routes>
                    <Route path="/" element={<OrdersPage />} />
                    <Route path="/orders" element={<OrdersPage />} />
                    <Route path="/orders/create" element={<CreateOrderPage />} />
                    <Route path="/orders/:id" element={<OrderDetailPage />} />
                </Routes>
            </main>
        </div>
    );
}

function App() {
    return (
        <QueryProvider>
            <Router>
                <AppContent />
            </Router>
        </QueryProvider>
    );
}

export default App;