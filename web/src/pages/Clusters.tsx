import { useState } from 'react'
import { Plus, Search, Activity, AlertCircle } from 'lucide-react'

interface Cluster {
  id: string
  name: string
  destinations: number
  healthyDestinations: number
  loadBalancer: string
  status: 'healthy' | 'degraded' | 'unhealthy'
}

export default function Clusters() {
  const [clusters] = useState<Cluster[]>([
    {
      id: '1',
      name: 'backend-cluster',
      destinations: 3,
      healthyDestinations: 3,
      loadBalancer: 'RoundRobin',
      status: 'healthy',
    },
    {
      id: '2',
      name: 'auth-cluster',
      destinations: 2,
      healthyDestinations: 2,
      loadBalancer: 'LeastRequests',
      status: 'healthy',
    },
    {
      id: '3',
      name: 'admin-cluster',
      destinations: 4,
      healthyDestinations: 2,
      loadBalancer: 'PowerOfTwoChoices',
      status: 'degraded',
    },
  ])

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'healthy':
        return 'text-green-600 bg-green-100'
      case 'degraded':
        return 'text-yellow-600 bg-yellow-100'
      case 'unhealthy':
        return 'text-red-600 bg-red-100'
      default:
        return 'text-gray-600 bg-gray-100'
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Clusters</h1>
          <p className="mt-1 text-xs text-gray-600">
            Manage backend service clusters and destinations
          </p>
        </div>
        <button className="px-4 py-2 text-sm bg-slate-800 text-white hover:bg-slate-900 rounded-lg font-medium flex items-center transition-all">
          <Plus className="h-4 w-4 mr-2" />
          New Cluster
        </button>
      </div>

      <div className="card">
        <div className="p-3 border-b border-gray-200">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search clusters..."
              className="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 p-4">
          {clusters.map((cluster) => (
            <div
              key={cluster.id}
              className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between mb-3">
                <h3 className="text-base font-semibold text-gray-900">
                  {cluster.name}
                </h3>
                <span
                  className={`p-1.5 rounded-full ${getStatusColor(
                    cluster.status
                  )}`}
                >
                  {cluster.status === 'healthy' ? (
                    <Activity className="h-3.5 w-3.5" />
                  ) : (
                    <AlertCircle className="h-3.5 w-3.5" />
                  )}
                </span>
              </div>

              <div className="space-y-2">
                <div className="flex justify-between text-xs">
                  <span className="text-gray-600">Destinations</span>
                  <span className="font-medium">
                    {cluster.healthyDestinations}/{cluster.destinations}
                  </span>
                </div>
                <div className="flex justify-between text-xs">
                  <span className="text-gray-600">Load Balancer</span>
                  <span className="font-medium text-gray-900">
                    {cluster.loadBalancer}
                  </span>
                </div>
                <div className="flex justify-between text-xs">
                  <span className="text-gray-600">Status</span>
                  <span
                    className={`badge ${
                      cluster.status === 'healthy'
                        ? 'badge-success'
                        : cluster.status === 'degraded'
                        ? 'badge-warning'
                        : 'badge-error'
                    }`}
                  >
                    {cluster.status}
                  </span>
                </div>
              </div>

              <div className="mt-3 pt-3 border-t border-gray-200">
                <button className="w-full text-xs font-medium text-slate-700 hover:text-slate-900">
                  View Details
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

