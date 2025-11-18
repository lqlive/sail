import React, { useState } from 'react';
import {
  Cog6ToothIcon,
  PuzzlePieceIcon,
  LinkIcon,
  DocumentArrowDownIcon,
  PlayIcon,
} from '@heroicons/react/24/outline';

type TabId = 'system' | 'runners' | 'integrations' | 'plugins' | 'backup';

interface Tab {
  id: TabId;
  name: string;
  icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
}

const Settings: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabId>('system');
  const [settings, setSettings] = useState({
    system: {
      maxConcurrentBuilds: '10',
      defaultTimeout: '30',
      retentionDays: '30',
      enableBuildCache: true,
    },
    runners: {
      dockerEnabled: true,
      maxRunners: '5',
      defaultImage: 'node:18',
    },
    integrations: {
      slackWebhook: '',
      emailServer: '',
      githubToken: '',
    },
  });

  const tabs: Tab[] = [
    { id: 'system', name: 'System', icon: Cog6ToothIcon },
    { id: 'runners', name: 'Runners', icon: PlayIcon },
    { id: 'integrations', name: 'Integrations', icon: LinkIcon },
    { id: 'plugins', name: 'Plugins', icon: PuzzlePieceIcon },
    { id: 'backup', name: 'Backup', icon: DocumentArrowDownIcon },
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
      {/* Page Title */}
      <div className="section-header">
        <h1 className="text-xl font-medium text-gray-900">System Settings</h1>
        <p className="text-sm text-gray-600">Configure CI/CD system settings and integrations</p>
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
            {/* System Settings */}
            {activeTab === 'system' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">System Configuration</h2>
                <div className="space-y-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Max Concurrent Builds
                    </label>
                    <input
                      type="number"
                      value={settings.system.maxConcurrentBuilds}
                      onChange={(e) => handleSettingChange('system', 'maxConcurrentBuilds', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="1"
                      max="50"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Maximum number of builds that can run simultaneously
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Default Build Timeout (minutes)
                    </label>
                    <input
                      type="number"
                      value={settings.system.defaultTimeout}
                      onChange={(e) => handleSettingChange('system', 'defaultTimeout', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="1"
                      max="600"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Default timeout for build jobs
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Build History Retention (days)
                    </label>
                    <input
                      type="number"
                      value={settings.system.retentionDays}
                      onChange={(e) => handleSettingChange('system', 'retentionDays', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="1"
                      max="365"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      How long to keep build logs and artifacts
                    </p>
                  </div>

                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.system.enableBuildCache}
                        onChange={(e) => handleSettingChange('system', 'enableBuildCache', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Enable Build Cache</div>
                        <div className="text-sm text-gray-500">Cache dependencies and build artifacts to speed up builds</div>
                      </div>
                    </label>
                  </div>
                </div>
              </div>
            )}

            {/* Runners Settings */}
            {activeTab === 'runners' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">Build Runners</h2>
                <div className="space-y-6">
                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.runners.dockerEnabled}
                        onChange={(e) => handleSettingChange('runners', 'dockerEnabled', e.target.checked)}
                        className="mr-3"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">Enable Docker Runners</div>
                        <div className="text-sm text-gray-500">Allow builds to run in Docker containers</div>
                      </div>
                    </label>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Maximum Runners
                    </label>
                    <input
                      type="number"
                      value={settings.runners.maxRunners}
                      onChange={(e) => handleSettingChange('runners', 'maxRunners', e.target.value)}
                      className="minimal-input max-w-xs"
                      min="1"
                      max="20"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Maximum number of concurrent runners
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Default Docker Image
                    </label>
                    <input
                      type="text"
                      value={settings.runners.defaultImage}
                      onChange={(e) => handleSettingChange('runners', 'defaultImage', e.target.value)}
                      className="minimal-input max-w-md"
                      placeholder="node:18"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Default Docker image for builds
                    </p>
                  </div>

                  <div className="pt-4 border-t border-gray-200">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Active Runners</h3>
                    <div className="space-y-3">
                      <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">Runner #1</div>
                          <div className="text-sm text-gray-500">Docker • Running • node:18</div>
                        </div>
                        <span className="text-xs text-green-600 font-medium">Active</span>
                      </div>
                      <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">Runner #2</div>
                          <div className="text-sm text-gray-500">Docker • Idle • python:3.11</div>
                        </div>
                        <span className="text-xs text-gray-500 font-medium">Idle</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Integrations Settings */}
            {activeTab === 'integrations' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">External Integrations</h2>
                <div className="space-y-8">
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Slack Integration</h3>
                    <div className="space-y-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Webhook URL
                        </label>
                        <input
                          type="url"
                          value={settings.integrations.slackWebhook}
                          onChange={(e) => handleSettingChange('integrations', 'slackWebhook', e.target.value)}
                          className="minimal-input"
                          placeholder="https://hooks.slack.com/services/..."
                        />
                      </div>
                      <button className="btn-secondary text-sm">Test Connection</button>
                    </div>
                  </div>

                  <div className="pt-6 border-t border-gray-200">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Email Server</h3>
                    <div className="space-y-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          SMTP Server
                        </label>
                        <input
                          type="text"
                          value={settings.integrations.emailServer}
                          onChange={(e) => handleSettingChange('integrations', 'emailServer', e.target.value)}
                          className="minimal-input"
                          placeholder="smtp.gmail.com:587"
                        />
                      </div>
                      <button className="btn-secondary text-sm">Test Email</button>
                    </div>
                  </div>

                  <div className="pt-6 border-t border-gray-200">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">GitHub Integration</h3>
                    <div className="space-y-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Personal Access Token
                        </label>
                        <input
                          type="password"
                          value={settings.integrations.githubToken}
                          onChange={(e) => handleSettingChange('integrations', 'githubToken', e.target.value)}
                          className="minimal-input"
                          placeholder="ghp_..."
                        />
                      </div>
                      <button className="btn-secondary text-sm">Verify Token</button>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Plugins Settings */}
            {activeTab === 'plugins' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">Plugin Management</h2>
                <div className="space-y-6">
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Installed Plugins</h3>
                    <div className="space-y-3">
                      <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">Docker Build Plugin</div>
                          <div className="text-sm text-gray-500">Build and push Docker images • v1.2.3</div>
                        </div>
                        <div className="flex space-x-2">
                          <button className="btn-secondary text-sm">Configure</button>
                          <button className="btn-danger text-sm">Disable</button>
                        </div>
                      </div>
                      <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">Slack Notifications</div>
                          <div className="text-sm text-gray-500">Send build notifications to Slack • v2.1.0</div>
                        </div>
                        <div className="flex space-x-2">
                          <button className="btn-secondary text-sm">Configure</button>
                          <button className="btn-danger text-sm">Disable</button>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="pt-6 border-t border-gray-200">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Available Plugins</h3>
                    <div className="space-y-3">
                      <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">AWS Deploy</div>
                          <div className="text-sm text-gray-500">Deploy applications to AWS services</div>
                        </div>
                        <button className="btn-primary text-sm">Install</button>
                      </div>
                      <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">Test Reporter</div>
                          <div className="text-sm text-gray-500">Generate test coverage reports</div>
                        </div>
                        <button className="btn-primary text-sm">Install</button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Backup Settings */}
            {activeTab === 'backup' && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-6">Backup & Recovery</h2>
                <div className="space-y-6">
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Automatic Backups</h3>
                    <div className="space-y-4">
                      <label className="flex items-center">
                        <input type="checkbox" defaultChecked className="mr-3" />
                        <div>
                          <div className="text-sm font-medium text-gray-900">Enable automatic backups</div>
                          <div className="text-sm text-gray-500">Automatically backup system data daily</div>
                        </div>
                      </label>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Backup Schedule
                        </label>
                        <select className="minimal-input max-w-xs">
                          <option value="daily">Daily at 2:00 AM</option>
                          <option value="weekly">Weekly on Sunday</option>
                          <option value="monthly">Monthly on 1st</option>
                        </select>
                      </div>
                    </div>
                  </div>

                  <div className="pt-6 border-t border-gray-200">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Manual Backup</h3>
                    <div className="space-y-4">
                      <p className="text-sm text-gray-600">
                        Create a manual backup of all system data including configurations, build history, and user data.
                      </p>
                      <button className="btn-primary">Create Backup Now</button>
                    </div>
                  </div>

                  <div className="pt-6 border-t border-gray-200">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Recent Backups</h3>
                    <div className="space-y-3">
                      <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">backup-2023-12-20.zip</div>
                          <div className="text-sm text-gray-500">December 20, 2023 • 45.2 MB</div>
                        </div>
                        <div className="flex space-x-2">
                          <button className="btn-secondary text-sm">Download</button>
                          <button className="btn-secondary text-sm">Restore</button>
                        </div>
                      </div>
                      <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                        <div>
                          <div className="text-sm font-medium text-gray-900">backup-2023-12-19.zip</div>
                          <div className="text-sm text-gray-500">December 19, 2023 • 43.8 MB</div>
                        </div>
                        <div className="flex space-x-2">
                          <button className="btn-secondary text-sm">Download</button>
                          <button className="btn-secondary text-sm">Restore</button>
                        </div>
                      </div>
                    </div>
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