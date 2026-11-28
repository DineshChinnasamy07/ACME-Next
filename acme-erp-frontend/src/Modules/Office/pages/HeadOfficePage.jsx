import { useCallback, useEffect, useState } from 'react';
import { Input, PasswordInput, SearchableSelect, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, StepFormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { isPortalSession } from '../../Common/services/apiRequest';
import { useCountryStates } from '../hooks/useCountryStates';
import { headOfficeApi, officeStatus } from '../services/officeApi';

const emptyOffice = {
    headOfficeId: 0, headOfficeCode: '', headOfficeName: '', typeId: '', belongsTo: '', inchargeName: '', designation: '',
    orgMailId: '', mobileNo: '', phoneNo: '', address: '', city: '', countryId: '', stateId: '', pincode: '', countryCode: '',
    srInchargeName: '', srMobileNo: '', srPhoneNo: '', srMailId: '', hostName: '', dbName: '', userName: '', password: '',
    accountingYearType: 0,
};

const columns = [
    { key: 'code', header: 'Code', width: '120px' },
    { key: 'name', header: 'Head Office Name' },
    { key: 'belongsTo', header: 'Belongs To' },
    { key: 'hoStatus', header: 'Status', width: '120px', render: (row) => <StatusText value={row.hoStatus}/> },
];

// Add / Edit is a 3-step form; each step lists the fields it validates before moving on.
// Senior Incharge fields were removed from the form (27-11-2026); existing values are kept on edit.
const STEPS = ['Head Office Info', 'Address', 'Database'];
const STEP_FIELDS = [
    ['headOfficeCode', 'headOfficeName', 'typeId', 'belongsTo', 'inchargeName', 'designation', 'orgMailId', 'mobileNo', 'phoneNo'],
    ['address', 'city', 'countryId', 'stateId', 'pincode', 'countryCode'],
    ['hostName', 'dbName', 'userName', 'password'],
];

function stepOf(field) {
    const index = STEP_FIELDS.findIndex((fields) => fields.includes(field));
    return index < 0 ? STEP_FIELDS.length - 1 : index;
}

function text(value) {
    return value === null || value === undefined ? '' : String(value);
}

// Show the one-time admin password the backend returns when it creates the office admin.
function announce(result, pushToast, fallback) {
    pushToast({ message: (result && result.message) || fallback });
    if (result && result.generatedAdminPassword) {
        window.alert(`Head office admin password (copy it now, it is shown only once):\n\n${result.generatedAdminPassword}`);
    }
}

export function HeadOfficePage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const portalUser = isPortalSession(session);
    const [rows, setRows] = useState([]);
    const [types, setTypes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [form, setForm] = useState(null);
    const [errors, setErrors] = useState({});
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);
    const [step, setStep] = useState(0);
    const { countryOptions, stateOptions, countryCodeOf } = useCountryStates(session, form ? form.countryId : '');

    const load = useCallback(async () => {
        if (!portalUser) {
            setLoading(false);
            return;
        }
        setLoading(true);
        setLoadError('');
        try {
            const [offices, typeList] = await Promise.all([headOfficeApi.fetchHeadOffices(session), headOfficeApi.fetchTypes(session)]);
            setRows(offices);
            setTypes(typeList);
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session, portalUser]);

    useEffect(() => { void load(); }, [load]);

    function set(field) {
        return (value) => {
            setForm((current) => {
                const next = { ...current, [field]: value };
                if (field === 'countryId') {
                    next.stateId = '';
                    next.countryCode = countryCodeOf(value) || current.countryCode;
                }
                return next;
            });
            setErrors((current) => ({ ...current, [field]: undefined }));
        };
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setStep(0);
        setForm({ ...emptyOffice });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setStep(0);
        setBusy(true);
        try {
            const detail = await headOfficeApi.fetchHeadOffice(session, row.headOfficeId);
            const next = { ...emptyOffice };
            Object.keys(emptyOffice).forEach((key) => { next[key] = text(detail[key]); });
            next.headOfficeId = detail.headOfficeId;
            next.accountingYearType = detail.accountingYearType || 0;
            next.password = '';
            setForm(next);
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Required: code and name; the database connection only when creating (it is provisioned on save).
    function validate() {
        const next = {};
        const creating = form.headOfficeId === 0;
        if (!form.headOfficeCode.trim()) next.headOfficeCode = 'Head Office Code is required.';
        if (!form.headOfficeName.trim()) next.headOfficeName = 'Head Office Name is required.';
        if (creating) {
            if (!form.hostName.trim()) next.hostName = 'Host Name is required.';
            if (!form.dbName.trim()) next.dbName = 'Database Name is required.';
            if (!form.userName.trim()) next.userName = 'DB User Name is required.';
            if (!form.password) next.password = 'DB Password is required.';
        }
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
            // Go back to the first step that still has a problem.
            setStep(Math.min(...failed.map(stepOf)));
            return;
        }
        setBusy(true);
        setFormError('');
        try {
            const request = {
                ...form,
                typeId: form.typeId ? Number(form.typeId) : null,
                countryId: form.countryId ? Number(form.countryId) : null,
                stateId: form.stateId ? Number(form.stateId) : null,
                // Not on the form any more: a new head office saves them empty, an edit sends back what was loaded.
                ...(form.headOfficeId === 0 ? { srInchargeName: '', srMailId: '', srMobileNo: '', srPhoneNo: '' } : {}),
            };
            const result = await headOfficeApi.saveHeadOffice(session, request);
            announce(result, pushToast, 'Head office saved.');
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
        const activate = row.status !== officeStatus.activated;
        if (!window.confirm(`${activate ? 'Activate' : 'Deactivate'} head office ${row.name}?`)) {
            return;
        }
        try {
            const result = await headOfficeApi.updateStatus(session, row.code, activate ? officeStatus.activated : officeStatus.deactivated);
            announce(result, pushToast, 'Status updated.');
            await load();
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    async function remove(row) {
        if (!window.confirm(`Delete head office ${row.name} (${row.code})?`)) {
            return;
        }
        try {
            const result = await headOfficeApi.deleteHeadOffice(session, row.code);
            pushToast({ message: (result && result.message) || 'Head office deleted.' });
            setRows((current) => current.filter((item) => item.headOfficeId !== row.headOfficeId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    if (!portalUser) {
        return (<section className="lg-page" aria-label="Head Office">
          <div className="lg-bar"><h1 className="lg-title">Head Office</h1></div>
          <p className="lg-note">Head offices are managed by the portal administrator. Sign in with the portal account to use this screen.</p>
        </section>);
    }

    const isNew = form && form.headOfficeId === 0;
    const typeOptions = types.map((type) => ({ value: String(type.typeId), label: type.type }));

    return (<section className="lg-page" aria-label="Head Office">
      <PageBar title="Head Office" addLabel="Add Head Office" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.headOfficeId} loading={loading} error={loadError} search={search} searchKeys={['code', 'name', 'belongsTo', 'hoStatus']} rowActions={(row) => {
          const activated = row.status === officeStatus.activated;
          return {
              onEdit: () => void openEdit(row),
              onToggle: () => void toggleStatus(row),
              toggleTitle: activated ? 'Deactivate' : 'Activate',
              onDelete: () => void remove(row),
              deleteDisabled: activated,
              deleteTitle: activated ? 'Deactivate the head office before deleting' : 'Delete',
          };
      }}/>

      <StepFormModal title={isNew ? 'Add Head Office' : 'Edit Head Office'} isOpen={Boolean(form)} busy={busy} error={formError} steps={STEPS} step={step} onStep={(index) => { setErrors({}); setStep(index); }} onNext={checkStep} onClose={() => setForm(null)} onSave={() => void save()}>
        {form && step === 0 ? <FieldGrid>
          <Input id="ho-code" label="Head Office Code" required value={form.headOfficeCode} error={errors.headOfficeCode} onChange={set('headOfficeCode')}/>
          <Input id="ho-name" label="Head Office Name" required value={form.headOfficeName} error={errors.headOfficeName} onChange={set('headOfficeName')}/>
          <Select id="ho-type" label="Type" value={form.typeId} options={typeOptions} onChange={set('typeId')}/>
          <Input id="ho-belongs" label="Belongs To" value={form.belongsTo} onChange={set('belongsTo')}/>
          <Input id="ho-incharge" label="Incharge Name" value={form.inchargeName} onChange={set('inchargeName')}/>
          <Input id="ho-designation" label="Designation" value={form.designation} onChange={set('designation')}/>
          <Input id="ho-mail" label="Organisation Email" type="email" value={form.orgMailId} onChange={set('orgMailId')}/>
          <Input id="ho-mobile" label="Mobile No" value={form.mobileNo} onChange={set('mobileNo')}/>
          <Input id="ho-phone" label="Phone No" value={form.phoneNo} onChange={set('phoneNo')}/>
        </FieldGrid> : null}

        {form && step === 1 ? <FieldGrid>
          <Input id="ho-address" label="Address" value={form.address} onChange={set('address')}/>
          <Input id="ho-city" label="City" value={form.city} onChange={set('city')}/>
          <SearchableSelect id="ho-country" label="Country" value={form.countryId} options={countryOptions} onChange={set('countryId')}/>
          <SearchableSelect id="ho-state" label="State" value={form.stateId} options={stateOptions} disabled={!form.countryId} onChange={set('stateId')}/>
          <Input id="ho-pincode" label="Pincode" value={form.pincode} onChange={set('pincode')}/>
          <Input id="ho-cc" label="Country Code" value={form.countryCode} onChange={set('countryCode')}/>
        </FieldGrid> : null}

        {form && step === 2 ? <FieldGrid>
          <Input id="ho-host" label={isNew ? 'Host Name (database is created on save)' : 'Host Name'} required={isNew} value={form.hostName} error={errors.hostName} onChange={set('hostName')}/>
          <Input id="ho-db" label="Database Name" required={isNew} value={form.dbName} error={errors.dbName} onChange={set('dbName')}/>
          <Input id="ho-dbuser" label="DB User Name" required={isNew} value={form.userName} error={errors.userName} autoComplete="off" onChange={set('userName')}/>
          <PasswordInput id="ho-dbpass" label={isNew ? 'DB Password' : 'DB Password (leave blank to keep)'} required={isNew} autoComplete="new-password" value={form.password} error={errors.password} onChange={set('password')}/>
        </FieldGrid> : null}
      </StepFormModal>
    </section>);
}
