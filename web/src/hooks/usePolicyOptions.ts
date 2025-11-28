import { useState, useEffect } from 'react';
import { MiddlewareService } from '../services/middlewareService';
import { AuthenticationPolicyService } from '../services/authenticationPolicyService';
import type { Middleware, AuthenticationPolicy } from '../types';

interface PolicyOption {
  value: string;
  label: string;
  type?: string;
}

export const usePolicyOptions = () => {
  const [authPolicies, setAuthPolicies] = useState<PolicyOption[]>([]);
  const [corsPolicies, setCorsPolicies] = useState<PolicyOption[]>([]);
  const [rateLimiterPolicies, setRateLimiterPolicies] = useState<PolicyOption[]>([]);
  const [timeoutPolicies, setTimeoutPolicies] = useState<PolicyOption[]>([]);
  const [retryPolicies, setRetryPolicies] = useState<PolicyOption[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadPolicies();
  }, []);

  const loadPolicies = async () => {
    try {
      setLoading(true);

      // Load authentication policies
      const authData = await AuthenticationPolicyService.getAuthenticationPolicies();
      const enabledAuthPolicies = authData
        .filter((p: AuthenticationPolicy) => p.enabled)
        .map((p: AuthenticationPolicy) => ({
          value: p.name,
          label: p.name,
          type: p.type,
        }));
      setAuthPolicies(enabledAuthPolicies);

      // Load middlewares
      const middlewares = await MiddlewareService.getMiddlewares();
      const enabledMiddlewares = middlewares.filter((m: Middleware) => m.enabled);

      // Extract CORS policies
      const cors = enabledMiddlewares
        .filter((m: Middleware) => m.type === 'Cors' && m.cors)
        .map((m: Middleware) => ({
          value: m.cors!.name,
          label: m.cors!.name,
          type: 'CORS',
        }));
      setCorsPolicies(cors);

      // Extract Rate Limiter policies
      const rateLimiter = enabledMiddlewares
        .filter((m: Middleware) => m.type === 'RateLimiter' && m.rateLimiter)
        .map((m: Middleware) => ({
          value: m.rateLimiter!.name,
          label: m.rateLimiter!.name,
          type: 'Rate Limiter',
        }));
      setRateLimiterPolicies(rateLimiter);

      // Extract Timeout policies
      const timeout = enabledMiddlewares
        .filter((m: Middleware) => m.type === 'Timeout' && m.timeout)
        .map((m: Middleware) => ({
          value: m.timeout!.name,
          label: m.timeout!.name,
          type: 'Timeout',
        }));
      setTimeoutPolicies(timeout);

      // Extract Retry policies
      const retry = enabledMiddlewares
        .filter((m: Middleware) => m.type === 'Retry' && m.retry)
        .map((m: Middleware) => ({
          value: m.retry!.name,
          label: m.retry!.name,
          type: 'Retry',
        }));
      setRetryPolicies(retry);
    } catch (error) {
      console.error('Failed to load policy options:', error);
    } finally {
      setLoading(false);
    }
  };

  return {
    authPolicies,
    corsPolicies,
    rateLimiterPolicies,
    timeoutPolicies,
    retryPolicies,
    loading,
    refresh: loadPolicies,
  };
};

