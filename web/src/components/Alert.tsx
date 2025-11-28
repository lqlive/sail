import React from 'react';
import { 
  InformationCircleIcon, 
  ExclamationTriangleIcon, 
  CheckCircleIcon, 
  XCircleIcon 
} from '@heroicons/react/24/outline';

type AlertType = 'info' | 'warning' | 'success' | 'error';

interface AlertProps {
  type?: AlertType;
  children: React.ReactNode;
  className?: string;
}

const alertStyles = {
  info: {
    container: 'bg-gray-50 border-gray-200',
    icon: 'text-gray-500',
    text: 'text-gray-700',
    Icon: InformationCircleIcon,
  },
  warning: {
    container: 'bg-yellow-50 border-yellow-200',
    icon: 'text-yellow-600',
    text: 'text-yellow-800',
    Icon: ExclamationTriangleIcon,
  },
  success: {
    container: 'bg-green-50 border-green-200',
    icon: 'text-green-600',
    text: 'text-green-800',
    Icon: CheckCircleIcon,
  },
  error: {
    container: 'bg-red-50 border-red-200',
    icon: 'text-red-600',
    text: 'text-red-800',
    Icon: XCircleIcon,
  },
};

export const Alert: React.FC<AlertProps> = ({ 
  type = 'info', 
  children, 
  className = '' 
}) => {
  const style = alertStyles[type];
  const Icon = style.Icon;

  return (
    <div className={`flex gap-2.5 px-3 py-2.5 border rounded-lg ${style.container} ${className}`}>
      <Icon className={`w-4 h-4 ${style.icon} flex-shrink-0 mt-0.5`} />
      <div className={`text-xs ${style.text} flex-1 leading-relaxed`}>
        {children}
      </div>
    </div>
  );
};

