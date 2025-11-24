import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  HomeIcon,
  GlobeAltIcon,
  ServerStackIcon,
  ShieldCheckIcon,
  BoltIcon,
  KeyIcon,
  Cog6ToothIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
} from '@heroicons/react/24/outline';
import classNames from 'classnames';

interface NavigationItem {
  name: string;
  href: string;
  icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
}

const navigation: NavigationItem[] = [
  { name: 'Dashboard', href: '/', icon: HomeIcon },
  { name: 'Routes', href: '/routes', icon: GlobeAltIcon },
  { name: 'Clusters', href: '/clusters', icon: ServerStackIcon },
  { name: 'Certificates', href: '/certificates', icon: ShieldCheckIcon },
  { name: 'Middlewares', href: '/middlewares', icon: BoltIcon },
  { name: 'Authentication', href: '/authentication-policies', icon: KeyIcon },
  { name: 'Settings', href: '/settings', icon: Cog6ToothIcon },
];

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ collapsed, onToggle }) => {
  const location = useLocation();

  const isActive = (href: string) => {
    if (href === '/') {
      return location.pathname === '/';
    }
    return location.pathname.startsWith(href);
  };

  return (
    <div
      className={classNames(
        'fixed left-0 top-0 h-full bg-white border-r border-gray-100 transition-all duration-300 z-40',
        collapsed ? 'w-16' : 'w-64'
      )}
    >
      {/* Logo Section */}
      <div className="flex items-center justify-between h-14 px-4 border-b border-gray-100">
        {!collapsed && (
          <div className="flex items-center">
            <GlobeAltIcon className="h-5 w-5 text-gray-900" />
            <span className="ml-2 text-sm font-semibold text-gray-900">Sail Gateway</span>
          </div>
        )}
        {collapsed && (
          <div className="flex items-center justify-center w-full">
            <GlobeAltIcon className="h-5 w-5 text-gray-900" />
          </div>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-3 py-4 space-y-1">
        {navigation.map((item) => {
          const Icon = item.icon;
          const active = isActive(item.href);

          return (
            <Link
              key={item.name}
              to={item.href}
              className={classNames(
                'flex items-center px-3 py-2.5 text-sm font-medium rounded-lg transition-all duration-200',
                active
                  ? 'bg-gray-100 text-gray-900'
                  : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
              )}
              title={collapsed ? item.name : undefined}
            >
              <Icon className={classNames('h-5 w-5 flex-shrink-0', collapsed ? '' : 'mr-3')} />
              {!collapsed && <span>{item.name}</span>}
            </Link>
          );
        })}
      </nav>

      {/* Toggle Button */}
      <div className="absolute bottom-4 left-0 right-0 px-3">
        <button
          onClick={onToggle}
          className={classNames(
            'flex items-center justify-center w-full px-3 py-2 text-sm font-medium text-gray-600 rounded-lg transition-all duration-200',
            'hover:bg-gray-50 hover:text-gray-900'
          )}
          title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {collapsed ? (
            <ChevronRightIcon className="h-5 w-5" />
          ) : (
            <>
              <ChevronLeftIcon className="h-5 w-5 mr-3" />
              <span>Collapse</span>
            </>
          )}
        </button>
      </div>
    </div>
  );
};

export default Sidebar;

