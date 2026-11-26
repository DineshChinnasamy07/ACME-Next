import { useCallback, useEffect, useState } from 'react';
import { Input, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { projectCategoryApi, toOptions } from '../services/projectApi';

/*
 * Data › Finance Masters › Project Category (Head Office). API: Acme.Masters.Api api/projectcategories.
 * Fields and rules follow the legacy ACCPPORTAL ProjectCategoryView.aspx / ProjectCategoryAdd.aspx
 * screens and the backend ProjectCategoryRepository (name required, ≤ 30, unique; delete blocked
 * while a project uses the category).
 */

const emptyCategory = { projectCatogoryId: 0, projectCatogoryName: '', groupId: '', itrGroupId: '' };

function idValue(value) {
    return value && Number(value) > 0 ? String(value) : '';
}

const columns = [
    { key: 'projectCatogoryName', header: 'Project Category', wrap: true },
    { key: 'group', header: 'Group', wrap: true },
    { key: 'itrGroup', header: 'ITR Group', wrap: true },
];

export function ProjectCategoryPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [groups, setGroups] = useState([]);
    const [itrGroups, setItrGroups] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [form, setForm] = useState(null);
    const [errors, setErrors] = useState({});
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            const [categories, groupRows, itrRows] = await Promise.all([
                projectCategoryApi.fetchCategories(session),
                projectCategoryApi.fetchGroups(session).catch(() => []),
                projectCategoryApi.fetchItrGroups(session).catch(() => []),
            ]);
            setRows(categories);
            setGroups(toOptions(groupRows));
            setItrGroups(toOptions(itrRows));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    function set(field) {
        return (value) => {
            setForm((current) => ({ ...current, [field]: value }));
            setErrors((current) => ({ ...current, [field]: undefined }));
        };
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setForm({ ...emptyCategory });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await projectCategoryApi.fetchCategory(session, row.projectCatogoryId);
            setForm({
                projectCatogoryId: detail.projectCatogoryId,
                projectCatogoryName: detail.projectCatogoryName || '',
                groupId: idValue(detail.projectCatogoryGroupId),
                itrGroupId: idValue(detail.projectCatogoryItrgroupId),
            });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Legacy ProjectCategoryAdd: name required (max 30), Group and ITR Group required.
    // Group / ITR Group are only enforced when the lookup has entries (backend treats 0 as none / default 1).
    function validate() {
        const next = {};
        const name = form.projectCatogoryName.trim();
        if (!name) next.projectCatogoryName = 'Enter the project category.';
        else if (name.length > 30) next.projectCatogoryName = 'Project category cannot exceed 30 characters.';
        else if (rows.some((row) => row.projectCatogoryId !== form.projectCatogoryId && (row.projectCatogoryName || '').trim().toLowerCase() === name.toLowerCase())) {
            next.projectCatogoryName = 'This project category already exists.';
        }
        if (groups.length > 0 && !form.groupId) next.groupId = 'Select the group.';
        if (itrGroups.length > 0 && !form.itrGroupId) next.itrGroupId = 'Select the ITR group.';
        return next;
    }

    async function save() {
        const next = validate();
        setErrors(next);
        if (Object.keys(next).length > 0) {
            return;
        }
        setBusy(true);
        setFormError('');
        try {
            const request = {
                projectCatogoryId: form.projectCatogoryId,
                projectCatogoryName: form.projectCatogoryName.trim(),
                generalateCategoryId: form.groupId ? Number(form.groupId) : 0,
                itrGroupId: form.itrGroupId ? Number(form.itrGroupId) : 0,
            };
            const result = await projectCategoryApi.saveCategory(session, request);
            pushToast({ message: (result && result.message) || 'Project category saved.' });
            setForm(null);
            await load();
        }
        catch (error) {
            setFormError(errorMessage(error));
        }
        finally {
            setBusy(false);
        }
    }

    async function remove(row) {
        if (!window.confirm(`Delete project category ${row.projectCatogoryName}?`)) {
            return;
        }
        try {
            const result = await projectCategoryApi.deleteCategory(session, row.projectCatogoryId);
            pushToast({ message: (result && result.message) || 'Project category deleted.' });
            setRows((current) => current.filter((item) => item.projectCatogoryId !== row.projectCatogoryId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.projectCatogoryId === 0;

    return (<section className="lg-page" aria-label="Project Category">
      <PageBar title="Project Category" addLabel="Add Project Category" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.projectCatogoryId} loading={loading} error={loadError} search={search} searchKeys={['projectCatogoryName', 'group', 'itrGroup']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add Project Category' : 'Edit Project Category'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="pc-name" label="Project Category" required value={form.projectCatogoryName} error={errors.projectCatogoryName} onChange={set('projectCatogoryName')}/>
          <Select id="pc-group" label="Group" required={groups.length > 0} value={form.groupId} options={groups} placeholder="Select group" error={errors.groupId} onChange={set('groupId')}/>
          <Select id="pc-itr" label="ITR Group" required={itrGroups.length > 0} value={form.itrGroupId} options={itrGroups} placeholder="Select ITR group" error={errors.itrGroupId} onChange={set('itrGroupId')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
