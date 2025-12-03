export interface ResourceCount {
  total: number;
  enabled: number;
}

export interface RecentItem {
  id: string;
  name: string;
  createdAt: string;
}

export interface DashboardStatistics {
  routes: ResourceCount;
  clusters: ResourceCount;
  certificates: ResourceCount;
  middlewares: ResourceCount;
  authenticationPolicies: ResourceCount;
  recentRoutes: RecentItem[];
  recentClusters: RecentItem[];
  recentCertificates: RecentItem[];
}

