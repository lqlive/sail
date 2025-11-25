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

export type DestinationHealth = 'healthy' | 'unhealthy' | 'unknown';

