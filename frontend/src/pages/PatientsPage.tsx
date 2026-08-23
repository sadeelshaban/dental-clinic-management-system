import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { patientsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, ConfirmDialog, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { CreatePatientRequest, PatientDetailDto, PatientListItemDto } from '@/types/api'
import { formatDate } from '@/utils/format'
import { useI18n, useGenderLabel } from '@/i18n/I18nContext'

const emptyForm: CreatePatientRequest = {
  firstName: '',
  lastName: '',
  phone: '',
  email: '',
  dateOfBirth: '',
  gender: 'UNKNOWN',
  nationalId: '',
  address: '',
  emergencyContactName: '',
  emergencyContactPhone: '',
  medicalAlerts: '',
  allergies: '',
  medications: '',
  medicalHistory: '',
  notes: '',
}

export function PatientsPage() {
  const canWrite = useCan([Role.Admin, Role.Secretary])
  const { t } = useI18n()
  const genderLabel = useGenderLabel()
  const [params, setParams] = useSearchParams()
  const [search, setSearch] = useState('')
  const [active, setActive] = useState<'true' | 'false' | 'all'>('true')
  const [page, setPage] = useState(1)
  const debounced = useDebouncedValue(search)
  const navigate = useNavigate()
  const toast = useToast()
  const [formOpen, setFormOpen] = useState(params.get('new') === '1')
  const [editing, setEditing] = useState<PatientDetailDto | null>(null)
  const [form, setForm] = useState<CreatePatientRequest>(emptyForm)
  const [busy, setBusy] = useState(false)
  const [formError, setFormError] = useState('')
  const [deactivateId, setDeactivateId] = useState<number | null>(null)

  const query = useAsync(
    () => patientsApi.list({
      search: debounced || undefined,
      isActive: active === 'all' ? null : active === 'true',
      page,
      pageSize: 20,
    }),
    [debounced, active, page],
  )

  useEffect(() => {
    if (params.get('new') === '1') setFormOpen(true)
  }, [params])

  function openCreate() {
    setEditing(null)
    setForm(emptyForm)
    setFormError('')
    setFormOpen(true)
  }

  async function openEdit(patient: PatientListItemDto) {
    const detail = await patientsApi.get(patient.patientId)
    setEditing(detail)
    setForm({
      firstName: detail.firstName,
      lastName: detail.lastName,
      phone: detail.phone ?? '',
      email: detail.email ?? '',
      dateOfBirth: detail.dateOfBirth ?? '',
      gender: detail.gender || 'UNKNOWN',
      nationalId: detail.nationalId ?? '',
      address: detail.address ?? '',
      emergencyContactName: detail.emergencyContactName ?? '',
      emergencyContactPhone: detail.emergencyContactPhone ?? '',
      medicalAlerts: detail.medicalAlerts ?? '',
      allergies: detail.allergies ?? '',
      medications: detail.medications ?? '',
      medicalHistory: detail.medicalHistory ?? '',
      notes: detail.notes ?? '',
    })
    setFormOpen(true)
  }

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    if (!form.firstName.trim() || !form.lastName.trim()) {
      setFormError(t('patients.needName'))
      return
    }
    setBusy(true)
    setFormError('')
    const payload: CreatePatientRequest = {
      ...form,
      phone: form.phone || null,
      email: form.email || null,
      dateOfBirth: form.dateOfBirth || null,
      nationalId: form.nationalId || null,
      address: form.address || null,
      emergencyContactName: form.emergencyContactName || null,
      emergencyContactPhone: form.emergencyContactPhone || null,
      medicalAlerts: form.medicalAlerts || null,
      allergies: form.allergies || null,
      medications: form.medications || null,
      medicalHistory: form.medicalHistory || null,
      notes: form.notes || null,
    }
    try {
      if (editing) {
        const saved = await patientsApi.update(editing.patientId, payload)
        toast.push(t('patients.updated'))
        setFormOpen(false)
        navigate(`/patients/${saved.patientId}`)
      } else {
        const saved = await patientsApi.create(payload)
        toast.push(t('patients.created'))
        setFormOpen(false)
        setParams({})
        navigate(`/patients/${saved.patientId}`)
      }
    } catch (err) {
      setFormError(isApiError(err) ? err.message : t('patients.saveFailed'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div>
      <PageHeader
        crumbs={t('patients.crumbs')}
        title={t('patients.title')}
        description={t('patients.lede')}
        actions={canWrite ? <Button type="button" onClick={openCreate}>{t('patients.add')}</Button> : null}
      />
      <div className="toolbar">
        <input className="control" value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} placeholder={t('patients.search')} />
        <select className="control" value={active} onChange={(e) => { setActive(e.target.value as typeof active); setPage(1) }}>
          <option value="true">{t('common.active')}</option>
          <option value="false">{t('common.inactive')}</option>
          <option value="all">{t('common.all')}</option>
        </select>
      </div>
      {query.loading ? <LoadingSkeleton /> : null}
      {query.error ? <ErrorState text={isApiError(query.error) ? query.error.message : t('common.unableLoad')} onRetry={() => void query.reload()} /> : null}
      {query.data && query.data.items.length === 0 ? (
        <EmptyState title={t('patients.emptyTitle')} text={t('patients.emptyText')} />
      ) : null}
      {query.data && query.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap desktop-table">
            <table className="data">
              <thead>
                <tr>
                  <th>{t('patients.number')}</th>
                  <th>{t('patients.name')}</th>
                  <th>{t('patients.phone')}</th>
                  <th>{t('patients.gender')}</th>
                  <th>{t('patients.registered')}</th>
                  <th>{t('patients.status')}</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {query.data.items.map((p) => (
                  <tr key={p.patientId}>
                    <td>{p.patientNumber}</td>
                    <td><Link to={`/patients/${p.patientId}`}>{p.fullName}</Link></td>
                    <td>{p.phone || '—'}</td>
                    <td>{genderLabel(p.gender)}</td>
                    <td>{formatDate(p.createdAt)}</td>
                    <td><StatusBadge status={p.isActive ? 'ACTIVE' : 'INACTIVE'} /></td>
                    <td>
                      <div className="row">
                        <Link className="btn btn-ghost btn-sm" to={`/patients/${p.patientId}`}>{t('common.open')}</Link>
                        {canWrite ? <Button variant="ghost" size="sm" type="button" onClick={() => void openEdit(p)}>{t('common.edit')}</Button> : null}
                        {canWrite && p.isActive ? <Button variant="danger" size="sm" type="button" onClick={() => setDeactivateId(p.patientId)}>{t('patients.deactivate')}</Button> : null}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="list-cards card-pad">
            {query.data.items.map((p) => (
              <Link key={p.patientId} to={`/patients/${p.patientId}`} className="card card-pad">
                <strong>{p.fullName}</strong>
                <div className="muted">{p.patientNumber} · {p.phone || t('patients.noPhone')}</div>
              </Link>
            ))}
          </div>
          <div className="card-pad">
            <Pagination page={query.data.page} totalPages={query.data.totalPages} totalCount={query.data.totalCount} onPage={setPage} />
          </div>
        </div>
      ) : null}

      {formOpen ? (
        <div className="modal-backdrop" onClick={() => setFormOpen(false)} role="presentation">
          <form className="modal patient-modal" onClick={(e) => e.stopPropagation()} onSubmit={onSubmit}>
            <header><h2>{editing ? t('patients.edit') : t('patients.new')}</h2></header>
            <div className="body">
              {formError ? <div className="form-error">{formError}</div> : null}

              <section className="form-section">
                <h3 className="form-section-title">{t('patients.identity')}</h3>
                <p className="form-section-hint">{t('patients.identityHint')}</p>
                <div className="form-grid">
                  <div className="field">
                    <label>{t('patients.firstName')}<span className="req">*</span></label>
                    <input className="control" required value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.lastName')}<span className="req">*</span></label>
                    <input className="control" required value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.phone')}</label>
                    <input className="control" value={form.phone ?? ''} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.dob')}</label>
                    <input className="control" type="date" value={form.dateOfBirth ?? ''} onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })} />
                  </div>
                  <div className="field span-2">
                    <label>{t('patients.gender')}</label>
                    <select className="control" value={form.gender} onChange={(e) => setForm({ ...form, gender: e.target.value })}>
                      <option value="UNKNOWN">{t('gender.UNKNOWN')}</option>
                      <option value="MALE">{t('gender.MALE')}</option>
                      <option value="FEMALE">{t('gender.FEMALE')}</option>
                      <option value="OTHER">{t('gender.OTHER')}</option>
                    </select>
                  </div>
                </div>
              </section>

              <section className="form-section">
                <h3 className="form-section-title">{t('patients.contact')}</h3>
                <div className="form-grid">
                  <div className="field span-2">
                    <label>{t('patients.address')}</label>
                    <input className="control" value={form.address ?? ''} onChange={(e) => setForm({ ...form, address: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.emergency')}</label>
                    <input className="control" value={form.emergencyContactName ?? ''} onChange={(e) => setForm({ ...form, emergencyContactName: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.emergencyPhone')}</label>
                    <input className="control" value={form.emergencyContactPhone ?? ''} onChange={(e) => setForm({ ...form, emergencyContactPhone: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.email')}</label>
                    <input className="control" type="text" inputMode="email" autoComplete="email" value={form.email ?? ''} onChange={(e) => setForm({ ...form, email: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.nationalId')}</label>
                    <input className="control" value={form.nationalId ?? ''} onChange={(e) => setForm({ ...form, nationalId: e.target.value })} />
                  </div>
                </div>
              </section>

              <section className="form-section">
                <h3 className="form-section-title">{t('patients.clinical')}</h3>
                <div className="form-grid">
                  <div className="field">
                    <label>{t('patients.allergies')}</label>
                    <textarea className="control compact" value={form.allergies ?? ''} onChange={(e) => setForm({ ...form, allergies: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.medications')}</label>
                    <textarea className="control compact" value={form.medications ?? ''} onChange={(e) => setForm({ ...form, medications: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.alerts')}</label>
                    <textarea className="control compact" value={form.medicalAlerts ?? ''} onChange={(e) => setForm({ ...form, medicalAlerts: e.target.value })} />
                  </div>
                  <div className="field">
                    <label>{t('patients.history')}</label>
                    <textarea className="control compact" value={form.medicalHistory ?? ''} onChange={(e) => setForm({ ...form, medicalHistory: e.target.value })} />
                  </div>
                  <div className="field span-2">
                    <label>{t('patients.notes')}</label>
                    <textarea className="control compact" value={form.notes ?? ''} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
                  </div>
                </div>
              </section>
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setFormOpen(false)}>{t('common.cancel')}</Button>
              <Button type="submit" disabled={busy}>{busy ? t('common.saving') : t('patients.save')}</Button>
            </footer>
          </form>
        </div>
      ) : null}

      {deactivateId ? (
        <ConfirmDialog
          title={t('patients.deactivateTitle')}
          message={t('patients.deactivateMsg')}
          confirmLabel={t('patients.deactivate')}
          danger
          onCancel={() => setDeactivateId(null)}
          onConfirm={async () => {
            try {
              await patientsApi.deactivate(deactivateId)
              toast.push(t('patients.deactivated'))
              setDeactivateId(null)
              void query.reload()
            } catch (err) {
              toast.push(isApiError(err) ? err.message : 'Unable to deactivate.', 'error')
            }
          }}
        />
      ) : null}
    </div>
  )
}
