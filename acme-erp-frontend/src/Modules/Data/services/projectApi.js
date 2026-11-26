import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api): Head Office › Finance Masters › Project and Project Category.
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// api/projects
export const projectApi = {
    // [{ projectId, projectCode, projectName, divisionId, division, projectCategoryId, category,
    //    customerId, society, dateStarted, dateClosed, description }]
    async fetchProjects(session) {
        return asList(await masters(session).get('/api/projects'));
    },
    // Lookups: [{ id, name }]
    async fetchDivisions(session) {
        return asList(await masters(session).get('/api/projects/divisions'));
    },
    async fetchCategories(session) {
        return asList(await masters(session).get('/api/projects/categories'));
    },
    async fetchSocieties(session) {
        return asList(await masters(session).get('/api/projects/societies'));
    },
    // Entity row: { projectId, projectCode, project, divisionId, accountDate, dateStarted, dateClosed,
    //   closedBy, description, notes, projectCategoryId, deleteFlag, customerId, contributionId }
    fetchProject(session, projectId) {
        return masters(session).get(`/api/projects/${projectId}`);
    },
    // Request: { projectId, projectCode, projectName, divisionId, projectCategroyId, societyId,
    //   accountDate, startedOn, closedOn, description, notes } → { success, message, projectId }
    saveProject(session, request) {
        return masters(session).post('/api/projects', request);
    },
    deleteProject(session, projectId) {
        return masters(session).delete(`/api/projects/${projectId}`);
    },
};

// api/projectcategories
export const projectCategoryApi = {
    // [{ projectCatogoryId, projectCatogoryName, projectCatogoryGroupId, group, projectCatogoryItrgroupId, itrGroup }]
    async fetchCategories(session) {
        return asList(await masters(session).get('/api/projectcategories'));
    },
    // Lookups: [{ id, name }]
    async fetchGroups(session) {
        return asList(await masters(session).get('/api/projectcategories/groups'));
    },
    async fetchItrGroups(session) {
        return asList(await masters(session).get('/api/projectcategories/itrgroups'));
    },
    // Entity row: { projectCatogoryId, projectCatogoryName, projectCatogoryGroupId, projectCatogoryItrgroupId }
    fetchCategory(session, projectCatogoryId) {
        return masters(session).get(`/api/projectcategories/${projectCatogoryId}`);
    },
    // Request: { projectCatogoryId, projectCatogoryName, generalateCategoryId, itrGroupId }
    //   → { success, message, projectCatogoryId }
    saveCategory(session, request) {
        return masters(session).post('/api/projectcategories', request);
    },
    deleteCategory(session, projectCatogoryId) {
        return masters(session).delete(`/api/projectcategories/${projectCatogoryId}`);
    },
};

// { id, name } lookup rows → Select/SearchableSelect options.
export function toOptions(list) {
    return asList(list).map((item) => ({ value: String(item.id), label: item.name || '' }));
}

// API DateTime ("2019-04-01T00:00:00") → yyyy-mm-dd for <input type="date">.
export function toInputDate(value) {
    if (!value) {
        return '';
    }
    const text = String(value).slice(0, 10);
    return /^\d{4}-\d{2}-\d{2}$/.test(text) && !text.startsWith('0001') ? text : '';
}

// API DateTime → dd/mm/yyyy (legacy grid format).
export function toGridDate(value) {
    const text = toInputDate(value);
    if (!text) {
        return '';
    }
    const [year, month, day] = text.split('-');
    return `${day}/${month}/${year}`;
}

// api/mapprojects — Project Mapping: one project mapped to many branches (Data › Mapping › Project Mapping).
export const projectMappingApi = {
    // { projects: [{ projectId, projectCode, projectName, createdOn, description, mappedBranches }],
    //   branches / locations / societies / categories / divisions: [{ id, code, name }],
    //   users: [{ id, branchId, name, userName }] }
    fetchLookups(session) {
        return masters(session).get('/api/mapprojects/lookups');
    },
    // { projectId, isSaved, rows: [{ branchId, locationId, societyId, categoryId, divisionId, startDate, endDate, userId, hasTransactions }] }
    fetchMapping(session, projectId) {
        return masters(session).get(`/api/mapprojects/${projectId}`);
    },
    // Request: { projectId, rows: [...] } → { success, message, projectId }; 400 → { message, rowIndex }
    saveMapping(session, request) {
        return masters(session).post('/api/mapprojects', request);
    },
};
