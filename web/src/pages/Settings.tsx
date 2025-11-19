import React, { useState } from 'react';
import {
  Cog6ToothIcon,
  BoltIcon,
  HeartIcon,
  DocumentTextIcon,
  ShieldCheckIcon,
} from '@heroicons/react/24/outline';

type TabId = 'general' | 'performance' | 'health' | 'logging' | 'security';

interface Tab {
  id: TabId;
  name: string;
  icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
}

const Settings: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabId>('general');
  const [settings, setSettings] = useState({
    general: {
      gatewayName: 'Sail Gateway',
      httpPort: '80',
      httpsPort: '443',
      adminPort: '8100',
      logLevel: 'Information',
    },
    performance: {
      maxConnections: '10000',
      requestTimeout: '30',
      responseBufferSize: '65536',
      enableCompression: true,
    },
    health: {
      enableHealthCheck: true,
      healthCheckInterval: '30',
      unhealthyThreshold: '3',
      healthCheckPath: '/health',
    },
    logging: {
      logLevel: 'Information',
      logFormat: 'json',
      enableRequestLogging: true,
      enableResponseLogging: false,
    },
    security: {
      enableCors: true,
      allowedOrigins: '*',
      enableHttpsRedirect: false,
      minTlsVersion: '1.2',
    },
  });

  const tabs: Tab[] = [
    { id: 'general', name: 'General', icon: Cog6ToothIcon },
    { id: 'performance', name: 'Performance', icon: BoltIcon },
    { id: 'health', name: 'Health Check', icon: HeartIcon },
    { id: 'logging', name: 'Logging', icon: DocumentTextIcon },
    { id: 'security', name: 'Security', icon: ShieldCheckIcon },
  ];

  const handleSettingChange = (category: string, key: string, value: any) => {
    setSettings(prev => ({
      ...prev,
      [category]: {
        ...prev[category as keyof typeof prev],
        [key]: value
      }
    }));
  };

  const saveSettings = () => {
    console.log('Save settings:', settings);
    // TODO: Implement actual save logic
  };

  return (
    <div className="fade-in">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">Gateway Settings</h1>
        <p className="mt-1 text-sm text-gray-500">Configure gateway behavior, performance, and security</p>
      </div>

      <div className="flex flex-col lg:flex-row gap-6">
        {/* Sidebar Navigation */}
        <div className="lg:w-64">
          <div className="card">
            <nav className="space-y-1">
              {tabs.map((tab) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id)}
                    className={`w-full flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                      activeTab === tab.id
                        ? 'bg-gray-100 text-gray-900'
                        : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
                    }`}
                  >
                    <Icon className="h-4 w-4 mr-3" />
                    {tab.name}
                  </button>
                );
              })}
            </nav>
          </div>
        </div>

        {/* Main Content */}
        <div className="flex-1">
          <div className="card">
            {/* General Settings */}
            {activeTab === 'general' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">General Configuration</h2>
                <div className="space-y-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Gateway Name
                    </label>
                    <input
                      type="text"
                      value={settings.general.gatewayName}
                      onChange={(e) => handleSettingChange('general', 'gatewayName', e.target.value)}
                      className="minimal-input max-w-md"
                      placeholder="Sail Gateway"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Display name for this gateway instance
                    </p>
                  </div>

                  <div className="grid grid-cols-3 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        HTTP Port
                      </label>
                      <input
                        type="number"
                        value={settings.general.httpPort}
                        onChange={(e) => handleSettingChange('general', 'httpPort', e.target.value)}
                        className="minimal-input"
                        min="1"
                        max="65535"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        HTTPS Port
                      </label>
                      <input
                        type="number"
                        value={settings.general.httpsPort}
                        onChange={(e) => handleSettingChange('general', 'httpsPort', e.target.value)}
                        className="minimal-input"
                        min="1"
                        max="65535"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Admin Port
                      </label>
                      <input
                        type="number"
                        value={settings.general.adminPort}
                        onChange={(e) => handleSettingChange('general', 'adminPort', e.target.value)}
                        className="minimal-input"
                        min="1"
                        max="65535"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Log Level
                    </label>
                    <select
                      value={settings.general.logLevel}
                      onChange={(e) => handleSettingChange('general', 'logLevel', e.target.value)}
                      className="minimal-input max-w-xs"
                    >
                      <option value="Trace">Trace</option>
                      <option value="Debug">Debug</option>
                      <option value="Information">Information</option>
                      <option value="Warning">Warning</option>
                      <option value="Error">Error</option>
                      <option value="Critical">Critical</option>
                    </select>
                    <p className="mt-1 text-sm text-gray-500">
                      Minimum log level to record
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Performance Settings */}
            {activeTab === 'performance' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">Performance Configuration</h2>
                <div className="space-y-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Max Connections
                    </label>
                    <input
                      type="number"
                      value={settings.performance.maxConnections}
                      onChange={(e) => handleSettingChange('performance', 'maxConnections', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="100"
                      max="100000"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Maximum number of concurrent connections
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Request Timeout (seconds)
                    </label>
                    <input
                      type="number"
                      value={settings.performance.requestTimeout}
                      onChange={(e) => handleSettingChange('performance', 'requestTimeout', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="1"
                      max="300"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Maximum time to wait for upstream response
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Response Buffer Size (bytes)
                    </label>
                    <input
                      type="number"
                      value={settings.performance.responseBufferSize}
                      onChange={(e) => handleSettingChange('performance', 'responseBufferSize', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="8192"
                      max="1048576"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Buffer size for response streaming
                    </p>
                  </div>

                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.performance.enableCompression}
                        onChange={(e) => handleSettingChange('performance', 'enableCompression', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Enable Response Compression</div>
                        <div className="text-sm text-gray-500">Compress responses using gzip/brotli</div>
                      </div>
                    </label>
                  </div>
                </div>
              </div>
            )}

            {/* Health Check Settings */}
            {activeTab === 'health' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">Health Check Configuration</h2>
                <div className="space-y-6">
                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.health.enableHealthCheck}
                        onChange={(e) => handleSettingChange('health', 'enableHealthCheck', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Enable Health Check Endpoint</div>
                        <div className="text-sm text-gray-500">Expose health check endpoint for monitoring</div>
                      </div>
                    </label>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Health Check Path
                    </label>
                    <input
                      type="text"
                      value={settings.health.healthCheckPath}
                      onChange={(e) => handleSettingChange('health', 'healthCheckPath', e.target.value)}
                      className="minimal-input max-w-md"
                      placeholder="/health"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      URL path for health check endpoint
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Check Interval (seconds)
                    </label>
                    <input
                      type="number"
                      value={settings.health.healthCheckInterval}
                      onChange={(e) => handleSettingChange('health', 'healthCheckInterval', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="5"
                      max="300"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      How often to check upstream health
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Unhealthy Threshold
                    </label>
                    <input
                      type="number"
                      value={settings.health.unhealthyThreshold}
                      onChange={(e) => handleSettingChange('health', 'unhealthyThreshold', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="1"
                      max="10"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Failed checks before marking unhealthy
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Logging Settings */}
            {activeTab === 'logging' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">Logging Configuration</h2>
                <div className="space-y-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Log Level
                    </label>
                    <select
                      value={settings.logging.logLevel}
                      onChange={(e) => handleSettingChange('logging', 'logLevel', e.target.value)}
                      className="minimal-input max-w-xs"
                    >
                      <option value="Trace">Trace</option>
                      <option value="Debug">Debug</option>
                      <option value="Information">Information</option>
                      <option value="Warning">Warning</option>
                      <option value="Error">Error</option>
                      <option value="Critical">Critical</option>
                    </select>
                    <p className="mt-1 text-sm text-gray-500">
                      Minimum log level to record
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Log Format
                    </label>
                    <select
                      value={settings.logging.logFormat}
                      onChange={(e) => handleSettingChange('logging', 'logFormat', e.target.value)}
                      className="minimal-input max-w-xs"
                    >
                      <option value="json">JSON</option>
                      <option value="text">Plain Text</option>
                    </select>
                    <p className="mt-1 text-sm text-gray-500">
                      Output format for log entries
                    </p>
                  </div>

                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.logging.enableRequestLogging}
                        onChange={(e) => handleSettingChange('logging', 'enableRequestLogging', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Enable Request Logging</div>
                        <div className="text-sm text-gray-500">Log incoming HTTP requests</div>
                      </div>
                    </label>
                  </div>

                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.logging.enableResponseLogging}
                        onChange={(e) => handleSettingChange('logging', 'enableResponseLogging', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Enable Response Logging</div>
                        <div className="text-sm text-gray-500">Log outgoing HTTP responses</div>
                      </div>
                    </label>
                  </div>
                </div>
              </div>
            )}

            {/* Security Settings */}
            {activeTab === 'security' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">Security Configuration</h2>
                <div className="space-y-6">
                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.security.enableCors}
                        onChange={(e) => handleSettingChange('security', 'enableCors', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Enable CORS</div>
                        <div className="text-sm text-gray-500">Allow cross-origin requests</div>
                      </div>
                    </label>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Allowed Origins
                    </label>
                    <input
                      type="text"
                      value={settings.security.allowedOrigins}
                      onChange={(e) => handleSettingChange('security', 'allowedOrigins', e.target.value)}
                      className="minimal-input"
                      placeholder="* or https://example.com"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Comma-separated list of allowed origins (* for all)
                    </p>
                  </div>

                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.security.enableHttpsRedirect}
                        onChange={(e) => handleSettingChange('security', 'enableHttpsRedirect', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Force HTTPS Redirect</div>
                        <div className="text-sm text-gray-500">Redirect HTTP requests to HTTPS</div>
                      </div>
                    </label>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Minimum TLS Version
                    </label>
                    <select
                      value={settings.security.minTlsVersion}
                      onChange={(e) => handleSettingChange('security', 'minTlsVersion', e.target.value)}
                      className="minimal-input max-w-xs"
                    >
                      <option value="1.0">TLS 1.0</option>
                      <option value="1.1">TLS 1.1</option>
                      <option value="1.2">TLS 1.2</option>
                      <option value="1.3">TLS 1.3</option>
                    </select>
                    <p className="mt-1 text-sm text-gray-500">
                      Minimum TLS version for secure connections
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Save Button */}
            <div className="mt-8 pt-6 border-t border-gray-200">
              <div className="flex justify-end space-x-3">
                <button className="btn-secondary">
                  Reset
                </button>
                <button onClick={saveSettings} className="btn-primary">
                  Save Settings
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Settings; 