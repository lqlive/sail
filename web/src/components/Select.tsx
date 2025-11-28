import React, { useState, useEffect, useRef } from 'react';
import { ChevronDownIcon, XMarkIcon, CheckIcon } from '@heroicons/react/24/outline';

export interface SelectOption {
  value: string;
  label: string;
  description?: string;
  disabled?: boolean;
}

interface SelectProps {
  value: string;
  onChange: (value: string) => void;
  options: SelectOption[];
  placeholder?: string;
  disabled?: boolean;
  allowClear?: boolean;
  className?: string;
}

export const Select: React.FC<SelectProps> = ({
  value,
  onChange,
  options,
  placeholder = 'Select an option',
  disabled = false,
  allowClear = false,
  className = '',
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find(opt => opt.value === value);

  const handleSelect = (optionValue: string) => {
    if (!disabled) {
      onChange(optionValue);
      setIsOpen(false);
    }
  };

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!disabled) {
      onChange('');
    }
  };

  const handleToggle = () => {
    if (!disabled) {
      setIsOpen(!isOpen);
    }
  };

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };

    const handleKeyDown = (e: KeyboardEvent) => {
      if (isOpen && e.key === 'Escape') {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      document.addEventListener('keydown', handleKeyDown);
      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
        document.removeEventListener('keydown', handleKeyDown);
      };
    }
  }, [isOpen]);

  return (
    <div ref={containerRef} className={`relative ${className}`}>
      <div
        onClick={handleToggle}
        className={`
          w-full px-3 py-2 border rounded-lg cursor-pointer transition-colors
          flex items-center justify-between text-sm
          ${disabled 
            ? 'bg-gray-50 text-gray-500 cursor-not-allowed border-gray-200' 
            : 'bg-white hover:border-gray-400'
          }
          ${isOpen && !disabled ? 'border-gray-400 ring-1 ring-gray-400' : 'border-gray-200'}
        `}
      >
        <span className={selectedOption ? 'text-gray-900' : 'text-gray-400'}>
          {selectedOption?.label || placeholder}
        </span>
        
        <div className="flex items-center gap-1 ml-2">
          {allowClear && value && !disabled && (
            <button
              type="button"
              onClick={handleClear}
              className="p-0.5 hover:bg-gray-100 rounded transition-colors"
              tabIndex={-1}
            >
              <XMarkIcon className="w-4 h-4 text-gray-400" />
            </button>
          )}
          <ChevronDownIcon
            className={`w-4 h-4 text-gray-400 transition-transform ${isOpen ? 'rotate-180' : ''}`}
          />
        </div>
      </div>

      {isOpen && !disabled && (
        <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg overflow-hidden">
          <div className="max-h-60 overflow-y-auto py-1">
            {options.length === 0 ? (
              <div className="px-3 py-6 text-center text-sm text-gray-400">
                No options
              </div>
            ) : (
              options.map((option) => (
                <div
                  key={option.value}
                  onClick={() => !option.disabled && handleSelect(option.value)}
                  className={`
                    px-3 py-1.5 text-sm cursor-pointer transition-colors
                    ${option.disabled ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-50'}
                    ${value === option.value ? 'bg-gray-50' : ''}
                  `}
                >
                  <div className="flex items-center gap-2">
                    {value === option.value && (
                      <CheckIcon className="w-3.5 h-3.5 text-gray-900 flex-shrink-0" />
                    )}
                    <div className="flex-1 min-w-0">
                      <div className={`${value === option.value ? 'font-medium text-gray-900' : 'text-gray-700'}`}>
                        {option.label}
                      </div>
                      {option.description && (
                        <div className="text-xs text-gray-500 leading-tight">
                          {option.description}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
};

