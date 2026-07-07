import { useEffect, useRef, useState } from 'react';
import { signalRService } from '../api/signalRService';

export const useSignalR = () => {
    const [isConnected, setIsConnected] = useState(false);
    const mountedRef = useRef(false);
    const startPromiseRef = useRef<Promise<void> | null>(null);

    useEffect(() => {
        mountedRef.current = true;

        const connect = async () => {
            try {
                const promise = signalRService.start();
                startPromiseRef.current = promise;
                await promise;
                if (mountedRef.current) {
                    setIsConnected(true);
                }
            } catch (error) {
                console.error('Failed to connect to SignalR:', error);
                if (mountedRef.current) {
                    setIsConnected(false);
                }
            } finally {
                startPromiseRef.current = null;
            }
        };

        connect();

        return () => {
            mountedRef.current = false;
            // Wait for the start to complete before stopping
            // This prevents interrupting the negotiation handshake
            const promise = startPromiseRef.current;
            if (promise) {
                promise.finally(() => {
                    signalRService.stop();
                });
            } else {
                signalRService.stop();
            }
            setIsConnected(false);
        };
    }, []);

    return { isConnected, signalRService };
};