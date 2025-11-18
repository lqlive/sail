import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { ChevronLeftIcon } from '@heroicons/react/24/outline';

const RouteDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  return (
    <div className="fade-in">
      <Link to="/routes" className="inline-flex items-center text-sm text-gray-600 hover:text-gray-900 mb-6">
        <ChevronLeftIcon className="h-4 w-4 mr-1" />
        Back to Routes
      </Link>
      
      <div className="section-header">
        <h1 className="text-xl font-medium text-gray-900">Route Details</h1>
        <p className="text-sm text-gray-600">Route ID: {id}</p>
      </div>

      <div className="bg-white border border-gray-200 rounded-lg p-6">
        <p className="text-sm text-gray-600">Route details page - To be implemented</p>
      </div>
    </div>
  );
};

export default RouteDetail;

