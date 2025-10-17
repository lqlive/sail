import { CheckCircle, AlertCircle, Info, Clock } from 'lucide-react'

interface Activity {
  id: string
  type: 'success' | 'error' | 'info'
  message: string
  timestamp: string
}

export default function RecentActivity() {
  const activities: Activity[] = [
    {
      id: '1',
      type: 'success',
      message: 'Route "API Gateway" was successfully updated',
      timestamp: '2 minutes ago',
    },
    {
      id: '2',
      type: 'info',
      message: 'New cluster "payment-service" was added',
      timestamp: '15 minutes ago',
    },
    {
      id: '3',
      type: 'error',
      message: 'Health check failed for "backend-cluster"',
      timestamp: '1 hour ago',
    },
    {
      id: '4',
      type: 'success',
      message: 'Certificate renewed for "*.example.com"',
      timestamp: '3 hours ago',
    },
  ]

  const getIcon = (type: string) => {
    switch (type) {
      case 'success':
        return <CheckCircle className="h-4 w-4 text-emerald-600" />
      case 'error':
        return <AlertCircle className="h-4 w-4 text-rose-600" />
      case 'info':
        return <Info className="h-4 w-4 text-blue-600" />
      default:
        return null
    }
  }

  return (
    <div className="card p-4">
      <h2 className="text-base font-semibold text-gray-900 mb-3 flex items-center">
        <Clock className="h-4 w-4 mr-2 text-gray-600" />
        Recent Activity
      </h2>
      <div className="space-y-2">
        {activities.map((activity) => (
          <div 
            key={activity.id} 
            className="flex items-start space-x-2.5 p-2.5 rounded-lg hover:bg-gray-50 transition-all duration-200"
          >
            <div className="mt-0.5">
              {getIcon(activity.type)}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs text-gray-900">
                {activity.message}
              </p>
              <p className="text-xs text-gray-500 mt-0.5">
                {activity.timestamp}
              </p>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

