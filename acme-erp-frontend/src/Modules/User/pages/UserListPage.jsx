import { useCallback, useEffect, useState } from 'react';
import { Input, PasswordInput, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, StepFormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { userApi } from '../services/userAdminApi';

const ACTIVE = 1;
const ADMIN_USER_ID = 1;

const emptyUser = {
    userId: 0, firstName: '', lastName: '', userName: '', password: '', mobileNo: '', email: '',
    roleId: '', gender: 'M', address: '', place: '', city: '', countryCode: '', notes: '',
};

// Add / Edit is a 2-step form; each step lists the fields it validates before moving on.
const STEPS = ['User Info', 'Address & Contact'];
const STEP_FIELDS = [
    ['firstName', 'lastName', 'userName', 'password', 'roleId', 'gender', 'mobileNo', 'email'],
    ['address', 'place', 'city', 'countryCode', 'notes'],
];

function stepOf(field) {
    const index = STEP_FIELDS.findIndex((fields) => fields.includes(field));
    return index < 0 ? STEP_FIELDS.length - 1 : index;
}

const columns = [
    { key: 'name', header: 'Name' },
    { key: 'userName', header: 'User Name' },
    { key: 'userRole', header: 'Role' },
    { key: 'contactNo', header: 'Contact' },
    { key: 'email', header: 'Email' },
    { key: 'userStatus', header: 'Status', render: (row) => <StatusText value={row.userStatus}/> },
];

export function UserListPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [roles, setRoles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [form, setForm] = useState(null);
    const [errors, setErrors] = useState({});
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);
    const [step, setStep] = useState(0);

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            const [users, roleOptions] = await Promise.all([userApi.fetchUsers(session), userApi.fetchRoleOptions(session)]);
            setRows(users);
            setRoles(roleOptions);
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
        setStep(0);
        setForm({ ...emptyUser });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setStep(0);
        setBusy(true);
        try {
            const detail = await userApi.fetchUser(session, row.userId);
            setForm({ ...emptyUser, ...detail, password: '', roleId: detail && detail.roleId ? String(detail.roleId) : '' });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Required: First Name, User Name, Mobile No, Email, Role, and Password for a new user.
    function validate() {
        const next = {};
        const isNew = form.userId === 0;
        if (!form.firstName.trim()) next.firstName = 'First Name is required.';
        if (!form.userName.trim()) next.userName = 'User Name is required.';
        if (isNew && !form.password) next.password = 'Password is required.';
        if (!form.roleId) next.roleId = 'Select the role.';
        if (!form.mobileNo.trim()) next.mobileNo = 'Mobile No is required.';
        if (!form.email.trim()) next.email = 'Email is required.';
        return next;
    }

    // Next: only the current step's fields must be valid.
    function checkStep(index) {
        const stepErrors = Object.fromEntries(Object.entries(validate()).filter(([field]) => stepOf(field) === index));
        setErrors(stepErrors);
        return Object.keys(stepErrors).length === 0;
    }

    async function save() {
        const next = validate();
        setErrors(next);
        const failed = Object.keys(next);
        if (failed.length > 0) {
            setFormError('Fill in all fields marked *.');
            // Go back to the first step that still has a problem.
            setStep(Math.min(...failed.map(stepOf)));
            return;
        }
        setBusy(true);
        setFormError('');
        try {
            const result = await userApi.saveUser(session, { ...form, roleId: Number(form.roleId) });
            pushToast({ message: (result && result.message) || 'User saved.' });
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

    async function toggleStatus(row) {
        const next = row.status === ACTIVE ? 0 : ACTIVE;
        if (!window.confirm(`${next === ACTIVE ? 'Activate' : 'Deactivate'} user ${row.name}?`)) {
            return;
        }
        try {
            const result = await userApi.updateStatus(session, row.userId, next);
            pushToast({ message: (result && result.message) || 'Status updated.' });
            await load();
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    async function remove(row) {
        if (!window.confirm(`Delete user ${row.name}?`)) {
            return;
        }
        try {
            const result = await userApi.deleteUser(session, row.userId);
            pushToast({ message: (result && result.message) || 'User deleted.' });
            setRows((current) => current.filter((item) => item.userId !== row.userId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const roleOptions = roles.map((role) => ({ value: String(role.userRoleId), label: role.name }));
    const isNew = form && form.userId === 0;

    return (<section className="lg-page" aria-label="Users">
      <PageBar title="User" addLabel="Add User" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.userId} loading={loading} error={loadError} search={search} searchKeys={['name', 'userName', 'userRole', 'contactNo', 'email']} rowActions={(row) => {
          const isAdmin = row.userId === ADMIN_USER_ID;
          const isActive = row.status === ACTIVE;
          return {
              onEdit: () => void openEdit(row),
              onToggle: () => void toggleStatus(row),
              toggleTitle: isActive ? 'Deactivate' : 'Activate',
              toggleDisabled: isAdmin,
              onDelete: () => void remove(row),
              deleteDisabled: isAdmin || isActive,
              deleteTitle: isAdmin ? 'The admin user cannot be deleted' : isActive ? 'Deactivate the user before deleting' : 'Delete',
          };
      }}/>

      <StepFormModal title={isNew ? 'Add User' : 'Edit User'} isOpen={Boolean(form)} busy={busy} error={formError} steps={STEPS} step={step} onStep={(index) => { setErrors({}); setStep(index); }} onNext={checkStep} onClose={() => setForm(null)} onSave={() => void save()}>
        {form && step === 0 ? <FieldGrid>
          <Input id="u-first" label="First Name" required value={form.firstName} error={errors.firstName} onChange={set('firstName')}/>
          <Input id="u-last" label="Last Name" value={form.lastName} onChange={set('lastName')}/>
          <Input id="u-user" label="User Name" required value={form.userName} autoComplete="off" error={errors.userName} onChange={set('userName')}/>
          {isNew ? <PasswordInput id="u-pass" label="Password" required value={form.password} autoComplete="new-password" error={errors.password} onChange={set('password')}/> : null}
          <Select id="u-role" label="Role" required value={form.roleId} options={roleOptions} error={errors.roleId} onChange={set('roleId')}/>
          <Select id="u-gender" label="Gender" hidePlaceholder value={form.gender} options={[{ value: 'M', label: 'Male' }, { value: 'F', label: 'Female' }]} onChange={set('gender')}/>
          <Input id="u-mobile" label="Mobile No" required value={form.mobileNo} error={errors.mobileNo} onChange={set('mobileNo')}/>
          <Input id="u-email" label="Email" required type="email" value={form.email} error={errors.email} onChange={set('email')}/>
        </FieldGrid> : null}

        {form && step === 1 ? <FieldGrid>
          <Input id="u-address" label="Address" value={form.address} onChange={set('address')}/>
          <Input id="u-place" label="Place" value={form.place} onChange={set('place')}/>
          <Input id="u-city" label="City" value={form.city} onChange={set('city')}/>
          <Input id="u-cc" label="Country Code" value={form.countryCode} onChange={set('countryCode')}/>
          <Input id="u-notes" label="Notes" value={form.notes} onChange={set('notes')}/>
        </FieldGrid> : null}
      </StepFormModal>
    </section>);
}
