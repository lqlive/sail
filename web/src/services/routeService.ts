import { apiClient } from './api';
import type { Route } from '../types';

export interface CreateRouteRequest {
  name: string;
  clusterId: string;
  order?: number;
  maxRequestBodySize?: number;
  authorizationPolicy?: string;
  rateLimiterPolicy?: string;
  corsPolicy?: string;
  outputCachePolicy?: string;
  timeoutPolicy?: string;
  timeout?: string;
  match?: {
    path?: string;
    methods?: string[];
    hosts?: string[];
    headers?: Array<{
      name: string;
      values?: string[];
      mode?: string;
      isCaseSensitive?: boolean;
    }>;
    queryParameters?: Array<{
      name: string;
      values?: string[];
      mode?: string;
      isCaseSensitive?: boolean;
    }>;
  };
  transforms?: Array<Record<string, string>>;
  metadata?: Record<string, string>;
}

export interface UpdateRouteRequest extends CreateRouteRequest {
  id?: string;
}

export interface RouteListResponse {
  items: Route[];
  totalCount?: number;
}

export class RouteService {
  private static readonly API_VERSION = '1.0';
  private static readonly BASE_PATH = '/api/routes';

  static async getRoutes(keywords: string = ''): Promise<Route[]> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });
      
      if (keywords) {
        queryParams.append('keywords', keywords);
      }

      const response = await apiClient.get<Route[] | RouteListResponse>(
        `${this.BASE_PATH}?${queryParams.toString()}`
      );
      
      if (Array.isArray(response)) {
        return response;
      }
      
      return response.items || [];
    } catch (error) {
      console.error('Failed to fetch routes:', error);
      throw error;
    }
  }

  static async getRoute(id: string): Promise<Route> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      const route = await apiClient.get<Route>(
        `${this.BASE_PATH}/${id}?${queryParams.toString()}`
      );
      
      return route;
    } catch (error) {
      console.error(`Failed to fetch route ${id}:`, error);
      throw error;
    }
  }

  static async createRoute(data: CreateRouteRequest): Promise<Route> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      const route = await apiClient.post<Route>(
        `${this.BASE_PATH}?${queryParams.toString()}`,
        data
      );
      
      return route;
    } catch (error) {
      console.error('Failed to create route:', error);
      throw error;
    }
  }

  static async updateRoute(id: string, data: UpdateRouteRequest): Promise<Route> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      const route = await apiClient.put<Route>(
        `${this.BASE_PATH}/${id}?${queryParams.toString()}`,
        data
      );
      
      return route;
    } catch (error) {
      console.error(`Failed to update route ${id}:`, error);
      throw error;
    }
  }

  static async deleteRoute(id: string): Promise<void> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      await apiClient.delete(
        `${this.BASE_PATH}/${id}?${queryParams.toString()}`
      );
    } catch (error) {
      console.error(`Failed to delete route ${id}:`, error);
      throw error;
    }
  }
}

