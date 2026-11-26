import { useCallback, useEffect, useMemo, useState } from 'react';
import { Input, SearchableSelect, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { FieldGrid, StepFormModal } from '../../Common/components/FormModal';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { accessFlag, ledgerApi, natureNames } from '../services/ledgerApi';

/*
 * Data › Finance Masters › Ledger (Head Office login).
 * Fields and rules follow the legacy ACCPPORTAL LedgerView.aspx / LedgerAdd.aspx screens and the
 * Acme.Masters LedgerRepository (api/ledgers).
 */

const BANK_ACCOUNTS_GROUP = 12;
const CASH_IN_HAND_GROUP = 13;
const FIXED_DEPOSIT_GROUP = 14;
const NON_RECURRING_EXPENSES = '2';
const NATURE_INCOME = 1;
const NATURE_EXPENSES = 2;

const ledgerTypeOptions = [{ value: 'GN', label: 'General' }, { value: 'IK', label: 'InKind' }];

// Legacy BUDGET_GROUP / BUDGET_SUB_GROUP / FD investment type values (Acme.Common enums); 0 = not set.
const budgetGroupOptions = [
    { value: '0', label: '-Select-' },
    { value: '1', label: 'Recurring Expenses' },
    { value: '2', label: 'Non-Recurring Expenses' },
    { value: '3', label: 'Recurring Income' },
    { value: '4', label: 'Non-Recurring Income' },
    { value: '5', label: 'Transfer other Sectors' },
    { value: '6', label: 'Contribution Province' },
];
const budgetSubGroupOptions = [
    { value: '0', label: '-Select-' },
    { value: '1', label: 'Regular Expenses' },
    { value: '2', label: 'Non-Regular Expenses' },
];
const fdInvestmentTypeOptions = [
    { value: '0', label: '-Select-' },
    { value: '1', label: 'FD' },
    { value: '2', label: 'RD - Post Office' },
    { value: '3', label: 'RD - Bank' },
    { value: '4', label: 'Mutual Fund' },
];

// Nature-specific options (legacy divIncomeNature / divExpenseNature).
const incomeFlags = [
    { key: 'isFDInterestLedger', label: 'Set as FD Interest Ledger' },
    { key: 'isSBInterestLedger', label: 'Set as SB Interest Ledger' },
    { key: 'isInKindLedger', label: 'Set as In-Kind Ledger' },
    { key: 'isAssetGainLedger', label: 'Set as Asset Gain Ledger' },
];
const expenseFlags = [
    { key: 'isFDPenaltyLedger', label: 'Set as FD Penalty Ledger' },
    { key: 'isBankCommissionLedger', label: 'Set as Bank Commission Ledger' },
    { key: 'isAssetLossLedger', label: 'Set as Asset Loss Ledger' },
    { key: 'isAssetDisposalLedger', label: 'Set as Asset Disposal Ledger' },
    { key: 'isDepriciationLedger', label: 'Set as Depreciation Ledger' },
];
// Only one of these may be ticked (legacy validateIncome/ExpenseCheckboxes_ClientValidate).
const exclusiveIncome = ['isFDInterestLedger', 'isInKindLedger', 'isAssetGainLedger'];
const exclusiveExpense = ['isAssetDisposalLedger', 'isDepriciationLedger', 'isAssetLossLedger'];

const emptyLedger = {
    ledgerId: 0, ledgerCode: '', ledgerName: '', groupId: '', ledgerType: 'GN', bankAccountId: 0,
    budgetGroupId: '0', budgetSubGroupId: '0', fdInvTypeId: '0', ledgerNotes: '', ledgerClosedOn: '',
    isCostCentre: false, isTDSApplicable: false,
    isFDInterestLedger: false, isSBInterestLedger: false, isInKindLedger: false, isAssetGainLedger: false,
    isFDPenaltyLedger: false, isBankCommissionLedger: false, isAssetLossLedger: false, isAssetDisposalLedger: false, isDepriciationLedger: false,
};

const MAX_CODE = 15;
const MAX_NAME = 100;
const MAX_NOTES = 500;

function text(value) {
    return value === null || value === undefined ? '' : String(value);
}

function flag(value) {
    return Number(value) === 1;
}

function isCashLedger(row) {
    return String(row.ledgerName || '').trim().toLowerCase() === 'cash';
}

function isBankLedger(row) {
    return String(row.ledgerSubType || '').toUpperCase() === 'BK' || row.groupId === BANK_ACCOUNTS_GROUP;
}

// Legacy LedgerView disables Edit and Delete for Cash, Bank Accounts and read-only ledgers.
function editBlockReason(row) {
    if (row.accessFlag === accessFlag.readonly) return 'Fixed ledger cannot be edited';
    if (isCashLedger(row)) return 'Cash ledger cannot be edited';
    if (isBankLedger(row)) return 'Bank ledgers are maintained from Bank Accounts';
    return '';
}

// Backend also refuses to delete ledgers under Cash-in-hand; ledgers with balances are refused by the server.
function deleteBlockReason(row) {
    const reason = editBlockReason(row);
    if (reason) return reason.replace('edited', 'deleted');
    if (row.groupId === CASH_IN_HAND_GROUP) return 'Cash-in-hand ledgers cannot be deleted';
    return '';
}

function TextArea({ id, label, value, onChange, rows = 2, maxLength, error }) {
    return (<div className="field">
      <label htmlFor={id}>{label}</label>
      <div>
        <textarea id={id} className={error ? 'control lg-textarea is-invalid' : 'control lg-textarea'} rows={rows} maxLength={maxLength} value={value} onChange={(event) => onChange(event.target.value)}/>
        {error ? <p className="field-error">{error}</p> : null}
      </div>
    </div>);
}

function Check({ id, label, checked, onChange }) {
    return (<label className="lg-check" htmlFor={id}>
      <input id={id} type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)}/>
      {label}
    </label>);
}

