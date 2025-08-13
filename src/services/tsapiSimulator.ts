import { Agent, AgentStatus, CallCenterStats, TSAPIEvent } from '../types/agent';

class TSAPISimulator {
  private agents: Agent[] = [];
  private stats: CallCenterStats;
  private events: TSAPIEvent[] = [];
  private eventListeners: ((event: TSAPIEvent) => void)[] = [];
  private statsListeners: ((stats: CallCenterStats) => void)[] = [];

  constructor() {
    this.initializeAgents();
    this.stats = this.calculateStats();
    this.startSimulation();
  }

  private initializeAgents() {
    const agentNames = [
      'Ahmet Yılmaz', 'Ayşe Kaya', 'Mehmet Demir', 'Fatma Şahin', 
      'Ali Özkan', 'Zeynep Arslan', 'Mustafa Çelik', 'Esra Doğan',
      'Hasan Yıldız', 'Selin Aydın', 'Emre Koç', 'Deniz Gürel'
    ];

    this.agents = agentNames.map((name, index) => ({
      id: `agent-${index + 1}`,
      name,
      extension: `${3000 + index}`,
      skillGroups: ['Satış', 'Teknik Destek', 'Müşteri Hizmetleri'].slice(0, Math.floor(Math.random() * 3) + 1),
      status: this.getRandomStatus(),
      lastStatusChange: new Date(Date.now() - Math.random() * 3600000),
      totalCallsToday: Math.floor(Math.random() * 50),
      totalTalkTime: Math.floor(Math.random() * 28800), // seconds
      totalIdleTime: Math.floor(Math.random() * 7200), // seconds
      currentCallDuration: Math.random() > 0.7 ? Math.floor(Math.random() * 1800) : undefined
    }));
  }

  private getRandomStatus(): AgentStatus {
    const statuses: AgentStatus[] = ['available', 'busy', 'acw', 'not-ready', 'on-call'];
    return statuses[Math.floor(Math.random() * statuses.length)];
  }

  private calculateStats(): CallCenterStats {
    const loggedOnAgents = this.agents.filter(a => a.status !== 'logged-off');
    
    return {
      totalAgents: this.agents.length,
      agentsLoggedOn: loggedOnAgents.length,
      agentsAvailable: this.agents.filter(a => a.status === 'available').length,
      agentsBusy: this.agents.filter(a => a.status === 'busy' || a.status === 'on-call').length,
      agentsInACW: this.agents.filter(a => a.status === 'acw').length,
      agentsNotReady: this.agents.filter(a => a.status === 'not-ready').length,
      callsInQueue: Math.floor(Math.random() * 20),
      averageWaitTime: Math.floor(Math.random() * 120),
      longestWaitTime: Math.floor(Math.random() * 300),
      callsAnswered: Math.floor(Math.random() * 500) + 100,
      callsAbandoned: Math.floor(Math.random() * 50),
      serviceLevel: Math.floor(Math.random() * 20) + 80
    };
  }

  private startSimulation() {
    // Simulate agent status changes every 5-15 seconds
    setInterval(() => {
      const agent = this.agents[Math.floor(Math.random() * this.agents.length)];
      const oldStatus = agent.status;
      const newStatus = this.getRandomStatus();
      
      if (oldStatus !== newStatus) {
        agent.status = newStatus;
        agent.lastStatusChange = new Date();
        
        if (newStatus === 'on-call') {
          agent.currentCallDuration = 0;
        } else if (oldStatus === 'on-call') {
          agent.currentCallDuration = undefined;
          agent.totalCallsToday++;
        }

        const event: TSAPIEvent = {
          id: `event-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
          timestamp: new Date(),
          type: 'AgentStateChanged',
          agentId: agent.id,
          oldState: oldStatus,
          newState: newStatus,
          details: `Agent ${agent.name} changed from ${oldStatus} to ${newStatus}`
        };

        this.events.unshift(event);
        this.events = this.events.slice(0, 100); // Keep last 100 events

        this.eventListeners.forEach(listener => listener(event));
      }

      // Update call durations for agents on call
      this.agents.forEach(agent => {
        if (agent.status === 'on-call' && agent.currentCallDuration !== undefined) {
          agent.currentCallDuration++;
        }
      });

      this.stats = this.calculateStats();
      this.statsListeners.forEach(listener => listener(this.stats));
    }, Math.random() * 10000 + 5000);

    // Update current call durations every second
    setInterval(() => {
      this.agents.forEach(agent => {
        if (agent.status === 'on-call' && agent.currentCallDuration !== undefined) {
          agent.currentCallDuration++;
        }
      });
    }, 1000);
  }

  public getAgents(): Agent[] {
    return [...this.agents];
  }

  public getStats(): CallCenterStats {
    return { ...this.stats };
  }

  public getEvents(): TSAPIEvent[] {
    return [...this.events];
  }

  public onEvent(listener: (event: TSAPIEvent) => void) {
    this.eventListeners.push(listener);
    return () => {
      const index = this.eventListeners.indexOf(listener);
      if (index > -1) {
        this.eventListeners.splice(index, 1);
      }
    };
  }

  public onStatsUpdate(listener: (stats: CallCenterStats) => void) {
    this.statsListeners.push(listener);
    return () => {
      const index = this.statsListeners.indexOf(listener);
      if (index > -1) {
        this.statsListeners.splice(index, 1);
      }
    };
  }
}

export const tsapiSimulator = new TSAPISimulator();