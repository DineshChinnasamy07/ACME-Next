import { useState } from 'react';
import { PasswordInput } from '../../../Common/components';
import { useAuth } from '../../../Context/AuthContext';
import { useToast } from '../../../Context/ToastContext';
import { errorMessage } from '../../../utilities/errors';
import { passwordApi } from '../services/userAdminApi';

// Same rule as the backend: 8+ characters with a letter, a number and a symbol.
const passwordRule = /^(?=.*[A-Za-z])(?=.*\d)(?=.*[$@!%^()\-*#?&])[A-Za-z\d$@!%^()\-*#?&]{8,}$/;
const empty = { currentPassword: '', newPassword: '', confirmPassword: '' };

// forced = shown right after login when the backend says passwordChangeRequired.
export function ChangePasswordPage({ forced = false }) {
    const { session, markPasswordChanged } = useAuth();
    const { pushToast } = useToast();
    const [form, setForm] = useState(empty);
    const [errors, setErrors] = useState({});
    const [saving, setSaving] = useState(false);

    function set(field) {
        return (value) => {
            setForm((current) => ({ ...current, [field]: value }));
            setErrors((current) => ({ ...current, [field]: undefined, form: undefined }));
        };
    }

    async function save(event) {
        event.preventDefault();
        const next = {};
        if (!form.currentPassword) next.currentPassword = 'Enter your current password.';
        if (!form.newPassword) next.newPassword = 'Enter a new password.';
        else if (!passwordRule.test(form.newPassword)) next.newPassword = 'At least 8 characters with a letter, a number and a symbol.';
        if (form.confirmPassword !== form.newPassword) next.confirmPassword = 'Passwords do not match.';
        setErrors(next);
        if (Object.keys(next).length > 0) {
            return;
        }
        setSaving(true);
        try {
            const result = await passwordApi.changePassword(session, form.currentPassword, form.newPassword, form.confirmPassword);
            pushToast({ message: (result && result.message) || 'Password changed.' });
            setForm(empty);
            if (forced || session.passwordChangeRequired) {
                markPasswordChanged();
            }
        }
        catch (error) {
            setErrors({ form: errorMessage(error) });
        }
        finally {
            setSaving(false);
        }
    }

    return (<section className="lg-page" aria-label="Change Password">
      <div className="lg-bar">
        <h1 className="lg-title">Change Password</h1>
      </div>
      <form className="lg-password" noValidate onSubmit={(event) => void save(event)}>
        {errors.form ? <p className="lg-form-error" role="alert">{errors.form}</p> : null}
        <PasswordInput id="cp-current" label="Current Password" required value={form.currentPassword} error={errors.currentPassword} onChange={set('currentPassword')}/>
        <PasswordInput id="cp-new" label="New Password" required autoComplete="new-password" value={form.newPassword} error={errors.newPassword} onChange={set('newPassword')}/>
        <PasswordInput id="cp-confirm" label="Confirm Password" required autoComplete="new-password" value={form.confirmPassword} error={errors.confirmPassword} onChange={set('confirmPassword')}/>
        <p className="lg-hint">Use at least 8 characters with a letter, a number and a symbol.</p>
        <div className="lg-form-actions">
          <button type="submit" className="lg-btn is-primary" disabled={saving}>{saving ? 'Saving…' : 'Change Password'}</button>
          <button type="button" className="lg-btn" disabled={saving} onClick={() => { setForm(empty); setErrors({}); }}>Clear</button>
        </div>
      </form>
    </section>);
}