// Add / Edit is a 2-step form; each step lists the fields it validates before moving on.
const STEPS = ['Ledger Info', 'Options'];
const STEP_FIELDS = [
    ['groupId', 'ledgerCode', 'ledgerType', 'ledgerName', 'budgetGroupId', 'budgetSubGroupId', 'fdInvTypeId', 'ledgerClosedOn', 'ledgerNotes'],
    ['isTDSApplicable', 'isCostCentre', 'flags'],
];

function stepOf(field) {
    const index = STEP_FIELDS.findIndex((fields) => fields.includes(field));
    return index < 0 ? STEP_FIELDS.length - 1 : index;
}

const columns = [
    { key: 'ledgerCode', header: 'Code', width: '90px' },
    { key: 'ledgerName', header: 'Name', wrap: true },
    { key: 'group', header: 'Group', wrap: true },
    { key: 'nature', header: 'Nature', width: '110px' },
    { key: 'type', header: 'Ledger Type', width: '100px' },
];

export function LedgerPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [rows, setRows] = useState([]);
    const [groups, setGroups] = useState([]);
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
            const [ledgers, lookup] = await Promise.all([
                ledgerApi.fetchLedgers(session),
                ledgerApi.fetchGroupLookup(session),
            ]);
            setRows(ledgers);
            setGroups(lookup);
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    const groupOptions = useMemo(() => groups.map((group) => ({ value: String(group.groupId), label: group.ledgerGroup })), [groups]);
    const natureOf = useCallback((groupId) => {
        const match = groups.find((group) => String(group.groupId) === String(groupId));
        return match ? match.natureId : 0;
    }, [groups]);

    function set(field) {
        return (value) => {
            setForm((current) => {
                const next = { ...current, [field]: value };
                if (field === 'budgetGroupId' && value === NON_RECURRING_EXPENSES) {
                    next.budgetSubGroupId = '0';
                }
                if (field === 'groupId' && String(value) !== String(FIXED_DEPOSIT_GROUP)) {
                    next.fdInvTypeId = '0';
                }
                return next;
            });
            setErrors((current) => ({ ...current, [field]: undefined, flags: undefined }));
        };
    }

    function openAdd() {
        setErrors({});
        setFormError('');
        setStep(0);
        setForm({ ...emptyLedger });
    }

    async function openEdit(row) {
        setErrors({});
        setFormError('');
        setStep(0);
        setBusy(true);
        try {
            const detail = await ledgerApi.fetchLedger(session, row.ledgerId);
            setForm({
                ...emptyLedger,
                ledgerId: detail.ledgerId,
                ledgerCode: text(detail.ledgerCode),
                ledgerName: text(detail.ledgerName),
                groupId: detail.groupId ? String(detail.groupId) : '',
                ledgerType: String(detail.ledgerType || 'GN').toUpperCase() === 'IK' ? 'IK' : 'GN',
                bankAccountId: detail.bankAccountId || 0,
                budgetGroupId: text(detail.budgetGroupId ?? 0),
                budgetSubGroupId: text(detail.budgetSubGroupId ?? 0),
                fdInvTypeId: text(detail.fdInvestmentTypeId ?? 0),
                ledgerNotes: text(detail.notes),
                ledgerClosedOn: detail.dateClosed ? String(detail.dateClosed).slice(0, 10) : '',
                isCostCentre: flag(detail.isCostCenter),
                isTDSApplicable: flag(detail.isTdsLedger),
                isFDInterestLedger: flag(detail.isBankInterestLedger),
                isSBInterestLedger: flag(detail.isBankSbInterestLedger),
                isInKindLedger: flag(detail.isInkindLedger),
                isAssetGainLedger: flag(detail.isAssetGainLedger),
                isFDPenaltyLedger: flag(detail.isBankFdPenaltyLedger),
                isBankCommissionLedger: flag(detail.isBankCommissionLedger),
                isAssetLossLedger: flag(detail.isAssetLossLedger),
                isAssetDisposalLedger: flag(detail.isDisposalLedger),
                isDepriciationLedger: flag(detail.isDepreciationLedger),
            });
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setBusy(false);
        }
    }

    // Legacy LedgerAdd + LedgerRepository: group, ledger type and name required; name ≤ 100, code ≤ 15;
    // only one special income / expense option at a time. Duplicate name is checked by the server.
    function validate(nature) {
        const next = {};
        if (!form.groupId) next.groupId = 'Select the ledger group.';
        if (!form.ledgerType) next.ledgerType = 'Select the ledger type.';
        const name = form.ledgerName.trim();
        if (!name) next.ledgerName = 'Enter the ledger name.';
        else if (name.length > MAX_NAME) next.ledgerName = `Ledger name cannot exceed ${MAX_NAME} characters.`;
        if (form.ledgerCode.trim().length > MAX_CODE) next.ledgerCode = `Code cannot exceed ${MAX_CODE} characters.`;
        if (form.ledgerNotes.length > MAX_NOTES) next.ledgerNotes = `Notes cannot exceed ${MAX_NOTES} characters.`;
        if (nature === NATURE_INCOME && exclusiveIncome.filter((key) => form[key]).length > 1) {
            next.flags = 'Choose only one of FD Interest, In-Kind or Asset Gain ledger.';
        }
        if (nature === NATURE_EXPENSES && exclusiveExpense.filter((key) => form[key]).length > 1) {
            next.flags = 'Choose only one of Asset Loss, Asset Disposal or Depreciation ledger.';
        }
        return next;
    }

    // Next: only the current step's fields must be valid.
    function checkStep(index) {
        const stepErrors = Object.fromEntries(Object.entries(validate(natureOf(form.groupId))).filter(([field]) => stepOf(field) === index));
        setErrors(stepErrors);
        return Object.keys(stepErrors).length === 0;
    }

    async function save() {
        const nature = natureOf(form.groupId);
        const next = validate(nature);
        setErrors(next);
        const failed = Object.keys(next);
        if (failed.length > 0) {
            setFormError(next.flags || '');
            // Go back to the first step that still has a problem.
            setStep(Math.min(...failed.map(stepOf)));
            return;
        }
        const income = nature === NATURE_INCOME;
        const expense = nature === NATURE_EXPENSES;
        const bit = (key, allowed) => (allowed && form[key] ? 1 : 0);
        const groupId = Number(form.groupId);
        const request = {
            ledgerId: form.ledgerId,
            ledgerCode: form.ledgerCode.trim(),
            ledgerName: form.ledgerName.trim(),
            groupId,
            ledgerType: form.ledgerType,
            bankAccountId: Number(form.bankAccountId) || 0,
            isCostCentre: form.isCostCentre ? 1 : 0,
            isTDSApplicable: form.isTDSApplicable ? 1 : 0,
            ledgerNotes: form.ledgerNotes.trim(),
            isFDInterestLedger: bit('isFDInterestLedger', income),
            isSBInterestLedger: bit('isSBInterestLedger', income),
            isInKindLedger: bit('isInKindLedger', income),
            isAssetGainLedger: bit('isAssetGainLedger', income),
            isFDPenaltyLedger: bit('isFDPenaltyLedger', expense),
            isBankCommissionLedger: bit('isBankCommissionLedger', expense),
            isAssetLossLedger: bit('isAssetLossLedger', expense),
            isAssetDisposalLedger: bit('isAssetDisposalLedger', expense),
            isDepriciationLedger: bit('isDepriciationLedger', expense),
            budgetGroupId: Number(form.budgetGroupId) || 0,
            budgetSubGroupId: form.budgetGroupId === NON_RECURRING_EXPENSES ? 0 : Number(form.budgetSubGroupId) || 0,
            fdInvTypeId: groupId === FIXED_DEPOSIT_GROUP ? Number(form.fdInvTypeId) || 0 : 0,
            ledgerClosedOn: form.ledgerClosedOn || null,
        };
        setBusy(true);
        setFormError('');
        try {
            const result = await ledgerApi.saveLedger(session, request);
            pushToast({ message: (result && result.message) || 'Ledger saved.' });
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
        if (!window.confirm(`Delete ledger ${row.ledgerName}?`)) {
            return;
        }
        try {
            const result = await ledgerApi.deleteLedger(session, row.ledgerId);
            pushToast({ message: (result && result.message) || 'Ledger deleted.' });
            setRows((current) => current.filter((item) => item.ledgerId !== row.ledgerId));
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
    }

    const gridRows = useMemo(() => rows.map((row) => ({
        ...row,
        nature: natureNames[row.natureId] || '',
        type: String(row.ledgerType || '').toUpperCase() === 'IK' ? 'InKind' : 'General',
    })), [rows]);

    const isNew = form && form.ledgerId === 0;
    const nature = form ? natureOf(form.groupId) : 0;
    const isFixedDeposit = form && String(form.groupId) === String(FIXED_DEPOSIT_GROUP);

    return (<section className="lg-page" aria-label="Ledger">
      <PageBar title="Ledger" addLabel="Add Ledger" busy={busy} search={search} onSearch={setSearch} onRefresh={() => void load()} onAdd={openAdd}/>
      <LegacyGrid columns={columns} rows={gridRows} getKey={(row) => row.ledgerId} loading={loading} error={loadError} search={search} searchKeys={['ledgerCode', 'ledgerName', 'group', 'nature', 'type']} rowActions={(row) => {
          const editBlocked = editBlockReason(row);
          const deleteBlocked = deleteBlockReason(row);
          return {
              onEdit: editBlocked ? undefined : () => void openEdit(row),
              onDelete: () => void remove(row),
              deleteDisabled: Boolean(deleteBlocked),
              deleteTitle: deleteBlocked || 'Delete',
          };
      }}/>

      <StepFormModal title={isNew ? 'Add Ledger' : 'Edit Ledger'} isOpen={Boolean(form)} busy={busy} error={formError} steps={STEPS} step={step} onStep={(index) => { setErrors({}); setStep(index); }} onNext={checkStep} onClose={() => setForm(null)} onSave={() => void save()}>
        {form && step === 0 ? <FieldGrid>
          <SearchableSelect id="ld-group" label="Under the Group" required value={form.groupId} options={groupOptions} placeholder="Select ledger group" error={errors.groupId} onChange={set('groupId')}/>
          <Input id="ld-code" label="Code" value={form.ledgerCode} error={errors.ledgerCode} onChange={set('ledgerCode')}/>
          <Select id="ld-type" label="Ledger Type" required hidePlaceholder value={form.ledgerType} options={ledgerTypeOptions} error={errors.ledgerType} onChange={set('ledgerType')}/>
          <Input id="ld-name" label="Name" required value={form.ledgerName} error={errors.ledgerName} onChange={set('ledgerName')}/>
          <Select id="ld-budget" label="Budget Group" hidePlaceholder value={form.budgetGroupId} options={budgetGroupOptions} onChange={set('budgetGroupId')}/>
          <Select id="ld-budget-sub" label="Budget Sub Group" hidePlaceholder value={form.budgetSubGroupId} options={budgetSubGroupOptions} disabled={form.budgetGroupId === NON_RECURRING_EXPENSES} onChange={set('budgetSubGroupId')}/>
          {isFixedDeposit ? <Select id="ld-fd-type" label="FD Investment Type" hidePlaceholder value={form.fdInvTypeId} options={fdInvestmentTypeOptions} onChange={set('fdInvTypeId')}/> : null}
          <Input id="ld-closed" label="Closed On" type="date" value={form.ledgerClosedOn} onChange={set('ledgerClosedOn')}/>
          <TextArea id="ld-notes" label="Notes" value={form.ledgerNotes} maxLength={MAX_NOTES} error={errors.ledgerNotes} onChange={set('ledgerNotes')}/>
        </FieldGrid> : null}

        {form && step === 1 ? <FieldGrid>
          <Check id="ld-tds" label="Is TDS Applicable" checked={form.isTDSApplicable} onChange={set('isTDSApplicable')}/>
          <Check id="ld-cc" label="Include Cost Centre For this Ledger" checked={form.isCostCentre} onChange={set('isCostCentre')}/>
          {nature === NATURE_INCOME ? incomeFlags.map((item) => <Check key={item.key} id={`ld-${item.key}`} label={item.label} checked={form[item.key]} onChange={set(item.key)}/>) : null}
          {nature === NATURE_EXPENSES ? expenseFlags.map((item) => <Check key={item.key} id={`ld-${item.key}`} label={item.label} checked={form[item.key]} onChange={set(item.key)}/>) : null}
        </FieldGrid> : null}
      </StepFormModal>
    </section>);
}
