import { useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { expensePaymentsApi, expensesApi, paymentMethodsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, ConfirmDialog, EmptyState, ErrorState, LoadingSkeleton, PageHeader, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync } from '@/hooks/useAsync'
import { formatDate, formatDateTime, fromDateTimeLocal, localDateTimeValue, money } from '@/utils/format'

export function ExpenseDetailPage() {
  const { expenseId } = useParams()
  const id = Number(expenseId)
  const canWrite = useCan([Role.Admin, Role.Secretary])
  const toast = useToast()
  const expenseQ = useAsync(() => expensesApi.get(id), [id])
  const payQ = useAsync(() => expensePaymentsApi.list({ expenseId: id, pageSize: 50 }), [id])
  const methods = useAsync(() => paymentMethodsApi.list(true), [])
  const [open, setOpen] = useState(false)
  const [voidExpense, setVoidExpense] = useState(false)
  const [voidPay, setVoidPay] = useState<number | null>(null)
  const [reason, setReason] = useState('')
  const [form, setForm] = useState({ amount: '', method: 'CASH', paymentMethodId: '', paymentDate: localDateTimeValue(), referenceNumber: '', notes: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const expense = expenseQ.data
  if (expenseQ.loading) return <LoadingSkeleton />
  if (expenseQ.error || !expense) return <ErrorState text={isApiError(expenseQ.error) ? expenseQ.error.message : 'Expense not found.'} />

  async function pay(event: FormEvent) {
    event.preventDefault()
    setBusy(true); setError('')
    try {
      await expensePaymentsApi.create({
        expenseId: id,
        amount: Number(form.amount),
        method: form.method,
        paymentMethodId: form.paymentMethodId ? Number(form.paymentMethodId) : null,
        paymentDate: fromDateTimeLocal(form.paymentDate),
        referenceNumber: form.referenceNumber || null,
        notes: form.notes || null,
      })
      toast.push('Expense payment recorded.')
      setOpen(false)
      void expenseQ.reload()
      void payQ.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to record payment.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <PageHeader crumbs="Expenses / Detail" title={expense.description} description={`${expense.expenseType.replaceAll('_', ' ')} · ${formatDate(expense.expenseDate)}`} />
      <div className="grid grid-3" style={{ marginBottom: 16 }}>
        <article className="card stat"><div className="stat-label">Obligation</div><div className="stat-value">{money(expense.totalAmount)}</div><div className="stat-hint">Not cash paid</div></article>
        <article className="card stat"><div className="stat-label">Status</div><div className="stat-value" style={{ fontSize: 22 }}><StatusBadge status={expense.status} /></div><div className="stat-hint">Derived by the server after payments</div></article>
        <article className="card stat"><div className="stat-label">Supplier</div><div className="stat-value" style={{ fontSize: 22 }}>{expense.supplierName || '—'}</div>{expense.supplierId ? <Link to={`/suppliers/${expense.supplierId}`}>Open statement</Link> : null}</article>
      </div>
      <p className="muted">Remaining amount is authoritative on the supplier statement when a supplier is linked. This expense DTO exposes total and status only.</p>
      <div className="row" style={{ margin: '16px 0' }}>
        {canWrite ? <Button type="button" onClick={() => setOpen(true)}>Record payment</Button> : null}
        {canWrite ? <Button variant="danger" type="button" onClick={() => { setVoidExpense(true); setReason('') }}>Void expense</Button> : null}
      </div>
      <div className="card">
        <div className="card-pad"><h2>Expense payments</h2></div>
        {payQ.data?.items.length ? (
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Date</th><th>Amount</th><th>Method</th><th>State</th><th></th></tr></thead>
              <tbody>
                {payQ.data.items.map((p) => (
                  <tr key={p.expensePaymentId}>
                    <td>{formatDateTime(p.paymentDate)}</td>
                    <td>{money(p.amount)}</td>
                    <td>{p.method}</td>
                    <td><StatusBadge status={p.isVoided ? 'VOIDED' : 'VALID'} /></td>
                    <td>{canWrite && !p.isVoided ? <Button size="sm" variant="danger" type="button" onClick={() => { setVoidPay(p.expensePaymentId); setReason('') }}>Void</Button> : null}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : <EmptyState title="No payments yet" text="Record a payment to reduce this obligation." />}
      </div>

      {open ? (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={pay}>
            <header><h2>Pay expense</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field"><label>Amount</label><input className="control" type="number" min={0.01} step="0.01" required value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} /></div>
              <div className="field">
                <label>Method</label>
                <select className="control" value={form.method} onChange={(e) => setForm({ ...form, method: e.target.value })}>
                  <option>CASH</option><option>CARD</option><option>BANK_TRANSFER</option><option>CHEQUE</option><option>OTHER</option>
                </select>
              </div>
              <div className="field">
                <label>Clinic method</label>
                <select className="control" value={form.paymentMethodId} onChange={(e) => setForm({ ...form, paymentMethodId: e.target.value })}>
                  <option value="">None</option>
                  {methods.data?.items.map((m) => <option key={m.paymentMethodId} value={m.paymentMethodId}>{m.name}</option>)}
                </select>
              </div>
              <div className="field"><label>Date</label><input className="control" type="datetime-local" value={form.paymentDate} onChange={(e) => setForm({ ...form, paymentDate: e.target.value })} /></div>
              <div className="field"><label>Reference</label><input className="control" value={form.referenceNumber} onChange={(e) => setForm({ ...form, referenceNumber: e.target.value })} /></div>
              <div className="field"><label>Notes</label><textarea className="control" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} /></div>
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save'}</Button>
            </footer>
          </form>
        </div>
      ) : null}

      {voidExpense ? (
        <ConfirmDialog title="Void expense" message="Blocked if valid payments exist." confirmLabel="Void" danger reasonRequired reason={reason} onReason={setReason} onCancel={() => setVoidExpense(false)} onConfirm={async () => {
          try {
            await expensesApi.void(id, { reason })
            toast.push('Expense voided.')
            setVoidExpense(false)
            void expenseQ.reload()
          } catch (err) {
            toast.push(isApiError(err) ? err.message : 'Unable to void.', 'error')
          }
        }} />
      ) : null}
      {voidPay ? (
        <ConfirmDialog title="Void expense payment" message="The payment remains stored but is excluded from totals." confirmLabel="Void" danger reasonRequired reason={reason} onReason={setReason} onCancel={() => setVoidPay(null)} onConfirm={async () => {
          try {
            await expensePaymentsApi.void(voidPay, { reason })
            toast.push('Expense payment voided.')
            setVoidPay(null)
            void expenseQ.reload()
            void payQ.reload()
          } catch (err) {
            toast.push(isApiError(err) ? err.message : 'Unable to void.', 'error')
          }
        }} />
      ) : null}
    </div>
  )
}
