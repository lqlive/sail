import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, PlusIcon, XMarkIcon } from '@heroicons/react/24/outline';
import type { Destination } from '../../types/gateway';

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
    health: '',
    host: '',
  });

  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit) {
      setLoading(true);
      setTimeout(() => {
        setFormData({
          name: 'Backend API',
          loadBalancingPolicy: 'RoundRobin',
          healthCheckEnabled: true,
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
        setDestinations([
          { id: 'd1', address: 'https://api1.example.com:443', health: 'healthy', host: '' },
          { id: 'd2', address: 'https://api2.example.com:443', health: 'healthy', host: '' },
        ]);
        setLoading(false);
      }, 300);
    }
  }, [isEdit]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Save cluster:', { ...formData, destinations });
    navigate('/clusters');
  };

  const addDestination = () => {
    if (newDestination.address) {
      setDestinations([
        ...destinations,
        { 
          id: `d${Date.now()}`, 
          ...newDestination 
        }
      ]);
      setNewDestination({ address: '', health: '', host: '' });
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
      <Link to="/clusters" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Clusters
      </Link>

      <div className="mb-6">
        <h1 className="text-lg font-medium text-gray-900">
          {isEdit ? 'Edit Cluster' : 'Add Cluster'}
        </h1>
        <p className="text-sm text-gray-600 mt-1">
          {isEdit ? 'Update cluster configuration' : 'Configure a new backend cluster'}
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <div className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                Cluster Name *
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                placeholder="e.g., Backend API"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                Load Balancing Policy
              </label>
              <select
                value={formData.loadBalancingPolicy}
                onChange={(e) => setFormData({ ...formData, loadBalancingPolicy: e.target.value })}
                className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
              >
                <option value="RoundRobin">Round Robin</option>
                <option value="LeastRequests">Least Requests</option>
                <option value="Random">Random</option>
                <option value="PowerOfTwoChoices">Power Of Two Choices</option>
                <option value="FirstAlphabetical">First Alphabetical</option>
              </select>
              <p className="mt-1.5 text-xs text-gray-500">
                Algorithm used to distribute requests across destinations
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-900 mb-1.5">
                Destinations
              </label>
              
              {destinations.length > 0 && (
                <div className="space-y-2 mb-3">
                  {destinations.map((dest) => (
                    <div key={dest.id} className="flex gap-2 items-start p-3 border border-gray-200 rounded-md bg-gray-50">
                      <div className="flex-1 space-y-2">
                        <input
                          type="text"
                          value={dest.address}
                          onChange={(e) => updateDestination(dest.id, 'address', e.target.value)}
                          className="block w-full px-3 py-1.5 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                          placeholder="https://api1.example.com:443"
                        />
                        <input
                          type="text"
                          value={dest.host || ''}
                          onChange={(e) => updateDestination(dest.id, 'host', e.target.value)}
                          className="block w-full px-3 py-1.5 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                          placeholder="Host header (optional)"
                        />
                      </div>
                      <button
                        type="button"
                        onClick={() => removeDestination(dest.id)}
                        className="p-1.5 text-gray-400 hover:text-red-600"
                      >
                        <XMarkIcon className="h-4 w-4" />
                      </button>
                    </div>
                  ))}
                </div>
              )}

              <div className="flex gap-2">
                <input
                  type="text"
                  value={newDestination.address}
                  onChange={(e) => setNewDestination({ ...newDestination, address: e.target.value })}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addDestination())}
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="https://api.example.com:443"
                />
                <button
                  type="button"
                  onClick={addDestination}
                  className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                >
                  <PlusIcon className="h-4 w-4 mr-2" />
                  Add
                </button>
              </div>
            </div>

            <div className="border-t border-gray-200 pt-5">
              <h3 className="text-sm font-medium text-gray-900 mb-4">Health Check Configuration</h3>
              
              <div className="space-y-4">
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="healthCheckEnabled"
                    checked={formData.healthCheckEnabled}
                    onChange={(e) => setFormData({ ...formData, healthCheckEnabled: e.target.checked })}
                    className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
                  />
                  <label htmlFor="healthCheckEnabled" className="ml-2 text-sm text-gray-900">
                    Enable health checks
                  </label>
                </div>

                {formData.healthCheckEnabled && (
                  <div className="pl-6 space-y-4">
                    <div className="flex items-center space-x-4">
                      <label className="flex items-center">
                        <input
                          type="radio"
                          checked={formData.enableActiveHealthCheck}
                          onChange={(e) => setFormData({ ...formData, enableActiveHealthCheck: e.target.checked })}
                          className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300"
                        />
                        <span className="ml-2 text-sm text-gray-900">Active</span>
                      </label>
                      <label className="flex items-center">
                        <input
                          type="radio"
                          checked={formData.enablePassiveHealthCheck}
                          onChange={(e) => setFormData({ ...formData, enablePassiveHealthCheck: e.target.checked, enableActiveHealthCheck: !e.target.checked })}
                          className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300"
                        />
                        <span className="ml-2 text-sm text-gray-900">Passive</span>
                      </label>
                    </div>

                    {formData.enableActiveHealthCheck && (
                      <div className="grid grid-cols-3 gap-4">
                        <div>
                          <label className="block text-sm font-medium text-gray-900 mb-1.5">
                            Path
                          </label>
                          <input
                            type="text"
                            value={formData.healthCheckPath}
                            onChange={(e) => setFormData({ ...formData, healthCheckPath: e.target.value })}
                            className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                            placeholder="/health"
                          />
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-900 mb-1.5">
                            Interval (seconds)
                          </label>
                          <input
                            type="number"
                            value={formData.healthCheckInterval}
                            onChange={(e) => setFormData({ ...formData, healthCheckInterval: e.target.value })}
                            className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                            placeholder="10"
                          />
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-900 mb-1.5">
                            Timeout (seconds)
                          </label>
                          <input
                            type="number"
                            value={formData.healthCheckTimeout}
                            onChange={(e) => setFormData({ ...formData, healthCheckTimeout: e.target.value })}
                            className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                            placeholder="5"
                          />
                        </div>
                      </div>
                    )}

                    {formData.enablePassiveHealthCheck && (
                      <div>
                        <label className="block text-sm font-medium text-gray-900 mb-1.5">
                          Reactivation Period (seconds)
                        </label>
                        <input
                          type="number"
                          value={formData.passiveReactivationPeriod}
                          onChange={(e) => setFormData({ ...formData, passiveReactivationPeriod: e.target.value })}
                          className="block w-1/3 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                          placeholder="60"
                        />
                        <p className="mt-1.5 text-xs text-gray-500">
                          Time before retrying an unhealthy destination
                        </p>
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>

            <div className="border-t border-gray-200 pt-5">
              <h3 className="text-sm font-medium text-gray-900 mb-4">Session Affinity</h3>
              
              <div className="space-y-4">
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="sessionAffinityEnabled"
                    checked={formData.sessionAffinityEnabled}
                    onChange={(e) => setFormData({ ...formData, sessionAffinityEnabled: e.target.checked })}
                    className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
                  />
                  <label htmlFor="sessionAffinityEnabled" className="ml-2 text-sm text-gray-900">
                    Enable session affinity
                  </label>
                </div>
                <p className="text-xs text-gray-500">
                  Route requests from the same client to the same destination
                </p>

                {formData.sessionAffinityEnabled && (
                  <div className="pl-6 space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-900 mb-1.5">
                          Affinity Policy
                        </label>
                        <select
                          value={formData.sessionAffinityPolicy}
                          onChange={(e) => setFormData({ ...formData, sessionAffinityPolicy: e.target.value })}
                          className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                        >
                          <option value="Cookie">Cookie</option>
                          <option value="CustomHeader">Custom Header</option>
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-900 mb-1.5">
                          Failure Policy
                        </label>
                        <select
                          value={formData.sessionAffinityFailurePolicy}
                          onChange={(e) => setFormData({ ...formData, sessionAffinityFailurePolicy: e.target.value })}
                          className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                        >
                          <option value="Redistribute">Redistribute</option>
                          <option value="Return503Error">Return 503 Error</option>
                        </select>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </div>

            <div className="border-t border-gray-200 pt-5">
              <h3 className="text-sm font-medium text-gray-900 mb-4">HTTP Client Configuration</h3>
              
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-900 mb-1.5">
                    HTTP Version
                  </label>
                  <select
                    value={formData.httpVersion}
                    onChange={(e) => setFormData({ ...formData, httpVersion: e.target.value })}
                    className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  >
                    <option value="1.0">HTTP/1.0</option>
                    <option value="1.1">HTTP/1.1</option>
                    <option value="2.0">HTTP/2.0</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-900 mb-1.5">
                    Request Timeout (seconds)
                  </label>
                  <input
                    type="number"
                    value={formData.requestTimeout}
                    onChange={(e) => setFormData({ ...formData, requestTimeout: e.target.value })}
                    className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                    placeholder="100"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-900 mb-1.5">
                    Max Connections Per Server
                  </label>
                  <input
                    type="number"
                    value={formData.maxConnectionsPerServer}
                    onChange={(e) => setFormData({ ...formData, maxConnectionsPerServer: e.target.value })}
                    className="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                    placeholder="Leave empty for unlimited"
                  />
                  <p className="mt-1.5 text-xs text-gray-500">
                    Leave empty for unlimited connections
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="mt-6 flex justify-start space-x-3">
          <button
            type="submit"
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-md text-sm font-medium text-white bg-gray-900 hover:bg-gray-800"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            {isEdit ? 'Update Cluster' : 'Create Cluster'}
          </button>
          <Link
            to="/clusters"
            className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            Cancel
          </Link>
        </div>
      </form>
    </div>
  );
};

export default ClusterEdit;
