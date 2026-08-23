import { CLINIC } from '@/clinic'

export function BrandMark({ size = 38 }: { size?: number }) {
  return (
    <img
      className="brand-logo"
      src={CLINIC.logo}
      alt=""
      width={size}
      height={size}
    />
  )
}
