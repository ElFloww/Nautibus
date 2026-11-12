import { describe, expect, it } from 'vitest'
import { delayClass, delayHex, delayLabel, lineColors, tripStopLabel, untilLabel } from './format'
import { errorMessages } from '../api/client'

describe('lineColors', () => {
  it('garde une couleur GTFS conforme', () => {
    expect(lineColors({ id: 'L', shortName: 'L', longName: null, color: 'E87FB0', textColor: 'FFFFFF', sortOrder: 1 })).toEqual({ background: '#E87FB0', color: '#FFFFFF' })
  })
  it('neutralise une valeur non conforme sans planter', () => {
    expect(lineColors({ id: 'L', shortName: 'L', longName: null, color: 'red;}', textColor: null, sortOrder: 1 }).background).toBe('#64748b')
    expect(lineColors(null).color).toBe('#ffffff')
  })
})

describe('untilLabel', () => {
  const now = new Date('2026-11-12T11:00:00Z')
  it('affiche les minutes restantes', () => expect(untilLabel('2026-11-12T11:04:20Z', now)).toBe('4 min'))
  it('signale un passage imminent ou dépassé', () => expect(untilLabel('2026-11-12T10:59:00Z', now)).toBe('à l’approche'))
  it('donne l’heure de Paris au-delà d’une heure', () => expect(untilLabel('2026-11-12T13:05:00Z', now)).toBe('14:05'))
})

describe('delayLabel', () => {
  it('considère moins d’une minute comme à l’heure', () => expect(delayLabel(-30)).toBe('à l’heure'))
  it('distingue retard et avance', () => {
    expect(delayLabel(130)).toBe('retard 2 min')
    expect(delayLabel(-156)).toBe('avance 3 min')
  })
})

describe('errorMessages', () => {
  it('lit les trois formats d’erreur de l’API', () => {
    expect(errorMessages(400, [{ code: 'ametis.data.badrequest', description: 'Limite invalide' }])).toEqual(['Limite invalide'])
    expect(errorMessages(400, { title: 'One or more validation errors occurred.', errors: { PasswordTooShort: ['Trop court'] } })).toEqual(['Trop court'])
    expect(errorMessages(401, '')).toEqual(['Connexion requise ou identifiants incorrects'])
  })
})

describe('delayClass', () => {
  it('colore à l’heure, avance, petit et gros retard différemment', () => {
    expect(delayClass(30)).toContain('emerald')
    expect(delayClass(-120)).toContain('sky')
    expect(delayClass(120)).toContain('amber')
    expect(delayClass(400)).toContain('red')
    expect(delayClass(null)).toContain('emerald')
  })
})

describe('tripStopLabel', () => {
  it('affiche heure effective, heure prévue et écart sur un arrêt desservi en retard', () => {
    expect(tripStopLabel('2026-11-12T13:44:00Z', '2026-11-12T13:49:00Z', 300, true)).toBe('passé 14:49 (prévu 14:44, retard 5 min)')
  })
  it('n’affiche que l’heure quand le bus est à l’heure', () => {
    expect(tripStopLabel('2026-11-12T13:44:00Z', '2026-11-12T13:44:30Z', 30, false)).toBe('14:44 · à l’heure')
  })
  it('colore la carte avec les mêmes seuils que le texte', () => {
    expect(delayHex(400, false)).toBe('#b91c1c')
    expect(delayHex(-120, false)).toBe('#0369a1')
  })
})
