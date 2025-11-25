export interface DestinationRuntimeState {
  clusterId: string;
  destinationId: string;
  address: string;
  host?: string;
  activeHealth: string;
  passiveHealth: string;
}

export interface ClusterRuntimeState {
  clusterId: string;
  destinationCount: number;
  destinations: DestinationRuntimeState[];
}

export const RuntimeService = {
  async getAllClusters(): Promise<ClusterRuntimeState[]> {
    try {
      const response = await fetch('/api/runtime/clusters');
      
      if (!response.ok) {
        throw new Error('Failed to fetch runtime clusters');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch runtime clusters:', error);
      return [];
    }
  },

  async getClusterDestinations(clusterId: string): Promise<DestinationRuntimeState[]> {
    try {
      const response = await fetch(`/api/runtime/clusters/${clusterId}/destinations`);
      
      if (!response.ok) {
        if (response.status === 404) {
          return [];
        }
        throw new Error('Failed to fetch cluster destinations');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch cluster destinations:', error);
      return [];
    }
  }
};

