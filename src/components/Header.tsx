import React from 'react';
import { Headphones, Activity } from 'lucide-react';

export function Header() {
  return (
    <header className="bg-white shadow-sm border-b border-gray-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <div className="flex items-center">
            <div className="flex-shrink-0 flex items-center">
              <Headphones className="h-8 w-8 text-blue-600" />
              <h1 className="ml-3 text-xl font-bold text-gray-900">
                TSAPI Çağrı Merkezi İzleme
              </h1>
            </div>
          </div>
          
          <div className="flex items-center space-x-4">
            <div className="flex items-center text-sm text-gray-500">
              <Activity className="h-4 w-4 text-green-500 mr-1" />
              <span>Canlı İzleme Aktif</span>
            </div>
            <div className="text-sm text-gray-500">
              {new Date().toLocaleString('tr-TR')}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}