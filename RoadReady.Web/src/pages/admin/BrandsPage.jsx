import { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { PlusIcon, EditIcon, TrashIcon } from '../../components/icons';
import { TableSkeleton } from '../../components/Skeleton';
import ApiV1 from '../../lib/apiV1';

export default function AdminBrandsPage() {
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({ name: '', logoUrl: '', country: '' });
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const fetchBrands = () => {
    setLoading(true);
    ApiV1.getBrands()
      .then((res) => {
        if (res.data?.success) setBrands(res.data.data || []);
      })
      .catch(() => toast.error('Failed to load brands.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchBrands(); }, []);

  const openCreate = () => { setEditing(null); setForm({ name: '', logoUrl: '', country: '' }); setShowModal(true); };
  const openEdit = (b) => { setEditing(b); setForm({ name: b.name, logoUrl: b.logoUrl || '', country: b.country || '' }); setShowModal(true); };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.name.trim()) return toast.error('Name is required.');
    setSaving(true);
    try {
      const payload = { name: form.name.trim(), country: form.country.trim() || null, logoUrl: form.logoUrl.trim() || null };
      const res = editing
        ? await ApiV1.updateBrand(editing.id, payload)
        : await ApiV1.createBrand(payload);
      if (res.data?.success) {
        toast.success(editing ? 'Updated!' : 'Created!');
        setShowModal(false);
        fetchBrands();
      } else {
        toast.error(res.data?.message || 'Operation failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Operation failed.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id, name) => {
    if (!window.confirm(`Delete "${name}"?`)) return;
    try {
      const res = await ApiV1.deleteBrand(id);
      if (res.data?.success) {
        toast.success('Deleted.');
        fetchBrands();
      } else {
        toast.error(res.data?.message || 'Delete failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Delete failed.');
    }
  };

  return (
    <AppShell>
      <div className="max-w-3xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold text-brand-ink">Brands</h1>
          <button onClick={openCreate} className="btn btn-primary btn-sm"><PlusIcon className="w-4 h-4" /> Add brand</button>
        </div>
        {loading ? <TableSkeleton rows={4} /> : (
          <div className="card overflow-hidden">
            {brands.length === 0 ? (
              <p className="p-8 text-center text-brand-muted text-sm">No brands yet.</p>
            ) : (
              <table className="w-full text-sm">
                <thead><tr className="border-b border-gray-100">
                  <th className="text-left px-5 py-3 font-medium text-brand-muted">Name</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden sm:table-cell">Country</th>
                  <th className="text-right px-5 py-3 font-medium text-brand-muted">Actions</th>
                </tr></thead>
                <tbody className="divide-y divide-gray-50">
                  {brands.map((b) => (
                    <tr key={b.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-5 py-3 font-medium">{b.name}</td>
                      <td className="px-5 py-3 text-brand-muted hidden sm:table-cell">{b.country || '—'}</td>
                      <td className="px-5 py-3 text-right space-x-1">
                        <button onClick={() => openEdit(b)} className="btn-ghost p-1.5"><EditIcon className="w-4 h-4" /></button>
                        <button onClick={() => handleDelete(b.id, b.name)} className="btn-ghost p-1.5 text-red-500"><TrashIcon className="w-4 h-4" /></button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>
      {showModal && (
        <div className="fixed inset-0 bg-black/20 z-50 flex items-center justify-center p-4" onClick={() => setShowModal(false)}>
          <div className="bg-white rounded-xl shadow-prominent p-6 w-full max-w-md" onClick={(e) => e.stopPropagation()}>
            <h2 className="font-semibold text-lg mb-4">{editing ? 'Edit brand' : 'Add brand'}</h2>
            <form onSubmit={handleSubmit} className="space-y-3">
              <div><label className="label">Name</label><input className="input" value={form.name} onChange={(e) => setForm((v) => ({ ...v, name: e.target.value }))} required /></div>
              <div><label className="label">Country</label><input className="input" value={form.country} onChange={(e) => setForm((v) => ({ ...v, country: e.target.value }))} /></div>
              <div className="flex gap-3 pt-2"><button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Saving…' : 'Save'}</button><button type="button" onClick={() => setShowModal(false)} className="btn btn-outline">Cancel</button></div>
            </form>
          </div>
        </div>
      )}
    </AppShell>
  );
}
