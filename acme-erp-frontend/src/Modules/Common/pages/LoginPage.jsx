import { useState } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { Input, PasswordInput } from '../../../Common/components';
import { appRoutes } from '../../../constants/routes';
import { SESSION_EXPIRED, useAuth } from '../../../Context/AuthContext';
import { takeSignOutNotice } from '../../../utilities/authEvents';
import { errorMessage } from '../../../utilities/errors';

// Login screen: only the sign-in card (Head Office Code, Username, Password).
export function LoginPage() {
    const { session, login } = useAuth();
    const navigate = useNavigate();
    const [headOfficeCode, setHeadOfficeCode] = useState('');
    const [userName, setUserName] = useState('');
    const [password, setPassword] = useState('');
    const [errors, setErrors] = useState({});
    const [submitting, setSubmitting] = useState(false);
    // Why the user landed here (signed out / session expired); read once.
    const [notice, setNotice] = useState(() => takeSignOutNotice());

    if (session) {
        return <Navigate to={appRoutes.home} replace/>;
    }


    async function handleSubmit(event) {
        event.preventDefault();
        const nextErrors = {};
        if (!headOfficeCode.trim()) {
            nextErrors.headOfficeCode = 'Enter the head office code.';
        }
        if (!userName.trim()) {
            nextErrors.userName = 'Enter the username.';
        }
        if (!password) {
            nextErrors.password = 'Enter a password.';
        }
        setErrors(nextErrors);
        if (Object.keys(nextErrors).length > 0) {
            return;
        }
        setSubmitting(true);
        setNotice('');
        try {
            await login({ headOfficeCode, userName, password });
            navigate(appRoutes.home);
        } catch (error) {
            setErrors({ form: errorMessage(error) });
        } finally {
            setSubmitting(false);
        }
    }

    return (<main className="login-plain" aria-label="Sign in">
      <form className="login-plain-form" onSubmit={(event) => void handleSubmit(event)} noValidate>
        {notice && !errors.form ? <p className={notice === SESSION_EXPIRED ? 'login-notice is-warning' : 'login-notice'} role="status">{notice}</p> : null}
        {errors.form ? <p className="form-error" role="alert">{errors.form}</p> : null}
        <Input id="head-office-code" label="Head Office Code" required value={headOfficeCode} disabled={submitting} autoComplete="organization" error={errors.headOfficeCode} onChange={(next) => {
            setHeadOfficeCode(next);
            setErrors((current) => ({ ...current, headOfficeCode: undefined, form: undefined }));
        }}/>
        <Input id="username" label="Username" required value={userName} disabled={submitting} autoComplete="username" error={errors.userName} onChange={(next) => {
            setUserName(next);
            setErrors((current) => ({ ...current, userName: undefined, form: undefined }));
        }}/>
        <PasswordInput id="password" label="Password" required value={password} disabled={submitting} error={errors.password} onChange={(next) => {
            setPassword(next);
            setErrors((current) => ({ ...current, password: undefined, form: undefined }));
        }}/>
        <button className="login-plain-submit" type="submit" disabled={submitting}>
          {submitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </main>);
}
