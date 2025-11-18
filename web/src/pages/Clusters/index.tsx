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
      <div className="section-header">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-xl font-medium text-gray-900">Clusters</h1>
            <p className="text-sm text-gray-600">Manage backend server clusters</p>
          </div>
          <Link to="/clusters/new" className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-gray-900 hover:bg-gray-800">
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Cluster
          </Link>
        </div>
      </div>

      <div className="mb-6">
        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <MagnifyingGlassIcon className="h-4 w-4 text-gray-400" />
          </div>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-gray-900 focus:border-gray-900 sm:text-sm"
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
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {clusters.length === 0 ? (
            <div className="col-span-2 text-center py-12 text-gray-500">
              <ServerStackIcon className="h-12 w-12 mx-auto mb-4 text-gray-400" />
              <p>No clusters yet</p>
              <Link
                to="/clusters/new"
                className="inline-block mt-4 text-sm text-gray-900 hover:underline"
              >
                Create your first cluster
              </Link>
            </div>
          ) : (
          clusters.map((cluster) => {
            const healthyCount = cluster.destinations?.filter(d => d.health === 'healthy').length || 0;
            const healthCheckEnabled = !!(cluster.healthCheck?.active?.enabled || cluster.healthCheck?.passive?.enabled);
          return (
            <Link
              key={cluster.id}
              to={`/clusters/${cluster.id}/edit`}
              className="block bg-white border border-gray-200 rounded-lg p-4 hover:border-gray-300 transition-colors"
            >
              <div className="flex items-center space-x-2 mb-3">
                <ServerStackIcon className="h-5 w-5 text-purple-600" />
                <h3 className="text-sm font-medium text-gray-900">{cluster.name}</h3>
              </div>
              
              <div className="space-y-2 text-xs">
                <div className="flex justify-between">
                  <span className="text-gray-600">Load Balancing</span>
                  <span className="font-medium">{cluster.loadBalancingPolicy || 'None'}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Destinations</span>
                  <span className="font-medium">{cluster.destinations?.length || 0}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Healthy</span>
                  <span className={`font-medium ${healthyCount === (cluster.destinations?.length || 0) ? 'text-green-600' : 'text-yellow-600'}`}>
                    {healthyCount}/{cluster.destinations?.length || 0}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Health Check</span>
                  <span className="font-medium">
                    {healthCheckEnabled ? (
                      <CheckCircleIcon className="h-4 w-4 text-green-600 inline" />
                    ) : (
                      'Disabled'
                    )}
                  </span>
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

