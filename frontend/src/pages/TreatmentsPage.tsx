import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import {
  doctorsApi,
  patientTreatmentsApi,
  patientsApi,
  treatmentCategoriesApi,
  treatmentsApi,
  visitsApi,
} from '@/api/services'
import { isApiError } from '@/api/client'
import { useAuth, useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatusBadge, Tabs } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { CreatePatientTreatmentRequest, CreateTreatmentRequest, TreatmentDetailDto } from '@/types/api'
import { formatDateTime, fromDateTimeLocal, localDateTimeValue, money } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

export function TreatmentsPage() {
  const { user } = useAuth()
  const isAdmin = useCan([Role.Admin])
  const canClinical = useCan([Role.Admin, Role.Doctor])
  const isDoctor = user?.role === Role.Doctor
  const { t } = useI18n()
  const [tab, setTab] = useState('performed')
  return (
    <div>
      <PageHeader
        crumbs={t('treat.crumbs')}
        title={t('treat.title')}
        description={t('treat.lede')}
      />
      <Tabs
        tabs={[
          { id: 'performed', label: t('treat.performed') },
          { id: 'catalog', label: t('treat.catalog') },
          { id: 'categories', label: t('treat.categories') },
        ]}
        value={tab}
        onChange={setTab}
      />
      {tab === 'performed' ? <PerformedPanel canWrite={canClinical} isDoctor={isDoctor} isAdmin={isAdmin} /> : null}
      {tab === 'catalog' ? <CatalogPanel canWrite={isAdmin} /> : null}
      {tab === 'categories' ? <CategoriesPanel canWrite={isAdmin} /> : null}
    </div>
  )
}

