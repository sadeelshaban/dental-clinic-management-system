import { useEffect, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { patientTreatmentsApi, patientsApi, paymentMethodsApi, paymentsApi, treatmentsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, ConfirmDialog, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { CreatePatientTreatmentRequest, CreatePaymentRequest, PatientTreatmentListItemDto } from '@/types/api'
import { formatDateTime, fromDateTimeLocal, localDateTimeValue, money } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

const METHODS = ['CASH', 'CARD', 'BANK_TRANSFER', 'CHEQUE', 'OTHER']

export function PaymentsPage() {
  const { t } = useI18n()
  const canWrite = useCan([Role.Admin, Role.Secretary])
  const toast = useToast()
  const [params] = useSearchParams()
  const [page, setPage] = useState(1)
  const [voided, setVoided] = useState('')
  const [open, setOpen] = useState(params.get('new') === '1')
  const [voidId, setVoidId] = useState<number | null>(null)
  const [reason, setReason] = useState('')
  const [patientSearch, setPatientSearch] = useState('')
  const pq = useDebouncedValue(patientSearch)
  const patients = useAsync(() => pq ? patientsApi.list({ search: pq, pageSize: 8, isActive: true }) : Promise.resolve({ items: [], page: 1, pageSize: 8, totalCount: 0, totalPages: 0 }), [pq])
  const methods = useAsync(() => paymentMethodsApi.list(true), [])
  const catalog = useAsync(
    () => (open ? treatmentsApi.list({ isActive: true, pageSize: 100 }) : Promise.resolve(null)),
    [open],
  )
  const catalogItems = catalog.data?.items ?? []
  const [selectedPatientId, setSelectedPatientId] = useState<number | null>(
    params.get('patientId') ? Number(params.get('patientId')) : null,
  )
  const [treatments, setTreatments] = useState<PatientTreatmentListItemDto[]>([])
  const [treatmentSelection, setTreatmentSelection] = useState('')
  const [form, setForm] = useState({ amount: '', method: 'CASH', paymentMethodId: '', paymentDate: localDateTimeValue(), referenceNumber: '', notes: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const list = useAsync(() => paymentsApi.list({ page, pageSize: 20, isVoided: voided === '' ? null : voided === 'true', patientId: params.get('patientId') ? Number(params.get('patientId')) : undefined }), [page, voided, params])

  useEffect(() => { if (params.get('new') === '1') setOpen(true) }, [params])

  useEffect(() => {
    if (!selectedPatientId) return
    void loadTreatments(selectedPatientId)
    void patientsApi.get(selectedPatientId).then((p) => setPatientSearch(p.fullName)).catch(() => {})
  }, [selectedPatientId])

  async function loadTreatments(patientId: number) {
    const res = await patientTreatmentsApi.list({ patientId, pageSize: 50 })
    setTreatments(res.items.filter((t) => t.status !== 'PAID' && t.status !== 'VOIDED'))
  }

  function selectPatient(patientId: number, fullName: string) {
    setSelectedPatientId(patientId)
    setPatientSearch(fullName)
    setTreatmentSelection('')
    setForm((f) => ({ ...f, amount: '' }))
    void loadTreatments(patientId)
  }

  function onTreatmentChange(value: string) {
    setTreatmentSelection(value)
    if (value.startsWith('cat:')) {
      const item = catalogItems.find((t) => String(t.treatmentId) === value.slice(4))
      if (item) setForm((f) => ({ ...f, amount: String(item.defaultPrice) }))
    } else if (value.startsWith('pt:')) {
      const line = treatments.find((t) => String(t.patientTreatmentId) === value.slice(3))
      if (line) setForm((f) => ({ ...f, amount: String(line.finalAmount) }))
    }
  }

  function onPatientSearchChange(value: string) {
    setPatientSearch(value)
    setSelectedPatientId(null)
    setTreatmentSelection('')
    setTreatments([])
    setForm((f) => ({ ...f, amount: '' }))
  }

  function openPaymentModal() {
    setOpen(true)
    setError('')
  }

  function closePaymentModal() {
    setOpen(false)
    setError('')
  }

  async function submit(event: FormEvent) {
    event.preventDefault()
    if (!selectedPatientId) {
      setError(t('pay.selectPatient'))
      return
    }
    if (!treatmentSelection || Number(form.amount) <= 0) {
      setError(t('pay.treatmentAmountRequired'))
      return
    }
    setBusy(true); setError('')
    try {
      let patientTreatmentId: number
      if (treatmentSelection.startsWith('cat:')) {
        const treatmentId = Number(treatmentSelection.slice(4))
        const body: CreatePatientTreatmentRequest = {
          patientId: selectedPatientId,
          treatmentId,
          quantity: 1,
          discountAmount: 0,
          treatmentDate: fromDateTimeLocal(form.paymentDate),
        }
        const created = await patientTreatmentsApi.create(body)
        patientTreatmentId = created.patientTreatmentId
      } else {
        patientTreatmentId = Number(treatmentSelection.slice(3))
      }
      const body: CreatePaymentRequest = {
        patientTreatmentId,
        amount: Number(form.amount),
        method: form.method,
        paymentMethodId: form.paymentMethodId ? Number(form.paymentMethodId) : null,
        paymentDate: fromDateTimeLocal(form.paymentDate),
        referenceNumber: form.referenceNumber || null,
        notes: form.notes || null,
      }
      await paymentsApi.create(body)
      toast.push(t('pay.saved'))
      setOpen(false)
      setSelectedPatientId(null)
      setTreatmentSelection('')
      setPatientSearch('')
      void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : t('pay.saveFailed'))
    } finally { setBusy(false) }
  }

  return (
    <div>
      <PageHeader
        crumbs={t('pay.crumbs')}
        title={t('pay.title')}
        description={t('pay.lede')}
        actions={canWrite ? <Button type="button" onClick={openPaymentModal}>{t('pay.record')}</Button> : null}
      />
      <div className="toolbar">
        <select className="control" value={voided} onChange={(e) => setVoided(e.target.value)}>
          <option value="">All</option>
          <option value="false">Valid only</option>
          <option value="true">Voided only</option>
        </select>
      </div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load payments.'} onRetry={() => void list.reload()} /> : null}
      {list.data?.items.length === 0 ? <EmptyState title="No payments" text="Collected payments will appear here." /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Date</th><th>Patient</th><th>Treatment</th><th>Amount</th><th>Method</th><th>State</th><th></th></tr></thead>
              <tbody>
                {list.data.items.map((p) => (
                  <tr key={p.paymentId}>
                    <td>{formatDateTime(p.paymentDate)}</td>
                    <td><Link to={`/patients/${p.patientId}`}>{p.patientName}</Link></td>
                    <td>{p.treatmentName}</td>
                    <td>{money(p.amount)}</td>
                    <td>{p.method}</td>
                    <td><StatusBadge status={p.isVoided ? 'VOIDED' : 'VALID'} /></td>
                    <td>{canWrite && !p.isVoided ? <Button size="sm" variant="danger" type="button" onClick={() => { setVoidId(p.paymentId); setReason('') }}>Void</Button> : null}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="card-pad"><Pagination page={list.data.page} totalPages={list.data.totalPages} totalCount={list.data.totalCount} onPage={setPage} /></div>
        </div>
      ) : null}

      {open && canWrite ? (
        <div className="modal-backdrop" onClick={closePaymentModal}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
            <header><h2>{t('pay.record')}</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field">
                <label>{t('pay.patient')}</label>
                <input
                  className="control"
                  value={patientSearch}
                  onChange={(e) => onPatientSearchChange(e.target.value)}
                  placeholder={t('pay.searchPatient')}
                />
                {selectedPatientId ? (
                  <p className="hint">{t('pay.patientSelected')}: <strong>{patientSearch}</strong></p>
                ) : null}
                {!selectedPatientId && patients.data?.items.length ? (
                  <select
                    className="control"
                    value=""
                    onChange={(e) => {
                      const patient = patients.data?.items.find((p) => String(p.patientId) === e.target.value)
                      if (patient) selectPatient(patient.patientId, patient.fullName)
                    }}
                  >
                    <option value="">{t('pay.choosePatient')}</option>
                    {patients.data.items.map((p) => (
                      <option key={p.patientId} value={p.patientId}>{p.fullName} · {p.patientNumber}</option>
                    ))}
                  </select>
                ) : null}
              </div>
              <div className="field">
                <label>{t('pay.treatmentLine')}</label>
                <select
                  className="control"
                  required
                  value={treatmentSelection}
                  onChange={(e) => onTreatmentChange(e.target.value)}
                >
                  <option value="">{t('pay.selectTreatment')}</option>
                  {treatments.length > 0 ? (
                    <optgroup label={t('pay.existingLines')}>
                      {treatments.map((t) => (
                        <option key={t.patientTreatmentId} value={`pt:${t.patientTreatmentId}`}>
                          {t.treatmentName} · {t.status} · {money(t.finalAmount)}
                        </option>
                      ))}
                    </optgroup>
                  ) : null}
                  {catalogItems.length > 0 ? (
                    <optgroup label={t('pay.catalogItems')}>
                      {catalogItems.map((t) => (
                        <option key={t.treatmentId} value={`cat:${t.treatmentId}`}>
                          {t.name} · {money(t.defaultPrice)}
                        </option>
                      ))}
                    </optgroup>
                  ) : null}
                </select>
                {!selectedPatientId ? <p className="hint">{t('pay.selectPatientFirst')}</p> : null}
                {catalog.loading ? <p className="hint">{t('pay.loadingCatalog')}</p> : null}
                {catalog.error ? (
                  <div className="form-error row" style={{ alignItems: 'center', gap: 8 }}>
                    <span>{isApiError(catalog.error) ? catalog.error.message : t('pay.catalogLoadFailed')}</span>
                    <Button size="sm" variant="ghost" type="button" onClick={() => void catalog.reload()}>{t('common.retry')}</Button>
                  </div>
                ) : null}
                {!catalog.loading && !catalog.error && catalogItems.length === 0 ? (
                  <p className="hint">{t('pay.catalogEmpty')}</p>
                ) : null}
              </div>
              <div className="field"><label>Amount</label><input className="control" type="number" min={0.01} step="0.01" required value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} /></div>
              <div className="field">
                <label>Method</label>
                <select className="control" value={form.method} onChange={(e) => setForm({ ...form, method: e.target.value })}>
                  {METHODS.map((m) => <option key={m}>{m}</option>)}
                </select>
              </div>
              <div className="field">
                <label>Clinic method (optional)</label>
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
              <Button variant="ghost" type="button" onClick={closePaymentModal}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save payment'}</Button>
            </footer>
          </form>
        </div>
      ) : null}

      {voidId ? (
        <ConfirmDialog
          title="Void payment"
          message="The payment stays stored for audit but stops counting toward revenue and balances. A reason is required."
          confirmLabel="Void payment"
          danger
          reasonRequired
          reason={reason}
          onReason={setReason}
          onCancel={() => setVoidId(null)}
          onConfirm={async () => {
            try {
              await paymentsApi.void(voidId, { reason })
              toast.push('Payment voided.')
              setVoidId(null)
              void list.reload()
            } catch (err) {
              toast.push(isApiError(err) ? err.message : 'Unable to void payment.', 'error')
            }
          }}
        />
      ) : null}
    </div>
  )
}
