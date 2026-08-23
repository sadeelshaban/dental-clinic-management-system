import type { SVGProps } from 'react'

type IconProps = SVGProps<SVGSVGElement>

function base(props: IconProps) {
  return {
    width: 18,
    height: 18,
    viewBox: '0 0 24 24',
    fill: 'none',
    stroke: 'currentColor',
    strokeWidth: 1.8,
    strokeLinecap: 'round' as const,
    strokeLinejoin: 'round' as const,
    'aria-hidden': true,
    ...props,
  }
}

export function IconMark(props: IconProps) {
  return (
    <svg width="22" height="22" viewBox="0 0 32 32" fill="none" aria-hidden {...props}>
      <path d="M16 4.5c-4 0-7.2 2.7-7.2 6.7 0 2.6 1.1 4.4 2.1 6.4.8 1.7 1.7 3.5 1.7 5.7 0 1.9.8 3.4 3.4 3.4s3.4-1.5 3.4-3.4c0-2.2.9-4 1.7-5.7 1-2 2.1-3.8 2.1-6.4C23.2 7.2 20 4.5 16 4.5Z" fill="#E8F4F3"/>
      <path d="M16 7c1.1 0 2 .8 2 2.1 0 2.6-1.3 3.9-2 5.4-.7-1.5-2-2.8-2-5.4C14 7.8 14.9 7 16 7Z" fill="#0B1F3A"/>
    </svg>
  )
}

export const IconDashboard = (p: IconProps) => (
  <svg {...base(p)}><rect x="3" y="3" width="7" height="9" rx="1.5"/><rect x="14" y="3" width="7" height="5" rx="1.5"/><rect x="14" y="12" width="7" height="9" rx="1.5"/><rect x="3" y="16" width="7" height="5" rx="1.5"/></svg>
)
export const IconPatients = (p: IconProps) => (
  <svg {...base(p)}><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="3"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
)
export const IconCalendar = (p: IconProps) => (
  <svg {...base(p)}><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18M8 3v4M16 3v4"/></svg>
)
export const IconVisit = (p: IconProps) => (
  <svg {...base(p)}><path d="M9 11h6M12 8v6"/><path d="M4 19V7a2 2 0 0 1 2-2h8l6 6v8a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2Z"/></svg>
)
export const IconTreatment = (p: IconProps) => (
  <svg {...base(p)}><path d="M12 3v18"/><path d="M5 8h14"/><path d="M7 16h10"/></svg>
)
export const IconPayment = (p: IconProps) => (
  <svg {...base(p)}><rect x="2" y="6" width="20" height="12" rx="2"/><path d="M2 10h20"/><path d="M7 15h3"/></svg>
)
export const IconExpense = (p: IconProps) => (
  <svg {...base(p)}><path d="M12 3v18"/><path d="M17 8H9.5a3.5 3.5 0 0 0 0 7H14a3.5 3.5 0 0 1 0 7H6"/></svg>
)
export const IconSupplier = (p: IconProps) => (
  <svg {...base(p)}><path d="M3 21V8l9-5 9 5v13"/><path d="M9 21v-8h6v8"/></svg>
)
export const IconFile = (p: IconProps) => (
  <svg {...base(p)}><path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8Z"/><path d="M14 3v5h5"/></svg>
)
export const IconReport = (p: IconProps) => (
  <svg {...base(p)}><path d="M4 19V5"/><path d="M4 19h16"/><path d="M8 15l4-5 3 3 5-7"/></svg>
)
export const IconUsers = (p: IconProps) => (
  <svg {...base(p)}><circle cx="9" cy="8" r="3"/><circle cx="17" cy="9" r="2.5"/><path d="M3 20a6 6 0 0 1 12 0"/><path d="M15.5 20a5 5 0 0 1 6 0"/></svg>
)
export const IconDoctor = (p: IconProps) => (
  <svg {...base(p)}><circle cx="12" cy="8" r="3"/><path d="M6 20a6 6 0 0 1 12 0"/><path d="M12 11v3M10.5 12.5h3"/></svg>
)
export const IconProfile = (p: IconProps) => (
  <svg {...base(p)}><circle cx="12" cy="8" r="3"/><path d="M5 20a7 7 0 0 1 14 0"/></svg>
)
export const IconSearch = (p: IconProps) => (
  <svg {...base(p)}><circle cx="11" cy="11" r="7"/><path d="M20 20l-3-3"/></svg>
)
export const IconMenu = (p: IconProps) => (
  <svg {...base(p)}><path d="M4 7h16M4 12h16M4 17h16"/></svg>
)
export const IconLogout = (p: IconProps) => (
  <svg {...base(p)}><path d="M9 6H6a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h3"/><path d="M15 8l5 4-5 4M10 12h10"/></svg>
)
export const IconPlus = (p: IconProps) => (
  <svg {...base(p)}><path d="M12 5v14M5 12h14"/></svg>
)
export const IconClose = (p: IconProps) => (
  <svg {...base(p)}><path d="M6 6l12 12M18 6L6 18"/></svg>
)
export const IconAlert = (p: IconProps) => (
  <svg {...base(p)}><circle cx="12" cy="12" r="9"/><path d="M12 8v5M12 16h.01"/></svg>
)
