import React, { createContext, useContext, useState, ReactNode } from 'react';
import { XMarkIcon } from '@heroicons/react/24/outline';

interface Notification {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  message: string;
  duration?: number;
}

interface NotificationContextType {
  notifications: Notification[];
  showNotification: (type: Notification['type'], message: string, duration?: number) => void;
  removeNotification: (id: string) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const showNotification = (type: Notification['type'], message: string, duration = 3000) => {
    const id = Math.random().toString(36).substr(2, 9);
    const notification = { id, type, message, duration };
    
    setNotifications(prev => [...prev, notification]);

    // Auto remove after duration
    if (duration > 0) {
      setTimeout(() => {
        removeNotification(id);
      }, duration);
    }
  };

  const removeNotification = (id: string) => {
    setNotifications(prev => prev.filter(notification => notification.id !== id));
  };

  const getNotificationStyles = (type: Notification['type']) => {
    switch (type) {
      case 'success':
        return 'bg-green-50 border-green-200 text-green-800';
      case 'error':
        return 'bg-red-50 border-red-200 text-red-800';
      case 'warning':
        return 'bg-yellow-50 border-yellow-200 text-yellow-800';
      case 'info':
        return 'bg-blue-50 border-blue-200 text-blue-800';
      default:
        return 'bg-gray-50 border-gray-200 text-gray-800';
    }
  };

  const getButtonStyles = (type: Notification['type']) => {
    switch (type) {
      case 'success':
        return 'text-green-500 hover:bg-green-500';
      case 'error':
        return 'text-red-500 hover:bg-red-500';
      case 'warning':
        return 'text-yellow-500 hover:bg-yellow-500';
      case 'info':
        return 'text-blue-500 hover:bg-blue-500';
      default:
        return 'text-gray-500 hover:bg-gray-500';
    }
  };

  return (
    <NotificationContext.Provider value={{ notifications, showNotification, removeNotification }}>
      {children}
      
      {/* Global Notification Container */}
      <div className="fixed top-20 right-4 z-40 space-y-2">
        {notifications.map((notification) => (
          <div
            key={notification.id}
            className={`rounded-lg p-4 shadow-lg max-w-sm border animate-in slide-in-from-right duration-300 ${getNotificationStyles(notification.type)}`}
          >
            <div className="flex items-center">
              <div className="flex-1">
                <p className="text-sm font-medium">
                  {notification.message}
                </p>
              </div>
              <button
                onClick={() => removeNotification(notification.id)}
                className={`ml-3 p-1 rounded-md hover:bg-opacity-20 transition-colors ${getButtonStyles(notification.type)}`}
              >
                <XMarkIcon className="h-4 w-4" />
              </button>
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
