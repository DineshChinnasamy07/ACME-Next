function envValue(value) {
    return (value ?? '').trim();
}

export const apiConfig = {
    apiBaseUrl: envValue(import.meta.env.VITE_API_BASE_URL),
    organizationApiBaseUrl: envValue(import.meta.env.VITE_ORG_API_BASE_URL) || envValue(import.meta.env.VITE_API_BASE_URL),
    mastersApiBaseUrl: envValue(import.meta.env.VITE_MASTERS_API_BASE_URL) || envValue(import.meta.env.VITE_API_BASE_URL),
    endpoints: {
        login: envValue(import.meta.env.VITE_API_LOGIN_PATH),
        logout: envValue(import.meta.env.VITE_API_LOGOUT_PATH),
    },
};

export function requireEndpoint(path, name) {
    if (!apiConfig.apiBaseUrl) {
        throw new Error('API base URL is not configured. Set VITE_API_BASE_URL.');
    }
    if (!path) {
        throw new Error(`${name} endpoint is not configured.`);
    }
    return path;
}
