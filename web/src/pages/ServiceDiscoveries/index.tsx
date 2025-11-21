import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  SignalIcon,
  GlobeAltIcon,
} from '@heroicons/react/24/outline';
import { ServiceDiscoveryType } from '../../types/gateway';
import type { ServiceDiscovery } from '../../types/gateway';
import { ServiceDiscoveryService } from '../../services/serviceDiscoveryService';

const ServiceDiscoveries: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [serviceDiscoveries, setServiceDiscoveries] = useState<ServiceDiscovery[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [typeFilter, setTypeFilter] = useState<'all' | ServiceDiscoveryType>('all');
  const [error, setError] = useState<string | null>(null);

  const loadServiceDiscoveries = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ServiceDiscoveryService.getServiceDiscoveries(searchTerm);
      setServiceDiscoveries(data);
    } catch (err) {
      console.error('Failed to load service discoveries:', err);
      setError('Failed to load service discoveries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadServiceDiscoveries();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== '') {
        loadServiceDiscoveries();
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const filteredServiceDiscoveries = serviceDiscoveries.filter(sd => {
    if (typeFilter === 'all') return true;
    return sd.type === typeFilter;
  });

  const stats = {
    total: serviceDiscoveries.length,
    enabled: serviceDiscoveries.filter(sd => sd.enabled).length,
    consul: serviceDiscoveries.filter(sd => sd.type === ServiceDiscoveryType.Consul).length,
    dns: serviceDiscoveries.filter(sd => sd.type === ServiceDiscoveryType.Dns).length,
  };

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
            <h1 className="text-2xl font-semibold text-gray-900">Service Discovery</h1>
            <p className="mt-1 text-sm text-gray-500">Manage service discovery providers</p>
          </div>
          <Link 
            to="/service-discoveries/new" 
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Add Provider
          </Link>
        </div>

        {/* Search and Filter */}
        <div className="flex flex-col sm:flex-row gap-4 sm:items-center sm:justify-between">
          <div className="relative flex-1 max-w-md">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="Search providers..."
            />
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={() => setTypeFilter('all')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                typeFilter === 'all'
                  ? 'bg-gray-900 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              All
            </button>
            <button
              onClick={() => setTypeFilter(ServiceDiscoveryType.Consul)}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                typeFilter === ServiceDiscoveryType.Consul
                  ? 'bg-purple-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              <MagnifyingGlassIcon className="h-4 w-4 mr-1.5" />
              Consul
            </button>
            <button
              onClick={() => setTypeFilter(ServiceDiscoveryType.Dns)}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                typeFilter === ServiceDiscoveryType.Dns
                  ? 'bg-blue-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              <GlobeAltIcon className="h-4 w-4 mr-1.5" />
              DNS
            </button>
          </div>
        </div>

        <div className="flex items-center text-sm text-gray-500 font-medium mt-4">
          <SignalIcon className="h-4 w-4 mr-1.5" />
          {filteredServiceDiscoveries.length} {filteredServiceDiscoveries.length === 1 ? 'provider' : 'providers'}
        </div>
      </div>

      {error ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="h-6 w-6 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">Failed to load service discovery providers</h3>
          <p className="text-sm text-gray-500 mb-4">Please check your connection and try again</p>
          <button
            onClick={loadServiceDiscoveries}
            className="inline-flex items-center px-4 py-2 bg-white border border-gray-200 text-sm font-medium rounded-lg hover:bg-gray-50 transition-colors"
          >
            Try Again
          </button>
        </div>
      ) : serviceDiscoveries.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <PlusIcon className="h-6 w-6 text-gray-400" />
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">No providers yet</h3>
          <p className="text-sm text-gray-500 mb-4">Get started by adding your first service discovery provider</p>
          <Link
            to="/service-discoveries/new"
            className="inline-flex items-center px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Add Provider
          </Link>
        </div>
      ) : filteredServiceDiscoveries.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <p className="text-sm text-gray-500">No providers match your search</p>
        </div>
      ) : (
        <div className="space-y-3">
          {filteredServiceDiscoveries.map((sd) => (
            <Link
              key={sd.id}
              to={`/service-discoveries/${sd.id}`}
              className="block bg-white border border-gray-200 rounded-lg p-5 hover:border-gray-300 hover:shadow-sm transition-all"
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4 flex-1 min-w-0">
                  <div className={`w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 ${
                    sd.type === ServiceDiscoveryType.Consul ? 'bg-purple-50' : 'bg-blue-50'
                  }`}>
                    {sd.type === ServiceDiscoveryType.Consul ? (
                      <MagnifyingGlassIcon className="h-5 w-5 text-purple-600" />
                    ) : (
                      <GlobeAltIcon className="h-5 w-5 text-blue-600" />
                    )}
                  </div>
                  
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-3">
                      <h3 className="text-base font-medium text-gray-900">{sd.name}</h3>
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        sd.type === ServiceDiscoveryType.Consul 
                          ? 'bg-purple-100 text-purple-800' 
                          : 'bg-blue-100 text-blue-800'
                      }`}>
                        {sd.type === ServiceDiscoveryType.Consul ? 'Consul' : 'DNS'}
                      </span>
                      {sd.enabled && (
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                          <div className="w-1.5 h-1.5 bg-green-500 rounded-full"></div>
                          Active
                        </span>
                      )}
                    </div>
                    <div className="mt-1 flex items-center gap-4 text-sm text-gray-500">
                      {sd.consul && (
                        <>
                          <span className="font-mono text-xs">{sd.consul.address}</span>
                          {sd.consul.datacenter && <span>DC: {sd.consul.datacenter}</span>}
                          <span>Refresh: {sd.consul.refreshIntervalSeconds}s</span>
                        </>
                      )}
                      {sd.dns && (
                        <>
                          {sd.dns.serverAddress && (
                            <span className="font-mono text-xs">{sd.dns.serverAddress}:{sd.dns.port}</span>
                          )}
                          <span>Refresh: {sd.dns.refreshIntervalSeconds}s</span>
                        </>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
};

export default ServiceDiscoveries;

