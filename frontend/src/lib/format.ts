import type { LineSummary, MatchConfidence } from '../api/types'

const hex = /^[0-9a-f]{6}$/i
const fallbackColor = '64748b'

/** Couleur GTFS sûre pour le style (valeur publiée non conforme : gris neutre, la donnée n'est pas modifiée). */
export function lineColors(p_Line: LineSummary | null | undefined): { background: string; color: string } {
  const v_Bg = p_Line?.color && hex.test(p_Line.color) ? p_Line.color : fallbackColor
  const v_Fg = p_Line?.textColor && hex.test(p_Line.textColor) ? p_Line.textColor : 'ffffff'
  return { background: `#${v_Bg}`, color: `#${v_Fg}` }
}

const timeFormat = new Intl.DateTimeFormat('fr-FR', { hour: '2-digit', minute: '2-digit', timeZone: 'Europe/Paris' })

export function clock(p_Iso: string): string {
  return timeFormat.format(new Date(p_Iso))
}

/** « à l'approche », « 3 min » ou l'heure au-delà d'une heure. */
export function untilLabel(p_Iso: string, p_Now: Date = new Date()): string {
  const v_Minutes = Math.round((new Date(p_Iso).getTime() - p_Now.getTime()) / 60000)
  if (v_Minutes <= 0) return 'à l’approche'
  if (v_Minutes < 60) return `${v_Minutes} min`
  return clock(p_Iso)
}

export function delayLabel(p_Seconds: number | null): string {
  if (p_Seconds === null || Math.abs(p_Seconds) < 60) return 'à l’heure'
  const v_Minutes = Math.round(Math.abs(p_Seconds) / 60)
  return p_Seconds > 0 ? `retard ${v_Minutes} min` : `avance ${v_Minutes} min`
}

/** Couleur : à l'heure (vert), retard (orange puis rouge au-delà de 5 min), avance (bleu). */
export function delayClass(p_Seconds: number | null): string {
  if (p_Seconds === null || Math.abs(p_Seconds) < 60) return 'text-emerald-700 dark:text-emerald-300'
  if (p_Seconds < 0) return 'text-sky-700 dark:text-sky-300'
  return p_Seconds >= 300 ? 'text-red-700 dark:text-red-400' : 'text-amber-700 dark:text-amber-300'
}

/** Texte court d'un arrêt de course : heure effective, heure prévue si différente, écart. */
export function tripStopLabel(p_Scheduled: string, p_Expected: string, p_Delay: number, p_Passed: boolean): string {
  const v_Shift = Math.abs(p_Delay) >= 60
  const v_Prefix = p_Passed ? 'passé ' : ''
  return v_Shift
    ? `${v_Prefix}${clock(p_Expected)} (prévu ${clock(p_Scheduled)}, ${delayLabel(p_Delay)})`
    : `${v_Prefix}${clock(p_Expected)} · à l’heure`
}

/** Couleur hexadécimale de l'écart, pour la carte (mêmes seuils que delayClass). */
export function delayHex(p_Seconds: number | null, p_Dark: boolean): string {
  if (p_Seconds === null || Math.abs(p_Seconds) < 60) return p_Dark ? '#6ee7b7' : '#047857'
  if (p_Seconds < 0) return p_Dark ? '#7dd3fc' : '#0369a1'
  return p_Seconds >= 300 ? (p_Dark ? '#f87171' : '#b91c1c') : (p_Dark ? '#fcd34d' : '#b45309')
}

export function ageLabel(p_Seconds: number | null): string {
  if (p_Seconds === null) return 'inconnu'
  if (p_Seconds < 60) return `il y a ${p_Seconds} s`
  return `il y a ${Math.round(p_Seconds / 60)} min`
}

export const confidenceLabel: Record<MatchConfidence, string> = {
  High: 'fiable',
  Medium: 'probable',
  Low: 'incertain',
  None: 'non rattaché'
}
