import { useCallback, useEffect, useMemo, useState } from 'react';
import { Input, Modal, SearchableSelect } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { accessFlag, ledgerGroupApi, natureNames } from '../services/ledgerApi';

/*
 * Data › Finance Masters › Ledger Group (Head Office login).
 * Legacy ACCPPORTAL LedgerGroup.aspx shows a nature → group → sub group tree with an Add / Edit popup
 * (Under the Group *, Code, Name *) and the ledgers of the selected group. Here the tree is one grid
 * indented by level; rules follow the Acme.Masters LedgerGroupRepository (api/ledgergroups).
 */

const emptyGroup = { groupId: 0, abbrevation: '', group: '', parentGroupId: '' };
const MAX_CODE = 10;
const MAX_NAME = 100;

function text(value) {
    return value === null || value === undefined ? '' : String(value);
}

function isRoot(row) {
    return row.parentGroupId === row.groupId || !row.parentGroupId;
}

// Flatten the group list into tree order (nature roots first, children under their parent) with a depth.
function toTree(rows) {
    const byId = new Map(rows.map((row) => [row.groupId, row]));
    const children = new Map();
    const roots = [];
    rows.forEach((row) => {
        if (isRoot(row) || !byId.has(row.parentGroupId)) {
            roots.push(row);
            return;
        }
        const list = children.get(row.parentGroupId) || [];
        list.push(row);
        children.set(row.parentGroupId, list);
    });
    const byName = (a, b) => String(a.ledgerGroup).localeCompare(String(b.ledgerGroup));
    roots.sort((a, b) => (a.natureId - b.natureId) || byName(a, b));
    const result = [];
    const seen = new Set();
    function walk(row, depth) {
        if (seen.has(row.groupId)) return;
        seen.add(row.groupId);
        const kids = (children.get(row.groupId) || []).slice().sort(byName);
        result.push({ ...row, depth, childCount: kids.length });
        kids.forEach((kid) => walk(kid, depth + 1));
    }
    roots.forEach((root) => walk(root, 0));
    return result;
}

// Backend allows a new group only under a nature root or a group directly under it (PARENT_GROUP_ID == NATURE_ID).
function canBeParent(row) {
    return row && row.parentGroupId === row.natureId;
}

function accessText(value) {
    if (value === accessFlag.readonly) return 'System';
    if (value === accessFlag.editable) return 'Fixed';
    return 'User';
}

function GroupName({ row }) {
    const label = row.depth === 0 ? <strong>{row.ledgerGroup}</strong> : row.ledgerGroup;
    return <span style={{ paddingLeft: `${row.depth * 18}px` }}>{row.depth > 0 ? '└ ' : ''}{label}</span>;
}

