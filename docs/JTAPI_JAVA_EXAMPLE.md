# JTAPI Java Implementation Example

## Overview
Java JTAPI (Java Telephony API) is another option for integrating with Avaya systems. It provides a Java-based interface to telephony services and can be easier to work with than native TSAPI in some environments.

## Prerequisites
- Java 8 or higher
- Avaya JTAPI libraries (from Avaya)
- Access to Avaya AES server
- Proper JTAPI licensing

## Java JTAPI Service Implementation

### 1. Maven Dependencies
```xml
<!-- pom.xml -->
<project>
    <modelVersion>4.0.0</modelVersion>
    <groupId>com.company</groupId>
    <artifactId>jtapi-service</artifactId>
    <version>1.0.0</version>
    
    <properties>
        <maven.compiler.source>11</maven.compiler.source>
        <maven.compiler.target>11</maven.compiler.target>
    </properties>
    
    <dependencies>
        <!-- Avaya JTAPI - These JARs come from Avaya installation -->
        <dependency>
            <groupId>com.avaya</groupId>
            <artifactId>jtapi</artifactId>
            <version>8.1</version>
            <scope>system</scope>
            <systemPath>${project.basedir}/lib/jtapi.jar</systemPath>
        </dependency>
        
        <dependency>
            <groupId>com.avaya</groupId>
            <artifactId>tsapi</artifactId>
            <version>8.1</version>
            <scope>system</scope>
            <systemPath>${project.basedir}/lib/tsapi.jar</systemPath>
        </dependency>
        
        <!-- Spring Boot for REST API -->
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-web</artifactId>
            <version>2.7.0</version>
        </dependency>
        
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-websocket</artifactId>
            <version>2.7.0</version>
        </dependency>
        
        <!-- JSON processing -->
        <dependency>
            <groupId>com.fasterxml.jackson.core</groupId>
            <artifactId>jackson-databind</artifactId>
            <version>2.13.3</version>
        </dependency>
        
        <!-- Logging -->
        <dependency>
            <groupId>org.slf4j</groupId>
            <artifactId>slf4j-api</artifactId>
            <version>1.7.36</version>
        </dependency>
    </dependencies>
</project>
```

