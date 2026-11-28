import { useCallback, useEffect, useMemo, useState } from 'react';
import { Modal, SearchableSelect, Select } from '../../../Common/components';
import { RefreshIcon } from '../../../Common/components/Icons';
import { useAuth } from '../../../Context/AuthContext';
import { errorMessage } from '../../../utilities/errors';
import { dashboardApi } from '../services/dashboardApi';

/*
 * Home › Head Office Dashboard. API: Acme.Masters.Api api/hodashboard.
 *
 * Ported from the legacy ACCPPORTAL home pages for a head office login:
 *   HomeLogin.aspx (Quick View)  – Branch / Project filter + View Status, Cash / Bank / FD closing balance
 *                                  ("Balance as on" the latest voucher date), project-wise data status and the
 *                                  Non Conforming Branches list (branches with months that have no vouchers).
 *   HomeDashboard.aspx (Detail)  – Data Status: number of vouchers by branch and month for the active financial year
 *                                  (green = data uploaded, red = nothing for that month).
 */

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

function monthShort(key) {
    const [year, month] = String(key).split('-');
    return `${MONTHS[Number(month) - 1]}-${year.slice(2)}`;
}

function monthLong(key) {
    const [year, month] = String(key).split('-');
    return `${MONTHS[Number(month) - 1]} ${year}`;
}

function formatDate(value) {
    if (!value) {
        return '—';
    }
    const [date] = String(value).split('T');
    const [year, month, day] = date.split('-');
    return `${day}-${month}-${year}`;
}

