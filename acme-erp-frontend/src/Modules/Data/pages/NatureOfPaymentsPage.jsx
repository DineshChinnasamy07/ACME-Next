import { useCallback, useEffect, useState } from 'react';
import { Input, SearchableSelect, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { natureOfPaymentApi } from '../services/tdsMastersApi';

/*
 * Data › Statutory Masters › Nature of Payments (Head Office). Legacy TDSPaymentNature.aspx
 * (grid: Code, Name, Section, Status) and NatureofPaymentSQL (fields: name, payment code, section, notes, status).
 * Rules (NatureOfPaymentRepository): Name required, max 150, unique; Payment Code required, max 7; Section
 * required and active (only active sections are offered); Description max 400; a nature used by a TDS policy
 * or a creditor's TDS profile cannot be made inactive or deleted (checked by the server).
 */

const statusOptions = [
    { value: '1', label: 'Active' },
    { value: '0', label: 'Inactive' },
];

const emptyNature = { naturePayId: 0, paymentCode: '', name: '', tdsSectionId: '', description: '', status: '1' };

const columns = [
    { key: 'paymentCode', header: 'Code', width: '100px' },
    { key: 'name', header: 'Name', wrap: true },
    { key: 'sectionName', header: 'Section', wrap: true },
    { key: 'statusName', header: 'Status', width: '110px', render: (row) => <StatusText value={row.statusName}/> },
];

function TextArea({ id, label, value, onChange, rows = 3, maxLength, error }) {
    return (<div className="field">
      <label htmlFor={id}>{label}</label>
      <div>
        <textarea id={id} className={error ? 'control lg-textarea is-invalid' : 'control lg-textarea'} rows={rows} maxLength={maxLength} value={value} onChange={(event) => onChange(event.target.value)}/>
        {error ? <p className="field-error">{error}</p> : null}
      </div>
    </div>);
}

// Active sections as { value, label }; an inactive section already on the record is kept so it still shows.
function toSectionOptions(sections, current) {
    const options = sections.map((section) => ({ value: String(section.tdsSectionId), label: section.name }));
    if (current && current.tdsSectionId && !options.some((option) => option.value === String(current.tdsSectionId))) {
        const label = [current.sectionCode, current.sectionName].filter(Boolean).join(' - ') || `Section ${current.tdsSectionId}`;
        options.push({ value: String(current.tdsSectionId), label: `${label} (Inactive)` });
    }
    return options;
}

export function NatureOfPaymentsPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [sectionOptions, setSectionOptions] = useState([]);
    const [form, setForm] = useState(null);
    const [errors, setErrors] = useState({});
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            setRows(await natureOfPaymentApi.fetchNatures(session));
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

    async function openAdd() {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const sections = await natureOfPaymentApi.fetchSections(session);
            setSectionOptions(toSectionOptions(sections, null));
            setForm({ ...emptyNature });
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
            const [detail, sections] = await Promise.all([
                natureOfPaymentApi.fetchNature(session, row.naturePayId),
                natureOfPaymentApi.fetchSections(session),
            ]);
            setSectionOptions(toSectionOptions(sections, detail));
            setForm({
                naturePayId: detail.naturePayId,
                paymentCode: detail.paymentCode ?? '',
                name: detail.name ?? '',
                tdsSectionId: detail.tdsSectionId ? String(detail.tdsSectionId) : '',
                description: detail.description ?? '',
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
        const code = form.paymentCode.trim();
        const name = form.name.trim();
        if (!name) next.name = 'Nature of Payment name is required.';
        else if (name.length > 150) next.name = 'Name cannot exceed 150 characters.';
        if (!code) next.paymentCode = 'Payment Code is required.';
        else if (code.length > 7) next.paymentCode = 'Payment Code cannot exceed 7 characters.';
        if (!form.tdsSectionId) next.tdsSectionId = 'Section is required.';
        if (form.description.trim().length > 400) next.description = 'Description cannot exceed 400 characters.';
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
            const result = await natureOfPaymentApi.saveNature(session, {
                naturePayId: form.naturePayId,
                name: form.name.trim(),
                paymentCode: form.paymentCode.trim(),
                tdsSectionId: Number(form.tdsSectionId),
                description: form.description.trim(),
                status: Number(form.status),
            });
            pushToast({ message: (result && result.message) || 'Nature of Payment saved.' });
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
        if (!window.confirm(`Delete Nature of Payment ${row.name} (${row.paymentCode})?`)) {
            return;
        }
        try {
            const result = await natureOfPaymentApi.deleteNature(session, row.naturePayId);
            pushToast({ message: (result && result.message) || 'Nature of Payment deleted.' });
            setRows((current) => current.filter((item) => item.naturePayId !== row.naturePayId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.naturePayId === 0;

    return (<section className="lg-page" aria-label="Nature of Payments">
      <PageBar title="Nature of Payments" addLabel="Add Nature of Payment" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={() => void openAdd()}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.naturePayId} loading={loading} error={loadError} search={search} searchKeys={['paymentCode', 'name', 'sectionName', 'statusName']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add Nature of Payment' : 'Edit Nature of Payment'} wide isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="tds-nop-code" label="Payment Code" required value={form.paymentCode} error={errors.paymentCode} onChange={set('paymentCode')}/>
          <Input id="tds-nop-name" label="Name" required value={form.name} error={errors.name} onChange={set('name')}/>
          <SearchableSelect id="tds-nop-section" label="Section" required value={form.tdsSectionId} options={sectionOptions} placeholder="Search section" error={errors.tdsSectionId} onChange={set('tdsSectionId')}/>
          <Select id="tds-nop-status" label="Status" required hidePlaceholder value={form.status} options={statusOptions} onChange={set('status')}/>
          <TextArea id="tds-nop-description" label="Description" maxLength={400} value={form.description} error={errors.description} onChange={set('description')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
