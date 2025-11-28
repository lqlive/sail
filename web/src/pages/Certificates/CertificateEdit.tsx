import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeftIcon, PlusIcon, TrashIcon } from '@heroicons/react/24/outline';
import type { SNI } from '../../types';
import { CertificateService, CreateCertificateRequest, CreateSNIRequest } from '../../services/certificateService';

const CertificateEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState<CreateCertificateRequest>({
    cert: '',
    key: '',
    snis: [],
  });
  const [snis, setSnis] = useState<SNI[]>([]);
  const [newSNI, setNewSNI] = useState<CreateSNIRequest>({ hostName: '', name: '' });

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
        await CertificateService.updateCertificate(id, formData);
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

  const handleAddSNI = async () => {
    if (!newSNI.hostName || !newSNI.name) return;
    
    if (isEditMode && id) {
      try {
        await CertificateService.createSNI(id, newSNI);
        setNewSNI({ hostName: '', name: '' });
        await loadCertificate(id);
      } catch (err) {
        console.error('Failed to add SNI:', err);
      }
    } else {
      setFormData({
        ...formData,
        snis: [...(formData.snis || []), { hostName: newSNI.hostName, name: newSNI.name }]
      });
      setNewSNI({ hostName: '', name: '' });
    }
  };

  const handleDeleteSNI = async (sniId: string) => {
    if (isEditMode && id) {
      try {
        await CertificateService.deleteSNI(id, sniId);
        await loadCertificate(id);
      } catch (err) {
        console.error('Failed to delete SNI:', err);
      }
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
          <h2 className="text-base font-semibold text-gray-900 mb-6">SNI Configuration</h2>
          
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Host Name
                </label>
                <input
                  type="text"
                  value={newSNI.hostName}
                  onChange={(e) => setNewSNI({ ...newSNI, hostName: e.target.value })}
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  placeholder="www.example.com"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Display Name
                </label>
                <input
                  type="text"
                  value={newSNI.name}
                  onChange={(e) => setNewSNI({ ...newSNI, name: e.target.value })}
                  className="block w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-gray-400 focus:ring-1 focus:ring-gray-400 transition-colors"
                  placeholder="Main Domain"
                />
              </div>
            </div>
            <button
              type="button"
              onClick={handleAddSNI}
              disabled={!newSNI.hostName || !newSNI.name}
              className="inline-flex items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <PlusIcon className="h-4 w-4 mr-2" />
              Add SNI
            </button>
          </div>

          {(isEditMode ? snis : formData.snis || []).length > 0 && (
            <div className="mt-6 pt-6 border-t border-gray-200">
              <h3 className="text-sm font-medium text-gray-900 mb-4">Configured SNIs</h3>
              <div className="space-y-3">
                {isEditMode ? (
                  snis.map((sni) => (
                    <div
                      key={sni.id}
                      className="flex items-center justify-between p-4 bg-gray-50 border border-gray-100 rounded-lg hover:border-gray-200 transition-colors"
                    >
                      <div className="flex items-center gap-4 flex-1 min-w-0">
                        <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center flex-shrink-0">
                          <svg className="w-5 h-5 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                          </svg>
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">{sni.hostName}</p>
                          <p className="text-sm text-gray-500 truncate">{sni.name}</p>
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={() => handleDeleteSNI(sni.id)}
                        className="p-2 text-red-600 hover:text-red-800 hover:bg-red-50 rounded-lg transition-colors flex-shrink-0"
                        title="Delete SNI"
                      >
                        <TrashIcon className="h-5 w-5" />
                      </button>
                    </div>
                  ))
                ) : (
                  formData.snis?.map((sni, index) => (
                    <div
                      key={index}
                      className="flex items-center justify-between p-4 bg-gray-50 border border-gray-100 rounded-lg hover:border-gray-200 transition-colors"
                    >
                      <div className="flex items-center gap-4 flex-1 min-w-0">
                        <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center flex-shrink-0">
                          <svg className="w-5 h-5 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                          </svg>
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">{sni.hostName}</p>
                          <p className="text-sm text-gray-500 truncate">{sni.name}</p>
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={() => handleDeleteSNI(index.toString())}
                        className="p-2 text-red-600 hover:text-red-800 hover:bg-red-50 rounded-lg transition-colors flex-shrink-0"
                        title="Delete SNI"
                      >
                        <TrashIcon className="h-5 w-5" />
                      </button>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
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

