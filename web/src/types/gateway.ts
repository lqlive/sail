export interface Route {
  id: string;
  name: string;
  clusterId?: string;
  order: number;
  path?: string;
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
  match?: {
    path?: string;
    methods?: string[];
    hosts?: string[];
    headers?: Array<{
      name: string;
      values?: string[];
      mode?: number;
      isCaseSensitive?: boolean;
    }>;
    queryParameters?: Array<{
      name: string;
      values?: string[];
      mode?: number;
      isCaseSensitive?: boolean;
    }>;
  };
  transforms?: Array<Record<string, string>>;
  metadata?: Record<string, string>;
}

export interface Cluster {
  id: string;
  name: string;
  loadBalancingPolicy?: string;
  healthCheckEnabled?: boolean;
  sessionAffinityEnabled?: boolean;
  destinations: Destination[];
  createdAt: string;
  updatedAt: string;
  healthCheck?: {
    availableDestinationsPolicy?: string;
    active?: {
      enabled?: boolean;
      interval?: string;
      timeout?: string;
      policy?: string;
      path?: string;
      query?: string;
    };
    passive?: {
      enabled?: boolean;
      policy?: string;
      reactivationPeriod?: string;
    };
  };
  sessionAffinity?: {
    enabled?: boolean;
    policy?: string;
    failurePolicy?: string;
    affinityKeyName?: string;
    cookie?: {
      domain?: string;
      expiration?: string;
      httpOnly?: boolean;
      isEssential?: boolean;
      maxAge?: string;
      path?: string;
      sameSite?: number;
      securePolicy?: number;
    };
  };
  httpClient?: {
    version?: string;
    requestTimeout?: string;
    maxConnectionsPerServer?: number;
  };
}

export interface Destination {
  id: string;
  address: string;
  health?: string;
  host?: string;
}

export interface SNI {
  id: string;
  name: string;
  hostName: string;
  createdAt: string;
  updatedAt: string;
}

export interface Certificate {
  id: string;
  cert?: string;
  key?: string;
  snis?: SNI[];
  createdAt: string;
  updatedAt: string;
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

