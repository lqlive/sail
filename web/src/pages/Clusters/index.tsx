import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  CheckCircleIcon,
  ServerStackIcon,
} from '@heroicons/react/24/outline';
import type { Cluster } from '../../types/gateway';
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
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">Clusters</h1>
            <p className="mt-1 text-sm text-gray-500">Manage backend server clusters</p>
          </div>
          <Link 
            to="/clusters/new" 
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Cluster
          </Link>
        </div>

        {/* Search Bar */}
        <div className="relative max-w-md">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <MagnifyingGlassIcon className="h-4 w-4 text-gray-400" />
          </div>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="Search clusters..."
          />
        </div>
      </div>

      {error ? (
        <div className="text-center py-12">
          <div className="mx-auto h-12 w-12 text-red-400 mb-4">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="mt-2 text-sm font-medium text-gray-900">Failed to load clusters</h3>
          <p className="mt-1 text-sm text-gray-500">Please check your connection and try again.</p>
          <div className="mt-6">
            <button
              onClick={loadClusters}
              className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50"
            >
              Try Again
            </button>
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {clusters.length === 0 ? (
            <div className="col-span-full bg-white border border-gray-200 rounded-lg p-12 text-center">
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
          clusters.map((cluster) => {
            const destinationCount = cluster.destinations?.length || 0;
            const healthyCount = cluster.destinations?.filter(d => d.health === 'healthy').length || 0;
            const healthCheckEnabled = !!(cluster.healthCheck?.active?.enabled || cluster.healthCheck?.passive?.enabled);
            const isFullyHealthy = destinationCount > 0 && healthyCount === destinationCount;
          return (
            <Link
              key={cluster.id}
              to={`/clusters/${cluster.id}/edit`}
              className="block bg-white border border-gray-200 rounded-lg p-5 hover:border-gray-300 hover:shadow-sm transition-all"
            >
              <div className="flex items-center gap-3 mb-4">
                <div className="w-10 h-10 bg-purple-50 rounded-lg flex items-center justify-center flex-shrink-0">
                  <ServerStackIcon className="h-5 w-5 text-purple-600" />
                </div>
                <h3 className="text-base font-medium text-gray-900 truncate">{cluster.name}</h3>
              </div>
              
              <div className="space-y-3">
                <div className="flex items-center justify-between py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-500">Load Balancing</span>
                  <span className="text-sm font-medium text-gray-900">{cluster.loadBalancingPolicy || 'RoundRobin'}</span>
                </div>
                
                <div className="flex items-center justify-between py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-500">Destinations</span>
                  <span className="text-sm font-medium text-gray-900">{destinationCount}</span>
                </div>
                
                <div className="flex items-center justify-between py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-500">Healthy</span>
                  <div className="flex items-center gap-2">
                    <span className={`text-sm font-medium ${
                      destinationCount === 0 ? 'text-gray-400' :
                      isFullyHealthy ? 'text-green-600' : 
                      healthyCount > 0 ? 'text-yellow-600' : 'text-red-600'
                    }`}>
                      {healthyCount}/{destinationCount}
                    </span>
                    {destinationCount > 0 && (
                      <div className={`w-2 h-2 rounded-full ${
                        isFullyHealthy ? 'bg-green-500' : 
                        healthyCount > 0 ? 'bg-yellow-500' : 'bg-red-500'
                      }`} />
                    )}
                  </div>
                </div>
                
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-500">Health Check</span>
                  {healthCheckEnabled ? (
                    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-50 text-green-700 border border-green-100">
                      <CheckCircleIcon className="h-3.5 w-3.5 mr-1" />
                      Enabled
                    </span>
                  ) : (
                    <span className="text-sm text-gray-400">Disabled</span>
                  )}
                </div>
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

export default Clusters;

