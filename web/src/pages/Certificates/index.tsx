import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  ShieldCheckIcon,
  PencilIcon,
  TrashIcon,
} from '@heroicons/react/24/outline';
import type { Certificate } from '../../types/gateway';
import { CertificateService } from '../../services/certificateService';

const Certificates: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [certificates, setCertificates] = useState<Certificate[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);

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
    if (!confirm('Are you sure you want to delete this certificate?')) {
      return;
    }

    try {
      await CertificateService.deleteCertificate(id);
      await loadCertificates();
    } catch (err) {
      console.error('Failed to delete certificate:', err);
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
      <div className="section-header">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-xl font-medium text-gray-900">Certificates</h1>
            <p className="text-sm text-gray-600">Manage SSL/TLS certificates and SNI configurations</p>
          </div>
          <Link
            to="/certificates/new"
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-gray-900 hover:bg-gray-800"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Add Certificate
          </Link>
        </div>
      </div>

      <div className="mb-6">
        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <MagnifyingGlassIcon className="h-4 w-4 text-gray-400" />
          </div>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-gray-900 focus:border-gray-900 sm:text-sm"
            placeholder="Search certificates..."
          />
        </div>
      </div>

      {error ? (
        <div className="text-center py-12">
          <div className="mx-auto h-12 w-12 text-red-400 mb-4">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
          </div>
          <h3 className="mt-2 text-sm font-medium text-gray-900">Failed to load certificates</h3>
          <p className="mt-1 text-sm text-gray-500">Please check your connection and try again.</p>
          <div className="mt-6">
            <button
              onClick={loadCertificates}
              className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50"
            >
              Try Again
            </button>
          </div>
        </div>
      ) : certificates.length === 0 && !loading ? (
        <div className="text-center py-12">
          <ShieldCheckIcon className="mx-auto h-12 w-12 text-gray-400" />
          <h3 className="mt-2 text-sm font-medium text-gray-900">No certificates</h3>
          <p className="mt-1 text-sm text-gray-500">Get started by adding a new certificate.</p>
          <div className="mt-6">
            <Link
              to="/certificates/new"
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-gray-900 hover:bg-gray-800"
            >
              <PlusIcon className="h-4 w-4 mr-2" />
              Add Certificate
            </Link>
          </div>
        </div>
      ) : (
        <div className="space-y-3">
          {certificates.map((cert) => (
            <div
              key={cert.id}
              className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-sm transition-shadow"
            >
              <div className="flex items-start justify-between">
                <div className="flex items-start space-x-3 flex-1">
                  <ShieldCheckIcon className="h-6 w-6 text-green-600" />
                  <div className="flex-1">
                    <div className="flex items-center space-x-2 mb-2">
                      <h3 className="text-sm font-medium text-gray-900">
                        Certificate {cert.id.substring(0, 8)}
                      </h3>
                    </div>
                    
                    {cert.snis && cert.snis.length > 0 && (
                      <div className="space-y-1">
                        <p className="text-xs text-gray-600">SNI Configurations:</p>
                        <div className="flex flex-wrap gap-2">
                          {cert.snis.map((sni) => (
                            <span
                              key={sni.id}
                              className="inline-flex items-center px-2 py-1 rounded-md text-xs bg-blue-50 text-blue-700"
                              title={sni.name}
                            >
                              {sni.hostName}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}
                    
                    <div className="mt-2 flex items-center space-x-4 text-xs text-gray-500">
                      <span>Created: {new Date(cert.createdAt).toLocaleDateString()}</span>
                      <span>Updated: {new Date(cert.updatedAt).toLocaleDateString()}</span>
                    </div>
                  </div>
                </div>
                
                <div className="flex items-center space-x-2 ml-4">
                  <Link
                    to={`/certificates/${cert.id}`}
                    className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded"
                  >
                    <PencilIcon className="h-4 w-4" />
                  </Link>
                  <button
                    onClick={() => handleDelete(cert.id)}
                    className="p-2 text-red-600 hover:text-red-800 hover:bg-red-50 rounded"
                  >
                    <TrashIcon className="h-4 w-4" />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Certificates;

