import { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { TableSkeleton } from '../../components/Skeleton';
import ApiV1 from '../../lib/apiV1';

export default function AdminUsersPage() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetch = () => ApiV1.getAllUsers().then(r => { if (r.data?.success) setUsers(r.data.data || []); }).catch(() => {}).finally(() => setLoading(false));
  useEffect(() => { fetch(); }, []);

  const toggle = async (u) => {
    try { const r = await ApiV1.updateUserStatus(u.id, { isActive: !u.isActive }); if (r.data?.success) { toast.success(`User ${u.isActive ? 'deactivated' : 'activated'}.`); fetch(); } else toast.error(r.data?.message); } catch { toast.error('Failed.'); }
  };

  return (
    <AppShell>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-brand-ink mb-6">Users</h1>
        {loading ? <TableSkeleton rows={6} /> : (
          <div className="card overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead><tr className="border-b border-gray-100">
                  <th className="text-left px-5 py-3 font-medium text-brand-muted">User</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden sm:table-cell">Email</th>
                  <th className="text-center px-5 py-3 font-medium text-brand-muted">Role</th>
                  <th className="text-center px-5 py-3 font-medium text-brand-muted">Status</th>
                  <th className="text-right px-5 py-3 font-medium text-brand-muted">Actions</th>
                </tr></thead>
                <tbody className="divide-y divide-gray-50">
                  {users.length === 0 ? (
                    <tr><td colSpan={5} className="p-8 text-center text-brand-muted">No users yet.</td></tr>
                  ) : users.map(u => (
                    <tr key={u.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-5 py-3">
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-full bg-brand-ink text-white flex items-center justify-center text-xs font-bold uppercase">{u.firstName?.[0] || '?'}</div>
                          <div><p className="font-medium">{u.firstName} {u.lastName}</p><p className="text-xs text-brand-muted">{u.phoneNumber}</p></div>
                        </div>
                      </td>
                      <td className="px-5 py-3 text-brand-muted hidden sm:table-cell">{u.email}</td>
                      <td className="px-5 py-3 text-center"><span className="badge-neutral">{u.role}</span></td>
                      <td className="px-5 py-3 text-center"><span className={u.isActive ? 'badge-success' : 'badge-danger'}>{u.isActive ? 'Active' : 'Inactive'}</span></td>
                      <td className="px-5 py-3 text-right">
                        <button onClick={() => toggle(u)} className={`btn btn-sm ${u.isActive ? 'btn-outline text-red-500 border-red-200 hover:bg-red-50' : 'btn-primary'}`}>{u.isActive ? 'Deactivate' : 'Activate'}</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </AppShell>
  );
}
