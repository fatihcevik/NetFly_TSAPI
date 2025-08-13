import React, { useState, useEffect } from 'react';
import { CallCenterStats } from '../types/agent';
import { BarChart3 } from 'lucide-react';

interface RealTimeChartProps {
  stats: CallCenterStats;
}

interface DataPoint {
  timestamp: string;
  available: number;
  busy: number;
  acw: number;
}

export function RealTimeChart({ stats }: RealTimeChartProps) {
  const [dataPoints, setDataPoints] = useState<DataPoint[]>([]);

  useEffect(() => {
    const newDataPoint: DataPoint = {
      timestamp: new Date().toLocaleTimeString('tr-TR', { 
        hour: '2-digit', 
        minute: '2-digit' 
      }),
      available: stats.agentsAvailable,
      busy: stats.agentsBusy,
      acw: stats.agentsInACW
    };

    setDataPoints(prev => [...prev.slice(-11), newDataPoint]);
  }, [stats]);

  const maxValue = Math.max(...dataPoints.map(d => Math.max(d.available, d.busy, d.acw))) || 1;

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center mb-4">
        <BarChart3 className="h-5 w-5 text-gray-600 mr-2" />
        <h3 className="text-lg font-semibold text-gray-900">Gerçek Zamanlı Durum Grafiği</h3>
      </div>
      
      <div className="space-y-4">
        <div className="flex justify-center space-x-6 text-sm">
          <div className="flex items-center">
            <div className="w-3 h-3 bg-green-500 rounded mr-2"></div>
            <span>Müsait</span>
          </div>
          <div className="flex items-center">
            <div className="w-3 h-3 bg-red-500 rounded mr-2"></div>
            <span>Meşgul</span>
          </div>
          <div className="flex items-center">
            <div className="w-3 h-3 bg-yellow-500 rounded mr-2"></div>
            <span>Çağrı Sonrası</span>
          </div>
        </div>
        
        <div className="h-32 flex items-end justify-between space-x-1">
          {dataPoints.map((point, index) => (
            <div key={index} className="flex flex-col items-center flex-1">
              <div className="flex flex-col justify-end h-24 w-full space-y-1">
                <div 
                  className="bg-green-500 rounded-t"
                  style={{ 
                    height: `${(point.available / maxValue) * 100}%`,
                    minHeight: point.available > 0 ? '2px' : '0'
                  }}
                ></div>
                <div 
                  className="bg-red-500"
                  style={{ 
                    height: `${(point.busy / maxValue) * 100}%`,
                    minHeight: point.busy > 0 ? '2px' : '0'
                  }}
                ></div>
                <div 
                  className="bg-yellow-500 rounded-b"
                  style={{ 
                    height: `${(point.acw / maxValue) * 100}%`,
                    minHeight: point.acw > 0 ? '2px' : '0'
                  }}
                ></div>
              </div>
              <span className="text-xs text-gray-500 mt-1 transform -rotate-45 origin-bottom-left">
                {point.timestamp}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}