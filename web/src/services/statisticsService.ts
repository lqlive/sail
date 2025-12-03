import { apiClient } from './api';
import type { RecentItem, ResourceCount, DashboardStatistics } from '../types/statistics';

export const StatisticsService = {
  async getAllStatistics(): Promise<DashboardStatistics> {
    const [
      routes,
      clusters,
      certificates,
      middlewares,
      authenticationPolicies,
      recentRoutes,
      recentClusters,
      recentCertificates,
    ] = await Promise.allSettled([
      this.getRouteStatistics(),
      this.getClusterStatistics(),
      this.getCertificateStatistics(),
      this.getMiddlewareStatistics(),
      this.getAuthenticationPolicyStatistics(),
      this.getRecentRoutes(),
      this.getRecentClusters(),
      this.getRecentCertificates(),
    ]);

    return {
      routes: routes.status === 'fulfilled' ? routes.value : { total: 0, enabled: 0 },
      clusters: clusters.status === 'fulfilled' ? clusters.value : { total: 0, enabled: 0 },
      certificates: certificates.status === 'fulfilled' ? certificates.value : { total: 0, enabled: 0 },
      middlewares: middlewares.status === 'fulfilled' ? middlewares.value : { total: 0, enabled: 0 },
      authenticationPolicies: authenticationPolicies.status === 'fulfilled' ? authenticationPolicies.value : { total: 0, enabled: 0 },
      recentRoutes: recentRoutes.status === 'fulfilled' ? recentRoutes.value : [],
      recentClusters: recentClusters.status === 'fulfilled' ? recentClusters.value : [],
      recentCertificates: recentCertificates.status === 'fulfilled' ? recentCertificates.value : [],
    };
  },

  async getRouteStatistics(): Promise<ResourceCount> {
    return await apiClient.get<ResourceCount>('/api/statistics/resources/routes');
  },

  async getClusterStatistics(): Promise<ResourceCount> {
    return await apiClient.get<ResourceCount>('/api/statistics/resources/clusters');
  },

  async getCertificateStatistics(): Promise<ResourceCount> {
    return await apiClient.get<ResourceCount>('/api/statistics/resources/certificates');
  },

  async getMiddlewareStatistics(): Promise<ResourceCount> {
    return await apiClient.get<ResourceCount>('/api/statistics/resources/middlewares');
  },

  async getAuthenticationPolicyStatistics(): Promise<ResourceCount> {
    return await apiClient.get<ResourceCount>('/api/statistics/resources/authentication-policies');
  },

  async getRecentRoutes(): Promise<RecentItem[]> {
    const response = await apiClient.get<{ items: RecentItem[] }>('/api/statistics/recent/routes');
    return response.items;
  },

  async getRecentClusters(): Promise<RecentItem[]> {
    const response = await apiClient.get<{ items: RecentItem[] }>('/api/statistics/recent/clusters');
    return response.items;
  },

  async getRecentCertificates(): Promise<RecentItem[]> {
    const response = await apiClient.get<{ items: RecentItem[] }>('/api/statistics/recent/certificates');
    return response.items;
  },
};

