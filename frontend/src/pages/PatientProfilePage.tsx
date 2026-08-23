import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  appointmentsApi,
  attachmentsApi,
  patientTreatmentsApi,
  patientsApi,
  paymentsApi,
  visitsApi,
} from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, ConfirmDialog, EmptyState, ErrorState, LoadingSkeleton, PageHeader, StatusBadge, Tabs } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync } from '@/hooks/useAsync'
import type { AttachmentDto } from '@/types/api'
import { fileSize, formatDate, formatDateTime, formatTime, initials, money } from '@/utils/format'
import { useI18n, useGenderLabel } from '@/i18n/I18nContext'

export function PatientProfilePage() {
  const { patientId } = useParams()
  const id = Number(patientId)
  const toast = useToast()
  const { t } = useI18n()
  const genderLabel = useGenderLabel()
  const canWrite = useCan([Role.Admin, Role.Secretary])
  const canClinical = useCan([Role.Admin, Role.Doctor])
  const canPay = useCan([Role.Admin, Role.Secretary])
  const [tab, setTab] = useState('overview')
  const [deactivate, setDeactivate] = useState(false)
  const [preview, setPreview] = useState<{ url: string; name: string; type: string } | null>(null)
  const [deleteAtt, setDeleteAtt] = useState<AttachmentDto | null>(null)

  const patientQ = useAsync(() => patientsApi.get(id), [id])
  const financialQ = useAsync(() => patientsApi.financial(id), [id])
  const apptQ = useAsync(() => appointmentsApi.list({ patientId: id, page: 1, pageSize: 50 }), [id])
  const visitQ = useAsync(() => visitsApi.list({ patientId: id, page: 1, pageSize: 50 }), [id])
  const treatQ = useAsync(() => patientTreatmentsApi.list({ patientId: id, page: 1, pageSize: 50 }), [id])
  const payQ = useAsync(() => paymentsApi.list({ patientId: id, page: 1, pageSize: 50 }), [id])
  const attQ = useAsync(() => attachmentsApi.listByPatient(id), [id])

  useEffect(() => () => { if (preview?.url) URL.revokeObjectURL(preview.url) }, [preview])

  const patient = patientQ.data
  if (patientQ.loading && !patient) return <LoadingSkeleton rows={8} />
  if (patientQ.error) return <ErrorState text={isApiError(patientQ.error) ? patientQ.error.message : 'Patient not found.'} />
  if (!patient) return <EmptyState title="Patient not found" text="This record is not available in your clinic." />

  async function openAttachment(item: AttachmentDto) {
    const { blob, contentType, fileName } = await attachmentsApi.download(item.attachmentId)
    const url = URL.createObjectURL(blob)
    if (contentType.startsWith('image/') || contentType === 'application/pdf') {
      if (preview?.url) URL.revokeObjectURL(preview.url)
      setPreview({ url, name: fileName, type: contentType })
    } else {
      const a = document.createElement('a')
      a.href = url
      a.download = fileName
      a.click()
      URL.revokeObjectURL(url)
    }
  }

  return (
    <div>
      <PageHeader crumbs={t('profile.crumbs')} title={patient.fullName} description={`${patient.patientNumber} · ${genderLabel(patient.gender)}`} />
      <section className="card card-pad profile-hero" style={{ marginBottom: 18 }}>
        <div className="avatar-lg">{initials(patient.fullName)}</div>
        <div>
          <div className="row">
            <StatusBadge status={patient.isActive ? 'ACTIVE' : 'INACTIVE'} />
            <span className="muted">{patient.phone || 'No phone'} · {patient.email || 'No email'}</span>
          </div>
          {patient.medicalAlerts ? <p style={{ marginTop: 8, color: '#b42318' }}>Alert: {patient.medicalAlerts}</p> : null}
        </div>
        <div className="row">
          {canWrite ? <Link className="btn btn-ghost" to={`/patients?edit=${patient.patientId}`}>Edit</Link> : null}
          <Link className="btn btn-ghost" to={`/appointments?patientId=${patient.patientId}&new=1`}>Book</Link>
          {canClinical ? <Link className="btn btn-ghost" to={`/visits?patientId=${patient.patientId}&new=1`}>Visit</Link> : null}
          {canPay ? <Link className="btn btn-primary" to={`/payments?patientId=${patient.patientId}&new=1`}>Record payment</Link> : null}
        </div>
      </section>

      <div className="grid grid-4" style={{ marginBottom: 18 }}>
        <article className="card stat"><div className="stat-label">Treatment value</div><div className="stat-value">{money(financialQ.data?.totalTreatments)}</div><div className="stat-hint">Not revenue</div></article>
        <article className="card stat stat-revenue"><div className="stat-label">Collected</div><div className="stat-value">{money(financialQ.data?.totalPaid)}</div><div className="stat-hint">Valid payments</div></article>
        <article className="card stat stat-out"><div className="stat-label">Outstanding</div><div className="stat-value">{money(financialQ.data?.totalRemaining)}</div><div className="stat-hint">Remaining balance</div></article>
        <article className="card stat"><div className="stat-label">Born</div><div className="stat-value" style={{ fontSize: 22 }}>{formatDate(patient.dateOfBirth)}</div><div className="stat-hint">{patient.nationalId || 'No national ID'}</div></article>
      </div>

      <Tabs
        tabs={[
          { id: 'overview', label: t('profile.overview') },
          { id: 'clinical', label: t('profile.clinical') },
          { id: 'treatments', label: t('nav.treatments') },
          { id: 'appointments', label: t('nav.appointments') },
          { id: 'visits', label: t('nav.visits') },
          { id: 'payments', label: t('nav.payments') },
          { id: 'files', label: t('profile.files') },
        ]}
        value={tab}
        onChange={setTab}
      />

      {tab === 'overview' ? (
        <div className="grid grid-2">
          <div className="card card-pad">
            <h2>Identity</h2>
            <dl className="dl" style={{ marginTop: 12 }}>
              <dt>Address</dt><dd>{patient.address || '—'}</dd>
              <dt>Emergency</dt><dd>{patient.emergencyContactName || '—'} {patient.emergencyContactPhone || ''}</dd>
              <dt>Allergies</dt><dd>{patient.allergies || '—'}</dd>
              <dt>Medications</dt><dd>{patient.medications || '—'}</dd>
              <dt>History</dt><dd>{patient.medicalHistory || '—'}</dd>
              <dt>Notes</dt><dd>{patient.notes || '—'}</dd>
            </dl>
            {canWrite && patient.isActive ? (
              <Button variant="danger" type="button" style={{ marginTop: 16 }} onClick={() => setDeactivate(true)}>Deactivate patient</Button>
            ) : null}
          </div>
          <div className="card card-pad">
            <h2>Financial statement</h2>
            <p className="metric-note">Totals come from the patient financial view. Status is server-derived.</p>
            {(financialQ.data?.lines.length ?? 0) === 0 ? <p className="muted" style={{ marginTop: 12 }}>No treatment lines yet.</p> : (
              <div className="table-wrap" style={{ marginTop: 12 }}>
                <table className="data">
                  <thead><tr><th>Treatment</th><th>Total</th><th>Paid</th><th>Remaining</th><th>Status</th></tr></thead>
                  <tbody>
                    {financialQ.data?.lines.map((line) => (
                      <tr key={line.patientTreatmentId}>
                        <td>{line.treatmentName}</td>
                        <td>{money(line.treatmentTotal)}</td>
                        <td>{money(line.paid)}</td>
                        <td>{money(line.remaining)}</td>
                        <td><StatusBadge status={line.status} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      ) : null}

      {tab === 'clinical' ? (
        <div className="card card-pad">
          <h2>Clinical snapshot</h2>
          <p>{patient.medicalAlerts || 'No medical alerts recorded.'}</p>
          <hr className="hr" />
          <p className="muted">Visits and treatments are listed in their tabs. Catalog prices never rewrite these historical records.</p>
        </div>
      ) : null}

      {tab === 'treatments' ? (
        <div className="card">
          {treatQ.data?.items.length ? (
            <div className="table-wrap">
              <table className="data">
                <thead><tr><th>Date</th><th>Name</th><th>Doctor</th><th>Amount</th><th>Status</th></tr></thead>
                <tbody>
                  {treatQ.data.items.map((t) => (
                    <tr key={t.patientTreatmentId}>
                      <td>{formatDateTime(t.treatmentDate)}</td>
                      <td>{t.treatmentName}</td>
                      <td>{t.doctorName}</td>
                      <td>{money(t.finalAmount)}</td>
                      <td><StatusBadge status={t.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : <EmptyState title="No treatments" text="Record a performed treatment from the clinical workspace." />}
        </div>
      ) : null}

      {tab === 'appointments' ? (
        <div className="card">
          {apptQ.data?.items.length ? (
            <div className="table-wrap">
              <table className="data">
                <thead><tr><th>Date</th><th>Time</th><th>Doctor</th><th>Status</th></tr></thead>
                <tbody>
                  {apptQ.data.items.map((a) => (
                    <tr key={a.appointmentId}>
                      <td>{formatDate(a.appointmentDate)}</td>
                      <td>{formatTime(a.startTime)} – {formatTime(a.endTime)}</td>
                      <td>{a.doctorName}</td>
                      <td><StatusBadge status={a.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : <EmptyState title="No appointments" text="Nothing scheduled for this patient." />}
        </div>
      ) : null}

      {tab === 'visits' ? (
        <div className="card">
          {visitQ.data?.items.length ? (
            <div className="table-wrap">
              <table className="data">
                <thead><tr><th>Date</th><th>Doctor</th><th>Complaint</th></tr></thead>
                <tbody>
                  {visitQ.data.items.map((v) => (
                    <tr key={v.visitId}>
                      <td>{formatDateTime(v.visitDate)}</td>
                      <td>{v.doctorName}</td>
                      <td>{v.chiefComplaint || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : <EmptyState title="No visits" text="Clinical encounters will appear here." />}
        </div>
      ) : null}

      {tab === 'payments' ? (
        <div className="card">
          {payQ.data?.items.length ? (
            <div className="table-wrap">
              <table className="data">
                <thead><tr><th>Date</th><th>Treatment</th><th>Amount</th><th>Method</th><th>State</th></tr></thead>
                <tbody>
                  {payQ.data.items.map((p) => (
                    <tr key={p.paymentId}>
                      <td>{formatDateTime(p.paymentDate)}</td>
                      <td>{p.treatmentName}</td>
                      <td>{money(p.amount)}</td>
                      <td>{p.method}</td>
                      <td><StatusBadge status={p.isVoided ? 'VOIDED' : 'VALID'} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : <EmptyState title="No payments" text="Collected payments against this patient will appear here." />}
        </div>
      ) : null}

      {tab === 'files' ? (
        <AttachmentsPanel
          items={attQ.data ?? []}
          loading={attQ.loading}
          canWrite={canWrite}
          patientId={id}
          onUploaded={() => void attQ.reload()}
          onOpen={(item) => void openAttachment(item).catch((err) => toast.push(isApiError(err) ? err.message : 'Download failed.', 'error'))}
          onDelete={setDeleteAtt}
        />
      ) : null}

      {preview ? (
        <div className="modal-backdrop" onClick={() => { URL.revokeObjectURL(preview.url); setPreview(null) }}>
          <div className="modal modal-lg" onClick={(e) => e.stopPropagation()}>
            <header><h2>{preview.name}</h2></header>
            <div className="body">
              {preview.type.startsWith('image/') ? <img src={preview.url} alt={preview.name} /> : (
                <iframe title={preview.name} src={preview.url} style={{ width: '100%', height: 480, border: 0 }} />
              )}
              <p className="muted" style={{ marginTop: 8 }}>Preview uses your signed-in session. Files are not served from public folders.</p>
            </div>
          </div>
        </div>
      ) : null}

      {deactivate ? (
        <ConfirmDialog
          title="Deactivate patient"
          message="Inactive patients cannot receive new appointments."
          confirmLabel="Deactivate"
          danger
          onCancel={() => setDeactivate(false)}
          onConfirm={async () => {
            await patientsApi.deactivate(patient.patientId)
            toast.push('Patient deactivated.')
            setDeactivate(false)
            void patientQ.reload()
          }}
        />
      ) : null}

      {deleteAtt ? (
        <ConfirmDialog
          title="Delete attachment"
          message={`Delete ${deleteAtt.fileName}? This cannot be undone.`}
          confirmLabel="Delete"
          danger
          onCancel={() => setDeleteAtt(null)}
          onConfirm={async () => {
            await attachmentsApi.remove(deleteAtt.attachmentId)
            toast.push('Attachment deleted.')
            setDeleteAtt(null)
            void attQ.reload()
          }}
        />
      ) : null}
    </div>
  )
}

function AttachmentsPanel({
  items,
  loading,
  canWrite,
  patientId,
  onUploaded,
  onOpen,
  onDelete,
}: {
  items: AttachmentDto[]
  loading: boolean
  canWrite: boolean
  patientId: number
  onUploaded: () => void
  onOpen: (item: AttachmentDto) => void
  onDelete: (item: AttachmentDto) => void
}) {
  const toast = useToast()
  const [busy, setBusy] = useState(false)
  async function upload(file: File) {
    setBusy(true)
    try {
      await attachmentsApi.upload(file, patientId)
      toast.push('File uploaded.')
      onUploaded()
    } catch (err) {
      toast.push(isApiError(err) ? err.message : 'Upload failed.', 'error')
    } finally {
      setBusy(false)
    }
  }
  return (
    <div className="card card-pad">
      <div className="row" style={{ justifyContent: 'space-between' }}>
        <h2>Attachments</h2>
        {canWrite ? (
          <label className="btn btn-ghost">
            {busy ? 'Uploading...' : 'Upload file'}
            <input type="file" accept="image/*,application/pdf" hidden disabled={busy} onChange={(e) => {
              const file = e.target.files?.[0]
              if (file) void upload(file)
              e.target.value = ''
            }} />
          </label>
        ) : null}
      </div>
      <p className="metric-note">Files are never served from public /uploads paths. Preview and download use the authenticated API.</p>
      {loading ? <LoadingSkeleton /> : null}
      {!loading && items.length === 0 ? <EmptyState title="No files" text="PDF and image files can be attached to this patient." /> : null}
      <div className="table-wrap" style={{ marginTop: 12 }}>
        <table className="data">
          <thead><tr><th>File</th><th>Type</th><th>Size</th><th>Uploaded</th><th></th></tr></thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.attachmentId}>
                <td>{item.fileName}</td>
                <td>{item.fileType || '—'}</td>
                <td>{fileSize(item.fileSize)}</td>
                <td>{formatDateTime(item.createdAt)}</td>
                <td>
                  <div className="row">
                    <Button size="sm" variant="ghost" type="button" onClick={() => onOpen(item)}>View</Button>
                    {canWrite ? <Button size="sm" variant="danger" type="button" onClick={() => onDelete(item)}>Delete</Button> : null}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
