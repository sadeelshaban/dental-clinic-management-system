import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Header } from '@/components/layout/Header'
import { Sidebar } from '@/components/layout/Sidebar'

export function AppShell() {
  const [open, setOpen] = useState(false)
  return (
    <div className="app-shell">
      <Sidebar open={open} onNavigate={() => setOpen(false)} />
      {open ? (
        <button type="button" className="modal-backdrop mobile-only" aria-label="Close navigation" onClick={() => setOpen(false)} />
      ) : null}
      <div className="app-main">
        <Header onMenu={() => setOpen(true)} />
        <main className="page">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
