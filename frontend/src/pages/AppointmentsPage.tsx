import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { appointmentsApi, doctorsApi, patientsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useAuth } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, ConfirmDialog, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { AppointmentDetailDto, AppointmentListItemDto, CreateAppointmentRequest } from '@/types/api'
import { addDaysIso, formatDate, formatTime, startOfWeekIso, todayIso, toTimeApi } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

const STATUSES = ['SCHEDULED', 'CONFIRMED', 'COMPLETED', 'CANCELLED', 'NO_SHOW']

export function AppointmentsPage() {
  const { user } = useAuth()
  const { t } = useI18n()
  const isDoctor = user?.role === Role.Doctor
  const toast = useToast()
  const [params] = useSearchParams()
  const [from, setFrom] = useState(startOfWeekIso())
  const [to, setTo] = useState(addDaysIso(startOfWeekIso(), 6))
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const [view, setView] = useState<'week' | 'list'>('week')
  const [open, setOpen] = useState(params.get('new') === '1')
  const [editing, setEditing] = useState<AppointmentDetailDto | null>(null)
  const [patientSearch, setPatientSearch] = useState('')
  const patientQ = useDebouncedValue(patientSearch)
  const patients = useAsync(() => patientQ ? patientsApi.list({ search: patientQ, page: 1, pageSize: 8, isActive: true }) : Promise.resolve({ items: [], page: 1, pageSize: 8, totalCount: 0, totalPages: 0 }), [patientQ])
  const doctors = useAsync(() => doctorsApi.list({ isActive: true, page: 1, pageSize: 100 }), [])
  const [form, setForm] = useState({ patientId: params.get('patientId') || '', doctorId: '', appointmentDate: todayIso(), startTime: '09:00', endTime: '09:30', reason: '', notes: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [action, setAction] = useState<{ type: 'cancel' | 'no-show'; id: number } | null>(null)

  const list = useAsync(
    () => appointmentsApi.list({ from, to, status: status || undefined, page, pageSize: view === 'week' ? 100 : 20 }),
    [from, to, status, page, view],
  )

  useEffect(() => {
    if (params.get('new') === '1') setOpen(true)
  }, [params])

  const days = useMemo(() => Array.from({ length: 7 }, (_, i) => addDaysIso(from, i)), [from])
  const byDay = useMemo(() => {
    const map: Record<string, AppointmentListItemDto[]> = {}
    for (const day of days) map[day] = []
    for (const item of list.data?.items ?? []) {
      map[item.appointmentDate]?.push(item)
    }
    return map
  }, [days, list.data])

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    if (!form.patientId) {
      setError('Select a patient.')
      return
    }
    setBusy(true)
    const body: CreateAppointmentRequest = {
      patientId: Number(form.patientId),
      doctorId: isDoctor || !form.doctorId ? null : Number(form.doctorId),
      appointmentDate: form.appointmentDate,
      startTime: toTimeApi(form.startTime),
      endTime: toTimeApi(form.endTime),
      reason: form.reason || null,
      notes: form.notes || null,
    }
    try {
      if (editing) {
        await appointmentsApi.update(editing.appointmentId, body)
        toast.push('Appointment updated.')
      } else {
        await appointmentsApi.create(body)
        toast.push('Appointment created.')
      }
      setOpen(false)
      setEditing(null)
      void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to save appointment.')
    } finally {
      setBusy(false)
    }
  }

  async function openEdit(item: AppointmentListItemDto) {
    const detail = await appointmentsApi.get(item.appointmentId)
    setEditing(detail)
    setForm({
      patientId: String(detail.patientId),
      doctorId: String(detail.doctorId),
      appointmentDate: detail.appointmentDate,
      startTime: detail.startTime.slice(0, 5),
      endTime: detail.endTime.slice(0, 5),
      reason: detail.reason ?? '',
      notes: detail.notes ?? '',
    })
    setPatientSearch(detail.patientName)
    setOpen(true)
  }

  const canMutate = (statusValue: string) => statusValue === 'SCHEDULED' || statusValue === 'CONFIRMED'

  return (
    <div>
      <PageHeader
        crumbs={t('appt.crumbs')}
        title={t('appt.title')}
        description={t('appt.lede')}
        actions={<Button type="button" onClick={() => { setEditing(null); setOpen(true) }}>{t('appt.new')}</Button>}
      />
      <div className="toolbar">
        <Button size="sm" variant={view === 'week' ? 'primary' : 'ghost'} type="button" onClick={() => setView('week')}>{t('appt.week')}</Button>
        <Button size="sm" variant={view === 'list' ? 'primary' : 'ghost'} type="button" onClick={() => setView('list')}>{t('appt.list')}</Button>
        <input className="control" type="date" value={from} onChange={(e) => { setFrom(e.target.value); setTo(addDaysIso(e.target.value, 6)); setPage(1) }} />
        <input className="control" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        <select className="control" value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s.replaceAll('_', ' ')}</option>)}
        </select>
      </div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load appointments.'} onRetry={() => void list.reload()} /> : null}

      {view === 'week' && list.data ? (
        <div className="card" style={{ overflow: 'auto' }}>
          <div className="cal">
            <div className="cal-head">Day</div>
            {days.map((day) => <div key={day} className="cal-head">{formatDate(day)}</div>)}
            <div className="cal-cell muted">Schedule</div>
            {days.map((day) => (
              <div key={day} className="cal-cell">
                {(byDay[day] ?? []).map((a) => (
                  <button key={a.appointmentId} type="button" className="appt-chip" onClick={() => void openEdit(a)}>
                    {formatTime(a.startTime)} {a.patientName}
                    <div className="muted">{a.status.replaceAll('_', ' ')}</div>
                  </button>
                ))}
              </div>
            ))}
          </div>
        </div>
      ) : null}

      {view === 'list' && list.data ? (
        <div className="card">
          {list.data.items.length === 0 ? <EmptyState title="No appointments" text="Nothing in this range." /> : (
            <div className="table-wrap">
              <table className="data">
                <thead><tr><th>Date</th><th>Time</th><th>Patient</th><th>Doctor</th><th>Status</th><th></th></tr></thead>
                <tbody>
                  {list.data.items.map((a) => (
                    <tr key={a.appointmentId}>
                      <td>{formatDate(a.appointmentDate)}</td>
                      <td>{formatTime(a.startTime)} – {formatTime(a.endTime)}</td>
                      <td><Link to={`/patients/${a.patientId}`}>{a.patientName}</Link></td>
                      <td>{a.doctorName}</td>
                      <td><StatusBadge status={a.status} /></td>
                      <td>
                        <div className="row">
                          {canMutate(a.status) ? <Button size="sm" variant="ghost" type="button" onClick={() => void openEdit(a)}>Edit</Button> : null}
                          {a.status === 'SCHEDULED' ? <Button size="sm" variant="ghost" type="button" onClick={() => void appointmentsApi.confirm(a.appointmentId).then(() => { toast.push('Confirmed'); void list.reload() })}>Confirm</Button> : null}
                          {canMutate(a.status) ? <Button size="sm" variant="ghost" type="button" onClick={() => void appointmentsApi.complete(a.appointmentId).then(() => { toast.push('Completed'); void list.reload() })}>Complete</Button> : null}
                          {canMutate(a.status) ? <Button size="sm" variant="ghost" type="button" onClick={() => setAction({ type: 'cancel', id: a.appointmentId })}>Cancel</Button> : null}
                          {canMutate(a.status) ? <Button size="sm" variant="ghost" type="button" onClick={() => setAction({ type: 'no-show', id: a.appointmentId })}>No-show</Button> : null}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          <div className="card-pad"><Pagination page={list.data.page} totalPages={list.data.totalPages} totalCount={list.data.totalCount} onPage={setPage} /></div>
        </div>
      ) : null}

      {open ? (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
            <header><h2>{editing ? 'Edit appointment' : 'New appointment'}</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field">
                <label>Patient</label>
                <input className="control" value={patientSearch} onChange={(e) => setPatientSearch(e.target.value)} placeholder="Search patient" />
                <div className="muted">{form.patientId ? `Selected #${form.patientId}` : 'Type to search'}</div>
                {patients.data?.items.map((p) => (
                  <Button key={p.patientId} type="button" variant="ghost" size="sm" onClick={() => { setForm({ ...form, patientId: String(p.patientId) }); setPatientSearch(p.fullName) }}>{p.fullName} ({p.patientNumber})</Button>
                ))}
              </div>
              {!isDoctor ? (
                <div className="field">
                  <label>Doctor</label>
                  <select className="control" value={form.doctorId} onChange={(e) => setForm({ ...form, doctorId: e.target.value })} required>
                    <option value="">Select doctor</option>
                    {doctors.data?.items.map((d) => <option key={d.doctorId} value={d.doctorId}>{d.fullName}</option>)}
                  </select>
                </div>
              ) : <p className="muted">Assigned to your doctor profile automatically.</p>}
              <div className="field"><label>Date</label><input className="control" type="date" required value={form.appointmentDate} onChange={(e) => setForm({ ...form, appointmentDate: e.target.value })} /></div>
              <div className="form-grid">
                <div className="field"><label>Start</label><input className="control" type="time" step={1800} required value={form.startTime} onChange={(e) => setForm({ ...form, startTime: e.target.value })} /></div>
                <div className="field"><label>End</label><input className="control" type="time" required value={form.endTime} onChange={(e) => setForm({ ...form, endTime: e.target.value })} /></div>
              </div>
              <div className="field"><label>Reason</label><input className="control" value={form.reason} onChange={(e) => setForm({ ...form, reason: e.target.value })} /></div>
              <div className="field"><label>Notes</label><textarea className="control" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} /></div>
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save'}</Button>
            </footer>
          </form>
        </div>
      ) : null}

      {action ? (
        <ConfirmDialog
          title={action.type === 'cancel' ? 'Cancel appointment' : 'Mark no-show'}
          message="This frees the time slot. Completed, cancelled, and no-show states cannot be changed again."
          confirmLabel="Confirm"
          danger
          onCancel={() => setAction(null)}
          onConfirm={async () => {
            if (action.type === 'cancel') await appointmentsApi.cancel(action.id)
            else await appointmentsApi.noShow(action.id)
            toast.push('Appointment updated.')
            setAction(null)
            void list.reload()
          }}
        />
      ) : null}
    </div>
  )
}
