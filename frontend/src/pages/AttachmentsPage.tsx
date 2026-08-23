import { useState } from 'react'
import { Link } from 'react-router-dom'
import { attachmentsApi, patientsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, EmptyState, PageHeader } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { AttachmentDto } from '@/types/api'
import { fileSize, formatDateTime } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

export function AttachmentsPage() {
  const { t } = useI18n()
  const canWrite = useCan([Role.Admin, Role.Secretary])
  const toast = useToast()
  const [search, setSearch] = useState('')
  const q = useDebouncedValue(search)
  const patients = useAsync(() => q ? patientsApi.list({ search: q, pageSize: 8, isActive: true }) : Promise.resolve({ items: [], page: 1, pageSize: 8, totalCount: 0, totalPages: 0 }), [q])
  const [patientId, setPatientId] = useState<number | null>(null)
  const [patientName, setPatientName] = useState('')
  const files = useAsync(() => patientId ? attachmentsApi.listByPatient(patientId) : Promise.resolve([] as AttachmentDto[]), [patientId])

  async function download(item: AttachmentDto) {
    try {
      const { blob, fileName } = await attachmentsApi.download(item.attachmentId)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = fileName
      a.click()
      URL.revokeObjectURL(url)
    } catch (err) {
      toast.push(isApiError(err) ? err.message : 'Download failed.', 'error')
    }
  }

  return (
    <div>
      <PageHeader
        crumbs={t('att.crumbs')}
        title={t('att.title')}
        description={t('att.lede')}
      />
      <div className="card card-pad stack">
        <div className="field">
          <label>Find patient</label>
          <input className="control" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search patients" />
        </div>
        {patients.data?.items.map((p) => (
          <Button key={p.patientId} type="button" variant="ghost" onClick={() => { setPatientId(p.patientId); setPatientName(p.fullName) }}>
            {p.fullName} ({p.patientNumber})
          </Button>
        ))}
      </div>
      {patientId ? (
        <div className="card" style={{ marginTop: 16 }}>
          <div className="card-pad row" style={{ justifyContent: 'space-between' }}>
            <h2>{patientName}</h2>
            <Link to={`/patients/${patientId}`}>Open profile</Link>
          </div>
          {files.data?.length === 0 ? <EmptyState title="No attachments" text="Upload from the patient profile. Public /uploads URLs are not used." /> : (
            <div className="table-wrap">
              <table className="data">
                <thead><tr><th>File</th><th>Type</th><th>Size</th><th>Uploaded</th><th></th></tr></thead>
                <tbody>
                  {files.data?.map((item) => (
                    <tr key={item.attachmentId}>
                      <td>{item.fileName}</td>
                      <td>{item.fileType || '—'}</td>
                      <td>{fileSize(item.fileSize)}</td>
                      <td>{formatDateTime(item.createdAt)}</td>
                      <td>
                        <div className="row">
                          <Button size="sm" variant="ghost" type="button" onClick={() => void download(item)}>Download</Button>
                          {canWrite ? <Button size="sm" variant="danger" type="button" onClick={async () => {
                            await attachmentsApi.remove(item.attachmentId)
                            toast.push('Attachment deleted.')
                            void files.reload()
                          }}>Delete</Button> : null}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      ) : null}
    </div>
  )
}
