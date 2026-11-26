// Lets API helpers report "the server rejected our token" (HTTP 401) without importing React.
// AuthContext subscribes and ends the session; the login page then shows why.
const listeners = new Set();

export function onUnauthorized(listener) {
    listeners.add(listener);
    return () => listeners.delete(listener);
}

export function notifyUnauthorized() {
    listeners.forEach((listener) => listener());
}

// One-shot note for the login page ("You have signed out." / "Your session has expired.").
const NOTICE_KEY = 'acme.erp.signout';

export function writeSignOutNotice(message) {
    try {
        sessionStorage.setItem(NOTICE_KEY, message);
    }
    catch {
        // storage unavailable: the login page simply shows no note
    }
}

export function takeSignOutNotice() {
    try {
        const message = sessionStorage.getItem(NOTICE_KEY) || '';
        sessionStorage.removeItem(NOTICE_KEY);
        return message;
    }
    catch {
        return '';
    }
}
