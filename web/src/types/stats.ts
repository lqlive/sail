export interface GatewayStats {
  totalRoutes: number;
  activeRoutes: number;
  totalClusters: number;
  activeClusters: number;
  totalDestinations: number;
  healthyDestinations: number;
  totalCertificates: number;
  validCertificates: number;
  expiringCertificates: number;
}