export function LedgerGroupPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [lookup, setLookup] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [form, setForm] = useState(null);
    const [errors, setErrors] = useState({});
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);
    const [ledgerView, setLedgerView] = useState(null);

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            const [groups, parents] = await Promise.all([
                ledgerGroupApi.fetchGroups(session),
                ledgerGroupApi.fetchParentLookup(session),
            ]);
            setRows(groups);
            setLookup(parents);
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    const tree = useMemo(() => toTree(rows).map((row) => ({
        ...row,
        nature: natureNames[row.natureId] || '',
        access: accessText(row.accessFlag),
        parentGroup: isRoot(row) ? '' : row.parentGroup,
    })), [rows]);
    const byId = useMemo(() => new Map(tree.map((row) => [row.groupId, row])), [tree]);

    // Parent dropdown: lookup endpoint (no Bank / Cash / FD) narrowed to the levels the backend accepts.
    const parentOptions = useMemo(() => {
        const currentId = form ? form.groupId : 0;
        return lookup
            .filter((option) => option.groupId !== currentId && canBeParent(byId.get(option.groupId)))
            .map((option) => {
                const row = byId.get(option.groupId);
                const nature = row && natureNames[row.natureId] ? ` (${natureNames[row.natureId]})` : '';
                return { value: String(option.groupId), label: `${option.ledgerGroup}${row && row.depth > 0 ? nature : ''}` };
            });
    }, [lookup, byId, form]);

    function set(field) {
        return (value) => {
            setForm((current) => ({ ...current, [field]: value }));
            setErrors((current) => ({ ...current, [field]: undefined }));
        };
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setForm({ ...emptyGroup, hasChildren: false });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await ledgerGroupApi.fetchGroup(session, row.groupId);
            setForm({
                groupId: detail.groupId,
                abbrevation: text(detail.groupCode),
                group: text(detail.ledgerGroup),
                parentGroupId: detail.parentGroupId ? String(detail.parentGroupId) : '',
                hasChildren: row.childCount > 0,
            });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Legacy LedgerGroup popup + LedgerGroupRepository: parent, code and name required; code ≤ 10, name ≤ 100;
    // parent must be a nature or a first-level group. Duplicate name is checked by the server.
    function validate() {
        const next = {};
        if (!form.parentGroupId) next.parentGroupId = 'Select the parent group.';
        else if (!form.hasChildren && !canBeParent(byId.get(Number(form.parentGroupId)))) next.parentGroupId = 'A group can be created only up to the second level.';
        const code = form.abbrevation.trim();
        if (!code) next.abbrevation = 'Enter the group code.';
        else if (code.length > MAX_CODE) next.abbrevation = `Code cannot exceed ${MAX_CODE} characters.`;
        const name = form.group.trim();
        if (!name) next.group = 'Enter the group name.';
        else if (name.length > MAX_NAME) next.group = `Name cannot exceed ${MAX_NAME} characters.`;
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
                groupId: form.groupId,
                abbrevation: form.abbrevation.trim().toUpperCase(),
                group: form.group.trim(),
                parentGroupId: Number(form.parentGroupId),
            };
            const result = await ledgerGroupApi.saveGroup(session, request);
            pushToast({ message: (result && result.message) || 'Ledger group saved.' });
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
        if (!window.confirm(`Delete ledger group ${row.ledgerGroup}?`)) {
            return;
        }
        try {
            const result = await ledgerGroupApi.deleteGroup(session, row.groupId);
            pushToast({ message: (result && result.message) || 'Ledger group deleted.' });
            await load();
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    async function showLedgers(row) {
        setLedgerView({ title: row.ledgerGroup, rows: [], loading: true, error: '' });
        try {
            const ledgers = await ledgerGroupApi.fetchLedgersUnder(session, [row.groupId]);
            setLedgerView({ title: row.ledgerGroup, rows: ledgers.map((item, index) => ({ ...item, rowNo: index })), loading: false, error: "" });
        }
        catch (error) {
            setLedgerView({ title: row.ledgerGroup, rows: [], loading: false, error: errorMessage(error) });
        }
    }

    const columns = [
        { key: 'ledgerGroup', header: 'Ledger Group', wrap: true, render: (row) => <GroupName row={row}/> },
        { key: 'groupCode', header: 'Code', width: '80px' },
        { key: 'parentGroup', header: 'Under the Group', wrap: true },
        { key: 'nature', header: 'Nature', width: '100px' },
        { key: 'access', header: 'Type', width: '70px' },
        { key: 'ledgers', header: 'Ledgers', width: '80px', render: (row) => <button type="button" className="lg-btn" onClick={() => void showLedgers(row)}>View</button> },
    ];

    const isNew = form && form.groupId === 0;
    const parentLocked = Boolean(form && form.hasChildren);

    return (<section className="lg-page" aria-label="Ledger Group">
      <PageBar title="Ledger Group" addLabel="Add Ledger Group" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={tree} getKey={(row) => row.groupId} loading={loading} error={loadError} search={search} searchKeys={['ledgerGroup', 'groupCode', 'parentGroup', 'nature']} rowActions={(row) => {
          const system = row.accessFlag !== accessFlag.accessable;
          const deleteTitle = system ? 'System group cannot be deleted' : row.childCount > 0 ? 'Delete its sub groups first' : 'Delete';
          return {
              onEdit: row.accessFlag === accessFlag.readonly ? undefined : () => void openEdit(row),
              onDelete: () => void remove(row),
              deleteDisabled: system || row.childCount > 0,
              deleteTitle,
          };
      }}/>

      <FormModal title={isNew ? 'Add Ledger Group' : 'Edit Ledger Group'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          {parentLocked
            ? <Input id="lgg-parent" label="Under the Group" required value={(byId.get(Number(form.parentGroupId)) || {}).ledgerGroup || ''} disabled onChange={() => {}}/>
            : <SearchableSelect id="lgg-parent" label="Under the Group" required value={form.parentGroupId} options={parentOptions} placeholder="Select parent group" error={errors.parentGroupId} onChange={set('parentGroupId')}/>}
          <Input id="lgg-code" label="Code" required value={form.abbrevation} error={errors.abbrevation} onChange={set('abbrevation')}/>
          <Input id="lgg-name" label="Name" required value={form.group} error={errors.group} onChange={set('group')}/>
        </FieldGrid> : null}
        {parentLocked ? <p className="lg-hint">This group has sub groups, so its parent group cannot be changed.</p> : null}
      </FormModal>

      <Modal title={ledgerView ? `Ledgers under ${ledgerView.title}` : ''} isOpen={Boolean(ledgerView)} onClose={() => setLedgerView(null)} showClose className="lg-modal is-wide" footer={<button type="button" className="lg-btn" onClick={() => setLedgerView(null)}>Close</button>}>
        {ledgerView ? <LegacyGrid columns={[{ key: 'ledgerCode', header: 'Ledger Code', width: '120px' }, { key: 'ledgerName', header: 'Ledger Name' }]} rows={ledgerView.rows} getKey={(row) => row.rowNo} loading={ledgerView.loading} error={ledgerView.error} emptyText="No ledgers under this group."/> : null}
      </Modal>
    </section>);
}