function formatAmount(value) {
    return Number(value || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatCount(value) {
    return Number(value || 0).toLocaleString('en-IN');
}

// "yyyy-MM" (month input) → first / last day of that month as "yyyy-MM-dd".
function monthStart(value) {
    return value ? `${value}-01` : '';
}

function monthEnd(value) {
    if (!value) {
        return '';
    }
    const [year, month] = value.split('-').map(Number);
    const last = new Date(year, month, 0).getDate();
    return `${value}-${String(last).padStart(2, '0')}`;
}

function toMonthInput(value) {
    return value ? String(value).slice(0, 7) : '';
}

const emptyCriteria = { yearId: '', branchId: '', projectId: '', pendingFrom: '', pendingTo: '' };

// Monthly voucher totals – one series, so no legend; the panel title names it. Hover a month for its count.
function MonthlyChart({ months, totals }) {
    const [hover, setHover] = useState(-1);
    const width = 720;
    const height = 190;
    const pad = { top: 18, right: 8, bottom: 26, left: 44 };
    const plotW = width - pad.left - pad.right;
    const plotH = height - pad.top - pad.bottom;
    const max = Math.max(...totals, 0);
    const step = max <= 0 ? 1 : Math.pow(10, Math.floor(Math.log10(max)));
    const top = max <= 0 ? 1 : Math.ceil(max / step) * step;
    const ticks = [0, top / 2, top];
    const slot = plotW / Math.max(months.length, 1);
    const barW = Math.min(34, slot - 8);
    const maxIndex = totals.indexOf(max);
    const y = (value) => pad.top + plotH - (value / top) * plotH;

    return (<div className="hd-chart">
      <svg viewBox={`0 0 ${width} ${height}`} role="img" aria-label="Vouchers per month" onMouseLeave={() => setHover(-1)}>
        {ticks.map((tick) => (<g key={tick}>
          <line className="hd-chart-grid" x1={pad.left} x2={width - pad.right} y1={y(tick)} y2={y(tick)}/>
          <text className="hd-chart-axis" x={pad.left - 6} y={y(tick) + 4} textAnchor="end">{formatCount(tick)}</text>
        </g>))}
        {totals.map((value, index) => {
            const x = pad.left + slot * index + (slot - barW) / 2;
            const h = Math.max(0, y(0) - y(value));
            const r = Math.min(4, h, barW / 2);
            const path = h > 0
                ? `M${x},${y(0)} V${y(value) + r} Q${x},${y(value)} ${x + r},${y(value)} H${x + barW - r} Q${x + barW},${y(value)} ${x + barW},${y(value) + r} V${y(0)} Z`
                : '';
            return (<g key={months[index]}>
              {path ? <path className={hover === index ? 'hd-chart-bar is-hover' : 'hd-chart-bar'} d={path}/> : null}
              <text className="hd-chart-axis" x={pad.left + slot * index + slot / 2} y={height - 8} textAnchor="middle">{monthShort(months[index])}</text>
              {index === maxIndex && value > 0 ? (<text className="hd-chart-value" x={x + barW / 2} y={y(value) - 5} textAnchor="middle">{formatCount(value)}</text>) : null}
              <rect className="hd-chart-hit" x={pad.left + slot * index} y={pad.top} width={slot} height={plotH} onMouseEnter={() => setHover(index)}>
                <title>{`${monthLong(months[index])}: ${formatCount(value)} vouchers`}</title>
              </rect>
            </g>);
        })}
        <line className="hd-chart-base" x1={pad.left} x2={width - pad.right} y1={y(0)} y2={y(0)}/>
      </svg>
      {hover >= 0 ? (<div className="hd-tooltip" style={{ left: `${((pad.left + slot * hover + slot / 2) / width) * 100}%` }}>
        <strong>{monthLong(months[hover])}</strong>
        <span>{formatCount(totals[hover])} vouchers</span>
      </div>) : null}
    </div>);
}

export function HeadOfficeDashboardPage() {
    const { session } = useAuth();
    const [filters, setFilters] = useState({ years: [], branches: [], projects: [] });
    const [filterError, setFilterError] = useState('');
    const [criteria, setCriteria] = useState(emptyCriteria);
    const [summary, setSummary] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [search, setSearch] = useState('');
    const [balance, setBalance] = useState(null);

    const loadSummary = useCallback(async (next) => {
        setLoading(true);
        setError('');
        try {
            const result = await dashboardApi.fetchSummary(session, {
                yearId: Number(next.yearId) || 0,
                branchId: Number(next.branchId) || 0,
                projectId: Number(next.projectId) || 0,
                pendingFrom: monthStart(next.pendingFrom),
                pendingTo: monthEnd(next.pendingTo),
            });
            setSummary(result);
            // Show the year and period the server actually used.
            setCriteria((current) => ({
                ...current,
                yearId: result.yearId ? String(result.yearId) : current.yearId,
                pendingFrom: toMonthInput(result.pendingFrom),
                pendingTo: toMonthInput(result.pendingTo),
            }));
        }
        catch (failure) {
            setError(errorMessage(failure));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    const loadAll = useCallback(async () => {
        setFilterError('');
        setLoading(true);
        try {
            const result = await dashboardApi.fetchFilters(session);
            setFilters({ years: result.years || [], branches: result.branches || [], projects: result.projects || [] });
            // No year sent: the server opens on the active year, or on the latest year with vouchers when the
            // active year has none yet; loadSummary then shows that year in the combo.
            setCriteria(emptyCriteria);
            await loadSummary(emptyCriteria);
        }
        catch (failure) {
            setFilterError(errorMessage(failure));
            setLoading(false);
        }
    }, [session, loadSummary]);

    useEffect(() => { void loadAll(); }, [loadAll]);

    const yearOptions = filters.years.map((year) => ({
        value: String(year.id),
        label: `${formatDate(year.yearFrom)} to ${formatDate(year.yearTo)}${year.isActive ? ' (Active)' : ''}`,
    }));
    const branchOptions = useMemo(() => [{ value: '', label: 'All branches' },
        ...filters.branches.map((branch) => ({ value: String(branch.id), label: branch.name }))], [filters.branches]);
    // Project combo follows the branch (legacy LoadProjects(BranchId)).
    const projectOptions = useMemo(() => {
        const branchId = Number(criteria.branchId) || 0;
        const list = filters.projects.filter((project) => branchId === 0 ? project.branchIds.length > 0 : project.branchIds.includes(branchId));
        return [{ value: '', label: 'All projects' }, ...list.map((project) => ({ value: String(project.id), label: project.name }))];
    }, [filters.projects, criteria.branchId]);

    function setField(key) {
        return (value) => setCriteria((current) => {
            const next = { ...current, [key]: value };
            if (key === 'branchId' && next.projectId) {
                const project = filters.projects.find((item) => String(item.id) === next.projectId);
                if (value && project && !project.branchIds.includes(Number(value))) {
                    next.projectId = '';
                }
            }
            if (key === 'yearId') {
                next.pendingFrom = '';
                next.pendingTo = '';
            }
            return next;
        });
    }

    function viewStatus() {
        void loadSummary(criteria);
    }

    function drillToBranch(branchId) {
        const next = { ...criteria, branchId: String(branchId), projectId: '' };
        setCriteria(next);
        void loadSummary(next);
    }

    function showAllBranches() {
        const next = { ...criteria, branchId: '', projectId: '' };
        setCriteria(next);
        void loadSummary(next);
    }

    const rows = useMemo(() => {
        const list = summary ? summary.rows : [];
        const text = search.trim().toLowerCase();
        return text ? list.filter((row) => row.name.toLowerCase().includes(text) || row.code.toLowerCase().includes(text)) : list;
    }, [summary, search]);

    const isBranchRows = summary && summary.rowKind === 'branch';
    const chosenBranch = summary && !isBranchRows ? filters.branches.find((item) => String(item.id) === criteria.branchId) : null;
    const silentBranches = summary ? summary.branches - summary.branchesWithData : 0;

    if (filterError) {
        return (<section className="hd-page" aria-label="Head Office Dashboard">
          <p className="lg-form-error hd-retry" role="alert">
            <span>{filterError}</span>
            <button type="button" className="lg-btn" onClick={() => void loadAll()}>Retry</button>
          </p>
        </section>);
    }

    return (<section className="hd-page" aria-label="Head Office Dashboard">
      <header className="hd-head">
        <div className="hd-title">
          <p>{summary ? `Financial year (${formatDate(summary.yearFrom)} to ${formatDate(summary.yearTo)})` : 'Loading…'}</p>
        </div>
        <div className="hd-filters">
          <div className="hd-filter is-year"><Select id="hd-year" label="Financial Year" value={criteria.yearId} options={yearOptions} hidePlaceholder disabled={loading} onChange={setField('yearId')}/></div>
          <div className="hd-filter"><SearchableSelect id="hd-branch" label="Branch" value={criteria.branchId} options={branchOptions} placeholder="All branches" disabled={loading} onChange={setField('branchId')}/></div>
          <div className="hd-filter is-wide"><SearchableSelect id="hd-project" label="Project" value={criteria.projectId} options={projectOptions} placeholder="All projects" disabled={loading} onChange={setField('projectId')}/></div>
          <button type="button" className="lg-btn is-primary hd-go" disabled={loading} onClick={viewStatus}>{loading ? 'Loading…' : 'View Status'}</button>
          <button type="button" className="lg-icon hd-refresh" title="Reload" aria-label="Reload" disabled={loading} onClick={() => void loadSummary(criteria)}><RefreshIcon/></button>
        </div>
      </header>

      {error ? <p className="lg-form-error" role="alert">{error}</p> : null}

      {summary ? (<div className={loading ? 'hd-body is-loading' : 'hd-body'} aria-busy={loading}>
        <div className="hd-kpis">
          <article className="hd-kpi">
            <span className="hd-kpi-label">Vouchers this year</span>
            <strong className="hd-kpi-value">{formatCount(summary.totalVouchers)}</strong>
            <span className="hd-kpi-note">Last voucher {formatDate(summary.lastVoucherDate)}</span>
          </article>
          <article className="hd-kpi">
            <span className="hd-kpi-label">Branches with data</span>
            <strong className="hd-kpi-value">{formatCount(summary.branchesWithData)}<small> / {formatCount(summary.branches)}</small></strong>
            <span className="hd-kpi-note">{silentBranches > 0 ? `${formatCount(silentBranches)} without any voucher` : 'All branches uploaded'}</span>
          </article>
          <article className="hd-kpi">
            <span className="hd-kpi-label">Projects</span>
            <strong className="hd-kpi-value">{formatCount(summary.projects)}</strong>
            <span className="hd-kpi-note">{chosenBranch ? `Mapped to ${chosenBranch.name}` : 'Mapped to branches'}</span>
          </article>
          <article className={summary.nonConforming.length ? 'hd-kpi is-alert' : 'hd-kpi'}>
            <span className="hd-kpi-label">Non conforming branches</span>
            <strong className="hd-kpi-value">{formatCount(summary.nonConforming.length)}</strong>
            <span className="hd-kpi-note">No vouchers in {monthLong(toMonthInput(summary.pendingFrom))} – {monthLong(toMonthInput(summary.pendingTo))}</span>
          </article>
        </div>

        <section className="hd-panel hd-balances" aria-label="Closing balance">
          <div className="hd-panel-head">
            <h2>Balance as on {formatDate(summary.balanceAsOn)}</h2>
            <span className="hd-panel-note">Cash, bank and fixed deposit ledgers{chosenBranch ? ` · ${chosenBranch.name}` : ' · all branches'}</span>
          </div>
          <div className="hd-balance-row">
            {summary.balances.map((group) => (<button type="button" key={group.groupId} className="hd-balance" disabled={!group.ledgers.length} onClick={() => setBalance(group)} title={group.ledgers.length ? `Show ${group.group.toLowerCase()} ledgers` : 'No balance'}>
              <span className="hd-balance-label">{group.group}</span>
              <span className="hd-balance-value">{formatAmount(group.amount)} <em className={group.mode === 'CR' ? 'is-cr' : 'is-dr'}>{group.mode}</em></span>
              <span className="hd-balance-note">{group.ledgers.length} ledger{group.ledgers.length === 1 ? '' : 's'}{group.ledgers.length ? ' · view' : ''}</span>
            </button>))}
          </div>
        </section>

        <div className="hd-split">
          <section className="hd-panel hd-chart-panel" aria-label="Vouchers per month">
            <div className="hd-panel-head">
              <h2>Vouchers per month</h2>
              <span className="hd-panel-note">{formatCount(summary.totalVouchers)} in the year</span>
            </div>
            <MonthlyChart months={summary.months} totals={summary.monthTotals}/>
          </section>

          <section className="hd-panel hd-nc" aria-label="Non conforming branches">
            <div className="hd-panel-head">
              <h2>Non Conforming Branches <span className="hd-count">{summary.nonConforming.length}</span></h2>
            </div>
            <div className="hd-period">
              <label htmlFor="hd-pending-from">From</label>
              <input id="hd-pending-from" className="control" type="month" value={criteria.pendingFrom} min={toMonthInput(summary.yearFrom)} max={toMonthInput(summary.yearTo)} onChange={(event) => setField('pendingFrom')(event.target.value)}/>
              <label htmlFor="hd-pending-to">To</label>
              <input id="hd-pending-to" className="control" type="month" value={criteria.pendingTo} min={toMonthInput(summary.yearFrom)} max={toMonthInput(summary.yearTo)} onChange={(event) => setField('pendingTo')(event.target.value)}/>
              <button type="button" className="lg-btn" disabled={loading} onClick={viewStatus}>Go</button>
            </div>
            <div className="hd-nc-list">
              {summary.nonConforming.length ? (<table className="hd-table">
                <thead><tr><th className="hd-col-no">No</th><th>Branch</th><th>Incharge / Contact</th><th>Pending Months</th></tr></thead>
                <tbody>
                  {summary.nonConforming.map((row, index) => (<tr key={row.branchId}>
                    <td className="hd-col-no">{index + 1}</td>
                    <td><strong>{row.name}</strong><span className="hd-sub">{row.code}</span></td>
                    <td>
                      {row.incharge || <span className="hd-muted">—</span>}
                      {row.mobile ? <span className="hd-sub">{row.mobile}</span> : null}
                      {row.email ? <span className="hd-sub hd-email" title={row.email}>{row.email.split(',')[0]}{row.email.includes(',') ? ' …' : ''}</span> : null}
                    </td>
                    <td><div className="hd-chips">{row.pendingMonths.map((month) => <span key={month} className="hd-chip">{month}</span>)}</div></td>
                  </tr>))}
                </tbody>
              </table>) : (<p className="hd-empty">Every branch has vouchers in each month of this period.</p>)}
            </div>
          </section>
        </div>

        <section className="hd-panel" aria-label="Data status">
          <div className="hd-panel-head">
            <h2>Data Status {isBranchRows ? 'by Branch' : 'by Project'}</h2>
            {chosenBranch ? (<span className="hd-crumb">{chosenBranch.name} <button type="button" className="hd-link" onClick={showAllBranches}>Show all branches</button></span>) : null}
            <input className="control hd-search" type="search" placeholder={isBranchRows ? 'Search branch' : 'Search project'} value={search} aria-label="Search rows" onChange={(event) => setSearch(event.target.value)}/>
          </div>
          <div className="hd-grid-box">
            <table className="hd-grid">
              <thead>
                <tr>
                  <th className="hd-col-name">{isBranchRows ? 'Branch Office' : 'Project'}</th>
                  {summary.months.map((month) => <th key={month} className="hd-col-month">{monthShort(month)}</th>)}
                  <th className="hd-col-total">Total</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => (<tr key={row.id}>
                  <th scope="row" className="hd-col-name">
                    {isBranchRows
                      ? <button type="button" className="hd-link" title="Show this branch's projects" onClick={() => drillToBranch(row.id)}>{row.name}</button>
                      : row.name}
                  </th>
                  {row.counts.map((count, index) => (<td key={summary.months[index]} className={count > 0 ? 'hd-cell is-data' : 'hd-cell is-zero'} title={`${row.name} · ${monthLong(summary.months[index])}: ${count} vouchers`}>{formatCount(count)}</td>))}
                  <td className="hd-col-total">{formatCount(row.total)}</td>
                </tr>))}
                {!rows.length ? (<tr><td className="hd-empty" colSpan={summary.months.length + 2}>{search ? 'No rows match the search.' : 'Nothing to show for this selection.'}</td></tr>) : null}
              </tbody>
              {rows.length ? (<tfoot>
                <tr>
                  <th scope="row" className="hd-col-name">Total</th>
                  {summary.months.map((month, index) => <td key={month}>{formatCount(rows.reduce((sum, row) => sum + row.counts[index], 0))}</td>)}
                  <td className="hd-col-total">{formatCount(rows.reduce((sum, row) => sum + row.total, 0))}</td>
                </tr>
              </tfoot>) : null}
            </table>
          </div>
          <p className="hd-note"><strong>Note:</strong> number of vouchers by {isBranchRows ? 'branch' : 'project'} and month for the selected financial year. <span className="hd-key is-data">green</span> = data uploaded, <span className="hd-key is-zero">red</span> = no vouchers for that month.</p>
        </section>
      </div>) : (loading ? <p className="hd-loading">Loading dashboard…</p> : null)}

      <Modal title={balance ? `${balance.group} balance as on ${formatDate(summary && summary.balanceAsOn)}` : ''} isOpen={Boolean(balance)} onClose={() => setBalance(null)} showClose className="hd-balance-modal"
        footer={<button type="button" className="lg-btn" onClick={() => setBalance(null)}>Close</button>}>
        {balance ? (<div className="hd-ledger-box">
          <table className="hd-table">
            <thead><tr><th className="hd-col-no">No</th><th>Ledger</th><th className="hd-num">Amount</th><th>Dr/Cr</th></tr></thead>
            <tbody>
              {balance.ledgers.map((ledger, index) => (<tr key={ledger.ledgerId}>
                <td className="hd-col-no">{index + 1}</td>
                <td>{ledger.name}</td>
                <td className="hd-num">{formatAmount(ledger.amount)}</td>
                <td>{ledger.mode}</td>
              </tr>))}
            </tbody>
            <tfoot><tr><th colSpan={2}>Total</th><td className="hd-num">{formatAmount(balance.amount)}</td><td>{balance.mode}</td></tr></tfoot>
          </table>
        </div>) : null}
      </Modal>
    </section>);
}
