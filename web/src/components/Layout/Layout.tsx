import React, { useState } from 'react';
import { Bars3Icon, XMarkIcon } from '@heroicons/react/24/outline';
import classNames from 'classnames';
import Sidebar from '../Sidebar';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const toggleSidebar = () => {
    setSidebarCollapsed(!sidebarCollapsed);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Desktop Sidebar */}
      <div className="hidden md:block">
        <Sidebar collapsed={sidebarCollapsed} onToggle={toggleSidebar} />
      </div>

      {/* Mobile Sidebar */}
      {mobileMenuOpen && (
        <div className="md:hidden fixed inset-0 z-50">
          <div 
            className="fixed inset-0 bg-gray-900 bg-opacity-50" 
            onClick={() => setMobileMenuOpen(false)} 
          />
          <div className="fixed top-0 left-0 w-64 h-full bg-white shadow-lg">
            <Sidebar collapsed={false} onToggle={() => setMobileMenuOpen(false)} />
          </div>
        </div>
      )}

      {/* Mobile Header */}
      <div className="md:hidden fixed top-0 left-0 right-0 h-14 bg-white border-b border-gray-100 z-30 flex items-center justify-between px-4">
        <span className="text-sm font-semibold text-gray-900">Sail Gateway</span>
        <button
          type="button"
          className="p-2 text-gray-600 hover:text-gray-900"
          onClick={() => setMobileMenuOpen(true)}
        >
          <Bars3Icon className="h-5 w-5" />
        </button>
      </div>

      {/* Main Content Area */}
      <main
        className={classNames(
          'transition-all duration-300',
          'md:ml-16 md:pt-0 pt-14',
          !sidebarCollapsed && 'md:ml-64'
        )}
      >
        <div className="px-6 py-8">
          {children}
        </div>
      </main>
    </div>
  );
};

export default Layout; 