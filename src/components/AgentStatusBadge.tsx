import React from 'react';
import { AgentStatus } from '../types/agent';

interface AgentStatusBadgeProps {
  status: AgentStatus;
}

const statusConfig = {
  'logged-off': { label: 'Çıkış Yapmış', color: 'bg-gray-500 text-white' },
  'logged-on': { label: 'Giriş Yapmış', color: 'bg-blue-500 text-white' },
  'available': { label: 'Müsait', color: 'bg-green-500 text-white' },
  'busy': { label: 'Meşgul', color: 'bg-red-500 text-white' },
  'acw': { label: 'Çağrı Sonrası', color: 'bg-yellow-500 text-white' },
  'not-ready': { label: 'Hazır Değil', color: 'bg-orange-500 text-white' },
  'on-call': { label: 'Aramada', color: 'bg-purple-500 text-white' }
};

export function AgentStatusBadge({ status }: AgentStatusBadgeProps) {
  const config = statusConfig[status];
  
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.color}`}>
      <span className="w-2 h-2 bg-current rounded-full mr-1.5 animate-pulse"></span>
      {config.label}
    </span>
  );
}