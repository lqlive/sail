import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  CheckCircleIcon,
  XCircleIcon,
} from '@heroicons/react/24/outline';
import type { Route } from '../../types/gateway';
import { RouteService } from '../../services/routeService';

const Routes: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [routes, setRoutes] = useState<Route[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all');
  const [error, setError] = useState<string | null>(null);

  const loadRoutes = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await RouteService.getRoutes(searchTerm);
      setRoutes(data);
    } catch (err) {
      console.error('Failed to load routes:', err);
      setError('Failed to load routes');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRoutes();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== '') {
        loadRoutes();
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const filteredRoutes = routes.filter(route => {
    const path = route.match?.path || route.path || '';
    const matchesSearch = route.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         path.toLowerCase().includes(searchTerm.toLowerCase());
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
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">Routes</h1>
            <p className="mt-1 text-sm text-gray-500">Manage gateway routing rules</p>
          </div>
          <Link 
            to="/routes/new" 
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Route
          </Link>
        </div>

        {/* Search and Filter Bar */}
        <div className="flex items-center gap-3">
          <div className="relative flex-1 max-w-md">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="Search routes by name or path..."
            />
          </div>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value as 'all' | 'active' | 'inactive')}
            className="px-3 py-2.5 text-sm border border-gray-200 rounded-lg focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 bg-white transition-colors"
          >
            <option value="all">All status</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
          <span className="text-sm text-gray-500 font-medium">{filteredRoutes.length} routes</span>
        </div>
      </div>

      {error ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="h-6 w-6 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">Failed to load routes</h3>
          <p className="text-sm text-gray-500 mb-4">Please check your connection and try again</p>
          <button
            onClick={loadRoutes}
            className="inline-flex items-center px-4 py-2 bg-white border border-gray-200 text-sm font-medium rounded-lg hover:bg-gray-50 transition-colors"
          >
            Try Again
          </button>
        </div>
      ) : routes.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <PlusIcon className="h-6 w-6 text-gray-400" />
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">No routes yet</h3>
          <p className="text-sm text-gray-500 mb-4">Get started by creating your first route</p>
          <Link
            to="/routes/new"
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Route
          </Link>
        </div>
      ) : filteredRoutes.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <p className="text-sm text-gray-500">No routes match your search</p>
        </div>
      ) : (
        <div className="space-y-3">
          {filteredRoutes.map((route) => {
            const path = route.match?.path || route.path || '';
            const methods = route.match?.methods || route.methods || [];
            const hosts = route.match?.hosts || route.hosts || [];
            return (
              <Link
                key={route.id}
                to={`/routes/${route.id}/edit`}
                className="block bg-white border border-gray-200 rounded-lg p-5 hover:border-gray-300 hover:shadow-sm transition-all"
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="flex items-center gap-3">
                    <h3 className="text-base font-medium text-gray-900">{route.name}</h3>
                    {route.enabled ? (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-50 text-green-700 border border-green-100">
                        <CheckCircleIcon className="h-3.5 w-3.5 mr-1" />
                        Active
                      </span>
                    ) : (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-50 text-gray-600 border border-gray-200">
                        <XCircleIcon className="h-3.5 w-3.5 mr-1" />
                        Inactive
                      </span>
                    )}
                  </div>
                  <span className="text-xs text-gray-400">Order: {route.order}</span>
                </div>
                
                <div className="mb-3">
                  <code className="text-sm text-gray-700 bg-gray-50 px-2 py-1 rounded border border-gray-100">{path}</code>
                </div>
                
                <div className="flex items-center gap-4 text-xs text-gray-500">
                  {methods.length > 0 && (
                    <div className="flex items-center gap-1">
                      <span className="font-medium text-gray-600">Methods:</span>
                      <span>{methods.join(', ')}</span>
                    </div>
                  )}
                  {hosts.length > 0 && (
                    <div className="flex items-center gap-1">
                      <span className="font-medium text-gray-600">Hosts:</span>
                      <span>{hosts.slice(0, 2).join(', ')}{hosts.length > 2 && ` +${hosts.length - 2}`}</span>
                    </div>
                  )}
                </div>
              </Link>
            );
          })
          }
        </div>
      )}
    </div>
  );
};

export default Routes;

