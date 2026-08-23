import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '@/api/services'
import {
  clearSession,
  getStoredSession,
  persistSession,
  setUnauthorizedHandler,
} from '@/api/client'
import { isApiError } from '@/api/client'
import type { UserDto } from '@/types/api'
import { hasRole, Role } from '@/auth/roles'

interface AuthState {
  user: UserDto | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  refresh: () => Promise<void>
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null)
  const [loading, setLoading] = useState(true)

  const logout = () => {
    clearSession()
    setUser(null)
  }

  const refresh = async () => {
    const session = getStoredSession()
    if (!session?.token) {
      setUser(null)
      return
    }
    try {
      const me = await authApi.me()
      setUser(me)
    } catch (error) {
      if (isApiError(error) && (error.status === 401 || error.status === 404)) {
        logout()
        return
      }
      throw error
    }
  }

  useEffect(() => {
    setUnauthorizedHandler(() => {
      clearSession()
      setUser(null)
    })
    const session = getStoredSession()
    if (!session?.token) {
      setLoading(false)
      return
    }
    void authApi
      .me()
      .then(setUser)
      .catch((error: unknown) => {
        if (isApiError(error) && (error.status === 401 || error.status === 404)) {
          clearSession()
          setUser(null)
        }
      })
      .finally(() => setLoading(false))
    return () => setUnauthorizedHandler(null)
  }, [])

  const login = async (email: string, password: string) => {
    const result = await authApi.login({ email, password })
    persistSession(result.token, result.expiresAt)
    setUser(result.user)
  }

  const value = useMemo<AuthState>(
    () => ({ user, loading, login, logout, refresh }),
    [user, loading],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthState {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within AuthProvider')
  return context
}

export function useCan(allowed: readonly string[]): boolean {
  const { user } = useAuth()
  return hasRole(user?.role, allowed)
}

export function useIsAdmin(): boolean {
  return useCan([Role.Admin])
}
