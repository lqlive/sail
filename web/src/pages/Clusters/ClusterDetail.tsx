import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import {
  ChevronLeftIcon,
  PencilIcon,
  CheckCircleIcon,
  XCircleIcon,
  QuestionMarkCircleIcon,
  ServerIcon,
  CpuChipIcon,
  ArrowPathIcon,
  CloudIcon,
  ServerStackIcon
} from '@heroicons/react/24/outline';
import { ClusterService } from '../../services/clusterService';
import { RuntimeService, type DestinationRuntimeState } from '../../services/runtimeService';
import type { Cluster, Destination, DestinationHealth } from '../../types';

const ClusterDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [cluster, setCluster] = useState<Cluster | null>(null);
  const [runtimeDestinations, setRuntimeDestinations] = useState<DestinationRuntimeState[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  const loadCluster = async () => {
    if (!id) return;

    try {
      setLoading(true);
      setError(null);
      const data = await ClusterService.getCluster(id);
      setCluster(data);
      
      const runtimeData = await RuntimeService.getClusterDestinations(id);
      setRuntimeDestinations(runtimeData);
    } catch (err) {
      console.error('Failed to load cluster:', err);
      setError('Failed to load cluster information');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    if (!id) return;

    try {
      setRefreshing(true);
      const data = await ClusterService.getCluster(id);
      setCluster(data);
      
      const runtimeData = await RuntimeService.getClusterDestinations(id);
      setRuntimeDestinations(runtimeData);
    } catch (err) {
      console.error('Failed to refresh cluster:', err);
    } finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    loadCluster();
  }, [id]);

  const getHealthStatus = (health?: string): DestinationHealth => {
    if (!health) return 'unknown';
    const normalized = health.toLowerCase();
    if (normalized === 'healthy') return 'healthy';
    if (normalized === 'unhealthy') return 'unhealthy';
    return 'unknown';
  };

  const getHealthBadge = (health: DestinationHealth, label: string) => {
    switch (health) {
      case 'healthy':
        return (
          <div className="inline-flex items-center gap-1.5">
            <div className="flex items-center gap-1 text-xs font-medium text-green-700">
              <div className="w-2 h-2 rounded-full bg-green-500"></div>
              {label}
            </div>
            <span className="text-xs text-gray-400">•</span>
            <span className="text-xs font-medium text-gray-900">Healthy</span>
          </div>
        );
      case 'unhealthy':
        return (
          <div className="inline-flex items-center gap-1.5">
            <div className="flex items-center gap-1 text-xs font-medium text-red-700">
              <div className="w-2 h-2 rounded-full bg-red-500"></div>
              {label}
            </div>
            <span className="text-xs text-gray-400">•</span>
            <span className="text-xs font-medium text-gray-900">Unhealthy</span>
          </div>
        );
      default:
        return (
          <div className="inline-flex items-center gap-1.5">
            <div className="flex items-center gap-1 text-xs font-medium text-gray-600">
              <div className="w-2 h-2 rounded-full bg-gray-400"></div>
              {label}
            </div>
            <span className="text-xs text-gray-400">•</span>
            <span className="text-xs font-medium text-gray-900">Unknown</span>
          </div>
        );
    }
  };

  const getHealthStats = () => {
    if (!runtimeDestinations || runtimeDestinations.length === 0) {
      return { healthy: 0, unhealthy: 0, unknown: 0, total: 0 };
    }

    const stats = { healthy: 0, unhealthy: 0, unknown: 0, total: runtimeDestinations.length };
    
    runtimeDestinations.forEach(dest => {
      const health = dest.activeHealth.toLowerCase() as DestinationHealth;
      stats[health]++;
    });

    return stats;
  };

  if (loading) {
    return (
      <div className="fade-in">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      </div>
    );
  }

  if (error || !cluster) {
    return (
      <div className="fade-in">
        <Link to="/clusters" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6">
          <ChevronLeftIcon className="h-4 w-4 mr-1" />
          Back to Clusters
        </Link>

        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <h3 className="text-sm font-medium text-red-900 mb-1">Failed to load</h3>
          <p className="text-sm text-red-700">{error || 'Cluster not found'}</p>
        </div>
      </div>
    );
  }

  const healthStats = getHealthStats();

  return (
    <div className="fade-in">
      <Link to="/clusters" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Clusters
      </Link>

      <div className="flex items-start justify-between mb-6">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">{cluster.name}</h1>
          <p className="text-sm text-gray-500 mt-1">Cluster Details</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={handleRefresh}
            disabled={refreshing}
            title="Refresh"
            className="inline-flex items-center justify-center w-9 h-9 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ArrowPathIcon className={`h-5 w-5 ${refreshing ? 'animate-spin' : ''}`} />
          </button>
          <button
            onClick={() => navigate(`/clusters/${id}/edit`)}
            title="Edit"
            className="inline-flex items-center justify-center w-9 h-9 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <PencilIcon className="h-5 w-5" />
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
        <div className="bg-white border border-gray-200 rounded-lg p-5">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-green-50 rounded-lg">
              <CheckCircleIcon className="h-5 w-5 text-green-600" />
            </div>
            <div>
              <p className="text-2xl font-semibold text-gray-900">{healthStats.healthy}</p>
              <p className="text-xs text-gray-500">Healthy</p>
            </div>
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-5">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-red-50 rounded-lg">
              <XCircleIcon className="h-5 w-5 text-red-600" />
            </div>
            <div>
              <p className="text-2xl font-semibold text-gray-900">{healthStats.unhealthy}</p>
              <p className="text-xs text-gray-500">Unhealthy</p>
            </div>
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-5">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-gray-50 rounded-lg">
              <ServerIcon className="h-5 w-5 text-gray-600" />
            </div>
            <div>
              <p className="text-2xl font-semibold text-gray-900">{healthStats.total}</p>
              <p className="text-xs text-gray-500">Total</p>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-lg p-5 mb-6">
        <h2 className="text-base font-medium text-gray-900 mb-4">Basic Information</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4">
          <div>
            <p className="text-xs text-gray-500">Cluster ID</p>
            <p className="text-sm text-gray-900 font-mono mt-1">{cluster.id}</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">Cluster Name</p>
            <p className="text-sm text-gray-900 mt-1">{cluster.name}</p>
          </div>
          {cluster.serviceName && (
            <div>
              <p className="text-xs text-gray-500">Service Name</p>
              <div className="flex items-center gap-2 mt-1">
                <CloudIcon className="h-4 w-4 text-blue-600" />
                <p className="text-sm text-gray-900">{cluster.serviceName}</p>
              </div>
            </div>
          )}
          {cluster.loadBalancingPolicy && (
            <div>
              <p className="text-xs text-gray-500">Load Balancing Policy</p>
              <p className="text-sm text-gray-900 mt-1">{cluster.loadBalancingPolicy}</p>
            </div>
          )}
          <div>
            <p className="text-xs text-gray-500">Created At</p>
            <p className="text-sm text-gray-900 mt-1">{new Date(cluster.createdAt).toLocaleString()}</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">Updated At</p>
            <p className="text-sm text-gray-900 mt-1">{new Date(cluster.updatedAt).toLocaleString()}</p>
          </div>
        </div>
      </div>

      {cluster.healthCheck?.active?.enabled && (
        <div className="bg-white border border-gray-200 rounded-lg p-5 mb-6">
          <h2 className="text-base font-medium text-gray-900 mb-4">Active Health Check</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-4">
            {cluster.healthCheck.active.interval && (
              <div>
                <p className="text-xs text-gray-500">Interval</p>
                <p className="text-sm text-gray-900 mt-1">{cluster.healthCheck.active.interval}</p>
              </div>
            )}
            {cluster.healthCheck.active.timeout && (
              <div>
                <p className="text-xs text-gray-500">Timeout</p>
                <p className="text-sm text-gray-900 mt-1">{cluster.healthCheck.active.timeout}</p>
              </div>
            )}
            {cluster.healthCheck.active.path && (
              <div>
                <p className="text-xs text-gray-500">Path</p>
                <p className="text-sm text-gray-900 mt-1 font-mono">{cluster.healthCheck.active.path}</p>
              </div>
            )}
            {cluster.healthCheck.active.policy && (
              <div>
                <p className="text-xs text-gray-500">Policy</p>
                <p className="text-sm text-gray-900 mt-1">{cluster.healthCheck.active.policy}</p>
              </div>
            )}
          </div>
        </div>
      )}

      {cluster.healthCheck?.passive?.enabled && (
        <div className="bg-white border border-gray-200 rounded-lg p-5 mb-6">
          <h2 className="text-base font-medium text-gray-900 mb-4">Passive Health Check</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4">
            {cluster.healthCheck.passive.policy && (
              <div>
                <p className="text-xs text-gray-500">Policy</p>
                <p className="text-sm text-gray-900 mt-1">{cluster.healthCheck.passive.policy}</p>
              </div>
            )}
            {cluster.healthCheck.passive.reactivationPeriod && (
              <div>
                <p className="text-xs text-gray-500">Reactivation Period</p>
                <p className="text-sm text-gray-900 mt-1">{cluster.healthCheck.passive.reactivationPeriod}</p>
              </div>
            )}
          </div>
        </div>
      )}

      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-medium text-gray-900">Destinations</h2>
          <span className="text-xs text-gray-500">{runtimeDestinations.length || 0} total</span>
        </div>

        {!runtimeDestinations || runtimeDestinations.length === 0 ? (
          <div className="text-center py-12">
            <ServerIcon className="h-12 w-12 text-gray-300 mx-auto mb-3" />
            <p className="text-sm text-gray-500">No destinations</p>
          </div>
        ) : (
          <div className="space-y-3">
            {runtimeDestinations.map((dest) => {
              const activeHealth = dest.activeHealth.toLowerCase() as DestinationHealth;
              const passiveHealth = dest.passiveHealth.toLowerCase() as DestinationHealth;
              return (
                <div
                  key={dest.destinationId}
                  className="border border-gray-200 rounded-lg p-4 hover:border-gray-300 hover:shadow-sm transition-all"
                >
                  <div className="flex items-center justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <div className="grid grid-cols-1 gap-2">
                        <div className="flex items-center gap-2">
                          <span className="text-xs font-medium text-gray-600 w-24 flex-shrink-0">Address</span>
                          <span className="text-xs text-gray-900 font-mono break-all">{dest.address}</span>
                        </div>
                        {dest.host && (
                          <div className="flex items-center gap-2">
                            <span className="text-xs font-medium text-gray-600 w-24 flex-shrink-0">Host</span>
                            <span className="text-xs text-gray-900 break-all">{dest.host}</span>
                          </div>
                        )}
                        <div className="flex items-center gap-2">
                          <span className="text-xs font-medium text-gray-600 w-24 flex-shrink-0">Destination ID</span>
                          <span className="text-xs text-gray-500 font-mono break-all">{dest.destinationId}</span>
                        </div>
                      </div>
                    </div>
                    
                    <div className="flex flex-col gap-2 items-end flex-shrink-0">
                      {getHealthBadge(activeHealth, 'Active')}
                      {passiveHealth !== 'unknown' && getHealthBadge(passiveHealth, 'Passive')}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default ClusterDetail;
