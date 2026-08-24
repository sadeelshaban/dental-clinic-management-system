import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { authApi, paymentMethodsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { staffDisplayName, staffRoleLabel } from '@/clinic'
import { useI18n } from '@/i18n/I18nContext'
import { useAuth, useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { IconLogout } from '@/components/icons'
import { Button, ErrorState, LoadingSkeleton, PageHeader, StatusBadge } from '@/components/ui/kit'
import { useToast } from '@/components/ui/Toast'
import { useAsync } from '@/hooks/useAsync'

export function AccountPage() {
  const { t, locale } = useI18n()
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const isAdmin = useCan([Role.Admin])
  const toast = useToast()
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const methods = useAsync(() => paymentMethodsApi.list(), [])
  const [methodName, setMethodName] = useState('')

  async function changePassword(event: FormEvent) {
    event.preventDefault()
    if (newPassword.length < 6) {
      setError('New password must be at least 6 characters.')
      return
    }
    setBusy(true); setError('')
    try {
      await authApi.changePassword({ currentPassword, newPassword })
      toast.push('Password updated.')
      setCurrentPassword('')
      setNewPassword('')
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Unable to change password.')
    } finally { setBusy(false) }
  }

  return (
    <div>
      <PageHeader crumbs={t('acct.crumbs')} title={t('acct.title')} description={t('acct.lede')} />
      <div className="grid grid-2">
        <div className="card card-pad">
          <h2>{staffDisplayName(user, locale)}</h2>
          <dl className="dl" style={{ marginTop: 12 }}>
            <dt>Email</dt><dd>{user?.email}</dd>
            <dt>Role</dt><dd>{staffRoleLabel(user, locale)}</dd>
            <dt>Phone</dt><dd>{user?.phone || '—'}</dd>
            <dt>Status</dt><dd><StatusBadge status={user?.isActive ? 'ACTIVE' : 'INACTIVE'} /></dd>
          </dl>
        </div>
        <form className="card card-pad stack" onSubmit={changePassword}>
          <h2>Change password</h2>
          {error ? <div className="form-error">{error}</div> : null}
          <div className="field"><label>Current password</label><input className="control" type="password" required value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} /></div>
          <div className="field"><label>New password</label><input className="control" type="password" required minLength={6} value={newPassword} onChange={(e) => setNewPassword(e.target.value)} /></div>
          <Button type="submit" disabled={busy}>{busy ? 'Saving...' : 'Update password'}</Button>
        </form>
      </div>
      {isAdmin ? (
        <section className="card card-pad" style={{ marginTop: 16 }}>
          <h2>Payment methods</h2>
          <p className="metric-note">Clinic-configurable methods. Soft-deactivated, never deleted.</p>
          {methods.loading ? <LoadingSkeleton rows={3} /> : null}
          {methods.error ? <ErrorState text={isApiError(methods.error) ? methods.error.message : 'Failed to load methods.'} /> : null}
          <div className="table-wrap" style={{ marginTop: 12 }}>
            <table className="data">
              <thead><tr><th>Name</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {methods.data?.items.map((m) => (
                  <tr key={m.paymentMethodId}>
                    <td>{m.name}</td>
                    <td><StatusBadge status={m.isActive ? 'ACTIVE' : 'INACTIVE'} /></td>
                    <td>
                      <Button size="sm" variant="ghost" type="button" onClick={async () => {
                        await paymentMethodsApi.update(m.paymentMethodId, { isActive: !m.isActive })
                        toast.push('Payment method updated.')
                        void methods.reload()
                      }}>{m.isActive ? 'Deactivate' : 'Activate'}</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <form className="row" style={{ marginTop: 16 }} onSubmit={async (e) => {
            e.preventDefault()
            await paymentMethodsApi.create({ name: methodName })
            toast.push('Payment method created.')
            setMethodName('')
            void methods.reload()
          }}>
            <input className="control" required value={methodName} onChange={(e) => setMethodName(e.target.value)} placeholder="New method name" />
            <Button type="submit">Add method</Button>
          </form>
        </section>
      ) : null}
      <section className="card card-pad account-signout" style={{ marginTop: 16 }}>
        <h2>{t('acct.session')}</h2>
        <p className="metric-note">{t('acct.signOutHint')}</p>
        <Button
          variant="danger"
          type="button"
          onClick={() => {
            logout()
            navigate('/login')
          }}
        >
          <IconLogout /> {t('header.signOut')}
        </Button>
      </section>
    </div>
  )
}
