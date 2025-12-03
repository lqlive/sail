import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  GlobeAltIcon,
  ServerStackIcon,
  ShieldCheckIcon,
  ArrowPathIcon,
  CpuChipIcon,
  KeyIcon,
} from '@heroicons/react/24/outline';
import { StatisticsService } from '../services/statisticsService';
import type { DashboardStatistics } from '../types/statistics';

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [statistics, setStatistics] = useState<DashboardStatistics | null>(null);

  useEffect(() => {
    loadAllStatistics();
  }, []);

  const loadAllStatistics = async () => {
    try {
      setLoading(true);
      const data = await StatisticsService.getAllStatistics();
      setStatistics(data);
    } catch (err) {
      console.error('Failed to load statistics:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  };


  return (
    <div className="fade-in">
      <div className="flex items-center justify-between mb-5">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-0.5 text-sm text-gray-600">Gateway overview and statistics</p>
        </div>
        <button
          onClick={loadAllStatistics}
          className="inline-flex items-center gap-2 px-3 py-1.5 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 hover:border-gray-300 transition-all shadow-sm"
        >
          <ArrowPathIcon className="h-4 w-4" />
          Refresh
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-3 mb-5">
        <Link
          to="/routes"
          className="group bg-white border border-gray-200 rounded-xl p-4 hover:shadow-lg hover:border-blue-200 transition-all duration-200"
        >
          <div className="flex items-center justify-between mb-3">
            <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center flex-shrink-0 group-hover:bg-blue-100 transition-colors">
              <GlobeAltIcon className="h-5 w-5 text-blue-600" />
            </div>
          </div>
          <div className="mb-2">
            <div className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1.5">Routes</div>
            {loading ? (
              <div className="h-8 flex items-center">
                <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
              </div>
            ) : statistics ? (
              <div className="text-2xl font-bold text-gray-900">{statistics.routes.total}</div>
            ) : (
              <div className="text-2xl font-bold text-gray-400">-</div>
            )}
          </div>
          <div className="flex items-center text-xs text-gray-600 pt-2 border-t border-gray-100">
            {loading ? (
              <span className="text-gray-400">Loading...</span>
            ) : statistics ? (
              <>
                <div className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></div>
                <span className="font-semibold text-gray-900">{statistics.routes.enabled}</span>
                <span className="ml-1">enabled</span>
              </>
            ) : (
              <span className="text-gray-400">-</span>
            )}
          </div>
        </Link>

        <Link
          to="/clusters"
          className="group bg-white border border-gray-200 rounded-xl p-4 hover:shadow-lg hover:border-purple-200 transition-all duration-200"
        >
          <div className="flex items-center justify-between mb-3">
            <div className="w-10 h-10 bg-purple-50 rounded-xl flex items-center justify-center flex-shrink-0 group-hover:bg-purple-100 transition-colors">
              <ServerStackIcon className="h-5 w-5 text-purple-600" />
            </div>
          </div>
          <div className="mb-2">
            <div className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1.5">Clusters</div>
            {loading ? (
              <div className="h-8 flex items-center">
                <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-purple-600"></div>
              </div>
            ) : statistics ? (
              <div className="text-2xl font-bold text-gray-900">{statistics.clusters.total}</div>
            ) : (
              <div className="text-2xl font-bold text-gray-400">-</div>
            )}
          </div>
          <div className="flex items-center text-xs text-gray-600 pt-2 border-t border-gray-100">
            {loading ? (
              <span className="text-gray-400">Loading...</span>
            ) : statistics ? (
              <>
                <div className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></div>
                <span className="font-semibold text-gray-900">{statistics.clusters.enabled}</span>
                <span className="ml-1">enabled</span>
              </>
            ) : (
              <span className="text-gray-400">-</span>
            )}
          </div>
        </Link>

        <Link
          to="/certificates"
          className="group bg-white border border-gray-200 rounded-xl p-4 hover:shadow-lg hover:border-green-200 transition-all duration-200"
        >
          <div className="flex items-center justify-between mb-3">
            <div className="w-10 h-10 bg-green-50 rounded-xl flex items-center justify-center flex-shrink-0 group-hover:bg-green-100 transition-colors">
              <KeyIcon className="h-5 w-5 text-green-600" />
            </div>
          </div>
          <div className="mb-2">
            <div className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1.5">Certificates</div>
            {loading ? (
              <div className="h-8 flex items-center">
                <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-green-600"></div>
              </div>
            ) : statistics ? (
              <div className="text-2xl font-bold text-gray-900">{statistics.certificates.total}</div>
            ) : (
              <div className="text-2xl font-bold text-gray-400">-</div>
            )}
          </div>
          <div className="flex items-center text-xs text-gray-600 pt-2 border-t border-gray-100">
            {loading ? (
              <span className="text-gray-400">Loading...</span>
            ) : (
              <>
                <div className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></div>
                <span className="text-gray-600">SSL/TLS</span>
              </>
            )}
          </div>
        </Link>

        <Link
          to="/middlewares"
          className="group bg-white border border-gray-200 rounded-xl p-4 hover:shadow-lg hover:border-orange-200 transition-all duration-200"
        >
          <div className="flex items-center justify-between mb-3">
            <div className="w-10 h-10 bg-orange-50 rounded-xl flex items-center justify-center flex-shrink-0 group-hover:bg-orange-100 transition-colors">
              <CpuChipIcon className="h-5 w-5 text-orange-600" />
            </div>
          </div>
          <div className="mb-2">
            <div className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1.5">Middlewares</div>
            {loading ? (
              <div className="h-8 flex items-center">
                <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-orange-600"></div>
              </div>
            ) : statistics ? (
              <div className="text-2xl font-bold text-gray-900">{statistics.middlewares.total}</div>
            ) : (
              <div className="text-2xl font-bold text-gray-400">-</div>
            )}
          </div>
          <div className="flex items-center text-xs text-gray-600 pt-2 border-t border-gray-100">
            {loading ? (
              <span className="text-gray-400">Loading...</span>
            ) : statistics ? (
              <>
                <div className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></div>
                <span className="font-semibold text-gray-900">{statistics.middlewares.enabled}</span>
                <span className="ml-1">enabled</span>
              </>
            ) : (
              <span className="text-gray-400">-</span>
            )}
          </div>
        </Link>

        <Link
          to="/authentication-policies"
          className="group bg-white border border-gray-200 rounded-xl p-4 hover:shadow-lg hover:border-red-200 transition-all duration-200"
        >
          <div className="flex items-center justify-between mb-3">
            <div className="w-10 h-10 bg-red-50 rounded-xl flex items-center justify-center flex-shrink-0 group-hover:bg-red-100 transition-colors">
              <ShieldCheckIcon className="h-5 w-5 text-red-600" />
            </div>
          </div>
          <div className="mb-2">
            <div className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1.5">Auth Policies</div>
            {loading ? (
              <div className="h-8 flex items-center">
                <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-red-600"></div>
              </div>
            ) : statistics ? (
              <div className="text-2xl font-bold text-gray-900">{statistics.authenticationPolicies.total}</div>
            ) : (
              <div className="text-2xl font-bold text-gray-400">-</div>
            )}
          </div>
          <div className="flex items-center text-xs text-gray-600 pt-2 border-t border-gray-100">
            {loading ? (
              <span className="text-gray-400">Loading...</span>
            ) : statistics ? (
              <>
                <div className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></div>
                <span className="font-semibold text-gray-900">{statistics.authenticationPolicies.enabled}</span>
                <span className="ml-1">enabled</span>
              </>
            ) : (
              <span className="text-gray-400">-</span>
            )}
          </div>
        </Link>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-3">
        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-sm font-bold text-gray-900">Recent Routes</h2>
            <div className="w-7 h-7 bg-blue-50 rounded-lg flex items-center justify-center">
              <GlobeAltIcon className="h-4 w-4 text-blue-600" />
            </div>
          </div>
          {loading ? (
            <div className="flex items-center justify-center py-4">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
            </div>
          ) : statistics && statistics.recentRoutes.length > 0 ? (
            <div className="space-y-1">
              {statistics.recentRoutes.map((route) => (
                <Link
                  key={route.id}
                  to={`/routes/${route.id}`}
                  className="group flex items-center justify-between p-2 rounded-lg hover:bg-blue-50 border border-transparent hover:border-blue-100 transition-all"
                >
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-gray-900 truncate group-hover:text-blue-600 transition-colors">{route.name}</p>
                    <p className="text-xs text-gray-500">{formatDate(route.createdAt)}</p>
                  </div>
                  <div className="ml-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <svg className="w-3.5 h-3.5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </Link>
              ))}
            </div>
          ) : (
            <div className="text-center py-4">
              <div className="w-8 h-8 bg-gray-100 rounded-lg mx-auto mb-1.5 flex items-center justify-center">
                <GlobeAltIcon className="h-4 w-4 text-gray-400" />
              </div>
              <p className="text-xs text-gray-500">No routes yet</p>
            </div>
          )}
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-sm font-bold text-gray-900">Recent Clusters</h2>
            <div className="w-7 h-7 bg-purple-50 rounded-lg flex items-center justify-center">
              <ServerStackIcon className="h-4 w-4 text-purple-600" />
            </div>
          </div>
          {loading ? (
            <div className="flex items-center justify-center py-4">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-purple-600"></div>
            </div>
          ) : statistics && statistics.recentClusters.length > 0 ? (
            <div className="space-y-1">
              {statistics.recentClusters.map((cluster) => (
                <Link
                  key={cluster.id}
                  to={`/clusters/${cluster.id}`}
                  className="group flex items-center justify-between p-2 rounded-lg hover:bg-purple-50 border border-transparent hover:border-purple-100 transition-all"
                >
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-gray-900 truncate group-hover:text-purple-600 transition-colors">{cluster.name}</p>
                    <p className="text-xs text-gray-500">{formatDate(cluster.createdAt)}</p>
                  </div>
                  <div className="ml-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <svg className="w-3.5 h-3.5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </Link>
              ))}
            </div>
          ) : (
            <div className="text-center py-4">
              <div className="w-8 h-8 bg-gray-100 rounded-lg mx-auto mb-1.5 flex items-center justify-center">
                <ServerStackIcon className="h-4 w-4 text-gray-400" />
              </div>
              <p className="text-xs text-gray-500">No clusters yet</p>
            </div>
          )}
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-sm font-bold text-gray-900">Recent Certificates</h2>
            <div className="w-7 h-7 bg-green-50 rounded-lg flex items-center justify-center">
              <KeyIcon className="h-4 w-4 text-green-600" />
            </div>
          </div>
          {loading ? (
            <div className="flex items-center justify-center py-4">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-green-600"></div>
            </div>
          ) : statistics && statistics.recentCertificates.length > 0 ? (
            <div className="space-y-1">
              {statistics.recentCertificates.map((cert) => (
                <Link
                  key={cert.id}
                  to={`/certificates/${cert.id}`}
                  className="group flex items-center justify-between p-2 rounded-lg hover:bg-green-50 border border-transparent hover:border-green-100 transition-all"
                >
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-gray-900 truncate group-hover:text-green-600 transition-colors">{cert.name}</p>
                    <p className="text-xs text-gray-500">{formatDate(cert.createdAt)}</p>
                  </div>
                  <div className="ml-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <svg className="w-3.5 h-3.5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </Link>
              ))}
            </div>
          ) : (
            <div className="text-center py-4">
              <div className="w-8 h-8 bg-gray-100 rounded-lg mx-auto mb-1.5 flex items-center justify-center">
                <KeyIcon className="h-4 w-4 text-gray-400" />
              </div>
              <p className="text-xs text-gray-500">No certificates yet</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 