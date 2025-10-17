import { Activity, Route, Server, Shield } from 'lucide-react'
import StatCard from '../components/StatCard'
import RecentActivity from '../components/RecentActivity'

export default function Dashboard() {
  const stats = [
    {
      name: 'Total Routes',
      value: '24',
      change: '+4.5%',
      icon: Route,
      color: 'primary',
    },
    {
      name: 'Active Clusters',
      value: '12',
      change: '+2.1%',
      icon: Server,
      color: 'green',
    },
    {
      name: 'Certificates',
      value: '8',
      change: '+1',
      icon: Shield,
      color: 'purple',
    },
    {
      name: 'Requests (24h)',
      value: '1.2M',
      change: '+12.3%',
      icon: Activity,
      color: 'blue',
    },
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Dashboard
          </h1>
          <p className="mt-1 text-xs text-gray-600">
            Welcome to Sail Gateway Admin Panel
          </p>
        </div>
        <div className="flex space-x-2">
          <button className="px-4 py-2 text-sm bg-white text-gray-700 hover:bg-gray-50 border border-gray-300 hover:border-gray-400 rounded-lg font-medium transition-all">
            View Reports
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <StatCard key={stat.name} {...stat} />
        ))}
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <div className="card p-4">
          <h2 className="text-base font-semibold text-gray-900 mb-3 flex items-center">
            <Activity className="h-4 w-4 mr-2 text-gray-600" />
            Traffic Overview
          </h2>
          <div className="h-56 flex items-center justify-center bg-gray-50 rounded-lg border border-gray-200">
            <div className="text-center">
              <Activity className="h-10 w-10 text-gray-400 mx-auto mb-2" />
              <p className="text-sm text-gray-600 font-medium">Chart will be displayed here</p>
              <p className="text-xs text-gray-500 mt-1">Real-time traffic analytics</p>
            </div>
          </div>
        </div>

        <RecentActivity />
      </div>
    </div>
  )
}

