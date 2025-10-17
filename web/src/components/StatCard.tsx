import { LucideIcon } from 'lucide-react'

interface StatCardProps {
  name: string
  value: string
  change: string
  icon: LucideIcon
  color: string
}

export default function StatCard({ name, value, change, icon: Icon, color }: StatCardProps) {
  const colorClasses = {
    primary: 'bg-slate-100 text-slate-700',
    green: 'bg-emerald-100 text-emerald-700',
    purple: 'bg-purple-100 text-purple-700',
    blue: 'bg-blue-100 text-blue-700',
  }

  return (
    <div className="card p-4">
      <div className="flex items-center justify-between">
        <div className="flex-1">
          <p className="text-xs font-medium text-gray-600">{name}</p>
          <p className="mt-1.5 text-2xl font-bold text-gray-900">{value}</p>
          <div className="mt-1.5 flex items-center">
            <span className="text-xs font-medium text-emerald-600">{change}</span>
            <span className="ml-2 text-xs text-gray-500">from last month</span>
          </div>
        </div>
        <div className={`p-2.5 rounded-lg ${colorClasses[color as keyof typeof colorClasses]}`}>
          <Icon className="h-5 w-5" />
        </div>
      </div>
    </div>
  )
}

