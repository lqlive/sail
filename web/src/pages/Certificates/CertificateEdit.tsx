import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeftIcon, PlusIcon, TrashIcon, XMarkIcon } from '@heroicons/react/24/outline';
import type { SNI } from '../../types';
import { CertificateService, CreateCertificateRequest, CreateSNIRequest } from '../../services/certificateService';
import { FormField } from '../../components/FormField';

const CertificateEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState<CreateCertificateRequest>({
    name: '',
    cert: '',
    key: '',
    snis: [],
  });
  const [snis, setSnis] = useState<SNI[]>([]);
  const [newSNI, setNewSNI] = useState<CreateSNIRequest>({ hostName: '', name: '' });
  const [sniHostNameError, setSniHostNameError] = useState<string | null>(null);
  const [sniNameError, setSniNameError] = useState<string | null>(null);

  useEffect(() => {
    if (isEditMode && id) {
      loadCertificate(id);
    }
  }, [id, isEditMode]);

  const loadCertificate = async (certificateId: string) => {
    try {
      setLoading(true);
      const certificate = await CertificateService.getCertificate(certificateId);
      setFormData({
        name: certificate.name || '',
        cert: certificate.cert || '',
        key: certificate.key || '',
        snis: [],
      });
      
      const sniList = await CertificateService.getSNIs(certificateId);
      setSnis(sniList);
    } catch (err) {
      console.error('Failed to load certificate:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setSaving(true);
      
      if (isEditMode && id) {
        const updateData = {
          name: formData.name,
          cert: formData.cert,
          key: formData.key,
          snis: snis.map(s => ({ hostName: s.hostName, name: s.name }))
        };
        await CertificateService.updateCertificate(id, updateData);
      } else {
        await CertificateService.createCertificate(formData);
      }
      
      navigate('/certificates');
    } catch (err) {
      console.error('Failed to save certificate:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleAddSNI = () => {
    setSniHostNameError(null);
    setSniNameError(null);

    let hasError = false;

    if (!newSNI.hostName || !newSNI.hostName.trim()) {
      setSniHostNameError('Please enter a host name');
      hasError = true;
    }

    if (!newSNI.name || !newSNI.name.trim()) {
      setSniNameError('Please enter a display name');
      hasError = true;
    }

    if (hasError) return;
    
    if (isEditMode) {
      setSnis([...snis, { 
        id: `temp-${Date.now()}`,
        hostName: newSNI.hostName, 
        name: newSNI.name,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }]);
    } else {
      setFormData({
        ...formData,
        snis: [...(formData.snis || []), { hostName: newSNI.hostName, name: newSNI.name }]
      });
    }
    setNewSNI({ hostName: '', name: '' });
  };

  const handleDeleteSNI = (sniId: string) => {
    if (isEditMode) {
      setSnis(snis.filter(s => s.id !== sniId));
    } else {
      const index = parseInt(sniId);
      setFormData({
        ...formData,
        snis: formData.snis?.filter((_, i) => i !== index)
      });
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
      <div className="mb-6">
        <button
          onClick={() => navigate('/certificates')}
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 transition-colors"
        >
          <ArrowLeftIcon className="h-4 w-4 mr-1" />
          Back to Certificates
        </button>
      </div>

      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">
          {isEditMode ? 'Edit Certificate' : 'Create Certificate'}
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          {isEditMode ? 'Update certificate details' : 'Add a new SSL/TLS certificate'}
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-6">Basic Information</h2>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Certificate Name
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
              placeholder="My Certificate"
              required
            />
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-6">SNI Configuration</h2>
          <div className="space-y-5">
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Host Name" error={sniHostNameError || undefined}>
                <input
                  type="text"
                  value={newSNI.hostName}
                  onChange={(e) => {
                    setNewSNI({ ...newSNI, hostName: e.target.value });
                    if (sniHostNameError) setSniHostNameError(null);
                  }}
                  placeholder="www.example.com"
                  className={`block w-full px-3 py-2 border rounded-lg text-sm focus:outline-none transition-colors ${
                    sniHostNameError 
                      ? 'border-red-300 focus:border-red-500 focus:ring-1 focus:ring-red-500' 
                      : 'border-gray-200 focus:border-gray-400 focus:ring-1 focus:ring-gray-400'
                  }`}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddSNI())}
                />
              </FormField>
              <FormField label="Display Name" error={sniNameError || undefined}>
                <input
                  type="text"
                  value={newSNI.name}
                  onChange={(e) => {
                    setNewSNI({ ...newSNI, name: e.target.value });
                    if (sniNameError) setSniNameError(null);
                  }}
                  placeholder="Main Domain"
                  className={`block w-full px-3 py-2 border rounded-lg text-sm focus:outline-none transition-colors ${
                    sniNameError 
                      ? 'border-red-300 focus:border-red-500 focus:ring-1 focus:ring-red-500' 
                      : 'border-gray-200 focus:border-gray-400 focus:ring-1 focus:ring-gray-400'
                  }`}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddSNI())}
                />
              </FormField>
            </div>

            <div>
              <button
                type="button"
                onClick={handleAddSNI}
                className="inline-flex items-center px-4 py-2 border border-gray-200 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
              >
                <PlusIcon className="h-4 w-4 mr-1.5" />
                Add SNI
              </button>
            </div>

            {(isEditMode ? snis : formData.snis || []).length > 0 && (
              <div className="mt-6 pt-6 border-t border-gray-200">
                <h3 className="text-sm font-medium text-gray-900 mb-3">
                  Configured SNIs ({(isEditMode ? snis : formData.snis || []).length})
                </h3>
                <div className="space-y-2">
                  {isEditMode ? (
                    snis.map((sni) => (
                      <div
                        key={sni.id}
                        className="flex items-center justify-between px-3 py-2.5 bg-white border border-gray-200 rounded-lg hover:border-gray-300 transition-colors"
                      >
                        <div className="flex items-center gap-2.5 flex-1 min-w-0">
                          <div className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                            <svg className="h-4 w-4 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                            </svg>
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="text-sm font-medium text-gray-900 truncate">{sni.hostName}</div>
                            <div className="text-xs text-gray-500 mt-0.5">{sni.name}</div>
                          </div>
                        </div>
                        <button
                          type="button"
                          onClick={() => handleDeleteSNI(sni.id)}
                          className="ml-3 text-gray-400 hover:text-red-600 transition-colors"
                        >
                          <XMarkIcon className="h-4 w-4" />
                        </button>
                      </div>
                    ))
                  ) : (
                    formData.snis?.map((sni, index) => (
                      <div
                        key={index}
                        className="flex items-center justify-between px-3 py-2.5 bg-white border border-gray-200 rounded-lg hover:border-gray-300 transition-colors"
                      >
                        <div className="flex items-center gap-2.5 flex-1 min-w-0">
                          <div className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                            <svg className="h-4 w-4 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                            </svg>
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="text-sm font-medium text-gray-900 truncate">{sni.hostName}</div>
                            <div className="text-xs text-gray-500 mt-0.5">{sni.name}</div>
                          </div>
                        </div>
                        <button
                          type="button"
                          onClick={() => handleDeleteSNI(index.toString())}
                          className="ml-3 text-gray-400 hover:text-red-600 transition-colors"
                        >
                          <XMarkIcon className="h-4 w-4" />
                        </button>
                      </div>
                    ))
                  )}
                </div>
              </div>
            )}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-6">Certificate Information</h2>
          
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Certificate (PEM)
              </label>
              <textarea
                value={formData.cert}
                onChange={(e) => setFormData({ ...formData, cert: e.target.value })}
                className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-xs font-mono"
                rows={6}
                placeholder="-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Private Key (PEM)
              </label>
              <textarea
                value={formData.key}
                onChange={(e) => setFormData({ ...formData, key: e.target.value })}
                className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-xs font-mono"
                rows={6}
                placeholder="-----BEGIN PRIVATE KEY-----
...
-----END PRIVATE KEY-----"
                required
              />
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <button
            type="button"
            onClick={() => navigate('/certificates')}
            className="btn-secondary"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={saving}
            className="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? 'Saving...' : isEditMode ? 'Update Certificate' : 'Create Certificate'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CertificateEdit;

