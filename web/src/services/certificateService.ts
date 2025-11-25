import { apiClient } from './api';
import type { Certificate, SNI } from '../types';

export interface CreateCertificateRequest {
  cert: string;
  key: string;
  snis?: Array<{
    hostName: string;
    name: string;
  }>;
}

export interface UpdateCertificateRequest {
  cert?: string;
  key?: string;
}

export interface CreateSNIRequest {
  hostName: string;
  name: string;
}

export interface UpdateSNIRequest {
  hostName?: string;
  name?: string;
}

export const CertificateService = {
  async getCertificates(search?: string): Promise<Certificate[]> {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    
    const data = await apiClient.get<Certificate[] | { items: Certificate[] }>(`/api/certificates${params.toString() ? `?${params.toString()}` : ''}`);
    
    if (Array.isArray(data)) {
      return data;
    }
    if (data && 'items' in data && Array.isArray(data.items)) {
      return data.items;
    }
    return [];
  },

  async getCertificate(id: string): Promise<Certificate> {
    return apiClient.get<Certificate>(`/api/certificates/${id}`);
  },

  async createCertificate(data: CreateCertificateRequest): Promise<void> {
    await apiClient.post<void>('/api/certificates', data);
  },

  async updateCertificate(id: string, data: UpdateCertificateRequest): Promise<void> {
    await apiClient.patch<void>(`/api/certificates/${id}`, data);
  },

  async deleteCertificate(id: string): Promise<void> {
    await apiClient.delete<void>(`/api/certificates/${id}`);
  },

  async getSNIs(certificateId: string): Promise<SNI[]> {
    return apiClient.get<SNI[]>(`/api/certificates/${certificateId}/snis`);
  },

  async createSNI(certificateId: string, data: CreateSNIRequest): Promise<void> {
    await apiClient.post<void>(`/api/certificates/${certificateId}/snis`, data);
  },

  async updateSNI(certificateId: string, sniId: string, data: UpdateSNIRequest): Promise<void> {
    await apiClient.post<void>(`/api/certificates/${certificateId}/snis/${sniId}`, data);
  },

  async deleteSNI(certificateId: string, sniId: string): Promise<void> {
    await apiClient.delete<void>(`/api/certificates/${certificateId}/snis/${sniId}`);
  },
};

