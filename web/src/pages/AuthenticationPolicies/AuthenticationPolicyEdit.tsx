import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, CheckIcon, PlusIcon, XMarkIcon, KeyIcon, LockClosedIcon } from '@heroicons/react/24/outline';
import { Checkbox } from '../../components/Checkbox';
import type { AuthenticationSchemeType } from '../../types';
import { AuthenticationPolicyService } from '../../services/authenticationPolicyService';

const AuthenticationPolicyEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== 'new';

  const [formData, setFormData] = useState({
    name: '',
    description: '',
    type: '' as AuthenticationSchemeType | '',
    enabled: true,
    // JWT Bearer fields
    jwtAuthority: '',
    jwtAudience: '',
    jwtRequireHttpsMetadata: true,
    jwtSaveToken: false,
    jwtValidIssuers: [] as string[],
    jwtValidAudiences: [] as string[],
    jwtValidateIssuer: true,
    jwtValidateAudience: true,
    jwtValidateLifetime: true,
    jwtValidateIssuerSigningKey: true,
    jwtClockSkew: '',
    // OpenID Connect fields
    oidcAuthority: '',
    oidcClientId: '',
    oidcClientSecret: '',
    oidcResponseType: 'code',
    oidcRequireHttpsMetadata: true,
    oidcSaveTokens: true,
    oidcGetClaimsFromUserInfoEndpoint: true,
    oidcScope: [] as string[],
    oidcClockSkew: '',
  });

  const [validIssuerInput, setValidIssuerInput] = useState('');
  const [validAudienceInput, setValidAudienceInput] = useState('');
  const [scopeInput, setScopeInput] = useState('');
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

  const handleDelete = async () => {
    if (!id || !confirm('Are you sure you want to delete this authentication policy? This action cannot be undone.')) {
      return;
    }

    try {
      setSaving(true);
      setError(null);
      await AuthenticationPolicyService.deleteAuthenticationPolicy(id);
      navigate('/authentication-policies');
    } catch (err) {
      console.error('Failed to delete authentication policy:', err);
      setError('Failed to delete authentication policy');
      setSaving(false);
    }
  };

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

        {/* JWT Bearer Configuration */}
        {formData.type === 'JwtBearer' && (
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
        )}

        {/* OpenID Connect Configuration */}
        {formData.type === 'OpenIdConnect' && (
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

      {/* Delete section for edit mode */}
      {isEdit && (
        <div className="mt-8 pt-8 border-t border-gray-200">
          <div className="bg-red-50 rounded-lg p-6">
            <h3 className="text-sm font-semibold text-red-900 mb-2">Delete Authentication Policy</h3>
            <p className="text-sm text-red-700 mb-4">
              Once you delete this policy, it cannot be recovered. Please be certain.
            </p>
            <button
              type="button"
              onClick={handleDelete}
              disabled={saving}
              className="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {saving ? 'Deleting...' : 'Delete Policy'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default AuthenticationPolicyEdit;

