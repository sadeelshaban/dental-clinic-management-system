export class ApiError extends Error {
  status: number
  code: 'unauthorized' | 'forbidden' | 'not_found' | 'validation' | 'network' | 'conflict' | 'unknown'

  constructor(message: string, status: number, code: ApiError['code'] = 'unknown') {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError
}

function readEnvelope(json: unknown): { success?: boolean; message?: string; data?: unknown } {
  if (!json || typeof json !== 'object') return {}
  const record = json as Record<string, unknown>
  return {
    success: Boolean(record.success ?? record.Success),
    message: typeof (record.message ?? record.Message) === 'string'
      ? String(record.message ?? record.Message)
      : undefined,
    data: record.data ?? record.Data,
  }
}

function messageFromBody(json: unknown, fallback: string): string {
  const envelope = readEnvelope(json)
  if (envelope.message) return envelope.message
  if (json && typeof json === 'object') {
    const record = json as Record<string, unknown>
    const title = record.title ?? record.Title
    const errors = record.errors ?? record.Errors
    if (typeof title === 'string' && title.trim()) return title
    if (errors && typeof errors === 'object') {
      const first = Object.values(errors as Record<string, unknown>)[0]
      if (Array.isArray(first) && typeof first[0] === 'string') return first[0]
    }
  }
  return fallback
}

export function toQuery(params: object): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params as Record<string, unknown>)) {
    if (value === undefined || value === null || value === '') continue
    search.set(key, String(value))
  }
  const qs = search.toString()
  return qs ? `?${qs}` : ''
}

const SESSION_KEY = 'dcms.session'

interface StoredSession {
  token: string
  expiresAt: string
}

let memoryToken: string | null = null
let unauthorizedHandler: (() => void) | null = null

export function setUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler
}

export function getStoredSession(): StoredSession | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as StoredSession
    if (!parsed.token) return null
    return parsed
  } catch {
    return null
  }
}

export function persistSession(token: string, expiresAt: string): void {
  memoryToken = token
  sessionStorage.setItem(SESSION_KEY, JSON.stringify({ token, expiresAt }))
}

export function clearSession(): void {
  memoryToken = null
  sessionStorage.removeItem(SESSION_KEY)
}

export function getAccessToken(): string | null {
  if (memoryToken) return memoryToken
  const stored = getStoredSession()
  memoryToken = stored?.token ?? null
  return memoryToken
}

function apiBase(): string {
  const value = import.meta.env.VITE_API_BASE_URL ?? ''
  return value.replace(/\/$/, '')
}

function resolveUrl(path: string): string {
  if (path.startsWith('http://') || path.startsWith('https://')) return path
  return `${apiBase()}${path.startsWith('/') ? path : `/${path}`}`
}

async function parseBody(response: Response): Promise<unknown> {
  const text = await response.text()
  if (!text) return null
  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
}

function statusCode(status: number): ApiError['code'] {
  if (status === 401) return 'unauthorized'
  if (status === 403) return 'forbidden'
  if (status === 404) return 'not_found'
  if (status === 400 || status === 409 || status === 422) return 'validation'
  return 'unknown'
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit & { skipAuth?: boolean; raw?: boolean } = {},
): Promise<T> {
  const headers = new Headers(options.headers)
  if (!headers.has('Accept')) headers.set('Accept', 'application/json')

  const isForm = options.body instanceof FormData
  if (!isForm && options.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  if (!options.skipAuth) {
    const token = getAccessToken()
    if (token) headers.set('Authorization', `Bearer ${token}`)
  }

  let response: Response
  try {
    response = await fetch(resolveUrl(path), {
      ...options,
      headers,
    })
  } catch {
    throw new ApiError('Unable to reach the clinic server. Check your connection and try again.', 0, 'network')
  }

  if (response.status === 401 && !options.skipAuth) {
    unauthorizedHandler?.()
    throw new ApiError('Your session has expired. Please sign in again.', 401, 'unauthorized')
  }

  if (options.raw) {
    if (!response.ok) {
      const body = await parseBody(response)
      throw new ApiError(messageFromBody(body, response.statusText || 'Request failed'), response.status, statusCode(response.status))
    }
    return response as unknown as T
  }

  const body = await parseBody(response)
  const envelope = readEnvelope(body)

  if (!response.ok) {
    throw new ApiError(
      messageFromBody(body, fallbackMessage(response.status)),
      response.status,
      statusCode(response.status),
    )
  }

  if (envelope.success === false) {
    throw new ApiError(envelope.message || 'Request failed.', response.status, statusCode(response.status))
  }

  return (envelope.data as T) ?? (body as T)
}

function fallbackMessage(status: number): string {
  if (status === 403) return 'You do not have permission to perform this action.'
  if (status === 404) return 'The requested record was not found.'
  if (status === 429) return 'Too many attempts. Please wait and try again.'
  return 'The request could not be completed.'
}

export function get<T>(path: string): Promise<T> {
  return apiRequest<T>(path, { method: 'GET' })
}

export function post<T>(path: string, body?: unknown): Promise<T> {
  return apiRequest<T>(path, {
    method: 'POST',
    body: body === undefined ? undefined : JSON.stringify(body),
  })
}

export function put<T>(path: string, body?: unknown): Promise<T> {
  return apiRequest<T>(path, {
    method: 'PUT',
    body: body === undefined ? undefined : JSON.stringify(body),
  })
}

export function del<T>(path: string): Promise<T> {
  return apiRequest<T>(path, { method: 'DELETE' })
}

export function postForm<T>(path: string, form: FormData): Promise<T> {
  return apiRequest<T>(path, { method: 'POST', body: form })
}
