import { apiConfig } from './apiConfig';
import { isRecord } from '../utilities/guards';
export class HttpError extends Error {
    constructor(message, status) {
        super(message);
        this.name = 'HttpError';
        this.status = status;
    }
}
function joinUrl(base, path) {
    return `${base.replace(/\/+$/, '')}/${path.replace(/^\/+/, '')}`;
}
async function readErrorDetail(response) {
    try {
        const body = await response.json();
        if (isRecord(body) && typeof body.message === 'string' && body.message.trim()) {
            return body.message.trim();
        }
    }
    catch {
        return '';
    }
    return '';
}
export async function httpRequest(options) {
    const baseUrl = (options.baseUrl || apiConfig.apiBaseUrl || '').trim();
    if (!baseUrl) {
        throw new Error('API base URL is not configured. Set VITE_API_BASE_URL.');
    }
    if (!options.path) {
        throw new Error('API endpoint path is not configured.');
    }
    let url;
    try {
        url = new URL(joinUrl(baseUrl, options.path));
    }
    catch {
        throw new Error('API base URL must be an absolute URL, for example http://localhost:5000/api.');
    }
    if (options.query) {
        Object.entries(options.query).forEach(([key, value]) => {
            url.searchParams.set(key, value);
        });
    }
    let response;
    try {
        response = await fetch(url.toString(), {
        method: options.method,
        headers: {
            Accept: 'application/json',
            ...(options.body !== undefined ? { 'Content-Type': 'application/json' } : {}),
            ...(options.headers || {}),
        },
        body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
        });
    }
    catch {
        // Network error, server stopped, or CORS refusal: fetch gives no status.
        throw new HttpError('Cannot reach the server. Check that the service is running and try again.', 0);
    }
    if (!response.ok) {
        const detail = await readErrorDetail(response);
        throw new HttpError(detail || `Request failed with status ${response.status}.`, response.status);
    }
    if (response.status === 204) {
        return undefined;
    }
    const text = await response.text();
    if (!text) {
        return undefined;
    }
    try {
        return JSON.parse(text);
    }
    catch {
        throw new Error('The server response was not valid JSON.');
    }
}
export const httpClient = {
    get(path, query, headers, baseUrl) {
        return httpRequest({ method: 'GET', path, query, headers, baseUrl });
    },
    post(path, body, headers, baseUrl) {
        return httpRequest({ method: 'POST', path, body, headers, baseUrl });
    },
    put(path, body, headers, baseUrl) {
        return httpRequest({ method: 'PUT', path, body, headers, baseUrl });
    },
    delete(path, headers, baseUrl) {
        return httpRequest({ method: 'DELETE', path, headers, baseUrl });
    },
};
