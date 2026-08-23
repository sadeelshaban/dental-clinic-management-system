import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { appointmentsApi, patientsApi, reportsApi } from '@/api/services'
import { useAuth, useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { BarCompare, LineChart } from '@/components/charts/Charts'
import { Button, ErrorState, LoadingSkeleton, PageHeader, StatCard, StatusBadge } from '@/components/ui/kit'
import { useAsync } from '@/hooks/useAsync'
import { endOfMonthIso, formatDate, formatTime, money, percent, startOfMonthIso, startOfWeekIso, todayIso } from '@/utils/format'
import { isApiError } from '@/api/client'
import { useI18n } from '@/i18n/I18nContext'
import { staffGreetingName } from '@/clinic'

type Period = 'week' | 'month' | 'custom'

export function DashboardPage() {
  const { user } = useAuth()
  const isAdmin = useCan([Role.Admin])
  const canWritePatients = useCan([Role.Admin, Role.Secretary])
  const canClinicalWrite = useCan([Role.Admin, Role.Doctor])
  const canPay = useCan([Role.Admin, Role.Secretary])
  const { t, locale } = useI18n()
  const [period, setPeriod] = useState<Period>('month')
  const [from, setFrom] = useState(startOfMonthIso())
  const [to, setTo] = useState(endOfMonthIso())

  function applyPeriod(next: Period) {
    setPeriod(next)
    if (next === 'week') {
      setFrom(startOfWeekIso())
      setTo(todayIso())
    } else if (next === 'month') {
      setFrom(startOfMonthIso())
      setTo(endOfMonthIso())
    }
  }

  const now = new Date()
  const adminQuery = useAsync(async () => {
    if (!isAdmin) return null
    const [daily, monthly, comparison] = await Promise.all([
      reportsApi.daily(from, to),
      reportsApi.monthly(now.getFullYear(), now.getMonth() + 1),
      reportsApi.comparison(now.getFullYear(), now.getMonth() + 1),
    ])
    return { daily, monthly, comparison }
  }, [isAdmin, from, to])

  const staffQuery = useAsync(async () => {
    const [appointments, outstanding, patients] = await Promise.all([
      appointmentsApi.list({ date: todayIso(), page: 1, pageSize: 50 }),
      reportsApi.outstandingBalances({ page: 1, pageSize: 8 }),
      patientsApi.list({ page: 1, pageSize: 8, isActive: true }),
    ])
    return { appointments, outstanding, patients }
  }, [])

  const periodTotals = useMemo(() => {
    const items = adminQuery.data?.daily.items ?? []
    return {
      revenue: items.reduce((s, i) => s + i.revenue, 0),
      expenses: items.reduce((s, i) => s + i.expenses, 0),
      netProfit: items.reduce((s, i) => s + i.netProfit, 0),
    }
  }, [adminQuery.data])

  return (
    <div>
      <PageHeader
        crumbs={t('dash.crumbs')}
        title={t('dash.hello', { part: t(greeting() === 'morning' ? 'dash.morning' : greeting() === 'afternoon' ? 'dash.afternoon' : 'dash.evening'), name: staffGreetingName(user, locale) })}
        description={t('dash.lede')}
        actions={
          <div className="row">
            {canWritePatients ? <Link className="btn btn-primary" to="/patients?new=1">{t('dash.addPatient')}</Link> : null}
            <Link className="btn btn-ghost" to="/appointments?new=1">{t('dash.addAppointment')}</Link>
            {canPay ? <Link className="btn btn-ghost" to="/payments?new=1">{t('dash.addPayment')}</Link> : null}
            {canClinicalWrite ? <Link className="btn btn-ghost" to="/visits?new=1">{t('dash.addVisit')}</Link> : null}
          </div>
        }
      />

      {isAdmin ? (
        <>
          <div className="toolbar">
            <Button type="button" variant={period === 'week' ? 'primary' : 'ghost'} size="sm" onClick={() => applyPeriod('week')}>{t('dash.week')}</Button>
            <Button type="button" variant={period === 'month' ? 'primary' : 'ghost'} size="sm" onClick={() => applyPeriod('month')}>{t('dash.month')}</Button>
            <Button type="button" variant={period === 'custom' ? 'primary' : 'ghost'} size="sm" onClick={() => applyPeriod('custom')}>{t('dash.custom')}</Button>
            <input className="control" type="date" value={from} onChange={(e) => { setPeriod('custom'); setFrom(e.target.value) }} />
            <input className="control" type="date" value={to} onChange={(e) => { setPeriod('custom'); setTo(e.target.value) }} />
          </div>
          {adminQuery.loading ? <LoadingSkeleton /> : null}
          {adminQuery.error ? (
            <ErrorState text={isApiError(adminQuery.error) ? adminQuery.error.message : 'Failed to load reports.'} onRetry={() => void adminQuery.reload()} />
          ) : null}
          {adminQuery.data ? (
            <div className="stack">
              <div className="grid grid-4">
                <StatCard label={t('dash.revenue')} value={money(periodTotals.revenue)} hint={t('dash.revenueHint')} tone="revenue" />
                <StatCard label={t('dash.expenses')} value={money(periodTotals.expenses)} hint={t('dash.expensesHint')} tone="expense" />
                <StatCard label={t('dash.profit')} value={money(periodTotals.netProfit)} hint={t('dash.profitHint')} tone="profit" />
                <StatCard
                  label={t('dash.outstanding')}
                  value={money(adminQuery.data.daily.outstandingPatientBalances)}
                  hint={t('dash.outstandingHint')}
                  tone="out"
                />
              </div>
              <div className="card card-pad">
                <h2>{t('dash.dailyTitle')}</h2>
                <p className="metric-note">{t('dash.dailyNote')}</p>
                <LineChart
                  labels={adminQuery.data.daily.items.map((i) => i.financialDate.slice(8))}
                  series={[
                    { name: t('dash.seriesRevenue'), values: adminQuery.data.daily.items.map((i) => i.revenue), color: '#1a8a86' },
                    { name: t('dash.seriesExpenses'), values: adminQuery.data.daily.items.map((i) => i.expenses), color: '#0b1f3a' },
                    { name: t('dash.seriesProfit'), values: adminQuery.data.daily.items.map((i) => i.netProfit), color: '#3db8b2' },
                  ]}
                />
              </div>
              <div className="grid grid-2">
                <div className="card card-pad">
                  <h2>{t('dash.monthTitle')}</h2>
                  <p className="metric-note">{t('dash.monthNote')}</p>
                  <dl className="dl" style={{ marginTop: 12 }}>
                    <dt>{t('dash.seriesRevenue')}</dt><dd>{money(adminQuery.data.monthly.revenue)}</dd>
                    <dt>{t('dash.seriesExpenses')}</dt><dd>{money(adminQuery.data.monthly.expenses)}</dd>
                    <dt>{t('dash.seriesProfit')}</dt><dd>{money(adminQuery.data.monthly.netProfit)}</dd>
                    <dt>{t('dash.outstanding')}</dt><dd>{money(adminQuery.data.monthly.outstandingPatientBalances)}</dd>
                    <dt>{t('nav.patients')}</dt><dd>{adminQuery.data.monthly.patients}</dd>
                    <dt>{t('nav.appointments')}</dt><dd>{adminQuery.data.monthly.appointments}</dd>
                  </dl>
                </div>
                <div className="card card-pad">
                  <h2>{t('dash.compareTitle')}</h2>
                  <p className="metric-note">{t('dash.compareNote')}</p>
                  <BarCompare
                    items={[
                      { label: t('dash.seriesRevenue'), current: adminQuery.data.comparison.revenue, previous: adminQuery.data.comparison.previousMonthRevenue ?? 0 },
                      { label: t('dash.seriesExpenses'), current: adminQuery.data.comparison.expenses, previous: adminQuery.data.comparison.previousMonthExpenses ?? 0 },
                      { label: t('dash.seriesProfit'), current: adminQuery.data.comparison.netProfit, previous: adminQuery.data.comparison.previousMonthProfit ?? 0 },
                    ]}
                  />
                  <p className="muted">
                    {t('dash.seriesRevenue')} {percent(adminQuery.data.comparison.revenueChangePercent)} ·
                    {t('dash.seriesExpenses')} {percent(adminQuery.data.comparison.expenseChangePercent)} ·
                    {t('dash.seriesProfit')} {percent(adminQuery.data.comparison.profitChangePercent)}
                  </p>
                </div>
              </div>
            </div>
          ) : null}
        </>
      ) : null}

      {staffQuery.loading ? <LoadingSkeleton /> : null}
      {staffQuery.error ? (
        <ErrorState text={isApiError(staffQuery.error) ? staffQuery.error.message : 'Failed to load clinic activity.'} onRetry={() => void staffQuery.reload()} />
      ) : null}
      {staffQuery.data ? (
        <div className="grid grid-2" style={{ marginTop: 16 }}>
          <div className="card card-pad">
            <h2>{t('dash.todayAppts')}</h2>
            {staffQuery.data.appointments.items.length === 0 ? (
              <p className="muted" style={{ marginTop: 12 }}>{t('dash.noAppts', { date: formatDate(todayIso()) })}</p>
            ) : (
              <div className="table-wrap" style={{ marginTop: 12 }}>
                <table className="data">
                  <thead><tr><th>{t('dash.time')}</th><th>{t('dash.patient')}</th><th>{t('dash.doctor')}</th><th>{t('patients.status')}</th></tr></thead>
                  <tbody>
                    {staffQuery.data.appointments.items.map((a) => (
                      <tr key={a.appointmentId}>
                        <td>{formatTime(a.startTime)} – {formatTime(a.endTime)}</td>
                        <td><Link to={`/patients/${a.patientId}`}>{a.patientName}</Link></td>
                        <td>{a.doctorName}</td>
                        <td><StatusBadge status={a.status} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
          <div className="card card-pad">
            <h2>{t('dash.outTitle')}</h2>
            <p className="metric-note">{t('dash.outNote')}</p>
            {staffQuery.data.outstanding.items.length === 0 ? (
              <p className="muted" style={{ marginTop: 12 }}>{t('dash.noOut')}</p>
            ) : (
              <div className="table-wrap" style={{ marginTop: 12 }}>
                <table className="data">
                  <thead><tr><th>{t('dash.patient')}</th><th>{t('dash.remaining')}</th></tr></thead>
                  <tbody>
                    {staffQuery.data.outstanding.items.map((p) => (
                      <tr key={p.patientId}>
                        <td><Link to={`/patients/${p.patientId}`}>{p.fullName}</Link></td>
                        <td>{money(p.totalRemaining)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
            <p style={{ marginTop: 12 }}><Link to="/reports">{t('dash.openReports')}</Link></p>
          </div>
        </div>
      ) : null}
      {isAdmin ? null : <p className="muted" style={{ marginTop: 16 }}>{t('dash.adminOnly')}</p>}
    </div>
  )
}

function greeting(): string {
  const h = new Date().getHours()
  if (h < 12) return 'morning'
  if (h < 17) return 'afternoon'
  return 'evening'
}
