import { useCallback, useEffect, useState } from 'react';
import { Input } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { roleApi } from '../services/userAdminApi';

const columns = [
    { key: 'name', header: 'Role Name' },
    { key: 'userCount', header: 'Users', width: '90px' },
    { key: 'isSystem', header: 'Type', width: '110px', render: (row) => (row.isSystem ? 'System' : 'Custom') },
];

export function UserRolePage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [form, setForm] = useState(null);
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            setRows(await roleApi.fetchRoles(session));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    async function save() {
        if (!form.name.trim()) {
            setFormError('Enter the role name.');
            return;
        }
        if (form.name.trim().length > 30) {
            setFormError('Role name can be at most 30 characters.');
            return;
        }
        setBusy(true);
        setFormError('');
        try {
            const result = await roleApi.saveRole(session, form.userRoleId, form.name.trim());
            pushToast({ message: (result && result.message) || 'Role saved.' });
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
        if (!window.confirm(`Delete role ${row.name}?`)) {
            return;
        }
        try {
            const result = await roleApi.deleteRole(session, row.userRoleId);
            pushToast({ message: (result && result.message) || 'Role deleted.' });
            setRows((current) => current.filter((item) => item.userRoleId !== row.userRoleId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    return (<section className="lg-page" aria-label="User Role">
      <PageBar title="User Role" addLabel="Add Role" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={() => { setFormError(''); setForm({ userRoleId: 0, name: '' }); }}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.userRoleId} loading={loading} error={loadError} search={search} searchKeys={['name']} rowActions={(row) => ({
          onEdit: row.isSystem ? undefined : () => { setFormError(''); setForm({ userRoleId: row.userRoleId, name: row.name }); },
          onDelete: () => void remove(row),
          deleteDisabled: row.isSystem || row.userCount > 0,
          deleteTitle: row.isSystem ? 'System roles cannot be deleted' : row.userCount > 0 ? 'Role is assigned to users' : 'Delete',
      })}/>

      <FormModal title={form && form.userRoleId ? 'Edit Role' : 'Add Role'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <Input id="role-name" label="Role Name" required value={form.name} onChange={(value) => setForm((current) => ({ ...current, name: value }))}/> : null}
      </FormModal>
    </section>);
}
