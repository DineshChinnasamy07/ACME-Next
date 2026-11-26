import { useCallback, useEffect, useState } from 'react';
import { Input, PasswordInput, SearchableSelect } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FieldSection, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { gridDate, isoDay, lockVoucherApi, requestDate } from '../services/periodLockApi';

/*
 * Data › Finance Masters › Lock Vouchers (Head Office login).
 * Mirrors legacy LockVoucherView.aspx / LockVoucherAdd.aspx on api/lockvouchers.
 * One lock = one branch + one project + date range + password / hint. On Add several projects of the
 * branch can be ticked; one lock is saved per project. On Edit the project is a single choice.
 * Server rules: branch and project required, Date From not in the future, Date To not before Date From,
 * password and hint required for a new lock (blank on edit keeps the stored values).
 */

const HINT_MAX = 25;
const REASON_MAX = 250;

const emptyLock = {
    lockTransId: 0, branchId: '', projectId: '', projectIds: [], dateFrom: '', dateTo: '',
    password: '', passwordHint: '', reason: '',
};

function today() {
    const now = new Date();
    const pad = (value) => String(value).padStart(2, '0');
    return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}

function TextArea({ id, label, value, onChange, error, rows = 3 }) {
    return (<div className="field">
      <label htmlFor={id}>{label}</label>
      <textarea id={id} className={error ? 'control lg-textarea is-invalid' : 'control lg-textarea'} rows={rows} value={value} onChange={(event) => onChange(event.target.value)}/>
      {error ? <p className="field-error">{error}</p> : null}
    </div>);
}

const columns = [
    { key: 'branchOffice', header: 'Branch Office', wrap: true },
    { key: 'project', header: 'Project', wrap: true },
    { key: 'dateFromText', header: 'Date From', width: '95px' },
    { key: 'dateToText', header: 'Date To', width: '95px' },
    { key: 'reason', header: 'Reason', wrap: true },
    { key: 'lockedText', header: 'Locked', width: '70px' },
];

