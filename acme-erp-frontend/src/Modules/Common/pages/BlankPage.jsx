import { useAuth } from '../../../Context/AuthContext';

// Portal user = head office code empty or "portal" (same rule as the backend HEAD_OFFICE_CODE claim).
function isPortalUser(session) {
    const code = session && session.headOfficeId ? String(session.headOfficeId).trim() : '';
    return code === '' || code.toLowerCase() === 'portal';
}

// Landing page after sign-in. Part 1: admin portal users see a blank page with the "Admin Portal" label.
export function BlankPage() {
    const { session } = useAuth();

    return (<main className="blank-page">
      {isPortalUser(session) ? <h1 className="blank-page-label">Admin Portal</h1> : null}
    </main>);
}
