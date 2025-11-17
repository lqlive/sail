import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  CheckCircleIcon,
  XCircleIcon,
} from '@heroicons/react/24/outline';
import type { Route } from '../../types/gateway';

const mockRoutes: Route[] = [
  {
    id: '1',
    name: 'API Gateway',
    clusterId: 'cluster-1',
    order: 1,
    path: '/api/{**catch-all}',
    methods: ['GET', 'POST', 'PUT', 'DELETE'],
    hosts: ['api.example.com'],
    enabled: true,
    authorizationPolicy: 'default',
    rateLimiterPolicy: 'standard',
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-15T10:00:00Z',
  },
  {
    id: '2',
    name: 'Web Application',
    clusterId: 'cluster-2',
    order: 2,
    path: '/{**catch-all}',
    methods: ['GET'],
    hosts: ['www.example.com', 'example.com'],
    enabled: true,
    createdAt: '2024-01-14T09:00:00Z',
    updatedAt: '2024-01-14T09:00:00Z',
  },
  {
    id: '3',
    name: 'Admin Portal',
    clusterId: 'cluster-3',
    order: 3,
    path: '/admin/{**catch-all}',
    methods: ['GET', 'POST'],
    hosts: ['admin.example.com'],
    enabled: false,
    authorizationPolicy: 'admin-only',
    createdAt: '2024-01-13T08:00:00Z',
    updatedAt: '2024-01-16T14:30:00Z',
  },
];

const Routes: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [routes, setRoutes] = useState<Route[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all');

  useEffect(() => {
    setLoading(true);
    setTimeout(() => {
      setRoutes(mockRoutes);
      setLoading(false);
    }, 500);
  }, []);

  const filteredRoutes = routes.filter(route => {
    const matchesSearch = route.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         route.path.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = filterStatus === 'all' ||
                         (filterStatus === 'active' && route.enabled) ||
                         (filterStatus === 'inactive' && !route.enabled);
    return matchesSearch && matchesStatus;
  });

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="section-header">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-xl font-medium text-gray-900">Routes</h1>
            <p className="text-sm text-gray-600">Manage gateway routing rules</p>
          </div>
          <Link to="/routes/new" className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-gray-900 hover:bg-gray-800">
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Route
          </Link>
        </div>
      </div>

      <div className="mb-6">
        <div className="relative mb-4">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <MagnifyingGlassIcon className="h-4 w-4 text-gray-400" />
          </div>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-gray-900 focus:border-gray-900 sm:text-sm"
            placeholder="Search routes by name or path..."
          />
        </div>

        <div className="flex items-center space-x-3">
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value as 'all' | 'active' | 'inactive')}
            className="text-xs border border-gray-300 rounded-md px-2 py-1 focus:outline-none focus:ring-2 focus:ring-gray-900 focus:border-transparent"
          >
            <option value="all">All status</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
          <span className="text-sm text-gray-500">{filteredRoutes.length} routes</span>
        </div>
      </div>

      <div className="space-y-3">
        {filteredRoutes.map((route) => (
          <Link
            key={route.id}
            to={`/routes/${route.id}/edit`}
            className="block bg-white border border-gray-200 rounded-lg p-4 hover:border-gray-300 transition-colors"
          >
            <div className="flex items-center space-x-2 mb-1.5">
              <h3 className="text-sm font-medium text-gray-900">{route.name}</h3>
              {route.enabled ? (
                <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs bg-green-100 text-green-800">
                  <CheckCircleIcon className="h-3 w-3 mr-1" />
                  Active
                </span>
              ) : (
                <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs bg-gray-100 text-gray-800">
                  <XCircleIcon className="h-3 w-3 mr-1" />
                  Inactive
                </span>
              )}
            </div>
            <p className="text-xs text-gray-600 mb-2 font-mono">{route.path}</p>
            <div className="flex items-center space-x-3 text-xs text-gray-500">
              {route.methods && route.methods.length > 0 && (
                <span>Methods: {route.methods.join(', ')}</span>
              )}
              {route.hosts && route.hosts.length > 0 && (
                <span>Hosts: {route.hosts.join(', ')}</span>
              )}
              <span>Order: {route.order}</span>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};

export default Routes;

