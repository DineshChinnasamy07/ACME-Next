import { useMemo, useState } from 'react';
import { DeleteIcon, EditIcon, PlusIcon, RefreshIcon } from '../../../Common/components/Icons';

function PowerIcon() {
    return (<svg className="icon" viewBox="0 0 24 24" aria-hidden="true">
      <path fill="currentColor" d="M11 3h2v10h-2V3zm6.07 2.93 1.41-1.41A9 9 0 1 1 5.52 4.52l1.41 1.41a7 7 0 1 0 10.14 0z"/>
    </svg>);
}

// Title bar for a list screen: title, Add, Refresh and a search box.
export function PageBar({ title, addLabel, onAdd, onRefresh, search, onSearch, busy = false }) {
    return (<div className="lg-bar">
      <h1 className="lg-title">{title}</h1>
      <div className="lg-bar-tools">
        {onSearch ? <input className="lg-search" type="search" placeholder="Search" aria-label={`Search ${title}`} value={search} onChange={(event) => onSearch(event.target.value)}/> : null}
        {onRefresh ? <button type="button" className="lg-tool" title="Refresh" aria-label="Refresh" disabled={busy} onClick={onRefresh}><RefreshIcon /></button> : null}
        {onAdd ? <button type="button" className="lg-add" disabled={busy} onClick={onAdd}><PlusIcon />{addLabel || 'Add'}</button> : null}
      </div>
    </div>);
}

// Status text with a colour per value (Active/Activated green, Created blue, others grey).
export function StatusText({ value }) {
    const text = String(value || '').toLowerCase();
    const tone = text === 'active' || text === 'activated' ? 'is-good' : text === 'created' ? 'is-info' : 'is-off';
    return <span className={`lg-status ${tone}`}>{value || '—'}</span>;
}

/*
 * Compact grid with Edit / Status / Delete icons on every row.
 * columns: [{ key, header, render?(row), width? }]
 * rowActions(row) → { onEdit, onToggle, toggleTitle, onDelete, deleteDisabled, deleteTitle }
 */
export function LegacyGrid({ columns, rows, getKey, loading = false, error = '', emptyText = 'No records to display.', search = '', searchKeys = [], rowActions }) {
    const [page, setPage] = useState(0);
    const pageSize = 15;

    const filtered = useMemo(() => {
        const term = search.trim().toLowerCase();
        if (!term) {
            return rows;
        }
        return rows.filter((row) => searchKeys.some((key) => String(row[key] ?? '').toLowerCase().includes(term)));
    }, [rows, search, searchKeys]);

    const pageCount = Math.max(1, Math.ceil(filtered.length / pageSize));
    const safePage = Math.min(page, pageCount - 1);
    const visible = filtered.slice(safePage * pageSize, safePage * pageSize + pageSize);
    const colSpan = columns.length + 2;

    return (<div className="lg-grid-box">
      <div className="lg-grid-wrap">
      <table className="lg-grid">
        <thead>
          <tr>
            <th className="lg-sno" scope="col">S.No</th>
            {columns.map((column) => <th key={column.key} scope="col" style={column.width ? { width: column.width } : undefined}>{column.header}</th>)}
            <th className="lg-actions" scope="col">Actions</th>
          </tr>
        </thead>
        <tbody>
          {loading ? <tr><td colSpan={colSpan} className="lg-note">Loading…</td></tr> : null}
          {!loading && error ? <tr><td colSpan={colSpan} className="lg-note is-error">{error}</td></tr> : null}
          {!loading && !error && visible.length === 0 ? <tr><td colSpan={colSpan} className="lg-note">{emptyText}</td></tr> : null}
          {!loading && !error ? visible.map((row, index) => {
              const actions = rowActions ? rowActions(row) : {};
              return (<tr key={getKey(row)}>
                <td className="lg-sno">{safePage * pageSize + index + 1}</td>
                {columns.map((column) => <td key={column.key} className={column.wrap ? 'lg-wrap' : undefined}>{column.render ? column.render(row) : (row[column.key] ?? '')}</td>)}
                <td className="lg-actions">
                  {actions.onEdit ? <button type="button" className="lg-icon is-edit" title="Edit" aria-label="Edit" onClick={actions.onEdit}><EditIcon /></button> : null}
                  {actions.onToggle ? <button type="button" className={actions.toggleTitle === 'Deactivate' ? 'lg-icon is-toggle is-off-action' : 'lg-icon is-toggle is-on-action'} title={actions.toggleTitle} aria-label={actions.toggleTitle} disabled={actions.toggleDisabled} onClick={actions.onToggle}><PowerIcon /></button> : null}
                  {actions.onDelete ? <button type="button" className="lg-icon is-delete" title={actions.deleteTitle || 'Delete'} aria-label="Delete" disabled={actions.deleteDisabled} onClick={actions.onDelete}><DeleteIcon /></button> : null}
                </td>
              </tr>);
          }) : null}
        </tbody>
      </table>
      </div>
      <div className="lg-grid-foot">
      {!loading && !error ? <p className="lg-count">{filtered.length} record{filtered.length === 1 ? '' : 's'}</p> : <span/>}
      {!loading && filtered.length > pageSize ? <div className="lg-pager">
        <button type="button" disabled={safePage === 0} onClick={() => setPage(safePage - 1)}>‹ Prev</button>
        <span>Page {safePage + 1} of {pageCount}</span>
        <button type="button" disabled={safePage >= pageCount - 1} onClick={() => setPage(safePage + 1)}>Next ›</button>
      </div> : null}
      </div>
    </div>);
}
