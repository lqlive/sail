import React, { useState, useEffect } from 'react';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  ShieldCheckIcon,
} from '@heroicons/react/24/outline';
import type { Certificate } from '../../types/gateway';

const mockCertificates: Certificate[] = [
  {
    id: '1',
    name: 'example.com',
    subject: 'CN=example.com',
    issuer: 'CN=Let\'s Encrypt Authority X3',
    notBefore: '2024-01-01T00:00:00Z',
    notAfter: '2024-12-31T23:59:59Z',
    thumbprint: '1A2B3C4D5E6F7G8H9I0J',
    enabled: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'api.example.com',
    subject: 'CN=api.example.com',
    issuer: 'CN=DigiCert Global Root CA',
    notBefore: '2024-02-01T00:00:00Z',
    notAfter: '2024-03-31T23:59:59Z',
    thumbprint: 'K0L1M2N3O4P5Q6R7S8T9',
    enabled: true,
    createdAt: '2024-02-01T00:00:00Z',
  },
];

const Certificates: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [certificates, setCertificates] = useState<Certificate[]>([]);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    setLoading(true);
    setTimeout(() => {
      setCertificates(mockCertificates);
      setLoading(false);
    }, 500);
  }, []);

  const filteredCertificates = certificates.filter(cert =>
    cert.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    cert.subject.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const isExpiringSoon = (notAfter: string) => {
    const expiry = new Date(notAfter);
    const now = new Date();
    const daysUntilExpiry = Math.floor((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    return daysUntilExpiry <= 30;
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
            <p className="text-sm text-gray-600">Manage SSL/TLS certificates</p>
          </div>
          <button className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-gray-900 hover:bg-gray-800">
            <PlusIcon className="h-4 w-4 mr-2" />
            Upload Certificate
          </button>
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

      <div className="space-y-3">
        {filteredCertificates.map((cert) => {
          const expiring = isExpiringSoon(cert.notAfter);
          return (
            <div
              key={cert.id}
              className="bg-white border border-gray-200 rounded-lg p-4"
            >
              <div className="flex items-start justify-between">
                <div className="flex items-start space-x-3 flex-1">
                  <ShieldCheckIcon className={`h-6 w-6 ${expiring ? 'text-yellow-600' : 'text-green-600'}`} />
                  <div className="flex-1">
                    <h3 className="text-sm font-medium text-gray-900 mb-1">{cert.name}</h3>
                    <p className="text-xs text-gray-600 mb-2">{cert.subject}</p>
                    
                    <div className="grid grid-cols-2 gap-3 text-xs">
                      <div>
                        <span className="text-gray-600">Issuer:</span>
                        <p className="font-medium">{cert.issuer}</p>
                      </div>
                      <div>
                        <span className="text-gray-600">Valid Until:</span>
                        <p className="font-medium">{new Date(cert.notAfter).toLocaleDateString()}</p>
                      </div>
                      <div>
                        <span className="text-gray-600">Thumbprint:</span>
                        <p className="font-mono text-xs">{cert.thumbprint}</p>
                      </div>
                      <div>
                        <span className="text-gray-600">Status:</span>
                        <div className="flex items-center space-x-2 mt-0.5">
                          {expiring ? (
                            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs bg-yellow-100 text-yellow-800">
                              <ExclamationTriangleIcon className="h-3 w-3 mr-1" />
                              Expiring Soon
                            </span>
                          ) : (
                            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs bg-green-100 text-green-800">
                              <CheckCircleIcon className="h-3 w-3 mr-1" />
                              Valid
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default Certificates;

