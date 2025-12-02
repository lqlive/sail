import React, { createContext, useContext, useState, ReactNode } from 'react';
import { XMarkIcon, CheckCircleIcon, ExclamationCircleIcon, InformationCircleIcon, ExclamationTriangleIcon } from '@heroicons/react/24/outline';

interface Notification {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  title?: string;
  message: string;
  details?: string[];
  duration?: number;
}

interface NotificationContextType {
  notifications: Notification[];
  showNotification: (type: Notification['type'], message: string, duration?: number) => void;
  showValidationErrors: (errors: Record<string, string[]>) => void;
  showError: (title: string, message: string, details?: string[]) => void;
  removeNotification: (id: string) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const removeNotification = (id: string) => {
    setNotifications(prev => prev.filter(notification => notification.id !== id));
  };

  const showNotification = (type: Notification['type'], message: string, duration = 3000) => {
    const id = Math.random().toString(36).substr(2, 9);
    const notification = { id, type, message, duration };
    
    setNotifications(prev => [...prev, notification]);

    if (duration > 0) {
      setTimeout(() => {
        removeNotification(id);
      }, duration);
    }
  };

  const showValidationErrors = (errors: Record<string, string[]>) => {
    const allErrors = Object.entries(errors).flatMap(([field, messages]) => 
      messages.map(msg => `${field}: ${msg.replace(/^\[.*?\]\s*/, '')}`)
    );
    
    const id = Math.random().toString(36).substr(2, 9);
    const notification = {
      id,
      type: 'error' as const,
      title: 'Validation Error',
      message: 'Please fix the following errors:',
      details: allErrors,
      duration: 3000
    };
    
    setNotifications(prev => [...prev, notification]);
    
    setTimeout(() => {
      removeNotification(id);
    }, 3000);
  };

  const showError = (title: string, message: string, details?: string[]) => {
    const id = Math.random().toString(36).substr(2, 9);
    const notification = {
      id,
      type: 'error' as const,
      title,
      message,
      details,
      duration: 3000
    };
    
    setNotifications(prev => [...prev, notification]);
    
    setTimeout(() => {
      removeNotification(id);
    }, 3000);
  };

  const getIcon = (type: Notification['type']) => {
    const iconClass = "h-5 w-5";
    switch (type) {
      case 'success':
        return <CheckCircleIcon className={`${iconClass} text-green-600`} />;
      case 'error':
        return <ExclamationCircleIcon className={`${iconClass} text-red-600`} />;
      case 'warning':
        return <ExclamationTriangleIcon className={`${iconClass} text-yellow-600`} />;
      case 'info':
        return <InformationCircleIcon className={`${iconClass} text-blue-600`} />;
      default:
        return <InformationCircleIcon className={`${iconClass} text-gray-600`} />;
    }
  };

  const getNotificationStyles = (type: Notification['type']) => {
    switch (type) {
      case 'success':
        return 'bg-white border-l-4 border-l-green-500';
      case 'error':
        return 'bg-white border-l-4 border-l-red-500';
      case 'warning':
        return 'bg-white border-l-4 border-l-yellow-500';
      case 'info':
        return 'bg-white border-l-4 border-l-blue-500';
      default:
        return 'bg-white border-l-4 border-l-gray-500';
    }
  };

  const getIconBgStyles = (type: Notification['type']) => {
    switch (type) {
      case 'success':
        return 'bg-green-100';
      case 'error':
        return 'bg-red-100';
      case 'warning':
        return 'bg-yellow-100';
      case 'info':
        return 'bg-blue-100';
      default:
        return 'bg-gray-100';
    }
  };

  return (
    <NotificationContext.Provider value={{ notifications, showNotification, showValidationErrors, showError, removeNotification }}>
      {children}
      
      <div className="fixed top-20 right-4 z-50 space-y-3 max-w-md">
        {notifications.map((notification) => (
          <div
            key={notification.id}
            className={`rounded-lg shadow-xl border border-gray-200 animate-in slide-in-from-right duration-300 ${getNotificationStyles(notification.type)}`}
          >
            <div className="p-4">
              <div className="flex items-start gap-3">
                <div className={`flex-shrink-0 w-8 h-8 rounded-lg flex items-center justify-center ${getIconBgStyles(notification.type)}`}>
                  {getIcon(notification.type)}
                </div>
                <div className="flex-1 min-w-0 pt-0.5">
                  {notification.title && (
                    <h3 className="text-sm font-semibold text-gray-900 mb-1">
                      {notification.title}
                    </h3>
                  )}
                  <p className="text-sm text-gray-600">
                    {notification.message}
                  </p>
                  {notification.details && notification.details.length > 0 && (
                    <ul className="mt-3 text-sm text-gray-600 space-y-1.5 pl-1">
                      {notification.details.map((detail, index) => (
                        <li key={index} className="flex items-start gap-2">
                          <span className="text-red-500 mt-0.5">•</span>
                          <span className="flex-1">{detail}</span>
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
                <button
                  onClick={() => removeNotification(notification.id)}
                  className="flex-shrink-0 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-md p-1 transition-colors"
                >
                  <XMarkIcon className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </NotificationContext.Provider>
  );
};

export const useNotification = (): NotificationContextType => {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error('useNotification must be used within a NotificationProvider');
  }
  return context;
};
