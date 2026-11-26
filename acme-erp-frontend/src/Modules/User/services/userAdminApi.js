import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Auth service (Acme.Auth.Api): users, roles, rights, change password.
function auth(session) {
    return serviceClient(apiConfig.apiBaseUrl, session);
}

export const userApi = {
    async fetchUsers(session) {
        return asList(await auth(session).get('/api/users'));
    },
    async fetchRoleOptions(session) {
        return asList(await auth(session).get('/api/users/roles'));
    },
    fetchUser(session, userId) {
        return auth(session).get(`/api/users/${userId}`);
    },
    saveUser(session, request) {
        return auth(session).post('/api/users', request);
    },
    updateStatus(session, userId, status) {
        return auth(session).put(`/api/users/${userId}/status`, { status });
    },
    deleteUser(session, userId) {
        return auth(session).delete(`/api/users/${userId}`);
    },
    resetPassword(session, userId, newPassword, confirmPassword) {
        return auth(session).post(`/api/users/${userId}/reset-password`, { newPassword, confirmPassword });
    },
};

export const roleApi = {
    async fetchRoles(session) {
        return asList(await auth(session).get('/api/roles'));
    },
    saveRole(session, userRoleId, userRoleName) {
        return auth(session).post('/api/roles', { userRoleId, userRoleName });
    },
    deleteRole(session, userRoleId) {
        return auth(session).delete(`/api/roles/${userRoleId}`);
    },
};

export const rightsApi = {
    async fetchRoles(session) {
        return asList(await auth(session).get('/api/rights/roles'));
    },
    async fetchRights(session, roleId) {
        return asList(await auth(session).get(`/api/rights/${roleId}`));
    },
    saveRights(session, roleId, rights) {
        return auth(session).put('/api/rights', { roleId, rights });
    },
};

export const passwordApi = {
    changePassword(session, currentPassword, newPassword, confirmPassword) {
        return auth(session).post('/api/auth/change-password', { currentPassword, newPassword, confirmPassword });
    },
};
