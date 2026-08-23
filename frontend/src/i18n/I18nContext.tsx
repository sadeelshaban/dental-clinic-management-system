import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { ar, en, type MessageKey } from '@/i18n/strings'

export type Locale = 'en' | 'ar'

const STORAGE_KEY = 'dcms.locale'

interface I18nValue {
  locale: Locale
  t: (key: MessageKey, vars?: Record<string, string | number>) => string
  toggle: () => void
  setLocale: (locale: Locale) => void
}

const I18nContext = createContext<I18nValue | null>(null)

function interpolate(template: string, vars?: Record<string, string | number>): string {
  if (!vars) return template
  return template.replace(/\{(\w+)\}/g, (_, name: string) => String(vars[name] ?? `{${name}}`))
}

export function I18nProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(() => {
    const stored = localStorage.getItem(STORAGE_KEY)
    return stored === 'ar' || stored === 'en' ? stored : 'en'
  })

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, locale)
    document.documentElement.lang = locale === 'ar' ? 'ar' : 'en'
    document.documentElement.dataset.lang = locale
  }, [locale])

  const value = useMemo<I18nValue>(() => {
    const table = locale === 'ar' ? ar : en
    return {
      locale,
      t: (key, vars) => interpolate(table[key] ?? en[key] ?? key, vars),
      toggle: () => setLocaleState((current) => (current === 'ar' ? 'en' : 'ar')),
      setLocale: setLocaleState,
    }
  }, [locale])

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}

export function useI18n(): I18nValue {
  const context = useContext(I18nContext)
  if (!context) throw new Error('useI18n must be used within I18nProvider')
  return context
}

export function useStatusLabel() {
  const { t } = useI18n()
  return (status: string) => {
    const key = `status.${status}` as MessageKey
    return key in en ? t(key) : status.replaceAll('_', ' ')
  }
}

export function useGenderLabel() {
  const { t } = useI18n()
  return (gender: string) => {
    const key = `gender.${gender}` as MessageKey
    return key in en ? t(key) : gender
  }
}
