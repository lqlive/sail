export interface Route {
  id: string;
  name: string;
  clusterId?: string;
  order: number;
  path: string;
  methods?: string[];
  hosts?: string[];
  enabled: boolean;
  authorizationPolicy?: string;
  rateLimiterPolicy?: string;
  corsPolicy?: string;
  timeout?: string;
  maxRequestBodySize?: number;
  createdAt: string;
  updatedAt: string;
}

export interface Cluster {
  id: string;
  name: string;
  loadBalancingPolicy?: string;
  healthCheckEnabled: boolean;
  sessionAffinityEnabled: boolean;
  destinations: Destination[];
  createdAt: string;
  updatedAt: string;
}

export interface Destination {
  id: string;
  address: string;
  health?: string;
  host?: string;
}

export interface Certificate {
  id: string;
  name: string;
  subject: string;
  issuer: string;
  notBefore: string;
  notAfter: string;
  thumbprint: string;
  enabled: boolean;
  createdAt: string;
}

export interface GatewayStats {
  totalRoutes: number;
  activeRoutes: number;
  totalClusters: number;
  activeClusters: number;
  totalDestinations: number;
  healthyDestinations: number;
  totalCertificates: number;
  validCertificates: number;
  expiringCertificates: number;
}

export type RouteStatus = 'active' | 'inactive' | 'error';
export type DestinationHealth = 'healthy' | 'unhealthy' | 'unknown';

