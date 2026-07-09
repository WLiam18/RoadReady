import { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import AppShell from '../components/AppShell';
import { Field, TextInput, PasswordInput } from '../components/FormControls';
import { UserIcon, LockIcon } from '../components/icons';
import { useAuth } from '../context/AuthContext';
import { validatePhone, validateName } from '../lib/validators';
import ApiV1 from '../lib/apiV1';

export default function ProfilePage() {
  const { user, refreshProfile } = useAuth();

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [profileErrors, setProfileErrors] = useState({});
  const [profileBusy, setProfileBusy] = useState(false);

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordErrors, setPasswordErrors] = useState({});
  const [passwordBusy, setPasswordBusy] = useState(false);

  useEffect(() => {
    if (user) {
      setFirstName(user.firstName || '');
      setLastName(user.lastName || '');
      setPhone(user.phoneNumber || '');
    }
  }, [user]);

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    const errs = {};
    errs.firstName = validateName(firstName, 'First name');
    errs.lastName = validateName(lastName, 'Last name');
    errs.phone = validatePhone(phone);
    setProfileErrors(errs);
    if (Object.values(errs).some(Boolean)) return;

    setProfileBusy(true);
    try {
      const res = await ApiV1.updateProfile({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phoneNumber: phone.trim(),
      });
      if (res.data?.success) {
        toast.success('Profile updated!');
        await refreshProfile();
      } else {
        toast.error(res.data?.message || 'Failed to update profile.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || err?.message || 'Failed to update profile.');
    } finally {
      setProfileBusy(false);
    }
  };

  const validatePasswords = () => {
    const errs = {};
    if (!currentPassword) errs.currentPassword = 'Current password is required.';
    if (!newPassword || newPassword.length < 8) errs.newPassword = 'Must be at least 8 characters.';
    if (newPassword !== confirmPassword) errs.confirmPassword = 'Passwords do not match.';
    setPasswordErrors(errs);
    return !Object.values(errs).some(Boolean);
  };

  const handlePasswordUpdate = async (e) => {
    e.preventDefault();
    if (!validatePasswords()) return;
    setPasswordBusy(true);
    try {
      const res = await ApiV1.updatePassword({ currentPassword, newPassword });
      if (res.data?.success) {
        toast.success('Password updated!');
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
      } else {
        toast.error(res.data?.message || 'Failed to update password.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || err?.message || 'Failed to update password.');
    } finally {
      setPasswordBusy(false);
    }
  };

  return (
    <AppShell>
      <div className="max-w-2xl mx-auto space-y-6">
        <h1 className="text-2xl font-bold text-brand-ink">My profile</h1>

        <div className="card p-6 flex items-center gap-4">
          <div className="w-16 h-16 rounded-full bg-brand-ink text-white flex items-center justify-center text-2xl font-bold uppercase">
            {user?.firstName?.[0] || user?.email?.[0] || 'A'}
          </div>
          <div>
            <p className="font-bold text-lg text-brand-ink">{user?.firstName} {user?.lastName}</p>
            <p className="text-sm text-brand-muted">{user?.email}</p>
            <span className="badge-info mt-1">{user?.role}</span>
          </div>
        </div>

        <form onSubmit={handleProfileUpdate} className="card p-6">
          <h2 className="font-semibold mb-4 flex items-center gap-2"><UserIcon className="w-4 h-4" /> Edit profile</h2>
          <div className="grid grid-cols-2 gap-4">
            <Field label="First name" error={profileErrors.firstName} required>
              <TextInput value={firstName} onChange={(e) => setFirstName(e.target.value)} error={profileErrors.firstName} />
            </Field>
            <Field label="Last name" error={profileErrors.lastName} required>
              <TextInput value={lastName} onChange={(e) => setLastName(e.target.value)} error={profileErrors.lastName} />
            </Field>
          </div>
          <Field label="Email" hint="Email change is not supported yet" required>
            <TextInput value={user?.email || ''} disabled />
          </Field>
          <Field label="Phone number" error={profileErrors.phone} required>
            <TextInput value={phone} onChange={(e) => setPhone(e.target.value)} error={profileErrors.phone} placeholder="+1 415 555 0199" />
          </Field>
          <button type="submit" className="btn btn-primary" disabled={profileBusy}>
            {profileBusy ? 'Saving…' : 'Save changes'}
          </button>
        </form>

        <form onSubmit={handlePasswordUpdate} className="card p-6">
          <h2 className="font-semibold mb-4 flex items-center gap-2"><LockIcon className="w-4 h-4" /> Change password</h2>
          <Field label="Current password" error={passwordErrors.currentPassword} required>
            <PasswordInput value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} autoComplete="current-password" />
          </Field>
          <Field label="New password" error={passwordErrors.newPassword} required hint="At least 8 characters">
            <PasswordInput value={newPassword} onChange={(e) => setNewPassword(e.target.value)} autoComplete="new-password" name="new-pw" />
          </Field>
          <Field label="Confirm new password" error={passwordErrors.confirmPassword} required>
            <PasswordInput value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} autoComplete="new-password" name="confirm-pw" />
          </Field>
          <button type="submit" className="btn btn-primary" disabled={passwordBusy}>
            {passwordBusy ? 'Updating…' : 'Update password'}
          </button>
        </form>
      </div>
    </AppShell>
  );
}
