import { useCallback, useEffect, useMemo, useState } from 'react';
import { Input, SearchableSelect, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FieldSection, StepFormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { countryApi } from '../../Office/services/officeApi';
import { EMAIL_PATTERN, legalEntityApi, requestDate, toGridDate, toInputDate, today } from '../services/statutoryLegalApi';

/*
 * Data › Statutory Masters › Legal Entity (Head Office). Legacy LegalEntityView.aspx / LegalEntityAdd.aspx,
 * API api/legalentities. Rules (legacy ValidateUserControl + LegalEntityRepository): Society Name, Country,
 * State/Province and Association Nature required; dates not in the future; "Others" association needs its
 * text; Religious needs a Denomination ("Others" needs its text); society name unique (server). Delete is
 * refused when the entity is linked to projects or governing bodies.
 *
 * Society master sheet (26-Nov-2026) adds: Society/Trust type, Trust Doc Number, Branch & Users In-charge,
 * PAN status/name/date, DARPAN, several TANs (number/date/name), and status + validity period + certificate
 * name for 12A, 80G and FCRA (stored in MASTER_INSTI_STATUTORY / MASTER_INSTI_TAN). Required: Society/Trust
 * Name, Society/Trust, Society Reg Number, Trust Doc Number, Registration Date, Branch and Users In-charge,
 * PAN Status, and PAN Number + PAN Name when PAN is obtained.
 * Codes sent to the API: entityType 1 Society / 2 Trust; *Status 1 Obtained|Registered / 0 Not.
 */

const RELIGIOUS = 3;
const ASSOCIATION_OTHERS = 5;
const DENOMINATION_OTHERS = 6;
const DEFAULT_COUNTRY = 'India';

// Select values are strings; the API takes numbers (see codes above).
const SOCIETY = '1';
const TRUST = '2';
const OBTAINED = '1';
const REGISTERED = '1';
const NOT_YET = '0';
const entityTypeOptions = [{ value: SOCIETY, label: 'Society' }, { value: TRUST, label: 'Trust' }];
const panStatusOptions = [{ value: OBTAINED, label: 'Obtained' }, { value: NOT_YET, label: 'Not Obtained' }];
const regStatusOptions = [{ value: REGISTERED, label: 'Registered' }, { value: NOT_YET, label: 'Not Registered' }];

// Income-tax formats: PAN = 5 letters, 4 digits, 1 letter; TAN = 4 letters, 5 digits, 1 letter.
const PAN_PATTERN = /^[A-Z]{5}[0-9]{4}[A-Z]$/;
const TAN_PATTERN = /^[A-Z]{4}[0-9]{5}[A-Z]$/;

// Fallback when the lookups call fails (same values as the legacy radio / checkbox lists).
const defaultNatures = [
    { id: 0, name: 'Cultural' }, { id: 1, name: 'Economic' }, { id: 2, name: 'Educational' },
    { id: 3, name: 'Religious' }, { id: 4, name: 'Social' }, { id: 5, name: 'Others' },
];
const defaultDenominations = [
    { id: 1, name: 'Hindu' }, { id: 2, name: 'Sikh' }, { id: 3, name: 'Muslim' },
    { id: 4, name: 'Christian' }, { id: 5, name: 'Buddhist' }, { id: 6, name: 'Others' },
];

// Column limits (master_insti_perference); PAN / TAN follow the legacy 10-character boxes.
const MAX = {
    societyName: 100, contactPerson: 100, address: 500, place: 100, state: 100, pincode: 20, phone: 20, fax: 20,
    email: 100, url: 100, regNo: 100, permissionNo: 50, a12No: 100, panNo: 10, girNo: 50, fcriNo: 30,
    eightyGNo: 20, otherAssociationNature: 100, otherDenomination: 100, principalActivity: 100,
    trustDocNo: 100, branchIncharge: 100, panName: 100, darpanId: 30, darpanName: 100, a12Name: 100, eightyGName: 100,
};

const CAPTION = {
    societyName: 'Society/Trust Name', contactPerson: 'Contact Person', address: 'Address', place: 'Place',
    state: 'State/Province', pincode: 'Pincode', phone: 'Phone', fax: 'Fax', email: 'Email', url: 'URL',
    regNo: 'Society Reg Number', permissionNo: 'Prior Permission No', a12No: '12A No (URN)', panNo: 'PAN Number',
    girNo: 'GIR No', fcriNo: 'FCRA No', eightyGNo: '80G No', otherAssociationNature: 'Other association nature',
    otherDenomination: 'Other denomination', principalActivity: 'Principal Activity', trustDocNo: 'Trust Doc Number',
    branchIncharge: 'Branch and Users In-charge', panName: 'PAN Name', darpanId: 'DARPAN ID', darpanName: 'DARPAN Name',
    a12Name: '12A Name', eightyGName: '80G Name',
};

const emptyTan = { tanNo: '', tanRegDate: '', tanName: '' };

const emptyEntity = {
    customerId: 0, societyName: '', entityType: '', regNo: '', trustDocNo: '', regDate: '', branchIncharge: '',
    permissionNo: '', permissionDate: '', principalActivity: '', isFoundation: false,
    natures: [], otherAssociationNature: '', denomination: '', otherDenomination: '',
    contactPerson: '', address: '', place: '', state: '', country: DEFAULT_COUNTRY,
    pincode: '', phone: '', fax: '', email: '', url: '',
    panStatus: OBTAINED, panNo: '', panName: '', panRegDate: '', girNo: '',
    darpanStatus: '', darpanId: '', darpanRegDate: '', darpanName: '',
    tans: [{ ...emptyTan }],
    a12Status: '', a12No: '', a12ValidFrom: '', a12ValidTo: '', a12Name: '',
    eightyGStatus: '', eightyGNo: '', eightyGNoRegDate: '', eightyGValidFrom: '', eightyGValidTo: '', eightyGName: '',
    fcraStatus: '', fcriNo: '', fcriRegDate: '', fcraValidFrom: '', fcraValidTo: '',
};

// Add / Edit is a 5-step form; each step lists the fields it validates before moving on.
const STEPS = ['Society Info', 'Association', 'Address & Contact', 'PAN, DARPAN & TAN', '12A, 80G & FCRA'];
const STEP_FIELDS = [
    ['societyName', 'entityType', 'regNo', 'trustDocNo', 'regDate', 'branchIncharge', 'permissionNo', 'permissionDate'],
    ['principalActivity', 'natures', 'otherAssociationNature', 'denomination', 'otherDenomination'],
    ['contactPerson', 'address', 'place', 'phone', 'country', 'state', 'pincode', 'fax', 'email', 'url'],
    ['panStatus', 'panNo', 'panName', 'panRegDate', 'girNo', 'darpanStatus', 'darpanId', 'darpanRegDate', 'darpanName', 'tans'],
    ['a12Status', 'a12No', 'a12ValidFrom', 'a12ValidTo', 'a12Name', 'eightyGStatus', 'eightyGNo', 'eightyGNoRegDate',
        'eightyGValidFrom', 'eightyGValidTo', 'eightyGName', 'fcraStatus', 'fcriNo', 'fcriRegDate', 'fcraValidFrom', 'fcraValidTo'],
];

// TAN row errors use keys like "tans.0.tanNo".
function stepOf(field) {
    const key = field.split('.')[0];
    const index = STEP_FIELDS.findIndex((fields) => fields.includes(key));
    return index < 0 ? STEP_FIELDS.length - 1 : index;
}

function text(value) {
    return value === null || value === undefined ? '' : String(value);
}

function parseNatures(value) {
    return text(value).split(',').map((item) => item.trim()).filter((item) => item !== '')
        .map(Number).filter((item) => Number.isInteger(item));
}

// Stored status code as a select value; old records have none yet, so a stored number means obtained / registered.
function statusOf(stored, number, yes) {
    if (stored === 0 || stored === 1) {
        return String(stored);
    }
    return text(number).trim() ? yes : '';
}

// '' (not chosen) -> null, '1' -> 1, '0' -> 0.
function code(value) {
    return value === '' || value === null || value === undefined ? null : Number(value);
}

function TextArea({ id, label, value, onChange, error, rows = 2 }) {
    return (<div className="field">
      <label htmlFor={id}>{label}</label>
      <textarea id={id} className={error ? 'control lg-textarea is-invalid' : 'control lg-textarea'} rows={rows} value={value} onChange={(event) => onChange(event.target.value)}/>
      {error ? <p className="field-error">{error}</p> : null}
    </div>);
}

function Check({ id, label, checked, onChange }) {
    return (<label className="lg-check" htmlFor={id}>
      <input id={id} type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)}/>
      {label}
    </label>);
}

