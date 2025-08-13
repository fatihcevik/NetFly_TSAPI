import { useState, useEffect } from 'react';
import { Agent, CallCenterStats, TSAPIEvent } from '../types/agent';
import { tsapiSimulator } from '../services/tsapiSimulator';
import { realTsapiClient } from '../services/realTsapiClient';

export function useTSAPI() {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [stats, setStats] = useState<CallCenterStats | null>(null);
  const [events, setEvents] = useState<TSAPIEvent[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  
  // Toggle between simulator and real TSAPI client
  const useRealTSAPI = import.meta.env.VITE_USE_REAL_TSAPI === 'true';

  useEffect(() => {
    if (useRealTSAPI) {
      // Use real TSAPI client
      const initializeRealTSAPI = async () => {
        try {
          const [agentsData, statsData, eventsData] = await Promise.all([
            realTsapiClient.getAgents(),
            realTsapiClient.getStats(),
            realTsapiClient.getEvents()
          ]);
          
          setAgents(agentsData);
          setStats(statsData);
          setEvents(eventsData);
        } catch (error) {
          console.error('Failed to initialize real TSAPI data:', error);
        }
      };

      initializeRealTSAPI();

      // Subscribe to real-time updates
      const unsubscribeEvents = realTsapiClient.onEvent((event) => {
        setEvents(prevEvents => [event, ...prevEvents.slice(0, 99)]);
      });

      const unsubscribeStats = realTsapiClient.onStatsUpdate((newStats) => {
        setStats(newStats);
      });

      const unsubscribeConnection = realTsapiClient.onConnectionChange((connected) => {
        setIsConnected(connected);
      });

      return () => {
        unsubscribeEvents();
        unsubscribeStats();
        unsubscribeConnection();
      };
    } else {
      // Use simulator (existing code)
      setAgents(tsapiSimulator.getAgents());
      setStats(tsapiSimulator.getStats());
      setEvents(tsapiSimulator.getEvents());
      setIsConnected(true);

      const unsubscribeEvents = tsapiSimulator.onEvent((event) => {
        setEvents(prevEvents => [event, ...prevEvents.slice(0, 99)]);
        setAgents(tsapiSimulator.getAgents());
      });

      const unsubscribeStats = tsapiSimulator.onStatsUpdate((newStats) => {
        setStats(newStats);
      });

      const interval = setInterval(() => {
        setAgents(tsapiSimulator.getAgents());
      }, 1000);

      return () => {
        unsubscribeEvents();
        unsubscribeStats();
        clearInterval(interval);
      };
    }
  }, [useRealTSAPI]);

  return {
    agents,
    stats,
    events,
    isConnected,
    isUsingRealTSAPI: useRealTSAPI
  };
}