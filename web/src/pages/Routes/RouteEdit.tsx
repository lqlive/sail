import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, PlusIcon, XMarkIcon, CheckIcon } from '@heroicons/react/24/outline';
import type { Route, Cluster } from '../../types/gateway';
import { RouteService } from '../../services/routeService';
import { ClusterService } from '../../services/clusterService';

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
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [clusters, setClusters] = useState<Cluster[]>([]);

  useEffect(() => {
    loadClusters();
    if (isEdit && id) {
      loadRoute(id);
    }
  }, [isEdit, id]);

  const loadClusters = async () => {
    try {
      const data = await ClusterService.getClusters();
      setClusters(data);
    } catch (err) {
      console.error('Failed to load clusters:', err);
    }
  };

  const loadRoute = async (routeId: string) => {
    try {
      setLoading(true);
      setError(null);
      const route = await RouteService.getRoute(routeId);
      
      const match = (route as any).match;
      
      setFormData({
        name: route.name || '',
        clusterId: route.clusterId || '',
        order: route.order || 1,
        path: match?.path || route.path || '',
        methods: match?.methods || route.methods || [],
        hosts: match?.hosts || route.hosts || [],
        enabled: route.enabled !== false,
        authorizationPolicy: route.authorizationPolicy || '',
        rateLimiterPolicy: route.rateLimiterPolicy || '',
        corsPolicy: route.corsPolicy || '',
        timeout: route.timeout || '',
        maxRequestBodySize: route.maxRequestBodySize?.toString() || '',
      });
    } catch (err) {
      console.error('Failed to load route:', err);
      setError('Failed to load route');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setSaving(true);
      setError(null);

      const routeData = {
        name: formData.name,
        clusterId: formData.clusterId,
        order: formData.order,
        maxRequestBodySize: formData.maxRequestBodySize ? parseInt(formData.maxRequestBodySize) : undefined,
        authorizationPolicy: formData.authorizationPolicy || undefined,
        rateLimiterPolicy: formData.rateLimiterPolicy || undefined,
        corsPolicy: formData.corsPolicy || undefined,
        timeout: formData.timeout || undefined,
        match: {
          path: formData.path,
          methods: formData.methods.length > 0 ? formData.methods : undefined,
          hosts: formData.hosts.length > 0 ? formData.hosts : undefined,
        },
      };

      if (isEdit && id) {
        await RouteService.updateRoute(id, routeData);
      } else {
        await RouteService.createRoute(routeData);
      }
      
      navigate('/routes');
    } catch (err) {
      console.error('Failed to save route:', err);
      setError('Failed to save route');
    } finally {
      setSaving(false);
    }
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
      <Link to="/routes" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Routes
      </Link>

      <div className="mb-4">
        <h1 className="text-lg font-medium text-gray-900">
          {isEdit ? 'Edit Route' : 'Create Route'}
        </h1>
        <p className="text-xs text-gray-600 mt-1">
          Define route capabilities and specifications
        </p>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
          <p className="text-xs text-red-600">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        {/* Basic Configuration */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-4">
          <h2 className="text-sm font-semibold text-gray-900 mb-3">Basic Configuration</h2>
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Route Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="e.g., API Gateway Route"
                className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Target Cluster <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.clusterId}
                  onChange={(e) => setFormData({ ...formData, clusterId: e.target.value })}
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  required
                >
                  <option value="">Select a cluster</option>
                  {clusters.map(cluster => (
                    <option key={cluster.id} value={cluster.id}>
                      {cluster.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Order
                </label>
                <input
                  type="number"
                  value={formData.order}
                  onChange={(e) => setFormData({ ...formData, order: parseInt(e.target.value) || 0 })}
                  min="0"
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                />
                <p className="mt-1 text-xs text-gray-500">Lower values have higher priority</p>
              </div>
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                id="enabled"
                checked={formData.enabled}
                onChange={(e) => setFormData({ ...formData, enabled: e.target.checked })}
                className="h-3.5 w-3.5 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
              />
              <label htmlFor="enabled" className="ml-2 block text-xs text-gray-700">
                Enable this route immediately
              </label>
            </div>
          </div>
        </div>

        {/* Matching Rules */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-4">
          <h2 className="text-sm font-semibold text-gray-900 mb-3">Matching Rules</h2>
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Path Pattern <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.path}
                onChange={(e) => setFormData({ ...formData, path: e.target.value })}
                placeholder="/api/{**catch-all}"
                className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm font-mono"
                required
              />
              <p className="mt-1 text-xs text-gray-500">Use {`{**catch-all}`} for wildcard matching</p>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                HTTP Methods
              </label>
              {formData.methods.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mb-2">
                  {formData.methods.map((method) => (
                    <span
                      key={method}
                      className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-blue-100 text-blue-800"
                    >
                      {method}
                      <button
                        type="button"
                        onClick={() => removeMethod(method)}
                        className="ml-1.5 inline-flex items-center hover:text-blue-900"
                      >
                        <XMarkIcon className="h-3 w-3" />
                      </button>
                    </span>
                  ))}
                </div>
              )}
              <div className="flex gap-2">
                <input
                  type="text"
                  value={methodInput}
                  onChange={(e) => setMethodInput(e.target.value.toUpperCase())}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addMethod())}
                  placeholder="GET, POST, PUT, DELETE..."
                  className="block flex-1 px-2.5 py-1.5 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900"
                />
                <button
                  type="button"
                  onClick={addMethod}
                  className="px-3 py-1.5 border border-gray-300 rounded-md text-xs font-medium text-gray-700 bg-white hover:bg-gray-50 inline-flex items-center"
                >
                  <PlusIcon className="h-3.5 w-3.5" />
                </button>
              </div>
              <p className="mt-1 text-xs text-gray-500">Leave empty to match all methods</p>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Host Names
              </label>
              {formData.hosts.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mb-2">
                  {formData.hosts.map((host) => (
                    <span
                      key={host}
                      className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-purple-100 text-purple-800"
                    >
                      {host}
                      <button
                        type="button"
                        onClick={() => removeHost(host)}
                        className="ml-1.5 inline-flex items-center hover:text-purple-900"
                      >
                        <XMarkIcon className="h-3 w-3" />
                      </button>
                    </span>
                  ))}
                </div>
              )}
              <div className="flex gap-2">
                <input
                  type="text"
                  value={hostInput}
                  onChange={(e) => setHostInput(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addHost())}
                  placeholder="api.example.com"
                  className="block flex-1 px-2.5 py-1.5 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900"
                />
                <button
                  type="button"
                  onClick={addHost}
                  className="px-3 py-1.5 border border-gray-300 rounded-md text-xs font-medium text-gray-700 bg-white hover:bg-gray-50 inline-flex items-center"
                >
                  <PlusIcon className="h-3.5 w-3.5" />
                </button>
              </div>
              <p className="mt-1 text-xs text-gray-500">Leave empty to match all hosts</p>
            </div>
          </div>
        </div>

        {/* Policies */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-4">
          <h2 className="text-sm font-semibold text-gray-900 mb-3">Policies & Limits</h2>
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Authorization Policy
                </label>
                <input
                  type="text"
                  value={formData.authorizationPolicy}
                  onChange={(e) => setFormData({ ...formData, authorizationPolicy: e.target.value })}
                  placeholder="e.g., default, admin-only"
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Rate Limiter Policy
                </label>
                <input
                  type="text"
                  value={formData.rateLimiterPolicy}
                  onChange={(e) => setFormData({ ...formData, rateLimiterPolicy: e.target.value })}
                  placeholder="e.g., standard, strict"
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  CORS Policy
                </label>
                <input
                  type="text"
                  value={formData.corsPolicy}
                  onChange={(e) => setFormData({ ...formData, corsPolicy: e.target.value })}
                  placeholder="e.g., allow-all, strict"
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Timeout
                </label>
                <input
                  type="text"
                  value={formData.timeout}
                  onChange={(e) => setFormData({ ...formData, timeout: e.target.value })}
                  placeholder="e.g., 00:00:30"
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                />
                <p className="mt-1 text-xs text-gray-500">Format: HH:mm:ss</p>
              </div>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Max Request Body Size (bytes)
              </label>
              <input
                type="number"
                value={formData.maxRequestBodySize}
                onChange={(e) => setFormData({ ...formData, maxRequestBodySize: e.target.value })}
                placeholder="e.g., 10485760 (10MB)"
                min="-1"
                className="block w-full max-w-xs px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
              />
              <p className="mt-1 text-xs text-gray-500">Use -1 for unlimited</p>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-end space-x-2">
          <Link
            to="/routes"
            className="px-3 py-1.5 border border-gray-300 rounded-md text-xs font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={saving}
            className="inline-flex items-center px-4 py-1.5 border border-transparent rounded-md shadow-sm text-xs font-medium text-white bg-gray-900 hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? (
              <>
                <div className="animate-spin rounded-full h-3.5 w-3.5 border-b-2 border-white mr-2"></div>
                Saving...
              </>
            ) : (
              <>
                <CheckIcon className="h-3.5 w-3.5 mr-2" />
                {isEdit ? 'Update Route' : 'Create Route'}
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default RouteEdit;

