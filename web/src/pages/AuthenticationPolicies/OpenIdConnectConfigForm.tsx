import React, { useState } from 'react';
import { PlusIcon, XMarkIcon, LockClosedIcon } from '@heroicons/react/24/outline';
import { Checkbox } from '../../components/Checkbox';
import type { AuthenticationPolicyFormData } from './types';

interface OpenIdConnectConfigFormProps {
  formData: AuthenticationPolicyFormData;
  setFormData: React.Dispatch<React.SetStateAction<AuthenticationPolicyFormData>>;
}

const OpenIdConnectConfigForm: React.FC<OpenIdConnectConfigFormProps> = ({ formData, setFormData }) => {
  const [scopeInput, setScopeInput] = useState('');

  const addScope = () => {
    if (scopeInput.trim() && !formData.oidcScope.includes(scopeInput.trim())) {
      setFormData(prev => ({
        ...prev,
        oidcScope: [...prev.oidcScope, scopeInput.trim()]
      }));
      setScopeInput('');
    }
  };

  const removeScope = (scope: string) => {
    setFormData(prev => ({
      ...prev,
      oidcScope: prev.oidcScope.filter(s => s !== scope)
    }));
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-6">
      <div className="flex items-center gap-2 mb-6">
        <LockClosedIcon className="h-5 w-5 text-indigo-600" />
        <h2 className="text-base font-semibold text-gray-900">OpenID Connect Configuration</h2>
      </div>
      
      <div className="space-y-5">
        <div>
          <label htmlFor="oidcAuthority" className="block text-sm font-medium text-gray-700 mb-2">
            Authority <span className="text-red-500">*</span>
          </label>
          <input
            type="url"
            id="oidcAuthority"
            required
            value={formData.oidcAuthority}
            onChange={(e) => setFormData(prev => ({ ...prev, oidcAuthority: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="https://your-identity-provider.com"
          />
        </div>

        <div>
          <label htmlFor="oidcClientId" className="block text-sm font-medium text-gray-700 mb-2">
            Client ID <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            id="oidcClientId"
            required
            value={formData.oidcClientId}
            onChange={(e) => setFormData(prev => ({ ...prev, oidcClientId: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="your-client-id"
          />
        </div>

        <div>
          <label htmlFor="oidcClientSecret" className="block text-sm font-medium text-gray-700 mb-2">
            Client Secret <span className="text-red-500">*</span>
          </label>
          <input
            type="password"
            id="oidcClientSecret"
            required
            value={formData.oidcClientSecret}
            onChange={(e) => setFormData(prev => ({ ...prev, oidcClientSecret: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="your-client-secret"
          />
        </div>

        <div>
          <label htmlFor="oidcResponseType" className="block text-sm font-medium text-gray-700 mb-2">
            Response Type
          </label>
          <select
            id="oidcResponseType"
            value={formData.oidcResponseType}
            onChange={(e) => setFormData(prev => ({ ...prev, oidcResponseType: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
          >
            <option value="code">code</option>
            <option value="id_token">id_token</option>
            <option value="id_token token">id_token token</option>
            <option value="code id_token">code id_token</option>
            <option value="code token">code token</option>
            <option value="code id_token token">code id_token token</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Scopes
          </label>
          <div className="flex gap-2 mb-2">
            <input
              type="text"
              value={scopeInput}
              onChange={(e) => setScopeInput(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addScope())}
              className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="openid, profile, email"
            />
            <button
              type="button"
              onClick={addScope}
              className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
            >
              <PlusIcon className="h-4 w-4" />
            </button>
          </div>
          {formData.oidcScope.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {formData.oidcScope.map((scope, index) => (
                <span
                  key={index}
                  className="inline-flex items-center px-3 py-1 bg-indigo-50 text-indigo-700 text-sm rounded-lg"
                >
                  {scope}
                  <button
                    type="button"
                    onClick={() => removeScope(scope)}
                    className="ml-2 text-indigo-500 hover:text-indigo-700"
                  >
                    <XMarkIcon className="h-4 w-4" />
                  </button>
                </span>
              ))}
            </div>
          )}
        </div>

        <div>
          <label htmlFor="oidcClockSkew" className="block text-sm font-medium text-gray-700 mb-2">
            Clock Skew (seconds)
          </label>
          <input
            type="number"
            id="oidcClockSkew"
            value={formData.oidcClockSkew}
            onChange={(e) => setFormData(prev => ({ ...prev, oidcClockSkew: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="300"
          />
        </div>

        <div className="pt-4 border-t border-gray-100">
          <label className="block text-sm font-medium text-gray-700 mb-3">
            Additional Options
          </label>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <Checkbox
              checked={formData.oidcRequireHttpsMetadata}
              onChange={(checked) => setFormData(prev => ({ ...prev, oidcRequireHttpsMetadata: checked }))}
              label="Require HTTPS metadata"
            />
            <Checkbox
              checked={formData.oidcSaveTokens}
              onChange={(checked) => setFormData(prev => ({ ...prev, oidcSaveTokens: checked }))}
              label="Save tokens"
            />
            <div className="md:col-span-2">
              <Checkbox
                checked={formData.oidcGetClaimsFromUserInfoEndpoint}
                onChange={(checked) => setFormData(prev => ({ ...prev, oidcGetClaimsFromUserInfoEndpoint: checked }))}
                label="Get claims from user info endpoint"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OpenIdConnectConfigForm;
