import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, PlusIcon, XMarkIcon, CheckIcon } from '@heroicons/react/24/outline';
import type { Route, Cluster } from '../../types';
import { RouteService } from '../../services/routeService';
import { PolicySelect } from '../../components/PolicySelect';
import { Select } from '../../components/Select';
import { FormField } from '../../components/FormField';
import { Checkbox } from '../../components/Checkbox';
import { usePolicyOptions } from '../../hooks/usePolicyOptions';
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
    timeoutPolicy: '',
    retryPolicy: '',
    timeout: '',
    maxRequestBodySize: '',
    httpsRedirect: false,
    transforms: [] as Array<Record<string, string>>,
  });

  const [methodInput, setMethodInput] = useState('');
  const [hostInput, setHostInput] = useState('');
  const [transformType, setTransformType] = useState('RequestHeader');
  const [transformKey, setTransformKey] = useState('');
  const [transformValue, setTransformValue] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [clusters, setClusters] = useState<Cluster[]>([]);
  const [activeTab, setActiveTab] = useState('basic');
  
  // Load policy options
  const {
    authPolicies,
    corsPolicies,
    rateLimiterPolicies,
    timeoutPolicies,
    retryPolicies,
    loading: policiesLoading,
  } = usePolicyOptions();

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
        timeoutPolicy: route.timeoutPolicy || '',
        retryPolicy: route.retryPolicy || '',
        timeout: route.timeout || '',
        maxRequestBodySize: route.maxRequestBodySize?.toString() || '',
        httpsRedirect: route.httpsRedirect || false,
        transforms: route.transforms || [],
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
        timeoutPolicy: formData.timeoutPolicy || undefined,
        retryPolicy: formData.retryPolicy || undefined,
        timeout: formData.timeout || undefined,
        httpsRedirect: formData.httpsRedirect,
        transforms: formData.transforms.length > 0 ? formData.transforms : undefined,
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

  const addTransform = () => {
    const newTransform: Record<string, string> = {};
    
    switch (transformType) {
      case 'RequestHeader':
        if (!transformKey || !transformValue) return;
        newTransform['RequestHeader'] = transformKey;
        newTransform['Set'] = transformValue;
        break;
      case 'ResponseHeader':
        if (!transformKey || !transformValue) return;
        newTransform['ResponseHeader'] = transformKey;
        newTransform['Set'] = transformValue;
        break;
      case 'PathPrefix':
        if (!transformValue) return;
        newTransform['PathPrefix'] = transformValue;
        break;
      case 'PathRemovePrefix':
        if (!transformValue) return;
        newTransform['PathRemovePrefix'] = transformValue;
        break;
      case 'QueryParameter':
        if (!transformKey || !transformValue) return;
        newTransform['QueryParameter'] = transformKey;
        newTransform['Set'] = transformValue;
        break;
      default:
        return;
    }
    
    setFormData({ ...formData, transforms: [...formData.transforms, newTransform] });
    setTransformKey('');
    setTransformValue('');
  };

  const removeTransform = (index: number) => {
    setFormData({ 
      ...formData, 
      transforms: formData.transforms.filter((_, i) => i !== index) 
    });
  };

  const getTransformDisplay = (transform: Record<string, string>) => {
    if (transform.RequestHeader) {
      return `Request Header: ${transform.RequestHeader} = ${transform.Set || transform.Append}`;
    }
    if (transform.ResponseHeader) {
      return `Response Header: ${transform.ResponseHeader} = ${transform.Set || transform.Append}`;
    }
    if (transform.PathPrefix) {
      return `Add Path Prefix: ${transform.PathPrefix}`;
    }
    if (transform.PathRemovePrefix) {
      return `Remove Path Prefix: ${transform.PathRemovePrefix}`;
    }
    if (transform.QueryParameter) {
      return `Query Parameter: ${transform.QueryParameter} = ${transform.Set || transform.Append}`;
    }
    return JSON.stringify(transform);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <div className="fade-in max-w-5xl mx-auto">
      <Link to="/routes" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6 transition-colors">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Routes
      </Link>

      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">
          {isEdit ? 'Edit Route' : 'Create Route'}
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          Define route capabilities and specifications
        </p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-700 font-medium">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Tabs Navigation */}
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              <button
                type="button"
                onClick={() => setActiveTab('basic')}
                className={`px-6 py-4 text-sm font-medium border-b-2 transition-colors ${
                  activeTab === 'basic'
                    ? 'border-gray-900 text-gray-900'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                Basic & Matching
              </button>
              <button
                type="button"
                onClick={() => setActiveTab('policies')}
                className={`px-6 py-4 text-sm font-medium border-b-2 transition-colors ${
                  activeTab === 'policies'
                    ? 'border-gray-900 text-gray-900'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                Policies & Limits
              </button>
              <button
                type="button"
                onClick={() => setActiveTab('transforms')}
                className={`px-6 py-4 text-sm font-medium border-b-2 transition-colors ${
                  activeTab === 'transforms'
                    ? 'border-gray-900 text-gray-900'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                Transforms
              </button>
            </nav>
          </div>

          <div className="p-6">
            {/* Basic Tab */}
            {activeTab === 'basic' && (
              <div className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Route Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="e.g., API Gateway Route"
                className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <FormField
                label="Target Cluster"
                required={true}
              >
                <Select
                  value={formData.clusterId}
                  onChange={(value) => setFormData({ ...formData, clusterId: value })}
                  options={clusters.map(cluster => ({
                    value: cluster.id,
                    label: cluster.name,
                  }))}
                  placeholder="Select a cluster"
                />
              </FormField>

              <FormField
                label="Order"
                hint="Lower values have higher priority"
              >
                <input
                  type="number"
                  value={formData.order}
                  onChange={(e) => setFormData({ ...formData, order: parseInt(e.target.value) || 0 })}
                  min="0"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                />
              </FormField>
            </div>

            <Checkbox
              checked={formData.enabled}
              onChange={(checked) => setFormData({ ...formData, enabled: checked })}
              label="Enable this route immediately"
            />

            {/* Matching Rules Section */}
            <div className="pt-6 border-t border-gray-200">
              <h3 className="text-sm font-semibold text-gray-900 mb-5">Matching Rules</h3>
              <div className="space-y-5">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Path Pattern <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={formData.path}
                    onChange={(e) => setFormData({ ...formData, path: e.target.value })}
                    placeholder="/api/{**catch-all}"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm font-mono focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                    required
                  />
                  <p className="mt-1 text-xs text-gray-500">Use {`{**catch-all}`} for wildcard matching</p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
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
                      className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                    />
                    <button
                      type="button"
                      onClick={addMethod}
                      className="px-3 py-2 border border-gray-200 rounded-lg text-xs font-medium text-gray-700 bg-white hover:bg-gray-50 inline-flex items-center"
                    >
                      <PlusIcon className="h-4 w-4" />
                    </button>
                  </div>
                  <p className="mt-1 text-xs text-gray-500">Leave empty to match all methods</p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
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
                      className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                    />
                    <button
                      type="button"
                      onClick={addHost}
                      className="px-3 py-2 border border-gray-200 rounded-lg text-xs font-medium text-gray-700 bg-white hover:bg-gray-50 inline-flex items-center"
                    >
                      <PlusIcon className="h-4 w-4" />
                    </button>
                  </div>
                  <p className="mt-1 text-xs text-gray-500">Leave empty to match all hosts</p>
                </div>
              </div>
            </div>
              </div>
            )}

            {/* Policies Tab */}
            {activeTab === 'policies' && (
              <div className="space-y-5">
            <div className="grid grid-cols-2 gap-3">
              <PolicySelect
                label="Authorization Policy"
                value={formData.authorizationPolicy}
                onChange={(value) => setFormData({ ...formData, authorizationPolicy: value })}
                placeholder="Select an authorization policy (optional)"
                options={authPolicies}
                loading={policiesLoading}
              />

              <PolicySelect
                label="Rate Limiter Policy"
                value={formData.rateLimiterPolicy}
                onChange={(value) => setFormData({ ...formData, rateLimiterPolicy: value })}
                placeholder="Select a rate limiter policy (optional)"
                options={rateLimiterPolicies}
                loading={policiesLoading}
              />

              <PolicySelect
                label="CORS Policy"
                value={formData.corsPolicy}
                onChange={(value) => setFormData({ ...formData, corsPolicy: value })}
                placeholder="Select a CORS policy (optional)"
                options={corsPolicies}
                loading={policiesLoading}
              />

              <PolicySelect
                label="Timeout Policy"
                value={formData.timeoutPolicy}
                onChange={(value) => setFormData({ ...formData, timeoutPolicy: value })}
                placeholder="Select a timeout policy (optional)"
                options={timeoutPolicies}
                loading={policiesLoading}
              />

              <PolicySelect
                label="Retry Policy"
                value={formData.retryPolicy}
                onChange={(value) => setFormData({ ...formData, retryPolicy: value })}
                placeholder="Select a retry policy (optional)"
                options={retryPolicies}
                loading={policiesLoading}
              />

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Timeout
                </label>
                <input
                  type="text"
                  value={formData.timeout}
                  onChange={(e) => setFormData({ ...formData, timeout: e.target.value })}
                  placeholder="e.g., 00:00:30"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                />
                <p className="mt-1 text-xs text-gray-500">Format: HH:mm:ss</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Max Request Body Size (bytes)
                </label>
                <input
                  type="number"
                  value={formData.maxRequestBodySize}
                  onChange={(e) => setFormData({ ...formData, maxRequestBodySize: e.target.value })}
                  placeholder="e.g., 10485760 (10MB)"
                  min="-1"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                />
                <p className="mt-1 text-xs text-gray-500">Use -1 for unlimited</p>
              </div>
            </div>

            <div className="pt-5 border-t border-gray-100">
              <Checkbox
                checked={formData.httpsRedirect}
                onChange={(checked) => setFormData({ ...formData, httpsRedirect: checked })}
                label="Redirect HTTP to HTTPS"
              />
            </div>
              </div>
            )}

            {/* Transforms Tab */}
            {activeTab === 'transforms' && (
              <div className="space-y-5">
            <div className="grid grid-cols-3 gap-3">
              <FormField label="Transform Type">
                <Select
                  value={transformType}
                  onChange={setTransformType}
                  options={[
                    { value: 'RequestHeader', label: 'Request Header' },
                    { value: 'ResponseHeader', label: 'Response Header' },
                    { value: 'PathPrefix', label: 'Add Path Prefix' },
                    { value: 'PathRemovePrefix', label: 'Remove Path Prefix' },
                    { value: 'QueryParameter', label: 'Query Parameter' },
                  ]}
                />
              </FormField>

              {(transformType === 'RequestHeader' || transformType === 'ResponseHeader' || transformType === 'QueryParameter') && (
                <FormField label={transformType === 'QueryParameter' ? 'Parameter Name' : 'Header Name'}>
                  <input
                    type="text"
                    value={transformKey}
                    onChange={(e) => setTransformKey(e.target.value)}
                    placeholder={transformType === 'QueryParameter' ? 'e.g., api-version' : 'e.g., X-Custom-Header'}
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                </FormField>
              )}

              <FormField label={transformType === 'PathPrefix' || transformType === 'PathRemovePrefix' ? 'Path' : 'Value'}>
                <input
                  type="text"
                  value={transformValue}
                  onChange={(e) => setTransformValue(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addTransform())}
                  placeholder={transformType === 'PathPrefix' ? '/api/v1' : transformType === 'PathRemovePrefix' ? '/old-prefix' : 'value'}
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                />
              </FormField>
            </div>

            <div>
              <button
                type="button"
                onClick={addTransform}
                className="inline-flex items-center px-4 py-2 border border-gray-200 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
              >
                <PlusIcon className="h-4 w-4 mr-1.5" />
                Add Transform
              </button>
            </div>

            {formData.transforms.length > 0 && (
              <div className="mt-6 pt-6 border-t border-gray-200">
                <h3 className="text-sm font-medium text-gray-900 mb-3">Configured Transforms</h3>
                <div className="space-y-2">
                  {formData.transforms.map((transform, index) => (
                    <div
                      key={index}
                      className="flex items-center justify-between px-3 py-2.5 bg-white border border-gray-200 rounded-lg hover:border-gray-300 transition-colors"
                    >
                      <div className="flex items-center gap-2.5">
                        <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center flex-shrink-0">
                          <svg className="h-4 w-4 text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                          </svg>
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm text-gray-900">{getTransformDisplay(transform)}</p>
                          <p className="text-xs text-gray-500">Transform #{index + 1}</p>
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={() => removeTransform(index)}
                        className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors flex-shrink-0"
                      >
                        <XMarkIcon className="h-4 w-4" />
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            )}
              </div>
            )}
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-2">
          <Link
            to="/routes"
            className="btn-secondary"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={saving}
            className="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Saving...
              </>
            ) : (
              <>
                <CheckIcon className="h-4 w-4 mr-2" />
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

