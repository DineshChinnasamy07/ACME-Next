import { HttpError, httpClient } from '../../../Api_Config/httpClient';
import { notifyUnauthorized } from '../../../utilities/authEvents';

// Bearer header from the signed-in session.
export function authHeaders(session) {
    const token = session && session.accessToken ? session.accessToken : '';
    if (!token) {
        throw new Error('Your session has ended. Please sign in again.');
    }
    return { Authorization: `${session.tokenType || 'Bearer'} ${token}` };
}

// Friendlier text for status codes that come back without a server message.
function friendly(error) {
    // 401 = token expired or rejected: end the session (AuthContext listens) and go to login.
    if (error instanceof HttpError && error.status === 401) {
        notifyUnauthorized();
    }
    if (error instanceof HttpError && /^Request failed with status/.test(error.message)) {
        if (error.status === 401) {
            return new HttpError('Your session has ended. Please sign in again.', 401);
        }
        if (error.status === 403) {
            return new HttpError('You do not have access to this screen.', 403);
        }
        if (error.status === 404) {
            return new HttpError('The record was not found.', 404);
        }
    }
    return error;
}

async function call(promise) {
    try {
        return await promise;
    }
    catch (error) {
        throw friendly(error);
    }
}

// Small client bound to one service base URL and the current session.
export function serviceClient(baseUrl, session) {
    const headers = () => authHeaders(session);
    return {
        get: (path) => call(httpClient.get(path, undefined, headers(), baseUrl)),
        post: (path, body) => call(httpClient.post(path, body ?? {}, headers(), baseUrl)),
        put: (path, body) => call(httpClient.put(path, body ?? {}, headers(), baseUrl)),
        delete: (path) => call(httpClient.delete(path, headers(), baseUrl)),
    };
}

export function asList(value) {
    return Array.isArray(value) ? value : [];
}

// Portal user = head office code empty or "portal" (same rule as the HEAD_OFFICE_CODE claim).
export function isPortalSession(session) {
    const code = session && session.headOfficeId ? String(session.headOfficeId).trim().toLowerCase() : '';
    return code === '' || code === 'portal';
}

// USER_ROLE id of the built-in "Branch Admin" role (legacy UserType.BranchOffice = 2).
export const BRANCH_ADMIN_ROLE_ID = 2;

// Branch office user = head office login that either has a branch code (BRANCH_CODE claim)
// or has the Branch Admin role (some users were created with that role but no branch).
// The session stores 'portal' as branchOfficeId when the user has no branch.
export function isBranchSession(session) {
    if (!session || isPortalSession(session)) {
        return false;
    }
    const branch = session.branchOfficeId ? String(session.branchOfficeId).trim().toLowerCase() : '';
    const hasBranch = branch !== '' && branch !== 'portal';
    return hasBranch || session.roleId === BRANCH_ADMIN_ROLE_ID;
}
