import { useState, type FormEvent } from 'react'
import { usersApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { Role } from '@/auth/roles'
import { Button, ConfirmDialog, EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import type { CreateUserRequest, UserDetailDto } from '@/types/api'
import { formatDateTime } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

export function UsersPage() {
  const { t } = useI18n()
  const toast = useToast()
  const [search, setSearch] = useState('')
  const [role, setRole] = useState('')
  const [page, setPage] = useState(1)
  const q = useDebouncedValue(search)
  const list = useAsync(() => usersApi.list({ search: q || undefined, role: role || undefined, page, pageSize: 20, isActive: null }), [q, role, page])
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<UserDetailDto | null>(null)
  const [form, setForm] = useState({ fullName: '', email: '', password: '', role: 'SECRETARY', phone: '', licenseNumber: '', specialization: '', bio: '' })
  const [resetId, setResetId] = useState<number | null>(null)
  const [newPassword, setNewPassword] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault()
    setBusy(true); setError('')
    try {
      if (editing) {
        await usersApi.update(editing.userId, { fullName: form.fullName, email: form.email, phone: form.phone || null, role: form.role })
        toast.push('User updated.')
      } else {
        const body: CreateUserRequest = {
          fullName: form.fullName,
          email: form.email,
          password: form.password,
          role: form.role,
          phone: form.phone || null,
          doctorProfile: form.role === Role.Doctor ? { licenseNumber: form.licenseNumber || null, specialization: form.specialization || null, bio: form.bio || null } : null,
        }
        await usersApi.create(body)
        toast.push('User created.')
      }
      setOpen(false)
      void list.reload()
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to save user.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <PageHeader crumbs={t('users.crumbs')} title={t('users.title')} description={t('users.lede')} actions={<Button type="button" onClick={() => { setEditing(null); setForm({ fullName: '', email: '', password: '', role: 'SECRETARY', phone: '', licenseNumber: '', specialization: '', bio: '' }); setOpen(true) }}>{t('users.add')}</Button>} />
      <div className="toolbar">
        <input className="control" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search name or email" />
        <select className="control" value={role} onChange={(e) => setRole(e.target.value)}>
          <option value="">All roles</option>
          <option>ADMIN</option><option>DOCTOR</option><option>SECRETARY</option>
        </select>
      </div>
      {list.loading ? <LoadingSkeleton /> : null}
      {list.error ? <ErrorState text={isApiError(list.error) ? list.error.message : 'Failed to load users.'} /> : null}
      {list.data?.items.length === 0 ? <EmptyState title="No users" text="No staff matched these filters." /> : null}
      {list.data && list.data.items.length > 0 ? (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Last login</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {list.data.items.map((u) => (
                  <tr key={u.userId}>
                    <td>{u.fullName}</td>
                    <td>{u.email}</td>
                    <td>{u.role}</td>
                    <td>{formatDateTime(u.lastLoginAt)}</td>
                    <td><StatusBadge status={u.isActive ? 'ACTIVE' : 'INACTIVE'} /></td>
                    <td>
                      <div className="row">
                        <Button size="sm" variant="ghost" type="button" onClick={async () => {
                          const d = await usersApi.get(u.userId)
                          setEditing(d)
                          setForm({ fullName: d.fullName, email: d.email, password: '', role: d.role, phone: d.phone ?? '', licenseNumber: d.doctorProfile?.licenseNumber ?? '', specialization: d.doctorProfile?.specialization ?? '', bio: d.doctorProfile?.bio ?? '' })
                          setOpen(true)
                        }}>Edit</Button>
                        <Button size="sm" variant="ghost" type="button" onClick={async () => {
                          if (u.isActive) await usersApi.deactivate(u.userId)
                          else await usersApi.activate(u.userId)
                          toast.push(u.isActive ? 'User deactivated.' : 'User activated.')
                          void list.reload()
                        }}>{u.isActive ? 'Deactivate' : 'Activate'}</Button>
                        <Button size="sm" variant="ghost" type="button" onClick={() => { setResetId(u.userId); setNewPassword('') }}>Reset password</Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="card-pad"><Pagination page={list.data.page} totalPages={list.data.totalPages} totalCount={list.data.totalCount} onPage={setPage} /></div>
        </div>
      ) : null}

      {open ? (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
            <header><h2>{editing ? 'Edit user' : 'New user'}</h2></header>
            <div className="body stack">
              {error ? <div className="form-error">{error}</div> : null}
              <div className="field"><label>Full name</label><input className="control" required value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} /></div>
              <div className="field"><label>Email</label><input className="control" type="email" required value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></div>
              {!editing ? <div className="field"><label>Password</label><input className="control" type="password" required minLength={6} value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} /></div> : null}
              <div className="field">
                <label>Role</label>
                <select className="control" value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
                  <option>ADMIN</option><option>DOCTOR</option><option>SECRETARY</option>
                </select>
              </div>
              <div className="field"><label>Phone</label><input className="control" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} /></div>
              {form.role === 'DOCTOR' && !editing ? (
                <>
                  <div className="field"><label>License</label><input className="control" value={form.licenseNumber} onChange={(e) => setForm({ ...form, licenseNumber: e.target.value })} /></div>
                  <div className="field"><label>Specialization</label><input className="control" value={form.specialization} onChange={(e) => setForm({ ...form, specialization: e.target.value })} /></div>
                  <div className="field"><label>Bio</label><textarea className="control" value={form.bio} onChange={(e) => setForm({ ...form, bio: e.target.value })} /></div>
                </>
              ) : null}
            </div>
            <footer>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Save'}</Button>
            </footer>
          </form>
        </div>
      ) : null}

      {resetId ? (
        <ConfirmDialog
          title="Reset password"
          message="The new password is never shown again in the interface."
          confirmLabel="Reset"
          reasonRequired
          reasonLabel="New password"
          secret
          reason={newPassword}
          onReason={setNewPassword}
          onCancel={() => setResetId(null)}
          onConfirm={async () => {
            if (newPassword.length < 6) {
              toast.push('Password must be at least 6 characters.', 'error')
              return
            }
            await usersApi.resetPassword(resetId, { newPassword })
            toast.push('Password reset.')
            setResetId(null)
          }}
        />
      ) : null}
    </div>
  )
}
