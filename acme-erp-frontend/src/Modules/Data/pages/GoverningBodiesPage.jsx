import { useCallback, useEffect, useState } from 'react';
import { Input, SearchableSelect } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, StepFormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { useCountryStates } from '../../Office/hooks/useCountryStates';
import { countryApi } from '../../Office/services/officeApi';
import { EMAIL_PATTERN, governingBodyApi, requestDate, toInputDate } from '../services/statutoryLegalApi';

/*
 * Data › Statutory Masters › Governing Bodies (Head Office). Legacy GoverningMemberView.aspx /
 * GoverningMemberAdd.aspx, API api/governingbodies. Rules (legacy ValidGoverningMember + GoverningBodiesRepository):
 * Name, Nationality, Country and State required; Date of Joining / Date of Exit not before Date of Birth and
 * Date of Exit not before Date of Joining; valid email when given; master_executive_committee column limits.
 * A member mapped to a legal entity cannot be deleted (server).
 * Country / State come from the Organization service (legacy combos used the portal COUNTRY / STATE tables).
 */

const DEFAULT_COUNTRY = 'india';
const DEFAULT_STATE = 'tamil nadu';

// Column limits (master_executive_committee).
const MAX = {
    executive: 50, name: 50, religion: 30, role: 30, nationality: 25, occupation: 25, association: 50,
    officeBearer: 50, place: 30, address: 150, pinCode: 10, panSsn: 10, phone: 20, fax: 20, email: 30, url: 30,
    notes: 500,
};

const CAPTION = {
    executive: 'Name', name: "Father/Husband's Name", religion: 'Religion', role: 'Role', nationality: 'Nationality',
    occupation: 'Occupation', association: 'Office Held in Association', officeBearer: 'Relationship with Office Bearer',
    place: 'Place', address: 'Address', pinCode: 'Pin/Zip Code', panSsn: 'PAN/UID #', phone: 'Contact No', fax: 'Fax',
    email: 'Email', url: 'URL', notes: 'Notes',
};

const emptyMember = {
    executiveId: 0, executive: '', name: '', dateOfBirth: '', religion: '', role: '', nationality: '', occupation: '',
    association: '', officeBearer: '', place: '', countryId: '', stateId: '', address: '', pinCode: '', panSsn: '',
    phone: '', fax: '', email: '', url: '', dateOfAppointment: '', dateOfExit: '', notes: '', defaultState: false,
};

function text(value) {
    return value === null || value === undefined ? '' : String(value);
}

function idValue(value) {
    return value && Number(value) > 0 ? String(value) : '';
}

function TextArea({ id, label, value, onChange, error, rows = 2 }) {
    return (<div className="field">
      <label htmlFor={id}>{label}</label>
      <textarea id={id} className={error ? 'control lg-textarea is-invalid' : 'control lg-textarea'} rows={rows} value={value} onChange={(event) => onChange(event.target.value)}/>
      {error ? <p className="field-error">{error}</p> : null}
    </div>);
}

// Add / Edit is a 3-step form; each step lists the fields it validates before moving on.
const STEPS = ['Members Info', 'Address', 'Tenure'];
const STEP_FIELDS = [
    ['executive', 'occupation', 'dateOfBirth', 'nationality', 'religion', 'role', 'name', 'officeBearer', 'association'],
    ['countryId', 'stateId', 'panSsn', 'fax', 'pinCode', 'address', 'place', 'phone', 'email', 'url'],
    ['dateOfAppointment', 'dateOfExit', 'notes'],
];

function stepOf(field) {
    const index = STEP_FIELDS.findIndex((fields) => fields.includes(field));
    return index < 0 ? STEP_FIELDS.length - 1 : index;
}

const columns = [
    { key: 'executive', header: 'Name', wrap: true },
    { key: 'name', header: 'Name of the Father/Husband', wrap: true },
    { key: 'nationality', header: 'Nationality' },
    { key: 'occupation', header: 'Occupation' },
    { key: 'stateName', header: 'State' },
    { key: 'countryName', header: 'Country' },
];

// Country / state names for the grid (one states call per distinct country).
async function resolvePlaces(session, members) {
    const countries = await countryApi.fetchCountries(session).catch(() => []);
    const countryNames = new Map(countries.map((item) => [Number(item.countryId), item.country]));
    const countryIds = [...new Set(members.map((row) => Number(row.countryId)).filter((id) => id > 0))];
    const stateNames = new Map();
    await Promise.all(countryIds.map(async (countryId) => {
        const states = await countryApi.fetchStates(session, countryId).catch(() => []);
        states.forEach((item) => stateNames.set(Number(item.stateId), item.state));
    }));
    return members.map((row) => ({
        ...row,
        countryName: countryNames.get(Number(row.countryId)) || '',
        stateName: stateNames.get(Number(row.stateId)) || '',
    }));
}

