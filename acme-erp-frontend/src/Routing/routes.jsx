import { Navigate, Route, Routes } from 'react-router-dom';
import { BlankPage } from '../Modules/Common/pages/BlankPage';
import { LoginPage } from '../Modules/Common/pages/LoginPage';
import { AppShell } from '../Layout/AppShell';
import { appRoutes } from '../constants/routes';
import { useAuth } from '../Context/AuthContext';
import { ProtectedRoute } from './ProtectedRoute';

function FallbackRedirect() {
    const { session } = useAuth();
    return <Navigate to={session ? appRoutes.home : appRoutes.login} replace/>;
}

export function AppRoutes() {
    return (<Routes>
      <Route path={appRoutes.login} element={<LoginPage />}/>
      <Route element={<ProtectedRoute />}>
        <Route path={appRoutes.home} element={<AppShell><BlankPage /></AppShell>}/>
      </Route>
      <Route path="*" element={<FallbackRedirect />}/>
    </Routes>);
}
