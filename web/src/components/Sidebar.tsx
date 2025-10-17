import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard,
  Route,
  Server,
  Shield,
  Settings,
} from 'lucide-react'

const navigation = [
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { name: 'Routes', href: '/routes', icon: Route },
  { name: 'Clusters', href: '/clusters', icon: Server },
  { name: 'Certificates', href: '/certificates', icon: Shield },
  { name: 'Settings', href: '/settings', icon: Settings },
]

export default function Sidebar() {
  return (
    <div className="w-64 bg-white border-r border-gray-200 flex flex-col">
      <div className="flex items-center justify-center h-16 border-b border-gray-200 px-4 flex-shrink-0">
        <div className="flex items-center space-x-3">
          <div className="text-3xl">⛵</div>
          <div>
            <h1 className="text-xl font-bold text-gray-900">
              Sail Gateway
            </h1>
            <p className="text-xs text-gray-500">Admin Panel</p>
          </div>
        </div>
      </div>
      <nav className="p-4 space-y-1 flex-1 overflow-y-auto">
        {navigation.map((item) => (
          <NavLink
            key={item.name}
            to={item.href}
            className={({ isActive }) =>
              `group flex items-center px-4 py-3 text-sm font-medium rounded-lg transition-all duration-200 ${
                isActive
                  ? 'bg-slate-900 text-white'
                  : 'text-gray-700 hover:bg-gray-100'
              }`
            }
          >
            <item.icon className="mr-3 h-5 w-5" />
            {item.name}
          </NavLink>
        ))}
      </nav>
      
      <div className="p-4 border-t border-gray-200 bg-gray-50 flex-shrink-0">
        <p className="text-xs text-gray-600 font-medium">Version 1.0.0</p>
        <p className="text-xs text-gray-500 mt-1">© 2025 Sail Gateway</p>
      </div>
    </div>
  )
}