### 2. JTAPI Service Implementation
```java
// JTAPIService.java
package com.company.jtapi.service;

import javax.telephony.*;
import javax.telephony.callcenter.*;
import com.avaya.jtapi.tsapi.*;
import org.springframework.stereotype.Service;
import org.springframework.beans.factory.annotation.Value;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArrayList;

@Service
public class JTAPIService implements CallCenterProviderObserver {
    
    private static final Logger logger = LoggerFactory.getLogger(JTAPIService.class);
    
    @Value("${jtapi.server.url}")
    private String serverUrl;
    
    @Value("${jtapi.username}")
    private String username;
    
    @Value("${jtapi.password}")
    private String password;
    
    private Provider provider;
    private boolean isConnected = false;
    private final Map<String, Agent> agents = new ConcurrentHashMap<>();
    private final List<AgentEvent> recentEvents = new CopyOnWriteArrayList<>();
    private final List<AgentEventListener> eventListeners = new CopyOnWriteArrayList<>();
    
    public boolean connect() {
        try {
            logger.info("Connecting to JTAPI server: {}", serverUrl);
            
            // Get JTAPI Peer
            JtapiPeer peer = JtapiPeerFactory.getJtapiPeer();
            
            // Create provider string
            String providerString = String.format("%s;login=%s;passwd=%s", 
                serverUrl, username, password);
            
            // Get provider
            provider = peer.getProvider(providerString);
            
            // Add observer for events
            provider.addObserver(this);
            
            // Initialize agent monitoring
            initializeAgentMonitoring();
            
            isConnected = true;
            logger.info("JTAPI connection established successfully");
            return true;
            
        } catch (Exception e) {
            logger.error("Failed to connect to JTAPI server", e);
            return false;
        }
    }
    
    private void initializeAgentMonitoring() throws Exception {
        // Get all terminals (agent devices)
        Terminal[] terminals = provider.getTerminals();
        
        for (Terminal terminal : terminals) {
            if (terminal instanceof AgentTerminal) {
                AgentTerminal agentTerminal = (AgentTerminal) terminal;
                
                // Add observer for agent events
                agentTerminal.addObserver(this);
                
                // Create agent object
                Agent agent = new Agent();
                agent.setId(agentTerminal.getName());
                agent.setExtension(agentTerminal.getName());
                agent.setName("Agent " + agentTerminal.getName());
                agent.setStatus(mapAgentState(agentTerminal.getAgentState()));
                agent.setLastStatusChange(new Date());
                
                agents.put(agent.getId(), agent);
                
                logger.debug("Monitoring agent terminal: {}", agentTerminal.getName());
            }
        }
        
        logger.info("Initialized monitoring for {} agents", agents.size());
    }
    
    @Override
    public void providerEventTransmissionEnded(ProviderEvent[] events) {
        for (ProviderEvent event : events) {
            handleProviderEvent(event);
        }
    }
    
    private void handleProviderEvent(ProviderEvent event) {
        try {
            if (event instanceof AgentTerminalEvent) {
                handleAgentTerminalEvent((AgentTerminalEvent) event);
            } else if (event instanceof CallCenterCallEvent) {
                handleCallCenterEvent((CallCenterCallEvent) event);
            }
        } catch (Exception e) {
            logger.error("Error handling provider event", e);
        }
    }
    
    private void handleAgentTerminalEvent(AgentTerminalEvent event) {
        AgentTerminal agentTerminal = (AgentTerminal) event.getTerminal();
        String agentId = agentTerminal.getName();
        
        Agent agent = agents.get(agentId);
        if (agent == null) {
            agent = new Agent();
            agent.setId(agentId);
            agent.setExtension(agentId);
            agent.setName("Agent " + agentId);
            agents.put(agentId, agent);
        }
        
        String oldState = agent.getStatus();
        String newState = mapAgentState(agentTerminal.getAgentState());
        
        if (!Objects.equals(oldState, newState)) {
            agent.setStatus(newState);
            agent.setLastStatusChange(new Date());
            
            // Create event
            AgentEvent agentEvent = new AgentEvent();
            agentEvent.setId(UUID.randomUUID().toString());
            agentEvent.setTimestamp(new Date());
            agentEvent.setType(getEventType(event));
            agentEvent.setAgentId(agentId);
            agentEvent.setOldState(oldState);
            agentEvent.setNewState(newState);
            agentEvent.setDetails(String.format("Agent %s changed from %s to %s", 
                agentId, oldState, newState));
            
            // Add to recent events
            recentEvents.add(0, agentEvent);
            if (recentEvents.size() > 100) {
                recentEvents.remove(recentEvents.size() - 1);
            }
            
            // Notify listeners
            notifyEventListeners(agentEvent);
            
            logger.info("Agent {} state changed: {} -> {}", agentId, oldState, newState);
        }
    }
    
    private void handleCallCenterEvent(CallCenterCallEvent event) {
        // Handle call center specific events
        // Update call statistics, queue information, etc.
        logger.debug("Call center event: {}", event.getClass().getSimpleName());
    }
    
    private String mapAgentState(int jtapiState) {
        switch (jtapiState) {
            case Agent.LOG_IN:
                return "logged-on";
            case Agent.LOG_OUT:
                return "logged-off";
            case Agent.NOT_READY:
                return "not-ready";
            case Agent.READY:
                return "available";
            case Agent.WORK_NOT_READY:
                return "busy";
            case Agent.WORK_READY:
                return "acw";
            case Agent.BUSY:
                return "on-call";
            default:
                return "unknown";
        }
    }
    
    private String getEventType(AgentTerminalEvent event) {
        if (event instanceof AgentTerminalLoggedOnEvent) {
            return "AgentLoggedOn";
        } else if (event instanceof AgentTerminalLoggedOffEvent) {
            return "AgentLoggedOff";
        } else if (event instanceof AgentTerminalStateChangedEvent) {
            return "AgentStateChanged";
        } else {
            return "AgentWorkMode";
        }
    }
    
    public void disconnect() {
        try {
            if (provider != null && isConnected) {
                provider.removeObserver(this);
                provider.shutdown();
                isConnected = false;
                logger.info("JTAPI connection closed");
            }
        } catch (Exception e) {
            logger.error("Error disconnecting from JTAPI", e);
        }
    }
    
    // Public API methods
    public boolean isConnected() {
        return isConnected;
    }
    
    public List<Agent> getAgents() {
        return new ArrayList<>(agents.values());
    }
    
    public Agent getAgent(String agentId) {
        return agents.get(agentId);
    }
    
    public List<AgentEvent> getRecentEvents() {
        return new ArrayList<>(recentEvents);
    }
    
    public CallCenterStats getCallCenterStats() {
        CallCenterStats stats = new CallCenterStats();
        
        List<Agent> agentList = getAgents();
        stats.setTotalAgents(agentList.size());
        stats.setAgentsLoggedOn((int) agentList.stream()
            .filter(a -> !"logged-off".equals(a.getStatus())).count());
        stats.setAgentsAvailable((int) agentList.stream()
            .filter(a -> "available".equals(a.getStatus())).count());
        stats.setAgentsBusy((int) agentList.stream()
            .filter(a -> "busy".equals(a.getStatus()) || "on-call".equals(a.getStatus())).count());
        stats.setAgentsInACW((int) agentList.stream()
            .filter(a -> "acw".equals(a.getStatus())).count());
        stats.setAgentsNotReady((int) agentList.stream()
            .filter(a -> "not-ready".equals(a.getStatus())).count());
        
        // Additional stats would come from call center queries
        stats.setCallsInQueue(0); // Query from call center
        stats.setAverageWaitTime(0);
        stats.setServiceLevel(95);
        
        return stats;
    }
    
    // Event listener management
    public void addEventListener(AgentEventListener listener) {
        eventListeners.add(listener);
    }
    
    public void removeEventListener(AgentEventListener listener) {
        eventListeners.remove(listener);
    }
    
    private void notifyEventListeners(AgentEvent event) {
        for (AgentEventListener listener : eventListeners) {
            try {
                listener.onAgentEvent(event);
            } catch (Exception e) {
                logger.error("Error notifying event listener", e);
            }
        }
    }
}
```

