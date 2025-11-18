import React, { useState, useEffect } from 'react';
import {
  GlobeAltIcon,
  ServerStackIcon,
  ShieldCheckIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
} from '@heroicons/react/24/outline';
import type { GatewayStats } from '../types/gateway';

const mockStats: GatewayStats = {
  totalRoutes: 45,
  activeRoutes: 42,
  totalClusters: 12,
  activeClusters: 11,
  totalDestinations: 36,
  healthyDestinations: 34,
  totalCertificates: 8,
  validCertificates: 7,
  expiringCertificates: 1,
};

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState<GatewayStats>(mockStats);

  useEffect(() => {
    setLoading(true);
    setTimeout(() => {
      setStats(mockStats);
      setLoading(false);
    }, 500);
  }, []);

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
        <h1 className="text-xl font-medium text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-600">Gateway overview and statistics</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <GlobeAltIcon className="h-6 w-6 text-blue-600" />
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">Routes</span>
          </div>
          <div className="text-2xl font-semibold text-gray-900 mb-1">{stats.totalRoutes}</div>
          <div className="flex items-center text-xs text-gray-600">
            <CheckCircleIcon className="h-3 w-3 text-green-600 mr-1" />
            {stats.activeRoutes} active
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <ServerStackIcon className="h-6 w-6 text-purple-600" />
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">Clusters</span>
          </div>
          <div className="text-2xl font-semibold text-gray-900 mb-1">{stats.totalClusters}</div>
          <div className="flex items-center text-xs text-gray-600">
            <CheckCircleIcon className="h-3 w-3 text-green-600 mr-1" />
            {stats.activeClusters} active
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <ShieldCheckIcon className="h-6 w-6 text-green-600" />
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">Certificates</span>
          </div>
          <div className="text-2xl font-semibold text-gray-900 mb-1">{stats.totalCertificates}</div>
          <div className="flex items-center text-xs text-gray-600">
            {stats.expiringCertificates > 0 ? (
              <>
                <ExclamationTriangleIcon className="h-3 w-3 text-yellow-600 mr-1" />
                {stats.expiringCertificates} expiring soon
              </>
            ) : (
              <>
                <CheckCircleIcon className="h-3 w-3 text-green-600 mr-1" />
                All valid
              </>
            )}
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <h3 className="text-sm font-medium text-gray-900 mb-4">Destination Health</h3>
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-xs text-gray-600">Total Destinations</span>
              <span className="text-base font-semibold">{stats.totalDestinations}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-xs text-gray-600">Healthy</span>
              <span className="text-base font-semibold text-green-600">{stats.healthyDestinations}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-xs text-gray-600">Unhealthy</span>
              <span className="text-base font-semibold text-red-600">
                {stats.totalDestinations - stats.healthyDestinations}
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2 mt-3">
              <div 
                className="bg-green-600 h-2 rounded-full" 
                style={{ 
                  width: `${(stats.healthyDestinations / stats.totalDestinations) * 100}%` 
                }}
              />
            </div>
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <h3 className="text-sm font-medium text-gray-900 mb-4">System Status</h3>
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-600">Active Routes</span>
              <span className="text-xs font-medium text-green-600">{stats.activeRoutes}/{stats.totalRoutes}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-600">Active Clusters</span>
              <span className="text-xs font-medium text-green-600">{stats.activeClusters}/{stats.totalClusters}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-600">Valid Certificates</span>
              <span className="text-xs font-medium text-green-600">{stats.validCertificates}/{stats.totalCertificates}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 