export function LockVoucherPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [branches, setBranches] = useState([]);
    const [projects, setProjects] = useState([]);
    const [projectsLoading, setProjectsLoading] = useState(false);
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
            const locks = await lockVoucherApi.fetchLocks(session);
            setRows(locks.map((row) => ({
                ...row,
                dateFromText: gridDate(row.dateFrom),
                dateToText: gridDate(row.dateTo),
                lockedText: row.locked ? 'Yes' : 'No',
            })));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    const branchId = form ? form.branchId : '';
    const lockTransId = form ? form.lockTransId : 0;

    // Projects of the chosen branch (reloaded when the branch changes).
    useEffect(() => {
        if (!branchId) {
            setProjects([]);
            return undefined;
        }
        let cancelled = false;
        setProjectsLoading(true);
        lockVoucherApi.fetchProjects(session, branchId, lockTransId)
            .then((list) => { if (!cancelled) setProjects(list); })
            .catch((error) => {
                if (!cancelled) {
                    setProjects([]);
                    setFormError(errorMessage(error));
                }
            })
            .finally(() => { if (!cancelled) setProjectsLoading(false); });
        return () => { cancelled = true; };
    }, [session, branchId, lockTransId]);

    async function ensureBranches() {
        if (branches.length > 0) {
            return;
        }
        setBranches(await lockVoucherApi.fetchBranches(session));
    }

    function set(field) {
        return (value) => {
            setForm((current) => {
                const next = { ...current, [field]: value };
                if (field === 'branchId' && value !== current.branchId) {
                    next.projectId = '';
                    next.projectIds = [];
                }
                return next;
            });
            setErrors((current) => ({ ...current, [field]: undefined, ...(field === 'branchId' ? { project: undefined } : {}) }));
        };
    }

    function toggleProject(id, checked) {
        setForm((current) => {
            const others = current.projectIds.filter((item) => item !== id);
            return { ...current, projectIds: checked ? [...others, id] : others };
        });
        setErrors((current) => ({ ...current, project: undefined }));
    }

    function toggleAllProjects(checked) {
        setForm((current) => ({ ...current, projectIds: checked ? projects.map((project) => project.id) : [] }));
        setErrors((current) => ({ ...current, project: undefined }));
    }

    async function openAdd() {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            await ensureBranches();
            setForm({ ...emptyLock });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const [detail] = await Promise.all([lockVoucherApi.fetchLock(session, row.lockTransId), ensureBranches()]);
            setForm({
                ...emptyLock,
                lockTransId: detail.lockTransId,
                branchId: detail.branchId ? String(detail.branchId) : '',
                projectId: detail.projectId ? String(detail.projectId) : '',
                dateFrom: isoDay(detail.dateFrom),
                dateTo: isoDay(detail.dateTo),
                reason: detail.reason || '',
            });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Legacy LockVoucherAdd rules (+ server rules from LockVoucherRepository).
    function validate() {
        const next = {};
        const isNew = form.lockTransId === 0;
        if (!form.branchId) next.branchId = 'Select the branch.';
        if (isNew ? form.projectIds.length === 0 : !form.projectId) next.project = 'Select the project.';
        if (!form.dateFrom) next.dateFrom = 'Select the Date From.';
        else if (form.dateFrom > today()) next.dateFrom = 'Date From cannot be a future date.';
        if (!form.dateTo) next.dateTo = 'Select the Date To.';
        else if (form.dateFrom && form.dateTo < form.dateFrom) next.dateTo = 'Date To cannot be before Date From.';
        if (isNew && !form.password.trim()) next.password = 'Enter the password.';
        if (isNew && !form.passwordHint.trim()) next.passwordHint = 'Enter the password hint.';
        else if (form.passwordHint.trim().length > HINT_MAX) next.passwordHint = `Hint can be at most ${HINT_MAX} characters.`;
        if (form.reason.trim().length > REASON_MAX) next.reason = `Reason can be at most ${REASON_MAX} characters.`;
        return next;
    }

    function requestFor(projectId) {
        return {
            lockTransId: form.lockTransId,
            branchId: Number(form.branchId),
            projectId: Number(projectId),
            dateFrom: requestDate(form.dateFrom),
            dateTo: requestDate(form.dateTo),
            password: form.password.trim(),
            passwordHint: form.passwordHint.trim(),
            reason: form.reason.trim(),
        };
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
            if (form.lockTransId !== 0) {
                const result = await lockVoucherApi.saveLock(session, requestFor(form.projectId));
                pushToast({ message: (result && result.message) || 'Lock voucher saved.' });
                setForm(null);
                await load();
                return;
            }
            // One lock per ticked project; keep the failed ones ticked so the user can retry.
            const failed = [];
            let saved = 0;
            let lastMessage = '';
            for (const projectId of form.projectIds) {
                try {
                    const result = await lockVoucherApi.saveLock(session, requestFor(projectId));
                    saved += 1;
                    lastMessage = (result && result.message) || lastMessage;
                }
                catch (error) {
                    const project = projects.find((item) => item.id === projectId);
                    failed.push({ projectId, text: `${project ? project.name : projectId}: ${errorMessage(error)}` });
                }
            }
            if (saved > 0) {
                pushToast({ message: saved === 1 ? (lastMessage || 'Lock voucher saved.') : `${saved} lock vouchers saved.` });
            }
            if (failed.length === 0) {
                setForm(null);
            }
            else {
                setForm((current) => ({ ...current, projectIds: failed.map((item) => item.projectId) }));
                setFormError(failed.map((item) => item.text).join(' | '));
            }
            if (saved > 0) {
                await load();
            }
        }
        catch (error) {
            setFormError(errorMessage(error));
        }
        finally {
            setBusy(false);
        }
    }

    async function remove(row) {
        if (!window.confirm(`Delete the voucher lock of ${row.project} (${row.branchOffice}), ${row.dateFromText} - ${row.dateToText}?`)) {
            return;
        }
        try {
            const result = await lockVoucherApi.deleteLock(session, row.lockTransId);
            pushToast({ message: (result && result.message) || 'Lock voucher deleted.' });
            setRows((current) => current.filter((item) => item.lockTransId !== row.lockTransId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.lockTransId === 0;
    const branchOptions = branches.map((branch) => ({ value: String(branch.id), label: branch.name }));
    const projectOptions = projects.map((project) => ({ value: String(project.id), label: project.name }));
    const ticked = form ? form.projectIds.length : 0;
    const allTicked = projects.length > 0 && ticked === projects.length;

    return (<section className="lg-page" aria-label="Lock Vouchers">
      <PageBar title="Lock Vouchers" addLabel="Add Lock Voucher" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={() => void openAdd()}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.lockTransId} loading={loading} error={loadError} search={search} searchKeys={['branchOffice', 'project', 'dateFromText', 'dateToText', 'reason', 'lockedText']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add Lock Voucher' : 'Edit Lock Voucher'} isOpen={Boolean(form)} busy={busy} error={formError} wide onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <SearchableSelect id="lv-branch" label="Branch" required value={form.branchId} options={branchOptions} placeholder="Select branch" error={errors.branchId} onChange={set('branchId')}/>
          {isNew ? null : <SearchableSelect id="lv-project" label="Project" required value={form.projectId} options={projectOptions} placeholder={projectsLoading ? 'Loading…' : 'Select project'} disabled={!form.branchId || projectsLoading} error={errors.project} onChange={(value) => { set('projectId')(value); setErrors((current) => ({ ...current, project: undefined })); }}/>}
          {isNew ? <fieldset className="lg-rights-module" style={{ gridColumn: '1 / -1' }}>
            <legend>
              <label className="lg-check">
                <input type="checkbox" checked={allTicked} disabled={projects.length === 0} ref={(node) => { if (node) node.indeterminate = ticked > 0 && !allTicked; }} onChange={(event) => toggleAllProjects(event.target.checked)}/>
                <strong>Project<span className="req" aria-hidden="true">*</span>{ticked > 0 ? ` (${ticked} selected)` : ''}</strong>
              </label>
            </legend>
            {!form.branchId ? <p className="lg-hint">Select a branch to list its projects.</p> : null}
            {form.branchId && projectsLoading ? <p className="lg-hint">Loading…</p> : null}
            {form.branchId && !projectsLoading && projects.length === 0 ? <p className="lg-hint">No projects of this branch are available to lock.</p> : null}
            {!projectsLoading && projects.length > 0 ? <div className="lg-rights-list" style={{ maxHeight: '180px', overflowY: 'auto' }}>
              {projects.map((project) => (<label key={project.id} className="lg-check">
                <input type="checkbox" checked={form.projectIds.includes(project.id)} onChange={(event) => toggleProject(project.id, event.target.checked)}/>
                {project.name}
              </label>))}
            </div> : null}
            {errors.project ? <p className="field-error">{errors.project}</p> : null}
          </fieldset> : null}

          <FieldSection title="Lock Period"/>
          <Input id="lv-from" label="Date From" required type="date" value={form.dateFrom} error={errors.dateFrom} onChange={set('dateFrom')}/>
          <Input id="lv-to" label="Date To" required type="date" value={form.dateTo} error={errors.dateTo} onChange={set('dateTo')}/>

          <FieldSection title="Password"/>
          <PasswordInput id="lv-password" label="Password" required={isNew} autoComplete="new-password" value={form.password} error={errors.password} onChange={set('password')}/>
          <Input id="lv-hint" label="Hint" required={isNew} autoComplete="off" value={form.passwordHint} error={errors.passwordHint} onChange={set('passwordHint')}/>
          {isNew ? null : <p className="lg-hint" style={{ gridColumn: '1 / -1' }}>Leave Password and Hint blank to keep the current ones.</p>}

          <FieldSection title="Other Details"/>
          <TextArea id="lv-reason" label="Reasons" value={form.reason} error={errors.reason} onChange={set('reason')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
