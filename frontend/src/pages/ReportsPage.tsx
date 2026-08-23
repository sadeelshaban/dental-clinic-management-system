import { useState } from 'react'
import { Link } from 'react-router-dom'
import { reportsApi } from '@/api/services'
import { isApiError } from '@/api/client'
import { useCan } from '@/auth/AuthContext'
import { Role } from '@/auth/roles'
import { LineChart } from '@/components/charts/Charts'
import { EmptyState, ErrorState, LoadingSkeleton, PageHeader, Pagination, StatCard, Tabs } from '@/components/ui/kit'
import { useAsync, useDebouncedValue } from '@/hooks/useAsync'
import { endOfMonthIso, formatDate, money, percent, startOfMonthIso } from '@/utils/format'
import { useI18n } from '@/i18n/I18nContext'

export function ReportsPage() {
  const { t } = useI18n()
  const isAdmin = useCan([Role.Admin])
  const [tab, setTab] = useState(isAdmin ? 'daily' : 'outstanding')
  return (
    <div>
      <PageHeader crumbs={t('rep.crumbs')} title={t('rep.title')} description={t('rep.lede')} />
      <Tabs
        tabs={[
          ...(isAdmin ? [
            { id: 'daily', label: t('rep.daily') },
            { id: 'monthly', label: t('rep.monthly') },
            { id: 'comparison', label: t('rep.comparison') },
          ] : []),
          { id: 'outstanding', label: t('rep.outstanding') },
          { id: 'directory', label: t('rep.directory') },
        ]}
        value={tab}
        onChange={setTab}
      />
      {tab === 'daily' && isAdmin ? <DailyPanel /> : null}
      {tab === 'monthly' && isAdmin ? <MonthlyPanel /> : null}
      {tab === 'comparison' && isAdmin ? <ComparePanel /> : null}
      {tab === 'outstanding' ? <OutstandingPanel /> : null}
      {tab === 'directory' ? <DirectoryPanel /> : null}
    </div>
  )
}