const columns = [
    { key: 'societyName', header: 'Society/Trust Name', wrap: true },
    { key: 'regNo', header: 'Reg No', wrap: true },
    { key: 'regDateText', header: 'Reg Date', width: '95px' },
];

export function LegalEntityPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [natureOptions, setNatureOptions] = useState(defaultNatures);
    const [denominationOptions, setDenominationOptions] = useState(defaultDenominations);
    const [countries, setCountries] = useState([]);
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
            // Lookups failing should not hide the list; the form falls back to the legacy option lists.
            const [entities, lookups, countryRows] = await Promise.all([
                legalEntityApi.fetchLegalEntities(session),
                legalEntityApi.fetchLookups(session).catch(() => null),
                countryApi.fetchCountries(session).catch(() => []),
            ]);
            setRows(entities.map((row) => ({ ...row, regDateText: toGridDate(row.regDate) })));
            if (lookups && Array.isArray(lookups.associationNatures) && lookups.associationNatures.length > 0) {
                setNatureOptions(lookups.associationNatures);
            }
            if (lookups && Array.isArray(lookups.denominations) && lookups.denominations.length > 0) {
                setDenominationOptions(lookups.denominations);
            }
            setCountries(countryRows);
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    // Country is stored as its name (legacy saved ddlCountry.SelectedItem.Text).
    const countryOptions = useMemo(() => {
        const options = countries.map((item) => ({ value: item.country, label: item.country }));
        const current = form ? form.country : '';
        if (current && !options.some((option) => option.value === current)) {
            options.unshift({ value: current, label: current });
        }
        return options;
    }, [countries, form]);

    function set(field) {
        return (value) => {
            setForm((current) => ({ ...current, [field]: value }));
            setErrors((current) => ({ ...current, [field]: undefined }));
        };
    }

    const upper = (field) => (value) => set(field)(value.toUpperCase());

    function setTan(index, field, value) {
        setForm((current) => ({ ...current, tans: current.tans.map((row, at) => (at === index ? { ...row, [field]: value } : row)) }));
        setErrors((current) => ({ ...current, [`tans.${index}.${field}`]: undefined }));
    }

    function addTan() {
        setForm((current) => ({ ...current, tans: [...current.tans, { ...emptyTan }] }));
    }

    function removeTan(index) {
        setForm((current) => {
            const tans = current.tans.filter((_, at) => at !== index);
            return { ...current, tans: tans.length > 0 ? tans : [{ ...emptyTan }] };
        });
        setErrors((current) => Object.fromEntries(Object.entries(current).filter(([key]) => !key.startsWith('tans.'))));
    }

    function toggleNature(id, checked) {
        setForm((current) => {
            const natures = checked ? [...current.natures.filter((item) => item !== id), id] : current.natures.filter((item) => item !== id);
            const next = { ...current, natures };
            if (!natures.includes(RELIGIOUS)) {
                next.denomination = '';
                next.otherDenomination = '';
            }
            if (!natures.includes(ASSOCIATION_OTHERS)) {
                next.otherAssociationNature = '';
            }
            return next;
        });
        setErrors((current) => ({ ...current, natures: undefined, denomination: undefined, otherAssociationNature: undefined }));
    }

    function chooseDenomination(id) {
        setForm((current) => ({ ...current, denomination: String(id), otherDenomination: id === DENOMINATION_OTHERS ? current.otherDenomination : '' }));
        setErrors((current) => ({ ...current, denomination: undefined, otherDenomination: undefined }));
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setStep(0);
        setForm({ ...emptyEntity, tans: [{ ...emptyTan }] });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setStep(0);
        setBusy(true);
        try {
            const detail = await legalEntityApi.fetchLegalEntity(session, row.customerId);
            const natures = parseNatures(detail.associationNature);
            const storedTans = Array.isArray(detail.tans) ? detail.tans : [];
            const tans = storedTans.length > 0
                ? storedTans.map((tan) => ({ tanNo: text(tan.tanNo), tanRegDate: toInputDate(tan.tanRegDate), tanName: text(tan.tanName) }))
                : [{ tanNo: text(detail.tanNo), tanRegDate: toInputDate(detail.tanRegDate), tanName: text(detail.tanName) }];
            setForm({
                customerId: detail.customerId,
                societyName: text(detail.societyName),
                // Records saved before the Society/Trust choice existed are societies.
                entityType: detail.entityType === 1 || detail.entityType === 2 ? String(detail.entityType) : SOCIETY,
                regNo: text(detail.regNo),
                trustDocNo: text(detail.trustDocNo),
                regDate: toInputDate(detail.regDate),
                branchIncharge: text(detail.branchIncharge),
                permissionNo: text(detail.permissionNo),
                permissionDate: toInputDate(detail.permissionDate),
                principalActivity: text(detail.principalActivity),
                isFoundation: Boolean(detail.isFoundation),
                natures,
                otherAssociationNature: natures.includes(ASSOCIATION_OTHERS) ? text(detail.otherAssociationNature) : '',
                denomination: natures.includes(RELIGIOUS) && Number(detail.denomination) > 0 ? String(detail.denomination) : '',
                otherDenomination: Number(detail.denomination) === DENOMINATION_OTHERS ? text(detail.otherDenomination) : '',
                contactPerson: text(detail.contactPerson),
                address: text(detail.address),
                place: text(detail.place),
                state: text(detail.state),
                country: text(detail.country),
                pincode: text(detail.pincode),
                phone: text(detail.phone),
                fax: text(detail.fax),
                email: text(detail.email),
                url: text(detail.url),
                panStatus: statusOf(detail.panStatus, detail.panNo, OBTAINED),
                panNo: text(detail.panNo),
                panName: text(detail.panName),
                panRegDate: toInputDate(detail.panRegDate),
                girNo: text(detail.girNo),
                darpanStatus: statusOf(detail.darpanStatus, detail.darpanId, REGISTERED),
                darpanId: text(detail.darpanId),
                darpanRegDate: toInputDate(detail.darpanRegDate),
                darpanName: text(detail.darpanName),
                tans,
                a12Status: statusOf(detail.a12Status, detail.a12No, REGISTERED),
                a12No: text(detail.a12No),
                a12ValidFrom: toInputDate(detail.a12ValidFrom),
                a12ValidTo: toInputDate(detail.a12ValidTo),
                a12Name: text(detail.a12Name),
                eightyGStatus: statusOf(detail.eightyGStatus, detail.eightyGNo, REGISTERED),
                eightyGNo: text(detail.eightyGNo),
                eightyGNoRegDate: toInputDate(detail.eightyGNoRegDate),
                eightyGValidFrom: toInputDate(detail.eightyGValidFrom),
                eightyGValidTo: toInputDate(detail.eightyGValidTo),
                eightyGName: text(detail.eightyGName),
                fcraStatus: statusOf(detail.fcraStatus, detail.fcriNo, REGISTERED),
                fcriNo: text(detail.fcriNo),
                fcriRegDate: toInputDate(detail.fcriRegDate),
                fcraValidFrom: toInputDate(detail.fcraValidFrom),
                fcraValidTo: toInputDate(detail.fcraValidTo),
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
        const now = today();
        const notFuture = (field, caption) => {
            if (form[field] && form[field] > now) next[field] = `${caption} cannot be a future date.`;
        };
        // Validity period: both dates together, and To must be after From.
        const period = (from, to, caption) => {
            if (form[from] && !form[to]) next[to] = `Enter the ${caption} validity To date.`;
            else if (!form[from] && form[to]) next[from] = `Enter the ${caption} validity From date.`;
            else if (form[from] && form[to] && form[to] < form[from]) next[to] = `${caption} validity To date must be after the From date.`;
        };

        // Society info
        const societyName = form.societyName.trim();
        if (!societyName) next.societyName = 'Society/Trust Name is required.';
        else if (rows.some((row) => row.customerId !== form.customerId && (row.societyName || '').trim().toLowerCase() === societyName.toLowerCase())) {
            next.societyName = 'A legal entity with this name already exists.';
        }
        if (!form.entityType) next.entityType = 'Select Society or Trust.';
        if (!form.regNo.trim()) next.regNo = 'Society Reg Number is required.';
        if (!form.trustDocNo.trim()) next.trustDocNo = 'Trust Doc Number is required.';
        if (!form.regDate) next.regDate = 'Registration Date is required.';
        if (!form.branchIncharge.trim()) next.branchIncharge = 'Branch and Users In-charge is required.';
        notFuture('regDate', 'Registration Date');
        notFuture('permissionDate', 'Prior Permission Date');

        // Association
        if (form.natures.length === 0) next.natures = 'Select the association nature.';
        else if (form.natures.includes(ASSOCIATION_OTHERS) && !form.otherAssociationNature.trim()) next.otherAssociationNature = 'Enter the other association nature.';
        if (form.natures.includes(RELIGIOUS) && !form.denomination) next.denomination = 'Select the denomination.';
        else if (Number(form.denomination) === DENOMINATION_OTHERS && !form.otherDenomination.trim()) next.otherDenomination = 'Enter the other denomination.';

        // Address & contact
        if (!form.country) next.country = 'Select the country.';
        if (!form.state.trim()) next.state = 'State/Province is required.';
        if (form.email.trim() && !EMAIL_PATTERN.test(form.email.trim())) next.email = 'Enter a valid email address.';

        // PAN
        if (!form.panStatus) next.panStatus = 'Select the PAN status.';
        if (form.panStatus === OBTAINED) {
            if (!form.panNo.trim()) next.panNo = 'PAN Number is required.';
            else if (!PAN_PATTERN.test(form.panNo.trim())) next.panNo = 'Enter a valid PAN (for example ABCDE1234F).';
            if (!form.panName.trim()) next.panName = 'PAN Name is required.';
            notFuture('panRegDate', 'PAN Reg Date');
        }
        // DARPAN
        if (form.darpanStatus === REGISTERED) {
            if (!form.darpanId.trim()) next.darpanId = 'DARPAN ID is required when registered.';
            notFuture('darpanRegDate', 'DARPAN Registration Date');
        }
        // TAN rows (blank rows are ignored)
        const seen = new Set();
        form.tans.forEach((tan, index) => {
            const number = tan.tanNo.trim();
            if (!number && !tan.tanRegDate && !tan.tanName.trim()) return;
            const key = `tans.${index}`;
            if (!number) next[`${key}.tanNo`] = 'Enter the TAN number.';
            else if (!TAN_PATTERN.test(number)) next[`${key}.tanNo`] = 'Enter a valid TAN (for example ABCD12345E).';
            else if (seen.has(number)) next[`${key}.tanNo`] = 'This TAN is already listed.';
            seen.add(number);
            if (tan.tanRegDate && tan.tanRegDate > now) next[`${key}.tanRegDate`] = 'TAN date cannot be a future date.';
            if (tan.tanName.trim().length > 100) next[`${key}.tanName`] = 'TAN Name cannot exceed 100 characters.';
        });
        // 12A
        if (form.a12Status === REGISTERED) {
            if (!form.a12No.trim()) next.a12No = '12A No (URN) is required when registered.';
            period('a12ValidFrom', 'a12ValidTo', '12A');
        }
        // 80G
        if (form.eightyGStatus === REGISTERED) {
            if (!form.eightyGNo.trim()) next.eightyGNo = '80G No is required when registered.';
            notFuture('eightyGNoRegDate', '80G Reg Date');
            period('eightyGValidFrom', 'eightyGValidTo', '80G');
        }
        // FCRA
        if (form.fcraStatus === REGISTERED) {
            if (!form.fcriNo.trim()) next.fcriNo = 'FCRA No is required when registered.';
            notFuture('fcriRegDate', 'FCRA Reg Date');
            period('fcraValidFrom', 'fcraValidTo', 'FCRA');
        }

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
            const religious = form.natures.includes(RELIGIOUS);
            const pan = form.panStatus === OBTAINED;
            const darpan = form.darpanStatus === REGISTERED;
            const a12 = form.a12Status === REGISTERED;
            const g80 = form.eightyGStatus === REGISTERED;
            const fcra = form.fcraStatus === REGISTERED;
            const tans = form.tans
                .filter((tan) => tan.tanNo.trim())
                .map((tan) => ({ tanNo: tan.tanNo.trim().toUpperCase(), tanRegDate: requestDate(tan.tanRegDate), tanName: tan.tanName.trim() }));
            const keep = (on, value) => (on ? value.trim() : '');
            const keepDate = (on, value) => (on ? requestDate(value) : requestDate(''));
            const request = {
                customerId: form.customerId,
                societyName: form.societyName.trim(),
                entityType: code(form.entityType),
                regNo: form.regNo.trim(),
                trustDocNo: form.trustDocNo.trim(),
                regDate: requestDate(form.regDate),
                branchIncharge: form.branchIncharge.trim(),
                permissionNo: form.permissionNo.trim(),
                permissionDate: requestDate(form.permissionDate),
                principalActivity: form.principalActivity.trim(),
                isFoundation: form.isFoundation,
                associationNature: [...form.natures].sort((a, b) => a - b).join(','),
                otherAssociationNature: form.natures.includes(ASSOCIATION_OTHERS) ? form.otherAssociationNature.trim() : '',
                denomination: religious && form.denomination ? Number(form.denomination) : 0,
                otherDenomination: religious && Number(form.denomination) === DENOMINATION_OTHERS ? form.otherDenomination.trim() : '',
                contactPerson: form.contactPerson.trim(),
                address: form.address.trim(),
                place: form.place.trim(),
                state: form.state.trim(),
                country: form.country,
                pincode: form.pincode.trim(),
                phone: form.phone.trim(),
                fax: form.fax.trim(),
                email: form.email.trim(),
                url: form.url.trim(),
                panStatus: code(form.panStatus),
                panNo: keep(pan, form.panNo).toUpperCase(),
                panName: keep(pan, form.panName),
                panRegDate: keepDate(pan, form.panRegDate),
                girNo: form.girNo.trim(),
                darpanStatus: code(form.darpanStatus),
                darpanId: keep(darpan, form.darpanId),
                darpanRegDate: keepDate(darpan, form.darpanRegDate),
                darpanName: keep(darpan, form.darpanName),
                // Existing single TAN column keeps the first TAN; the full list goes in "tans".
                tanNo: tans.length > 0 ? tans[0].tanNo : '',
                tans,
                a12Status: code(form.a12Status),
                a12No: keep(a12, form.a12No),
                a12ValidFrom: keepDate(a12, form.a12ValidFrom),
                a12ValidTo: keepDate(a12, form.a12ValidTo),
                a12Name: keep(a12, form.a12Name),
                eightyGStatus: code(form.eightyGStatus),
                eightyGNo: keep(g80, form.eightyGNo),
                eightyGNoRegDate: keepDate(g80, form.eightyGNoRegDate),
                eightyGValidFrom: keepDate(g80, form.eightyGValidFrom),
                eightyGValidTo: keepDate(g80, form.eightyGValidTo),
                eightyGName: keep(g80, form.eightyGName),
                fcraStatus: code(form.fcraStatus),
                fcriNo: keep(fcra, form.fcriNo),
                fcriRegDate: keepDate(fcra, form.fcriRegDate),
                fcraValidFrom: keepDate(fcra, form.fcraValidFrom),
                fcraValidTo: keepDate(fcra, form.fcraValidTo),
            };
            const result = await legalEntityApi.saveLegalEntity(session, request);
            pushToast({ message: (result && result.message) || 'Legal entity saved.' });
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
        if (!window.confirm(`Delete legal entity ${row.societyName}?`)) {
            return;
        }
        try {
            const result = await legalEntityApi.deleteLegalEntity(session, row.customerId);
            pushToast({ message: (result && result.message) || 'Legal entity deleted.' });
            setRows((current) => current.filter((item) => item.customerId !== row.customerId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.customerId === 0;
    const religious = form ? form.natures.includes(RELIGIOUS) : false;
    const associationOthers = form ? form.natures.includes(ASSOCIATION_OTHERS) : false;

    return (<section className="lg-page" aria-label="Legal Entity">
      <PageBar title="Legal Entity" addLabel="Add Legal Entity" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.customerId} loading={loading} error={loadError} search={search} searchKeys={['societyName', 'regNo', 'regDateText']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <StepFormModal title={isNew ? 'Add Legal Entity' : 'Edit Legal Entity'} isOpen={Boolean(form)} busy={busy} error={formError} steps={STEPS} step={step} onStep={(index) => { setErrors({}); setStep(index); }} onNext={checkStep} onClose={() => setForm(null)} onSave={() => void save()}>
        {form && step === 0 ? <FieldGrid>
          <Input id="le-society" label="Society/Trust Name" required placeholder="As per registration document" value={form.societyName} error={errors.societyName} onChange={set('societyName')}/>
          <Select id="le-type" label="Society/Trust" required placeholder="Select Type" value={form.entityType} options={entityTypeOptions} error={errors.entityType} onChange={set('entityType')}/>
          <Input id="le-regno" label="Society Reg Number" required value={form.regNo} error={errors.regNo} onChange={set('regNo')}/>
          <Input id="le-trustdoc" label="Trust Doc Number" required value={form.trustDocNo} error={errors.trustDocNo} onChange={set('trustDocNo')}/>
          <Input id="le-regdate" label="Registration Date" required type="date" value={form.regDate} error={errors.regDate} onChange={set('regDate')}/>
          <Input id="le-incharge" label="Branch and Users In-charge" required value={form.branchIncharge} error={errors.branchIncharge} onChange={set('branchIncharge')}/>
          <Input id="le-permno" label="Prior Permission No" value={form.permissionNo} error={errors.permissionNo} onChange={set('permissionNo')}/>
          <Input id="le-permdate" label="Prior Permission Date" type="date" value={form.permissionDate} error={errors.permissionDate} onChange={set('permissionDate')}/>
        </FieldGrid> : null}

        {form && step === 1 ? <FieldGrid>
          <Input id="le-activity" label="Principal Activity" value={form.principalActivity} error={errors.principalActivity} onChange={set('principalActivity')}/>
          <div className="field">
            <span>&nbsp;</span>
            <Check id="le-foundation" label="Is Foundation?" checked={form.isFoundation} onChange={set('isFoundation')}/>
          </div>

          <fieldset className="lg-rights-module" style={{ gridColumn: '1 / -1' }}>
            <legend><strong>Association Nature<span className="req" aria-hidden="true">*</span></strong></legend>
            <div className="lg-rights-list">
              {natureOptions.map((option) => (<label key={option.id} className="lg-check">
                <input type="checkbox" checked={form.natures.includes(option.id)} onChange={(event) => toggleNature(option.id, event.target.checked)}/>
                {option.name}
              </label>))}
            </div>
            {errors.natures ? <p className="field-error">{errors.natures}</p> : null}
          </fieldset>
          {associationOthers ? <Input id="le-assoc-other" label="Other Association Nature" required value={form.otherAssociationNature} error={errors.otherAssociationNature} onChange={set('otherAssociationNature')}/> : null}

          {religious ? <fieldset className="lg-rights-module" style={{ gridColumn: '1 / -1' }}>
            <legend><strong>Denomination<span className="req" aria-hidden="true">*</span></strong></legend>
            <div className="lg-rights-list" role="radiogroup" aria-label="Denomination">
              {denominationOptions.map((option) => (<label key={option.id} className="lg-check">
                <input type="radio" name="le-denomination" checked={form.denomination === String(option.id)} onChange={() => chooseDenomination(option.id)}/>
                {option.name}
              </label>))}
            </div>
            {errors.denomination ? <p className="field-error">{errors.denomination}</p> : null}
          </fieldset> : null}
          {religious && Number(form.denomination) === DENOMINATION_OTHERS ? <Input id="le-denom-other" label="Other Denomination" required value={form.otherDenomination} error={errors.otherDenomination} onChange={set('otherDenomination')}/> : null}
        </FieldGrid> : null}

        {form && step === 2 ? <FieldGrid>
          <Input id="le-contact" label="Contact Person" value={form.contactPerson} error={errors.contactPerson} onChange={set('contactPerson')}/>
          <Input id="le-phone" label="Phone" value={form.phone} error={errors.phone} onChange={set('phone')}/>
          <TextArea id="le-address" label="Address" value={form.address} error={errors.address} onChange={set('address')}/>
          <Input id="le-place" label="Place" value={form.place} error={errors.place} onChange={set('place')}/>
          <SearchableSelect id="le-country" label="Country" required value={form.country} options={countryOptions} placeholder="Select country" error={errors.country} onChange={set('country')}/>
          <Input id="le-state" label="State/Province" required value={form.state} error={errors.state} onChange={set('state')}/>
          <Input id="le-pincode" label="Pincode" value={form.pincode} error={errors.pincode} onChange={set('pincode')}/>
          <Input id="le-fax" label="Fax" value={form.fax} error={errors.fax} onChange={set('fax')}/>
          <Input id="le-email" label="Email" type="email" value={form.email} error={errors.email} onChange={set('email')}/>
          <Input id="le-url" label="URL" value={form.url} placeholder="http://" error={errors.url} onChange={set('url')}/>
        </FieldGrid> : null}

        {form && step === 3 ? <FieldGrid>
          <FieldSection title="PAN"/>
          <Select id="le-panstatus" label="PAN Status" required placeholder="Select Status" value={form.panStatus} options={panStatusOptions} error={errors.panStatus} onChange={set('panStatus')}/>
          <Input id="le-gir" label="GIR No" value={form.girNo} error={errors.girNo} onChange={set('girNo')}/>
          {form.panStatus === OBTAINED ? (<>
            <Input id="le-pan" label="PAN Number" required placeholder="ABCDE1234F" value={form.panNo} error={errors.panNo} onChange={upper('panNo')}/>
            <Input id="le-panname" label="PAN Name" required value={form.panName} error={errors.panName} onChange={set('panName')}/>
            <Input id="le-pandate" label="PAN Reg Date" type="date" value={form.panRegDate} error={errors.panRegDate} onChange={set('panRegDate')}/>
          </>) : null}

          <FieldSection title="DARPAN"/>
          <Select id="le-darpanstatus" label="DARPAN Status" placeholder="Select Status" value={form.darpanStatus} options={regStatusOptions} error={errors.darpanStatus} onChange={set('darpanStatus')}/>
          {form.darpanStatus === REGISTERED ? (<>
            <Input id="le-darpanid" label="DARPAN ID" required value={form.darpanId} error={errors.darpanId} onChange={set('darpanId')}/>
            <Input id="le-darpandate" label="DARPAN Registration Date" type="date" value={form.darpanRegDate} error={errors.darpanRegDate} onChange={set('darpanRegDate')}/>
            <Input id="le-darpanname" label="DARPAN Name" placeholder="As per certificate" value={form.darpanName} error={errors.darpanName} onChange={set('darpanName')}/>
          </>) : <div/>}

          <FieldSection title="TAN"/>
          <div className="lg-tan-list" style={{ gridColumn: '1 / -1' }}>
            <div className="lg-tan-row is-head" aria-hidden="true">
              <span>TAN Register Number</span><span>TAN Register Date</span><span>TAN Name (as per certificate)</span><span/>
            </div>
            {form.tans.map((tan, index) => (<div key={index} className="lg-tan-row">
              <div>
                <input className={errors[`tans.${index}.tanNo`] ? 'control is-invalid' : 'control'} aria-label={`TAN ${index + 1} number`} placeholder="ABCD12345E" value={tan.tanNo} onChange={(event) => setTan(index, 'tanNo', event.target.value.toUpperCase())}/>
                {errors[`tans.${index}.tanNo`] ? <p className="field-error">{errors[`tans.${index}.tanNo`]}</p> : null}
              </div>
              <div>
                <input type="date" className={errors[`tans.${index}.tanRegDate`] ? 'control is-invalid' : 'control'} aria-label={`TAN ${index + 1} register date`} value={tan.tanRegDate} onChange={(event) => setTan(index, 'tanRegDate', event.target.value)}/>
                {errors[`tans.${index}.tanRegDate`] ? <p className="field-error">{errors[`tans.${index}.tanRegDate`]}</p> : null}
              </div>
              <div>
                <input className={errors[`tans.${index}.tanName`] ? 'control is-invalid' : 'control'} aria-label={`TAN ${index + 1} name`} value={tan.tanName} onChange={(event) => setTan(index, 'tanName', event.target.value)}/>
                {errors[`tans.${index}.tanName`] ? <p className="field-error">{errors[`tans.${index}.tanName`]}</p> : null}
              </div>
              <button type="button" className="lg-tan-remove" aria-label={`Remove TAN ${index + 1}`} title="Remove" onClick={() => removeTan(index)}>&times;</button>
            </div>))}
            <button type="button" className="lg-btn lg-tan-add" onClick={addTan}>+ Add TAN</button>
          </div>
        </FieldGrid> : null}

        {form && step === 4 ? <FieldGrid>
          <FieldSection title="12A"/>
          <Select id="le-12astatus" label="12A Status" placeholder="Select Status" value={form.a12Status} options={regStatusOptions} error={errors.a12Status} onChange={set('a12Status')}/>
          {form.a12Status === REGISTERED ? (<>
            <Input id="le-a12" label="12A No (URN)" required value={form.a12No} error={errors.a12No} onChange={set('a12No')}/>
            <Input id="le-12afrom" label="12A Validity From" type="date" value={form.a12ValidFrom} error={errors.a12ValidFrom} onChange={set('a12ValidFrom')}/>
            <Input id="le-12ato" label="12A Validity To" type="date" value={form.a12ValidTo} error={errors.a12ValidTo} onChange={set('a12ValidTo')}/>
            <Input id="le-12aname" label="12A Name" placeholder="As per certificate" value={form.a12Name} error={errors.a12Name} onChange={set('a12Name')}/>
          </>) : <div/>}

          <FieldSection title="80G"/>
          <Select id="le-80gstatus" label="80G Status" placeholder="Select Status" value={form.eightyGStatus} options={regStatusOptions} error={errors.eightyGStatus} onChange={set('eightyGStatus')}/>
          {form.eightyGStatus === REGISTERED ? (<>
            <Input id="le-80g" label="80G No" required value={form.eightyGNo} error={errors.eightyGNo} onChange={set('eightyGNo')}/>
            <Input id="le-80gdate" label="80G Reg Date" type="date" value={form.eightyGNoRegDate} error={errors.eightyGNoRegDate} onChange={set('eightyGNoRegDate')}/>
            <Input id="le-80gname" label="80G Name" placeholder="As per certificate" value={form.eightyGName} error={errors.eightyGName} onChange={set('eightyGName')}/>
            <Input id="le-80gfrom" label="80G Validity From" type="date" value={form.eightyGValidFrom} error={errors.eightyGValidFrom} onChange={set('eightyGValidFrom')}/>
            <Input id="le-80gto" label="80G Validity To" type="date" value={form.eightyGValidTo} error={errors.eightyGValidTo} onChange={set('eightyGValidTo')}/>
          </>) : <div/>}

          <FieldSection title="FCRA"/>
          <Select id="le-fcrastatus" label="FCRA Status" placeholder="Select Status" value={form.fcraStatus} options={regStatusOptions} error={errors.fcraStatus} onChange={set('fcraStatus')}/>
          {form.fcraStatus === REGISTERED ? (<>
            <Input id="le-fcra" label="FCRA No" required value={form.fcriNo} error={errors.fcriNo} onChange={set('fcriNo')}/>
            <Input id="le-fcradate" label="FCRA Reg Date" type="date" value={form.fcriRegDate} error={errors.fcriRegDate} onChange={set('fcriRegDate')}/>
            <Input id="le-fcrafrom" label="FCRA Validity From" type="date" value={form.fcraValidFrom} error={errors.fcraValidFrom} onChange={set('fcraValidFrom')}/>
            <Input id="le-fcrato" label="FCRA Validity To" type="date" value={form.fcraValidTo} error={errors.fcraValidTo} onChange={set('fcraValidTo')}/>
          </>) : <div/>}
        </FieldGrid> : null}
      </StepFormModal>
    </section>);
}
