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

      {error ? (
        <div className="text-center py-12">
          <div className="mx-auto h-12 w-12 text-red-400 mb-4">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="mt-2 text-sm font-medium text-gray-900">Failed to load routes</h3>
          <p className="mt-1 text-sm text-gray-500">Please check your connection and try again.</p>
          <div className="mt-6">
            <button
              onClick={loadRoutes}
              className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50"
            >
              Try Again
            </button>
          </div>
        </div>
      ) : (
        <div className="space-y-3">
          {routes.length === 0 ? (
            <div className="text-center py-12 text-gray-500">
              <PlusIcon className="h-12 w-12 mx-auto mb-4 text-gray-400" />
              <p>No routes yet</p>
              <Link
                to="/routes/new"
                className="inline-block mt-4 text-sm text-gray-900 hover:underline"
              >
                Create your first route
              </Link>
            </div>
          ) : filteredRoutes.length === 0 ? (
            <div className="text-center py-12 text-gray-500">
              <p>No matching routes</p>
            </div>
          ) : (
          filteredRoutes.map((route) => {
            const path = route.match?.path || route.path || '';
            const methods = route.match?.methods || route.methods || [];
            const hosts = route.match?.hosts || route.hosts || [];
            return (
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
                <p className="text-xs text-gray-600 mb-2 font-mono">{path}</p>
                <div className="flex items-center space-x-3 text-xs text-gray-500">
                  {methods.length > 0 && (
                    <span>Methods: {methods.join(', ')}</span>
                  )}
                  {hosts.length > 0 && (
                    <span>Hosts: {hosts.join(', ')}</span>
                  )}
                  <span>Order: {route.order}</span>
                </div>
              </Link>
            );
          })
          )}
        </div>
      )}
    </div>
  );
};

export default Routes;

