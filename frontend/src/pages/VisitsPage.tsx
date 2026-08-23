import { useEffect, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { doctorsApi, patientsApi, visitsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useAuth, useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { CreateVisitRequest, VisitDetailDto } from '@/types/api'
import { formatDateTime, fromDateTimeLocal, localDateTimeValue } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

export function VisitsPage() {
  const { user } = useAuth()
  const { t } = useI18n()
  const canWrite = useCan([Role.Admin, Role.Doctor])
  const isDoctor = user?.role === Role.Doctor
  const isAdmin = user?.role === Role.Admin
  const toast = useToast()
  const [params] = useSearchParams()
  const [page, setPage] = useState(1)
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [open, setOpen] = useState(params.get('new') === '1')
  const [editing, setEditing] = useState<VisitDetailDto | null>(null)
  const [patientSearch, setPatientSearch] = useState('')
  const patientQ = useDebouncedValue(patientSearch)
  const patients = useAsync(() => patientQ ? patientsApi.list({ search: patientQ, pageSize: 8, isActive: true }) : Promise.resolve({ items: [], page: 1, pageSize: 8, totalCount: 0, totalPages: 0 }), [patientQ])
  const doctors = useAsync(() => doctorsApi.list({ isActive: true, pageSize: 100 }), [])
  const [form, setForm] = useState({ patientId: params.get('patientId') || '', doctorId: '', visitDate: localDateTimeValue(), chiefComplaint: '', diagnosis: '', clinicalNotes: '', followUpDate: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const list = useAsync(() => visitsApi.list({ from: from || undefined, to: to || undefined, page, pageSize: 20 }), [from, to, page])

  useEffect(() => { if (params.get('new') === '1') setOpen(true) }, [params])

  async function submit(event: FormEvent) {
    event.preventDefault()
    if (!form.patientId) { setError('Select a patient.'); return }
    setBusy(true); setError('')
    const body: CreateVisitRequest = {
      patientId: Number(form.patientId),
      doctorId: isDoctor || !form.doctorId ? null : Number(form.doctorId),
      visitDate: fromDateTimeLocal(form.visitDate),
      chiefComplaint: form.chiefComplaint || null,
      diagnosis: form.diagnosis || null,
      clinicalNotes: form.clinicalNotes || null,
      followUpDate: form.followUpDate || null,
    }
    try {
      if (editing) {
        await visitsApi.update(editing.visitId, {
          doctorId: isAdmin && form.doctorId ? Number(form.doctorId) : null,
          visitDate: fromDateTimeLocal(form.visitDate),
          chiefComplaint: form.chiefComplaint || null,
          diagnosis: form.diagnosis || null,
          clinicalNotes: form.clinicalNotes || null,
          followUpDate: form.followUpDate || null,
        })
        toast.push('Visit updated.')
      } else {
        await visitsApi.create(body)
        toast.push('Visit recorded.')
      }
      setOpen(false); setEditing(null); void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to save visit.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <PageHeader
        crumbs={t('visits.crumbs')}
        title={t('visits.title')}
        description={t('visits.lede')}
        actions={canWrite ? <Button type="button" onClick={() => { setEditing(null); setOpen(true) }}>{t('visits.new')}</Button> : null}
      />
      <div className="toolbar">
        <input className="control" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        <input className="control" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
      </div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load visits.'} onRetry={() => void list.reload()} /> : null}
      {list.data?.items.length === 0 ? <EmptyState title="No visits" text="Clinical encounters will appear here." /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>When</th><th>Patient</th><th>Doctor</th><th>Complaint</th><th></th></tr></thead>
              <tbody>
                {list.data.items.map((v) => (
                  <tr key={v.visitId}>
                    <td>{formatDateTime(v.visitDate)}</td>
                    <td><Link to={`/patients/${v.patientId}`}>{v.patientName}</Link></td>
                    <td>{v.doctorName}</td>
                    <td>{v.chiefComplaint || '—'}</td>
                    <td>{canWrite ? <Button size="sm" variant="ghost" type="button" onClick={async () => {
                      const d = await visitsApi.get(v.visitId)
                      setEditing(d)
                      setForm({
                        patientId: String(d.patientId),
                        doctorId: String(d.doctorId),
                        visitDate: localDateTimeValue(d.visitDate),
                        chiefComplaint: d.chiefComplaint ?? '',
                        diagnosis: d.diagnosis ?? '',
                        clinicalNotes: d.clinicalNotes ?? '',
                        followUpDate: d.followUpDate ?? '',
                      })
                      setPatientSearch(d.patientName)
                      setOpen(true)
                    }}>Open</Button> : null}</td>
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
            <header><h2>{editing ? 'Edit visit' : 'New visit'}</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field">
                <label>Patient</label>
                <input className="control" disabled={Boolean(editing)} value={patientSearch} onChange={(e) => setPatientSearch(e.target.value)} placeholder="Search patient" />
                {patients.data?.items.map((p) => (
                  <Button key={p.patientId} type="button" size="sm" variant="ghost" onClick={() => { setForm({ ...form, patientId: String(p.patientId) }); setPatientSearch(p.fullName) }}>{p.fullName}</Button>
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
              ) : null}
              <div className="field"><label>Visit date and time</label><input className="control" type="datetime-local" required value={form.visitDate} onChange={(e) => setForm({ ...form, visitDate: e.target.value })} /></div>
              <div className="field"><label>Chief complaint</label><textarea className="control" value={form.chiefComplaint} onChange={(e) => setForm({ ...form, chiefComplaint: e.target.value })} /></div>
              <div className="field"><label>Diagnosis</label><textarea className="control" value={form.diagnosis} onChange={(e) => setForm({ ...form, diagnosis: e.target.value })} /></div>
              <div className="field"><label>Clinical notes</label><textarea className="control" value={form.clinicalNotes} onChange={(e) => setForm({ ...form, clinicalNotes: e.target.value })} /></div>
              <div className="field"><label>Follow-up date</label><input className="control" type="date" value={form.followUpDate} onChange={(e) => setForm({ ...form, followUpDate: e.target.value })} /></div>
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save visit'}</Button>
            </footer>
          </form>
        </div>
      ) : null}
    </div>
  )
}
