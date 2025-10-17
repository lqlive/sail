import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Search, MoreVertical, Edit, Trash2 } from 'lucide-react'

interface Route {
  id: string
  name: string
  path: string
  methods: string[]
  cluster: string
  status: 'active' | 'inactive'
}

export default function Routes() {
  const navigate = useNavigate()
  const [routes] = useState<Route[]>([
    {
      id: '1',
      name: 'API Gateway',
      path: '/api/*',
      methods: ['GET', 'POST'],
      cluster: 'backend-cluster',
      status: 'active',
    },
    {
      id: '2',
      name: 'Auth Service',
      path: '/auth/*',
      methods: ['POST'],
      cluster: 'auth-cluster',
      status: 'active',
    },
    {
      id: '3',
      name: 'Admin Panel',
      path: '/admin/*',
      methods: ['GET', 'POST', 'PUT', 'DELETE'],
      cluster: 'admin-cluster',
      status: 'inactive',
    },
  ])

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Routes</h1>
          <p className="mt-1 text-xs text-gray-600">
            Manage your gateway routes and endpoints
          </p>
        </div>
        <button 
          onClick={() => navigate('/routes/new')}
          className="px-4 py-2 text-sm bg-slate-800 text-white hover:bg-slate-900 rounded-lg font-medium flex items-center transition-all"
        >
          <Plus className="h-4 w-4 mr-2" />
          New Route
        </button>
      </div>

      <div className="card">
        <div className="p-3 border-b border-gray-200">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search routes..."
              className="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Route
                </th>
                <th className="px-4 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Path
                </th>
                <th className="px-4 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Methods
                </th>
                <th className="px-4 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Cluster
                </th>
                <th className="px-4 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-4 py-2.5 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {routes.map((route) => (
                <tr key={route.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {route.name}
                    </div>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <div className="text-xs text-gray-600 font-mono">
                      {route.path}
                    </div>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <div className="flex gap-1">
                      {route.methods.map((method) => (
                        <span
                          key={method}
                          className="inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800"
                        >
                          {method}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <div className="text-xs text-gray-600">{route.cluster}</div>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <span
                      className={`badge ${
                        route.status === 'active'
                          ? 'badge-success'
                          : 'badge-error'
                      }`}
                    >
                      {route.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap text-right text-sm font-medium">
                    <div className="flex items-center justify-end space-x-1">
                      <button 
                        onClick={() => navigate(`/routes/${route.id}/edit`)}
                        className="p-1.5 rounded hover:bg-gray-100"
                        title="Edit route"
                      >
                        <Edit className="h-4 w-4 text-gray-600" />
                      </button>
                      <button 
                        className="p-1.5 rounded hover:bg-gray-100"
                        title="Delete route"
                      >
                        <Trash2 className="h-4 w-4 text-red-600" />
                      </button>
                      <button 
                        className="p-1.5 rounded hover:bg-gray-100"
                        title="More options"
                      >
                        <MoreVertical className="h-4 w-4 text-gray-600" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

