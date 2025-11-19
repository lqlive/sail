import { apiClient } from './api';
import type { Middleware, MiddlewareType } from '../types/gateway';

export interface CreateMiddlewareRequest {
  name: string;
  description?: string;
  type: MiddlewareType;
  enabled: boolean;
  cors?: {
    name: string;
    allowOrigins?: string[];
    allowMethods?: string[];
    allowHeaders?: string[];
    exposeHeaders?: string[];
    allowCredentials: boolean;
    maxAge?: number;
  };
  rateLimiter?: {
    name: string;
    permitLimit: number;
    window: number;
    queueLimit: number;
  };
}

export const MiddlewareService = {
  getMiddlewares: async (keywords?: string): Promise<Middleware[]> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });
    
    if (keywords) {
      queryParams.append('keywords', keywords);
    }

    return apiClient.get<Middleware[]>(`/api/middlewares?${queryParams.toString()}`);
  },

  getMiddleware: async (id: string): Promise<Middleware> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    return apiClient.get<Middleware>(`/api/middlewares/${id}?${queryParams.toString()}`);
  },

  createMiddleware: async (data: CreateMiddlewareRequest): Promise<void> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.post(`/api/middlewares?${queryParams.toString()}`, data);
  },

  updateMiddleware: async (id: string, data: CreateMiddlewareRequest): Promise<void> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.put(`/api/middlewares/${id}?${queryParams.toString()}`, data);
  },

  deleteMiddleware: async (id: string): Promise<void> => {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.delete(`/api/middlewares/${id}?${queryParams.toString()}`);
  },
};

