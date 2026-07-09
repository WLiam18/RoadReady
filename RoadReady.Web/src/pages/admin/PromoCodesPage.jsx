import { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { PlusIcon, EditIcon, TrashIcon } from '../../components/icons';
import { TableSkeleton } from '../../components/Skeleton';
import ApiV1 from '../../lib/apiV1';

const EMPTY = { code: '', description: '', discountType: 'Percentage', discountValue: '', minBookingAmount: '', validFrom: '', validUntil: '', maxUses: '', isActive: true };

export default function AdminPromoCodesPage() {
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({ ...EMPTY });
  const [promos, setPromos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const fetchPromos = () => {
    setLoading(true);
    ApiV1.getPromoCodes()
      .then((res) => {
        if (res.data?.success) setPromos(res.data.data || []);
      })
      .catch(() => toast.error('Failed to load promo codes.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchPromos(); }, []);

  const openCreate = () => { setEditing(null); setForm({ ...EMPTY }); setShowModal(true); };
  const openEdit = (p) => {
    setEditing(p);
    setForm({
      code: p.code,
      description: p.description,
      discountType: p.discountType,
      discountValue: p.discountValue,
      minBookingAmount: p.minBookingAmount || '',
      validFrom: p.validFrom?.slice(0, 16) || '',
      validUntil: p.validUntil?.slice(0, 16) || '',
      maxUses: p.maxUses || '',
      isActive: p.isActive,
    });
    setShowModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const body = {
        ...form,
        discountValue: Number(form.discountValue),
        minBookingAmount: form.minBookingAmount ? Number(form.minBookingAmount) : null,
        maxUses: form.maxUses ? Number(form.maxUses) : null,
        validFrom: form.validFrom ? new Date(form.validFrom).toISOString() : null,
        validUntil: form.validUntil ? new Date(form.validUntil).toISOString() : null,
      };
      const res = editing
        ? await ApiV1.updatePromoCode(editing.id, body)
        : await ApiV1.createPromoCode(body);
      if (res.data?.success) {
        toast.success(editing ? 'Updated!' : 'Created!');
        setShowModal(false);
        fetchPromos();
      } else {
        toast.error(res.data?.message || 'Failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Failed.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id, code) => {
    if (!window.confirm(`Delete "${code}"?`)) return;
    try {
      const res = await ApiV1.deletePromoCode(id);
      if (res.data?.success) {
        toast.success('Deleted.');
        fetchPromos();
      } else {
        toast.error(res.data?.message || 'Failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Failed.');
    }
  };

  return (
    <AppShell>
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold text-brand-ink">Promo codes</h1>
          <button onClick={openCreate} className="btn btn-primary btn-sm"><PlusIcon className="w-4 h-4" /> Add</button>
        </div>
        {loading ? <TableSkeleton rows={4} /> : (
          <div className="card overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead><tr className="border-b border-gray-100">
                  <th className="text-left px-5 py-3 font-medium text-brand-muted">Code</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden md:table-cell">Discount</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden sm:table-cell">Uses</th>
                  <th className="text-center px-5 py-3 font-medium text-brand-muted">Status</th>
                  <th className="text-right px-5 py-3 font-medium text-brand-muted">Actions</th>
                </tr></thead>
                <tbody className="divide-y divide-gray-50">
                  {promos.length === 0 ? <tr><td colSpan={5} className="p-8 text-center text-brand-muted">No promo codes yet.</td></tr>
                  : promos.map(p => (
                    <tr key={p.id} className="hover:bg-gray-50/50">
                      <td className="px-5 py-3 font-mono font-bold uppercase">{p.code}</td>
                      <td className="px-5 py-3 hidden md:table-cell">{p.discountType === 'Percentage' ? `${p.discountValue}%` : `₹${p.discountValue}`}</td>
                      <td className="px-5 py-3 text-brand-muted hidden sm:table-cell">{p.currentUses}{p.maxUses ? `/${p.maxUses}` : ''}</td>
                      <td className="px-5 py-3 text-center"><span className={p.isActive ? 'badge-success' : 'badge-danger'}>{p.isActive ? 'Active' : 'Inactive'}</span></td>
                      <td className="px-5 py-3 text-right space-x-1">
                        <button onClick={() => openEdit(p)} className="btn-ghost p-1.5"><EditIcon className="w-4 h-4" /></button>
                        <button onClick={() => handleDelete(p.id, p.code)} className="btn-ghost p-1.5 text-red-500"><TrashIcon className="w-4 h-4" /></button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
      {showModal && (
        <div className="fixed inset-0 bg-black/20 z-50 flex items-start justify-center p-4 pt-12 overflow-y-auto" onClick={() => setShowModal(false)}>
          <div className="bg-white rounded-xl shadow-prominent p-6 w-full max-w-md" onClick={(e) => e.stopPropagation()}>
            <h2 className="font-semibold text-lg mb-4">{editing ? 'Edit promo' : 'Add promo'}</h2>
            <form onSubmit={handleSubmit} className="space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div><label className="label">Code</label><input className="input uppercase" value={form.code} onChange={e => setForm(v => ({ ...v, code: e.target.value }))} placeholder="SUMMER20" required /></div>
                <div><label className="label">Type</label><select className="input" value={form.discountType} onChange={e => setForm(v => ({ ...v, discountType: e.target.value }))}><option>Percentage</option><option>FlatAmount</option></select></div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div><label className="label">Value</label><input className="input" type="number" value={form.discountValue} onChange={e => setForm(v => ({ ...v, discountValue: e.target.value }))} required /></div>
                <div><label className="label">Min amount</label><input className="input" type="number" value={form.minBookingAmount} onChange={e => setForm(v => ({ ...v, minBookingAmount: e.target.value }))} /></div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div><label className="label">Valid from</label><input className="input" type="datetime-local" value={form.validFrom} onChange={e => setForm(v => ({ ...v, validFrom: e.target.value }))} /></div>
                <div><label className="label">Valid until</label><input className="input" type="datetime-local" value={form.validUntil} onChange={e => setForm(v => ({ ...v, validUntil: e.target.value }))} /></div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div><label className="label">Max uses</label><input className="input" type="number" value={form.maxUses} onChange={e => setForm(v => ({ ...v, maxUses: e.target.value }))} /></div>
                <div><label className="label">Active</label><select className="input" value={String(form.isActive)} onChange={e => setForm(v => ({ ...v, isActive: e.target.value === 'true' }))}><option value="true">Active</option><option value="false">Inactive</option></select></div>
              </div>
              <div className="flex gap-3 pt-2"><button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Saving…' : 'Save'}</button><button type="button" onClick={() => setShowModal(false)} className="btn btn-outline">Cancel</button></div>
            </form>
          </div>
        </div>
      )}
    </AppShell>
  );
}
