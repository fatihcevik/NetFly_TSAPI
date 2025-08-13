import React from 'react';
import { TSAPIEvent } from '../types/agent';
import { Activity, Clock } from 'lucide-react';

interface EventLogProps {
  events: TSAPIEvent[];
}

export function EventLog({ events }: EventLogProps) {
  const formatTime = (date: Date) => {
    return date.toLocaleTimeString('tr-TR', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  };

  const getEventTypeLabel = (type: string) => {
    const labels = {
      'AgentLoggedOn': 'Giriş Yaptı',
      'AgentLoggedOff': 'Çıkış Yaptı',
      'AgentWorkMode': 'Çalışma Modu',
      'AgentStateChanged': 'Durum Değişti'
    };
    return labels[type as keyof typeof labels] || type;
  };

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="px-6 py-4 bg-gray-50 border-b border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900 flex items-center">
          <Activity className="mr-2 h-5 w-5" />
          TSAPI Olayları
        </h3>
      </div>
      <div className="max-h-96 overflow-y-auto">
        {events.length === 0 ? (
          <div className="px-6 py-8 text-center text-gray-500">
            Henüz olay kaydı bulunmuyor
          </div>
        ) : (
          <div className="divide-y divide-gray-200">
            {events.map((event) => (
              <div key={event.id} className="px-6 py-4 hover:bg-gray-50 transition-colors duration-150">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center">
                      <span className="text-sm font-medium text-gray-900">
                        {getEventTypeLabel(event.type)}
                      </span>
                      <span className="ml-2 text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">
                        {event.agentId}
                      </span>
                    </div>
                    <p className="text-sm text-gray-600 mt-1">{event.details}</p>
                    {event.oldState && event.newState && (
                      <div className="flex items-center mt-2 text-xs text-gray-500">
                        <span className="bg-red-100 text-red-800 px-2 py-1 rounded mr-2">
                          {event.oldState}
                        </span>
                        <span className="mx-1">→</span>
                        <span className="bg-green-100 text-green-800 px-2 py-1 rounded">
                          {event.newState}
                        </span>
                      </div>
                    )}
                  </div>
                  <div className="flex items-center text-xs text-gray-500 ml-4">
                    <Clock className="mr-1 h-3 w-3" />
                    {formatTime(event.timestamp)}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}