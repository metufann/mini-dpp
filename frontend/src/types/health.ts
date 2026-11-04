export interface HealthResponse {
  service: string;
  status: string;
}

export type ConnectionStatus = "checking" | "connected" | "unavailable";
