import { apiClient } from './api';
import type { Cluster, Destination } from '../types/gateway';

export interface ClusterHealthCheck {
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
}

export interface ClusterSessionAffinity {
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
}

export interface CreateClusterRequest {
  name: string;
  serviceName?: string;
  serviceDiscoveryType?: number;
  loadBalancingPolicy?: string;
  healthCheck?: ClusterHealthCheck;
  sessionAffinity?: ClusterSessionAffinity;
  destinations?: Array<{
    address: string;
    health?: string;
    host?: string;
  }>;
}

export interface UpdateClusterRequest {
  name: string;
  serviceName?: string;
  serviceDiscoveryType?: number;
  loadBalancingPolicy?: string;
  healthCheck?: ClusterHealthCheck;
  sessionAffinity?: ClusterSessionAffinity;
  destinations?: Array<{
    id?: string;
    address: string;
    health?: string;
    host?: string;
  }>;
}

export interface ClusterListResponse {
  items: Cluster[];
  totalCount?: number;
}

export class ClusterService {
  private static readonly API_VERSION = '1.0';
  private static readonly BASE_PATH = '/api/clusters';

  static async getClusters(keywords: string = ''): Promise<Cluster[]> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });
      
      if (keywords) {
        queryParams.append('keywords', keywords);
      }

      const response = await apiClient.get<Cluster[] | ClusterListResponse>(
        `${this.BASE_PATH}?${queryParams.toString()}`
      );
      
      if (Array.isArray(response)) {
        return response;
      }
      
      return response.items || [];
    } catch (error) {
      console.error('Failed to fetch clusters:', error);
      throw error;
    }
  }

  static async getCluster(id: string): Promise<Cluster> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      const cluster = await apiClient.get<Cluster>(
        `${this.BASE_PATH}/${id}?${queryParams.toString()}`
      );
      
      return cluster;
    } catch (error) {
      console.error(`Failed to fetch cluster ${id}:`, error);
      throw error;
    }
  }

  static async createCluster(data: CreateClusterRequest): Promise<Cluster> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      const cluster = await apiClient.post<Cluster>(
        `${this.BASE_PATH}?${queryParams.toString()}`,
        data
      );
      
      return cluster;
    } catch (error) {
      console.error('Failed to create cluster:', error);
      throw error;
    }
  }

  static async updateCluster(id: string, data: UpdateClusterRequest): Promise<Cluster> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      const cluster = await apiClient.put<Cluster>(
        `${this.BASE_PATH}/${id}?${queryParams.toString()}`,
        data
      );
      
      return cluster;
    } catch (error) {
      console.error(`Failed to update cluster ${id}:`, error);
      throw error;
    }
  }

  static async deleteCluster(id: string): Promise<void> {
    try {
      const queryParams = new URLSearchParams({
        'api-version': this.API_VERSION,
      });

      await apiClient.delete(
        `${this.BASE_PATH}/${id}?${queryParams.toString()}`
      );
    } catch (error) {
      console.error(`Failed to delete cluster ${id}:`, error);
      throw error;
    }
  }
}

