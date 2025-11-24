// API configuration file
// Use relative path so Vite proxy handles the routing
export const API_BASE_URL = '';

// OAuth base URL for external redirects (needs full URL)
// This can be overridden by environment variables
export const OAUTH_BASE_URL = (import.meta as any).env?.VITE_OAUTH_BASE_URL || 'http://localhost:5169';

// API error type
export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail: string;
}

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
        // Handle error response
        let errorData: ApiError;
        try {
          errorData = await response.json();
        } catch {
          // If response is not JSON, create a basic error object
          errorData = {
            type: 'error',
            title: 'Request Failed',
            status: response.status,
            detail: response.statusText || 'API request failed'
          };
        }
        
        // Create error with response status attached
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