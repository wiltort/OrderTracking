import React from 'react';

interface ErrorMessageProps {
    error: Error | string;
    onRetry?: () => void;
}

export const ErrorMessage: React.FC<ErrorMessageProps> = ({ error, onRetry }) => {
    const message = typeof error === 'string' ? error : error.message;

    return (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded relative">
            <strong className="font-bold">Ошибка: </strong>
            <span className="block sm:inline">{message}</span>
            {onRetry && (
                <button
                    onClick={onRetry}
                    className="ml-4 px-3 py-1 bg-red-100 hover:bg-red-200 rounded text-sm"
                >
                    Повторить
                </button>
            )}
        </div>
    );
};