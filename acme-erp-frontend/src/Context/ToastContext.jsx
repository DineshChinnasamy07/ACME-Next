import { createContext, useCallback, useContext, useMemo, useState } from 'react';

const ToastContext = createContext(null);

const maxToasts = 3;
const toastIcons = { success: '✓', error: '✕', warning: '!', info: 'i' };
const toastTitles = { success: 'Success', error: 'Error', warning: 'Warning', info: 'Information' };

export function ToastProvider({ children }) {
    const [toasts, setToasts] = useState([]);

    const dismissToast = useCallback((id) => {
        setToasts((current) => current.filter((toast) => toast.id !== id));
    }, []);

    const pushToast = useCallback((toast) => {
        const id = `${Date.now()}-${Math.random().toString(16).slice(2)}`;
        const next = { id, tone: 'success', ...toast };
        setToasts((current) => {
            // The same message replaces the earlier one instead of stacking, and only the newest few stay.
            const kept = current.filter((item) => !(item.message === next.message && item.tone === next.tone));
            return [...kept, next].slice(-maxToasts);
        });
        window.setTimeout(() => dismissToast(id), 4500);
    }, [dismissToast]);

    const value = useMemo(() => ({ pushToast }), [pushToast]);

    return (
        <ToastContext.Provider value={value}>
            {children}
            <div className="toast-stack" aria-live="polite">
                {toasts.map((toast) => (
                    <div key={toast.id} className={`toast toast-${toast.tone}`} role={toast.tone === 'error' ? 'alert' : 'status'}>
                        <span className="toast-icon" aria-hidden="true">{toastIcons[toast.tone] || toastIcons.info}</span>
                        <div className="toast-body">
                            <strong className="toast-title">{toast.title || toastTitles[toast.tone] || toastTitles.info}</strong>
                            <p>{toast.message}</p>
                        </div>
                        <button type="button" className="toast-close" aria-label="Dismiss notification" onClick={() => dismissToast(toast.id)}>
                            ×
                        </button>
                        <span className="toast-progress" aria-hidden="true"/>
                    </div>
                ))}
            </div>
        </ToastContext.Provider>
    );
}

export function useToast() {
    const value = useContext(ToastContext);
    if (!value) {
        throw new Error('useToast must be used within ToastProvider.');
    }
    return value;
}
