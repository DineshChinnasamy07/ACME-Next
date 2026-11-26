import { useCallback, useEffect, useState } from 'react';
import { Input, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { tdsSectionApi } from '../services/tdsMastersApi';

/*
 * Data › Statutory Masters › TDS Section (Head Office). Legacy TDSSectionView.aspx (grid: Code, Name, Status).
 * Rules (TdsSectionRepository): Code required, max 45; Section Name required, max 100; code and name unique;
 * a section used by an active nature of payment cannot be made inactive; a section used by any nature of
 * payment cannot be deleted (checked by the server).
 */

const statusOptions = [
    { value: '1', label: 'Active' },
    { value: '0', label: 'Inactive' },
];

const emptySection = { tdsSectionId: 0, code: '', sectionName: '', status: '1' };

const columns = [
    { key: 'code', header: 'Code', width: '120px' },
    { key: 'sectionName', header: 'Name', wrap: true },
    { key: 'statusName', header: 'Status', width: '110px', render: (row) => <StatusText value={row.statusName}/> },
];

export function TdsSectionPage() {
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
            setRows(await tdsSectionApi.fetchSections(session));
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
        setForm({ ...emptySection });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await tdsSectionApi.fetchSection(session, row.tdsSectionId);
            setForm({
                tdsSectionId: detail.tdsSectionId,
                code: detail.code ?? '',
                sectionName: detail.sectionName ?? '',
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
        const code = form.code.trim();
        const name = form.sectionName.trim();
        if (!code) next.code = 'Section Code is required.';
        else if (code.length > 45) next.code = 'Code cannot exceed 45 characters.';
        if (!name) next.sectionName = 'Section Name is required.';
        else if (name.length > 100) next.sectionName = 'Section Name cannot exceed 100 characters.';
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
            const result = await tdsSectionApi.saveSection(session, {
                tdsSectionId: form.tdsSectionId,
                code: form.code.trim(),
                sectionName: form.sectionName.trim(),
                status: Number(form.status),
            });
            pushToast({ message: (result && result.message) || 'TDS Section saved.' });
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
        if (!window.confirm(`Delete TDS Section ${row.code} - ${row.sectionName}?`)) {
            return;
        }
        try {
            const result = await tdsSectionApi.deleteSection(session, row.tdsSectionId);
            pushToast({ message: (result && result.message) || 'TDS Section deleted.' });
            setRows((current) => current.filter((item) => item.tdsSectionId !== row.tdsSectionId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.tdsSectionId === 0;

    return (<section className="lg-page" aria-label="TDS Section">
      <PageBar title="TDS Section" addLabel="Add TDS Section" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.tdsSectionId} loading={loading} error={loadError} search={search} searchKeys={['code', 'sectionName', 'statusName']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add TDS Section' : 'Edit TDS Section'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="tds-sec-code" label="Code" required value={form.code} error={errors.code} onChange={set('code')}/>
          <Input id="tds-sec-name" label="Section Name" required value={form.sectionName} error={errors.sectionName} onChange={set('sectionName')}/>
          <Select id="tds-sec-status" label="Status" required hidePlaceholder value={form.status} options={statusOptions} onChange={set('status')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
