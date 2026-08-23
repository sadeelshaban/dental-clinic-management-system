export const Role = {
  Admin: 'ADMIN',
  Doctor: 'DOCTOR',
  Secretary: 'SECRETARY',
} as const

export type Role = (typeof Role)[keyof typeof Role]

export const Roles = {
  clinicalStaff: [Role.Admin, Role.Doctor, Role.Secretary] as const,
  adminOnly: [Role.Admin] as const,
  adminOrSecretary: [Role.Admin, Role.Secretary] as const,
  adminOrDoctor: [Role.Admin, Role.Doctor] as const,
}

export function isRole(value: string | undefined | null): value is Role {
  return value === Role.Admin || value === Role.Doctor || value === Role.Secretary
}

export function hasRole(userRole: string | undefined | null, allowed: readonly string[]): boolean {
  if (!userRole) return false
  return allowed.includes(userRole)
}
