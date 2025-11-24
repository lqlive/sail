import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ChevronLeftIcon, CheckIcon, PlusIcon, XMarkIcon, ShieldCheckIcon, BoltIcon, ClockIcon, ArrowPathIcon } from '@heroicons/react/24/outline';
import type { MiddlewareType } from '../../types/gateway';
import { MiddlewareService } from '../../services/middlewareService';

const MiddlewareEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== 'new';

  const [formData, setFormData] = useState({
    name: '',
    description: '',
    type: '' as MiddlewareType | '',
    enabled: true,
    corsName: '',
    corsAllowOrigins: [] as string[],
    corsAllowMethods: [] as string[],
    corsAllowHeaders: [] as string[],
    corsExposeHeaders: [] as string[],
    corsAllowCredentials: false,
    corsMaxAge: '',
    rateLimiterName: '',
    rateLimiterPermitLimit: '',
    rateLimiterWindow: '',
    rateLimiterQueueLimit: '',
    timeoutName: '',
    timeoutSeconds: '',
    timeoutStatusCode: '',
    retryName: '',
    retryMaxRetryAttempts: '3',
    retryStatusCodes: [] as number[],
    retryDelayMilliseconds: '1000',
    retryUseExponentialBackoff: false,
  });

  const [originInput, setOriginInput] = useState('');
  const [methodInput, setMethodInput] = useState('');
  const [allowHeaderInput, setAllowHeaderInput] = useState('');
  const [exposeHeaderInput, setExposeHeaderInput] = useState('');
  const [statusCodeInput, setStatusCodeInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadMiddleware(id);
    }
  }, [isEdit, id]);

  const loadMiddleware = async (middlewareId: string) => {
    try {
      setLoading(true);
      setError(null);
      const middleware = await MiddlewareService.getMiddleware(middlewareId);
      
      setFormData({
        name: middleware.name || '',
        description: middleware.description || '',
        type: middleware.type,
        enabled: middleware.enabled,
        corsName: middleware.cors?.name || '',
        corsAllowOrigins: middleware.cors?.allowOrigins || [],
        corsAllowMethods: middleware.cors?.allowMethods || [],
        corsAllowHeaders: middleware.cors?.allowHeaders || [],
        corsExposeHeaders: middleware.cors?.exposeHeaders || [],
        corsAllowCredentials: middleware.cors?.allowCredentials || false,
        corsMaxAge: middleware.cors?.maxAge?.toString() || '',
        rateLimiterName: middleware.rateLimiter?.name || '',
        rateLimiterPermitLimit: middleware.rateLimiter?.permitLimit.toString() || '',
        rateLimiterWindow: middleware.rateLimiter?.window.toString() || '',
        rateLimiterQueueLimit: middleware.rateLimiter?.queueLimit.toString() || '',
        timeoutName: middleware.timeout?.name || '',
        timeoutSeconds: middleware.timeout?.seconds.toString() || '',
        timeoutStatusCode: middleware.timeout?.timeoutStatusCode?.toString() || '',
        retryName: middleware.retry?.name || '',
        retryMaxRetryAttempts: middleware.retry?.maxRetryAttempts.toString() || '3',
        retryStatusCodes: middleware.retry?.retryStatusCodes || [],
        retryDelayMilliseconds: middleware.retry?.retryDelayMilliseconds.toString() || '1000',
        retryUseExponentialBackoff: middleware.retry?.useExponentialBackoff || false,
      });
    } catch (err) {
      console.error('Failed to load middleware:', err);
      setError('Failed to load middleware');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.type) {
      setError('Please select a middleware type');
      return;
    }
    
    try {
      setSaving(true);
      setError(null);

      const request = {
        name: formData.name,
        description: formData.description || undefined,
        type: formData.type as MiddlewareType,
        enabled: formData.enabled,
        cors: formData.type === 'Cors' ? {
          name: formData.corsName,
          allowOrigins: formData.corsAllowOrigins.length > 0 ? formData.corsAllowOrigins : undefined,
          allowMethods: formData.corsAllowMethods.length > 0 ? formData.corsAllowMethods : undefined,
          allowHeaders: formData.corsAllowHeaders.length > 0 ? formData.corsAllowHeaders : undefined,
          exposeHeaders: formData.corsExposeHeaders.length > 0 ? formData.corsExposeHeaders : undefined,
          allowCredentials: formData.corsAllowCredentials,
          maxAge: formData.corsMaxAge ? parseInt(formData.corsMaxAge) : undefined,
        } : undefined,
        rateLimiter: formData.type === 'RateLimiter' ? {
          name: formData.rateLimiterName,
          permitLimit: parseInt(formData.rateLimiterPermitLimit) || 0,
          window: parseInt(formData.rateLimiterWindow) || 0,
          queueLimit: parseInt(formData.rateLimiterQueueLimit) || 0,
        } : undefined,
        timeout: formData.type === 'Timeout' ? {
          name: formData.timeoutName,
          seconds: parseInt(formData.timeoutSeconds) || 0,
          timeoutStatusCode: formData.timeoutStatusCode ? parseInt(formData.timeoutStatusCode) : undefined,
        } : undefined,
        retry: formData.type === 'Retry' ? {
          name: formData.retryName,
          maxRetryAttempts: parseInt(formData.retryMaxRetryAttempts) || 3,
          retryStatusCodes: formData.retryStatusCodes,
          retryDelayMilliseconds: parseInt(formData.retryDelayMilliseconds) || 1000,
          useExponentialBackoff: formData.retryUseExponentialBackoff,
        } : undefined,
      };

      if (isEdit && id) {
        await MiddlewareService.updateMiddleware(id, request);
      } else {
        await MiddlewareService.createMiddleware(request);
      }
      
      navigate('/middlewares');
    } catch (err) {
      console.error('Failed to save middleware:', err);
      setError('Failed to save middleware');
    } finally {
      setSaving(false);
    }
  };

  const addOrigin = () => {
    if (originInput && !formData.corsAllowOrigins.includes(originInput)) {
      setFormData({ ...formData, corsAllowOrigins: [...formData.corsAllowOrigins, originInput] });
      setOriginInput('');
    }
  };

  const removeOrigin = (origin: string) => {
    setFormData({ ...formData, corsAllowOrigins: formData.corsAllowOrigins.filter(o => o !== origin) });
  };

  const addMethod = () => {
    if (methodInput && !formData.corsAllowMethods.includes(methodInput.toUpperCase())) {
      setFormData({ ...formData, corsAllowMethods: [...formData.corsAllowMethods, methodInput.toUpperCase()] });
      setMethodInput('');
    }
  };

  const removeMethod = (method: string) => {
    setFormData({ ...formData, corsAllowMethods: formData.corsAllowMethods.filter(m => m !== method) });
  };

  const addAllowHeader = () => {
    if (allowHeaderInput && !formData.corsAllowHeaders.includes(allowHeaderInput)) {
      setFormData({ ...formData, corsAllowHeaders: [...formData.corsAllowHeaders, allowHeaderInput] });
      setAllowHeaderInput('');
    }
  };

  const removeAllowHeader = (header: string) => {
    setFormData({ ...formData, corsAllowHeaders: formData.corsAllowHeaders.filter(h => h !== header) });
  };

  const addExposeHeader = () => {
    if (exposeHeaderInput && !formData.corsExposeHeaders.includes(exposeHeaderInput)) {
      setFormData({ ...formData, corsExposeHeaders: [...formData.corsExposeHeaders, exposeHeaderInput] });
      setExposeHeaderInput('');
    }
  };

  const removeExposeHeader = (header: string) => {
    setFormData({ ...formData, corsExposeHeaders: formData.corsExposeHeaders.filter(h => h !== header) });
  };

  const addStatusCode = () => {
    const code = parseInt(statusCodeInput);
    if (code && code >= 400 && code <= 599 && !formData.retryStatusCodes.includes(code)) {
      setFormData({ ...formData, retryStatusCodes: [...formData.retryStatusCodes, code] });
      setStatusCodeInput('');
    }
  };

  const removeStatusCode = (code: number) => {
    setFormData({ ...formData, retryStatusCodes: formData.retryStatusCodes.filter(c => c !== code) });
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
      <Link to="/middlewares" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6 transition-colors">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Middlewares
      </Link>

      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">
          {isEdit ? 'Edit Middleware' : 'Create Middleware'}
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          Configure rate limiting, CORS, and timeout policies
        </p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-700 font-medium">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-6">Basic Information</h2>
          <div className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Middleware Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="e.g., Production API Policy"
                className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Description
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe what this middleware is used for"
                className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                rows={3}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Type <span className="text-red-500">*</span>
              </label>
              {!isEdit ? (
                <div className="grid grid-cols-4 gap-3">
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, type: 'Cors' })}
                    className={`p-4 border-2 rounded-lg transition-all ${
                      formData.type === 'Cors'
                        ? 'border-blue-500 bg-blue-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <ShieldCheckIcon className={`h-6 w-6 mx-auto mb-2 ${
                      formData.type === 'Cors' ? 'text-blue-600' : 'text-gray-400'
                    }`} />
                    <div className="text-sm font-medium text-gray-900">CORS</div>
                    <div className="text-xs text-gray-500 mt-1">Cross-Origin Resource Sharing</div>
                  </button>
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, type: 'RateLimiter' })}
                    className={`p-4 border-2 rounded-lg transition-all ${
                      formData.type === 'RateLimiter'
                        ? 'border-indigo-500 bg-indigo-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <BoltIcon className={`h-6 w-6 mx-auto mb-2 ${
                      formData.type === 'RateLimiter' ? 'text-indigo-600' : 'text-gray-400'
                    }`} />
                    <div className="text-sm font-medium text-gray-900">Rate Limiter</div>
                    <div className="text-xs text-gray-500 mt-1">Request rate limiting</div>
                  </button>
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, type: 'Timeout' })}
                    className={`p-4 border-2 rounded-lg transition-all ${
                      formData.type === 'Timeout'
                        ? 'border-orange-500 bg-orange-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <ClockIcon className={`h-6 w-6 mx-auto mb-2 ${
                      formData.type === 'Timeout' ? 'text-orange-600' : 'text-gray-400'
                    }`} />
                    <div className="text-sm font-medium text-gray-900">Timeout</div>
                    <div className="text-xs text-gray-500 mt-1">Request timeout policy</div>
                  </button>
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, type: 'Retry' })}
                    className={`p-4 border-2 rounded-lg transition-all ${
                      formData.type === 'Retry'
                        ? 'border-green-500 bg-green-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <ArrowPathIcon className={`h-6 w-6 mx-auto mb-2 ${
                      formData.type === 'Retry' ? 'text-green-600' : 'text-gray-400'
                    }`} />
                    <div className="text-sm font-medium text-gray-900">Retry</div>
                    <div className="text-xs text-gray-500 mt-1">Automatic retry policy</div>
                  </button>
                </div>
              ) : (
                <div className="flex items-center gap-2 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg">
                  {formData.type === 'Cors' ? (
                    <>
                      <ShieldCheckIcon className="h-5 w-5 text-blue-600" />
                      <span className="text-sm font-medium text-gray-900">CORS</span>
                    </>
                  ) : formData.type === 'RateLimiter' ? (
                    <>
                      <BoltIcon className="h-5 w-5 text-indigo-600" />
                      <span className="text-sm font-medium text-gray-900">Rate Limiter</span>
                    </>
                  ) : formData.type === 'Timeout' ? (
                    <>
                      <ClockIcon className="h-5 w-5 text-orange-600" />
                      <span className="text-sm font-medium text-gray-900">Timeout</span>
                    </>
                  ) : (
                    <>
                      <ArrowPathIcon className="h-5 w-5 text-green-600" />
                      <span className="text-sm font-medium text-gray-900">Retry</span>
                    </>
                  )}
                  <span className="text-xs text-gray-500 ml-auto">Cannot change type when editing</span>
                </div>
              )}
            </div>

            <div>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={formData.enabled}
                  onChange={(e) => setFormData({ ...formData, enabled: e.target.checked })}
                  className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
                />
                <span className="text-sm font-medium text-gray-700">Enabled</span>
              </label>
              <p className="text-xs text-gray-500 mt-1 ml-6">
                {formData.enabled ? 'This middleware is active' : 'This middleware is disabled'}
              </p>
            </div>
          </div>
        </div>

        {formData.type === 'Cors' && (
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <h2 className="text-base font-semibold text-gray-900 mb-6">CORS Configuration</h2>
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Policy Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.corsName}
                  onChange={(e) => setFormData({ ...formData, corsName: e.target.value })}
                  placeholder="e.g., default"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  required={formData.type === 'Cors'}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Allowed Origins
                </label>
                <div className="flex gap-2 mb-3">
                  <input
                    type="text"
                    value={originInput}
                    onChange={(e) => setOriginInput(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addOrigin())}
                    placeholder="https://example.com"
                    className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                  <button
                    type="button"
                    onClick={addOrigin}
                    className="inline-flex items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                  >
                    <PlusIcon className="h-4 w-4 mr-2" />
                    Add
                  </button>
                </div>
                {formData.corsAllowOrigins.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {formData.corsAllowOrigins.map((origin) => (
                      <span
                        key={origin}
                        className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-700"
                      >
                        {origin}
                        <button
                          type="button"
                          onClick={() => removeOrigin(origin)}
                          className="text-gray-500 hover:text-gray-700"
                        >
                          <XMarkIcon className="h-3.5 w-3.5" />
                        </button>
                      </span>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Allowed Methods
                </label>
                <div className="flex gap-2 mb-3">
                  <input
                    type="text"
                    value={methodInput}
                    onChange={(e) => setMethodInput(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addMethod())}
                    placeholder="GET, POST, PUT"
                    className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                  <button
                    type="button"
                    onClick={addMethod}
                    className="inline-flex items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                  >
                    <PlusIcon className="h-4 w-4 mr-2" />
                    Add
                  </button>
                </div>
                {formData.corsAllowMethods.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {formData.corsAllowMethods.map((method) => (
                      <span
                        key={method}
                        className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-700"
                      >
                        {method}
                        <button
                          type="button"
                          onClick={() => removeMethod(method)}
                          className="text-gray-500 hover:text-gray-700"
                        >
                          <XMarkIcon className="h-3.5 w-3.5" />
                        </button>
                      </span>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Allowed Headers
                </label>
                <div className="flex gap-2 mb-3">
                  <input
                    type="text"
                    value={allowHeaderInput}
                    onChange={(e) => setAllowHeaderInput(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addAllowHeader())}
                    placeholder="Content-Type, Authorization"
                    className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                  <button
                    type="button"
                    onClick={addAllowHeader}
                    className="inline-flex items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                  >
                    <PlusIcon className="h-4 w-4 mr-2" />
                    Add
                  </button>
                </div>
                {formData.corsAllowHeaders.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {formData.corsAllowHeaders.map((header) => (
                      <span
                        key={header}
                        className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-700"
                      >
                        {header}
                        <button
                          type="button"
                          onClick={() => removeAllowHeader(header)}
                          className="text-gray-500 hover:text-gray-700"
                        >
                          <XMarkIcon className="h-3.5 w-3.5" />
                        </button>
                      </span>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Expose Headers
                </label>
                <div className="flex gap-2 mb-3">
                  <input
                    type="text"
                    value={exposeHeaderInput}
                    onChange={(e) => setExposeHeaderInput(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addExposeHeader())}
                    placeholder="X-Custom-Header"
                    className="block flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                  <button
                    type="button"
                    onClick={addExposeHeader}
                    className="inline-flex items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                  >
                    <PlusIcon className="h-4 w-4 mr-2" />
                    Add
                  </button>
                </div>
                {formData.corsExposeHeaders.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {formData.corsExposeHeaders.map((header) => (
                      <span
                        key={header}
                        className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-700"
                      >
                        {header}
                        <button
                          type="button"
                          onClick={() => removeExposeHeader(header)}
                          className="text-gray-500 hover:text-gray-700"
                        >
                          <XMarkIcon className="h-3.5 w-3.5" />
                        </button>
                      </span>
                    ))}
                  </div>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={formData.corsAllowCredentials}
                      onChange={(e) => setFormData({ ...formData, corsAllowCredentials: e.target.checked })}
                      className="h-4 w-4 text-gray-900 focus:ring-gray-900 border-gray-300 rounded"
                    />
                    <span className="text-sm font-medium text-gray-700">Allow Credentials</span>
                  </label>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Max Age (seconds)
                  </label>
                  <input
                    type="number"
                    value={formData.corsMaxAge}
                    onChange={(e) => setFormData({ ...formData, corsMaxAge: e.target.value })}
                    placeholder="3600"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                </div>
              </div>
            </div>
          </div>
        )}

        {formData.type === 'RateLimiter' && (
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <h2 className="text-base font-semibold text-gray-900 mb-6">Rate Limiter Configuration</h2>
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Policy Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.rateLimiterName}
                  onChange={(e) => setFormData({ ...formData, rateLimiterName: e.target.value })}
                  placeholder="e.g., default"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  required={formData.type === 'RateLimiter'}
                />
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Permit Limit <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number"
                    value={formData.rateLimiterPermitLimit}
                    onChange={(e) => setFormData({ ...formData, rateLimiterPermitLimit: e.target.value })}
                  placeholder="100"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  required={formData.type === 'RateLimiter'}
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Window (seconds) <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number"
                    value={formData.rateLimiterWindow}
                    onChange={(e) => setFormData({ ...formData, rateLimiterWindow: e.target.value })}
                  placeholder="60"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  required={formData.type === 'RateLimiter'}
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Queue Limit <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number"
                    value={formData.rateLimiterQueueLimit}
                    onChange={(e) => setFormData({ ...formData, rateLimiterQueueLimit: e.target.value })}
                  placeholder="0"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  required={formData.type === 'RateLimiter'}
                  />
                </div>
              </div>
            </div>
          </div>
        )}

        {formData.type === 'Timeout' && (
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <h2 className="text-base font-semibold text-gray-900 mb-6">Timeout Configuration</h2>
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Policy Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.timeoutName}
                  onChange={(e) => setFormData({ ...formData, timeoutName: e.target.value })}
                  placeholder="e.g., default"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  required={formData.type === 'Timeout'}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Timeout (seconds) <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number"
                    value={formData.timeoutSeconds}
                    onChange={(e) => setFormData({ ...formData, timeoutSeconds: e.target.value })}
                    placeholder="30"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                    required={formData.type === 'Timeout'}
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Status Code
                  </label>
                  <input
                    type="number"
                    value={formData.timeoutStatusCode}
                    onChange={(e) => setFormData({ ...formData, timeoutStatusCode: e.target.value })}
                    placeholder="504"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                  <p className="text-xs text-gray-500 mt-1">Default is 504 (Gateway Timeout)</p>
                </div>
              </div>
            </div>
          </div>
        )}

        {formData.type === 'Retry' && (
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <h2 className="text-base font-semibold text-gray-900 mb-6">Retry Configuration</h2>
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Policy Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.retryName}
                  onChange={(e) => setFormData({ ...formData, retryName: e.target.value })}
                  placeholder="e.g., api-retry-policy"
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  required={formData.type === 'Retry'}
                />
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Max Retry Attempts <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number"
                    min="1"
                    max="10"
                    value={formData.retryMaxRetryAttempts}
                    onChange={(e) => setFormData({ ...formData, retryMaxRetryAttempts: e.target.value })}
                    placeholder="3"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                    required={formData.type === 'Retry'}
                  />
                  <p className="text-xs text-gray-500 mt-1">Number of retry attempts (1-10)</p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Retry Delay (ms) <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number"
                    min="100"
                    value={formData.retryDelayMilliseconds}
                    onChange={(e) => setFormData({ ...formData, retryDelayMilliseconds: e.target.value })}
                    placeholder="1000"
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                    required={formData.type === 'Retry'}
                  />
                  <p className="text-xs text-gray-500 mt-1">Delay between retries</p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Backoff Strategy
                  </label>
                  <select
                    value={formData.retryUseExponentialBackoff ? 'exponential' : 'linear'}
                    onChange={(e) => setFormData({ ...formData, retryUseExponentialBackoff: e.target.value === 'exponential' })}
                    className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  >
                    <option value="linear">Linear</option>
                    <option value="exponential">Exponential</option>
                  </select>
                  <p className="text-xs text-gray-500 mt-1">Delay calculation method</p>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Retry Status Codes <span className="text-red-500">*</span>
                </label>
                <p className="text-xs text-gray-500 mb-2">Add HTTP status codes that should trigger a retry (e.g., 500, 502, 503, 504)</p>
                <div className="flex gap-2 mb-3">
                  <input
                    type="number"
                    min="400"
                    max="599"
                    value={statusCodeInput}
                    onChange={(e) => setStatusCodeInput(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addStatusCode())}
                    placeholder="500"
                    className="block w-32 px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  />
                  <button
                    type="button"
                    onClick={addStatusCode}
                    className="inline-flex items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                  >
                    <PlusIcon className="h-4 w-4 mr-2" />
                    Add
                  </button>
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, retryStatusCodes: [500, 502, 503, 504] })}
                    className="inline-flex items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                  >
                    Use Common Codes
                  </button>
                </div>
                {formData.retryStatusCodes.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {formData.retryStatusCodes.sort((a, b) => a - b).map((code) => (
                      <span
                        key={code}
                        className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-700"
                      >
                        {code}
                        <button
                          type="button"
                          onClick={() => removeStatusCode(code)}
                          className="text-gray-500 hover:text-gray-700"
                        >
                          <XMarkIcon className="h-3.5 w-3.5" />
                        </button>
                      </span>
                    ))}
                  </div>
                )}
                {formData.retryStatusCodes.length === 0 && (
                  <p className="text-sm text-red-600">At least one status code is required</p>
                )}
              </div>
            </div>
          </div>
        )}

        <div className="flex justify-end gap-3 pt-2">
          <Link
            to="/middlewares"
            className="px-5 py-2.5 border border-gray-200 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={saving}
            className="inline-flex items-center px-5 py-2.5 border border-transparent rounded-lg text-sm font-medium text-white bg-black hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {saving ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Saving...
              </>
            ) : (
              <>
                <CheckIcon className="h-4 w-4 mr-2" />
                {isEdit ? 'Update Middleware' : 'Create Middleware'}
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default MiddlewareEdit;
