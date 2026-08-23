import type { UserDto } from '@/types/api'

export type ClinicLocale = 'en' | 'ar'

export const CLINIC = {
  nameEn: 'Elmasry Dental Clinic',
  nameAr: 'عيادة طب الأسنان د. محمد المصري',
  shortEn: 'Elmasry Dental',
  shortAr: 'عيادة المصري',
  doctorEn: 'Dr. Mohammed Elmasry',
  doctorAr: 'د. محمد المصري',
  phone: '0569 360 226',
  phoneHref: 'tel:+970569360226',
  addressEn: 'Jenin, Abu Bakr Street, below Al-Qala’a Coffee Shop',
  addressAr: 'جنين، شارع أبو بكر، تحت كوفي شوب القلعة',
  cityEn: 'Jenin, Palestine',
  cityAr: 'جنين، فلسطين',
  facebook: 'https://www.facebook.com/share/1ChjiCr1oY/',
  logo: '/elmasry-logo.png',
} as const

export function clinicName(locale: ClinicLocale): string {
  return locale === 'ar' ? CLINIC.nameAr : CLINIC.nameEn
}

export function clinicShortName(locale: ClinicLocale): string {
  return locale === 'ar' ? CLINIC.shortAr : CLINIC.shortEn
}

export function clinicDoctorName(locale: ClinicLocale): string {
  return locale === 'ar' ? CLINIC.doctorAr : CLINIC.doctorEn
}

export function clinicAddress(locale: ClinicLocale): string {
  return locale === 'ar' ? CLINIC.addressAr : CLINIC.addressEn
}

export function isClinicOwnerAccount(user?: UserDto | null): boolean {
  if (!user) return false
  if (user.role !== 'ADMIN') return false
  const name = user.fullName.trim()
  return (
    user.email === 'admin@demo.com' ||
    name === 'Demo Admin' ||
    name === CLINIC.doctorEn ||
    name === CLINIC.doctorAr
  )
}

export function staffDisplayName(user: UserDto | null | undefined, locale: ClinicLocale): string {
  if (isClinicOwnerAccount(user)) return clinicDoctorName(locale)
  return user?.fullName ?? ''
}

export function staffGreetingName(user: UserDto | null | undefined, locale: ClinicLocale): string {
  if (isClinicOwnerAccount(user)) return locale === 'ar' ? 'محمد' : 'Mohammed'
  return user?.fullName?.split(/\s+/)[0] || ''
}

export function staffRoleLabel(user: UserDto | null | undefined, locale: ClinicLocale): string {
  if (isClinicOwnerAccount(user)) return locale === 'ar' ? 'طبيب الأسنان' : 'Dentist'
  return user?.role ?? ''
}
