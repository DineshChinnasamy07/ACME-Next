/*  Component       : BranchMasterListPage (+ per-master wrappers)
 *  Purpose         : Branch login › Finance › Masters list, read-only. Shows the records of one master
 *                    type that Head Office mapped to this branch (api/mapbranchmasters/branch/{type}).
 *                    Branch users cannot edit masters, so there are no actions.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../../../Context/AuthContext';
import { errorMessage } from '../../../utilities/errors';
import { LegacyGrid, PageBar } from '../../Common/components/LegacyGrid';
import { branchMasterApi } from '../../Data/services/branchMasterApi';

export function BranchMasterListPage({ masterType, title, codeHeader, showCode = true }) {
    const { session } = useAuth();
    const [rows, setRows] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [search, setSearch] = useState('');

    const load = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            setRows(await branchMasterApi.fetchBranchMasters(session, masterType));
        }
        catch (error) {
            setLoadError(errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }, [session, masterType]);

    useEffect(() => { void load(); }, [load]);

    const columns = showCode
        ? [{ key: 'code', header: codeHeader || 'Code', width: '140px' }, { key: 'name', header: title, wrap: true }]
        : [{ key: 'name', header: title, wrap: true }];

    return (<section className="lg-page" aria-label={title}>
      <PageBar title={title} search={search} onSearch={setSearch} onRefresh={() => void load()}/>
      <LegacyGrid
        columns={columns}
        rows={rows}
        getKey={(row) => row.id}
        loading={loading}
        error={loadError}
        emptyText={`No ${title.toLowerCase()} records are mapped to this branch yet.`}
        search={search}
        searchKeys={['code', 'name']}/>
    </section>);
}

export function BranchLedgerPage() {
    return <BranchMasterListPage masterType="ledger" title="Ledger"/>;
}

export function BranchLedgerGroupPage() {
    return <BranchMasterListPage masterType="ledgergroup" title="Ledger Group"/>;
}

export function BranchProjectCategoryPage() {
    return <BranchMasterListPage masterType="projectcategory" title="Project Category" showCode={false}/>;
}

export function BranchLegalEntityPage() {
    return <BranchMasterListPage masterType="legalentity" title="Legal Entity" codeHeader="Reg. No"/>;
}

export function BranchPurposePage() {
    return <BranchMasterListPage masterType="fcpurpose" title="Purpose"/>;
}
