import React from 'react';
import { Wifi, WifiOff } from 'lucide-react';

interface ConnectionStatusProps {
  isConnected: boolean;
}

export function ConnectionStatus({ isConnected }: ConnectionStatusProps) {
  return (
    <div className={`flex items-center px-3 py-2 rounded-lg text-sm font-medium ${
      isConnected 
        ? 'bg-green-100 text-green-800' 
        : 'bg-red-100 text-red-800'
    }`}>
      {isConnected ? (
        <>
          <Wifi className="h-4 w-4 mr-2" />
          TSAPI Bağlantısı Aktif
        </>
      ) : (
        <>
          <WifiOff className="h-4 w-4 mr-2" />
          TSAPI Bağlantısı Kesildi
        </>
      )}
    </div>
  );
}