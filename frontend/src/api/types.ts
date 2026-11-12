export interface LineSummary { id: string; shortName: string | null; longName: string | null; color: string | null; textColor: string | null; sortOrder: number | null }
export interface StopSummary { id: string; name: string; latitude: number; longitude: number; lines: LineSummary[] }
export interface LineDirection { directionId: number; destination: string | null; stops: StopSummary[] }
export interface LineDetail { line: LineSummary; directions: LineDirection[] }
export interface LineShape { shapeId: string; coordinates: [number, number][] }
export interface StopDetail { stop: StopSummary; quays: StopSummary[] }
export interface Departure { tripId: string; stopId: string; line: LineSummary; destination: string | null; directionId: number | null; scheduledTime: string }
export type MatchConfidence = 'None' | 'Low' | 'Medium' | 'High'
export interface RealtimeArrival {
  tripId: string; stopId: string; line: LineSummary | null; destination: string | null; expectedTime: string; delaySeconds: number | null
  provenance: string; confidence: MatchConfidence; feedAgeSeconds: number; isStale: boolean
}
export interface VehicleView {
  vehicleId: string; label: string | null; line: LineSummary | null; destination: string | null; directionId: number | null
  latitude: number; longitude: number; bearing: number | null; speedKmh: number | null; timestampUtc: string; ageSeconds: number; isStale: boolean; lineConfidence: MatchConfidence
  delaySeconds: number | null; nextStopName: string | null; isOffRoute: boolean; offRouteMeters: number | null; offRouteTrack: [number, number][]; tripId: string | null
}
export type TripStopStatus = 'Passed' | 'Next' | 'Upcoming'
export interface TripStopView { order: number; stopName: string; networkStopId: string | null; scheduledTime: string; expectedTime: string; delaySeconds: number; status: TripStopStatus; latitude: number | null; longitude: number | null }
export interface TripDetail {
  tripId: string; line: LineSummary | null; lineConfidence: MatchConfidence; destination: string | null; directionId: number | null; vehicleId: string | null
  latitude: number | null; longitude: number | null; bearing: number | null; positionAgeSeconds: number | null; feedAgeSeconds: number | null; isStale: boolean
  stops: TripStopView[]; trace: [number, number][]
}
export interface StopBoardEntry {
  line: LineSummary | null; destination: string | null; scheduledTime: string; expectedTime: string | null; delaySeconds: number | null
  tripId: string | null; isRealtime: boolean; confidence: MatchConfidence; isStale: boolean
}
export interface StopBoard { upcoming: StopBoardEntry[]; recent: StopBoardEntry[]; feedAgeSeconds: number | null }
export interface RealtimeStatus { feedTimestampUtc: string | null; collectedAtUtc: string | null; feedAgeSeconds: number | null; isStale: boolean; vehicleCount: number; alertCount: number; errors: string[] }
export interface ServiceAlert { id: string | null; header: string | null; description: string | null; url: string | null; startUtc: string | null; endUtc: string | null; routeIds: string[]; stopIds: string[] }
export interface NetworkEventView {
  id: number; kind: 'Official' | 'DetectedDetour'; title: string; description: string | null; url: string | null; lines: LineSummary[]
  startUtc: string; endUtc: string | null; closedUtc: string | null; lastSeenUtc: string; confidence: MatchConfidence; isActive: boolean
}
export type FavoriteKind = 'Stop' | 'Line'
export interface Favorite { kind: FavoriteKind; targetId: string; createdAtUtc: string }
export interface AccountInfo { email: string; emailConfirmed: boolean }
export interface FeedVersionInfo {
  id: number; source: 'Network' | 'Realtime'; status: 'Running' | 'Succeeded' | 'Failed' | 'Skipped'; isActive: boolean; startedAtUtc: string; finishedAtUtc: string | null
  routeCount: number; stopCount: number; tripCount: number; stopTimeCount: number; rejectedCount: number; validFrom: string | null; validTo: string | null; changeSummary: string | null; errorMessage: string | null
}
export interface DataHealth { status: 'Healthy' | 'Degraded' | 'Unhealthy'; messages: string[] }