function PerformedPanel({ canWrite, isDoctor, isAdmin }: { canWrite: boolean; isDoctor: boolean; isAdmin: boolean }) {
  const { t } = useI18n()
  const toast = useToast()
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('')
  const [open, setOpen] = useState(false)
  const [patientSearch, setPatientSearch] = useState('')
  const pq = useDebouncedValue(patientSearch)
  const patients = useAsync(() => pq ? patientsApi.list({ search: pq, pageSize: 8, isActive: true }) : Promise.resolve({ items: [], page: 1, pageSize: 8, totalCount: 0, totalPages: 0 }), [pq])
  const doctors = useAsync(() => doctorsApi.list({ isActive: true, pageSize: 100 }), [])
  const catalog = useAsync(() => treatmentsApi.list({ isActive: true, pageSize: 100 }), [])
  const list = useAsync(() => patientTreatmentsApi.list({ page, pageSize: 20, status: status || undefined }), [page, status])
  const [form, setForm] = useState({ patientId: '', doctorId: '', visitId: '', treatmentId: '', treatmentName: '', unitPrice: '', quantity: '1', discountAmount: '0', notes: '', treatmentDate: localDateTimeValue() })
  const [visits, setVisits] = useState<{ visitId: number; visitDate: string }[]>([])
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function loadVisits(patientId: string) {
    if (!patientId) return
    const res = await visitsApi.list({ patientId: Number(patientId), pageSize: 50 })
    setVisits(res.items.map((v) => ({ visitId: v.visitId, visitDate: v.visitDate })))
  }

  async function submit(event: FormEvent) {
    event.preventDefault()
    if (!form.patientId) { setError('Select a patient.'); return }
    if (!form.treatmentId && (!form.treatmentName || form.unitPrice === '')) {
      setError('Choose a catalog item or enter a custom name and unit price.')
      return
    }
    setBusy(true); setError('')
    const body: CreatePatientTreatmentRequest = {
      patientId: Number(form.patientId),
      doctorId: isDoctor || !form.doctorId ? null : Number(form.doctorId),
      visitId: form.visitId ? Number(form.visitId) : null,
      treatmentId: form.treatmentId ? Number(form.treatmentId) : null,
      treatmentName: form.treatmentId ? null : form.treatmentName,
      unitPrice: form.treatmentId ? (form.unitPrice ? Number(form.unitPrice) : null) : Number(form.unitPrice),
      quantity: Number(form.quantity),
      discountAmount: Number(form.discountAmount),
      notes: form.notes || null,
      treatmentDate: fromDateTimeLocal(form.treatmentDate),
    }
    try {
      await patientTreatmentsApi.create(body)
      toast.push('Treatment recorded. Name and price are snapshotted.')
      setOpen(false)
      void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to record treatment.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <div className="toolbar">
        <select className="control" value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All statuses</option>
          <option>UNPAID</option><option>PARTIALLY_PAID</option><option>PAID</option><option>VOIDED</option>
        </select>
        {canWrite ? <Button type="button" onClick={() => setOpen(true)}>{t('treat.record')}</Button> : null}
      </div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load treatments.'} /> : null}
      {list.data?.items.length === 0 ? <EmptyState title={t('treat.emptyPerformed')} text={t('treat.emptyPerformedText')} /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Date</th><th>Patient</th><th>Treatment</th><th>Qty</th><th>Unit</th><th>Final</th><th>Status</th></tr></thead>
              <tbody>
                {list.data.items.map((t) => (
                  <tr key={t.patientTreatmentId}>
                    <td>{formatDateTime(t.treatmentDate)}</td>
                    <td><Link to={`/patients/${t.patientId}`}>{t.patientName}</Link></td>
                    <td>{t.treatmentName}{t.treatmentId ? '' : ' (custom)'}</td>
                    <td>{t.quantity}</td>
                    <td>{money(t.unitPrice)}</td>
                    <td>{money(t.finalAmount)}</td>
                    <td><StatusBadge status={t.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="card-pad"><Pagination page={list.data.page} totalPages={list.data.totalPages} totalCount={list.data.totalCount} onPage={setPage} /></div>
        </div>
      ) : null}
      {open && canWrite ? (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
            <header><h2>Record performed treatment</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field">
                <label>Patient</label>
                <input className="control" value={patientSearch} onChange={(e) => setPatientSearch(e.target.value)} placeholder="Search" />
                {patients.data?.items.map((p) => (
                  <Button key={p.patientId} type="button" size="sm" variant="ghost" onClick={() => { setForm({ ...form, patientId: String(p.patientId) }); setPatientSearch(p.fullName); void loadVisits(String(p.patientId)) }}>{p.fullName}</Button>
                ))}
              </div>
              {isAdmin ? (
                <div className="field">
                  <label>Doctor</label>
                  <select className="control" value={form.doctorId} onChange={(e) => setForm({ ...form, doctorId: e.target.value })}>
                    <option value="">Select doctor</option>
                    {doctors.data?.items.map((d) => <option key={d.doctorId} value={d.doctorId}>{d.fullName}</option>)}
                  </select>
                </div>
              ) : <p className="muted">Assigned to your doctor profile.</p>}
              <div className="field">
                <label>Visit (optional, same patient)</label>
                <select className="control" value={form.visitId} onChange={(e) => setForm({ ...form, visitId: e.target.value })}>
                  <option value="">None</option>
                  {visits.map((v) => <option key={v.visitId} value={v.visitId}>{formatDateTime(v.visitDate)}</option>)}
                </select>
              </div>
              <div className="field">
                <label>Catalog item</label>
                <select className="control" value={form.treatmentId} onChange={(e) => {
                  const id = e.target.value
                  const item = catalog.data?.items.find((t) => String(t.treatmentId) === id)
                  setForm({ ...form, treatmentId: id, treatmentName: item?.name ?? '', unitPrice: item ? String(item.defaultPrice) : form.unitPrice })
                }}>
                  <option value="">Custom / ad-hoc</option>
                  {catalog.data?.items.map((t) => <option key={t.treatmentId} value={t.treatmentId}>{t.name} · {money(t.defaultPrice)}</option>)}
                </select>
              </div>
              {!form.treatmentId ? (
                <div className="field"><label>Custom name</label><input className="control" value={form.treatmentName} onChange={(e) => setForm({ ...form, treatmentName: e.target.value })} /></div>
              ) : null}
              <div className="form-grid">
                <div className="field"><label>Unit price</label><input className="control" type="number" min={0} step="0.01" value={form.unitPrice} onChange={(e) => setForm({ ...form, unitPrice: e.target.value })} /></div>
                <div className="field"><label>Quantity</label><input className="control" type="number" min={0.01} step="0.01" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: e.target.value })} /></div>
                <div className="field"><label>Discount</label><input className="control" type="number" min={0} step="0.01" value={form.discountAmount} onChange={(e) => setForm({ ...form, discountAmount: e.target.value })} /></div>
                <div className="field"><label>Date</label><input className="control" type="datetime-local" value={form.treatmentDate} onChange={(e) => setForm({ ...form, treatmentDate: e.target.value })} /></div>
              </div>
              <div className="field"><label>Notes</label><textarea className="control" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} /></div>
              <p className="hint">Final amount is calculated by the database. Status starts as UNPAID.</p>
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save'}</Button>
            </footer>
          </form>
        </div>
      ) : null}
    </div>
  )
}