### 3. REST API Controller
```java
// JTAPIController.java
package com.company.jtapi.controller;

import com.company.jtapi.service.JTAPIService;
import com.company.jtapi.model.*;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;
import java.util.HashMap;

@RestController
@RequestMapping("/api")
@CrossOrigin(origins = "*")
public class JTAPIController {
    
    @Autowired
    private JTAPIService jtapiService;
    
    @GetMapping("/health")
    public ResponseEntity<Map<String, Object>> getHealth() {
        Map<String, Object> health = new HashMap<>();
        health.put("status", "ok");
        health.put("connected", jtapiService.isConnected());
        health.put("timestamp", System.currentTimeMillis());
        return ResponseEntity.ok(health);
    }
    
    @GetMapping("/agents")
    public ResponseEntity<List<Agent>> getAgents() {
        return ResponseEntity.ok(jtapiService.getAgents());
    }
    
    @GetMapping("/agents/{id}")
    public ResponseEntity<Agent> getAgent(@PathVariable String id) {
        Agent agent = jtapiService.getAgent(id);
        if (agent == null) {
            return ResponseEntity.notFound().build();
        }
        return ResponseEntity.ok(agent);
    }
    
    @GetMapping("/agents/status")
    public ResponseEntity<List<Agent>> getAgentStatus() {
        return ResponseEntity.ok(jtapiService.getAgents());
    }
    
    @GetMapping("/stats")
    public ResponseEntity<CallCenterStats> getStats() {
        return ResponseEntity.ok(jtapiService.getCallCenterStats());
    }
    
    @GetMapping("/events/recent")
    public ResponseEntity<List<AgentEvent>> getRecentEvents() {
        return ResponseEntity.ok(jtapiService.getRecentEvents());
    }
}
```

### 4. WebSocket Configuration
```java
// WebSocketConfig.java
package com.company.jtapi.config;

import com.company.jtapi.service.JTAPIService;
import com.company.jtapi.websocket.AgentEventWebSocketHandler;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.socket.config.annotation.*;

@Configuration
@EnableWebSocket
public class WebSocketConfig implements WebSocketConfigurer {
    
    @Autowired
    private JTAPIService jtapiService;
    
    @Override
    public void registerWebSocketHandlers(WebSocketHandlerRegistry registry) {
        registry.addHandler(new AgentEventWebSocketHandler(jtapiService), "/ws/events")
                .setAllowedOrigins("*");
    }
}
```

### 5. Application Configuration
```properties
# application.properties
server.port=8080

# JTAPI Configuration
jtapi.server.url=your-aes-server:450
jtapi.username=jtapi_user
jtapi.password=jtapi_password

# Logging
logging.level.com.company.jtapi=DEBUG
logging.level.root=INFO
```

### 6. Spring Boot Application
```java
// JTAPIApplication.java
package com.company.jtapi;

import com.company.jtapi.service.JTAPIService;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.ConfigurableApplicationContext;

@SpringBootApplication
public class JTAPIApplication {
    
    public static void main(String[] args) {
        ConfigurableApplicationContext context = SpringApplication.run(JTAPIApplication.class, args);
        
        // Initialize JTAPI connection
        JTAPIService jtapiService = context.getBean(JTAPIService.class);
        
        // Add shutdown hook
        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            jtapiService.disconnect();
        }));
        
        // Connect to JTAPI
        if (jtapiService.connect()) {
            System.out.println("JTAPI Service started successfully");
        } else {
            System.err.println("Failed to start JTAPI Service");
            System.exit(1);
        }
    }
}
```

## Building and Running

### 1. Build the Application
```bash
# Copy Avaya JTAPI JARs to lib/ directory
mkdir lib
cp /path/to/avaya/jtapi/*.jar lib/

# Build with Maven
mvn clean package

# Or build with Gradle
./gradlew build
```

### 2. Run as Service
```bash
# Run directly
java -jar target/jtapi-service-1.0.0.jar

# Or install as system service (Linux)
sudo systemctl enable jtapi-service
sudo systemctl start jtapi-service
```

### 3. Test the API
```bash
# Test health endpoint
curl http://localhost:8080/api/health

# Get agents
curl http://localhost:8080/api/agents

# Get statistics
curl http://localhost:8080/api/stats
```

## Advantages of JTAPI over TSAPI

1. **Platform Independence**: Java runs on multiple platforms
2. **Memory Management**: Automatic garbage collection
3. **Exception Handling**: Better error handling mechanisms
4. **Integration**: Easier integration with Java ecosystem
5. **Maintenance**: Easier to maintain and debug

## Considerations

1. **Performance**: May have slightly higher overhead than native TSAPI
2. **Licensing**: Still requires Avaya JTAPI licensing
3. **Dependencies**: Requires Java runtime environment
4. **Library Availability**: JTAPI libraries must be obtained from Avaya

This JTAPI implementation provides a robust, maintainable solution for integrating with Avaya systems while exposing clean REST APIs for your Node.js application to consume.