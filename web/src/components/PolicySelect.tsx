import React, { useState, useEffect } from 'react';
import { ChevronDownIcon, XMarkIcon } from '@heroicons/react/24/outline';

interface PolicyOption {
  value: string;
  label: string;
  type?: string;
}

interface PolicySelectProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  options: PolicyOption[];
  loading?: boolean;
  allowClear?: boolean;
  className?: string;
}

export const PolicySelect: React.FC<PolicySelectProps> = ({
  label,
  value,
  onChange,
  placeholder = 'Select a policy',
  options,
  loading = false,
  allowClear = true,
  className = '',
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  const filteredOptions = options.filter(option =>
    option.label.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const selectedOption = options.find(opt => opt.value === value);

  const handleSelect = (optionValue: string) => {
    onChange(optionValue);
    setIsOpen(false);
    setSearchTerm('');
  };

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange('');
    setSearchTerm('');
  };

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      const target = e.target as HTMLElement;
      if (!target.closest('.policy-select-container')) {
        setIsOpen(false);
        setSearchTerm('');
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => document.removeEventListener('mousedown', handleClickOutside);
    }
  }, [isOpen]);

  return (
    <div className={className}>
      <label className="block text-sm font-medium text-gray-700 mb-2">
        {label}
      </label>
      
      <div className="policy-select-container relative">
        <div
          onClick={() => !loading && setIsOpen(!isOpen)}
          className={`
            w-full px-3 py-2 border border-gray-200 rounded-lg text-sm
            cursor-pointer transition-colors flex items-center justify-between
            ${loading ? 'bg-gray-50 cursor-not-allowed' : 'hover:border-gray-400 focus-within:border-gray-400 focus-within:ring-1 focus-within:ring-gray-400'}
            ${isOpen ? 'border-gray-400 ring-1 ring-gray-400' : ''}
          `}
        >
          <span className={selectedOption ? 'text-gray-900' : 'text-gray-400'}>
            {loading ? 'Loading...' : selectedOption?.label || placeholder}
          </span>
          
          <div className="flex items-center gap-1">
            {allowClear && value && !loading && (
              <button
                type="button"
                onClick={handleClear}
                className="p-0.5 hover:bg-gray-100 rounded transition-colors"
              >
                <XMarkIcon className="w-4 h-4 text-gray-400" />
              </button>
            )}
            <ChevronDownIcon
              className={`w-4 h-4 text-gray-400 transition-transform ${isOpen ? 'rotate-180' : ''}`}
            />
          </div>
        </div>

        {isOpen && !loading && (
          <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg overflow-hidden">
            <div className="p-2 border-b border-gray-100">
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search policies..."
                className="w-full px-3 py-1.5 text-sm border border-gray-200 rounded focus:outline-none focus:border-gray-400"
                autoFocus
              />
            </div>

            <div className="max-h-60 overflow-y-auto">
              {filteredOptions.length === 0 ? (
                <div className="px-3 py-8 text-center text-sm text-gray-400">
                  No policies found
                </div>
              ) : (
                filteredOptions.map((option) => (
                  <div
                    key={option.value}
                    onClick={() => handleSelect(option.value)}
                    className={`
                      px-3 py-2 text-sm cursor-pointer transition-colors
                      hover:bg-gray-50
                      ${value === option.value ? 'bg-gray-50 text-gray-900 font-medium' : 'text-gray-700'}
                    `}
                  >
                    <div className="flex items-center justify-between">
                      <span>{option.label}</span>
                      {option.type && (
                        <span className="text-xs text-gray-400 ml-2">
                          {option.type}
                        </span>
                      )}
                    </div>
                  </div>
                ))
              )}
            </div>

            {options.length > 0 && (
              <div className="px-3 py-2 border-t border-gray-100 text-xs text-gray-400">
                {options.length} {options.length === 1 ? 'policy' : 'policies'} available
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

