import React from 'react';
import { CallCenterStats } from '../types/agent';
import { StatCard } from './StatCard';
import { Users, UserCheck, UserX, Clock, Phone, TrendingUp } from 'lucide-react';

interface StatsOverviewProps {
  stats: CallCenterStats;
}

export function StatsOverview({ stats }: StatsOverviewProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <StatCard
        title="Toplam Temsilci"
        value={stats.totalAgents}
        color="gray"
        icon={<Users className="h-6 w-6" />}
      />
      
      <StatCard
        title="Giriş Yapmış"
        value={stats.agentsLoggedOn}
        subtitle={`${stats.totalAgents} temsilciden`}
        color="blue"
        icon={<UserCheck className="h-6 w-6" />}
      />
      
      <StatCard
        title="Müsait"
        value={stats.agentsAvailable}
        color="green"
        icon={<UserCheck className="h-6 w-6" />}
      />
      
      <StatCard
        title="Meşgul/Aramada"
        value={stats.agentsBusy}
        color="red"
        icon={<Phone className="h-6 w-6" />}
      />
      
      <StatCard
        title="Çağrı Sonrası"
        value={stats.agentsInACW}
        color="yellow"
        icon={<Clock className="h-6 w-6" />}
      />
      
      <StatCard
        title="Hazır Değil"
        value={stats.agentsNotReady}
        color="purple"
        icon={<UserX className="h-6 w-6" />}
      />
      
      <StatCard
        title="Kuyrukta Bekleyen"
        value={stats.callsInQueue}
        subtitle={`Ort. bekleme: ${stats.averageWaitTime}s`}
        color="blue"
        icon={<Phone className="h-6 w-6" />}
      />
      
      <StatCard
        title="Hizmet Seviyesi"
        value={`${stats.serviceLevel}%`}
        subtitle="Günlük performans"
        color="green"
        icon={<TrendingUp className="h-6 w-6" />}
      />
    </div>
  );
}