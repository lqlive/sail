import apiClient from './client'

export interface Destination {
  destinationId: string
  address: string
  health?: string
  host?: string
}

export interface Cluster {
  clusterId: string
  destinations?: Destination[]
  loadBalancingPolicy?: string
  healthCheck?: {
    active?: {
      enabled?: boolean
      interval?: string
      timeout?: string
      path?: string
    }
    passive?: {
      enabled?: boolean
      policy?: string
    }
  }
}

export const clustersApi = {
  list: async () => {
    const response = await apiClient.get<{ clusters: Cluster[] }>('/clusters')
    return response.data
  },

  get: async (id: string) => {
    const response = await apiClient.get<Cluster>(`/clusters/${id}`)
    return response.data
  },

  create: async (cluster: Omit<Cluster, 'clusterId'>) => {
    const response = await apiClient.post<Cluster>('/clusters', cluster)
    return response.data
  },

  update: async (id: string, cluster: Partial<Cluster>) => {
    const response = await apiClient.put<Cluster>(`/clusters/${id}`, cluster)
    return response.data
  },

  delete: async (id: string) => {
    await apiClient.delete(`/clusters/${id}`)
  },
}

