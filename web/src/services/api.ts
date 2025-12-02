export const API_BASE_URL = '';
export const OAUTH_BASE_URL = (import.meta as any).env?.VITE_OAUTH_BASE_URL || 'http://localhost:5169';

export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export interface ApiErrorEvent {
  status: number;
  error: ApiError;
}

const API_ERROR_EVENT = 'api-error';

export const emitApiError = (event: ApiErrorEvent) => {
  window.dispatchEvent(new CustomEvent(API_ERROR_EVENT, { detail: event }));
};

export const onApiError = (callback: (event: ApiErrorEvent) => void) => {
  const handler = (e: Event) => callback((e as CustomEvent<ApiErrorEvent>).detail);
  window.addEventListener(API_ERROR_EVENT, handler);
  return () => window.removeEventListener(API_ERROR_EVENT, handler);
};

// Generic API response handler
export class ApiClient {
  private baseURL: string;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
  }

  async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const separator = endpoint.includes('?') ? '&' : '?';
    const url = `${this.baseURL}${endpoint}${separator}api-version=1.0`;
    
    const defaultHeaders = {
      'Content-Type': 'application/json',
    };

    const config: RequestInit = {
      ...options,
      credentials: 'include', // Include cookies in requests
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
    };

    try {
      const response = await fetch(url, config);
      
      if (!response.ok) {
        let errorData: ApiError;
        try {
          errorData = await response.json();
        } catch {
          errorData = {
            type: 'error',
            title: 'Request Failed',
            status: response.status,
            detail: response.statusText || 'API request failed'
          };
        }
        
        emitApiError({ status: response.status, error: errorData });
        
        const error = new Error(errorData.detail || errorData.title || 'API request failed') as any;
        error.response = { status: response.status, data: errorData };
        error.status = response.status;
        throw error;
      }

      // If response status is 204 (No Content), return empty object
      if (response.status === 204) {
        return {} as T;
      }

      // Check if response has content before trying to parse JSON
      const contentType = response.headers.get('content-type');
      const contentLength = response.headers.get('content-length');
      
      // If no content-type or content-length is 0, return empty object
      if (!contentType?.includes('application/json') || contentLength === '0') {
        return {} as T;
      }

      // Check if response body is empty
      const text = await response.text();
      if (!text.trim()) {
        return {} as T;
      }

      return JSON.parse(text);
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }
      throw new Error('Network error occurred');
    }
  }

  // GET request
  async get<T>(endpoint: string, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'GET',
      headers,
    });
  }

  // POST request
  async post<T>(
    endpoint: string,
    data?: any,
    headers?: Record<string, string>
  ): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
      headers,
    });
  }

  // PUT request
  async put<T>(
    endpoint: string,
    data?: any,
    headers?: Record<string, string>
  ): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
      headers,
    });
  }

  // PATCH request
  async patch<T>(
    endpoint: string,
    data?: any,
    headers?: Record<string, string>
  ): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PATCH',
      body: data ? JSON.stringify(data) : undefined,
      headers,
    });
  }

  // DELETE request
  async delete<T>(endpoint: string, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'DELETE',
      headers,
    });
  }
}

// Create default API client instance
export const apiClient = new ApiClient(API_BASE_URL);