function CatalogPanel({ canWrite }: { canWrite: boolean }) {
  const { t } = useI18n()
  const toast = useToast()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const q = useDebouncedValue(search)
  const categories = useAsync(() => treatmentCategoriesApi.list({ isActive: true, pageSize: 100 }), [])
  const list = useAsync(() => treatmentsApi.list({ search: q || undefined, page, pageSize: 20 }), [q, page])
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<TreatmentDetailDto | null>(null)
  const [form, setForm] = useState({ name: '', categoryId: '', description: '', defaultPrice: '', durationMinutes: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault()
    setBusy(true); setError('')
    const body: CreateTreatmentRequest = {
      name: form.name,
      categoryId: form.categoryId ? Number(form.categoryId) : null,
      description: form.description || null,
      defaultPrice: Number(form.defaultPrice),
      durationMinutes: form.durationMinutes ? Number(form.durationMinutes) : null,
    }
    try {
      if (editing) {
        await treatmentsApi.update(editing.treatmentId, { ...body, isActive: editing.isActive })
        toast.push('Catalog item updated. Historical patient treatments are unchanged.')
      } else {
        await treatmentsApi.create(body)
        toast.push('Catalog item created.')
      }
      setOpen(false)
      void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to save catalog item.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <p className="muted" style={{ marginBottom: 12 }}>{t('treat.jeninNote')}</p>
      <div className="toolbar">
        <input className="control" value={search} onChange={(e) => setSearch(e.target.value)} placeholder={t('treat.searchCatalog')} />
        {canWrite ? <Button type="button" onClick={() => { setEditing(null); setForm({ name: '', categoryId: '', description: '', defaultPrice: '', durationMinutes: '' }); setOpen(true) }}>{t('treat.addCatalog')}</Button> : null}
      </div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.data?.items.length === 0 ? <EmptyState title="No catalog items" text="Administrators can add reusable treatments here." /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Name</th><th>Category</th><th>Default price</th><th>Duration</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {list.data.items.map((item) => (
                  <tr key={item.treatmentId}>
                    <td>{item.name}</td>
                    <td>{item.categoryName || '—'}</td>
                    <td>{money(item.defaultPrice)}</td>
                    <td>{item.durationMinutes ? `${item.durationMinutes} min` : '—'}</td>
                    <td><StatusBadge status={item.isActive ? 'ACTIVE' : 'INACTIVE'} /></td>
                    <td>{canWrite ? <Button size="sm" variant="ghost" type="button" onClick={async () => {
                      const d = await treatmentsApi.get(item.treatmentId)
                      setEditing(d)
                      setForm({ name: d.name, categoryId: d.categoryId ? String(d.categoryId) : '', description: d.description ?? '', defaultPrice: String(d.defaultPrice), durationMinutes: d.durationMinutes ? String(d.durationMinutes) : '' })
                      setOpen(true)
                    }}>{t('common.edit')}</Button> : null}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="card-pad"><Pagination page={list.data.page} totalPages={list.data.totalPages} totalCount={list.data.totalCount} onPage={setPage} /></div>
        </div>
      ) : null}
      {open && canWrite ? (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
            <header><h2>{editing ? 'Edit catalog item' : 'New catalog item'}</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field"><label>Name</label><input className="control" required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} /></div>
              <div className="field">
                <label>Category</label>
                <select className="control" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })}>
                  <option value="">None</option>
                  {categories.data?.items.map((c) => <option key={c.categoryId} value={c.categoryId}>{c.name}</option>)}
                </select>
              </div>
              <div className="field"><label>Default price (ILS)</label><input className="control" type="number" min={0} step="0.01" required value={form.defaultPrice} onChange={(e) => setForm({ ...form, defaultPrice: e.target.value })} /></div>
              <div className="field"><label>Duration (minutes)</label><input className="control" type="number" min={1} value={form.durationMinutes} onChange={(e) => setForm({ ...form, durationMinutes: e.target.value })} /></div>
              <div className="field"><label>Description</label><textarea className="control" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></div>
              {editing ? (
                <Button type="button" variant="ghost" onClick={async () => {
                  await treatmentsApi.update(editing.treatmentId, { isActive: !editing.isActive })
                  toast.push(editing.isActive ? 'Deactivated.' : 'Activated.')
                  setOpen(false)
                  void list.reload()
                }}>{editing.isActive ? 'Deactivate' : 'Activate'}</Button>
              ) : null}
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save'}</Button>
            </footer>
          </form>
        </div>
      ) : null}
    </div>
  )
}

function CategoriesPanel({ canWrite }: { canWrite: boolean }) {
  const toast = useToast()
  const list = useAsync(() => treatmentCategoriesApi.list({ isActive: null, pageSize: 100 }), [])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  if (list.loading) return <LoadingSkeleton />
  if (list.error) return <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load categories.'} />
  return (
    <div className="grid grid-2">
      <div className="card">
        <div className="table-wrap">
          <table className="data">
            <thead><tr><th>Name</th><th>Status</th><th></th></tr></thead>
            <tbody>
              {list.data?.items.map((c) => (
                <tr key={c.categoryId}>
                  <td>{c.name}</td>
                  <td><StatusBadge status={c.isActive ? 'ACTIVE' : 'INACTIVE'} /></td>
                  <td>{canWrite ? <Button size="sm" variant="ghost" type="button" onClick={async () => {
                    await treatmentCategoriesApi.update(c.categoryId, { isActive: !c.isActive })
                    toast.push('Category updated.')
                    void list.reload()
                  }}>{c.isActive ? 'Deactivate' : 'Activate'}</Button> : null}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
      {canWrite ? (
        <form className="card card-pad stack" onSubmit={async (e) => {
          e.preventDefault()
          await treatmentCategoriesApi.create({ name, description: description || null })
          toast.push('Category created.')
          setName(''); setDescription('')
          void list.reload()
        }}>
          <h2>New category</h2>
          <div className="field"><label>Name</label><input className="control" required value={name} onChange={(e) => setName(e.target.value)} /></div>
          <div className="field"><label>Description</label><textarea className="control" value={description} onChange={(e) => setDescription(e.target.value)} /></div>
          <Button type="submit">Create</Button>
        </form>
      ) : null}
    </div>
  )
}
