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

export type RouteStatus = 'active' | 'inactive' | 'error';

