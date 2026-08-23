import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { expenseCategoriesApi, expensesApi, suppliersApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, ConfirmDialog, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync } from '@/hooks/useAsync'
import type { CreateExpenseRequest } from '@/types/api'
import { ExpenseType } from '@/types/api'
import { formatDate, money, todayIso } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

const TYPES = Object.values(ExpenseType)

export function ExpensesPage() {
  const { t } = useI18n()
  const canWrite = useCan([Role.Admin, Role.Secretary])
  const isAdmin = useCan([Role.Admin])
  const toast = useToast()
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('')
  const [type, setType] = useState('')
  const [open, setOpen] = useState(false)
  const [voidId, setVoidId] = useState<number | null>(null)
  const [reason, setReason] = useState('')
  const suppliers = useAsync(() => suppliersApi.list({ isActive: true }), [])
  const categories = useAsync(() => expenseCategoriesApi.list(), [])
  const list = useAsync(() => expensesApi.list({ page, pageSize: 20, status: status || undefined, expenseType: type || undefined }), [page, status, type])
  const [form, setForm] = useState({ supplierId: '', categoryId: '', expenseType: 'GENERAL', description: '', expenseDate: todayIso(), dueDate: '', totalAmount: '', notes: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [catName, setCatName] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault()
    setBusy(true); setError('')
    const body: CreateExpenseRequest = {
      supplierId: form.supplierId ? Number(form.supplierId) : null,
      categoryId: form.categoryId ? Number(form.categoryId) : null,
      expenseType: form.expenseType,
      description: form.description,
      expenseDate: form.expenseDate || null,
      dueDate: form.dueDate || null,
      totalAmount: Number(form.totalAmount),
      notes: form.notes || null,
    }
    try {
      await expensesApi.create(body)
      toast.push('Expense created as an unpaid obligation.')
      setOpen(false)
      void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to save expense.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <PageHeader
        crumbs={t('exp.crumbs')}
        title={t('exp.title')}
        description={t('exp.lede')}
        actions={canWrite ? <Button type="button" onClick={() => setOpen(true)}>{t('exp.new')}</Button> : null}
      />
      <div className="toolbar">
        <select className="control" value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All statuses</option>
          <option>UNPAID</option><option>PARTIALLY_PAID</option><option>PAID</option>
        </select>
        <select className="control" value={type} onChange={(e) => setType(e.target.value)}>
          <option value="">All types</option>
          {TYPES.map((t) => <option key={t}>{t}</option>)}
        </select>
      </div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load expenses.'} /> : null}
      {list.data?.items.length === 0 ? <EmptyState title="No expenses" text="Clinic obligations will appear here." /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Date</th><th>Description</th><th>Supplier</th><th>Type</th><th>Total</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {list.data.items.map((e) => (
                  <tr key={e.expenseId}>
                    <td>{formatDate(e.expenseDate)}</td>
                    <td><Link to={`/expenses/${e.expenseId}`}>{e.description}</Link></td>
                    <td>{e.supplierName || '—'}</td>
                    <td>{e.expenseType.replaceAll('_', ' ')}</td>
                    <td>{money(e.totalAmount)}</td>
                    <td><StatusBadge status={e.status} /></td>
                    <td>{canWrite && e.status !== 'PAID' ? <Button size="sm" variant="danger" type="button" onClick={() => { setVoidId(e.expenseId); setReason('') }}>Void</Button> : null}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="card-pad"><Pagination page={list.data.page} totalPages={list.data.totalPages} totalCount={list.data.totalCount} onPage={setPage} /></div>
        </div>
      ) : null}

      {isAdmin ? (
        <form className="card card-pad stack" style={{ marginTop: 16 }} onSubmit={async (e) => {
          e.preventDefault()
          await expenseCategoriesApi.create({ name: catName })
          toast.push('Expense category created.')
          setCatName('')
          void categories.reload()
        }}>
          <h2>Expense categories</h2>
          <div className="row">
            {categories.data?.items.map((c) => <span key={c.categoryId} className="badge badge-neutral">{c.name}</span>)}
          </div>
          <div className="row">
            <input className="control" required value={catName} onChange={(e) => setCatName(e.target.value)} placeholder="New category name" />
            <Button type="submit">Add category</Button>
          </div>
        </form>
      ) : null}

      {open && canWrite ? (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
            <header><h2>New expense</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field"><label>Description</label><input className="control" required value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></div>
              <div className="field"><label>Total obligation</label><input className="control" type="number" min={0.01} step="0.01" required value={form.totalAmount} onChange={(e) => setForm({ ...form, totalAmount: e.target.value })} /></div>
              <div className="field">
                <label>Type</label>
                <select className="control" value={form.expenseType} onChange={(e) => setForm({ ...form, expenseType: e.target.value })}>
                  {TYPES.map((t) => <option key={t}>{t}</option>)}
                </select>
              </div>
              <div className="field">
                <label>Supplier</label>
                <select className="control" value={form.supplierId} onChange={(e) => setForm({ ...form, supplierId: e.target.value })}>
                  <option value="">None</option>
                  {suppliers.data?.items.map((s) => <option key={s.supplierId} value={s.supplierId}>{s.name}</option>)}
                </select>
              </div>
              <div className="field">
                <label>Category</label>
                <select className="control" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })}>
                  <option value="">None</option>
                  {categories.data?.items.map((c) => <option key={c.categoryId} value={c.categoryId}>{c.name}</option>)}
                </select>
              </div>
              <div className="form-grid">
                <div className="field"><label>Date</label><input className="control" type="date" value={form.expenseDate} onChange={(e) => setForm({ ...form, expenseDate: e.target.value })} /></div>
                <div className="field"><label>Due</label><input className="control" type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} /></div>
              </div>
              <div className="field"><label>Notes</label><textarea className="control" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} /></div>
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save'}</Button>
            </footer>
          </form>
        </div>
      ) : null}

      {voidId ? (
        <ConfirmDialog
          title="Void expense"
          message="Voiding is blocked if valid payments exist. A reason is required."
          confirmLabel="Void expense"
          danger
          reasonRequired
          reason={reason}
          onReason={setReason}
          onCancel={() => setVoidId(null)}
          onConfirm={async () => {
            try {
              await expensesApi.void(voidId, { reason })
              toast.push('Expense voided.')
              setVoidId(null)
              void list.reload()
            } catch (err) {
              toast.push(isApiError(err) ? err.message : 'Unable to void expense.', 'error')
            }
          }}
        />
      ) : null}
    </div>
  )
}
