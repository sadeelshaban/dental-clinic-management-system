import { useState } from 'react'
import { doctorsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useI18n } from '@/i18n/I18nContext'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { Button, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { DoctorDetailDto } from '@/types/api'

export function DoctorsPage() {
  const { t } = useI18n()
  const canWrite = useCan([Role.Admin])
  const toast = useToast()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const q = useDebouncedValue(search)
  const list = useAsync(() => doctorsApi.list({ search: q || undefined, page, pageSize: 20, isActive: null }), [q, page])
  const [editing, setEditing] = useState<DoctorDetailDto | null>(null)
  const [form, setForm] = useState({ licenseNumber: '', specialization: '', bio: '' })

  return (
    <div>
      <PageHeader crumbs={t('docs.crumbs')} title={t('docs.title')} description={t('docs.lede')} />
      <div className="toolbar"><input className="control" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search name, email, specialization, license" /></div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load doctors.'} /> : null}
      {list.data?.items.length === 0 ? <EmptyState title="No doctors" text="Create a user with the DOCTOR role to add a profile." /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Name</th><th>Specialization</th><th>License</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {list.data.items.map((d) => (
                  <tr key={d.doctorId}>
                    <td>{d.fullName}<div className="muted">{d.email}</div></td>
                    <td>{d.specialization || '—'}</td>
                    <td>{d.licenseNumber || '—'}</td>
                    <td><StatusBadge status={d.isActive ? 'ACTIVE' : 'INACTIVE'} /></td>
                    <td>{canWrite ? <Button size="sm" variant="ghost" type="button" onClick={async () => {
                      const detail = await doctorsApi.get(d.doctorId)
                      setEditing(detail)
                      setForm({ licenseNumber: detail.licenseNumber ?? '', specialization: detail.specialization ?? '', bio: detail.bio ?? '' })
                    }}>Edit profile</Button> : null}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="card-pad"><Pagination page={list.data.page} totalPages={list.data.totalPages} totalCount={list.data.totalCount} onPage={setPage} /></div>
        </div>
      ) : null}
      {editing && canWrite ? (
        <div className="modal-backdrop" onClick={() => setEditing(null)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={async (e) => {
            e.preventDefault()
            await doctorsApi.update(editing.doctorId, {
              licenseNumber: form.licenseNumber || null,
              specialization: form.specialization || null,
              bio: form.bio || null,
            })
            toast.push('Doctor profile updated.')
            setEditing(null)
            void list.reload()
          }}>
            <header><h2>{editing.fullName}</h2></header>
            <div className="body stack">
              <div className="field"><label>License</label><input className="control" value={form.licenseNumber} onChange={(e) => setForm({ ...form, licenseNumber: e.target.value })} /></div>
              <div className="field"><label>Specialization</label><input className="control" value={form.specialization} onChange={(e) => setForm({ ...form, specialization: e.target.value })} /></div>
              <div className="field"><label>Bio</label><textarea className="control" value={form.bio} onChange={(e) => setForm({ ...form, bio: e.target.value })} /></div>
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setEditing(null)}>Cancel</Button>
              <Button type="submit">Save</Button>
            </footer>
          </form>
        </div>
      ) : null}
    </div>
  )
}
