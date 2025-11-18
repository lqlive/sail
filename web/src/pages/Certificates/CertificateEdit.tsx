import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeftIcon, PlusIcon, TrashIcon } from '@heroicons/react/24/outline';
import type { SNI } from '../../types/gateway';
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
    <div className="fade-in">
      <div className="mb-6">
        <button
          onClick={() => navigate('/certificates')}
          className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900"
        >
          <ArrowLeftIcon className="h-4 w-4 mr-1" />
          Back to Certificates
        </button>
      </div>

      <div className="section-header mb-4">
        <h1 className="text-lg font-medium text-gray-900">
          {isEditMode ? 'Edit Certificate' : 'Create Certificate'}
        </h1>
        <p className="text-xs text-gray-600">
          {isEditMode ? 'Update certificate details' : 'Add a new SSL/TLS certificate'}
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="bg-white border border-gray-200 rounded-lg shadow-sm p-4">
          <h2 className="text-sm font-medium text-gray-900 mb-3">SNI Configuration</h2>
          
          <div className="space-y-3 mb-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Host Name
                </label>
                <input
                  type="text"
                  value={newSNI.hostName}
                  onChange={(e) => setNewSNI({ ...newSNI, hostName: e.target.value })}
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="www.example.com"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Display Name
                </label>
                <input
                  type="text"
                  value={newSNI.name}
                  onChange={(e) => setNewSNI({ ...newSNI, name: e.target.value })}
                  className="block w-full px-2.5 py-1.5 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-900 focus:border-gray-900 text-sm"
                  placeholder="Main Domain"
                />
              </div>
            </div>
            <button
              type="button"
              onClick={handleAddSNI}
              disabled={!newSNI.hostName || !newSNI.name}
              className="inline-flex items-center px-3 py-1.5 border border-gray-300 text-xs font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <PlusIcon className="h-3.5 w-3.5 mr-1.5" />
              Add SNI
            </button>
          </div>

          {(isEditMode ? snis : formData.snis || []).length > 0 && (
            <div className="space-y-2">
              <h3 className="text-xs font-medium text-gray-700 mb-2">Configured SNIs</h3>
              {isEditMode ? (
                snis.map((sni) => (
                  <div
                    key={sni.id}
                    className="flex items-center justify-between p-2 bg-gray-50 rounded-md"
                  >
                    <div>
                      <p className="text-xs font-medium text-gray-900">{sni.hostName}</p>
                      <p className="text-xs text-gray-600">{sni.name}</p>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleDeleteSNI(sni.id)}
                      className="text-red-600 hover:text-red-800"
                    >
                      <TrashIcon className="h-3.5 w-3.5" />
                    </button>
                  </div>
                ))
              ) : (
                formData.snis?.map((sni, index) => (
                  <div
                    key={index}
                    className="flex items-center justify-between p-2 bg-gray-50 rounded-md"
                  >
                    <div>
                      <p className="text-xs font-medium text-gray-900">{sni.hostName}</p>
                      <p className="text-xs text-gray-600">{sni.name}</p>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleDeleteSNI(index.toString())}
                      className="text-red-600 hover:text-red-800"
                    >
                      <TrashIcon className="h-3.5 w-3.5" />
                    </button>
                  </div>
                ))
              )}
            </div>
          )}
        </div>

        <div className="bg-white border border-gray-200 rounded-lg shadow-sm p-4">
          <h2 className="text-sm font-medium text-gray-900 mb-3">Certificate Information</h2>
          
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

        <div className="flex justify-end space-x-2">
          <button
            type="button"
            onClick={() => navigate('/certificates')}
            className="px-3 py-1.5 border border-gray-300 text-xs font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={saving}
            className="px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-gray-900 hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? 'Saving...' : isEditMode ? 'Update Certificate' : 'Create Certificate'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CertificateEdit;

