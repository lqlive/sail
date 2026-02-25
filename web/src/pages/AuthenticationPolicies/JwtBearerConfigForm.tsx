import React, { useState } from 'react';
import { PlusIcon, XMarkIcon, KeyIcon } from '@heroicons/react/24/outline';
import { Checkbox } from '../../components/Checkbox';
import type { AuthenticationPolicyFormData } from './types';

interface JwtBearerConfigFormProps {
  formData: AuthenticationPolicyFormData;
  setFormData: React.Dispatch<React.SetStateAction<AuthenticationPolicyFormData>>;
}

const JwtBearerConfigForm: React.FC<JwtBearerConfigFormProps> = ({ formData, setFormData }) => {
  const [validIssuerInput, setValidIssuerInput] = useState('');
  const [validAudienceInput, setValidAudienceInput] = useState('');

  const addValidIssuer = () => {
    if (validIssuerInput.trim() && !formData.jwtValidIssuers.includes(validIssuerInput.trim())) {
      setFormData(prev => ({
        ...prev,
        jwtValidIssuers: [...prev.jwtValidIssuers, validIssuerInput.trim()]
      }));
      setValidIssuerInput('');
    }
  };

  const removeValidIssuer = (issuer: string) => {
    setFormData(prev => ({
      ...prev,
      jwtValidIssuers: prev.jwtValidIssuers.filter(i => i !== issuer)
    }));
  };

  const addValidAudience = () => {
    if (validAudienceInput.trim() && !formData.jwtValidAudiences.includes(validAudienceInput.trim())) {
      setFormData(prev => ({
        ...prev,
        jwtValidAudiences: [...prev.jwtValidAudiences, validAudienceInput.trim()]
      }));
      setValidAudienceInput('');
    }
  };

  const removeValidAudience = (audience: string) => {
    setFormData(prev => ({
      ...prev,
      jwtValidAudiences: prev.jwtValidAudiences.filter(a => a !== audience)
    }));
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-6">
      <div className="flex items-center gap-2 mb-6">
        <KeyIcon className="h-5 w-5 text-blue-600" />
        <h2 className="text-base font-semibold text-gray-900">JWT Bearer Configuration</h2>
      </div>
      
      <div className="space-y-5">
        <div>
          <label htmlFor="jwtAuthority" className="block text-sm font-medium text-gray-700 mb-2">
            Authority <span className="text-red-500">*</span>
          </label>
          <input
            type="url"
            id="jwtAuthority"
            required
            value={formData.jwtAuthority}
            onChange={(e) => setFormData(prev => ({ ...prev, jwtAuthority: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="https://your-auth-server.com"
          />
        </div>

        <div>
          <label htmlFor="jwtAudience" className="block text-sm font-medium text-gray-700 mb-2">
            Audience <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            id="jwtAudience"
            required
            value={formData.jwtAudience}
            onChange={(e) => setFormData(prev => ({ ...prev, jwtAudience: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="your-api-audience"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Valid Issuers (Optional)
          </label>
          <div className="flex gap-2 mb-2">
            <input
              type="text"
              value={validIssuerInput}
              onChange={(e) => setValidIssuerInput(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addValidIssuer())}
              className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="https://issuer.example.com"
            />
            <button
              type="button"
              onClick={addValidIssuer}
              className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
            >
              <PlusIcon className="h-4 w-4" />
            </button>
          </div>
          {formData.jwtValidIssuers.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {formData.jwtValidIssuers.map((issuer, index) => (
                <span
                  key={index}
                  className="inline-flex items-center px-3 py-1 bg-blue-50 text-blue-700 text-sm rounded-lg"
                >
                  {issuer}
                  <button
                    type="button"
                    onClick={() => removeValidIssuer(issuer)}
                    className="ml-2 text-blue-500 hover:text-blue-700"
                  >
                    <XMarkIcon className="h-4 w-4" />
                  </button>
                </span>
              ))}
            </div>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Valid Audiences (Optional)
          </label>
          <div className="flex gap-2 mb-2">
            <input
              type="text"
              value={validAudienceInput}
              onChange={(e) => setValidAudienceInput(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addValidAudience())}
              className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="audience-value"
            />
            <button
              type="button"
              onClick={addValidAudience}
              className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
            >
              <PlusIcon className="h-4 w-4" />
            </button>
          </div>
          {formData.jwtValidAudiences.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {formData.jwtValidAudiences.map((audience, index) => (
                <span
                  key={index}
                  className="inline-flex items-center px-3 py-1 bg-blue-50 text-blue-700 text-sm rounded-lg"
                >
                  {audience}
                  <button
                    type="button"
                    onClick={() => removeValidAudience(audience)}
                    className="ml-2 text-blue-500 hover:text-blue-700"
                  >
                    <XMarkIcon className="h-4 w-4" />
                  </button>
                </span>
              ))}
            </div>
          )}
        </div>

        <div>
          <label htmlFor="jwtClockSkew" className="block text-sm font-medium text-gray-700 mb-2">
            Clock Skew (seconds)
          </label>
          <input
            type="number"
            id="jwtClockSkew"
            value={formData.jwtClockSkew}
            onChange={(e) => setFormData(prev => ({ ...prev, jwtClockSkew: e.target.value }))}
            className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="300"
          />
        </div>

        <div className="pt-4 border-t border-gray-100">
          <label className="block text-sm font-medium text-gray-700 mb-3">
            Validation Options
          </label>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <Checkbox
              checked={formData.jwtRequireHttpsMetadata}
              onChange={(checked) => setFormData(prev => ({ ...prev, jwtRequireHttpsMetadata: checked }))}
              label="Require HTTPS metadata"
            />
            <Checkbox
              checked={formData.jwtSaveToken}
              onChange={(checked) => setFormData(prev => ({ ...prev, jwtSaveToken: checked }))}
              label="Save token"
            />
            <Checkbox
              checked={formData.jwtValidateIssuer}
              onChange={(checked) => setFormData(prev => ({ ...prev, jwtValidateIssuer: checked }))}
              label="Validate issuer"
            />
            <Checkbox
              checked={formData.jwtValidateAudience}
              onChange={(checked) => setFormData(prev => ({ ...prev, jwtValidateAudience: checked }))}
              label="Validate audience"
            />
            <Checkbox
              checked={formData.jwtValidateLifetime}
              onChange={(checked) => setFormData(prev => ({ ...prev, jwtValidateLifetime: checked }))}
              label="Validate lifetime"
            />
            <Checkbox
              checked={formData.jwtValidateIssuerSigningKey}
              onChange={(checked) => setFormData(prev => ({ ...prev, jwtValidateIssuerSigningKey: checked }))}
              label="Validate signing key"
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default JwtBearerConfigForm;
