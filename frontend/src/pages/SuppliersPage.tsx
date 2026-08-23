import { useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { suppliersApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, EmptyState, ErrorState, LoadingSkeleton, PageHeader, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { CreateSupplierRequest } from '@/types/api'
import { formatDate, money } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

export function SuppliersPage() {
  const { t } = useI18n()
  const canWrite = useCan([Role.Admin])
  const toast = useToast()
  const [search, setSearch] = useState('')
  const q = useDebouncedValue(search)
  const list = useAsync(() => suppliersApi.list({ search: q || undefined, isActive: true }), [q])
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState<CreateSupplierRequest>({ name: '', phone: '', email: '', address: '', contactPerson: '', notes: '' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function submit(event: FormEvent) {
    event.preventDefault()
    setBusy(true); setError('')
    try {
      await suppliersApi.create({ ...form, phone: form.phone || null, email: form.email || null, address: form.address || null, contactPerson: form.contactPerson || null, notes: form.notes || null })
      toast.push('Supplier created.')
      setOpen(false)
      void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to save supplier.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <PageHeader crumbs={t('sup.crumbs')} title={t('sup.title')} description={t('sup.lede')} actions={canWrite ? <Button type="button" onClick={() => setOpen(true)}>{t('sup.add')}</Button> : null} />
      <div className="toolbar"><input className="control" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search suppliers" /></div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load suppliers.'} /> : null}
      {list.data?.items.length === 0 ? <EmptyState title="No suppliers" text="Administrators can register suppliers here." /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Name</th><th>Contact</th><th>Phone</th><th></th></tr></thead>
              <tbody>
                {list.data.items.map((s) => (
                  <tr key={s.supplierId}>
                    <td><Link to={`/suppliers/${s.supplierId}`}>{s.name}</Link></td>
                    <td>{s.contactPerson || '—'}</td>
                    <td>{s.phone || '—'}</td>
                    <td><Link className="btn btn-ghost btn-sm" to={`/suppliers/${s.supplierId}`}>Statement</Link></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : null}
      {open && canWrite ? (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
            <header><h2>New supplier</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field"><label>Name</label><input className="control" required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} /></div>
              <div className="field"><label>Contact person</label><input className="control" value={form.contactPerson ?? ''} onChange={(e) => setForm({ ...form, contactPerson: e.target.value })} /></div>
              <div className="field"><label>Phone</label><input className="control" value={form.phone ?? ''} onChange={(e) => setForm({ ...form, phone: e.target.value })} /></div>
              <div className="field"><label>Email</label><input className="control" type="email" value={form.email ?? ''} onChange={(e) => setForm({ ...form, email: e.target.value })} /></div>
              <div className="field"><label>Address</label><input className="control" value={form.address ?? ''} onChange={(e) => setForm({ ...form, address: e.target.value })} /></div>
              <div className="field"><label>Notes</label><textarea className="control" value={form.notes ?? ''} onChange={(e) => setForm({ ...form, notes: e.target.value })} /></div>
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

export function SupplierDetailPage() {
  const { supplierId } = useParams()
  const id = Number(supplierId)
  const canWrite = useCan([Role.Admin])
  const toast = useToast()
  const supplierQ = useAsync(() => suppliersApi.get(id), [id])
  const stmtQ = useAsync(() => suppliersApi.statement(id), [id])
  const [name, setName] = useState('')
  const supplier = supplierQ.data
  if (supplierQ.loading) return <LoadingSkeleton />
  if (!supplier) return <ErrorState text={isApiError(supplierQ.error) ? supplierQ.error.message : 'Supplier not found.'} />

  return (
    <div>
      <PageHeader crumbs="Suppliers / Statement" title={supplier.name} description={supplier.contactPerson || supplier.email || ''} />
      <div className="grid grid-4" style={{ marginBottom: 16 }}>
        <article className="card stat"><div className="stat-label">Transactions</div><div className="stat-value">{stmtQ.data?.totalTransactions ?? 0}</div></article>
        <article className="card stat"><div className="stat-label">Obligations</div><div className="stat-value">{money(stmtQ.data?.totalPurchases)}</div><div className="stat-hint">Not cash outflow</div></article>
        <article className="card stat stat-expense"><div className="stat-label">Paid</div><div className="stat-value">{money(stmtQ.data?.totalPaid)}</div></article>
        <article className="card stat stat-out"><div className="stat-label">Remaining</div><div className="stat-value">{money(stmtQ.data?.totalRemaining)}</div></article>
      </div>
      <div className="card" style={{ marginBottom: 16 }}>
        <div className="card-pad"><h2>Expense lines</h2></div>
        {stmtQ.data?.lines.length ? (
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Date</th><th>Description</th><th>Total</th><th>Paid</th><th>Remaining</th><th>Status</th></tr></thead>
              <tbody>
                {stmtQ.data.lines.map((line) => (
                  <tr key={line.expenseId}>
                    <td>{formatDate(line.expenseDate)}</td>
                    <td><Link to={`/expenses/${line.expenseId}`}>{line.description}</Link></td>
                    <td>{money(line.totalAmount)}</td>
                    <td>{money(line.paid)}</td>
                    <td>{money(line.remaining)}</td>
                    <td><StatusBadge status={line.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : <EmptyState title="No expenses" text="No obligations are linked to this supplier." />}
      </div>
      {canWrite ? (
        <form className="card card-pad stack" onSubmit={async (e) => {
          e.preventDefault()
          await suppliersApi.update(id, { name: name || supplier.name })
          toast.push('Supplier updated.')
          void supplierQ.reload()
        }}>
          <h2>Update name</h2>
          <input className="control" value={name} onChange={(e) => setName(e.target.value)} placeholder={supplier.name} />
          <Button type="submit">Save</Button>
        </form>
      ) : null}
    </div>
  )
}
