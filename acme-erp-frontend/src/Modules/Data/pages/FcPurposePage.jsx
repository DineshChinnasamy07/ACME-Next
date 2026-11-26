import { useCallback, useEffect, useState } from 'react';
import { Input } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { fcPurposeApi } from '../services/purposeGeneralateApi';

/*
 * Data › Finance Masters › FC Purpose (Head Office). Legacy FCPurposeView.aspx / FCPurposeAdd.aspx.
 * Rules (legacy + FcPurposeRepository): Code required, max 10; FC Purpose required, max 150;
 * duplicate purpose name checked by the server.
 */

const emptyPurpose = { contributionId: 0, code: '', fcPurpose: '' };

const columns = [
    { key: 'code', header: 'Code', width: '120px' },
    { key: 'fcPurpose', header: 'FC Purpose', wrap: true },
];

export function FcPurposePage() {
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
            setRows(await fcPurposeApi.fetchPurposes(session));
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
        setForm({ ...emptyPurpose });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await fcPurposeApi.fetchPurpose(session, row.contributionId);
            setForm({
                contributionId: detail.contributionId,
                code: detail.code ?? '',
                fcPurpose: detail.fcPurpose ?? '',
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
        const purpose = form.fcPurpose.trim();
        if (!code) next.code = 'FC Purpose Code is required.';
        else if (code.length > 10) next.code = 'Code cannot exceed 10 characters.';
        if (!purpose) next.fcPurpose = 'FC Purpose is required.';
        else if (purpose.length > 150) next.fcPurpose = 'FC Purpose cannot exceed 150 characters.';
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
            const result = await fcPurposeApi.savePurpose(session, {
                contributionId: form.contributionId,
                code: form.code.trim(),
                fcPurpose: form.fcPurpose.trim(),
            });
            pushToast({ message: (result && result.message) || 'FC Purpose saved.' });
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
        if (!window.confirm(`Delete FC Purpose ${row.fcPurpose} (${row.code})?`)) {
            return;
        }
        try {
            const result = await fcPurposeApi.deletePurpose(session, row.contributionId);
            pushToast({ message: (result && result.message) || 'FC Purpose deleted.' });
            setRows((current) => current.filter((item) => item.contributionId !== row.contributionId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.contributionId === 0;

    return (<section className="lg-page" aria-label="FC Purpose">
      <PageBar title="FC Purpose" addLabel="Add FC Purpose" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.contributionId} loading={loading} error={loadError} search={search} searchKeys={['code', 'fcPurpose']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add FC Purpose' : 'Edit FC Purpose'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="fcp-code" label="Code" required value={form.code} error={errors.code} onChange={set('code')}/>
          <Input id="fcp-purpose" label="FC Purpose" required value={form.fcPurpose} error={errors.fcPurpose} onChange={set('fcPurpose')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
