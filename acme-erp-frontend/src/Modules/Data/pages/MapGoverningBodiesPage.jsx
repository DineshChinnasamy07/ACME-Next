import { useCallback, useEffect, useState } from 'react';
import { SearchableSelect } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { LegacyGrid } from '../../Common/components/LegacyGrid';
import { mapGoverningBodyApi, toGridDate } from '../services/statutoryLegalApi';

/*
 * Data › Statutory Masters › Map Governing Bodies (Head Office). Legacy GoverningMemberMapping.aspx,
 * API api/mapgoverningbodies. Pick a legal entity (society), tick its governing body members and Save:
 * the ticked members replace the entity's mapping (legacy delete + insert in one transaction).
 * Mapped members are listed first, as in the legacy grid (sort "SELECT DESC").
 */

function sameSet(left, right) {
    if (left.size !== right.size) {
        return false;
    }
    for (const item of left) {
        if (!right.has(item)) {
            return false;
        }
    }
    return true;
}

export function MapGoverningBodiesPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [entities, setEntities] = useState([]);
    const [customerId, setCustomerId] = useState('');
    const [members, setMembers] = useState([]);
    const [selected, setSelected] = useState(() => new Set());
    const [saved, setSaved] = useState(() => new Set());
    const [entitiesLoading, setEntitiesLoading] = useState(true);
    const [loading, setLoading] = useState(false);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');
    const [saving, setSaving] = useState(false);

    // Legal entity combo; the first entity is selected, as on the legacy page.
    useEffect(() => {
        let cancelled = false;
        setEntitiesLoading(true);
        mapGoverningBodyApi.fetchLegalEntities(session)
            .then((list) => {
                if (cancelled) return;
                setEntities(list);
                setCustomerId((current) => current || (list.length > 0 ? String(list[0].id) : ''));
            })
            .catch((error) => { if (!cancelled) setLoadError(errorMessage(error)); })
            .finally(() => { if (!cancelled) setEntitiesLoading(false); });
        return () => { cancelled = true; };
    }, [session]);

    const loadMembers = useCallback(async (id) => {
        if (!id) {
            setMembers([]);
            setSelected(new Set());
            setSaved(new Set());
            return;
        }
        setLoading(true);
        setLoadError('');
        try {
            const list = await mapGoverningBodyApi.fetchMembers(session, Number(id));
            const mapped = new Set(list.filter((row) => row.selected).map((row) => row.executiveId));
            setMembers(list.map((row) => ({
                ...row,
                joinText: toGridDate(row.dateOfAppointment),
                exitText: toGridDate(row.dateOfExit),
            })));
            setSelected(new Set(mapped));
            setSaved(mapped);
        }
        catch (error) {
            setMembers([]);
            setSelected(new Set());
            setSaved(new Set());
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void loadMembers(customerId); }, [customerId, loadMembers]);

    function toggle(executiveId, checked) {
        setSelected((current) => {
            const next = new Set(current);
            if (checked) next.add(executiveId);
            else next.delete(executiveId);
            return next;
        });
    }

    function toggleAll(checked) {
        setSelected(checked ? new Set(members.map((row) => row.executiveId)) : new Set());
    }

    async function save() {
        if (!customerId) {
            pushToast({ message: 'Select the legal entity.', tone: 'error' });
            return;
        }
        setSaving(true);
        try {
            const result = await mapGoverningBodyApi.saveMapping(session, {
                customerId: Number(customerId),
                executiveIds: [...selected],
            });
            pushToast({ message: (result && result.message) || 'Governing bodies mapped.' });
            await loadMembers(customerId);
        }
        catch (error) {
            pushToast({ message: errorMessage(error), tone: 'error' });
        }
        finally {
            setSaving(false);
        }
    }

    const entityOptions = entities.map((item) => ({ value: String(item.id), label: item.name }));
    const dirty = !sameSet(selected, saved);
    const ticked = selected.size;
    const allTicked = members.length > 0 && ticked === members.length;

    const columns = [
        {
            key: 'select',
            width: '44px',
            header: (<input type="checkbox" aria-label="Select all members" checked={allTicked} disabled={members.length === 0} ref={(node) => { if (node) node.indeterminate = ticked > 0 && !allTicked; }} onChange={(event) => toggleAll(event.target.checked)}/>),
            render: (row) => (<input type="checkbox" aria-label={`Map ${row.executive}`} checked={selected.has(row.executiveId)} onChange={(event) => toggle(row.executiveId, event.target.checked)}/>),
        },
        { key: 'executive', header: 'Name', wrap: true },
        { key: 'role', header: 'Role' },
        { key: 'joinText', header: 'Date of Joining', width: '110px' },
        { key: 'exitText', header: 'Date of Exit', width: '110px' },
    ];

    return (<section className="lg-page" aria-label="Map Governing Bodies">
      <div className="lg-bar">
        <h1 className="lg-title">Map Governing Bodies</h1>
        <div className="lg-bar-tools">
          <input className="lg-search" type="search" placeholder="Search" aria-label="Search governing body members" value={search} onChange={(event) => setSearch(event.target.value)}/>
        </div>
      </div>
      <div className="lg-rights-head">
        <SearchableSelect id="mgb-entity" label="Society Name" required value={customerId} options={entityOptions} placeholder={entitiesLoading ? 'Loading…' : 'Select legal entity'} disabled={entitiesLoading || saving} onChange={setCustomerId}/>
        <button type="button" className="lg-btn is-primary" disabled={!customerId || !dirty || saving || loading} onClick={() => void save()}>{saving ? 'Saving…' : 'Save'}</button>
      </div>
      {!entitiesLoading && entities.length === 0 && !loadError ? <p className="lg-note">No legal entities are available. Add a legal entity first.</p> : null}
      {customerId ? <p className="lg-hint">{ticked} of {members.length} member{members.length === 1 ? '' : 's'} mapped{dirty ? ' (unsaved changes)' : ''}.</p> : null}
      <LegacyGrid columns={columns} rows={members} getKey={(row) => row.executiveId} loading={loading} error={loadError} emptyText={customerId ? 'No governing body members to display.' : 'Select a legal entity.'} search={search} searchKeys={['executive', 'role', 'joinText', 'exitText']}/>
    </section>);
}
