import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, PlusIcon, XMarkIcon } from '@heroicons/react/24/outline';
import type { Route, Cluster } from '../../types/gateway';

const mockClusters: Cluster[] = [
  { id: 'cluster-1', name: 'Backend API', loadBalancingPolicy: 'RoundRobin', healthCheckEnabled: true, sessionAffinityEnabled: false, destinations: [], createdAt: '', updatedAt: '' },
  { id: 'cluster-2', name: 'Web Servers', loadBalancingPolicy: 'LeastRequests', healthCheckEnabled: true, sessionAffinityEnabled: true, destinations: [], createdAt: '', updatedAt: '' },
  { id: 'cluster-3', name: 'Admin Services', loadBalancingPolicy: 'Random', healthCheckEnabled: false, sessionAffinityEnabled: false, destinations: [], createdAt: '', updatedAt: '' },
];

const RouteEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== 'new';

  const [formData, setFormData] = useState({
    name: '',
    clusterId: '',
    order: 1,
    path: '',
    methods: [] as string[],
    hosts: [] as string[],
    enabled: true,
    authorizationPolicy: '',
    rateLimiterPolicy: '',
    corsPolicy: '',
    timeout: '',
    maxRequestBodySize: '',
  });

  const [methodInput, setMethodInput] = useState('');
  const [hostInput, setHostInput] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit) {
      setLoading(true);
      setTimeout(() => {
        setFormData({
          name: 'API Gateway',
          clusterId: 'cluster-1',
          order: 1,
          path: '/api/{**catch-all}',
          methods: ['GET', 'POST', 'PUT', 'DELETE'],
          hosts: ['api.example.com'],
          enabled: true,
          authorizationPolicy: 'default',
          rateLimiterPolicy: 'standard',
          corsPolicy: '',
          timeout: '30s',
          maxRequestBodySize: '10485760',
        });
        setLoading(false);
      }, 300);
    }
  }, [isEdit]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Save route:', formData);
    navigate('/routes');
  };

  const addMethod = () => {
    if (methodInput && !formData.methods.includes(methodInput.toUpperCase())) {
      setFormData({ ...formData, methods: [...formData.methods, methodInput.toUpperCase()] });
      setMethodInput('');
    }
  };

  const removeMethod = (method: string) => {
    setFormData({ ...formData, methods: formData.methods.filter(m => m !== method) });
  };

  const addHost = () => {
    if (hostInput && !formData.hosts.includes(hostInput)) {
      setFormData({ ...formData, hosts: [...formData.hosts, hostInput] });
      setHostInput('');
    }
  };

  const removeHost = (host: string) => {
    setFormData({ ...formData, hosts: formData.hosts.filter(h => h !== host) });
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <div className="fade-in max-w-4xl">
      <Link to="/routes" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Routes
      </Link>

      <div className="mb-6">
        <h1 className="text-lg font-medium text-gray-900">
          {isEdit ? 'Edit Route' : 'Add Route'}
        </h1>
        <p className="text-sm text-gray-600 mt-1">
          {isEdit ? 'Update route configuration' : 'Create a new routing rule'}
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <div className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                Route Name *
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                placeholder="e.g., API Gateway"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                Path Pattern *
              </label>
              <input
                type="text"
                value={formData.path}
                onChange={(e) => setFormData({ ...formData, path: e.target.value })}
                className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm font-mono"
                placeholder="/api/{**catch-all}"
                required
              />
              <p className="mt-1.5 text-xs text-gray-500">
                Use {'{**catch-all}'} for wildcard matching
              </p>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-900 mb-1.5">
                  Target Cluster *
                </label>
                <select
                  value={formData.clusterId}
                  onChange={(e) => setFormData({ ...formData, clusterId: e.target.value })}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  required
                >
                  <option value="">Select a cluster</option>
                  {mockClusters.map(cluster => (
                    <option key={cluster.id} value={cluster.id}>
                      {cluster.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-900 mb-1.5">
                  Order
                </label>
                <input
                  type="number"
                  value={formData.order}
                  onChange={(e) => setFormData({ ...formData, order: parseInt(e.target.value) })}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  min="1"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                HTTP Methods
              </label>
              <div className="flex gap-2 mb-2">
                <input
                  type="text"
                  value={methodInput}
                  onChange={(e) => setMethodInput(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addMethod())}
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="GET, POST, PUT, DELETE..."
                />
                <button
                  type="button"
                  onClick={addMethod}
                  className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                >
                  <PlusIcon className="h-4 w-4" />
                </button>
              </div>
              {formData.methods.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {formData.methods.map(method => (
                    <span
                      key={method}
                      className="inline-flex items-center px-2.5 py-1 rounded text-xs bg-gray-100 text-gray-800"
                    >
                      {method}
                      <button
                        type="button"
                        onClick={() => removeMethod(method)}
                        className="ml-1.5 hover:text-gray-600"
                      >
                        <XMarkIcon className="h-3 w-3" />
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                Host Names
              </label>
              <div className="flex gap-2 mb-2">
                <input
                  type="text"
                  value={hostInput}
                  onChange={(e) => setHostInput(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addHost())}
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="api.example.com"
                />
                <button
                  type="button"
                  onClick={addHost}
                  className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                >
                  <PlusIcon className="h-4 w-4" />
                </button>
              </div>
              {formData.hosts.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {formData.hosts.map(host => (
                    <span
                      key={host}
                      className="inline-flex items-center px-2.5 py-1 rounded text-xs bg-gray-100 text-gray-800"
                    >
                      {host}
                      <button
                        type="button"
                        onClick={() => removeHost(host)}
                        className="ml-1.5 hover:text-gray-600"
                      >
                        <XMarkIcon className="h-3 w-3" />
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-900 mb-1.5">
                  Authorization Policy
                </label>
                <input
                  type="text"
                  value={formData.authorizationPolicy}
                  onChange={(e) => setFormData({ ...formData, authorizationPolicy: e.target.value })}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="default"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-900 mb-1.5">
                  Rate Limiter Policy
                </label>
                <input
                  type="text"
                  value={formData.rateLimiterPolicy}
                  onChange={(e) => setFormData({ ...formData, rateLimiterPolicy: e.target.value })}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="standard"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-900 mb-1.5">
                  CORS Policy
                </label>
                <input
                  type="text"
                  value={formData.corsPolicy}
                  onChange={(e) => setFormData({ ...formData, corsPolicy: e.target.value })}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="default"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-900 mb-1.5">
                  Timeout
                </label>
                <input
                  type="text"
                  value={formData.timeout}
                  onChange={(e) => setFormData({ ...formData, timeout: e.target.value })}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="30s"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                Max Request Body Size (bytes)
              </label>
              <input
                type="number"
                value={formData.maxRequestBodySize}
                onChange={(e) => setFormData({ ...formData, maxRequestBodySize: e.target.value })}
                className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                placeholder="10485760"
              />
              <p className="mt-1.5 text-xs text-gray-500">
                Leave empty for no limit. Example: 10485760 = 10MB
              </p>
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                id="enabled"
                checked={formData.enabled}
                onChange={(e) => setFormData({ ...formData, enabled: e.target.checked })}
                className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
              />
              <label htmlFor="enabled" className="ml-2 text-sm text-gray-900">
                Enable this route immediately
              </label>
            </div>
          </div>
        </div>

        <div className="mt-6 flex justify-start space-x-3">
          <button
            type="submit"
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-md text-sm font-medium text-white bg-gray-900 hover:bg-gray-800"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            {isEdit ? 'Update Route' : 'Create Route'}
          </button>
          <Link
            to="/routes"
            className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            Cancel
          </Link>
        </div>
      </form>
    </div>
  );
};

export default RouteEdit;
