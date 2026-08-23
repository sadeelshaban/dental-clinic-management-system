import { useState, type FormEvent } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import { isApiError } from '@/api/client'
import { BrandMark } from '@/components/BrandMark'
import { Button, TextInput } from '@/components/ui/kit'
import { LanguageToggle } from '@/i18n/LanguageToggle'
import { useI18n } from '@/i18n/I18nContext'
import { CLINIC } from '@/clinic'

export function LoginPage() {
  const { user, login } = useAuth()
  const { t } = useI18n()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const from = (location.state as { from?: string } | null)?.from || '/'

  if (user) return <Navigate to={from} replace />

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')
    if (!email.trim() || password.length < 6) {
      setError(t('login.invalid'))
      return
    }
    setBusy(true)
    try {
      await login(email.trim(), password)
    } catch (err) {
      setError(isApiError(err) ? err.message : t('login.failed'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="login-shell">
      <section className="login-panel">
        <div>
          <div className="brand" style={{ paddingLeft: 0 }}>
            <BrandMark size={48} />
            <div>
              <div className="brand-title">{t('brand.title')}</div>
              <div className="brand-sub">{t('brand.sub')}</div>
            </div>
          </div>
          <h1>{t('login.hero')}</h1>
          <p style={{ marginTop: 18 }}>{t('login.lede')}</p>
        </div>
        <div className="login-meta">
          <a href={CLINIC.phoneHref}>{CLINIC.phone}</a>
          <p>{t('login.meta')}</p>
        </div>
      </section>
      <section className="login-form">
        <form className="login-card stack" onSubmit={onSubmit} noValidate>
          <LanguageToggle />
          <div>
            <div className="crumbs">{t('login.access')}</div>
            <h2>{t('login.title')}</h2>
            <p className="lede">{t('login.hint')}</p>
          </div>
          {error ? <div className="form-error" role="alert">{error}</div> : null}
          <div className="field">
            <label htmlFor="email">{t('login.email')}</label>
            <TextInput id="email" type="email" autoComplete="username" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </div>
          <div className="field">
            <label htmlFor="password">{t('login.password')}</label>
            <TextInput id="password" type="password" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} />
          </div>
          <Button type="submit" disabled={busy}>{busy ? t('login.busy') : t('login.submit')}</Button>
        </form>
      </section>
    </div>
  )
}
