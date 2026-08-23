import { useI18n } from '@/i18n/I18nContext'

export function LanguageToggle() {
  const { locale, setLocale } = useI18n()
  return (
    <div className="lang-toggle" role="group" aria-label="Language">
      <button
        type="button"
        className={locale === 'en' ? 'is-active' : ''}
        onClick={() => setLocale('en')}
      >
        EN
      </button>
      <button
        type="button"
        className={locale === 'ar' ? 'is-active' : ''}
        onClick={() => setLocale('ar')}
      >
        عربي
      </button>
    </div>
  )
}
