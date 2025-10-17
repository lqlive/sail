import { Bell, User, Search } from 'lucide-react'

export default function Header() {
  return (
    <header className="bg-white border-b border-gray-200 h-16 flex items-center justify-between px-6">
      <div className="flex-1 max-w-xl">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search..."
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500 transition-all duration-200 text-sm"
          />
        </div>
      </div>
      
      <div className="flex items-center space-x-2">
        <button className="relative p-2 rounded-lg hover:bg-gray-100 transition-all duration-200">
          <Bell className="h-5 w-5 text-gray-600" />
          <span className="absolute top-1.5 right-1.5 h-2 w-2 bg-red-500 rounded-full"></span>
        </button>
        
        <div className="h-6 w-px bg-gray-300 mx-2"></div>
        
        <button className="flex items-center space-x-2 px-3 py-2 rounded-lg hover:bg-gray-100 transition-all duration-200">
          <div className="h-8 w-8 rounded-lg bg-slate-700 flex items-center justify-center">
            <User className="h-4 w-4 text-white" />
          </div>
          <div className="text-left">
            <p className="text-sm font-medium text-gray-900">Admin</p>
          </div>
        </button>
      </div>
    </header>
  )
}

