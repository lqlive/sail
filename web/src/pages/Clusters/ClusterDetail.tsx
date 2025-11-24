import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import {
  ChevronLeftIcon,
  PencilIcon,
  TrashIcon,
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
import type { Cluster, Destination, DestinationHealth } from '../../types/gateway';

const ClusterDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [cluster, setCluster] = useState<Cluster | null>(null);
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
    } catch (err) {
      console.error('Failed to refresh cluster:', err);
    } finally {
      setRefreshing(false);
    }
  };

  const handleDelete = async () => {
    if (!id || !cluster) return;

    if (!window.confirm(`Are you sure you want to delete cluster "${cluster.name}"?`)) {
      return;
    }

    try {
      await ClusterService.deleteCluster(id);
      navigate('/clusters');
    } catch (err) {
      console.error('Failed to delete cluster:', err);
      alert('Failed to delete cluster');
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

  const getHealthBadge = (health: DestinationHealth) => {
    switch (health) {
      case 'healthy':
        return (
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium bg-green-50 text-green-700">
            <CheckCircleIcon className="h-3.5 w-3.5" />
            Healthy
          </span>
        );
      case 'unhealthy':
        return (
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium bg-red-50 text-red-700">
            <XCircleIcon className="h-3.5 w-3.5" />
            Unhealthy
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium bg-gray-50 text-gray-700">
            <QuestionMarkCircleIcon className="h-3.5 w-3.5" />
            Unknown
          </span>
        );
    }
  };

  const getHealthStats = () => {
    if (!cluster?.destinations) return { healthy: 0, unhealthy: 0, unknown: 0, total: 0 };

    const stats = { healthy: 0, unhealthy: 0, unknown: 0, total: cluster.destinations.length };
    
    cluster.destinations.forEach(dest => {
      const health = getHealthStatus(dest.health);
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
            className="inline-flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ArrowPathIcon className={`h-4 w-4 ${refreshing ? 'animate-spin' : ''}`} />
            Refresh
          </button>
          <button
            onClick={() => navigate(`/clusters/${id}/edit`)}
            className="inline-flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
          >
            <PencilIcon className="h-4 w-4" />
            Edit
          </button>
          <button
            onClick={handleDelete}
            className="inline-flex items-center gap-2 px-4 py-2 border border-red-300 rounded-lg text-sm font-medium text-red-700 bg-white hover:bg-red-50 transition-colors"
          >
            <TrashIcon className="h-4 w-4" />
            Delete
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
          <span className="text-xs text-gray-500">{cluster.destinations?.length || 0} total</span>
        </div>

        {!cluster.destinations || cluster.destinations.length === 0 ? (
          <div className="text-center py-12">
            <ServerIcon className="h-12 w-12 text-gray-300 mx-auto mb-3" />
            <p className="text-sm text-gray-500">No destinations</p>
          </div>
        ) : (
          <div className="space-y-3">
            {cluster.destinations.map((dest) => {
              const health = getHealthStatus(dest.health);
              return (
                <div
                  key={dest.id}
                  className="border border-gray-200 rounded-lg p-4 hover:border-gray-300 transition-colors"
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex items-center gap-2">
                      <div className="p-2 bg-purple-50 rounded-lg">
                        <ServerStackIcon className="h-4 w-4 text-purple-600" />
                      </div>
                      {getHealthBadge(health)}
                    </div>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex items-start gap-2">
                      <span className="text-xs text-gray-500 w-16 flex-shrink-0">Address:</span>
                      <span className="text-xs text-gray-900 font-mono break-all">{dest.address}</span>
                    </div>
                    {dest.host && (
                      <div className="flex items-start gap-2">
                        <span className="text-xs text-gray-500 w-16 flex-shrink-0">Host:</span>
                        <span className="text-xs text-gray-700 break-all">{dest.host}</span>
                      </div>
                    )}
                    <div className="flex items-start gap-2">
                      <span className="text-xs text-gray-500 w-16 flex-shrink-0">ID:</span>
                      <span className="text-xs text-gray-400 font-mono">{dest.id}</span>
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
