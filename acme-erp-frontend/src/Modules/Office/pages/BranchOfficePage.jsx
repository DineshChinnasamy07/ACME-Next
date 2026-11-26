import { useCallback, useEffect, useState } from 'react';
import { Input, SearchableSelect, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, StepFormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { isPortalSession } from '../../Common/services/apiRequest';
import { useCountryStates } from '../hooks/useCountryStates';
import { branchOfficeApi, headOfficeApi, officeStatus } from '../services/officeApi';

const emptyBranch = {
    branchOfficeId: 0, headOfficeCode: '', branchPartCode: '', branchOfficeName: '', deploymentType: '0', personIncharge: '',
    isSubBranch: '0', mobileNo: '', phoneNo: '', branchEmail: '', address: '', city: '', countryId: '', stateId: '',
    pincode: '', countryCode: '', thirdPartyCode: '',
};

const deploymentOptions = [{ value: '0', label: 'Standalone' }, { value: '1', label: 'Client-Server' }];
const yesNoOptions = [{ value: '0', label: 'No' }, { value: '1', label: 'Yes' }];

// Add / Edit is a 2-step form; each step lists the fields it validates before moving on.
const STEPS = ['Branch Info', 'Address'];
const STEP_FIELDS = [
    ['headOfficeCode', 'branchPartCode', 'branchOfficeName', 'personIncharge', 'deploymentType', 'isSubBranch', 'mobileNo', 'phoneNo', 'branchEmail', 'thirdPartyCode'],
    ['address', 'city', 'countryId', 'stateId', 'pincode', 'countryCode'],
];

function stepOf(field) {
    const index = STEP_FIELDS.findIndex((fields) => fields.includes(field));
    return index < 0 ? STEP_FIELDS.length - 1 : index;
}

function text(value) {
    return value === null || value === undefined ? '' : String(value);
}

// Full branch code the status and delete endpoints expect: head office code + branch part code.
function fullCode(row) {
    return `${row.headOfficeCode || ''}${row.code || ''}`;
}

function announce(result, pushToast, fallback) {
    pushToast({ message: (result && result.message) || fallback });
    if (result && result.generatedAdminPassword) {
        window.alert(`Branch admin password (copy it now, it is shown only once):\n\n${result.generatedAdminPassword}`);
    }
}

export function BranchOfficePage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const portalUser = isPortalSession(session);
    const [rows, setRows] = useState([]);
    const [headOffices, setHeadOffices] = useState([]);
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
        setLoading(true);
        setLoadError('');
        try {
            const [branches, offices] = await Promise.all([
                branchOfficeApi.fetchBranchOffices(session),
                portalUser ? headOfficeApi.fetchActiveHeadOffices(session) : Promise.resolve([]),
            ]);
            setRows(branches);
            setHeadOffices(offices);
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
        setForm({ ...emptyBranch, headOfficeCode: portalUser ? '' : session.headOfficeId });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setStep(0);
        setBusy(true);
        try {
            const detail = await branchOfficeApi.fetchBranchOffice(session, row.branchOfficeId);
            const next = { ...emptyBranch };
            Object.keys(emptyBranch).forEach((key) => { next[key] = text(detail[key]); });
            next.branchOfficeId = detail.branchOfficeId;
            next.deploymentType = text(detail.deploymentType ?? 0);
            next.isSubBranch = text(detail.isSubBranch ?? 0);
            setForm(next);
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Required: head office, branch code and branch name.
    function validate() {
        const next = {};
        if (!form.headOfficeCode.trim()) next.headOfficeCode = 'Head Office is required.';
        if (!form.branchPartCode.trim()) next.branchPartCode = 'Branch Code is required.';
        if (!form.branchOfficeName.trim()) next.branchOfficeName = 'Branch Office Name is required.';
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
                deploymentType: Number(form.deploymentType),
                isSubBranch: Number(form.isSubBranch),
                countryId: form.countryId ? Number(form.countryId) : null,
                stateId: form.stateId ? Number(form.stateId) : null,
            };
            const result = await branchOfficeApi.saveBranchOffice(session, request);
            announce(result, pushToast, 'Branch office saved.');
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
        if (!window.confirm(`${activate ? 'Activate' : 'Deactivate'} branch office ${row.name}?`)) {
            return;
        }
        try {
            const result = await branchOfficeApi.updateStatus(session, fullCode(row), activate ? officeStatus.activated : officeStatus.deactivated);
            announce(result, pushToast, 'Status updated.');
            await load();
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    async function remove(row) {
        if (!window.confirm(`Delete branch office ${row.name} (${fullCode(row)})?`)) {
            return;
        }
        try {
            const result = await branchOfficeApi.deleteBranchOffice(session, fullCode(row));
            pushToast({ message: (result && result.message) || 'Branch office deleted.' });
            setRows((current) => current.filter((item) => item.branchOfficeId !== row.branchOfficeId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const columns = [
        ...(portalUser ? [{ key: 'headOfficeCode', header: 'Head Office', width: '110px' }] : []),
        { key: 'code', header: 'Code', width: '100px' },
        { key: 'name', header: 'Branch Office Name' },
        { key: 'systemType', header: 'System Type', width: '120px' },
        { key: 'subBranch', header: 'Sub Branch', width: '95px' },
        { key: 'officeStatus', header: 'Status', width: '110px', render: (row) => <StatusText value={row.officeStatus}/> },
    ];
    const headOfficeOptions = headOffices.map((office) => ({ value: office.headOfficeCode, label: office.headOfficeCode }));
    const isNew = form && form.branchOfficeId === 0;

    return (<section className="lg-page" aria-label="Branch Office">
      <PageBar title="Branch Office" addLabel="Add Branch Office" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.branchOfficeId} loading={loading} error={loadError} search={search} searchKeys={['headOfficeCode', 'code', 'name', 'systemType', 'officeStatus']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onToggle: () => void toggleStatus(row),
          toggleTitle: row.status === officeStatus.activated ? 'Deactivate' : 'Activate',
          onDelete: () => void remove(row),
      })}/>

      <StepFormModal title={isNew ? 'Add Branch Office' : 'Edit Branch Office'} isOpen={Boolean(form)} busy={busy} error={formError} steps={STEPS} step={step} onStep={(index) => { setErrors({}); setStep(index); }} onNext={checkStep} onClose={() => setForm(null)} onSave={() => void save()}>
        {form && step === 0 ? <FieldGrid>
          {portalUser && isNew
            ? <SearchableSelect id="bo-ho" label="Head Office" required value={form.headOfficeCode} options={headOfficeOptions} placeholder="Select head office" error={errors.headOfficeCode} onChange={set('headOfficeCode')}/>
            : <Input id="bo-ho" label="Head Office" required value={form.headOfficeCode} error={errors.headOfficeCode} disabled onChange={() => {}}/>}
          <Input id="bo-code" label="Branch Code" required value={form.branchPartCode} error={errors.branchPartCode} onChange={set('branchPartCode')}/>
          <Input id="bo-name" label="Branch Office Name" required value={form.branchOfficeName} error={errors.branchOfficeName} onChange={set('branchOfficeName')}/>
          <Input id="bo-incharge" label="Person Incharge" value={form.personIncharge} onChange={set('personIncharge')}/>
          <Select id="bo-deploy" label="System Type" hidePlaceholder value={form.deploymentType} options={deploymentOptions} onChange={set('deploymentType')}/>
          <Select id="bo-sub" label="Sub Branch" hidePlaceholder value={form.isSubBranch} options={yesNoOptions} onChange={set('isSubBranch')}/>
          <Input id="bo-mobile" label="Mobile No" value={form.mobileNo} onChange={set('mobileNo')}/>
          <Input id="bo-phone" label="Phone No" value={form.phoneNo} onChange={set('phoneNo')}/>
          <Input id="bo-email" label="Email" type="email" value={form.branchEmail} onChange={set('branchEmail')}/>
          <Input id="bo-third" label="Third Party Code" value={form.thirdPartyCode} onChange={set('thirdPartyCode')}/>
        </FieldGrid> : null}

        {form && step === 1 ? <FieldGrid>
          <Input id="bo-address" label="Address" value={form.address} onChange={set('address')}/>
          <Input id="bo-city" label="City" value={form.city} onChange={set('city')}/>
          <SearchableSelect id="bo-country" label="Country" value={form.countryId} options={countryOptions} onChange={set('countryId')}/>
          <SearchableSelect id="bo-state" label="State" value={form.stateId} options={stateOptions} disabled={!form.countryId} onChange={set('stateId')}/>
          <Input id="bo-pincode" label="Pincode" value={form.pincode} onChange={set('pincode')}/>
          <Input id="bo-cc" label="Country Code" value={form.countryCode} onChange={set('countryCode')}/>
        </FieldGrid> : null}
      </StepFormModal>
    </section>);
}