export function GoverningBodiesPage() {
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
    const [step, setStep] = useState(0);
    const { countryOptions, stateOptions } = useCountryStates(session, form ? form.countryId : '');

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            const members = await governingBodyApi.fetchMembers(session);
            setRows(await resolvePlaces(session, members));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    // Legacy SetDefaultCoutryState: a new member starts with India / Tamil Nadu when present.
    useEffect(() => {
        if (!form || !form.defaultState || form.stateId || stateOptions.length === 0) {
            return;
        }
        const match = stateOptions.find((option) => option.label.trim().toLowerCase() === DEFAULT_STATE);
        setForm((current) => (current ? { ...current, defaultState: false, stateId: match ? match.value : current.stateId } : current));
    }, [form, stateOptions]);

    function set(field) {
        return (value) => {
            setForm((current) => {
                const next = { ...current, [field]: value };
                if (field === 'countryId' && value !== current.countryId) {
                    next.stateId = '';
                    next.defaultState = false;
                }
                return next;
            });
            setErrors((current) => ({ ...current, [field]: undefined, ...(field === 'countryId' ? { stateId: undefined } : {}) }));
        };
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setStep(0);
        const india = countryOptions.find((option) => option.label.trim().toLowerCase() === DEFAULT_COUNTRY);
        setForm({ ...emptyMember, countryId: india ? india.value : '', defaultState: Boolean(india) });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setStep(0);
        setBusy(true);
        try {
            const detail = await governingBodyApi.fetchMember(session, row.executiveId);
            setForm({
                executiveId: detail.executiveId,
                executive: text(detail.executive),
                name: text(detail.name),
                dateOfBirth: toInputDate(detail.dateOfBirth),
                religion: text(detail.religion),
                role: text(detail.role),
                nationality: text(detail.nationality),
                occupation: text(detail.occupation),
                association: text(detail.association),
                officeBearer: text(detail.officeBearer),
                place: text(detail.place),
                countryId: idValue(detail.countryId),
                stateId: idValue(detail.stateId),
                address: text(detail.address),
                pinCode: text(detail.pinCode),
                panSsn: text(detail.panSsn),
                phone: text(detail.phone),
                fax: text(detail.fax),
                email: text(detail.email),
                url: text(detail.url),
                dateOfAppointment: toInputDate(detail.dateOfAppointment),
                dateOfExit: toInputDate(detail.dateOfExit),
                notes: text(detail.notes),
                defaultState: false,
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
        if (!form.executive.trim()) next.executive = 'Name is required.';
        if (!form.nationality.trim()) next.nationality = 'Nationality is required.';
        if (form.dateOfBirth && form.dateOfAppointment && form.dateOfAppointment < form.dateOfBirth) {
            next.dateOfAppointment = "Date of Joining can't be less than Date of Birth.";
        }
        if (form.dateOfBirth && form.dateOfExit && form.dateOfExit < form.dateOfBirth) {
            next.dateOfExit = "Date of Exit can't be less than Date of Birth.";
        }
        else if (form.dateOfAppointment && form.dateOfExit && form.dateOfExit < form.dateOfAppointment) {
            next.dateOfExit = "Date of Exit can't be less than Date of Joining.";
        }
        if (!form.countryId) next.countryId = 'Country is required.';
        if (!form.stateId) next.stateId = 'State is required.';
        if (form.email.trim() && !EMAIL_PATTERN.test(form.email.trim())) next.email = 'Enter a valid email address.';
        Object.keys(MAX).forEach((field) => {
            if (!next[field] && text(form[field]).trim().length > MAX[field]) {
                next[field] = `${CAPTION[field]} cannot exceed ${MAX[field]} characters.`;
            }
        });
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
                executiveId: form.executiveId,
                executive: form.executive.trim(),
                name: form.name.trim(),
                dateOfBirth: requestDate(form.dateOfBirth),
                religion: form.religion.trim(),
                role: form.role.trim(),
                nationality: form.nationality.trim(),
                occupation: form.occupation.trim(),
                association: form.association.trim(),
                officeBearer: form.officeBearer.trim(),
                place: form.place.trim(),
                countryId: Number(form.countryId),
                stateId: Number(form.stateId),
                address: form.address.trim(),
                pinCode: form.pinCode.trim(),
                panSsn: form.panSsn.trim().toUpperCase(),
                phone: form.phone.trim(),
                fax: form.fax.trim(),
                email: form.email.trim(),
                url: form.url.trim(),
                dateOfAppointment: requestDate(form.dateOfAppointment),
                dateOfExit: requestDate(form.dateOfExit),
                notes: form.notes.trim(),
            };
            const result = await governingBodyApi.saveMember(session, request);
            pushToast({ message: (result && result.message) || 'Governing body member saved.' });
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
        if (!window.confirm(`Delete governing body member ${row.executive}?`)) {
            return;
        }
        try {
            const result = await governingBodyApi.deleteMember(session, row.executiveId);
            pushToast({ message: (result && result.message) || 'Governing body member deleted.' });
            setRows((current) => current.filter((item) => item.executiveId !== row.executiveId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.executiveId === 0;

    return (<section className="lg-page" aria-label="Governing Bodies">
      <PageBar title="Governing Bodies" addLabel="Add Member" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.executiveId} loading={loading} error={loadError} search={search} searchKeys={['executive', 'name', 'nationality', 'occupation', 'stateName', 'countryName']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <StepFormModal title={isNew ? 'Add Governing Body Member' : 'Edit Governing Body Member'} isOpen={Boolean(form)} busy={busy} error={formError} steps={STEPS} step={step} onStep={(index) => { setErrors({}); setStep(index); }} onNext={checkStep} onClose={() => setForm(null)} onSave={() => void save()}>
        {form && step === 0 ? <FieldGrid>
          <Input id="gb-name" label="Name" required value={form.executive} error={errors.executive} onChange={set('executive')}/>
          <Input id="gb-occupation" label="Occupation" value={form.occupation} error={errors.occupation} onChange={set('occupation')}/>
          <Input id="gb-dob" label="Date of Birth" type="date" value={form.dateOfBirth} error={errors.dateOfBirth} onChange={set('dateOfBirth')}/>
          <Input id="gb-nationality" label="Nationality" required value={form.nationality} error={errors.nationality} onChange={set('nationality')}/>
          <Input id="gb-religion" label="Religion" value={form.religion} error={errors.religion} onChange={set('religion')}/>
          <Input id="gb-role" label="Role" value={form.role} error={errors.role} onChange={set('role')}/>
          <Input id="gb-father" label="Father/Husband's Name" value={form.name} error={errors.name} onChange={set('name')}/>
          <Input id="gb-bearer" label="Relationship with Office Bearer" value={form.officeBearer} error={errors.officeBearer} onChange={set('officeBearer')}/>
          <Input id="gb-office" label="Office Held in Association" value={form.association} error={errors.association} onChange={set('association')}/>
        </FieldGrid> : null}

        {form && step === 1 ? <FieldGrid>
          <SearchableSelect id="gb-country" label="Country" required value={form.countryId} options={countryOptions} placeholder="Select country" error={errors.countryId} onChange={set('countryId')}/>
          <SearchableSelect id="gb-state" label="State/Province" required value={form.stateId} options={stateOptions} placeholder="Select state" disabled={!form.countryId} error={errors.stateId} onChange={set('stateId')}/>
          <Input id="gb-pan" label="PAN/UID #" value={form.panSsn} error={errors.panSsn} onChange={(value) => set('panSsn')(value.toUpperCase())}/>
          <Input id="gb-fax" label="Fax" value={form.fax} error={errors.fax} onChange={set('fax')}/>
          <Input id="gb-pin" label="Pin/Zip Code" value={form.pinCode} error={errors.pinCode} onChange={set('pinCode')}/>
          <TextArea id="gb-address" label="Address" value={form.address} error={errors.address} onChange={set('address')}/>
          <Input id="gb-place" label="Place" value={form.place} error={errors.place} onChange={set('place')}/>
          <Input id="gb-phone" label="Contact No" value={form.phone} error={errors.phone} onChange={set('phone')}/>
          <Input id="gb-email" label="Email" type="email" value={form.email} error={errors.email} onChange={set('email')}/>
          <Input id="gb-url" label="URL" value={form.url} error={errors.url} onChange={set('url')}/>
        </FieldGrid> : null}

        {form && step === 2 ? <FieldGrid>
          <Input id="gb-join" label="Date of Joining" type="date" value={form.dateOfAppointment} error={errors.dateOfAppointment} onChange={set('dateOfAppointment')}/>
          <Input id="gb-exit" label="Date of Exit" type="date" value={form.dateOfExit} error={errors.dateOfExit} onChange={set('dateOfExit')}/>
          <TextArea id="gb-notes" label="Notes" rows={3} value={form.notes} error={errors.notes} onChange={set('notes')}/>
        </FieldGrid> : null}
      </StepFormModal>
    </section>);
}
