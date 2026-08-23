import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import { IconLogout, IconMenu, IconSearch } from '@/components/icons'
import { LanguageToggle } from '@/i18n/LanguageToggle'
import { useI18n } from '@/i18n/I18nContext'
import { staffDisplayName, staffRoleLabel } from '@/clinic'
import { initials } from '@/utils/format'
import { patientsApi } from '@/api/services'
import { useDebouncedValue } from '@/hooks/useAsync'

export function Header({ onMenu }: { onMenu: () => void }) {
  const { user, logout } = useAuth()
  const { t, locale } = useI18n()
  const [open, setOpen] = useState(false)
  const [q, setQ] = useState('')
  const search = useDebouncedValue(q, 250)
  const [hits, setHits] = useState<{ patientId: number; fullName: string; patientNumber: string }[]>([])
  const wrap = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()

  useEffect(() => {
    if (!search.trim()) {
      setHits([])
      return
    }
    void patientsApi.list({ search, page: 1, pageSize: 6, isActive: true }).then((res) => {
      setHits(res.items.map((p) => ({ patientId: p.patientId, fullName: p.fullName, patientNumber: p.patientNumber })))
    }).catch(() => setHits([]))
  }, [search])

  useEffect(() => {
    const onDoc = (event: MouseEvent) => {
      if (!wrap.current?.contains(event.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', onDoc)
    return () => document.removeEventListener('mousedown', onDoc)
  }, [])

  return (
    <header className="header">
      <button type="button" className="icon-btn mobile-only" onClick={onMenu} aria-label={t('header.menu')}>
        <IconMenu />
      </button>
      <div className="header-search">
        <IconSearch />
        <label className="sr-only" htmlFor="global-patient-search">{t('patients.search')}</label>
        <input
          id="global-patient-search"
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder={t('header.search')}
        />
        {hits.length > 0 ? (
          <div className="menu" style={{ left: 0, right: 'auto', width: '100%' }}>
            {hits.map((hit) => (
              <button
                key={hit.patientId}
                type="button"
                onClick={() => {
                  setQ('')
                  setHits([])
                  navigate(`/patients/${hit.patientId}`)
                }}
              >
                {hit.fullName}
                <span className="muted" style={{ marginLeft: 8 }}>{hit.patientNumber}</span>
              </button>
            ))}
          </div>
        ) : null}
      </div>
      <div className="header-actions" ref={wrap}>
        <LanguageToggle />
        <div className="muted desktop-only header-clinic">{t('header.workspace')}</div>
        <button type="button" className="menu-btn user-chip" onClick={() => setOpen((v) => !v)} aria-haspopup="menu">
          <span className="avatar">{initials(staffDisplayName(user, locale) || 'U')}</span>
          <span className="user-meta">
            <strong>{staffDisplayName(user, locale)}</strong>
            <span>{staffRoleLabel(user, locale)}</span>
          </span>
        </button>
        {open ? (
          <div className="menu" role="menu">
            <Link to="/account" onClick={() => setOpen(false)}>{t('header.account')}</Link>
            <button type="button" className="menu-danger" onClick={() => { logout(); navigate('/login') }}>
              <IconLogout /> {t('header.signOut')}
            </button>
          </div>
        ) : null}
      </div>
    </header>
  )
}
