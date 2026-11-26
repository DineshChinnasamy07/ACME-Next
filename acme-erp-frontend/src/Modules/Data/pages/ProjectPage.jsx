import { useCallback, useEffect, useState } from 'react';
import { Input } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { projectApi, toGridDate, toInputDate } from '../services/projectApi';

/*
 * Data › Finance Masters › Project (Head Office). API: Acme.Masters.Api api/projects.
 * Project creation only: Project Name, Project Code, Description and Project Creation Date.
 * Society, Project Category, Division, Start / End Date, Branches, Locations and Users are set
 * later in Data › Mapping › Project Mapping.
 * Rules: code ≤ 15, name ≤ 100 and unique, description ≤ 100, creation date not in the future.
 * Delete is blocked by the backend when ledger balances exist for the project.
 */

// The mapping fields (societyId … notes) are not edited here; they are kept so an edit sends them back unchanged.
const emptyProject = {
    projectId: 0, projectCode: '', projectName: '', description: '', createdOn: '',
    societyId: '', projectCategoryId: '', divisionId: '', startedOn: '', closedOn: '', notes: '',
};

function text(value) {
    return value === null || value === undefined ? '' : String(value);
}

// Lookup id from the API (0 = none) → form value.
function idValue(value) {
    return value && Number(value) > 0 ? String(value) : '';
}

// Local date as yyyy-mm-dd (toISOString would shift to UTC).
function today() {
    const now = new Date();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${now.getFullYear()}-${month}-${day}`;
}

function TextArea({ id, label, value, onChange, error, rows = 2 }) {
    return (<div className="field">
      <label htmlFor={id}>{label}</label>
      <textarea id={id} className={error ? 'control lg-textarea is-invalid' : 'control lg-textarea'} rows={rows} value={value} onChange={(event) => onChange(event.target.value)}/>
      {error ? <p className="field-error">{error}</p> : null}
    </div>);
}

const columns = [
    { key: 'projectCode', header: 'Code', width: '90px' },
    { key: 'projectName', header: 'Project Name', wrap: true },
    { key: 'description', header: 'Description', wrap: true },
    { key: 'createdOn', header: 'Created On', width: '100px', render: (row) => toGridDate(row.createdOn || row.accountDate) },
    { key: 'mapping', header: 'Mapping', width: '110px', render: (row) => (row.society || row.category
        ? <span className="pj-status is-mapped">Mapped</span>
        : <span className="pj-status">Not mapped</span>) },
];

export function ProjectPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
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
            setRows(await projectApi.fetchProjects(session));
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
        setForm({ ...emptyProject, createdOn: today() });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await projectApi.fetchProject(session, row.projectId);
            setForm({
                projectId: detail.projectId,
                projectCode: text(detail.projectCode),
                projectName: text(detail.project),
                description: text(detail.description),
                createdOn: toInputDate(detail.createdOn || detail.accountDate),
                societyId: idValue(detail.customerId),
                projectCategoryId: idValue(detail.projectCategoryId),
                divisionId: idValue(detail.divisionId),
                startedOn: toInputDate(detail.dateStarted),
                closedOn: toInputDate(detail.dateClosed),
                notes: text(detail.notes),
            });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Creation needs only name, code and creation date; the rest belongs to Project Mapping.
    function validate() {
        const next = {};
        const code = form.projectCode.trim();
        const name = form.projectName.trim();
        if (!name) next.projectName = 'Enter the project name.';
        else if (name.length > 100) next.projectName = 'Project name cannot exceed 100 characters.';
        else if (rows.some((row) => row.projectId !== form.projectId && (row.projectName || '').trim().toLowerCase() === name.toLowerCase())) {
            next.projectName = 'A project with this name already exists.';
        }
        if (!code) next.projectCode = 'Enter the project code.';
        else if (code.length > 15) next.projectCode = 'Project code cannot exceed 15 characters.';
        if (form.description.trim().length > 100) next.description = 'Description cannot exceed 100 characters.';
        if (!form.createdOn) next.createdOn = 'Select the project creation date.';
        else if (form.createdOn > today()) next.createdOn = 'Creation date cannot be a future date.';
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
                projectId: form.projectId,
                projectCode: form.projectCode.trim().toUpperCase(),
                projectName: form.projectName.trim(),
                description: form.description.trim(),
                createdOn: form.createdOn,
                accountDate: form.createdOn || null,
                // Owned by Project Mapping: sent back unchanged (0 / null for a new project).
                // The backend still requires division + category; that rule moves to mapping in the backend phase.
                divisionId: form.divisionId ? Number(form.divisionId) : 0,
                projectCategroyId: form.projectCategoryId ? Number(form.projectCategoryId) : 0, // backend spelling
                societyId: form.societyId ? Number(form.societyId) : 0,
                startedOn: form.startedOn || null,
                closedOn: form.closedOn || null,
                notes: form.notes,
            };
            const result = await projectApi.saveProject(session, request);
            pushToast({ message: (result && result.message) || 'Project saved.' });
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
        if (!window.confirm(`Delete project ${row.projectName}?`)) {
            return;
        }
        try {
            const result = await projectApi.deleteProject(session, row.projectId);
            pushToast({ message: (result && result.message) || 'Project deleted.' });
            setRows((current) => current.filter((item) => item.projectId !== row.projectId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.projectId === 0;

    return (<section className="lg-page" aria-label="Project">
      <PageBar title="Project" addLabel="Add Project" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.projectId} loading={loading} error={loadError} search={search} searchKeys={['projectCode', 'projectName', 'description']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add Project' : 'Edit Project'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="pj-name" label="Project Name" required value={form.projectName} error={errors.projectName} onChange={set('projectName')}/>
          <Input id="pj-code" label="Project Code" required value={form.projectCode} error={errors.projectCode} onChange={set('projectCode')}/>
          <Input id="pj-created" label="Project Creation Date" required type="date" value={form.createdOn} error={errors.createdOn} onChange={set('createdOn')}/>
          <div/>
          <div style={{ gridColumn: '1 / -1' }}>
            <TextArea id="pj-desc" label="Description" rows={3} value={form.description} error={errors.description} onChange={set('description')}/>
          </div>
          <p className="pj-mapping-hint" style={{ gridColumn: '1 / -1' }}>Society, Project Category, Division, Start / End Date, Branches, Locations and Users are set in <strong>Mapping › Project Mapping</strong>.</p>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
