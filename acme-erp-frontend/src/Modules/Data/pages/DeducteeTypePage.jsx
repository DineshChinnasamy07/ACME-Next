import { useCallback, useEffect, useState } from 'react';
import { Input, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { deducteeTypeApi } from '../services/tdsMastersApi';

/*
 * Data › Statutory Masters › Deductee Type (Head Office). Legacy TDSDeducteeType.aspx
 * (grid: Name, Residential Status, Deductee Status, Status).
 * Codes (Acme.Common TDSResidentialStatus / TDSDeducteeStatus): residential 0 = Resident, 1 = Non Resident;
 * deductee 0 = Company, 1 = Non Company.
 * Rules (DeducteeTypeRepository): Name required, max 100, unique; a type used by a TDS policy or a creditor's
 * TDS profile cannot be made inactive or deleted (checked by the server).
 */

const statusOptions = [
    { value: '1', label: 'Active' },
    { value: '0', label: 'Inactive' },
];

const residentialOptions = [
    { value: '0', label: 'Resident' },
    { value: '1', label: 'Non Resident' },
];

const deducteeOptions = [
    { value: '0', label: 'Company' },
    { value: '1', label: 'Non Company' },
];

const emptyDeductee = { deducteeTypeId: 0, name: '', residentialStatus: '0', deducteeType: '0', status: '1' };

const columns = [
    { key: 'name', header: 'Name', wrap: true },
    { key: 'residentialStatusName', header: 'Residential Status', width: '160px' },
    { key: 'deducteeTypeName', header: 'Deductee Status', width: '150px' },
    { key: 'statusName', header: 'Status', width: '110px', render: (row) => <StatusText value={row.statusName}/> },
];

export function DeducteeTypePage() {
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
            setRows(await deducteeTypeApi.fetchDeducteeTypes(session));
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
        setForm({ ...emptyDeductee });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await deducteeTypeApi.fetchDeducteeType(session, row.deducteeTypeId);
            setForm({
                deducteeTypeId: detail.deducteeTypeId,
                name: detail.name ?? '',
                residentialStatus: String(detail.residentialStatus ?? 0),
                deducteeType: String(detail.deducteeType ?? 0),
                status: String(detail.status ?? 1),
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
        const name = form.name.trim();
        if (!name) next.name = 'Name is required.';
        else if (name.length > 100) next.name = 'Name cannot exceed 100 characters.';
        if (form.residentialStatus !== '0' && form.residentialStatus !== '1') next.residentialStatus = 'Residential Status is required.';
        if (form.deducteeType !== '0' && form.deducteeType !== '1') next.deducteeType = 'Deductee Status is required.';
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
            const result = await deducteeTypeApi.saveDeducteeType(session, {
                deducteeTypeId: form.deducteeTypeId,
                name: form.name.trim(),
                residentialStatus: Number(form.residentialStatus),
                deducteeType: Number(form.deducteeType),
                status: Number(form.status),
            });
            pushToast({ message: (result && result.message) || 'Deductee Type saved.' });
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
        if (!window.confirm(`Delete Deductee Type ${row.name}?`)) {
            return;
        }
        try {
            const result = await deducteeTypeApi.deleteDeducteeType(session, row.deducteeTypeId);
            pushToast({ message: (result && result.message) || 'Deductee Type deleted.' });
            setRows((current) => current.filter((item) => item.deducteeTypeId !== row.deducteeTypeId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.deducteeTypeId === 0;

    return (<section className="lg-page" aria-label="Deductee Type">
      <PageBar title="Deductee Type" addLabel="Add Deductee Type" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.deducteeTypeId} loading={loading} error={loadError} search={search} searchKeys={['name', 'residentialStatusName', 'deducteeTypeName', 'statusName']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add Deductee Type' : 'Edit Deductee Type'} wide isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="tds-ded-name" label="Name" required value={form.name} error={errors.name} onChange={set('name')}/>
          <Select id="tds-ded-residential" label="Residential Status" required hidePlaceholder value={form.residentialStatus} options={residentialOptions} error={errors.residentialStatus} onChange={set('residentialStatus')}/>
          <Select id="tds-ded-type" label="Deductee Status" required hidePlaceholder value={form.deducteeType} options={deducteeOptions} error={errors.deducteeType} onChange={set('deducteeType')}/>
          <Select id="tds-ded-status" label="Status" required hidePlaceholder value={form.status} options={statusOptions} onChange={set('status')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
