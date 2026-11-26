import { useCallback, useEffect, useState } from 'react';
import { Input } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, FormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar, StatusText } from '../../Common/components/LegacyGrid';
import { accountingPeriodApi, gridDate, isoDay, requestDate } from '../services/periodLockApi';

/*
 * Data › Finance Masters › Accounting Period (Head Office login).
 * Mirrors legacy AccountingPeriodView.aspx / AccountingPeriodAdd.aspx on api/accountingperiods.
 * Server rules: Year From / Year To required, Year From not after Year To, no existing period inside the
 * range, the first period becomes active, only one active period, the active period or one with ledger
 * balances cannot be deleted.
 */

const emptyPeriod = { accPeriodId: 0, yearFrom: '', yearTo: '' };

// Local calendar arithmetic on yyyy-mm-dd strings (no time zone shift).
function shiftDay(day, { years = 0, days = 0 }) {
    const [year, month, date] = day.split('-').map(Number);
    const next = new Date(year + years, month - 1, date + days);
    const pad = (value) => String(value).padStart(2, '0');
    return `${next.getFullYear()}-${pad(next.getMonth() + 1)}-${pad(next.getDate())}`;
}

// Legacy defaults for a new period: day after the latest Year To, else 1 April of this year; one year long.
function nextPeriod(rows) {
    const latest = rows.map((row) => isoDay(row.yearTo)).filter(Boolean).sort().pop();
    const yearFrom = latest ? shiftDay(latest, { days: 1 }) : `${new Date().getFullYear()}-04-01`;
    return { yearFrom, yearTo: shiftDay(yearFrom, { years: 1, days: -1 }) };
}

function periodText(row) {
    return `${gridDate(row.yearFrom)} - ${gridDate(row.yearTo)}`;
}

const columns = [
    { key: 'yearFromText', header: 'Year From', width: '140px' },
    { key: 'yearToText', header: 'Year To', width: '140px' },
    { key: 'yearStatus', header: 'Year Status', render: (row) => <StatusText value={row.yearStatus}/> },
];

export function AccountingPeriodPage() {
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
            const periods = await accountingPeriodApi.fetchPeriods(session);
            setRows(periods.map((row) => ({
                ...row,
                yearFromText: gridDate(row.yearFrom),
                yearToText: gridDate(row.yearTo),
                yearStatus: row.yearStatus || (row.status === 1 ? 'Active' : 'Inactive'),
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

    function set(field) {
        return (value) => {
            setForm((current) => ({ ...current, [field]: value }));
            setErrors((current) => ({ ...current, [field]: undefined }));
        };
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setForm({ ...emptyPeriod, ...nextPeriod(rows) });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setBusy(true);
        try {
            const detail = await accountingPeriodApi.fetchPeriod(session, row.accYearId);
            setForm({ accPeriodId: detail.accYearId, yearFrom: isoDay(detail.yearFrom), yearTo: isoDay(detail.yearTo) });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Legacy AccountingPeriodAdd rules + a client-side overlap check against the loaded periods
    // (the server only rejects a range that fully contains another period).
    function validate() {
        const next = {};
        if (!form.yearFrom) next.yearFrom = 'Select the Year From date.';
        if (!form.yearTo) next.yearTo = 'Select the Year To date.';
        if (form.yearFrom && form.yearTo && form.yearFrom > form.yearTo) next.yearTo = 'Year To cannot be before Year From.';
        if (!next.yearFrom && !next.yearTo) {
            const clash = rows.find((row) => row.accYearId !== form.accPeriodId
                && isoDay(row.yearFrom) <= form.yearTo && isoDay(row.yearTo) >= form.yearFrom);
            if (clash) next.yearFrom = `This period overlaps the accounting period ${periodText(clash)}.`;
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
            const result = await accountingPeriodApi.savePeriod(session, {
                accPeriodId: form.accPeriodId,
                yearFrom: requestDate(form.yearFrom),
                yearTo: requestDate(form.yearTo),
            });
            pushToast({ message: (result && result.message) || 'Accounting period saved.' });
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

    async function activate(row) {
        if (!window.confirm(`Make ${periodText(row)} the active accounting period? The current active period will be deactivated.`)) {
            return;
        }
        try {
            const result = await accountingPeriodApi.activatePeriod(session, row.accYearId);
            pushToast({ message: (result && result.message) || 'Accounting period activated.' });
            await load();
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    async function remove(row) {
        if (!window.confirm(`Delete accounting period ${periodText(row)}?`)) {
            return;
        }
        try {
            const result = await accountingPeriodApi.deletePeriod(session, row.accYearId);
            pushToast({ message: (result && result.message) || 'Accounting period deleted.' });
            setRows((current) => current.filter((item) => item.accYearId !== row.accYearId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const isNew = form && form.accPeriodId === 0;

    return (<section className="lg-page" aria-label="Accounting Period">
      <PageBar title="Accounting Period" addLabel="Add Accounting Period" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={rows} getKey={(row) => row.accYearId} loading={loading} error={loadError} search={search} searchKeys={['yearFromText', 'yearToText', 'yearStatus']} rowActions={(row) => {
          const active = row.status === 1;
          return {
              onEdit: () => void openEdit(row),
              // Only an inactive period can be switched on; activating one deactivates the others.
              onToggle: active ? undefined : () => void activate(row),
              toggleTitle: 'Activate',
              onDelete: () => void remove(row),
              deleteDisabled: active,
              deleteTitle: active ? 'The active period cannot be deleted' : 'Delete',
          };
      }}/>

      <FormModal title={isNew ? 'Add Accounting Period' : 'Edit Accounting Period'} isOpen={Boolean(form)} busy={busy} error={formError} onClose={() => setForm(null)} onSave={() => void save()}>
        {form ? <FieldGrid>
          <Input id="ap-from" label="Year From" required type="date" value={form.yearFrom} error={errors.yearFrom} onChange={set('yearFrom')}/>
          <Input id="ap-to" label="Year To" required type="date" value={form.yearTo} error={errors.yearTo} onChange={set('yearTo')}/>
        </FieldGrid> : null}
      </FormModal>
    </section>);
}
