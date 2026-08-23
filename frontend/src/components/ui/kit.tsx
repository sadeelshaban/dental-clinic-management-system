import { useEffect, useId, type ButtonHTMLAttributes, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes, type TextareaHTMLAttributes } from 'react'
import { useI18n, useStatusLabel } from '@/i18n/I18nContext'
import { statusTone } from '@/utils/format'

export function Button({
  variant = 'primary',
  size,
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: 'primary' | 'accent' | 'ghost' | 'danger'; size?: 'sm' }) {
  return <button className={`btn btn-${variant}${size === 'sm' ? ' btn-sm' : ''}`} {...props} />
}

export function Field({
  label,
  hint,
  error,
  children,
}: {
  label: string
  hint?: string
  error?: string
  children: ReactNode
}) {
  const id = useId()
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <div className="field-control" data-id={id}>
        {children}
      </div>
      {error ? <span className="error">{error}</span> : hint ? <span className="hint">{hint}</span> : null}
    </div>
  )
}

export function TextInput(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input className="control" {...props} />
}

export function TextArea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea className="control" {...props} />
}

export function Select(props: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select className="control" {...props} />
}

export function StatusBadge({ status }: { status: string }) {
  const label = useStatusLabel()
  return <span className={`badge badge-${statusTone(status)}`}>{label(status)}</span>
}

export function EmptyState({ title, text, action }: { title: string; text: string; action?: ReactNode }) {
  return (
    <div className="empty">
      <h3>{title}</h3>
      <p>{text}</p>
      {action ? <div style={{ marginTop: 16 }}>{action}</div> : null}
    </div>
  )
}

export function ErrorState({ title, text, onRetry }: { title?: string; text: string; onRetry?: () => void }) {
  const { t } = useI18n()
  return (
    <div className="error-box">
      <h3>{title ?? t('common.unableLoad')}</h3>
      <p>{text}</p>
      {onRetry ? (
        <div style={{ marginTop: 16 }}>
          <Button variant="ghost" type="button" onClick={onRetry}>
            {t('common.retry')}
          </Button>
        </div>
      ) : null}
    </div>
  )
}

export function LoadingSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div className="stack">
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className="skeleton" style={{ height: 46 }} />
      ))}
    </div>
  )
}

export function StatCard({
  label,
  value,
  hint,
  tone = 'default',
}: {
  label: string
  value: string
  hint?: string
  tone?: 'revenue' | 'expense' | 'profit' | 'out' | 'default'
}) {
  return (
    <article className={`card stat${tone !== 'default' ? ` stat-${tone}` : ''}`}>
      <div className="stat-label">{label}</div>
      <div className="stat-value">{value}</div>
      {hint ? <div className="stat-hint">{hint}</div> : null}
    </article>
  )
}

export function PageHeader({
  crumbs,
  title,
  description,
  actions,
}: {
  crumbs?: string
  title: string
  description?: string
  actions?: ReactNode
}) {
  return (
    <header className="page-header">
      <div>
        {crumbs ? <div className="crumbs">{crumbs}</div> : null}
        <h1>{title}</h1>
        {description ? <p className="lede">{description}</p> : null}
      </div>
      {actions}
    </header>
  )
}

export function Pagination({
  page,
  totalPages,
  totalCount,
  onPage,
}: {
  page: number
  totalPages: number
  totalCount: number
  onPage: (page: number) => void
}) {
  const { t } = useI18n()
  return (
    <div className="pager">
      <span>
        {t(totalCount === 1 ? 'common.records' : 'common.records_plural', { count: totalCount })}
      </span>
      <div className="row">
        <Button variant="ghost" size="sm" type="button" disabled={page <= 1} onClick={() => onPage(page - 1)}>
          {t('common.previous')}
        </Button>
        <span>
          {t('common.page', { page, total: Math.max(totalPages, 1) })}
        </span>
        <Button variant="ghost" size="sm" type="button" disabled={page >= totalPages} onClick={() => onPage(page + 1)}>
          {t('common.next')}
        </Button>
      </div>
    </div>
  )
}

export function Modal({
  title,
  children,
  footer,
  onClose,
  large,
}: {
  title: string
  children: ReactNode
  footer?: ReactNode
  onClose: () => void
  large?: boolean
}) {
  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div className="modal-backdrop" onClick={onClose} role="presentation">
      <div
        className={`modal${large ? ' modal-lg' : ''}`}
        role="dialog"
        aria-modal="true"
        aria-labelledby="dialog-title"
        onClick={(e) => e.stopPropagation()}
      >
        <header>
          <h2 id="dialog-title">{title}</h2>
        </header>
        <div className="body">{children}</div>
        {footer ? <footer>{footer}</footer> : null}
      </div>
    </div>
  )
}

export function ConfirmDialog({
  title,
  message,
  confirmLabel,
  danger,
  reasonRequired,
  reasonLabel,
  secret,
  reason,
  onReason,
  busy,
  error,
  onCancel,
  onConfirm,
}: {
  title: string
  message: string
  confirmLabel?: string
  danger?: boolean
  reasonRequired?: boolean
  reasonLabel?: string
  secret?: boolean
  reason?: string
  onReason?: (value: string) => void
  busy?: boolean
  error?: string
  onCancel: () => void
  onConfirm: () => void
}) {
  const { t } = useI18n()
  const disabled = Boolean(busy || (reasonRequired && !reason?.trim()))
  return (
    <Modal
      title={title}
      onClose={onCancel}
      footer={
        <>
          <Button variant="ghost" type="button" onClick={onCancel} disabled={busy}>
            {t('common.cancel')}
          </Button>
          <Button variant={danger ? 'danger' : 'primary'} type="button" onClick={onConfirm} disabled={disabled}>
            {busy ? t('common.working') : (confirmLabel ?? t('common.confirm'))}
          </Button>
        </>
      }
    >
      <p>{message}</p>
      {reasonRequired ? (
        <div className="field" style={{ marginTop: 14 }}>
          <label htmlFor="void-reason">{reasonLabel ?? t('common.reason')}</label>
          {secret ? (
            <input
              id="void-reason"
              className="control"
              type="password"
              required
              minLength={6}
              value={reason}
              onChange={(e) => onReason?.(e.target.value)}
            />
          ) : (
            <textarea
              id="void-reason"
              className="control"
              required
              value={reason}
              onChange={(e) => onReason?.(e.target.value)}
              placeholder={t('common.reasonHint')}
            />
          )}
        </div>
      ) : null}
      {error ? <p className="form-error" style={{ marginTop: 12 }}>{error}</p> : null}
    </Modal>
  )
}

export function Tabs({
  tabs,
  value,
  onChange,
}: {
  tabs: { id: string; label: string }[]
  value: string
  onChange: (id: string) => void
}) {
  return (
    <div className="tabs" role="tablist">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          type="button"
          role="tab"
          aria-selected={tab.id === value}
          className={`tab${tab.id === value ? ' active' : ''}`}
          onClick={() => onChange(tab.id)}
        >
          {tab.label}
        </button>
      ))}
    </div>
  )
}

export function errorMessage(error: unknown, fallback?: string): string {
  if (error && typeof error === 'object' && 'message' in error && typeof error.message === 'string') {
    return error.message
  }
  return fallback ?? 'Something went wrong.'
}
