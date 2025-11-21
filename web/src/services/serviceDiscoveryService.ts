import { apiClient } from './api';
import type { ServiceDiscovery, ServiceDiscoveryType } from '../types/gateway';

export interface CreateServiceDiscoveryRequest {
  name: string;
  type: ServiceDiscoveryType;
  enabled: boolean;
  consul?: {
    address: string;
    token?: string;
    datacenter?: string;
    refreshIntervalSeconds?: number;
  };
  dns?: {
    serverAddress: string;
    port?: number;
    refreshIntervalSeconds?: number;
  };
}

export const ServiceDiscoveryService = {
  async getServiceDiscoveries(keywords?: string): Promise<ServiceDiscovery[]> {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });
    
    if (keywords) {
      queryParams.append('keywords', keywords);
    }

    return apiClient.get<ServiceDiscovery[]>(`/api/service-discoveries?${queryParams.toString()}`);
  },

  async getServiceDiscovery(id: string): Promise<ServiceDiscovery> {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    return apiClient.get<ServiceDiscovery>(`/api/service-discoveries/${id}?${queryParams.toString()}`);
  },

  async createServiceDiscovery(data: CreateServiceDiscoveryRequest): Promise<void> {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.post(`/api/service-discoveries?${queryParams.toString()}`, data);
  },

  async updateServiceDiscovery(id: string, data: CreateServiceDiscoveryRequest): Promise<void> {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.put(`/api/service-discoveries/${id}?${queryParams.toString()}`, data);
  },

  async deleteServiceDiscovery(id: string): Promise<void> {
    const queryParams = new URLSearchParams({
      'api-version': '1.0',
    });

    await apiClient.delete(`/api/service-discoveries/${id}?${queryParams.toString()}`);
  },
};

