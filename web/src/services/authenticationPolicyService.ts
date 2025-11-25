import { apiClient } from './api';
import type { AuthenticationPolicy, AuthenticationSchemeType, JwtBearerConfig, OpenIdConnectConfig } from '../types';

export interface CreateAuthenticationPolicyRequest {
  name: string;
  type: AuthenticationSchemeType;
  enabled: boolean;
  description?: string;
  jwtBearer?: JwtBearerConfig;
  openIdConnect?: OpenIdConnectConfig;
}

export const AuthenticationPolicyService = {
  getAuthenticationPolicies: async (keywords?: string): Promise<AuthenticationPolicy[]> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });
    
    if (keywords) {
      queryParams.append('keywords', keywords);
    }

    return apiClient.get<AuthenticationPolicy[]>(`/api/authentication-policies?${queryParams.toString()}`);
  },

  getAuthenticationPolicy: async (id: string): Promise<AuthenticationPolicy> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    return apiClient.get<AuthenticationPolicy>(`/api/authentication-policies/${id}?${queryParams.toString()}`);
  },

  createAuthenticationPolicy: async (data: CreateAuthenticationPolicyRequest): Promise<void> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.post(`/api/authentication-policies?${queryParams.toString()}`, data);
  },

  updateAuthenticationPolicy: async (id: string, data: CreateAuthenticationPolicyRequest): Promise<void> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.put(`/api/authentication-policies/${id}?${queryParams.toString()}`, data);
  },

  deleteAuthenticationPolicy: async (id: string): Promise<void> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.delete(`/api/authentication-policies/${id}?${queryParams.toString()}`);
  },
};

