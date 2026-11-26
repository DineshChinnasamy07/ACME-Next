import { useCallback, useEffect, useState } from 'react';
import { Input, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { dutyTaxApi } from '../services/tdsMastersApi';

/*
 * Data › Statutory Masters › Duty Tax (Head Office). Legacy TDSDutyTax.aspx (grid: Name, Status).
 * Rules (DutyTaxRepository): Name required, max 50, unique. Built-in types 1-5 (TDS With PAN, TDS Without PAN,
 * Surcharge, Ed Cess, Sec Ed Cess) are used by id in voucher TDS: name is read-only, delete is blocked.
 * A tax type used by a TDS tax rate cannot be deleted (checked by the server).
 */

const statusOptions = [
    { value: '1', label: 'Active' },
    { value: '0', label: 'Inactive' },
];

const emptyDutyTax = { tdsDutyTaxTypeId: 0, taxTypeName: '', status: '1', isSystem: false };

const columns = [
    { key: 'taxTypeName', header: 'Name', wrap: true },
    { key: 'statusName', header: 'Status', width: '110px', render: (row) => <StatusText value={row.statusName}/> },
];

export function DutyTaxPage() {
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
            setRows(await dutyTaxApi.fetchDutyTaxes(session));
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
        setForm({ ...emptyDutyTax });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await dutyTaxApi.fetchDutyTax(session, row.tdsDutyTaxTypeId);
            setForm({
                tdsDutyTaxTypeId: detail.tdsDutyTaxTypeId,
                taxTypeName: detail.taxTypeName ?? '',
                status: String(detail.status ?? 1),
                isSystem: Boolean(detail.isSystem),
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
        const name = form.taxTypeName.trim();
        if (!name) next.taxTypeName = 'Name is required.';
        else if (name.length > 50) next.taxTypeName = 'Name cannot exceed 50 characters.';
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
            const result = await dutyTaxApi.saveDutyTax(session, {
                tdsDutyTaxTypeId: form.tdsDutyTaxTypeId,
                taxTypeName: form.taxTypeName.trim(),
                status: Number(form.status),
            });
            pushToast({ message: (result && result.message) || 'Duty Tax saved.' });
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
        if (!window.confirm(`Delete Duty Tax ${row.taxTypeName}?`)) {
            return;
        }
        try {
            const result = await dutyTaxApi.deleteDutyTax(session, row.tdsDutyTaxTypeId);
            pushToast({ message: (result && result.message) || 'Duty Tax deleted.' });
            setRows((current) => current.filter((item) => item.tdsDutyTaxTypeId !== row.tdsDutyTaxTypeId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.tdsDutyTaxTypeId === 0;

    return (<section className="lg-page" aria-label="Duty Tax">
      <PageBar title="Duty Tax" addLabel="Add Duty Tax" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.tdsDutyTaxTypeId} loading={loading} error={loadError} search={search} searchKeys={['taxTypeName', 'statusName']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
          deleteDisabled: Boolean(row.isSystem),
          deleteTitle: row.isSystem ? 'Built-in tax types cannot be deleted' : 'Delete',
      })}/>

      <FormModal title={isNew ? 'Add Duty Tax' : 'Edit Duty Tax'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="tds-duty-name" label="Name" required disabled={form.isSystem} value={form.taxTypeName} error={errors.taxTypeName} onChange={set('taxTypeName')}/>
          <Select id="tds-duty-status" label="Status" required hidePlaceholder value={form.status} options={statusOptions} onChange={set('status')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
