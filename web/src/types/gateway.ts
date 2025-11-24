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
  timeoutPolicy?: string;
  retryPolicy?: string;
  timeout?: string;
  maxRequestBodySize?: number;
  httpsRedirect?: boolean;
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

export enum ServiceDiscoveryType {
  Consul = 1,
  Dns = 2,
}

export interface ServiceDiscovery {
  id: string;
  name: string;
  type: ServiceDiscoveryType;
  enabled: boolean;
  consul?: {
    address: string;
    token?: string;
    datacenter?: string;
    refreshIntervalSeconds: number;
  };
  dns?: {
    serverAddress: string;
    port: number;
    refreshIntervalSeconds: number;
  };
  createdAt: string;
  updatedAt: string;
}

export interface Cluster {
  id: string;
  name: string;
  serviceName?: string;
  serviceDiscoveryType?: ServiceDiscoveryType;
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

export type MiddlewareType = 'Cors' | 'RateLimiter' | 'Timeout' | 'Retry';

export interface Middleware {
  id: string;
  name: string;
  description?: string;
  type: MiddlewareType;
  enabled: boolean;
  cors?: CorsConfig;
  rateLimiter?: RateLimiterConfig;
  timeout?: TimeoutConfig;
  retry?: RetryConfig;
  createdAt: string;
  updatedAt: string;
}

export interface CorsConfig {
  name: string;
  allowOrigins?: string[];
  allowMethods?: string[];
  allowHeaders?: string[];
  exposeHeaders?: string[];
  allowCredentials: boolean;
  maxAge?: number;
}

export interface RateLimiterConfig {
  name: string;
  permitLimit: number;
  window: number;
  queueLimit: number;
}

export interface TimeoutConfig {
  name: string;
  seconds: number;
  timeoutStatusCode?: number;
}

export interface RetryConfig {
  name: string;
  maxRetryAttempts: number;
  retryStatusCodes: number[];
  retryDelayMilliseconds: number;
  useExponentialBackoff: boolean;
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

export type AuthenticationSchemeType = 'JwtBearer' | 'OpenIdConnect';

export interface AuthenticationPolicy {
  id: string;
  name: string;
  type: AuthenticationSchemeType;
  enabled: boolean;
  description?: string;
  jwtBearer?: JwtBearerConfig;
  openIdConnect?: OpenIdConnectConfig;
  createdAt: string;
  updatedAt: string;
}

export interface JwtBearerConfig {
  authority: string;
  audience: string;
  requireHttpsMetadata: boolean;
  saveToken: boolean;
  validIssuers?: string[];
  validAudiences?: string[];
  validateIssuer: boolean;
  validateAudience: boolean;
  validateLifetime: boolean;
  validateIssuerSigningKey: boolean;
  clockSkew?: number;
}

export interface OpenIdConnectConfig {
  authority: string;
  clientId: string;
  clientSecret: string;
  responseType?: string;
  requireHttpsMetadata: boolean;
  saveTokens: boolean;
  getClaimsFromUserInfoEndpoint: boolean;
  scope?: string[];
  clockSkew?: number;
}
