import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, CheckIcon, KeyIcon, LockClosedIcon } from '@heroicons/react/24/outline';
import { Checkbox } from '../../components/Checkbox';
import type { AuthenticationSchemeType } from '../../types';
import { AuthenticationPolicyService } from '../../services/authenticationPolicyService';
import type { AuthenticationPolicyFormData } from './types';
import JwtBearerConfigForm from './JwtBearerConfigForm';
import OpenIdConnectConfigForm from './OpenIdConnectConfigForm';

const AuthenticationPolicyEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== 'new';

  const [formData, setFormData] = useState<AuthenticationPolicyFormData>({
    name: '',
    description: '',
    type: '',
    enabled: true,
    jwtAuthority: '',
    jwtAudience: '',
    jwtRequireHttpsMetadata: true,
    jwtSaveToken: false,
    jwtValidIssuers: [],
    jwtValidAudiences: [],
    jwtValidateIssuer: true,
    jwtValidateAudience: true,
    jwtValidateLifetime: true,
    jwtValidateIssuerSigningKey: true,
    jwtClockSkew: '',
    oidcAuthority: '',
    oidcClientId: '',
    oidcClientSecret: '',
    oidcResponseType: 'code',
    oidcRequireHttpsMetadata: true,
    oidcSaveTokens: true,
    oidcGetClaimsFromUserInfoEndpoint: true,
    oidcScope: [],
    oidcClockSkew: '',
  });

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadPolicy(id);
    }
  }, [isEdit, id]);

  const loadPolicy = async (policyId: string) => {
    try {
      setLoading(true);
      setError(null);
      const policy = await AuthenticationPolicyService.getAuthenticationPolicy(policyId);
      
      setFormData({
        name: policy.name || '',
        description: policy.description || '',
        type: policy.type,
        enabled: policy.enabled,
        jwtAuthority: policy.jwtBearer?.authority || '',
        jwtAudience: policy.jwtBearer?.audience || '',
        jwtRequireHttpsMetadata: policy.jwtBearer?.requireHttpsMetadata ?? true,
        jwtSaveToken: policy.jwtBearer?.saveToken || false,
        jwtValidIssuers: policy.jwtBearer?.validIssuers || [],
        jwtValidAudiences: policy.jwtBearer?.validAudiences || [],
        jwtValidateIssuer: policy.jwtBearer?.validateIssuer ?? true,
        jwtValidateAudience: policy.jwtBearer?.validateAudience ?? true,
        jwtValidateLifetime: policy.jwtBearer?.validateLifetime ?? true,
        jwtValidateIssuerSigningKey: policy.jwtBearer?.validateIssuerSigningKey ?? true,
        jwtClockSkew: policy.jwtBearer?.clockSkew?.toString() || '',
        oidcAuthority: policy.openIdConnect?.authority || '',
        oidcClientId: policy.openIdConnect?.clientId || '',
        oidcClientSecret: policy.openIdConnect?.clientSecret || '',
        oidcResponseType: policy.openIdConnect?.responseType || 'code',
        oidcRequireHttpsMetadata: policy.openIdConnect?.requireHttpsMetadata ?? true,
        oidcSaveTokens: policy.openIdConnect?.saveTokens ?? true,
        oidcGetClaimsFromUserInfoEndpoint: policy.openIdConnect?.getClaimsFromUserInfoEndpoint ?? true,
        oidcScope: policy.openIdConnect?.scope || [],
        oidcClockSkew: policy.openIdConnect?.clockSkew?.toString() || '',
      });
    } catch (err) {
      console.error('Failed to load authentication policy:', err);
      setError('Failed to load authentication policy');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.type) {
      setError('Please select an authentication type');
      return;
    }
    
    try {
      setSaving(true);
      setError(null);

      const request = {
        name: formData.name,
        description: formData.description || undefined,
        type: formData.type as AuthenticationSchemeType,
        enabled: formData.enabled,
        jwtBearer: formData.type === 'JwtBearer' ? {
          authority: formData.jwtAuthority,
          audience: formData.jwtAudience,
          requireHttpsMetadata: formData.jwtRequireHttpsMetadata,
          saveToken: formData.jwtSaveToken,
          validIssuers: formData.jwtValidIssuers.length > 0 ? formData.jwtValidIssuers : undefined,
          validAudiences: formData.jwtValidAudiences.length > 0 ? formData.jwtValidAudiences : undefined,
          validateIssuer: formData.jwtValidateIssuer,
          validateAudience: formData.jwtValidateAudience,
          validateLifetime: formData.jwtValidateLifetime,
          validateIssuerSigningKey: formData.jwtValidateIssuerSigningKey,
          clockSkew: formData.jwtClockSkew ? parseInt(formData.jwtClockSkew) : undefined,
        } : undefined,
        openIdConnect: formData.type === 'OpenIdConnect' ? {
          authority: formData.oidcAuthority,
          clientId: formData.oidcClientId,
          clientSecret: formData.oidcClientSecret,
          responseType: formData.oidcResponseType || undefined,
          requireHttpsMetadata: formData.oidcRequireHttpsMetadata,
          saveTokens: formData.oidcSaveTokens,
          getClaimsFromUserInfoEndpoint: formData.oidcGetClaimsFromUserInfoEndpoint,
          scope: formData.oidcScope.length > 0 ? formData.oidcScope : undefined,
          clockSkew: formData.oidcClockSkew ? parseInt(formData.oidcClockSkew) : undefined,
        } : undefined,
      };

      if (isEdit && id) {
        await AuthenticationPolicyService.updateAuthenticationPolicy(id, request);
      } else {
        await AuthenticationPolicyService.createAuthenticationPolicy(request);
      }

      navigate('/authentication-policies');
    } catch (err) {
      console.error('Failed to save authentication policy:', err);
      setError('Failed to save authentication policy');
    } finally {
      setSaving(false);
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
      <Link to="/authentication-policies" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6 transition-colors">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Authentication Policies
      </Link>

      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">
          {isEdit ? 'Edit Authentication Policy' : 'Create Authentication Policy'}
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          {isEdit ? 'Update your authentication policy configuration' : 'Configure JWT Bearer or OpenID Connect authentication'}
        </p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-700 font-medium">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Information */}
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-6">Basic Information</h2>
          
          <div className="space-y-5">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
                Policy Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                id="name"
                required
                value={formData.name}
                onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                placeholder="e.g., Production API Auth"
              />
            </div>

            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
                Description
              </label>
              <textarea
                id="description"
                rows={3}
                value={formData.description}
                onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                placeholder="Describe what this authentication policy is used for"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Type <span className="text-red-500">*</span>
              </label>
              {!isEdit ? (
                <div className="grid grid-cols-2 gap-3">
                  <button
                    type="button"
                    onClick={() => setFormData(prev => ({ ...prev, type: 'JwtBearer' }))}
                    className={`p-4 border-2 rounded-lg transition-all ${
                      formData.type === 'JwtBearer'
                        ? 'border-blue-500 bg-blue-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <KeyIcon className={`h-6 w-6 mx-auto mb-2 ${
                      formData.type === 'JwtBearer' ? 'text-blue-600' : 'text-gray-400'
                    }`} />
                    <div className="text-sm font-medium text-gray-900">JWT Bearer</div>
                    <div className="text-xs text-gray-500 mt-1">Token-based authentication</div>
                  </button>
                  <button
                    type="button"
                    onClick={() => setFormData(prev => ({ ...prev, type: 'OpenIdConnect' }))}
                    className={`p-4 border-2 rounded-lg transition-all ${
                      formData.type === 'OpenIdConnect'
                        ? 'border-indigo-500 bg-indigo-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <LockClosedIcon className={`h-6 w-6 mx-auto mb-2 ${
                      formData.type === 'OpenIdConnect' ? 'text-indigo-600' : 'text-gray-400'
                    }`} />
                    <div className="text-sm font-medium text-gray-900">OpenID Connect</div>
                    <div className="text-xs text-gray-500 mt-1">OAuth2/OIDC flow</div>
                  </button>
                </div>
              ) : (
                <div className="p-4 bg-gray-50 border border-gray-200 rounded-lg">
                  <div className="flex items-center gap-3">
                    {formData.type === 'JwtBearer' ? (
                      <>
                        <KeyIcon className="h-6 w-6 text-blue-600" />
                        <div>
                          <div className="text-sm font-medium text-gray-900">JWT Bearer</div>
                          <div className="text-xs text-gray-500">Token-based authentication</div>
                        </div>
                      </>
                    ) : (
                      <>
                        <LockClosedIcon className="h-6 w-6 text-indigo-600" />
                        <div>
                          <div className="text-sm font-medium text-gray-900">OpenID Connect</div>
                          <div className="text-xs text-gray-500">OAuth2/OIDC flow</div>
                        </div>
                      </>
                    )}
                  </div>
                </div>
              )}
            </div>

            <Checkbox
              checked={formData.enabled}
              onChange={(checked) => setFormData(prev => ({ ...prev, enabled: checked }))}
              label="Enable this policy"
            />
          </div>
        </div>

        {formData.type === 'JwtBearer' && (
          <JwtBearerConfigForm formData={formData} setFormData={setFormData} />
        )}

        {formData.type === 'OpenIdConnect' && (
          <OpenIdConnectConfigForm formData={formData} setFormData={setFormData} />
        )}

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-2">
          <Link
            to="/authentication-policies"
            className="btn-secondary"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={saving}
            className="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Saving...
              </>
            ) : (
              <>
                <CheckIcon className="h-4 w-4 mr-2" />
                {isEdit ? 'Update Policy' : 'Create Policy'}
              </>
            )}
          </button>
        </div>
      </form>

    </div>
  );
};

export default AuthenticationPolicyEdit;

