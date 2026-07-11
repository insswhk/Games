import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { api } from '../api/client'
import type { AuthResponse, Permission } from '../types'

interface AuthContextValue {
  user: AuthResponse | null
  permissions: Permission[]
  login: (userName: string, password: string) => Promise<void>
  logout: () => void
  canOpen: (formName: string) => boolean
  canViewReports: (formName: string) => boolean
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const storedUser = localStorage.getItem('gameCenterUser')

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthResponse | null>(storedUser ? JSON.parse(storedUser) : null)
  const [permissions, setPermissions] = useState<Permission[]>([])

  useEffect(() => {
    if (!user || permissions.length > 0) return
    api.get<Permission[]>('/auth/permissions')
      .then((response) => setPermissions(response.data))
      .catch(() => logout())
  }, [permissions.length, user])

  async function login(userName: string, password: string) {
    const { data } = await api.post<AuthResponse>('/auth/login', { userName, password })
    localStorage.setItem('gameCenterToken', data.token)
    localStorage.setItem('gameCenterUser', JSON.stringify(data))
    setUser(data)
    const permissionResponse = await api.get<Permission[]>('/auth/permissions')
    setPermissions(permissionResponse.data)
  }

  function logout() {
    localStorage.removeItem('gameCenterToken')
    localStorage.removeItem('gameCenterUser')
    setUser(null)
    setPermissions([])
  }

  const value = useMemo<AuthContextValue>(() => ({
    user,
    permissions,
    login,
    logout,
    canOpen: (formName: string) => user?.role === 'Admin' || permissions.some((permission) => permission.formName === formName && permission.canOpen),
    canViewReports: (formName: string) => user?.role === 'Admin' || permissions.some((permission) => permission.formName === formName && permission.canViewReports),
  }), [permissions, user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
