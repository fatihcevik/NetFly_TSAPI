import React from 'react';
import { Header } from './components/Header';
import { StatsOverview } from './components/StatsOverview';
import { AgentTable } from './components/AgentTable';
import { EventLog } from './components/EventLog';
import { RealTimeChart } from './components/RealTimeChart';
import { ConnectionStatus } from './components/ConnectionStatus';
import { useTSAPI } from './hooks/useTSAPI';

function App() {
  const { agents, stats, events, isConnected } = useTSAPI();

  if (!stats) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">TSAPI bağlantısı kuruluyor...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      
      <main className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <div className="mb-6 flex justify-between items-center">
          <h2 className="text-2xl font-bold text-gray-900">Çağrı Merkezi Dashboard</h2>
          <ConnectionStatus isConnected={isConnected} />
        </div>

        {/* Stats Overview */}
        <div className="mb-8">
          <StatsOverview stats={stats} />
        </div>

        {/* Main Content Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Agent Table - Takes 2 columns on large screens */}
          <div className="lg:col-span-2">
            <AgentTable agents={agents} />
          </div>
          
          {/* Right Sidebar */}
          <div className="space-y-6">
            <RealTimeChart stats={stats} />
            <EventLog events={events} />
          </div>
        </div>
      </main>
    </div>
  );
}

export default App;