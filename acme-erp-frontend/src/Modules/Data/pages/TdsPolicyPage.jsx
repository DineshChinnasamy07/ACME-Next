import { useCallback, useEffect, useMemo, useState } from 'react';
import { Input, SearchableSelect, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FieldSection, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { gridDate, isoDay, requestDate } from '../services/periodLockApi';
import { tdsPolicyApi } from '../services/tdsPolicyApi';

/*
 * Data › Statutory Masters › TDS Policy (Head Office login).
 * Mirrors legacy TDSPolicyView.aspx (deductee type filter, grid of nature of payment + applicable from with a
 * Rate(%) / Exemption Limit band per duty / tax type) on api/tdspolicies, plus add / edit / delete.
 * One policy = deductee type + nature of payment + applicable from date, with one tax rate row per duty / tax type.
 * Server rules: deductee type, nature of payment and applicable from required; one policy per deductee type +
 * nature of payment + applicable date; rate 0–100 %, exemption limit 0–99,99,99,999.99 (2 decimals);
 * at least one tax type with a rate; rows with rate and exemption both 0 are not stored.
 */

const MAX_RATE = 100;
const MAX_EXEMPTION = 99999999.99;

const amountFormat = new Intl.NumberFormat('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

function money(value) {
    const number = Number(value);
    return amountFormat.format(Number.isFinite(number) ? number : 0);
}

// '' → 0; otherwise a number with at most 2 decimals (NaN when not).
function parseDecimal(text) {
    const value = String(text ?? '').trim();
    if (value === '') {
        return 0;
    }
    if (!/^\d+(\.\d{1,2})?$/.test(value)) {
        return Number.NaN;
    }
    return Number(value);
}

function decimalText(value) {
    const number = Number(value);
    return Number.isFinite(number) && number !== 0 ? number.toFixed(2) : '';
}

const emptyPolicy = { tdsPolicyId: 0, deducteeTypeId: '', naturePayId: '', applicableFrom: '', rates: [] };

export function TdsPolicyPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [lookups, setLookups] = useState({ deducteeTypes: [], natureOfPayments: [], taxTypes: [] });
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [deducteeFilter, setDeducteeFilter] = useState('');
    const [form, setForm] = useState(null);
    const [errors, setErrors] = useState({});
    const [formError, setFormError] = useState('');
    const [busy, setBusy] = useState(false);

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            const [policies, options] = await Promise.all([tdsPolicyApi.fetchPolicies(session), tdsPolicyApi.fetchLookups(session)]);
            setLookups(options);
            setRows(policies.map((row) => ({
                ...row,
                applicableFromText: gridDate(row.applicableFrom),
                sectionText: row.sectionCode || row.sectionName || '',
                ratesText: (row.taxRates || []).map((rate) => `${rate.taxTypeName} ${money(rate.tdsRate)}%`).join(' '),
            })));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    // Grid bands: active tax types first, then any other tax type stored on a policy.
    const taxTypeColumns = useMemo(() => {
        const list = lookups.taxTypes.map((type) => ({ id: type.id, name: type.name }));
        const known = new Set(list.map((type) => type.id));
        rows.forEach((row) => (row.taxRates || []).forEach((rate) => {
            if (!known.has(rate.tdsTaxTypeId)) {
                known.add(rate.tdsTaxTypeId);
                list.push({ id: rate.tdsTaxTypeId, name: rate.taxTypeName || `Tax Type ${rate.tdsTaxTypeId}` });
            }
        }));
        return list;
    }, [lookups.taxTypes, rows]);

    const columns = useMemo(() => [
        { key: 'deducteeType', header: 'Deductee Type', wrap: true },
        { key: 'natureOfPayment', header: 'Nature of Payments', wrap: true },
        { key: 'sectionText', header: 'Section', width: '80px' },
        { key: 'applicableFromText', header: 'Applicable From', width: '110px' },
        ...taxTypeColumns.map((type) => ({
            key: `tax-${type.id}`,
            header: `${type.name} — Rate(%) / Exemption Limit`,
            wrap: true,
            render: (row) => {
                const rate = (row.taxRates || []).find((item) => item.tdsTaxTypeId === type.id);
                return rate ? `${money(rate.tdsRate)} / ${money(rate.tdsExemptionLimit)}` : '0.00 / 0.00';
            },
        })),
    ], [taxTypeColumns]);

    const visibleRows = deducteeFilter ? rows.filter((row) => String(row.deducteeTypeId) === deducteeFilter) : rows;
    const filterDeductee = deducteeFilter ? lookups.deducteeTypes.find((item) => String(item.id) === deducteeFilter) : null;

    // Rate rows for the form: every active tax type, plus tax types already stored on the policy.
    function rateRows(taxRates) {
        const stored = taxRates || [];
        const list = lookups.taxTypes.map((type) => {
            const rate = stored.find((item) => item.tdsTaxTypeId === type.id);
            return { tdsTaxTypeId: type.id, taxTypeName: type.name, tdsRate: rate ? decimalText(rate.tdsRate) : '', tdsExemptionLimit: rate ? decimalText(rate.tdsExemptionLimit) : '' };
        });
        stored.forEach((rate) => {
            if (!list.some((item) => item.tdsTaxTypeId === rate.tdsTaxTypeId)) {
                list.push({ tdsTaxTypeId: rate.tdsTaxTypeId, taxTypeName: rate.taxTypeName || `Tax Type ${rate.tdsTaxTypeId}`, tdsRate: decimalText(rate.tdsRate), tdsExemptionLimit: decimalText(rate.tdsExemptionLimit) });
            }
        });
        return list;
    }

    function set(field) {
        return (value) => {
            setForm((current) => ({ ...current, [field]: value }));
            setErrors((current) => ({ ...current, [field]: undefined, duplicate: undefined }));
        };
    }

    function setRate(index, field, value) {
        setForm((current) => ({
            ...current,
            rates: current.rates.map((rate, position) => (position === index ? { ...rate, [field]: value } : rate)),
        }));
        setErrors((current) => ({ ...current, rates: undefined, [`rate-${index}`]: undefined, [`limit-${index}`]: undefined }));
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        if (lookups.taxTypes.length === 0) {
            pushToast({ message: 'No active duty / tax types are defined. Add them in Duty Tax first.', tone: 'error' });
            return;
        }
        setForm({ ...emptyPolicy, deducteeTypeId: deducteeFilter, rates: rateRows([]) });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await tdsPolicyApi.fetchPolicy(session, row.tdsPolicyId);
            setForm({
                tdsPolicyId: detail.tdsPolicyId,
                deducteeTypeId: detail.deducteeTypeId ? String(detail.deducteeTypeId) : '',
                naturePayId: detail.naturePayId ? String(detail.naturePayId) : '',
                applicableFrom: isoDay(detail.applicableFrom),
                rates: rateRows(detail.taxRates),
                // Inactive deductee type / nature of payment stored on the policy stay selectable on edit.
                storedDeductee: { id: row.deducteeTypeId, name: row.deducteeType },
                storedNature: { id: row.naturePayId, name: row.natureOfPayment, detail: row.sectionCode },
            });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Legacy TDS policy rules (+ server rules from TdsPolicyRepository).
    function validate() {
        const next = {};
        if (!form.deducteeTypeId) next.deducteeTypeId = 'Select the deductee type.';
        if (!form.naturePayId) next.naturePayId = 'Select the nature of payment.';
        if (!form.applicableFrom) next.applicableFrom = 'Select the Applicable From date.';
        let anyRate = false;
        form.rates.forEach((rate, index) => {
            const value = parseDecimal(rate.tdsRate);
            const limit = parseDecimal(rate.tdsExemptionLimit);
            if (Number.isNaN(value) || value < 0 || value > MAX_RATE) next[`rate-${index}`] = `0 – ${MAX_RATE}, up to 2 decimals.`;
            else if (value > 0) anyRate = true;
            if (Number.isNaN(limit) || limit < 0 || limit > MAX_EXEMPTION) next[`limit-${index}`] = 'Positive amount, up to 2 decimals.';
        });
        if (!anyRate && !Object.keys(next).some((key) => key.startsWith('rate-'))) next.rates = 'Enter the TDS rate for at least one tax type.';
        if (form.deducteeTypeId && form.naturePayId && form.applicableFrom) {
            const duplicate = rows.some((row) => row.tdsPolicyId !== form.tdsPolicyId
                && String(row.deducteeTypeId) === form.deducteeTypeId
                && String(row.naturePayId) === form.naturePayId
                && isoDay(row.applicableFrom) === form.applicableFrom);
            if (duplicate) next.applicableFrom = 'A policy already exists for this deductee type, nature of payment and date.';
        }
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
            const request = {
                tdsPolicyId: form.tdsPolicyId,
                deducteeTypeId: Number(form.deducteeTypeId),
                naturePayId: Number(form.naturePayId),
                applicableFrom: requestDate(form.applicableFrom),
                taxRates: form.rates
                    .map((rate) => ({ tdsTaxTypeId: rate.tdsTaxTypeId, tdsRate: parseDecimal(rate.tdsRate), tdsExemptionLimit: parseDecimal(rate.tdsExemptionLimit) }))
                    .filter((rate) => rate.tdsRate > 0 || rate.tdsExemptionLimit > 0),
            };
            const result = await tdsPolicyApi.savePolicy(session, request);
            pushToast({ message: (result && result.message) || 'TDS policy saved.' });
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
        if (!window.confirm(`Delete the TDS policy of ${row.natureOfPayment} (${row.deducteeType}) applicable from ${row.applicableFromText}?`)) {
            return;
        }
        try {
            const result = await tdsPolicyApi.deletePolicy(session, row.tdsPolicyId);
            pushToast({ message: (result && result.message) || 'TDS policy deleted.' });
            setRows((current) => current.filter((item) => item.tdsPolicyId !== row.tdsPolicyId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    function withStored(options, stored, withDetail) {
        const list = options.map((item) => ({ value: String(item.id), label: withDetail && item.detail ? `${item.name} (${item.detail})` : item.name }));
        if (stored && stored.id && !list.some((item) => item.value === String(stored.id))) {
            list.push({ value: String(stored.id), label: `${stored.name || stored.id}${withDetail && stored.detail ? ` (${stored.detail})` : ''} — inactive` });
        }
        return list;
    }

    const isNew = form && form.tdsPolicyId === 0;
    const filterOptions = lookups.deducteeTypes.map((item) => ({ value: String(item.id), label: item.name }));
    const deducteeOptions = form ? withStored(lookups.deducteeTypes, form.storedDeductee, false) : [];
    const natureOptions = form ? withStored(lookups.natureOfPayments, form.storedNature, true) : [];
    const selectedDeductee = form ? lookups.deducteeTypes.find((item) => String(item.id) === form.deducteeTypeId) : null;

    return (<section className="lg-page" aria-label="TDS Policy">
      <PageBar title="TDS Policy" addLabel="Add TDS Policy" busy={busy || loading} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <FieldGrid>
        <Select id="tp-filter" label="Deductee Type" value={deducteeFilter} options={filterOptions} placeholder="All deductee types" onChange={setDeducteeFilter}/>
        {filterDeductee && filterDeductee.detail ? <p className="lg-hint" style={{ alignSelf: 'end' }}>Status: Active · {filterDeductee.detail}</p> : <span/>}
      </FieldGrid>
      <LegacyGrid columns={columns} rows={visibleRows} getKey={(row) => row.tdsPolicyId} loading={loading} error={loadError} search={search} searchKeys={['deducteeType', 'natureOfPayment', 'sectionText', 'applicableFromText', 'ratesText']} rowActions={(row) => ({
          onEdit: () => void openEdit(row),
          onDelete: () => void remove(row),
      })}/>

      <FormModal title={isNew ? 'Add TDS Policy' : 'Edit TDS Policy'} isOpen={Boolean(form)} busy={busy} error={formError} wide onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <SearchableSelect id="tp-deductee" label="Deductee Type" required value={form.deducteeTypeId} options={deducteeOptions} placeholder="Select deductee type" error={errors.deducteeTypeId} onChange={set('deducteeTypeId')}/>
          <SearchableSelect id="tp-nature" label="Nature of Payment" required value={form.naturePayId} options={natureOptions} placeholder="Select nature of payment" error={errors.naturePayId} onChange={set('naturePayId')}/>
          <Input id="tp-from" label="Applicable From" required type="date" value={form.applicableFrom} error={errors.applicableFrom} onChange={set('applicableFrom')}/>
          {selectedDeductee && selectedDeductee.detail ? <p className="lg-hint" style={{ alignSelf: 'end' }}>{selectedDeductee.detail}</p> : <span/>}

          <FieldSection title="Tax Rates"/>
          <div style={{ gridColumn: '1 / -1' }}>
            <div className="lg-grid-wrap">
              <table className="lg-grid">
                <thead>
                  <tr>
                    <th className="lg-sno" scope="col">S.No</th>
                    <th scope="col">Duty / Tax Type</th>
                    <th scope="col" style={{ width: '160px' }}>Rate (%)</th>
                    <th scope="col" style={{ width: '200px' }}>Exemption Limit</th>
                  </tr>
                </thead>
                <tbody>
                  {form.rates.map((rate, index) => (<tr key={rate.tdsTaxTypeId}>
                    <td className="lg-sno">{index + 1}</td>
                    <td>{rate.taxTypeName}</td>
                    <td>
                      <input className={errors[`rate-${index}`] ? 'control is-invalid' : 'control'} type="text" inputMode="decimal" aria-label={`${rate.taxTypeName} rate (%)`} placeholder="0.00" style={{ textAlign: 'right' }} value={rate.tdsRate} onChange={(event) => setRate(index, 'tdsRate', event.target.value)} onBlur={() => { const value = parseDecimal(rate.tdsRate); if (!Number.isNaN(value)) setRate(index, 'tdsRate', decimalText(value)); }}/>
                      {errors[`rate-${index}`] ? <p className="field-error">{errors[`rate-${index}`]}</p> : null}
                    </td>
                    <td>
                      <input className={errors[`limit-${index}`] ? 'control is-invalid' : 'control'} type="text" inputMode="decimal" aria-label={`${rate.taxTypeName} exemption limit`} placeholder="0.00" style={{ textAlign: 'right' }} value={rate.tdsExemptionLimit} onChange={(event) => setRate(index, 'tdsExemptionLimit', event.target.value)} onBlur={() => { const value = parseDecimal(rate.tdsExemptionLimit); if (!Number.isNaN(value)) setRate(index, 'tdsExemptionLimit', decimalText(value)); }}/>
                      {errors[`limit-${index}`] ? <p className="field-error">{errors[`limit-${index}`]}</p> : null}
                    </td>
                  </tr>))}
                </tbody>
              </table>
            </div>
            {errors.rates ? <p className="field-error">{errors.rates}</p> : null}
            <p className="lg-hint">Leave a tax type blank (0.00) when it does not apply to this policy.</p>
          </div>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
