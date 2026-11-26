import { apiConfig, requireEndpoint } from '../../../Api_Config/apiConfig';
import { httpClient } from '../../../Api_Config/httpClient';
import { contractError, isRecord, readString } from '../../../utilities/guards';

function displayName(user) {
    const name = [readString(user, 'firstName'), readString(user, 'lastName')].filter(Boolean).join(' ');
    return name || readString(user, 'userName');
}

function mapSession(value) {
    if (!isRecord(value) || !isRecord(value.user)) {
        throw contractError('Login');
    }
    const user = value.user;
    const accessToken = readString(value, 'accessToken');
    const name = displayName(user);
    if (!accessToken || !name) {
        throw contractError('Login');
    }
    const headOfficeCode = readString(user, 'headOfficeCode');
    const branchCode = readString(user, 'branchCode');
    // Portal accounts come back without a head office code (HEAD_OFFICE_CODE claim is empty).
    const portalUser = user.isPortalUser === true || !headOfficeCode;
    const expiresInMinutes = typeof value.expiresInMinutes === 'number' ? value.expiresInMinutes : 0;
    const headOfficeName = readString(user, 'headOfficeName') || (portalUser ? 'Portal' : headOfficeCode || 'Head Office');
    const branchOfficeName = readString(user, 'branchOfficeName') || (branchCode || (portalUser ? 'Portal' : 'Head Office'));
    return {
        headOfficeId: headOfficeCode || 'portal',
        headOfficeName,
        branchOfficeId: branchCode || 'portal',
        branchOfficeName,
        displayName: name,
        userName: readString(user, 'userName') || '',
        roleId: typeof user.roleId === 'number' ? user.roleId : 0,
        userContext: readString(user, 'userContext') || '',
        accessToken,
        tokenType: readString(value, 'tokenType') || 'Bearer',
        expiresInMinutes,
        expiresAt: expiresInMinutes > 0 ? Date.now() + expiresInMinutes * 60000 : 0,
        passwordChangeRequired: value.passwordChangeRequired === true || user.passwordChangeRequired === true,
    };
}

export const authApiRepository = {
    async login(request) {
        const path = requireEndpoint(apiConfig.endpoints.login, 'Login');
        const payload = await httpClient.post(path, {
            headOfficeCode: request.headOfficeCode.trim(),
            userName: request.userName.trim(),
            password: request.password,
        });
        return mapSession(payload);
    },
    // POST /api/auth/logout with the bearer token. The token is not revoked server-side
    // (it stays valid until expiry), so the caller always discards it locally as well.
    async logout(session) {
        const path = requireEndpoint(apiConfig.endpoints.logout, 'Logout');
        const token = session && readString(session, 'accessToken');
        if (!token) {
            return;
        }
        const tokenType = readString(session, 'tokenType') || 'Bearer';
        await httpClient.post(path, {}, {
            Authorization: `${tokenType} ${token}`,
        });
    },
};
