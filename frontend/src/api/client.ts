import type {
  NetworkEventView,
  StopBoard,
  TripDetail,
  AccountInfo, DataHealth, Departure, Favorite, FavoriteKind, FeedVersionInfo, LineDetail, LineShape, LineSummary,
  RealtimeArrival, RealtimeStatus, ServiceAlert, StopDetail, StopSummary, VehicleView
} from './types'

/** Erreur renvoyée par l'API : statut HTTP et messages lisibles. */
export class ApiError extends Error {
  constructor(public readonly status: number, public readonly messages: string[]) {
    super(messages[0] ?? `Erreur ${status}`)
  }
}

const base = '/api/v1'
const e = encodeURIComponent

async function request<T>(p_Path: string, p_Init: RequestInit = {}): Promise<T> {
  const v_Response = await fetch(p_Path.startsWith('/') ? p_Path : `${base}/${p_Path}`, {
    credentials: 'same-origin',
    ...p_Init,
    headers: { Accept: 'application/json', ...(p_Init.body ? { 'Content-Type': 'application/json' } : {}), ...p_Init.headers }
  })
  const v_Text = await v_Response.text()
  const v_Body = v_Text ? safeJson(v_Text) : undefined
  if (!v_Response.ok) throw new ApiError(v_Response.status, errorMessages(v_Response.status, v_Body))
  return v_Body as T
}

function safeJson(p_Text: string): unknown {
  try { return JSON.parse(p_Text) } catch { return p_Text }
}

/** Messages des différents formats d'erreur : liste d'ErrorDto, problème HTTP Identity, ou rien. */
export function errorMessages(p_Status: number, p_Body: unknown): string[] {
  if (Array.isArray(p_Body)) return p_Body.map((p_E) => String((p_E as { description?: string }).description ?? p_E))
  if (p_Body && typeof p_Body === 'object') {
    const v_Errors = (p_Body as { errors?: Record<string, string[]> }).errors
    if (v_Errors) return Object.values(v_Errors).flat()
    const v_Title = (p_Body as { title?: string; detail?: string }).detail ?? (p_Body as { title?: string }).title
    if (v_Title) return [v_Title]
  }
  if (p_Status === 401) return ['Connexion requise ou identifiants incorrects']
  if (p_Status === 429) return ['Trop de tentatives, réessayez dans une minute']
  return [`Le service a répondu avec une erreur (${p_Status})`]
}

export const api = {
  lines: (p_Query = '') => request<LineSummary[]>(`lines?q=${e(p_Query)}`),
  line: (p_Id: string) => request<LineDetail>(`lines/${e(p_Id)}`),
  lineShapes: (p_Id: string) => request<LineShape[]>(`lines/${e(p_Id)}/shapes`),
  stops: (p_Params: { q?: string; lat?: number; lon?: number; radius?: number; limit?: number }) => {
    const v_Query = new URLSearchParams(Object.entries(p_Params).filter(([, p_V]) => p_V !== undefined && p_V !== '').map(([p_K, p_V]) => [p_K, String(p_V)]))
    return request<StopSummary[]>(`stops?${v_Query}`)
  },
  stop: (p_Id: string) => request<StopDetail>(`stops/${e(p_Id)}`),
  departures: (p_Id: string, p_Limit = 20) => request<Departure[]>(`stops/${e(p_Id)}/departures?limit=${p_Limit}`),
  stopBoard: (p_Id: string) => request<StopBoard>(`stops/${e(p_Id)}/board`),
  trip: (p_TripId: string) => request<TripDetail>(`trips/${e(p_TripId)}`),
  stopRealtime: (p_Id: string) => request<RealtimeArrival[]>(`stops/${e(p_Id)}/realtime`),
  vehicles: () => request<VehicleView[]>('vehicles'),
  realtimeStatus: () => request<RealtimeStatus>('vehicles/status'),
  alerts: () => request<ServiceAlert[]>('alerts'),
  events: (p_Days = 7) => request<NetworkEventView[]>(`events?days=${p_Days}`),
  pushKey: () => request<string>('notifications/key'),
  subscribePush: (p_Body: { endpoint: string; p256dh: string; auth: string; allLines: boolean }) =>
    request<boolean>('notifications/subscription', { method: 'PUT', body: JSON.stringify(p_Body) }),
  unsubscribePush: (p_Endpoint: string) =>
    request<boolean>('notifications/unsubscribe', { method: 'POST', body: JSON.stringify({ endpoint: p_Endpoint, p256dh: '-', auth: '-', allLines: false }) }),
  feeds: () => request<FeedVersionInfo[]>('feeds?count=20'),
  dataHealth: () => request<DataHealth>('feeds/health'),
  favorites: () => request<Favorite[]>('favorites'),
  addFavorite: (p_Kind: FavoriteKind, p_Id: string) => request<boolean>(`favorites/${p_Kind}/${e(p_Id)}`, { method: 'PUT' }),
  removeFavorite: (p_Kind: FavoriteKind, p_Id: string) => request<boolean>(`favorites/${p_Kind}/${e(p_Id)}`, { method: 'DELETE' }),
  me: () => request<AccountInfo>('accounts/me'),
  logout: () => request<void>('accounts/logout', { method: 'POST' }),
  deleteAccount: () => request<boolean>('accounts/me', { method: 'DELETE' }),
  register: (p_Email: string, p_Password: string) => request<void>('auth/register', { method: 'POST', body: JSON.stringify({ email: p_Email, password: p_Password }) }),
  confirmEmail: (p_UserId: string, p_Code: string) => request<unknown>(`auth/confirmEmail?userId=${e(p_UserId)}&code=${e(p_Code)}`),
  resendConfirmation: (p_Email: string) => request<void>('auth/resendConfirmationEmail', { method: 'POST', body: JSON.stringify({ email: p_Email }) }),
  forgotPassword: (p_Email: string) => request<void>('auth/forgotPassword', { method: 'POST', body: JSON.stringify({ email: p_Email }) }),
  resetPassword: (p_Email: string, p_Code: string, p_Password: string) =>
    request<void>('auth/resetPassword', { method: 'POST', body: JSON.stringify({ email: p_Email, resetCode: p_Code, newPassword: p_Password }) }),
  login: (p_Email: string, p_Password: string) => request<void>('auth/login?useCookies=true', { method: 'POST', body: JSON.stringify({ email: p_Email, password: p_Password }) })
}
