import { useState } from 'react'
import { Plus, Search, Shield, Calendar, AlertTriangle } from 'lucide-react'

interface Certificate {
  id: string
  domain: string
  issuer: string
  validFrom: string
  validUntil: string
  status: 'valid' | 'expiring' | 'expired'
}

export default function Certificates() {
  const [certificates] = useState<Certificate[]>([
    {
      id: '1',
      domain: '*.example.com',
      issuer: "Let's Encrypt",
      validFrom: '2024-01-01',
      validUntil: '2025-01-01',
      status: 'valid',
    },
    {
      id: '2',
      domain: 'api.example.com',
      issuer: 'DigiCert',
      validFrom: '2024-03-15',
      validUntil: '2024-11-30',
      status: 'expiring',
    },
    {
      id: '3',
      domain: 'old.example.com',
      issuer: "Let's Encrypt",
      validFrom: '2023-06-01',
      validUntil: '2024-06-01',
      status: 'expired',
    },
  ])

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'valid':
        return <Shield className="h-4 w-4 text-green-600" />
      case 'expiring':
        return <AlertTriangle className="h-4 w-4 text-yellow-600" />
      case 'expired':
        return <AlertTriangle className="h-4 w-4 text-red-600" />
      default:
        return <Shield className="h-4 w-4 text-gray-600" />
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Certificates</h1>
          <p className="mt-1 text-xs text-gray-600">
            Manage SSL/TLS certificates for secure connections
          </p>
        </div>
        <button className="px-4 py-2 text-sm bg-slate-800 text-white hover:bg-slate-900 rounded-lg font-medium flex items-center transition-all">
          <Plus className="h-4 w-4 mr-2" />
          Upload Certificate
        </button>
      </div>

      <div className="card">
        <div className="p-3 border-b border-gray-200">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search certificates..."
              className="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-slate-500 focus:border-slate-500"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 p-4">
          {certificates.map((cert) => (
            <div
              key={cert.id}
              className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center space-x-2.5">
                  {getStatusIcon(cert.status)}
                  <div>
                    <h3 className="text-base font-semibold text-gray-900">
                      {cert.domain}
                    </h3>
                    <p className="text-xs text-gray-600">Issued by {cert.issuer}</p>
                  </div>
                </div>
                <span
                  className={`badge ${
                    cert.status === 'valid'
                      ? 'badge-success'
                      : cert.status === 'expiring'
                      ? 'badge-warning'
                      : 'badge-error'
                  }`}
                >
                  {cert.status}
                </span>
              </div>

              <div className="space-y-2">
                <div className="flex items-center text-xs text-gray-600">
                  <Calendar className="h-3.5 w-3.5 mr-2" />
                  <span>Valid from: {cert.validFrom}</span>
                </div>
                <div className="flex items-center text-xs text-gray-600">
                  <Calendar className="h-3.5 w-3.5 mr-2" />
                  <span>Valid until: {cert.validUntil}</span>
                </div>
              </div>

              <div className="mt-3 pt-3 border-t border-gray-200 flex space-x-2">
                <button className="flex-1 text-xs font-medium text-slate-700 hover:text-slate-900">
                  View Details
                </button>
                <button className="flex-1 text-xs font-medium text-gray-600 hover:text-gray-700">
                  Renew
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

