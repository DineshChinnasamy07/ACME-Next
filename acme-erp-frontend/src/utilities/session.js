import { isRecord, readString } from './guards';
const SESSION_KEY = 'acme.erp.session';
function isSession(value) {
    if (!isRecord(value)) {
        return false;
    }
    return Boolean(readString(value, 'headOfficeName') &&
        readString(value, 'branchOfficeName') &&
        readString(value, 'displayName') &&
        readString(value, 'accessToken'));
}
export function readSession() {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (!raw) {
        return null;
    }
    try {
        const parsed = JSON.parse(raw);
        if (!isSession(parsed)) {
            sessionStorage.removeItem(SESSION_KEY);
            return null;
        }
        // Token lifetime comes from the login response (expiresInMinutes); drop it once passed.
        const expiresAt = typeof parsed.expiresAt === 'number' ? parsed.expiresAt : 0;
        if (expiresAt > 0 && expiresAt <= Date.now()) {
            sessionStorage.removeItem(SESSION_KEY);
            return null;
        }
        return {
            headOfficeId: readString(parsed, 'headOfficeId') || '',
            headOfficeName: parsed.headOfficeName,
            branchOfficeId: readString(parsed, 'branchOfficeId') || '',
            branchOfficeName: parsed.branchOfficeName,
            displayName: parsed.displayName,
            userName: readString(parsed, 'userName') || '',
            roleId: typeof parsed.roleId === 'number' ? parsed.roleId : 0,
            userContext: readString(parsed, 'userContext') || '',
            accessToken: parsed.accessToken,
            tokenType: readString(parsed, 'tokenType') || 'Bearer',
            expiresInMinutes: typeof parsed.expiresInMinutes === 'number' ? parsed.expiresInMinutes : 0,
            expiresAt,
            passwordChangeRequired: parsed.passwordChangeRequired === true,
        };
    }
    catch {
        sessionStorage.removeItem(SESSION_KEY);
        return null;
    }
}
export function writeSession(session) {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify({
        headOfficeId: session.headOfficeId,
        headOfficeName: session.headOfficeName,
        branchOfficeId: session.branchOfficeId,
        branchOfficeName: session.branchOfficeName,
        displayName: session.displayName,
        userName: session.userName,
        roleId: session.roleId,
        userContext: session.userContext,
        accessToken: session.accessToken,
        tokenType: session.tokenType,
        expiresInMinutes: session.expiresInMinutes,
        expiresAt: session.expiresAt || 0,
        passwordChangeRequired: session.passwordChangeRequired,
    }));
}
export function clearSession() {
    sessionStorage.removeItem(SESSION_KEY);
}

// True when the stored session has passed its token expiry time.
export function isSessionExpired(session) {
    return Boolean(session && session.expiresAt > 0 && session.expiresAt <= Date.now());
}
