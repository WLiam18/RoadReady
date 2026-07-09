import { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { PlusIcon, EditIcon, TrashIcon } from '../../components/icons';
import { TableSkeleton } from '../../components/Skeleton';
import ApiV1 from '../../lib/apiV1';
import { resolveAssetUrl } from '../../lib/api';

const EMPTY = {
  make: '', model: '', year: new Date().getFullYear(), color: '', licensePlate: '',
  location: '', pricePerDay: '', transmission: '', fuelType: '', seatingCapacity: 4,
  description: '', brandId: '', status: 'Available', imageUrls: []
};

export default function AdminCarsPage() {
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({ ...EMPTY });
  const [cars, setCars] = useState([]);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);

  const fetchCars = () => {
    setLoading(true);
    ApiV1.getAllCars()
      .then((res) => {
        if (res.data?.success) setCars(res.data.data || []);
      })
      .catch(() => toast.error('Failed to load cars.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchCars();
    ApiV1.getBrands().then((res) => {
      if (res.data?.success) setBrands(res.data.data || []);
    }).catch(() => {});
  }, []);

  const handleImageUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      const res = await ApiV1.uploadCarImage(file);
      if (res.data?.success) {
        setForm((v) => ({ ...v, imageUrls: [...(v.imageUrls || []), res.data.data] }));
        toast.success('Image uploaded!');
      } else {
        toast.error(res.data?.message || 'Upload failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Upload failed.');
    } finally {
      setUploading(false);
      e.target.value = '';
    }
  };

  const removeImage = (index) => {
    setForm((v) => ({ ...v, imageUrls: (v.imageUrls || []).filter((_, i) => i !== index) }));
  };

  const openCreate = () => { setEditing(null); setForm({ ...EMPTY }); setShowModal(true); };

  const openEdit = (c) => {
    setEditing(c);
    setForm({
      make: c.make, model: c.model, year: c.year, color: c.color || '',
      licensePlate: c.licensePlate, location: c.location,
      pricePerDay: c.pricePerDay, transmission: c.transmission, fuelType: c.fuelType,
      seatingCapacity: c.seatingCapacity || 4, description: c.description || '',
      brandId: c.brandId, status: c.status, imageUrls: c.imageUrls || []
    });
    setShowModal(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const body = {
        ...form,
        pricePerDay: Number(form.pricePerDay),
        year: Number(form.year),
        seatingCapacity: Number(form.seatingCapacity),
        brandId: Number(form.brandId),
      };
      const res = editing
        ? await ApiV1.updateCar(editing.id, body)
        : await ApiV1.createCar(body);
      if (res.data?.success) {
        toast.success(editing ? 'Car updated!' : 'Car created!');
        setShowModal(false);
        fetchCars();
      } else {
        toast.error(res.data?.message || 'Failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Failed.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id, name) => {
    if (!window.confirm(`Delete "${name}"?`)) return;
    try {
      const res = await ApiV1.deleteCar(id);
      if (res.data?.success) {
        toast.success('Deleted.');
        fetchCars();
      } else {
        toast.error(res.data?.message || 'Delete failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Delete failed.');
    }
  };

  return (
    <AppShell>
      <div className="max-w-5xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold text-brand-ink">Cars</h1>
          <button onClick={openCreate} className="btn btn-primary btn-sm"><PlusIcon className="w-4 h-4" /> Add car</button>
        </div>
        {loading ? <TableSkeleton rows={6} /> : (
          <div className="card overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead><tr className="border-b border-gray-100">
                  <th className="text-left px-5 py-3 font-medium text-brand-muted">Car</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden md:table-cell">Brand</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden lg:table-cell">Location</th>
                  <th className="text-right px-5 py-3 font-medium text-brand-muted">Price</th>
                  <th className="text-center px-5 py-3 font-medium text-brand-muted">Status</th>
                  <th className="text-right px-5 py-3 font-medium text-brand-muted">Actions</th>
                </tr></thead>
                <tbody className="divide-y divide-gray-50">
                  {cars.length === 0 ? (
                    <tr><td colSpan={6} className="p-8 text-center text-brand-muted">No cars yet.</td></tr>
                  ) : cars.map((c) => (
                    <tr key={c.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-5 py-3">
                        <div className="flex items-center gap-3 min-w-0">
                          {Array.isArray(c.imageUrls) && c.imageUrls.length > 0 ? (
                            <img
                              src={resolveAssetUrl(c.imageUrls[0])}
                              alt={`${c.make} ${c.model}`}
                              className="w-14 h-10 rounded-md object-cover border flex-shrink-0 bg-gray-100"
                              loading="lazy"
                            />
                          ) : (
                            <div className="w-14 h-10 rounded-md bg-gray-100 border flex-shrink-0 flex items-center justify-center text-[10px] text-brand-muted">
                              no img
                            </div>
                          )}
                          <div className="min-w-0">
                            <p className="font-medium truncate">{c.make} {c.model}</p>
                            <p className="text-xs text-brand-muted">{c.year} · {c.transmission}</p>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3 text-brand-muted hidden md:table-cell">{c.brandName || '—'}</td>
                      <td className="px-5 py-3 text-brand-muted hidden lg:table-cell">{c.location || '—'}</td>
                      <td className="px-5 py-3 text-right font-medium">₹{c.pricePerDay?.toLocaleString()}/d</td>
                      <td className="px-5 py-3 text-center"><span className={c.status === 'Available' ? 'badge-success' : c.status === 'Rented' ? 'badge-info' : c.status === 'UnderMaintenance' ? 'badge-warning' : 'badge-neutral'}>{c.status}</span></td>
                      <td className="px-5 py-3 text-right space-x-1">
                        <button onClick={() => openEdit(c)} className="btn-ghost p-1.5"><EditIcon className="w-4 h-4" /></button>
                        <button onClick={() => handleDelete(c.id, `${c.make} ${c.model}`)} className="btn-ghost p-1.5 text-red-500"><TrashIcon className="w-4 h-4" /></button>
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
          <div className="bg-white rounded-xl shadow-prominent p-6 w-full max-w-lg" onClick={(e) => e.stopPropagation()}>
            <h2 className="font-semibold text-lg mb-4">{editing ? 'Edit car' : 'Add car'}</h2>
            <form onSubmit={handleSave} className="space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div><label className="label">Make</label><input className="input" value={form.make} onChange={(e) => setForm(v => ({ ...v, make: e.target.value }))} required /></div>
                <div><label className="label">Model</label><input className="input" value={form.model} onChange={(e) => setForm(v => ({ ...v, model: e.target.value }))} required /></div>
              </div>
              <div className="grid grid-cols-3 gap-3">
                <div><label className="label">Year</label><input className="input" type="number" value={form.year} onChange={(e) => setForm(v => ({ ...v, year: e.target.value }))} /></div>
                <div><label className="label">Color</label><input className="input" value={form.color} onChange={(e) => setForm(v => ({ ...v, color: e.target.value }))} /></div>
                <div><label className="label">Seats</label><input className="input" type="number" value={form.seatingCapacity} onChange={(e) => setForm(v => ({ ...v, seatingCapacity: e.target.value }))} /></div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div><label className="label">Transmission</label><select className="input" value={form.transmission} onChange={(e) => setForm(v => ({ ...v, transmission: e.target.value }))}><option value="">Select</option><option>Automatic</option><option>Manual</option></select></div>
                <div><label className="label">Fuel</label><select className="input" value={form.fuelType} onChange={(e) => setForm(v => ({ ...v, fuelType: e.target.value }))}><option value="">Select</option><option>Petrol</option><option>Diesel</option><option>Electric</option><option>Hybrid</option></select></div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div><label className="label">Brand</label><select className="input" value={form.brandId} onChange={(e) => setForm(v => ({ ...v, brandId: e.target.value }))}><option value="">Select</option>{brands.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}</select></div>
                <div><label className="label">Price/day</label><input className="input" type="number" value={form.pricePerDay} onChange={(e) => setForm(v => ({ ...v, pricePerDay: e.target.value }))} /></div>
              </div>
              <div><label className="label">License plate</label><input className="input" value={form.licensePlate} onChange={(e) => setForm(v => ({ ...v, licensePlate: e.target.value }))} /></div>
              <div><label className="label">Location</label><input className="input" value={form.location} onChange={(e) => setForm(v => ({ ...v, location: e.target.value }))} /></div>
              <div><label className="label">Status</label><select className="input" value={form.status} onChange={(e) => setForm(v => ({ ...v, status: e.target.value }))}><option>Available</option><option>Rented</option><option>UnderMaintenance</option><option>Inactive</option></select></div>
              <div><label className="label">Description</label><textarea className="input resize-none" rows={2} value={form.description} onChange={(e) => setForm(v => ({ ...v, description: e.target.value }))} /></div>
              <div>
                <label className="label">Images</label>
                <div className="flex items-center gap-3">
                  <input type="file" accept="image/*" onChange={handleImageUpload} className="text-sm" disabled={uploading} />
                  {uploading && <span className="text-xs text-brand-muted">Uploading…</span>}
                </div>
                {(form.imageUrls || []).length > 0 && (
                  <div className="flex flex-wrap gap-2 mt-2">
                    {(form.imageUrls || []).map((url, i) => (
                      <div key={i} className="relative group">
                        <img src={url} alt="" className="w-16 h-14 rounded-lg object-cover border" />
                        <button type="button" onClick={() => removeImage(i)} className="absolute -top-1.5 -right-1.5 w-5 h-5 bg-red-500 text-white rounded-full text-xs flex items-center justify-center opacity-0 group-hover:opacity-100 transition">&times;</button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
              <div className="flex gap-3 pt-2"><button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Saving…' : 'Save'}</button><button type="button" onClick={() => setShowModal(false)} className="btn btn-outline">Cancel</button></div>
            </form>
          </div>
        </div>
      )}
    </AppShell>
  );
}
