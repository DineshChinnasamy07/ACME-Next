import { useCallback, useEffect, useMemo, useState } from 'react';
import { Input, Modal, SearchableSelect, Select } from '../../../Common/components';
import { DeleteIcon, PlusIcon, RefreshIcon } from '../../../Common/components/Icons';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { projectMappingApi, toInputDate } from '../services/projectApi';

/*
 * Data › Mapping › Project Mapping (Head Office). API: Acme.Masters.Api api/mapprojects.
 *
 * Select a project, then add one row per branch office it is mapped to:
 *   # | Branch Office | Branch Location | Society | Project Category | Division | Start Date | End Date | User Incharge
 * Every cell is a lookup (or date) control. Preview shows the overall details, then one Save inserts / updates
 * all rows (PROJECT_BRANCH_MAPPING, kept in sync with legacy PROJECT_BRANCH and PROJECT_USER).
 * A branch that already has vouchers for the project cannot be removed, and its branch, society, category,
 * division and start date are locked. Built for 100+ rows: sticky header, horizontal scroll, the # and
 * Branch Office columns stay fixed.
 */

const LOCKED_FIELDS = ['societyId', 'categoryId', 'divisionId', 'startDate'];
const COPY_FIELDS = ['societyId', 'categoryId', 'divisionId', 'startDate', 'endDate'];
const emptyLookups = { projects: [], branches: [], locations: [], societies: [], categories: [], divisions: [], users: [] };

let rowSeed = 0;
function newKey() {
    rowSeed += 1;
    return `r${rowSeed}`;
}

function text(value) {
    return value === null || value === undefined || Number(value) <= 0 ? '' : String(value);
}

function options(list) {
    return (list || []).map((item) => ({ value: String(item.id), label: item.code ? `${item.code} - ${item.name}` : item.name }));
}

function labelOf(list, value) {
    return list.find((option) => option.value === value)?.label || '';
}

function gridDate(value) {
    if (!value) return '';
    const [year, month, day] = value.split('-');
    return `${day}/${month}/${year}`;
}

function LockIcon() {
    return (<svg className="pm-lock" viewBox="0 0 24 24" aria-hidden="true">
      <path fill="currentColor" d="M12 2a5 5 0 0 0-5 5v3H6a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8a2 2 0 0 0-2-2h-1V7a5 5 0 0 0-5-5zm-3 8V7a3 3 0 1 1 6 0v3H9z"/>
    </svg>);
}

// API row → grid row (select values are strings; a row with vouchers is locked).
function toGridRow(row) {
    return {
        key: newKey(),
        branchId: text(row.branchId),
        locationId: text(row.locationId),
        societyId: text(row.societyId),
        categoryId: text(row.categoryId),
        divisionId: text(row.divisionId),
        startDate: toInputDate(row.startDate),
        endDate: toInputDate(row.endDate),
        userId: text(row.userId),
        locked: Boolean(row.hasTransactions),
    };
}

