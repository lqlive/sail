import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  ShieldCheckIcon,
  TrashIcon,
  FunnelIcon,
} from '@heroicons/react/24/outline';
import type { Certificate } from '../../types';
import { CertificateService } from '../../services/certificateService';

const Certificates: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [certificates, setCertificates] = useState<Certificate[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<{ id: string; name: string } | null>(null);

  const loadCertificates = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await CertificateService.getCertificates(searchTerm);
      setCertificates(data);
    } catch (err) {
      console.error('Failed to load certificates:', err);
      setError('Failed to load certificates');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCertificates();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== '') {
        loadCertificates();
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const handleDelete = async (id: string) => {
    try {
      setDeletingId(id);
      await CertificateService.deleteCertificate(id);
      await loadCertificates();
      setDeleteConfirm(null);
    } catch (err) {
      console.error('Failed to delete certificate:', err);
      setError('Failed to delete certificate');
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
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">Certificates</h1>
            <p className="mt-1 text-sm text-gray-500">Manage SSL/TLS certificates and SNI configurations</p>
          </div>
          <Link
            to="/certificates/new"
            className="btn-primary"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Add Certificate
          </Link>
        </div>

        {/* Search Bar */}
        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
          </div>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
            placeholder="Search certificates..."
          />
        </div>

        <div className="flex items-center text-sm text-gray-500 font-medium mt-4">
          <FunnelIcon className="h-4 w-4 mr-1.5" />
          {certificates.length} {certificates.length === 1 ? 'certificate' : 'certificates'}
        </div>
      </div>

      {error ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="h-6 w-6 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">Failed to load certificates</h3>
          <p className="text-sm text-gray-500 mb-4">Please check your connection and try again</p>
          <button
            onClick={loadCertificates}
            className="inline-flex items-center px-4 py-2 bg-white border border-gray-200 text-sm font-medium rounded-lg hover:bg-gray-50 transition-colors"
          >
            Try Again
          </button>
        </div>
      ) : certificates.length === 0 && !loading ? (
        <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <ShieldCheckIcon className="h-6 w-6 text-gray-400" />
          </div>
          <h3 className="text-sm font-medium text-gray-900 mb-1">No certificates</h3>
          <p className="text-sm text-gray-500 mb-4">Get started by adding a new certificate</p>
          <Link
            to="/certificates/new"
            className="btn-primary"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Add Certificate
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4 gap-4 max-w-7xl">
          {certificates.map((cert) => {
            const sniCount = cert.snIs?.length || 0;

            return (
              <div
                key={cert.id}
                className="bg-white border border-gray-200 rounded-lg hover:border-gray-300 hover:shadow-sm transition-all relative"
              >
                <button
                  onClick={(e) => {
                    e.preventDefault();
                    setDeleteConfirm({ id: cert.id, name: cert.name || `Certificate ${cert.id.substring(0, 8)}` });
                  }}
                  className="absolute top-3 right-3 p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-md transition-colors z-10"
                  title="Delete certificate"
                >
                  <TrashIcon className="h-4 w-4" />
                </button>

                <Link
                  to={`/certificates/${cert.id}`}
                  className="block p-4"
                >
                  <div className="flex items-center gap-3 mb-3">
                    <div className="w-10 h-10 bg-green-50 rounded-lg flex items-center justify-center flex-shrink-0">
                      <ShieldCheckIcon className="h-5 w-5 text-green-600" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className="text-sm font-medium text-gray-900 truncate">{cert.name || `Certificate ${cert.id.substring(0, 8)}`}</h3>
                      <div className="flex items-center gap-1.5 mt-1">
                        <span className="text-xs text-green-600 font-medium">● Configured</span>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <span className="text-xs text-gray-600">Certificate</span>
                      <span className="inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium bg-green-50 text-green-700">
                        <div className="w-1.5 h-1.5 rounded-full bg-green-500 mr-1" />
                        Configured
                      </span>
                    </div>

                    <div className="flex items-center justify-between py-1.5 border-b border-gray-100">
                      <span className="text-xs text-gray-600">Private Key</span>
                      <span className="inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium bg-green-50 text-green-700">
                        <div className="w-1.5 h-1.5 rounded-full bg-green-500 mr-1" />
                        Configured
                      </span>
                    </div>

                    <div className="flex items-center justify-between py-1.5">
                      <span className="text-xs text-gray-600">SNI Hostnames</span>
                      <span className="text-xs font-medium text-gray-900">{sniCount}</span>
                    </div>

                    <div className="pt-3 text-xs text-gray-400">
                      {new Date(cert.updatedAt).toLocaleDateString('en-US', { 
                        year: 'numeric', 
                        month: 'short', 
                        day: 'numeric' 
                      })}
                    </div>
                  </div>
                </Link>
              </div>
            );
          })}
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-medium text-gray-900 mb-2">Delete Certificate</h3>
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

export default Certificates;


