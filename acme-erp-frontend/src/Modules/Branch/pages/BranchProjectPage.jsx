/*  Component       : BranchProjectPage
 *  Purpose         : Branch login › Finance › Masters › Project.
 *                    Read-only list of the projects mapped to the logged-in branch.
 *                    The backend (GET api/projects) scopes the result to the branch via the
 *                    BRANCH_CODE claim, so this page only shows that branch's mapped projects.
 *                    Branch users cannot create / edit / delete masters, so there are no actions.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../../../Context/AuthContext';
import { errorMessage } from '../../../utilities/errors';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { projectApi, toGridDate } from '../../Data/services/projectApi';

const columns = [
    { key: 'projectCode', header: 'Code', width: '90px' },
    { key: 'projectName', header: 'Project Name', wrap: true },
    { key: 'society', header: 'Society', wrap: true },
    { key: 'category', header: 'Category', width: '160px', wrap: true },
    { key: 'division', header: 'Division', width: '110px' },
    { key: 'dateStarted', header: 'Start Date', width: '100px', render: (row) => toGridDate(row.dateStarted) },
    { key: 'dateClosed', header: 'End Date', width: '100px', render: (row) => toGridDate(row.dateClosed) },
];

export function BranchProjectPage() {
    const { session } = useAuth();
    const [rows, setRows] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            setRows(await projectApi.fetchProjects(session));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session]);

    useEffect(() => { void load(); }, [load]);

    return (<section className="lg-page" aria-label="Project">
      <PageBar title="Project" search={search} onSearch={setSearch} onRefresh={() => void load()}/>
      <LegacyGrid
        columns={columns}
        rows={rows}
        getKey={(row) => row.projectId}
        loading={loading}
        error={loadError}
        emptyText="No projects are mapped to this branch yet."
        search={search}
        searchKeys={['projectCode', 'projectName', 'society', 'category']}/>
    </section>);
}
