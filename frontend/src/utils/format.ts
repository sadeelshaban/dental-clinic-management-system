const ILS = new Intl.NumberFormat('en-PS', {
  style: 'currency',
  currency: 'ILS',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const NUMBER = new Intl.NumberFormat('en-US', {
  minimumFractionDigits: 0,
  maximumFractionDigits: 2,
})

export function money(value: number | null | undefined): string {
  return ILS.format(value ?? 0)
}

export function number(value: number | null | undefined): string {
  return NUMBER.format(value ?? 0)
}

export function percent(value: number | null | undefined): string {
  if (value === null || value === undefined) return '—'
  const sign = value > 0 ? '+' : ''
  return `${sign}${value.toFixed(1)}%`
}

export function formatDate(value: string | null | undefined): string {
  if (!value) return '—'
  const date = value.length <= 10 ? new Date(`${value}T00:00:00`) : new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatTime(value: string | null | undefined): string {
  if (!value) return '—'
  const parts = value.split(':')
  if (parts.length < 2) return value
  return `${parts[0]}:${parts[1]}`
}

export function todayIso(): string {
  const now = new Date()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${now.getFullYear()}-${month}-${day}`
}

export function addDaysIso(iso: string, days: number): string {
  const date = new Date(`${iso}T00:00:00`)
  date.setDate(date.getDate() + days)
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

export function startOfWeekIso(iso = todayIso()): string {
  const date = new Date(`${iso}T00:00:00`)
  const day = date.getDay()
  date.setDate(date.getDate() - day)
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${d}`
}

export function startOfMonthIso(iso = todayIso()): string {
  return `${iso.slice(0, 7)}-01`
}

export function endOfMonthIso(iso = todayIso()): string {
  const year = Number(iso.slice(0, 4))
  const month = Number(iso.slice(5, 7))
  const last = new Date(year, month, 0).getDate()
  return `${year}-${String(month).padStart(2, '0')}-${String(last).padStart(2, '0')}`
}

export function localDateTimeValue(value?: string | null): string {
  const date = value ? new Date(value) : new Date()
  if (Number.isNaN(date.getTime())) return ''
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

export function fromDateTimeLocal(value: string): string {
  if (!value) return ''
  return value.length === 16 ? `${value}:00` : value
}

export function toTimeApi(value: string): string {
  if (!value) return ''
  return value.length === 5 ? `${value}:00` : value
}

export function fromTimeApi(value: string | null | undefined): string {
  if (!value) return ''
  return value.slice(0, 5)
}

export function labelize(value: string | null | undefined): string {
  if (!value) return '—'
  return value
    .toLowerCase()
    .split('_')
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ')
}

export function initials(name: string): string {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')
}

export function fileSize(bytes?: number | null): string {
  if (!bytes) return '—'
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export function statusTone(status: string): 'neutral' | 'info' | 'success' | 'warning' | 'danger' {
  const value = status.toUpperCase()
  if (value === 'PAID' || value === 'COMPLETED' || value === 'CONFIRMED' || value === 'ACTIVE') return 'success'
  if (value === 'PARTIALLY_PAID' || value === 'SCHEDULED') return 'info'
  if (value === 'UNPAID' || value === 'NO_SHOW') return 'warning'
  if (value === 'VOIDED' || value === 'CANCELLED' || value === 'INACTIVE') return 'danger'
  return 'neutral'
}
