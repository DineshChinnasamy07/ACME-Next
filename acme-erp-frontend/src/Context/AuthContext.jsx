import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react';
import { authService } from '../services/dataProvider';
import { onUnauthorized, writeSignOutNotice } from '../utilities/authEvents';
import { clearSession, readSession, writeSession } from '../utilities/session';

const AuthContext = createContext(null);

export const SIGNED_OUT = 'You have signed out successfully.';
export const SESSION_EXPIRED = 'Your session has expired. Please sign in again.';

// Session = login response from POST /api/auth/login (token + user), kept in sessionStorage.
// Logout calls POST /api/auth/logout, then always discards the token locally — even when the
// call fails (server down, token already expired) the user is never left stuck signed in.
export function AuthProvider({ children }) {
    const [session, setSession] = useState(() => readSession());
    const sessionRef = useRef(session);
    sessionRef.current = session;

    const endSession = useCallback((notice) => {
        clearSession();
        writeSignOutNotice(notice);
        setSession(null);
    }, []);

    const login = useCallback(async (request) => {
        const nextSession = await authService.login(request);
        writeSession(nextSession);
        setSession(nextSession);
        return nextSession;
    }, []);

    const logout = useCallback(async () => {
        const current = sessionRef.current;
        try {
            await authService.logout(current);
        }
        catch {
            // Logout is informational on the server (token is stateless); ignore failures.
        }
        finally {
            endSession(SIGNED_OUT);
        }
    }, [endSession]);

    // After a successful change of an automatic/reset password.
    const markPasswordChanged = useCallback(() => {
        setSession((current) => {
            if (!current) {
                return current;
            }
            const next = { ...current, passwordChangeRequired: false };
            writeSession(next);
            return next;
        });
    }, []);

    // Any API answering 401 means the token is no longer accepted.
    useEffect(() => onUnauthorized(() => {
        if (sessionRef.current) {
            endSession(SESSION_EXPIRED);
        }
    }), [endSession]);

    // Sign out automatically when the token's lifetime (expiresInMinutes) runs out.
    useEffect(() => {
        if (!session || !(session.expiresAt > 0)) {
            return undefined;
        }
        const remaining = session.expiresAt - Date.now();
        if (remaining <= 0) {
            endSession(SESSION_EXPIRED);
            return undefined;
        }
        // setTimeout caps at ~24.8 days; tokens are far shorter, but guard anyway.
        const timer = setTimeout(() => endSession(SESSION_EXPIRED), Math.min(remaining, 2147483647));
        return () => clearTimeout(timer);
    }, [session, endSession]);

    const value = useMemo(() => ({ session, login, logout, markPasswordChanged }), [session, login, logout, markPasswordChanged]);
    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const value = useContext(AuthContext);
    if (!value) {
        throw new Error('useAuth must be used within AuthProvider.');
    }
    return value;
}
