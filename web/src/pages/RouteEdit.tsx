import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Save, X } from 'lucide-react'

interface RouteFormData {
  name: string
  path: string
  methods: string[]
  cluster: string
  priority: number
  timeout: number
  retries: number
  headers: { key: string; value: string }[]
  queryParameters: { key: string; value: string }[]
  status: 'active' | 'inactive'
}

const HTTP_METHODS = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS', 'HEAD']

export default function RouteEdit() {
  const navigate = useNavigate()
  const { id } = useParams()
  const isEdit = !!id

  const [formData, setFormData] = useState<RouteFormData>({
    name: '',
    path: '',
    methods: ['GET'],
    cluster: '',
    priority: 0,
    timeout: 30,
    retries: 3,
    headers: [],
    queryParameters: [],
    status: 'active',
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    console.log('Form submitted:', formData)
    // TODO: API call to save route
    navigate('/routes')
  }

  const handleMethodToggle = (method: string) => {
    setFormData((prev) => ({
      ...prev,
      methods: prev.methods.includes(method)
        ? prev.methods.filter((m) => m !== method)
        : [...prev.methods, method],
    }))
  }

  const addHeader = () => {
    setFormData((prev) => ({
      ...prev,
      headers: [...prev.headers, { key: '', value: '' }],
    }))
  }

  const removeHeader = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      headers: prev.headers.filter((_, i) => i !== index),
    }))
  }

  const updateHeader = (index: number, field: 'key' | 'value', value: string) => {
    setFormData((prev) => ({
      ...prev,
      headers: prev.headers.map((header, i) =>
        i === index ? { ...header, [field]: value } : header
      ),
    }))
  }

  const addQueryParameter = () => {
    setFormData((prev) => ({
      ...prev,
      queryParameters: [...prev.queryParameters, { key: '', value: '' }],
    }))
  }

  const removeQueryParameter = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      queryParameters: prev.queryParameters.filter((_, i) => i !== index),
    }))
  }

  const updateQueryParameter = (index: number, field: 'key' | 'value', value: string) => {
    setFormData((prev) => ({
      ...prev,
      queryParameters: prev.queryParameters.map((param, i) =>
        i === index ? { ...param, [field]: value } : param
      ),
    }))
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <button
            onClick={() => navigate('/routes')}
            className="p-1.5 rounded-lg hover:bg-gray-100 transition-all"
          >
            <ArrowLeft className="h-4 w-4 text-gray-600" />
          </button>
          <div>
            <h1 className="text-xl font-bold text-gray-900">
              {isEdit ? 'Edit Route' : 'Create New Route'}
            </h1>
            <p className="mt-0.5 text-xs text-gray-600">
              Configure route settings and routing rules
            </p>
          </div>
        </div>
        <div className="flex space-x-2">
          <button
            onClick={() => navigate('/routes')}
            className="px-4 py-2 text-sm bg-white text-gray-700 hover:bg-gray-50 border border-gray-300 hover:border-gray-400 rounded-lg font-medium transition-all"
          >
            Cancel
          </button>
          <button onClick={handleSubmit} className="px-4 py-2 text-sm bg-slate-800 text-white hover:bg-slate-900 rounded-lg font-medium flex items-center transition-all">
            <Save className="h-3.5 w-3.5 mr-1.5" />
            Save Route
          </button>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        {/* Basic Information */}
        <div className="card p-4">
          <h2 className="text-base font-semibold text-gray-900 mb-3">Basic Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Route Name *
              </label>
              <input
                type="text"
                required
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
                placeholder="e.g., API Gateway"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Path Pattern *
              </label>
              <input
                type="text"
                required
                value={formData.path}
                onChange={(e) => setFormData({ ...formData, path: e.target.value })}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
                placeholder="e.g., /api/*"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Target Cluster *
              </label>
              <select
                required
                value={formData.cluster}
                onChange={(e) => setFormData({ ...formData, cluster: e.target.value })}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
              >
                <option value="">Select a cluster</option>
                <option value="backend-cluster">backend-cluster</option>
                <option value="auth-cluster">auth-cluster</option>
                <option value="admin-cluster">admin-cluster</option>
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Status
              </label>
              <select
                value={formData.status}
                onChange={(e) =>
                  setFormData({ ...formData, status: e.target.value as 'active' | 'inactive' })
                }
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
              >
                <option value="active">Active</option>
                <option value="inactive">Inactive</option>
              </select>
            </div>
          </div>
        </div>

        {/* HTTP Methods */}
        <div className="card p-4">
          <h2 className="text-base font-semibold text-gray-900 mb-3">HTTP Methods</h2>
          <div className="flex flex-wrap gap-2">
            {HTTP_METHODS.map((method) => (
              <button
                key={method}
                type="button"
                onClick={() => handleMethodToggle(method)}
                className={`px-3 py-1.5 text-sm rounded-lg font-medium transition-all ${
                  formData.methods.includes(method)
                    ? 'bg-slate-800 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                {method}
              </button>
            ))}
          </div>
        </div>

        {/* Advanced Settings */}
        <div className="card p-4">
          <h2 className="text-base font-semibold text-gray-900 mb-3">Advanced Settings</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Priority
              </label>
              <input
                type="number"
                value={formData.priority}
                onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
                min="0"
              />
              <p className="text-xs text-gray-500 mt-1">Higher priority routes match first</p>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Timeout (seconds)
              </label>
              <input
                type="number"
                value={formData.timeout}
                onChange={(e) => setFormData({ ...formData, timeout: Number(e.target.value) })}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
                min="1"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Max Retries
              </label>
              <input
                type="number"
                value={formData.retries}
                onChange={(e) => setFormData({ ...formData, retries: Number(e.target.value) })}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
                min="0"
                max="10"
              />
            </div>
          </div>
        </div>

        {/* Headers */}
        <div className="card p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-base font-semibold text-gray-900">Request Headers</h2>
            <button
              type="button"
              onClick={addHeader}
              className="text-xs text-slate-700 hover:text-slate-900 font-medium"
            >
              + Add Header
            </button>
          </div>
          {formData.headers.length === 0 ? (
            <p className="text-xs text-gray-500">No headers configured</p>
          ) : (
            <div className="space-y-2">
              {formData.headers.map((header, index) => (
                <div key={index} className="flex gap-2">
                  <input
                    type="text"
                    value={header.key}
                    onChange={(e) => updateHeader(index, 'key', e.target.value)}
                    placeholder="Header name"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500 flex-1"
                  />
                  <input
                    type="text"
                    value={header.value}
                    onChange={(e) => updateHeader(index, 'value', e.target.value)}
                    placeholder="Header value"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500 flex-1"
                  />
                  <button
                    type="button"
                    onClick={() => removeHeader(index)}
                    className="p-2 rounded-lg hover:bg-gray-100 text-red-600"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Query Parameters */}
        <div className="card p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-base font-semibold text-gray-900">Query Parameters</h2>
            <button
              type="button"
              onClick={addQueryParameter}
              className="text-xs text-slate-700 hover:text-slate-900 font-medium"
            >
              + Add Parameter
            </button>
          </div>
          {formData.queryParameters.length === 0 ? (
            <p className="text-xs text-gray-500">No query parameters configured</p>
          ) : (
            <div className="space-y-2">
              {formData.queryParameters.map((param, index) => (
                <div key={index} className="flex gap-2">
                  <input
                    type="text"
                    value={param.key}
                    onChange={(e) => updateQueryParameter(index, 'key', e.target.value)}
                    placeholder="Parameter name"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500 flex-1"
                  />
                  <input
                    type="text"
                    value={param.value}
                    onChange={(e) => updateQueryParameter(index, 'value', e.target.value)}
                    placeholder="Parameter value"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500 flex-1"
                  />
                  <button
                    type="button"
                    onClick={() => removeQueryParameter(index)}
                    className="p-2 rounded-lg hover:bg-gray-100 text-red-600"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </form>
    </div>
  )
}

