import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  KeyIcon,
  LockClosedIcon,
  FunnelIcon,
  TrashIcon,
} from '@heroicons/react/24/outline';
import type { AuthenticationPolicy, AuthenticationSchemeType } from '../../types';
import { AuthenticationPolicyService } from '../../services/authenticationPolicyService';

const AuthenticationPolicies: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [policies, setPolicies] = useState<AuthenticationPolicy[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState<'all' | AuthenticationSchemeType>('all');
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<{ id: string; name: string } | null>(null);

  const loadPolicies = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await AuthenticationPolicyService.getAuthenticationPolicies(searchTerm);
      setPolicies(data);
    } catch (err) {
      console.error('Failed to load authentication policies:', err);
      setError('Failed to load authentication policies');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPolicies();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== '') {
        loadPolicies();
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const filteredPolicies = policies.filter(policy => {
    if (filterType !== 'all' && policy.type !== filterType) {
      return false;
    }
    return true;
  });

  const handleDelete = async (id: string) => {
    try {
      setDeletingId(id);
      await AuthenticationPolicyService.deleteAuthenticationPolicy(id);
      await loadPolicies();
      setDeleteConfirm(null);
    } catch (err) {
      console.error('Failed to delete authentication policy:', err);
      setError('Failed to delete authentication policy');
    } finally {
      setDeletingId(null);
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
    <div className="fade-in">
      <div className="mb-8">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">Authentication Policies</h1>
            <p className="mt-1 text-sm text-gray-500">Configure JWT Bearer and OpenID Connect authentication</p>
          </div>
          <Link 
            to="/authentication-policies/new" 
            className="btn-primary"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Policy
          </Link>
        </div>

        <div className="flex flex-col sm:flex-row gap-4 sm:items-center sm:justify-between">
          <div className="relative flex-1 max-w-md">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="Search policies..."
            />
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={() => setFilterType('all')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                filterType === 'all'
                  ? 'bg-gray-900 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              All
            </button>
            <button
              onClick={() => setFilterType('JwtBearer')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                filterType === 'JwtBearer'
                  ? 'bg-blue-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              <KeyIcon className="h-4 w-4 mr-1.5" />
              JWT Bearer
            </button>
            <button
              onClick={() => setFilterType('OpenIdConnect')}
              className={`inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                filterType === 'OpenIdConnect'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'
              }`}
            >
              <LockClosedIcon className="h-4 w-4 mr-1.5" />
              OpenID Connect
            </button>
          </div>
        </div>

        <div className="flex items-center text-sm text-gray-500 font-medium mt-4">
          <FunnelIcon className="h-4 w-4 mr-1.5" />
          {filteredPolicies.length} {filteredPolicies.length === 1 ? 'policy' : 'policies'}
        </div>
      </div>

      {error ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="h-6 w-6 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">Failed to load authentication policies</h3>
          <p className="text-sm text-gray-500 mb-4">Please check your connection and try again</p>
          <button
            onClick={loadPolicies}
            className="inline-flex items-center px-4 py-2 bg-white border border-gray-200 text-sm font-medium rounded-lg hover:bg-gray-50 transition-colors"
          >
            Try Again
          </button>
        </div>
      ) : policies.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <KeyIcon className="h-6 w-6 text-gray-400" />
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">No authentication policies yet</h3>
          <p className="text-sm text-gray-500 mb-4">Get started by creating your first authentication policy</p>
          <Link
            to="/authentication-policies/new"
            className="btn-primary"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Policy
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4 gap-4 max-w-7xl">
          {filteredPolicies.map((policy) => (
            <div
              key={policy.id}
              className="bg-white border border-gray-200 rounded-lg hover:border-gray-300 hover:shadow-sm transition-all relative"
            >
              <button
                onClick={(e) => {
                  e.preventDefault();
                  setDeleteConfirm({ id: policy.id, name: policy.name });
                }}
                className="absolute top-3 right-3 p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-md transition-colors z-10"
                title="Delete policy"
              >
                <TrashIcon className="h-4 w-4" />
              </button>

              <Link
                to={`/authentication-policies/${policy.id}`}
                className="block p-4"
              >
              <div className="flex items-center gap-3 mb-3">
                <div className={`w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 ${
                  policy.type === 'JwtBearer' ? 'bg-blue-50' : 'bg-indigo-50'
                }`}>
                  {policy.type === 'JwtBearer' ? (
                    <KeyIcon className="h-5 w-5 text-blue-600" />
                  ) : (
                    <LockClosedIcon className="h-5 w-5 text-indigo-600" />
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-sm font-medium text-gray-900 truncate">{policy.name}</h3>
                  <div className="flex items-center gap-1.5 mt-1 flex-wrap">
                    <span className={`inline-flex items-center px-2 py-0.5 text-xs font-medium rounded ${
                      policy.type === 'JwtBearer'
                        ? 'bg-blue-50 text-blue-700'
                        : 'bg-indigo-50 text-indigo-700'
                    }`}>
                      {policy.type === 'JwtBearer' ? 'JWT Bearer' : 'OpenID Connect'}
                    </span>
                    {policy.enabled ? (
                      <span className="text-xs text-green-600 font-medium">● Enabled</span>
                    ) : (
                      <span className="text-xs text-gray-400">○ Disabled</span>
                    )}
                  </div>
                </div>
              </div>

              <div className="space-y-2">
                {policy.jwtBearer && (
                  <>
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <span className="text-xs text-gray-600">Authority</span>
                      <span className="text-xs font-medium text-gray-900 truncate ml-2 max-w-[150px]">{policy.jwtBearer.authority}</span>
                    </div>
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <span className="text-xs text-gray-600">Audience</span>
                      <span className="text-xs font-medium text-gray-900 truncate ml-2 max-w-[150px]">{policy.jwtBearer.audience}</span>
                    </div>
                  </>
                )}

                {policy.openIdConnect && (
                  <>
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <span className="text-xs text-gray-600">Authority</span>
                      <span className="text-xs font-medium text-gray-900 truncate ml-2 max-w-[150px]">{policy.openIdConnect.authority}</span>
                    </div>
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <span className="text-xs text-gray-600">Client ID</span>
                      <span className="text-xs font-medium text-gray-900 truncate ml-2 max-w-[150px]">{policy.openIdConnect.clientId}</span>
                    </div>
                  </>
                )}

                {!policy.jwtBearer && !policy.openIdConnect && (
                  <div className="text-xs text-gray-400 py-1.5 border-b border-gray-100">No configuration</div>
                )}

                <div className="pt-3 text-xs text-gray-400">
                  {policy.updatedAt && new Date(policy.updatedAt).toLocaleDateString('en-US', { 
                    year: 'numeric', 
                    month: 'short', 
                    day: 'numeric' 
                  })}
                </div>
              </div>
            </Link>
            </div>
          ))}
        </div>
      )}

      {deleteConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-medium text-gray-900 mb-2">Delete Authentication Policy</h3>
            <p className="text-sm text-gray-500 mb-6">
              Are you sure you want to delete <span className="font-medium text-gray-900">"{deleteConfirm.name}"</span>?
              This action cannot be undone.
            </p>

            <div className="flex justify-end gap-3">
              <button
                onClick={() => setDeleteConfirm(null)}
                disabled={deletingId === deleteConfirm.id}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={() => handleDelete(deleteConfirm.id)}
                disabled={deletingId === deleteConfirm.id}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {deletingId === deleteConfirm.id ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                    Deleting...
                  </>
                ) : (
                  <>
                    <TrashIcon className="h-4 w-4" />
                    Delete
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AuthenticationPolicies;

