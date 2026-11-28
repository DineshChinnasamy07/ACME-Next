import { useAuth } from '../../../Context/AuthContext';
import { HeadOfficeDashboardPage } from '../../Dashboard/pages/HeadOfficeDashboardPage';
import { isBranchSession } from '../services/apiRequest';

// Portal user = head office code empty or "portal" (same rule as the backend HEAD_OFFICE_CODE claim).
function isPortalUser(session) {
    const code = session && session.headOfficeId ? String(session.headOfficeId).trim() : '';
    return code === '' || code.toLowerCase() === 'portal';
}

// Landing page after sign-in.
//   Admin portal users: blank page with the "Admin Portal" label.
//   Head office users: the Head Office dashboard (legacy HomeLogin / HomeDashboard).
//   Branch office users: blank for now (the branch dashboard is not built yet).
export function BlankPage() {
    const { session } = useAuth();

    if (session && !isPortalUser(session) && !isBranchSession(session)) {
        return <HeadOfficeDashboardPage/>;
    }

    return (<main className="blank-page">
      {isPortalUser(session) ? <h1 className="blank-page-label">Admin Portal</h1> : null}
    </main>);
}
