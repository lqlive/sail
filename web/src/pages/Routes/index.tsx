import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  CheckCircleIcon,
  XCircleIcon,
  TrashIcon,
} from '@heroicons/react/24/outline';
import type { Route } from '../../types';
import { RouteService } from '../../services/routeService';

const Routes: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [routes, setRoutes] = useState<Route[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all');
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<{ id: string; name: string } | null>(null);

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

  const handleDelete = async (id: string) => {
    try {
      setDeletingId(id);
      await RouteService.deleteRoute(id);
      await loadRoutes();
      setDeleteConfirm(null);
    } catch (err) {
      console.error('Failed to delete route:', err);
      setError('Failed to delete route');
    } finally {
      setDeletingId(null);
    }
  };

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
            className="btn-primary"
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
            className="btn-primary"
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
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4 gap-4 max-w-7xl">
          {filteredRoutes.map((route) => {
            const path = route.match?.path || route.path || '';
            const methods = route.match?.methods || route.methods || [];
            const hosts = route.match?.hosts || route.hosts || [];
            const policyCount = [route.authorizationPolicy, route.rateLimiterPolicy, route.corsPolicy, route.timeoutPolicy, route.retryPolicy].filter(Boolean).length;
            const transformCount = route.transforms?.length || 0;
            
            return (
              <div
                key={route.id}
                className="bg-white border border-gray-200 rounded-lg hover:border-gray-300 hover:shadow-sm transition-all relative"
              >
                {/* Delete Button - Top Right Corner */}
                <button
                  onClick={(e) => {
                    e.preventDefault();
                    setDeleteConfirm({ id: route.id, name: route.name });
                  }}
                  className="absolute top-3 right-3 p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-md transition-colors z-10"
                  title="Delete route"
                >
                  <TrashIcon className="h-4 w-4" />
                </button>

                <Link
                  to={`/routes/${route.id}/edit`}
                  className="block p-4"
                >
                  {/* Header with Icon */}
                  <div className="flex items-center gap-3 mb-3 pr-8">
                    <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center flex-shrink-0">
                      <svg className="h-5 w-5 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
                      </svg>
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className="text-sm font-medium text-gray-900 truncate">{route.name}</h3>
                      <div className="flex items-center gap-1.5 mt-1 flex-wrap">
                        {route.enabled ? (
                          <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded bg-green-50 text-green-700">
                            <CheckCircleIcon className="h-3 w-3 mr-0.5" />
                            Active
                          </span>
                        ) : (
                          <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded bg-gray-100 text-gray-600">
                            <XCircleIcon className="h-3 w-3 mr-0.5" />
                            Inactive
                          </span>
                        )}
                        <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded bg-gray-100 text-gray-600">
                          #{route.order}
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Path */}
                  <div className="mb-3 p-2 bg-gray-50 rounded border border-gray-100">
                    <code className="text-xs font-mono text-gray-700 break-all">{path}</code>
                  </div>

                  {/* Info Rows */}
                  <div className="space-y-2">
                    {/* Methods */}
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <div className="flex items-center gap-1.5">
                        <svg className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                        </svg>
                        <span className="text-xs text-gray-600">Methods</span>
                      </div>
                      <div className="flex flex-wrap gap-1 justify-end">
                        {methods.length > 0 ? (
                          methods.slice(0, 3).map((method) => (
                            <span key={method} className="text-xs font-medium text-blue-700">
                              {method}
                            </span>
                          ))
                        ) : (
                          <span className="text-xs text-gray-400">All</span>
                        )}
                        {methods.length > 3 && (
                          <span className="text-xs text-gray-400">+{methods.length - 3}</span>
                        )}
                      </div>
                    </div>

                    {/* Hosts */}
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <div className="flex items-center gap-1.5">
                        <svg className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                        </svg>
                        <span className="text-xs text-gray-600">Hosts</span>
                      </div>
                      <span className="text-xs font-medium text-gray-900 truncate ml-2 max-w-[120px]" title={hosts[0]}>
                        {hosts.length > 0 ? (
                          <>
                            {hosts[0]}
                            {hosts.length > 1 && ` +${hosts.length - 1}`}
                          </>
                        ) : (
                          <span className="text-gray-400">All</span>
                        )}
                      </span>
                    </div>

                    {/* Policies */}
                    {policyCount > 0 && (
                      <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                        <div className="flex items-center gap-1.5">
                          <svg className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                          </svg>
                          <span className="text-xs text-gray-600">Policies</span>
                        </div>
                        <span className="text-xs font-medium text-gray-900">{policyCount}</span>
                      </div>
                    )}

                    {/* Transforms */}
                    {transformCount > 0 && (
                      <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                        <div className="flex items-center gap-1.5">
                          <svg className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                          </svg>
                          <span className="text-xs text-gray-600">Transforms</span>
                        </div>
                        <span className="text-xs font-medium text-gray-900">{transformCount}</span>
                      </div>
                    )}

                    {/* HTTPS Redirect */}
                    {route.httpsRedirect && (
                      <div className="flex items-center justify-between py-1.5">
                        <div className="flex items-center gap-1.5">
                          <svg className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                          </svg>
                          <span className="text-xs text-gray-600">HTTPS Redirect</span>
                        </div>
                        <CheckCircleIcon className="h-4 w-4 text-green-600" />
                      </div>
                    )}
                  </div>
                </Link>
              </div>
            );
          })
          }
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6 shadow-xl">
            <div className="flex items-start gap-4 mb-4">
              <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center flex-shrink-0">
                <TrashIcon className="h-5 w-5 text-red-600" />
              </div>
              <div className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900 mb-1">Delete Route</h3>
                <p className="text-sm text-gray-600">
                  Are you sure you want to delete <span className="font-medium text-gray-900">"{deleteConfirm.name}"</span>? This action cannot be undone.
                </p>
              </div>
            </div>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setDeleteConfirm(null)}
                disabled={deletingId === deleteConfirm.id}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={() => handleDelete(deleteConfirm.id)}
                disabled={deletingId === deleteConfirm.id}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {deletingId === deleteConfirm.id ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                    Deleting...
                  </>
                ) : (
                  <>
                    <TrashIcon className="h-4 w-4" />
                    Delete
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Routes;

