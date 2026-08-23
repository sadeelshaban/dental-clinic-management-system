import { NavLink } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { useI18n } from '@/i18n/I18nContext'
import type { MessageKey } from '@/i18n/strings'
import { BrandMark } from '@/components/BrandMark'
import { CLINIC } from '@/clinic'
import {
  IconCalendar,
  IconDashboard,
  IconDoctor,
  IconExpense,
  IconFile,
  IconPatients,
  IconPayment,
  IconProfile,
  IconReport,
  IconSupplier,
  IconTreatment,
  IconUsers,
  IconVisit,
} from '@/components/icons'

const links: { to: string; labelKey: MessageKey; icon: typeof IconDashboard; roles: readonly string[] }[] = [
  { to: '/', labelKey: 'nav.dashboard', icon: IconDashboard, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/patients', labelKey: 'nav.patients', icon: IconPatients, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/appointments', labelKey: 'nav.appointments', icon: IconCalendar, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/visits', labelKey: 'nav.visits', icon: IconVisit, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/treatments', labelKey: 'nav.treatments', icon: IconTreatment, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/payments', labelKey: 'nav.payments', icon: IconPayment, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/expenses', labelKey: 'nav.expenses', icon: IconExpense, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/suppliers', labelKey: 'nav.suppliers', icon: IconSupplier, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/attachments', labelKey: 'nav.attachments', icon: IconFile, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/reports', labelKey: 'nav.reports', icon: IconReport, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/users', labelKey: 'nav.users', icon: IconUsers, roles: [Role.Admin] },
  { to: '/doctors', labelKey: 'nav.doctors', icon: IconDoctor, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
  { to: '/account', labelKey: 'nav.account', icon: IconProfile, roles: [Role.Admin, Role.Doctor, Role.Secretary] },
]

export function Sidebar({ open, onNavigate }: { open: boolean; onNavigate: () => void }) {
  const { user } = useAuth()
  const { t } = useI18n()
  const role = user?.role

  return (
    <aside className={`sidebar${open ? ' open' : ''}`} aria-label="Clinic navigation">
      <div className="brand">
        <BrandMark />
        <div className="brand-copy">
          <div className="brand-title">{t('brand.title')}</div>
          <div className="brand-sub">{t('brand.sub')}</div>
        </div>
      </div>
      <nav>
        <div className="nav-label">{t('nav.workspace')}</div>
        <div className="nav-group">
          {links
            .filter((link) => role && link.roles.includes(role as typeof Role.Admin))
            .map((link) => {
              const Icon = link.icon
              return (
                <NavLink
                  key={link.to}
                  to={link.to}
                  end={link.to === '/'}
                  className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
                  onClick={onNavigate}
                >
                  <Icon />
                  {t(link.labelKey)}
                </NavLink>
              )
            })}
        </div>
      </nav>
      <div className="sidebar-foot">
        <a href={CLINIC.phoneHref}>{CLINIC.phone}</a>
        <span>{t('clinic.address')}</span>
      </div>
    </aside>
  )
}
