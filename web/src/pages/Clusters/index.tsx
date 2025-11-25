import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  CheckCircleIcon,
  ServerStackIcon,
  CloudIcon,
  FunnelIcon,
  CpuChipIcon
} from '@heroicons/react/24/outline';
import type { Cluster } from '../../types';
import { ClusterService } from '../../services/clusterService';

const Clusters: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [clusters, setClusters] = useState<Cluster[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);

  const loadClusters = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ClusterService.getClusters(searchTerm);
      setClusters(data);
    } catch (err) {
      console.error('Failed to load clusters:', err);
      setError('Failed to load clusters');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadClusters();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== '') {
        loadClusters();
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

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
            <h1 className="text-2xl font-semibold text-gray-900">Clusters</h1>
            <p className="mt-1 text-sm text-gray-500">Manage backend service clusters and destinations</p>
          </div>
          <Link 
            to="/clusters/new" 
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Cluster
          </Link>
        </div>

        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
          </div>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="Search clusters..."
          />
        </div>

        <div className="flex items-center text-sm text-gray-500 font-medium mt-4">
          <FunnelIcon className="h-4 w-4 mr-1.5" />
          {clusters.length} {clusters.length === 1 ? 'cluster' : 'clusters'}
        </div>
      </div>

      {error ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="h-6 w-6 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">Failed to load clusters</h3>
          <p className="text-sm text-gray-500 mb-4">Please check your connection and try again</p>
          <button
            onClick={loadClusters}
            className="inline-flex items-center px-4 py-2 bg-white border border-gray-200 text-sm font-medium rounded-lg hover:bg-gray-50 transition-colors"
          >
            Try Again
          </button>
        </div>
      ) : clusters.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <ServerStackIcon className="h-6 w-6 text-gray-400" />
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">No clusters yet</h3>
          <p className="text-sm text-gray-500 mb-4">Get started by creating your first cluster</p>
          <Link
            to="/clusters/new"
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Cluster
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4 gap-4 max-w-7xl">
          {clusters.map((cluster) => {
            const destinationCount = cluster.destinations?.length || 0;
            const healthyCount = cluster.destinations?.filter(d => d.health === 'healthy').length || 0;
            const healthCheckEnabled = !!(cluster.healthCheck?.active?.enabled || cluster.healthCheck?.passive?.enabled);
            const isFullyHealthy = destinationCount > 0 && healthyCount === destinationCount;
            const hasServiceDiscovery = !!(cluster as any).serviceName;
            
            return (
              <Link
                key={cluster.id}
                to={`/clusters/${cluster.id}`}
                className="block bg-white border border-gray-200 rounded-lg p-4 hover:border-gray-300 hover:shadow-sm transition-all"
              >
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-10 h-10 bg-purple-50 rounded-lg flex items-center justify-center flex-shrink-0">
                    <ServerStackIcon className="h-5 w-5 text-purple-600" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="text-sm font-medium text-gray-900 truncate">{cluster.name}</h3>
                    <div className="flex items-center gap-1.5 mt-1">
                      {hasServiceDiscovery && (
                        <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded bg-blue-50 text-blue-700">
                          <CloudIcon className="h-3 w-3 mr-0.5" />
                          Service Discovery
                        </span>
                      )}
                    </div>
                  </div>
                </div>

                <div className="space-y-2">
                  {hasServiceDiscovery && (
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <div className="flex items-center gap-1.5">
                        <CloudIcon className="h-4 w-4 text-gray-400" />
                        <span className="text-xs text-gray-600">Service Name</span>
                      </div>
                      <span className="text-xs font-medium text-gray-900 truncate ml-2">{(cluster as any).serviceName}</span>
                    </div>
                  )}

                  <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                    <div className="flex items-center gap-1.5">
                      <CpuChipIcon className="h-4 w-4 text-gray-400" />
                      <span className="text-xs text-gray-600">Load Balancing</span>
                    </div>
                    <span className="text-xs font-medium text-gray-900">{cluster.loadBalancingPolicy || 'RoundRobin'}</span>
                  </div>

                  <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                    <div className="flex items-center gap-1.5">
                      <ServerStackIcon className="h-4 w-4 text-gray-400" />
                      <span className="text-xs text-gray-600">Destinations</span>
                    </div>
                    {destinationCount > 0 ? (
                      <div className="flex items-center gap-1.5">
                        <span className={`inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-xs font-medium ${
                          isFullyHealthy ? 'bg-green-50 text-green-700' : 
                          healthyCount > 0 ? 'bg-yellow-50 text-yellow-700' : 
                          'bg-red-50 text-red-700'
                        }`}>
                          <div className={`w-1.5 h-1.5 rounded-full ${
                            isFullyHealthy ? 'bg-green-500' : 
                            healthyCount > 0 ? 'bg-yellow-500' : 'bg-red-500'
                          }`} />
                          {healthyCount}/{destinationCount}
                        </span>
                      </div>
                    ) : (
                      <span className="text-xs font-medium text-gray-400">0</span>
                    )}
                  </div>

                  {healthCheckEnabled && (
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <div className="flex items-center gap-1.5">
                        <CheckCircleIcon className="h-4 w-4 text-gray-400" />
                        <span className="text-xs text-gray-600">Health Check</span>
                      </div>
                      <span className="text-xs text-green-600 font-medium">● Enabled</span>
                    </div>
                  )}

                  {!hasServiceDiscovery && !healthCheckEnabled && destinationCount === 0 && (
                    <div className="text-xs text-gray-400 py-1.5">Basic configuration</div>
                  )}
                </div>
              </Link>
            );
          })}
        </div>
      )}
    </div>
  );
};

export default Clusters;
