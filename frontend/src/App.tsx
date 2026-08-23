import { Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { AppShell } from '@/components/layout/AppShell'
import { useI18n } from '@/i18n/I18nContext'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { PatientsPage } from '@/pages/PatientsPage'
import { PatientProfilePage } from '@/pages/PatientProfilePage'
import { AppointmentsPage } from '@/pages/AppointmentsPage'
import { VisitsPage } from '@/pages/VisitsPage'
import { TreatmentsPage } from '@/pages/TreatmentsPage'
import { PaymentsPage } from '@/pages/PaymentsPage'
import { ExpensesPage } from '@/pages/ExpensesPage'
import { ExpenseDetailPage } from '@/pages/ExpenseDetailPage'
import { SupplierDetailPage, SuppliersPage } from '@/pages/SuppliersPage'
import { AttachmentsPage } from '@/pages/AttachmentsPage'
import { ReportsPage } from '@/pages/ReportsPage'
import { UsersPage } from '@/pages/UsersPage'
import { DoctorsPage } from '@/pages/DoctorsPage'
import { AccountPage } from '@/pages/AccountPage'

function Guard({ roles }: { roles?: readonly string[] }) {
  const { user, loading } = useAuth()
  const location = useLocation()
  if (loading) return <div className="page"><div className="skeleton" style={{ height: 120 }} /></div>
  if (!user) return <Navigate to="/login" replace state={{ from: location.pathname }} />
  if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />
  return <Outlet />
}

function NotFoundPage() {
  const { t } = useI18n()
  return (
    <div>
      <h1>{t('common.notFound')}</h1>
      <p className="lede">{t('common.notFoundText')}</p>
    </div>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<Guard />}>
        <Route element={<AppShell />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/patients" element={<PatientsPage />} />
          <Route path="/patients/:patientId" element={<PatientProfilePage />} />
          <Route path="/appointments" element={<AppointmentsPage />} />
          <Route path="/visits" element={<VisitsPage />} />
          <Route path="/treatments" element={<TreatmentsPage />} />
          <Route path="/payments" element={<PaymentsPage />} />
          <Route path="/expenses" element={<ExpensesPage />} />
          <Route path="/expenses/:expenseId" element={<ExpenseDetailPage />} />
          <Route path="/suppliers" element={<SuppliersPage />} />
          <Route path="/suppliers/:supplierId" element={<SupplierDetailPage />} />
          <Route path="/attachments" element={<AttachmentsPage />} />
          <Route path="/reports" element={<ReportsPage />} />
          <Route path="/doctors" element={<DoctorsPage />} />
          <Route path="/account" element={<AccountPage />} />
          <Route element={<Guard roles={[Role.Admin]} />}>
            <Route path="/users" element={<UsersPage />} />
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>
    </Routes>
  )
}
