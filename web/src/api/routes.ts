import apiClient from './client'

export interface Route {
  routeId: string
  clusterId: string
  match: {
    path?: string
    hosts?: string[]
    methods?: string[]
  }
  order?: number
  authorizationPolicy?: string
  rateLimiterPolicy?: string
}

export const routesApi = {
  list: async () => {
    const response = await apiClient.get<{ routes: Route[] }>('/routes')
    return response.data
  },

  get: async (id: string) => {
    const response = await apiClient.get<Route>(`/routes/${id}`)
    return response.data
  },

  create: async (route: Omit<Route, 'routeId'>) => {
    const response = await apiClient.post<Route>('/routes', route)
    return response.data
  },

  update: async (id: string, route: Partial<Route>) => {
    const response = await apiClient.put<Route>(`/routes/${id}`, route)
    return response.data
  },

  delete: async (id: string) => {
    await apiClient.delete(`/routes/${id}`)
  },
}

