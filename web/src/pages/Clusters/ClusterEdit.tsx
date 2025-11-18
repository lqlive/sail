import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, PlusIcon, XMarkIcon, CheckIcon } from '@heroicons/react/24/outline';
import type { Destination } from '../../types/gateway';
import { ClusterService } from '../../services/clusterService';

const ClusterEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== 'new';

  const [formData, setFormData] = useState({
    name: '',
    loadBalancingPolicy: 'RoundRobin',
    healthCheckEnabled: false,
    sessionAffinityEnabled: false,
    healthCheckPath: '/health',
    healthCheckInterval: '10',
    healthCheckTimeout: '5',
    sessionAffinityPolicy: 'Cookie',
    sessionAffinityFailurePolicy: 'Redistribute',
    enableActiveHealthCheck: true,
    enablePassiveHealthCheck: false,
    passiveReactivationPeriod: '60',
    httpVersion: '2.0',
    requestTimeout: '100',
    maxConnectionsPerServer: '',
  });

  const [destinations, setDestinations] = useState<Destination[]>([]);
  const [newDestination, setNewDestination] = useState({
    address: '',
    host: '',
  });

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadCluster(id);
    }
  }, [isEdit, id]);

  const loadCluster = async (clusterId: string) => {
    try {
      setLoading(true);
      setError(null);
      const cluster = await ClusterService.getCluster(clusterId);
      
      const parseTimeSpan = (timespan?: string): string => {
        if (!timespan) return '';
        const parts = timespan.split(':');
        if (parts.length === 3) {
          const hours = parseInt(parts[0]) || 0;
          const minutes = parseInt(parts[1]) || 0;
          const seconds = parseInt(parts[2].split('.')[0]) || 0;
          const totalSeconds = hours * 3600 + minutes * 60 + seconds;
          return totalSeconds.toString();
        }
        return '';
      };

      const healthCheck = (cluster as any).healthCheck;
      const sessionAffinity = (cluster as any).sessionAffinity;
      const httpClient = (cluster as any).httpClient;
      
      setFormData({
        name: cluster.name || '',
        loadBalancingPolicy: cluster.loadBalancingPolicy || 'RoundRobin',
        healthCheckEnabled: !!(healthCheck?.active?.enabled || healthCheck?.passive?.enabled),
        sessionAffinityEnabled: sessionAffinity?.enabled || false,
        healthCheckPath: healthCheck?.active?.path || '/health',
        healthCheckInterval: parseTimeSpan(healthCheck?.active?.interval) || '10',
        healthCheckTimeout: parseTimeSpan(healthCheck?.active?.timeout) || '5',
        sessionAffinityPolicy: sessionAffinity?.policy || 'Cookie',
        sessionAffinityFailurePolicy: sessionAffinity?.failurePolicy || 'Redistribute',
        enableActiveHealthCheck: healthCheck?.active?.enabled || false,
        enablePassiveHealthCheck: healthCheck?.passive?.enabled || false,
        passiveReactivationPeriod: parseTimeSpan(healthCheck?.passive?.reactivationPeriod) || '60',
        httpVersion: httpClient?.version || '2.0',
        requestTimeout: httpClient?.requestTimeout ? parseTimeSpan(httpClient.requestTimeout) : '100',
        maxConnectionsPerServer: httpClient?.maxConnectionsPerServer?.toString() || '',
      });
      
      setDestinations(cluster.destinations || []);
    } catch (err) {
      console.error('Failed to load cluster:', err);
      setError('Failed to load cluster');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setSaving(true);
      setError(null);

      const formatTimeSpan = (seconds: string): string => {
        const num = parseInt(seconds) || 0;
        const hours = Math.floor(num / 3600);
        const minutes = Math.floor((num % 3600) / 60);
        const secs = num % 60;
        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
      };

      const clusterData: any = {
        name: formData.name,
        loadBalancingPolicy: formData.loadBalancingPolicy || undefined,
        destinations: destinations.map(d => ({
          address: d.address,
          host: d.host || undefined,
        })),
      };

      if (formData.healthCheckEnabled && (formData.enableActiveHealthCheck || formData.enablePassiveHealthCheck)) {
        clusterData.healthCheck = {};
        
        if (formData.enableActiveHealthCheck) {
          clusterData.healthCheck.active = {
            enabled: true,
            interval: formatTimeSpan(formData.healthCheckInterval),
            timeout: formatTimeSpan(formData.healthCheckTimeout),
            policy: 'ConsecutiveFailures',
            path: formData.healthCheckPath,
          };
        }
        
        if (formData.enablePassiveHealthCheck) {
          clusterData.healthCheck.passive = {
            enabled: true,
            policy: 'TransportFailureRate',
            reactivationPeriod: formatTimeSpan(formData.passiveReactivationPeriod),
          };
        }
      }

      if (formData.sessionAffinityEnabled) {
        clusterData.sessionAffinity = {
          enabled: true,
          policy: formData.sessionAffinityPolicy,
          failurePolicy: formData.sessionAffinityFailurePolicy,
        };
      }

      if (isEdit && id) {
        await ClusterService.updateCluster(id, clusterData);
      } else {
        await ClusterService.createCluster(clusterData);
      }
      
      navigate('/clusters');
    } catch (err) {
      console.error('Failed to save cluster:', err);
      setError('Failed to save cluster');
    } finally {
      setSaving(false);
    }
  };

  const addDestination = () => {
    if (newDestination.address) {
      setDestinations([
        ...destinations,
        { 
          id: `d${Date.now()}`, 
          address: newDestination.address,
          host: newDestination.host,
        }
      ]);
      setNewDestination({ address: '', host: '' });
    }
  };

  const removeDestination = (id: string) => {
    setDestinations(destinations.filter(d => d.id !== id));
  };

  const updateDestination = (id: string, field: keyof Destination, value: string) => {
    setDestinations(destinations.map(d => 
      d.id === id ? { ...d, [field]: value } : d
    ));
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
      <Link to="/clusters" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-4">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Clusters
      </Link>

      <div className="mb-6">
        <h1 className="text-2xl font-medium text-gray-900">
          {isEdit ? 'Edit Cluster' : 'Create Cluster'}
        </h1>
        <p className="text-sm text-gray-600 mt-1">
          Define cluster capabilities and specifications
        </p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-md">
          <p className="text-sm text-red-600">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Configuration */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-4">Basic Configuration</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Cluster Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="e.g., Backend API Cluster"
                className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Load Balancing Policy
              </label>
              <select
                value={formData.loadBalancingPolicy}
                onChange={(e) => setFormData({ ...formData, loadBalancingPolicy: e.target.value })}
                className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
              >
                <option value="RoundRobin">Round Robin</option>
                <option value="LeastRequests">Least Requests</option>
                <option value="Random">Random</option>
                <option value="PowerOfTwoChoices">Power Of Two Choices</option>
              </select>
              <p className="mt-1 text-xs text-gray-500">Algorithm used to distribute requests across destinations</p>
            </div>
          </div>
        </div>

        {/* Destinations */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-4">
            Destinations
            {destinations.length > 0 && (
              <span className="ml-2 text-sm font-normal text-gray-500">({destinations.length})</span>
            )}
            </h2>
            <div className="space-y-4">
            {destinations.length > 0 && (
              <div className="space-y-3">
                {destinations.map((dest) => (
                  <div key={dest.id} className="flex gap-3 items-start p-3 border border-gray-200 rounded-md bg-gray-50">
                    <div className="flex-1 grid grid-cols-2 gap-3">
                      <div>
                        <input
                          type="text"
                          value={dest.address}
                          onChange={(e) => updateDestination(dest.id, 'address', e.target.value)}
                          placeholder="https://example.com/"
                          className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 bg-white"
                        />
                      </div>
                      <div>
                        <input
                          type="text"
                          value={dest.host || ''}
                          onChange={(e) => updateDestination(dest.id, 'host', e.target.value)}
                          placeholder="Host header (optional)"
                          className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 bg-white"
                        />
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => removeDestination(dest.id)}
                      className="p-2 text-gray-400 hover:text-red-600"
                    >
                      <XMarkIcon className="h-5 w-5" />
                    </button>
                  </div>
                ))}
              </div>
            )}

            <div className="flex gap-3 items-end pt-2">
              <div className="flex-1 grid grid-cols-2 gap-3">
                <input
                  type="text"
                  value={newDestination.address}
                  onChange={(e) => setNewDestination({ ...newDestination, address: e.target.value })}
                  placeholder="https://api.example.com:443"
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900"
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addDestination())}
                />
                <input
                  type="text"
                  value={newDestination.host}
                  onChange={(e) => setNewDestination({ ...newDestination, host: e.target.value })}
                  placeholder="Host header (optional)"
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900"
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addDestination())}
                />
              </div>
              <button
                type="button"
                onClick={addDestination}
                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 inline-flex items-center"
              >
                <PlusIcon className="h-4 w-4 mr-1" />
                Add
              </button>
            </div>
          </div>
        </div>

        {/* Health Check */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-base font-semibold text-gray-900">Health Check Configuration</h2>
            <label className="flex items-center cursor-pointer">
              <input
                type="checkbox"
                checked={formData.healthCheckEnabled}
                onChange={(e) => setFormData({ ...formData, healthCheckEnabled: e.target.checked })}
                className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
              />
              <span className="ml-2 text-sm text-gray-700">Enable</span>
            </label>
          </div>

          <div className="space-y-4">
              <div className="flex items-center space-x-4 pb-3 border-b border-gray-200">
                <label className="flex items-center cursor-pointer">
                  <input
                    type="radio"
                    checked={formData.enableActiveHealthCheck && !formData.enablePassiveHealthCheck}
                    onChange={() => setFormData({ 
                      ...formData, 
                      enableActiveHealthCheck: true,
                      enablePassiveHealthCheck: false
                    })}
                    disabled={!formData.healthCheckEnabled}
                    className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  />
                  <span className={`ml-2 text-sm ${formData.healthCheckEnabled ? 'text-gray-700' : 'text-gray-400'}`}>Active</span>
                </label>
                <label className="flex items-center cursor-pointer">
                  <input
                    type="radio"
                    checked={formData.enablePassiveHealthCheck && !formData.enableActiveHealthCheck}
                    onChange={() => setFormData({ 
                      ...formData, 
                      enableActiveHealthCheck: false,
                      enablePassiveHealthCheck: true
                    })}
                    disabled={!formData.healthCheckEnabled}
                    className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  />
                  <span className={`ml-2 text-sm ${formData.healthCheckEnabled ? 'text-gray-700' : 'text-gray-400'}`}>Passive</span>
                </label>
                <label className="flex items-center cursor-pointer">
                  <input
                    type="radio"
                    checked={formData.enableActiveHealthCheck && formData.enablePassiveHealthCheck}
                    onChange={() => setFormData({ 
                      ...formData, 
                      enableActiveHealthCheck: true,
                      enablePassiveHealthCheck: true
                    })}
                    disabled={!formData.healthCheckEnabled}
                    className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  />
                  <span className={`ml-2 text-sm ${formData.healthCheckEnabled ? 'text-gray-700' : 'text-gray-400'}`}>Both</span>
                </label>
              </div>

              {formData.enableActiveHealthCheck && (
                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Path</label>
                    <input
                      type="text"
                      value={formData.healthCheckPath}
                      onChange={(e) => setFormData({ ...formData, healthCheckPath: e.target.value })}
                      placeholder="/api/health"
                      disabled={!formData.healthCheckEnabled}
                      className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Interval (seconds)</label>
                    <input
                      type="number"
                      value={formData.healthCheckInterval}
                      onChange={(e) => setFormData({ ...formData, healthCheckInterval: e.target.value })}
                      min="1"
                      disabled={!formData.healthCheckEnabled}
                      className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Timeout (seconds)</label>
                    <input
                      type="number"
                      value={formData.healthCheckTimeout}
                      onChange={(e) => setFormData({ ...formData, healthCheckTimeout: e.target.value })}
                      min="1"
                      disabled={!formData.healthCheckEnabled}
                      className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed"
                    />
                  </div>
                </div>
              )}

              {formData.enablePassiveHealthCheck && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Reactivation Period (seconds)</label>
                  <input
                    type="number"
                    value={formData.passiveReactivationPeriod}
                    onChange={(e) => setFormData({ ...formData, passiveReactivationPeriod: e.target.value })}
                    min="1"
                    disabled={!formData.healthCheckEnabled}
                    className="block w-full max-w-xs px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed"
                  />
                  <p className="mt-1 text-xs text-gray-500">Time before retrying an unhealthy destination</p>
                </div>
              )}
          </div>
        </div>

        {/* Session Affinity */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-base font-semibold text-gray-900">Session Affinity</h2>
            <label className="flex items-center cursor-pointer">
              <input
                type="checkbox"
                checked={formData.sessionAffinityEnabled}
                onChange={(e) => setFormData({ ...formData, sessionAffinityEnabled: e.target.checked })}
                className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
              />
              <span className="ml-2 text-sm text-gray-700">Enable</span>
            </label>
          </div>

          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Affinity Policy</label>
                <select
                  value={formData.sessionAffinityPolicy}
                  onChange={(e) => setFormData({ ...formData, sessionAffinityPolicy: e.target.value })}
                  disabled={!formData.sessionAffinityEnabled}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed"
                >
                  <option value="Cookie">Cookie</option>
                  <option value="CustomHeader">Custom Header</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Failure Policy</label>
                <select
                  value={formData.sessionAffinityFailurePolicy}
                  onChange={(e) => setFormData({ ...formData, sessionAffinityFailurePolicy: e.target.value })}
                  disabled={!formData.sessionAffinityEnabled}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed"
                >
                  <option value="Redistribute">Redistribute</option>
                  <option value="Return503Error">Return 503 Error</option>
                </select>
              </div>
            </div>
            <p className="text-xs text-gray-500">Route requests from the same client to the same destination</p>
          </div>
        </div>

        {/* HTTP Client Configuration */}
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-4">HTTP Client Configuration</h2>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">HTTP Version</label>
              <select
                value={formData.httpVersion}
                onChange={(e) => setFormData({ ...formData, httpVersion: e.target.value })}
                className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900"
              >
                <option value="1.0">HTTP/1.0</option>
                <option value="1.1">HTTP/1.1</option>
                <option value="2.0">HTTP/2.0</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Request Timeout (seconds)</label>
              <input
                type="number"
                value={formData.requestTimeout}
                onChange={(e) => setFormData({ ...formData, requestTimeout: e.target.value })}
                min="1"
                className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Max Connections</label>
              <input
                type="number"
                value={formData.maxConnectionsPerServer}
                onChange={(e) => setFormData({ ...formData, maxConnectionsPerServer: e.target.value })}
                placeholder="Unlimited"
                min="1"
                className="block w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900"
              />
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-end space-x-3 pt-2">
          <Link
            to="/clusters"
            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={saving}
            className="inline-flex items-center px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-gray-900 hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Saving...
              </>
            ) : (
              <>
                <CheckIcon className="h-4 w-4 mr-2" />
                {isEdit ? 'Update Cluster' : 'Create Cluster'}
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ClusterEdit;
