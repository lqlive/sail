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