export function ProjectMappingPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [lookups, setLookups] = useState(emptyLookups);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [projectId, setProjectId] = useState('');
    const [savedRows, setSavedRows] = useState([]);
    const [rows, setRows] = useState([]);
    const [isSaved, setIsSaved] = useState(false);
    const [dirty, setDirty] = useState(false);
    const [busy, setBusy] = useState(false);
    const [search, setSearch] = useState('');
    const [errors, setErrors] = useState({});
    const [preview, setPreview] = useState(false);

    const loadLookups = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            setLookups({ ...emptyLookups, ...(await projectMappingApi.fetchLookups(session)) });
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void loadLookups(); }, [loadLookups]);

    const projectOptions = useMemo(() => lookups.projects.map((item) => ({
        value: String(item.projectId),
        label: `${item.projectCode ? `${item.projectCode} - ` : ''}${item.projectName}${item.mappedBranches ? ` (${item.mappedBranches} branch${item.mappedBranches === 1 ? '' : 'es'})` : ''}`,
    })), [lookups.projects]);
    const branchOptions = useMemo(() => options(lookups.branches), [lookups.branches]);
    const locationOptions = useMemo(() => options(lookups.locations), [lookups.locations]);
    // Locations of each branch (created in Office › Branch Location), then the shared Primary location (branch 0).
    const locationsByBranch = useMemo(() => {
        const map = new Map();
        const shared = [];
        lookups.locations.forEach((item) => {
            const option = { value: String(item.id), label: item.name };
            if (!item.branchId) { shared.push(option); return; }
            const list = map.get(String(item.branchId)) || [];
            list.push(option);
            map.set(String(item.branchId), list);
        });
        return { map, shared };
    }, [lookups.locations]);
    const locationsFor = (branchId) => (branchId ? [...(locationsByBranch.map.get(branchId) || []), ...locationsByBranch.shared] : []);
    const societyOptions = useMemo(() => options(lookups.societies), [lookups.societies]);
    const categoryOptions = useMemo(() => options(lookups.categories), [lookups.categories]);
    const divisionOptions = useMemo(() => options(lookups.divisions), [lookups.divisions]);
    const usersByBranch = useMemo(() => {
        const map = new Map();
        lookups.users.forEach((user) => {
            const list = map.get(String(user.branchId)) || [];
            list.push({ value: String(user.id), label: user.userName && user.userName !== user.name ? `${user.name} (${user.userName})` : user.name });
            map.set(String(user.branchId), list);
        });
        return map;
    }, [lookups.users]);

    const project = projectId ? lookups.projects.find((item) => String(item.projectId) === projectId) : null;
    const createdOn = project ? toInputDate(project.createdOn) : '';
    const branchCode = (branchId) => lookups.branches.find((item) => String(item.id) === branchId)?.code || '';
    const branchName = (branchId) => lookups.branches.find((item) => String(item.id) === branchId)?.name || '';

    function blankRow(from) {
        return {
            key: newKey(), branchId: '', locationId: '', userId: '', locked: false,
            societyId: from?.societyId || '', categoryId: from?.categoryId || '', divisionId: from?.divisionId || '',
            startDate: from?.startDate || createdOn, endDate: from?.endDate || '',
        };
    }

    async function chooseProject(value) {
        if (dirty && !window.confirm('Discard the changes made to this project mapping?')) return;
        setProjectId(value);
        setErrors({});
        setDirty(false);
        setSearch('');
        setRows([]);
        setSavedRows([]);
        if (!value) return;
        setBusy(true);
        try {
            const detail = await projectMappingApi.fetchMapping(session, value);
            const loaded = (detail.rows || []).map(toGridRow);
            const start = lookups.projects.find((item) => String(item.projectId) === value);
            setSavedRows(loaded);
            setIsSaved(Boolean(detail.isSaved));
            setRows(loaded.length ? loaded : [{ ...blankRow(), startDate: toInputDate(start?.createdOn) }]);
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    function clearErrors(key, fields) {
        setErrors((current) => {
            const next = { ...current, form: undefined };
            fields.forEach((field) => { delete next[`${key}.${field}`]; });
            return next;
        });
    }

    function setCell(key, field, value) {
        setRows((current) => current.map((row) => {
            if (row.key !== key) return row;
            const next = { ...row, [field]: value };
            // A new branch has its own locations and users: reset both (pick the only one automatically;
            // a branch without locations of its own gets the shared Primary location).
            if (field === 'branchId') {
                const users = usersByBranch.get(value) || [];
                next.userId = users.length === 1 ? users[0].value : '';
                const own = locationsByBranch.map.get(value) || [];
                next.locationId = own.length === 1 ? own[0].value : own.length === 0 && locationsByBranch.shared.length ? locationsByBranch.shared[0].value : '';
            }
            return next;
        }));
        clearErrors(key, field === 'branchId' ? ['branchId', 'userId', 'locationId'] : [field]);
        setDirty(true);
    }

    // New rows copy society, category, division and dates from the last row to save typing.
    function addRow() {
        setRows((current) => [...current, blankRow(current[current.length - 1])]);
        setErrors((current) => ({ ...current, form: undefined }));
        setSearch('');
        setDirty(true);
        setTimeout(() => {
            const box = document.querySelector('.pm-grid-box');
            if (box) box.scrollTop = box.scrollHeight;
        }, 0);
    }

    function removeRow(row) {
        if (row.locked) return;
        setRows((current) => current.filter((item) => item.key !== row.key));
        clearErrors(row.key, ['branchId', 'locationId', 'societyId', 'categoryId', 'divisionId', 'startDate', 'endDate', 'userId']);
        setDirty(true);
    }

    function copyFirstRow() {
        const [first, ...others] = rows;
        if (!first || others.length === 0) return;
        setRows((current) => current.map((row, index) => {
            if (index === 0) return row;
            const fields = row.locked ? COPY_FIELDS.filter((field) => !LOCKED_FIELDS.includes(field)) : COPY_FIELDS;
            return { ...row, ...Object.fromEntries(fields.map((field) => [field, first[field]])) };
        }));
        others.forEach((row) => clearErrors(row.key, COPY_FIELDS));
        setDirty(true);
        pushToast({ message: `Copied row 1 details to ${others.length} row${others.length === 1 ? '' : 's'}.` });
    }

    function reset() {
        if (!window.confirm('Undo all changes and reload the saved mapping?')) return;
        setRows(savedRows.length ? savedRows.map((row) => ({ ...row, key: newKey() })) : [blankRow()]);
        setErrors({});
        setDirty(false);
    }

    function validate() {
        const next = {};
        if (rows.length === 0) next.form = 'Add at least one branch to map this project.';
        const seen = new Map();
        rows.forEach((row) => {
            const key = (field) => `${row.key}.${field}`;
            if (!row.branchId) next[key('branchId')] = 'Required';
            else if (seen.has(row.branchId)) next[key('branchId')] = 'Already added';
            else seen.set(row.branchId, row.key);
            if (!row.locationId) next[key('locationId')] = 'Required';
            else if (row.branchId && !locationsFor(row.branchId).some((option) => option.value === row.locationId)) next[key('locationId')] = 'Not a location of this branch';
            if (!row.societyId) next[key('societyId')] = 'Required';
            if (!row.categoryId) next[key('categoryId')] = 'Required';
            if (!row.divisionId) next[key('divisionId')] = 'Required';
            if (!row.startDate) next[key('startDate')] = 'Required';
            else if (!row.locked && createdOn && row.startDate < createdOn) next[key('startDate')] = `Not before ${gridDate(createdOn)}`;
            if (row.startDate && row.endDate && row.endDate < row.startDate) next[key('endDate')] = 'Before start date';
            if (!row.userId && (usersByBranch.get(row.branchId) || []).length > 0) next[key('userId')] = 'Required';
        });
        return next;
    }

    function showPreview() {
        const next = validate();
        const count = Object.keys(next).filter((key) => key !== 'form').length;
        if (count > 0) {
            next.form = `${count} field${count === 1 ? ' needs' : 's need'} attention in the highlighted rows.`;
            setSearch('');
            setTimeout(() => document.querySelector('.pm-grid .is-invalid')?.scrollIntoView({ block: 'center', inline: 'center', behavior: 'smooth' }), 0);
        }
        setErrors(next);
        if (Object.keys(next).length === 0) setPreview(true);
    }

    async function save() {
        setBusy(true);
        try {
            const request = {
                projectId: Number(projectId),
                rows: rows.map((row) => ({
                    branchId: Number(row.branchId),
                    locationId: Number(row.locationId),
                    societyId: Number(row.societyId),
                    categoryId: Number(row.categoryId),
                    divisionId: Number(row.divisionId),
                    startDate: row.startDate,
                    endDate: row.endDate || null,
                    userId: row.userId ? Number(row.userId) : null,
                    hasTransactions: row.locked,
                })),
            };
            const result = await projectMappingApi.saveMapping(session, request);
            pushToast({ message: (result && result.message) || 'Project mapping saved.' });
            setPreview(false);
            setDirty(false);
            const [detail, fresh] = await Promise.all([
                projectMappingApi.fetchMapping(session, projectId),
                projectMappingApi.fetchLookups(session),
            ]);
            const loaded = (detail.rows || []).map(toGridRow);
            setSavedRows(loaded);
            setRows(loaded);
            setIsSaved(Boolean(detail.isSaved));
            setLookups({ ...emptyLookups, ...fresh });
        }
        catch (error) {
            setPreview(false);
            setErrors({ form: errorMessage(error) });
        }
        finally {
            setBusy(false);
        }
    }

    const err = (key, field) => errors[`${key}.${field}`];
    const rowHasError = (key) => Object.keys(errors).some((name) => name.startsWith(`${key}.`));
    const term = search.trim().toLowerCase();
    const shownRows = rows.filter((row) => !term || `${branchCode(row.branchId)} ${branchName(row.branchId)}`.toLowerCase().includes(term));
    const removed = savedRows.filter((saved) => !rows.some((row) => row.branchId === saved.branchId)).map((row) => branchCode(row.branchId));

    return (<section className="lg-page pm-page" aria-label="Project Mapping">
      <div className="lg-bar">
        <h1 className="lg-title">Project Mapping</h1>
        <div className="lg-bar-tools">
          <button type="button" className="lg-tool" title="Reload lists" aria-label="Reload lists" disabled={busy || loading} onClick={() => void loadLookups()}><RefreshIcon /></button>
        </div>
      </div>

      {loadError ? (<p className="lg-form-error pm-banner pm-retry" role="alert">
        <span>{loadError} The project list could not be loaded.</span>
        <button type="button" className="lg-btn" disabled={loading} onClick={() => void loadLookups()}>{loading ? 'Loading…' : 'Retry'}</button>
      </p>) : null}

      <div className="pm-project-bar">
        <div className="pm-project-pick">
          <SearchableSelect id="pm-project" label="Project Name" required value={projectId} options={projectOptions} placeholder={loading ? 'Loading projects…' : 'Type to search a project'} disabled={loading || busy} onChange={(value) => void chooseProject(value)}/>
        </div>
        {project ? (<dl className="pm-project-facts">
          <div><dt>Code</dt><dd>{project.projectCode || '—'}</dd></div>
          <div><dt>Created On</dt><dd>{gridDate(createdOn) || '—'}</dd></div>
          <div className="is-wide"><dt>Description</dt><dd>{project.description || '—'}</dd></div>
          <div><dt>Mapped Branches</dt><dd><span className="pm-count">{rows.filter((row) => row.branchId).length}</span>{isSaved || savedRows.length === 0 ? '' : <em className="pm-legacy-tag"> from old mapping</em>}</dd></div>
        </dl>) : null}
      </div>

      {!project ? (<div className="pm-empty-state">
        <svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M3 5h8v6H3V5zm10 0h8v6h-8V5zM3 13h8v6H3v-6zm10 0h8v6h-8v-6z"/></svg>
        <h2>Select a project to start</h2>
        <p>Choose a project above, then add the branch offices it runs in. For each branch pick the location, society, project category, division, period and user in charge.</p>
      </div>) : (<>
        <div className="pm-toolbar">
          <h2 className="pm-toolbar-title">Branch-wise Mapping</h2>
          <input className="lg-search" type="search" placeholder="Search branch" aria-label="Search branch" value={search} onChange={(event) => setSearch(event.target.value)}/>
          <button type="button" className="lg-btn pm-copy" disabled={rows.length < 2 || busy} onClick={copyFirstRow} title="Copy Society, Category, Division and dates from row 1 to the other rows">Copy row 1 to all rows</button>
          <button type="button" className="lg-add" disabled={busy || rows.length >= lookups.branches.length} onClick={addRow}><PlusIcon />Add Branch</button>
        </div>

        {errors.form ? <p className="lg-form-error pm-banner" role="alert">{errors.form}</p> : null}

        <div className="pm-grid-box">
          <table className="pm-grid">
            <thead>
              <tr>
                <th className="pm-col-no">#</th>
                <th className="pm-col-branch">Branch Office <span className="req">*</span></th>
                <th className="pm-col-md">Branch Location <span className="req">*</span></th>
                <th className="pm-col-lg">Society <span className="req">*</span></th>
                <th className="pm-col-md">Project Category <span className="req">*</span></th>
                <th className="pm-col-sm">Division <span className="req">*</span></th>
                <th className="pm-col-date">Start Date <span className="req">*</span></th>
                <th className="pm-col-date">End Date</th>
                <th className="pm-col-md">User Incharge <span className="req">*</span></th>
                <th className="pm-col-act" aria-label="Remove"/>
              </tr>
            </thead>
            <tbody>
              {rows.length === 0 ? <tr><td colSpan={10} className="pm-no-rows">No branch added yet. Use <strong>Add Branch</strong> to map this project to a branch office.</td></tr> : null}
              {rows.length > 0 && shownRows.length === 0 ? <tr><td colSpan={10} className="pm-no-rows">No branch matches the search.</td></tr> : null}
              {shownRows.map((row) => {
                  const index = rows.indexOf(row);
                  const id = (field) => `pm-${row.key}-${field}`;
                  const users = usersByBranch.get(row.branchId) || [];
                  const usedElsewhere = new Set(rows.filter((item) => item.key !== row.key).map((item) => item.branchId));
                  const rowBranches = branchOptions.filter((option) => option.value === row.branchId || !usedElsewhere.has(option.value));
                  const lockedValue = (value) => <span className="pm-locked-value" title="Locked: this branch has transactions for the project">{value || '—'}</span>;
                  const className = ['pm-row', row.locked ? 'is-locked' : '', rowHasError(row.key) ? 'has-error' : ''].join(' ').trim();
                  return (<tr key={row.key} className={className}>
                    <td className="pm-col-no">{index + 1}</td>
                    <td className="pm-col-branch">
                      {row.locked ? (<div className="pm-branch-cell">
                        <strong>{branchCode(row.branchId)}<LockIcon /></strong>
                        <span>{branchName(row.branchId)}</span>
                        <em className="pm-tx-badge">Has transactions</em>
                      </div>) : <SearchableSelect id={id('branch')} label="Branch Office" value={row.branchId} options={rowBranches} placeholder="Select branch office" error={err(row.key, 'branchId')} onChange={(value) => setCell(row.key, 'branchId', value)}/>}
                    </td>
                    <td><SearchableSelect id={id('location')} label="Branch Location" value={row.locationId} options={locationsFor(row.branchId)} placeholder={row.branchId ? 'Select location' : 'Select branch first'} disabled={!row.branchId} error={err(row.key, 'locationId')} onChange={(value) => setCell(row.key, 'locationId', value)}/></td>
                    <td>{row.locked ? lockedValue(labelOf(societyOptions, row.societyId))
                        : <SearchableSelect id={id('society')} label="Society" value={row.societyId} options={societyOptions} placeholder="Select society" error={err(row.key, 'societyId')} onChange={(value) => setCell(row.key, 'societyId', value)}/>}</td>
                    <td>{row.locked ? lockedValue(labelOf(categoryOptions, row.categoryId))
                        : <SearchableSelect id={id('category')} label="Project Category" value={row.categoryId} options={categoryOptions} placeholder="Select category" error={err(row.key, 'categoryId')} onChange={(value) => setCell(row.key, 'categoryId', value)}/>}</td>
                    <td>{row.locked ? lockedValue(labelOf(divisionOptions, row.divisionId))
                        : <Select id={id('division')} label="Division" value={row.divisionId} options={divisionOptions} placeholder="Select" error={err(row.key, 'divisionId')} onChange={(value) => setCell(row.key, 'divisionId', value)}/>}</td>
                    <td>{row.locked ? lockedValue(gridDate(row.startDate))
                        : <Input id={id('start')} label="Start Date" type="date" value={row.startDate} error={err(row.key, 'startDate')} onChange={(value) => setCell(row.key, 'startDate', value)}/>}</td>
                    <td><Input id={id('end')} label="End Date" type="date" value={row.endDate} error={err(row.key, 'endDate')} onChange={(value) => setCell(row.key, 'endDate', value)}/></td>
                    <td><SearchableSelect id={id('user')} label="User Incharge" value={row.userId} options={users} placeholder={!row.branchId ? 'Select branch first' : users.length ? 'Select user' : 'No users in branch'} disabled={users.length === 0} error={err(row.key, 'userId')} onChange={(value) => setCell(row.key, 'userId', value)}/></td>
                    <td className="pm-col-act">
                      <button type="button" className="lg-icon is-delete" disabled={row.locked} title={row.locked ? 'This branch has transactions and cannot be removed' : 'Remove this branch'} aria-label={`Remove row ${index + 1}`} onClick={() => removeRow(row)}><DeleteIcon /></button>
                    </td>
                  </tr>);
              })}
            </tbody>
          </table>
        </div>

        <div className="pm-footer">
          <span className="pm-legend"><LockIcon /> Has transactions: cannot be removed; Society, Category, Division and Start Date are locked.</span>
          <span className="pm-footer-count"><strong>{rows.length}</strong> branch{rows.length === 1 ? '' : 'es'}{dirty ? <em> · unsaved changes</em> : null}</span>
          <button type="button" className="lg-btn" disabled={!dirty || busy} onClick={reset}>Reset</button>
          <button type="button" className="lg-btn is-primary" disabled={busy} onClick={showPreview}>Preview <span aria-hidden="true">&rarr;</span></button>
        </div>
      </>)}

      <Modal title="Preview Project Mapping" isOpen={preview} busy={busy} onClose={() => setPreview(false)} showClose className="lg-modal is-mapping" footer={<>
          <button type="button" className="lg-btn pm-footer-left" disabled={busy} onClick={() => setPreview(false)}><span aria-hidden="true">&larr;</span> Back to Edit</button>
          <button type="button" className="lg-btn is-primary" disabled={busy} onClick={() => void save()}>{busy ? 'Saving…' : 'Save'}</button>
        </>}>
        {project ? (<div className="pm-preview">
          <dl className="pm-preview-grid">
            <div><dt>Project</dt><dd>{project.projectCode ? `${project.projectCode} - ` : ''}{project.projectName}</dd></div>
            <div><dt>Created On</dt><dd>{gridDate(createdOn) || '—'}</dd></div>
            <div><dt>Branches Mapped</dt><dd>{rows.length}</dd></div>
          </dl>
          <div className="pm-preview-scroll">
            <table className="pm-preview-table">
              <thead><tr><th>#</th><th>Branch Office</th><th>Location</th><th>Society</th><th>Category</th><th>Division</th><th>Start</th><th>End</th><th>User Incharge</th></tr></thead>
              <tbody>
                {rows.map((row, index) => (<tr key={row.key}>
                  <td>{index + 1}</td>
                  <td><strong>{branchCode(row.branchId)}</strong>{row.locked ? <LockIcon /> : null}<br/><span className="pm-muted">{branchName(row.branchId)}</span></td>
                  <td>{labelOf(locationOptions, row.locationId)}</td>
                  <td>{labelOf(societyOptions, row.societyId)}</td>
                  <td>{labelOf(categoryOptions, row.categoryId)}</td>
                  <td>{labelOf(divisionOptions, row.divisionId)}</td>
                  <td>{gridDate(row.startDate)}</td>
                  <td>{gridDate(row.endDate) || 'Open'}</td>
                  <td>{labelOf(usersByBranch.get(row.branchId) || [], row.userId) || '—'}</td>
                </tr>))}
              </tbody>
            </table>
          </div>
          {removed.length > 0 ? <p className="pm-remove-note">Will be unmapped: {removed.join(', ')}</p> : null}
        </div>) : null}
      </Modal>
    </section>);
}
