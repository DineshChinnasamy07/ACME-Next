import { useEffect, useState } from 'react';
import { Select } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { rightsApi } from '../services/userAdminApi';

// Pick a role, tick the activities it may use per module, then Save.
export function UserRightsPage() {
    const { session } = useAuth();
    const { pushToast } = useToast();
    const [roles, setRoles] = useState([]);
    const [roleId, setRoleId] = useState('');
    const [modules, setModules] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [saving, setSaving] = useState(false);
    const [dirty, setDirty] = useState(false);

    useEffect(() => {
        rightsApi.fetchRoles(session)
            .then(setRoles)
            .catch((loadError) => setError(errorMessage(loadError)));
    }, [session]);

    useEffect(() => {
        if (!roleId) {
            setModules([]);
            return;
        }
        let current = true;
        setLoading(true);
        setError('');
        rightsApi.fetchRights(session, Number(roleId))
            .then((list) => { if (current) { setModules(list); setDirty(false); } })
            .catch((loadError) => { if (current) { setModules([]); setError(errorMessage(loadError)); } })
            .finally(() => { if (current) setLoading(false); });
        return () => { current = false; };
    }, [session, roleId]);

    function setActivity(moduleCode, activityCode, allow) {
        setModules((list) => list.map((module) => module.moduleCode !== moduleCode ? module : {
            ...module,
            activities: module.activities.map((activity) => activity.activityCode === activityCode ? { ...activity, allow } : activity),
        }));
        setDirty(true);
    }

    function setModule(moduleCode, allow) {
        setModules((list) => list.map((module) => module.moduleCode !== moduleCode ? module : {
            ...module,
            activities: module.activities.map((activity) => ({ ...activity, allow })),
        }));
        setDirty(true);
    }

    async function save() {
        const rights = modules.flatMap((module) => module.activities.map((activity) => ({
            moduleCode: module.moduleCode,
            activityCode: activity.activityCode,
            allow: Boolean(activity.allow),
        })));
        setSaving(true);
        try {
            const result = await rightsApi.saveRights(session, Number(roleId), rights);
            pushToast({ message: (result && result.message) || 'Rights saved.' });
            setDirty(false);
        }
        catch (saveError) {
            pushToast({ message: errorMessage(saveError), tone: 'error' });
        }
        finally {
            setSaving(false);
        }
    }

    const roleOptions = roles.map((role) => ({ value: String(role.userRoleId), label: role.userRole }));

    return (<section className="lg-page" aria-label="User Rights">
      <div className="lg-bar">
        <h1 className="lg-title">User Rights</h1>
      </div>
      <div className="lg-rights-head">
        <Select id="rights-role" label="Role" value={roleId} options={roleOptions} placeholder="Select role" onChange={setRoleId}/>
        <button type="button" className="lg-btn is-primary" disabled={!roleId || !dirty || saving || loading} onClick={() => void save()}>{saving ? 'Saving…' : 'Save'}</button>
      </div>
      {error ? <p className="lg-form-error" role="alert">{error}</p> : null}
      {!roleId && !error ? <p className="lg-note">Select a role to view and change its rights.</p> : null}
      {loading ? <p className="lg-note">Loading…</p> : null}
      {!loading && roleId && modules.length === 0 && !error ? <p className="lg-note">No rights are defined.</p> : null}
      {!loading ? <div className="lg-rights">
        {modules.map((module) => {
            const allowed = module.activities.filter((activity) => activity.allow).length;
            const all = module.activities.length > 0 && allowed === module.activities.length;
            return (<fieldset key={module.moduleCode} className="lg-rights-module">
              <legend>
                <label className="lg-check">
                  <input type="checkbox" checked={all} ref={(node) => { if (node) node.indeterminate = allowed > 0 && !all; }} onChange={(event) => setModule(module.moduleCode, event.target.checked)}/>
                  <strong>{module.module}</strong>
                </label>
              </legend>
              <div className="lg-rights-list">
                {module.activities.map((activity) => (<label key={activity.activityCode} className="lg-check">
                  <input type="checkbox" checked={Boolean(activity.allow)} onChange={(event) => setActivity(module.moduleCode, activity.activityCode, event.target.checked)}/>
                  {activity.activity}
                </label>))}
              </div>
            </fieldset>);
        })}
      </div> : null}
    </section>);
}
