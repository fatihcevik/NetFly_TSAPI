export interface Agent {
  id: string;
  name: string;
  extension: string;
  skillGroups: string[];
  status: AgentStatus;
  lastStatusChange: Date;
  totalCallsToday: number;
  totalTalkTime: number;
  totalIdleTime: number;
  currentCallDuration?: number;
}

export type AgentStatus = 
  | 'logged-off'
  | 'logged-on'
  | 'available'
  | 'busy'
  | 'acw'
  | 'not-ready'
  | 'on-call';

export interface CallCenterStats {
  totalAgents: number;
  agentsLoggedOn: number;
  agentsAvailable: number;
  agentsBusy: number;
  agentsInACW: number;
  agentsNotReady: number;
  callsInQueue: number;
  averageWaitTime: number;
  longestWaitTime: number;
  callsAnswered: number;
  callsAbandoned: number;
  serviceLevel: number;
}

export interface TSAPIEvent {
  id: string;
  timestamp: Date;
  type: 'AgentLoggedOn' | 'AgentLoggedOff' | 'AgentWorkMode' | 'AgentStateChanged';
  agentId: string;
  oldState?: AgentStatus;
  newState: AgentStatus;
  details?: string;
}