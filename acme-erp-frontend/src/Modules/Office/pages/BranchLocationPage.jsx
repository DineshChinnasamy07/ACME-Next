import { useCallback, useEffect, useState } from 'react';
import { Input, SearchableSelect } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { branchLocationApi } from '../services/officeApi';

// Same limit as BRANCH_LOCATION.LOCATION_NAME varchar(200) and the API check.
const LOCATION_NAME_MAX = 200;

const columns = [
    { key: 'location', header: 'Location' },
    { key: 'branch', header: 'Branch', render: (row) => (row.isPrimary && !row.branch ? '—' : row.branch) },
];

const emptyForm = { locationId: 0, branchId: '', locationName: '' };

// Office → Branch Location: list, add, edit and delete the head office's branch locations.
// A location is created for one branch; Project Mapping offers each branch only its own locations (plus Primary).
export function BranchLocationPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [branches, setBranches] = useState([]);
    const [form, setForm] = useState(null);
    const [errors, setErrors] = useState({});
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            setRows(await branchLocationApi.fetchLocations(session));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    // Branch dropdown: activated main branches. When editing, keep the location's current
    // branch selectable even if it is no longer activated.
    async function openForm(next, currentBranch) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            let options = await branchLocationApi.fetchBranches(session);
            if (currentBranch && currentBranch.branchId > 0 && !options.some((item) => item.branchOfficeId === currentBranch.branchId)) {
                options = [...options, { branchOfficeId: currentBranch.branchId, branchOfficeName: currentBranch.branch || `Branch ${currentBranch.branchId}` }];
            }
            setBranches(options);
            setForm(next);
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    function openAdd() {
        void openForm({ ...emptyForm });
    }

    async function openEdit(row) {
        setBusy(true);
        try {
            const current = await branchLocationApi.fetchLocation(session, row.locationId);
            setBusy(false);
            await openForm({
                locationId: current.locationId,
                branchId: current.branchId > 0 ? String(current.branchId) : '',
                locationName: current.location || '',
            }, current);
        }
        catch (error) {
            setBusy(false);
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    function validate() {
        const next = {};
        const name = form.locationName.trim();
        if (!form.branchId) {
            next.branchId = 'Select the branch.';
        }
        if (!name) {
            next.locationName = 'Enter the location name.';
        }
        else if (name.length > LOCATION_NAME_MAX) {
            next.locationName = `Location name can be at most ${LOCATION_NAME_MAX} characters.`;
        }
        setErrors(next);
        return Object.keys(next).length === 0;
    }

    async function save() {
        if (!validate()) {
            return;
        }
        setBusy(true);
        setFormError('');
        try {
            const result = await branchLocationApi.saveLocation(session, {
                locationId: form.locationId,
                branchId: Number(form.branchId),
                locationName: form.locationName.trim(),
            });
            pushToast({ message: (result && result.message) || 'Branch location saved.' });
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
        if (!window.confirm(`Delete location ${row.location}?`)) {
            return;
        }
        try {
            const result = await branchLocationApi.deleteLocation(session, row.locationId);
            pushToast({ message: (result && result.message) || 'Branch location deleted.' });
            setRows((current) => current.filter((item) => item.locationId !== row.locationId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const branchOptions = branches.map((item) => ({ value: String(item.branchOfficeId), label: item.branchOfficeName }));

    function setField(key) {
        return (value) => {
            setForm((current) => ({ ...current, [key]: value }));
            setErrors((current) => ({ ...current, [key]: undefined }));
        };
    }

    return (<section className="lg-page" aria-label="Branch Location">
      <PageBar title="Branch Location" addLabel="Add Location" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.locationId} loading={loading} error={loadError} search={search} searchKeys={['location', 'branch']} rowActions={(row) => ({
          onEdit: row.isPrimary ? undefined : () => void openEdit(row),
          onDelete: () => void remove(row),
          deleteDisabled: row.isPrimary,
          deleteTitle: row.isPrimary ? 'The Primary location cannot be deleted' : 'Delete',
      })}/>

      <FormModal title={form && form.locationId ? 'Edit Branch Location' : 'Add Branch Location'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? (<>
            <SearchableSelect id="bl-branch" label="Branch" required value={form.branchId} options={branchOptions} placeholder="Select branch" error={errors.branchId} disabled={busy} onChange={setField('branchId')}/>
            <Input id="bl-name" label="Location" required value={form.locationName} error={errors.locationName} disabled={busy} onChange={setField('locationName')}/>
          </>) : null}
      </FormModal>
    </section>);
}
