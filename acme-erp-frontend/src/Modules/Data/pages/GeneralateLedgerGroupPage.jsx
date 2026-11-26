import { useCallback, useEffect, useMemo, useState } from 'react';
import { Input, SearchableSelect } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { generalateLedgerApi } from '../services/purposeGeneralateApi';

/*
 * Data › Finance Masters › Generalate Ledger Group (Head Office). Legacy GeneralateLedger.aspx (tree + popup).
 * Rules (legacy + GeneralateLedgerGroupRepository): Parent ledger required for a new ledger; Code required,
 * max 10, stored upper case; Name required, max 100 (DB column); duplicate code+name checked by the server.
 * A top-level ledger points to itself (conParentLedgerId === conLedgerId). A ledger with sub ledgers
 * cannot be deleted.
 */

const emptyLedger = { conLedgerId: 0, conLedgerCode: '', conLedgerName: '', conParentLedgerId: '', isRoot: false };

function isRootRow(row) {
    return !row.conParentLedgerId || row.conParentLedgerId <= 0 || row.conParentLedgerId === row.conLedgerId;
}

// Depth-first order (parent followed by its sub ledgers) with a depth for indenting the name.
function toTree(rows) {
    const ids = new Set(rows.map((row) => row.conLedgerId));
    const children = new Map();
    const roots = [];
    rows.forEach((row) => {
        if (isRootRow(row) || !ids.has(row.conParentLedgerId)) {
            roots.push(row);
            return;
        }
        const list = children.get(row.conParentLedgerId) || [];
        list.push(row);
        children.set(row.conParentLedgerId, list);
    });
    const ordered = [];
    const seen = new Set();
    function walk(row, depth) {
        if (seen.has(row.conLedgerId)) {
            return;
        }
        seen.add(row.conLedgerId);
        ordered.push({ ...row, depth });
        (children.get(row.conLedgerId) || []).forEach((child) => walk(child, depth + 1));
    }
    roots.forEach((row) => walk(row, 0));
    // Rows caught in a parent loop (bad data) are still listed.
    rows.forEach((row) => walk(row, 0));
    return ordered;
}

// The ledger itself and all of its sub ledgers (cannot be chosen as its parent).
function selfAndDescendants(rows, conLedgerId) {
    const result = new Set([conLedgerId]);
    let grew = true;
    while (grew) {
        grew = false;
        rows.forEach((row) => {
            if (!result.has(row.conLedgerId) && !isRootRow(row) && result.has(row.conParentLedgerId)) {
                result.add(row.conLedgerId);
                grew = true;
            }
        });
    }
    return result;
}

const columns = [
    { key: 'conLedgerCode', header: 'Ledger Code', width: '120px' },
    {
        key: 'conLedgerName',
        header: 'Ledger Name',
        wrap: true,
        render: (row) => (<span style={{ paddingLeft: `${row.depth * 18}px`, fontWeight: row.depth === 0 ? 600 : undefined }}>
          {row.depth > 0 ? '└ ' : ''}{row.conLedgerName}
        </span>),
    },
    { key: 'parentName', header: 'Parent Ledger', wrap: true, render: (row) => row.parentName || '—' },
];

export function GeneralateLedgerGroupPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [parents, setParents] = useState([]);
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
            const [ledgers, parentOptions] = await Promise.all([
                generalateLedgerApi.fetchLedgers(session),
                generalateLedgerApi.fetchParentLedgers(session),
            ]);
            setRows(ledgers);
            setParents(parentOptions);
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    const treeRows = useMemo(() => toTree(rows), [rows]);

    const parentOptions = useMemo(() => {
        if (!form) {
            return [];
        }
        const blocked = form.conLedgerId ? selfAndDescendants(rows, form.conLedgerId) : new Set();
        return parents
            .filter((option) => !blocked.has(option.id))
            .map((option) => ({ value: String(option.id), label: option.name }));
    }, [form, rows, parents]);

    function set(field) {
        return (value) => {
            setForm((current) => ({ ...current, [field]: field === 'conLedgerCode' ? value.toUpperCase() : value }));
            setErrors((current) => ({ ...current, [field]: undefined }));
        };
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setForm({ ...emptyLedger });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await generalateLedgerApi.fetchLedger(session, row.conLedgerId);
            const root = isRootRow(detail);
            setForm({
                conLedgerId: detail.conLedgerId,
                conLedgerCode: detail.conLedgerCode ?? '',
                conLedgerName: detail.conLedgerName ?? '',
                conParentLedgerId: root ? '' : String(detail.conParentLedgerId),
                isRoot: root,
            });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    function validate() {
        const next = {};
        const code = form.conLedgerCode.trim();
        const name = form.conLedgerName.trim();
        if (!form.isRoot && !form.conParentLedgerId) next.conParentLedgerId = 'Select parent ledger.';
        if (!code) next.conLedgerCode = 'Ledger Code is required.';
        else if (code.length > 10) next.conLedgerCode = 'Code cannot exceed 10 characters.';
        if (!name) next.conLedgerName = 'Ledger Name is required.';
        else if (name.length > 100) next.conLedgerName = 'Name cannot exceed 100 characters.';
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
            const result = await generalateLedgerApi.saveLedger(session, {
                conLedgerId: form.conLedgerId,
                conLedgerCode: form.conLedgerCode.trim().toUpperCase(),
                conLedgerName: form.conLedgerName.trim(),
                // 0 on edit keeps the current parent (used for top-level ledgers).
                conParentLedgerId: form.isRoot ? 0 : Number(form.conParentLedgerId),
            });
            pushToast({ message: (result && result.message) || 'Generalate ledger saved.' });
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
        if (row.hasChildren) {
            return;
        }
        if (!window.confirm(`Delete generalate ledger ${row.conLedgerName} (${row.conLedgerCode})? Its ledger mappings are removed too.`)) {
            return;
        }
        try {
            const result = await generalateLedgerApi.deleteLedger(session, row.conLedgerId);
            pushToast({ message: (result && result.message) || 'Generalate ledger deleted.' });
            await load();
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.conLedgerId === 0;

    return (<section className="lg-page" aria-label="Generalate Ledger Group">
      <PageBar title="Generalate Ledger Group" addLabel="Add Ledger" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={treeRows} getKey={(row) => row.conLedgerId} loading={loading} error={loadError} search={search} searchKeys={['conLedgerCode', 'conLedgerName', 'parentName']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
          deleteDisabled: row.hasChildren,
          deleteTitle: row.hasChildren ? 'Has sub ledgers — delete them first' : 'Delete',
      })}/>

      <FormModal title={isNew ? 'Add Generalate Ledger' : 'Edit Generalate Ledger'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          {form.isRoot
            ? <Input id="gl-parent" label="Parent Ledger" value="(Top level ledger)" disabled onChange={() => {}}/>
            : <SearchableSelect id="gl-parent" label="Parent Ledger" required value={form.conParentLedgerId} options={parentOptions} placeholder="Select parent ledger" error={errors.conParentLedgerId} onChange={set('conParentLedgerId')}/>}
          <Input id="gl-code" label="Code" required value={form.conLedgerCode} error={errors.conLedgerCode} onChange={set('conLedgerCode')}/>
          <Input id="gl-name" label="Name" required value={form.conLedgerName} error={errors.conLedgerName} onChange={set('conLedgerName')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
