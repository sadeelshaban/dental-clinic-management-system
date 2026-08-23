export function LineChart({
  labels,
  series,
}: {
  labels: string[]
  series: { name: string; values: number[]; color: string }[]
}) {
  const width = 720
  const height = 260
  const pad = { l: 48, r: 16, t: 18, b: 36 }
  const innerW = width - pad.l - pad.r
  const innerH = height - pad.t - pad.b
  const all = series.flatMap((s) => s.values)
  const max = Math.max(1, ...all, 0)
  const min = Math.min(0, ...all)
  const span = max - min || 1
  const x = (i: number) => pad.l + (labels.length <= 1 ? innerW / 2 : (i / (labels.length - 1)) * innerW)
  const y = (v: number) => pad.t + innerH - ((v - min) / span) * innerH

  return (
    <svg className="chart" viewBox={`0 0 ${width} ${height}`} role="img" aria-label="Financial trend">
      {[0, 0.5, 1].map((t) => {
        const value = min + span * (1 - t)
        const yy = pad.t + innerH * t
        return (
          <g key={t}>
            <line x1={pad.l} x2={width - pad.r} y1={yy} y2={yy} stroke="#e4ebea" />
            <text x={8} y={yy + 4}>{Math.round(value)}</text>
          </g>
        )
      })}
      {series.map((s) => {
        const d = s.values.map((v, i) => `${i === 0 ? 'M' : 'L'} ${x(i)} ${y(v)}`).join(' ')
        return (
          <g key={s.name}>
            <path d={d} fill="none" stroke={s.color} strokeWidth="2.4" />
            {s.values.map((v, i) => (
              <circle key={i} cx={x(i)} cy={y(v)} r="3.2" fill={s.color} />
            ))}
          </g>
        )
      })}
      {labels.map((label, i) => (
        <text key={label + i} x={x(i)} y={height - 10} textAnchor="middle">
          {label}
        </text>
      ))}
    </svg>
  )
}

export function BarCompare({
  items,
}: {
  items: { label: string; current: number; previous: number | null }[]
}) {
  const width = 720
  const height = 240
  const max = Math.max(1, ...items.flatMap((i) => [i.current, i.previous ?? 0]))
  const barW = 18
  const gap = width / Math.max(items.length, 1)

  return (
    <svg className="chart" viewBox={`0 0 ${width} ${height}`} role="img" aria-label="Monthly comparison">
      {items.map((item, i) => {
        const cx = gap * i + gap / 2
        const h1 = (item.current / max) * 170
        const h2 = ((item.previous ?? 0) / max) * 170
        return (
          <g key={item.label}>
            <rect x={cx - barW - 4} y={190 - h2} width={barW} height={h2} rx="4" fill="#c5d4d3" />
            <rect x={cx + 4} y={190 - h1} width={barW} height={h1} rx="4" fill="#1a8a86" />
            <text x={cx} y={220} textAnchor="middle">{item.label}</text>
          </g>
        )
      })}
    </svg>
  )
}
