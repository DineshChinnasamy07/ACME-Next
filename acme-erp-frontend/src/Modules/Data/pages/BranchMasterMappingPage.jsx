/*  Component       : BranchMasterMappingPage
 *  Purpose         : Head Office › Data › Branch Master Mapping. Pick a branch and a master type, then tick
 *                    the master records (ledgers, legal entities, categories, …) that branch should see.
 *                    Saving replaces that branch's mapping for that master type. A branch login then shows
 *                    only the ticked records. Backend: api/mapbranchmasters.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
import { useCallback, useEffect, useMemo, useState } from 'react';
import { SearchableSelect, Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { PageBar } from '../../Common/components/LegacyGrid';
import { BRANCH_MASTER_TYPES, branchMasterApi } from '../services/branchMasterApi';

export function BranchMasterMappingPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [branches, setBranches] = useState([]);
    const [branchId, setBranchId] = useState('');
    const [masterType, setMasterType] = useState(BRANCH_MASTER_TYPES[0].key);
    const [records, setRecords] = useState([]);
    const [checked, setChecked] = useState(() => new Set());
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(false);
    const [loadError, setLoadError] = useState('');
    const [busy, setBusy] = useState(false);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const list = await branchMasterApi.fetchBranches(session);
                if (alive) setBranches(list);
            }
            catch (error) {
                if (alive) pushToast({ tone: 'error', message: errorMessage(error) });
            }
        })();
        return () => { alive = false; };
    }, [session, pushToast]);

    // Load the master records + the branch's current selection whenever branch or type changes.
    const load = useCallback(async () => {
        if (!branchId) {
            setRecords([]);
            setChecked(new Set());
            return;
        }
        setLoading(true);
        setLoadError('');
        try {
            const [rows, mapped] = await Promise.all([
                branchMasterApi.fetchRecords(session, masterType),
                branchMasterApi.fetchMappedIds(session, masterType, branchId),
            ]);
            setRecords(rows);
            setChecked(new Set(mapped.map((id) => Number(id))));
        }
        catch (error) {
            setLoadError(errorMessage(error));
            setRecords([]);
            setChecked(new Set());
        }
        finally {
            setLoading(false);
        }
    }, [session, branchId, masterType]);

    useEffect(() => { void load(); }, [load]);

    const branchOptions = useMemo(
        () => branches.map((branch) => ({ value: String(branch.id), label: branch.name })),
        [branches],
    );
    const typeOptions = useMemo(
        () => BRANCH_MASTER_TYPES.map((type) => ({ value: type.key, label: type.label })),
        [],
    );

    const term = search.trim().toLowerCase();
    const visible = useMemo(() => {
        if (!term) return records;
        return records.filter((row) => `${row.code || ''} ${row.name || ''}`.toLowerCase().includes(term));
    }, [records, term]);

    const toggle = (id) => {
        setChecked((prev) => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id); else next.add(id);
            return next;
        });
    };
    const selectAllVisible = () => {
        setChecked((prev) => {
            const next = new Set(prev);
            visible.forEach((row) => next.add(row.id));
            return next;
        });
    };
    const clearVisible = () => {
        setChecked((prev) => {
            const next = new Set(prev);
            visible.forEach((row) => next.delete(row.id));
            return next;
        });
    };

    const save = async () => {
        if (!branchId) {
            pushToast({ tone: 'error', message: 'Select a branch first.' });
            return;
        }
        setBusy(true);
        try {
            const result = await branchMasterApi.saveMapping(session, {
                branchId: Number(branchId),
                masterType,
                masterIds: Array.from(checked),
            });
            pushToast({ tone: 'success', message: (result && result.message) || 'Mapping saved.' });
        }
        catch (error) {
            pushToast({ tone: 'error', message: errorMessage(error) });
        }
        finally {
            setBusy(false);
        }
    };

    const typeLabel = (BRANCH_MASTER_TYPES.find((type) => type.key === masterType) || {}).label || 'records';

    return (<section className="lg-page bm-page" aria-label="Branch Master Mapping">
      <PageBar title="Branch Master Mapping"/>
      <p className="bm-intro">Map head office master records to a branch. The branch login shows only the records you tick here.</p>

      <div className="bm-filters">
        <div className="bm-filter"><SearchableSelect id="bm-branch" label="Branch" value={branchId} options={branchOptions} placeholder="Select a branch" onChange={setBranchId}/></div>
        <div className="bm-filter"><Select id="bm-type" label="Master" value={masterType} options={typeOptions} hidePlaceholder onChange={setMasterType}/></div>
      </div>

      {!branchId
        ? <p className="bm-empty">Select a branch to begin.</p>
        : (<div className="bm-list-box">
            <div className="bm-list-head">
              <input type="search" className="control bm-search" placeholder={`Search ${typeLabel.toLowerCase()}`} value={search} onChange={(event) => setSearch(event.target.value)}/>
              <span className="bm-count">{checked.size} selected</span>
              <button type="button" className="bm-link" onClick={selectAllVisible} disabled={loading || !visible.length}>Select all</button>
              <button type="button" className="bm-link" onClick={clearVisible} disabled={loading || !visible.length}>Clear</button>
              <button type="button" className="btn btn-primary bm-save" onClick={() => void save()} disabled={busy || loading}>{busy ? 'Saving…' : 'Save Mapping'}</button>
            </div>
            {loadError ? <p className="bm-error" role="alert">{loadError}</p> : null}
            {loading
              ? <p className="bm-empty">Loading…</p>
              : visible.length === 0
                ? <p className="bm-empty">No {typeLabel.toLowerCase()} records{term ? ' match the search' : ''}.</p>
                : (<ul className="bm-list">
                    {visible.map((row) => (<li key={row.id}>
                      <label className="bm-item">
                        <input type="checkbox" checked={checked.has(row.id)} onChange={() => toggle(row.id)}/>
                        {row.code ? <span className="bm-item-code">{row.code}</span> : null}
                        <span className="bm-item-name">{row.name}</span>
                      </label>
                    </li>))}
                  </ul>)}
          </div>)}
    </section>);
}
