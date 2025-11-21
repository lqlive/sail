import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, CheckIcon, TrashIcon, MagnifyingGlassIcon, GlobeAltIcon } from '@heroicons/react/24/outline';
import { ServiceDiscoveryType } from '../../types/gateway';
import { ServiceDiscoveryService } from '../../services/serviceDiscoveryService';

const ServiceDiscoveryEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== 'new';

  const [formData, setFormData] = useState({
    name: '',
    type: ServiceDiscoveryType.Consul,
    enabled: true,
    // Consul
    consulAddress: '',
    consulToken: '',
    consulDatacenter: '',
    consulRefreshInterval: '60',
    // DNS
    dnsServerAddress: '',
    dnsRefreshInterval: '300',
    dnsPort: '53',
  });

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadServiceDiscovery(id);
    }
  }, [isEdit, id]);

  const loadServiceDiscovery = async (sdId: string) => {
    try {
      setLoading(true);
      setError(null);
      const sd = await ServiceDiscoveryService.getServiceDiscovery(sdId);
      
      setFormData({
        name: sd.name,
        type: sd.type,
        enabled: sd.enabled,
        consulAddress: sd.consul?.address || '',
        consulToken: sd.consul?.token || '',
        consulDatacenter: sd.consul?.datacenter || '',
        consulRefreshInterval: sd.consul?.refreshIntervalSeconds.toString() || '60',
        dnsServerAddress: sd.dns?.serverAddress || '',
        dnsRefreshInterval: sd.dns?.refreshIntervalSeconds.toString() || '300',
        dnsPort: sd.dns?.port.toString() || '53',
      });
    } catch (err) {
      console.error('Failed to load service discovery:', err);
      setError('Failed to load service discovery');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setSaving(true);
      setError(null);

      const data: any = {
        name: formData.name,
        type: formData.type,
        enabled: formData.enabled,
      };

      if (formData.type === ServiceDiscoveryType.Consul) {
        data.consul = {
          address: formData.consulAddress,
          token: formData.consulToken || undefined,
          datacenter: formData.consulDatacenter || undefined,
          refreshIntervalSeconds: parseInt(formData.consulRefreshInterval) || 60,
        };
      } else {
        data.dns = {
          serverAddress: formData.dnsServerAddress || undefined,
          refreshIntervalSeconds: parseInt(formData.dnsRefreshInterval) || 300,
          port: parseInt(formData.dnsPort) || 53,
        };
      }

      if (isEdit && id) {
        await ServiceDiscoveryService.updateServiceDiscovery(id, data);
      } else {
        await ServiceDiscoveryService.createServiceDiscovery(data);
      }
      
      navigate('/service-discoveries');
    } catch (err) {
      console.error('Failed to save service discovery:', err);
      setError('Failed to save service discovery');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!isEdit || !id) return;
    
    if (!confirm('Are you sure you want to delete this service discovery provider? This action cannot be undone.')) {
      return;
    }

    try {
      setDeleting(true);
      setError(null);
      await ServiceDiscoveryService.deleteServiceDiscovery(id);
      navigate('/service-discoveries');
    } catch (err) {
      console.error('Failed to delete service discovery:', err);
      setError('Failed to delete service discovery');
    } finally {
      setDeleting(false);
    }
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
      <Link to="/service-discoveries" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6 transition-colors">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Service Discovery
      </Link>

      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">
          {isEdit ? 'Edit Service Discovery' : 'Add Service Discovery'}
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          Configure service discovery provider settings
        </p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-700 font-medium">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Configuration */}
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-6">Basic Configuration</h2>
          <div className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Provider Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="e.g., Production Consul"
                className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                required
              />
            </div>

            <div>
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="enabled"
                  checked={formData.enabled}
                  onChange={(e) => setFormData({ ...formData, enabled: e.target.checked })}
                  className="h-4 w-4 text-gray-900 border-gray-300 rounded focus:ring-gray-400"
                />
                <label htmlFor="enabled" className="ml-2 text-sm text-gray-700">
                  Enable this provider
                </label>
              </div>
              <p className="mt-1 text-xs text-gray-500 ml-6">When enabled, this provider will be used for service discovery</p>
            </div>
          </div>
        </div>

        {/* Provider Type */}
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-6">Provider Type</h2>
          
          {isEdit ? (
            <div className="p-4 bg-gray-50 border border-gray-200 rounded-lg">
              <div className="flex items-center gap-3">
                {formData.type === ServiceDiscoveryType.Consul ? (
                  <>
                    <MagnifyingGlassIcon className="h-6 w-6 text-purple-600" />
                    <div>
                      <div className="text-sm font-medium text-gray-900">Consul</div>
                      <div className="text-xs text-gray-500">HashiCorp Consul service mesh</div>
                    </div>
                  </>
                ) : (
                  <>
                    <GlobeAltIcon className="h-6 w-6 text-blue-600" />
                    <div>
                      <div className="text-sm font-medium text-gray-900">DNS</div>
                      <div className="text-xs text-gray-500">Domain Name System discovery</div>
                    </div>
                  </>
                )}
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-2 gap-3">
              <button
                type="button"
                onClick={() => setFormData({ ...formData, type: ServiceDiscoveryType.Consul })}
                className={`p-4 border-2 rounded-lg transition-all ${
                  formData.type === ServiceDiscoveryType.Consul
                    ? 'border-purple-500 bg-purple-50'
                    : 'border-gray-200 hover:border-gray-300'
                }`}
              >
                <MagnifyingGlassIcon className={`h-6 w-6 mx-auto mb-2 ${
                  formData.type === ServiceDiscoveryType.Consul ? 'text-purple-600' : 'text-gray-400'
                }`} />
                <div className="text-sm font-medium text-gray-900">Consul</div>
                <div className="text-xs text-gray-500 mt-1">HashiCorp Consul service mesh</div>
              </button>

              <button
                type="button"
                onClick={() => setFormData({ ...formData, type: ServiceDiscoveryType.Dns })}
                className={`p-4 border-2 rounded-lg transition-all ${
                  formData.type === ServiceDiscoveryType.Dns
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200 hover:border-gray-300'
                }`}
              >
                <GlobeAltIcon className={`h-6 w-6 mx-auto mb-2 ${
                  formData.type === ServiceDiscoveryType.Dns ? 'text-blue-600' : 'text-gray-400'
                }`} />
                <div className="text-sm font-medium text-gray-900">DNS</div>
                <div className="text-xs text-gray-500 mt-1">Domain Name System discovery</div>
              </button>
            </div>
          )}
        </div>

        {/* Consul Configuration */}
        {formData.type === ServiceDiscoveryType.Consul && (
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <h2 className="text-base font-semibold text-gray-900 mb-6">Consul Configuration</h2>
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Address <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.consulAddress}
                  onChange={(e) => setFormData({ ...formData, consulAddress: e.target.value })}
                  placeholder="http://localhost:8500"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors font-mono"
                  required
                />
                <p className="mt-1 text-xs text-gray-500">Consul server address with protocol and port</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Token (Optional)
                </label>
                <input
                  type="password"
                  value={formData.consulToken}
                  onChange={(e) => setFormData({ ...formData, consulToken: e.target.value })}
                  placeholder="ACL token for authentication"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors font-mono"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Datacenter (Optional)
                  </label>
                  <input
                    type="text"
                    value={formData.consulDatacenter}
                    onChange={(e) => setFormData({ ...formData, consulDatacenter: e.target.value })}
                    placeholder="dc1"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Refresh Interval (seconds)
                  </label>
                  <input
                    type="number"
                    value={formData.consulRefreshInterval}
                    onChange={(e) => setFormData({ ...formData, consulRefreshInterval: e.target.value })}
                    min="1"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                </div>
              </div>
            </div>
          </div>
        )}

        {/* DNS Configuration */}
        {formData.type === ServiceDiscoveryType.Dns && (
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <h2 className="text-base font-semibold text-gray-900 mb-6">DNS Configuration</h2>
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  DNS Server Address (Optional)
                </label>
                <input
                  type="text"
                  value={formData.dnsServerAddress}
                  onChange={(e) => setFormData({ ...formData, dnsServerAddress: e.target.value })}
                  placeholder="8.8.8.8 (leave empty for system default)"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors font-mono"
                />
                <p className="mt-1 text-xs text-gray-500">Leave empty to use system default DNS server</p>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Port
                  </label>
                  <input
                    type="number"
                    value={formData.dnsPort}
                    onChange={(e) => setFormData({ ...formData, dnsPort: e.target.value })}
                    min="1"
                    max="65535"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Refresh Interval (seconds)
                  </label>
                  <input
                    type="number"
                    value={formData.dnsRefreshInterval}
                    onChange={(e) => setFormData({ ...formData, dnsRefreshInterval: e.target.value })}
                    min="1"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-2">
          <Link
            to="/service-discoveries"
            className="px-5 py-2.5 border border-gray-200 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={saving}
            className="inline-flex items-center px-5 py-2.5 border border-transparent rounded-lg text-sm font-medium text-white bg-black hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {saving ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Saving...
              </>
            ) : (
              <>
                <CheckIcon className="h-4 w-4 mr-2" />
                {isEdit ? 'Update Provider' : 'Add Provider'}
              </>
            )}
          </button>
        </div>
      </form>

      {/* Delete section for edit mode */}
      {isEdit && (
        <div className="mt-8 pt-8 border-t border-gray-200">
          <div className="bg-red-50 rounded-lg p-6">
            <h3 className="text-sm font-semibold text-red-900 mb-2">Delete Service Discovery Provider</h3>
            <p className="text-sm text-red-700 mb-4">
              Once you delete this provider, it cannot be recovered. Please be certain.
            </p>
            <button
              type="button"
              onClick={handleDelete}
              disabled={deleting}
              className="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {deleting ? 'Deleting...' : 'Delete Provider'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default ServiceDiscoveryEdit;

