import React from 'react';
import { CheckIcon } from '@heroicons/react/24/outline';

interface CheckboxProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label?: string;
  disabled?: boolean;
  className?: string;
}

export const Checkbox: React.FC<CheckboxProps> = ({
  checked,
  onChange,
  label,
  disabled = false,
  className = '',
}) => {
  return (
    <label className={`inline-flex items-center group ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'} ${className}`}>
      <div className="relative flex items-center">
        <input
          type="checkbox"
          checked={checked}
          onChange={(e) => !disabled && onChange(e.target.checked)}
          disabled={disabled}
          className="sr-only"
        />
        <div
          className={`
            w-[18px] h-[18px] rounded border-2 transition-all duration-200
            ${checked
              ? 'bg-gray-900 border-gray-900 shadow-sm'
              : 'bg-white border-gray-300 group-hover:border-gray-400 group-hover:shadow-sm'
            }
            ${disabled ? 'cursor-not-allowed' : 'cursor-pointer'}
          `}
        >
          {checked && (
            <div className="absolute inset-0 flex items-center justify-center">
              <CheckIcon className="w-3 h-3 text-white" strokeWidth={3.5} />
            </div>
          )}
        </div>
      </div>
      {label && (
        <span className={`ml-2 text-sm select-none ${disabled ? 'text-gray-400' : 'text-gray-700'}`}>
          {label}
        </span>
      )}
    </label>
  );
};

