import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  BoltIcon,
  ShieldCheckIcon,
  ClockIcon,
  ArrowPathIcon,
  FunnelIcon,
} from '@heroicons/react/24/outline';
import type { Middleware, MiddlewareType } from '../../types/gateway';
import { MiddlewareService } from '../../services/middlewareService';

const Middlewares: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [middlewares, setMiddlewares] = useState<Middleware[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState<'all' | MiddlewareType>('all');
  const [error, setError] = useState<string | null>(null);

  const loadMiddlewares = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await MiddlewareService.getMiddlewares(searchTerm);
      setMiddlewares(data);
    } catch (err) {
      console.error('Failed to load middlewares:', err);
      setError('Failed to load middlewares');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMiddlewares();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== '') {
        loadMiddlewares();
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const filteredMiddlewares = middlewares.filter(middleware => {
    if (filterType !== 'all' && middleware.type !== filterType) {
      return false;
    }
    return true;
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
      <div className="mb-8">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">Middlewares</h1>
            <p className="mt-1 text-sm text-gray-500">Configure rate limiting, CORS, and timeout policies</p>
          </div>
          <Link 
            to="/middlewares/new" 
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Middleware
          </Link>
        </div>

        <div className="flex flex-col sm:flex-row gap-4 sm:items-center sm:justify-between">
          <div className="relative flex-1 max-w-md">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="Search middlewares..."
            />
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={() => setFilterType('all')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                filterType === 'all'
                  ? 'bg-gray-900 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              All
            </button>
            <button
              onClick={() => setFilterType('Cors')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                filterType === 'Cors'
                  ? 'bg-blue-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              <ShieldCheckIcon className="h-4 w-4 mr-1.5" />
              CORS
            </button>
            <button
              onClick={() => setFilterType('RateLimiter')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                filterType === 'RateLimiter'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              <BoltIcon className="h-4 w-4 mr-1.5" />
              Rate Limiter
            </button>
            <button
              onClick={() => setFilterType('Timeout')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                filterType === 'Timeout'
                  ? 'bg-orange-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              <ClockIcon className="h-4 w-4 mr-1.5" />
              Timeout
            </button>
          </div>
        </div>

        <div className="flex items-center text-sm text-gray-500 font-medium mt-4">
          <FunnelIcon className="h-4 w-4 mr-1.5" />
          {filteredMiddlewares.length} {filteredMiddlewares.length === 1 ? 'middleware' : 'middlewares'}
        </div>
      </div>

      {error ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="h-6 w-6 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">Failed to load middlewares</h3>
          <p className="text-sm text-gray-500 mb-4">Please check your connection and try again</p>
          <button
            onClick={loadMiddlewares}
            className="inline-flex items-center px-4 py-2 bg-white border border-gray-200 text-sm font-medium rounded-lg hover:bg-gray-50 transition-colors"
          >
            Try Again
          </button>
        </div>
      ) : middlewares.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <BoltIcon className="h-6 w-6 text-gray-400" />
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">No middlewares yet</h3>
          <p className="text-sm text-gray-500 mb-4">Get started by creating your first middleware</p>
          <Link
            to="/middlewares/new"
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Middleware
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {filteredMiddlewares.map((middleware) => (
            <Link
              key={middleware.id}
              to={`/middlewares/${middleware.id}`}
              className="block bg-white border border-gray-200 rounded-lg p-5 hover:border-gray-300 hover:shadow-sm transition-all"
            >
              <div className="flex items-center gap-3 mb-4">
                <div className={`w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 ${
                  middleware.type === 'Cors' ? 'bg-blue-50' : 
                  middleware.type === 'RateLimiter' ? 'bg-indigo-50' : 
                  middleware.type === 'Timeout' ? 'bg-orange-50' : 'bg-green-50'
                }`}>
                  {middleware.type === 'Cors' ? (
                    <ShieldCheckIcon className="h-5 w-5 text-blue-600" />
                  ) : middleware.type === 'RateLimiter' ? (
                    <BoltIcon className="h-5 w-5 text-indigo-600" />
                  ) : middleware.type === 'Timeout' ? (
                    <ClockIcon className="h-5 w-5 text-orange-600" />
                  ) : (
                    <ArrowPathIcon className="h-5 w-5 text-green-600" />
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-base font-medium text-gray-900 truncate">{middleware.name}</h3>
                  <div className="flex items-center gap-2 mt-1">
                    <span className={`inline-flex items-center px-2 py-0.5 text-xs font-medium rounded ${
                      middleware.type === 'Cors'
                        ? 'bg-blue-50 text-blue-700'
                        : middleware.type === 'RateLimiter'
                          ? 'bg-indigo-50 text-indigo-700'
                          : middleware.type === 'Timeout'
                            ? 'bg-orange-50 text-orange-700'
                            : 'bg-green-50 text-green-700'
                    }`}>
                      {middleware.type === 'Cors' ? 'CORS' : 
                       middleware.type === 'RateLimiter' ? 'Rate Limiter' : 
                       middleware.type === 'Timeout' ? 'Timeout' : 'Retry'}
                    </span>
                    {middleware.enabled ? (
                      <span className="text-xs text-green-600 font-medium">● Enabled</span>
                    ) : (
                      <span className="text-xs text-gray-400">○ Disabled</span>
                    )}
                  </div>
                </div>
              </div>

              {middleware.description && (
                <p className="text-sm text-gray-500 mb-4 line-clamp-2">{middleware.description}</p>
              )}

              <div className="space-y-3">
                {middleware.cors && (
                  <div className="flex items-center justify-between py-2 border-b border-gray-100">
                    <div className="flex items-center gap-2">
                      <ShieldCheckIcon className="h-4 w-4 text-gray-400" />
                      <span className="text-sm text-gray-600">CORS Policy</span>
                    </div>
                    <span className="text-sm font-medium text-gray-900">{middleware.cors.name}</span>
                  </div>
                )}

                {middleware.rateLimiter && (
                  <div className="flex items-center justify-between py-2 border-b border-gray-100">
                    <div className="flex items-center gap-2">
                      <BoltIcon className="h-4 w-4 text-gray-400" />
                      <span className="text-sm text-gray-600">Rate Limiter</span>
                    </div>
                    <span className="text-sm font-medium text-gray-900">{middleware.rateLimiter.permitLimit}/s</span>
                  </div>
                )}

                {middleware.timeout && (
                  <div className="flex items-center justify-between py-2 border-b border-gray-100">
                    <div className="flex items-center gap-2">
                      <ClockIcon className="h-4 w-4 text-gray-400" />
                      <span className="text-sm text-gray-600">Timeout</span>
                    </div>
                    <span className="text-sm font-medium text-gray-900">{middleware.timeout.seconds}s</span>
                  </div>
                )}

                {middleware.retry && (
                  <div className="flex items-center justify-between py-2 border-b border-gray-100">
                    <div className="flex items-center gap-2">
                      <ArrowPathIcon className="h-4 w-4 text-gray-400" />
                      <span className="text-sm text-gray-600">Retry</span>
                    </div>
                    <span className="text-sm font-medium text-gray-900">{middleware.retry.maxRetryAttempts} attempts</span>
                  </div>
                )}

                {!middleware.cors && !middleware.rateLimiter && !middleware.timeout && !middleware.retry && (
                  <div className="text-sm text-gray-400 py-2">No policies configured</div>
                )}
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
};

export default Middlewares;
