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
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">Dashboard</h1>
        <p className="mt-1 text-sm text-gray-500">Gateway overview and statistics</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
        {/* Routes Card */}
        <div className="bg-white border border-gray-200 rounded-lg p-6 hover:border-gray-300 transition-colors">
          <div className="flex items-center gap-4 mb-4">
            <div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center flex-shrink-0">
              <GlobeAltIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="flex-1">
              <div className="text-sm font-medium text-gray-500 mb-1">Routes</div>
              <div className="text-3xl font-semibold text-gray-900">{stats.totalRoutes}</div>
            </div>
          </div>
          <div className="flex items-center text-sm text-gray-600 pt-4 border-t border-gray-100">
            <CheckCircleIcon className="h-4 w-4 text-green-600 mr-2" />
            <span className="font-medium">{stats.activeRoutes}</span>
            <span className="ml-1">active routes</span>
          </div>
        </div>

        {/* Clusters Card */}
        <div className="bg-white border border-gray-200 rounded-lg p-6 hover:border-gray-300 transition-colors">
          <div className="flex items-center gap-4 mb-4">
            <div className="w-12 h-12 bg-purple-50 rounded-lg flex items-center justify-center flex-shrink-0">
              <ServerStackIcon className="h-6 w-6 text-purple-600" />
            </div>
            <div className="flex-1">
              <div className="text-sm font-medium text-gray-500 mb-1">Clusters</div>
              <div className="text-3xl font-semibold text-gray-900">{stats.totalClusters}</div>
            </div>
          </div>
          <div className="flex items-center text-sm text-gray-600 pt-4 border-t border-gray-100">
            <CheckCircleIcon className="h-4 w-4 text-green-600 mr-2" />
            <span className="font-medium">{stats.activeClusters}</span>
            <span className="ml-1">active clusters</span>
          </div>
        </div>

        {/* Certificates Card */}
        <div className="bg-white border border-gray-200 rounded-lg p-6 hover:border-gray-300 transition-colors">
          <div className="flex items-center gap-4 mb-4">
            <div className="w-12 h-12 bg-green-50 rounded-lg flex items-center justify-center flex-shrink-0">
              <ShieldCheckIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="flex-1">
              <div className="text-sm font-medium text-gray-500 mb-1">Certificates</div>
              <div className="text-3xl font-semibold text-gray-900">{stats.totalCertificates}</div>
            </div>
          </div>
          <div className="flex items-center text-sm pt-4 border-t border-gray-100">
            {stats.expiringCertificates > 0 ? (
              <>
                <ExclamationTriangleIcon className="h-4 w-4 text-yellow-600 mr-2" />
                <span className="font-medium text-yellow-700">{stats.expiringCertificates}</span>
                <span className="ml-1 text-gray-600">expiring soon</span>
              </>
            ) : (
              <>
                <CheckCircleIcon className="h-4 w-4 text-green-600 mr-2" />
                <span className="text-gray-600">All certificates valid</span>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Detail Cards */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Destination Health Card */}
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <h3 className="text-base font-semibold text-gray-900 mb-6">Destination Health</h3>
          <div className="space-y-5">
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">Total Destinations</span>
              <span className="text-lg font-semibold text-gray-900">{stats.totalDestinations}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">Healthy</span>
              <span className="text-lg font-semibold text-green-600">{stats.healthyDestinations}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">Unhealthy</span>
              <span className="text-lg font-semibold text-red-600">
                {stats.totalDestinations - stats.healthyDestinations}
              </span>
            </div>
            
            {/* Progress Bar */}
            <div className="pt-2">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs font-medium text-gray-500">Health Rate</span>
                <span className="text-xs font-semibold text-gray-900">
                  {Math.round((stats.healthyDestinations / stats.totalDestinations) * 100)}%
                </span>
              </div>
              <div className="w-full bg-gray-100 rounded-full h-2.5">
                <div 
                  className="bg-green-500 h-2.5 rounded-full transition-all duration-300" 
                  style={{ 
                    width: `${(stats.healthyDestinations / stats.totalDestinations) * 100}%` 
                  }}
                />
              </div>
            </div>
          </div>
        </div>

        {/* System Status Card */}
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <h3 className="text-base font-semibold text-gray-900 mb-6">System Status</h3>
          <div className="space-y-5">
            <div className="flex items-center justify-between py-3 border-b border-gray-100">
              <span className="text-sm text-gray-600">Active Routes</span>
              <div className="flex items-center gap-2">
                <span className="text-sm font-semibold text-gray-900">{stats.activeRoutes}/{stats.totalRoutes}</span>
                <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              </div>
            </div>
            <div className="flex items-center justify-between py-3 border-b border-gray-100">
              <span className="text-sm text-gray-600">Active Clusters</span>
              <div className="flex items-center gap-2">
                <span className="text-sm font-semibold text-gray-900">{stats.activeClusters}/{stats.totalClusters}</span>
                <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              </div>
            </div>
            <div className="flex items-center justify-between py-3">
              <span className="text-sm text-gray-600">Valid Certificates</span>
              <div className="flex items-center gap-2">
                <span className="text-sm font-semibold text-gray-900">{stats.validCertificates}/{stats.totalCertificates}</span>
                <div className={`w-2 h-2 rounded-full ${stats.expiringCertificates > 0 ? 'bg-yellow-500' : 'bg-green-500'}`}></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 