function DailyPanel() {
  const [from, setFrom] = useState(startOfMonthIso())
  const [to, setTo] = useState(endOfMonthIso())
  const q = useAsync(() => reportsApi.daily(from, to), [from, to])
  if (q.loading) return <LoadingSkeleton />
  if (q.error) return <ErrorState text={isApiError(q.error) ? q.error.message : 'Failed to load daily report.'} />
  const items = q.data?.items ?? []
  const revenue = items.reduce((s, i) => s + i.revenue, 0)
  const expenses = items.reduce((s, i) => s + i.expenses, 0)
  const profit = items.reduce((s, i) => s + i.netProfit, 0)
  return (
    <div className="stack">
      <div className="toolbar">
        <input className="control" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        <input className="control" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
      </div>
      <div className="grid grid-4">
        <StatCard label="Revenue" value={money(revenue)} hint="Sum of daily collected payments" tone="revenue" />
        <StatCard label="Expenses" value={money(expenses)} hint="Sum of daily expense payments" tone="expense" />
        <StatCard label="Net profit" value={money(profit)} hint="Collected minus paid" tone="profit" />
        <StatCard label="Outstanding" value={money(q.data?.outstandingPatientBalances)} hint="Current clinic snapshot" tone="out" />
      </div>
      <div className="card card-pad">
        <LineChart
          labels={items.map((i) => i.financialDate.slice(8))}
          series={[
            { name: 'Revenue', values: items.map((i) => i.revenue), color: '#1a8a86' },
            { name: 'Expenses', values: items.map((i) => i.expenses), color: '#0b1f3a' },
          ]}
        />
      </div>
      <div className="card">
        <div className="table-wrap">
          <table className="data">
            <thead><tr><th>Date</th><th>Revenue</th><th>Expenses</th><th>Net profit</th></tr></thead>
            <tbody>
              {items.map((i) => (
                <tr key={i.financialDate}>
                  <td>{formatDate(i.financialDate)}</td>
                  <td>{money(i.revenue)}</td>
                  <td>{money(i.expenses)}</td>
                  <td>{money(i.netProfit)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

function MonthlyPanel() {
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const q = useAsync(() => reportsApi.monthly(year, month), [year, month])
  if (q.loading) return <LoadingSkeleton />
  if (q.error) return <ErrorState text={isApiError(q.error) ? q.error.message : 'Failed to load monthly report.'} />
  const d = q.data
  if (!d) return null
  return (
    <div className="stack">
      <div className="toolbar">
        <input className="control" type="number" value={year} onChange={(e) => setYear(Number(e.target.value))} />
        <input className="control" type="number" min={1} max={12} value={month} onChange={(e) => setMonth(Number(e.target.value))} />
      </div>
      <div className="grid grid-4">
        <StatCard label="Revenue" value={money(d.revenue)} tone="revenue" />
        <StatCard label="Expenses" value={money(d.expenses)} tone="expense" />
        <StatCard label="Net profit" value={money(d.netProfit)} tone="profit" />
        <StatCard label="Outstanding" value={money(d.outstandingPatientBalances)} tone="out" />
      </div>
      <div className="card card-pad">
        <dl className="dl">
          <dt>Patients</dt><dd>{d.patients}</dd>
          <dt>Appointments</dt><dd>{d.appointments}</dd>
          <dt>Month</dt><dd>{d.month}</dd>
        </dl>
      </div>
    </div>
  )
}

function ComparePanel() {
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const q = useAsync(() => reportsApi.comparison(year, month), [year, month])
  if (q.loading) return <LoadingSkeleton />
  if (q.error) return <ErrorState text={isApiError(q.error) ? q.error.message : 'Failed to load comparison.'} />
  const d = q.data
  if (!d) return null
  return (
    <div className="stack">
      <div className="toolbar">
        <input className="control" type="number" value={year} onChange={(e) => setYear(Number(e.target.value))} />
        <input className="control" type="number" min={1} max={12} value={month} onChange={(e) => setMonth(Number(e.target.value))} />
      </div>
      <div className="card">
        <div className="table-wrap">
          <table className="data">
            <thead><tr><th>Metric</th><th>Current</th><th>Previous</th><th>Change</th></tr></thead>
            <tbody>
              <tr><td>Revenue</td><td>{money(d.revenue)}</td><td>{money(d.previousMonthRevenue)}</td><td>{percent(d.revenueChangePercent)}</td></tr>
              <tr><td>Expenses</td><td>{money(d.expenses)}</td><td>{money(d.previousMonthExpenses)}</td><td>{percent(d.expenseChangePercent)}</td></tr>
              <tr><td>Net profit</td><td>{money(d.netProfit)}</td><td>{money(d.previousMonthProfit)}</td><td>{percent(d.profitChangePercent)}</td></tr>
              <tr><td>Patients</td><td>{d.patients}</td><td>{d.previousMonthPatients ?? '—'}</td><td>{percent(d.patientChangePercent)}</td></tr>
              <tr><td>Appointments</td><td>{d.appointments}</td><td>{d.previousMonthAppointments ?? '—'}</td><td>{percent(d.appointmentChangePercent)}</td></tr>
              <tr><td>Outstanding</td><td>{money(d.outstandingPatientBalances)}</td><td>—</td><td>Current snapshot</td></tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

function OutstandingPanel() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const q = useDebouncedValue(search)
  const data = useAsync(() => reportsApi.outstandingBalances({ search: q || undefined, page, pageSize: 20 }), [q, page])
  if (data.loading) return <LoadingSkeleton />
  if (data.error) return <ErrorState text={isApiError(data.error) ? data.error.message : 'Failed to load outstanding balances.'} />
  return (
    <div>
      <div className="toolbar"><input className="control" value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} placeholder="Search patients" /></div>
      {data.data?.items.length === 0 ? <EmptyState title="No outstanding balances" text="No remaining patient balances matched." /> : (
        <div className="card">
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Patient</th><th>Treatment value</th><th>Paid</th><th>Remaining</th></tr></thead>
              <tbody>
                {data.data?.items.map((p) => (
                  <tr key={p.patientId}>
                    <td><Link to={`/patients/${p.patientId}`}>{p.fullName}</Link> <span className="muted">{p.patientNumber}</span></td>
                    <td>{money(p.totalTreatments)}</td>
                    <td>{money(p.totalPaid)}</td>
                    <td>{money(p.totalRemaining)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {data.data ? <div className="card-pad"><Pagination page={data.data.page} totalPages={data.data.totalPages} totalCount={data.data.totalCount} onPage={setPage} /></div> : null}
        </div>
      )}
    </div>
  )
}

function DirectoryPanel() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const q = useDebouncedValue(search)
  const data = useAsync(() => reportsApi.patientDirectory({ search: q || undefined, page, pageSize: 20 }), [q, page])
  if (data.loading) return <LoadingSkeleton />
  if (data.error) return <ErrorState text={isApiError(data.error) ? data.error.message : 'Failed to load directory.'} />
  return (
    <div>
      <div className="toolbar"><input className="control" value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} placeholder="Search directory" /></div>
      <div className="card">
        <div className="table-wrap">
          <table className="data">
            <thead><tr><th>Patient</th><th>Contact</th><th>Treatments</th><th>Paid</th><th>Remaining</th></tr></thead>
            <tbody>
              {data.data?.items.map((p) => (
                <tr key={p.patientId}>
                  <td><Link to={`/patients/${p.patientId}`}>{p.fullName}</Link></td>
                  <td>{p.phone || p.email || '—'}</td>
                  <td>{money(p.totalTreatments)}</td>
                  <td>{money(p.totalPaid)}</td>
                  <td>{money(p.totalRemaining)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {data.data ? <div className="card-pad"><Pagination page={data.data.page} totalPages={data.data.totalPages} totalCount={data.data.totalCount} onPage={setPage} /></div> : null}
      </div>
    </div>
  )
}
