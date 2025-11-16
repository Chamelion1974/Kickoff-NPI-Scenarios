# Shop Steward Hub Digital Twin - Architecture Overview

## Executive Summary
A real-time 3D digital twin platform that merges manufacturing operations management with immersive visualization, enabling strategic planning, NPI process optimization, and bottleneck identification through interactive simulation.

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER (UE5)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│  │ 3D Shop Floor│  │  Interactive │  │  CNC Simulation    │   │
│  │ Visualization│  │  Dashboard   │  │  & Tool Paths      │   │
│  └──────────────┘  └──────────────┘  └────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ Blueprint Interfaces
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    INTEGRATION LAYER (C#)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│  │  REST API    │  │  WebSocket   │  │  Data Transform    │   │
│  │  Gateway     │  │  Hub         │  │  & Validation      │   │
│  └──────────────┘  └──────────────┘  └────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ HTTP/WS
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATA LAYER (Docker Services)                  │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│  │  TimeSeries  │  │  PostgreSQL  │  │  Redis Cache       │   │
│  │  Database    │  │  (Metadata)  │  │  (Real-time Data)  │   │
│  └──────────────┘  └──────────────┘  └────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ Data Ingestion
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    SOURCE SYSTEMS                                │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│  │ Machine Tool │  │  ERP/MES     │  │  Quality Systems   │   │
│  │ Controllers  │  │  Systems     │  │  & Sensors         │   │
│  └──────────────┘  └──────────────┘  └────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## Core Components

### 1. UE5 Presentation Layer

#### Modular Shop Layout System
- **Blueprint-Based Department Modules**: Each department (CNC Mill, Lathes, Programming, etc.) is a self-contained actor that can be instantiated, positioned, and scaled
- **Dynamic Resizing**: Departments adapt to physical space constraints while maintaining realistic proportions
- **Snap-to-Grid System**: Intelligent placement system for optimal layout planning
- **Material Library**: Realistic materials for machines, flooring, walls using UE5's physically-based rendering

#### Digital Twin Visualization
- **Real-time Data Binding**: Live connection to machine status, job queues, and operator actions
- **Heat Maps**: Visual overlays showing utilization, bottlenecks, and efficiency metrics
- **Timeline Scrubbing**: Ability to replay historical data or simulate future scenarios
- **Comparison Mode**: Side-by-side view of estimated vs. actual timelines

#### CNC Simulation Engine
- **Material Removal Simulation**: Physics-based cutting simulation using Chaos Physics
- **Tool Path Visualization**: 3D representation of G-code with collision detection
- **Robot Cell Integration**: Simulate loading/unloading automation
- **Training Mode**: Interactive operator training scenarios

### 2. C# Integration Layer (This Project)

#### API Gateway Component
```csharp
namespace ShopStewardHub.DigitalTwin
{
    // REST API for UE5 to query shop data
    public sealed class ShopDataGateway
    {
        // Department layout configuration
        // Machine status and telemetry
        // Job routing and NPI workflow
        // Performance metrics
    }

    // WebSocket hub for real-time updates
    public sealed class RealtimeDataHub
    {
        // Push notifications for state changes
        // Streaming sensor data
        // Alert notifications
    }

    // Data transformation layer
    public sealed class DataTransformService
    {
        // Convert ERP/MES data to UE5-friendly format
        // Aggregate and cache frequently accessed data
        // Validate and sanitize inputs
    }
}
```

#### Key Responsibilities
- Expose RESTful endpoints for UE5 HTTP requests
- Manage WebSocket connections for streaming data
- Transform manufacturing data into 3D-ready formats
- Handle authentication and rate limiting
- Cache frequently accessed data in Redis

### 3. Docker Services Layer

#### Container Architecture
```yaml
services:
  # Time-series database for sensor/machine data
  timescaledb:
    - Store machine telemetry
    - Job progress tracking
    - Historical performance data

  # PostgreSQL for metadata
  postgres:
    - Department configurations
    - Machine definitions
    - User permissions
    - NPI workflow definitions

  # Redis for real-time caching
  redis:
    - Current machine states
    - Active job queues
    - Session management
    - PubSub for events

  # Integration layer API
  api-gateway:
    - C# WinRT component hosted service
    - Exposes REST and WebSocket endpoints
    - Connects to all data sources
```

## Modular Department System

Each department is a self-contained module with:

### Department Structure
```
DepartmentBase (Blueprint)
├── PhysicalBounds (resizable)
├── MachineSlots (array of machine instances)
├── WorkflowConnections (links to other departments)
├── UtilizationMetrics (real-time data display)
└── VisualEffects (status indicators, heat maps)
```

### Implemented Departments
1. **CNC Mill Department**: Multi-axis mills with tool changers
2. **CNC Lathe Department**: Turning centers with bar feeders
3. **Programming Room**: CAM workstations, verification stations
4. **Saw Area**: Material cutting and prep stations
5. **Shipping/Receiving**: Loading docks, inspection areas
6. **Deburr**: Manual and automated deburring stations
7. **Parts Cleaning**: Washing systems, drying stations
8. **Front Office**: Administrative and planning areas
9. **Tool Crib**: Tool storage, presetting, management

## NPI Workflow Visualization

### Timeline Mode
- Visual representation of each process step
- Estimated vs. actual time comparison
- Critical path highlighting
- Dependency visualization

### Real-time Mode
- Live tracking of job location
- Current operation status
- Operator assignments
- Queue positions

### Simulation Mode
- "What-if" scenario modeling
- Capacity planning
- Bottleneck identification
- Resource optimization

## Data Flow Architecture

### Inbound Data Sources
1. **Machine Controllers**: Direct connection to CNC controllers for real-time status
2. **ERP/MES Systems**: Job orders, routing, material requirements
3. **Quality Systems**: Inspection results, first article data
4. **IoT Sensors**: Environmental, vibration, power consumption
5. **Manual Input**: Operator logs, downtime reasons

### Data Transformation Pipeline
```
Raw Data → Validation → Normalization → Enrichment → Cache → UE5
```

### Outbound Interfaces
1. **UE5 HTTP Requests**: On-demand data queries
2. **WebSocket Streams**: Push updates for state changes
3. **Batch Exports**: Historical data for analysis
4. **Alert Systems**: Email/SMS for critical events

## Performance Optimization Strategy

### For Threshold Hardware

#### UE5 Settings
- **Nanite**: Use selectively for static machinery only
- **Lumen**: Disable for real-time view; enable for presentations
- **Scalability Groups**: Implement Low/Medium/High/Cinematic presets
- **Level of Detail (LOD)**: Aggressive LOD chains for all assets
- **Instanced Meshes**: Use instancing for repeated machines

#### Blueprint Optimization
- Minimize Tick events; use timers instead
- Event-driven updates rather than polling
- Async loading for department modules
- Culling for off-screen departments

#### Data Optimization
- Local caching of static data in UE5
- Throttled update rates (1Hz for most metrics)
- Delta updates (only changed values)
- Compressed WebSocket messages

## Development Phases

### Phase 1: Foundation (Weeks 1-4)
- Set up UE5 project structure
- Create basic department module system
- Build C# API gateway skeleton
- Configure Docker development environment
- Establish CI/CD pipeline

### Phase 2: Core Visualization (Weeks 5-8)
- Implement all 9 department modules
- Create machine asset library
- Build layout editor interface
- Develop real-time data binding system
- Implement basic UI overlays

### Phase 3: Data Integration (Weeks 9-12)
- Connect to sample data sources
- Implement REST API endpoints
- Set up WebSocket streaming
- Create data transformation layer
- Build caching strategy

### Phase 4: NPI Workflow (Weeks 13-16)
- Design workflow visualization system
- Implement timeline mode
- Build real-time tracking
- Create simulation engine
- Add comparison views

### Phase 5: CNC Simulation (Weeks 17-20)
- Integrate CAD import (Datasmith)
- Build tool path visualization
- Implement basic material removal
- Add collision detection
- Create training scenarios

### Phase 6: Polish & Optimization (Weeks 21-24)
- Performance profiling and optimization
- UI/UX refinement
- Documentation and training materials
- C-suite presentation templates
- Deployment packaging

## Security Considerations

### ITAR Compliance
- Visual boundaries for controlled areas
- Access control integration
- Audit logging for all interactions
- Data encryption in transit and at rest

### Network Security
- API authentication (OAuth 2.0)
- Rate limiting and DDoS protection
- Input validation and sanitization
- Secure WebSocket (WSS) only

## Technology Stack Summary

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Visualization | Unreal Engine 5.4+ | 3D rendering, simulation, UI |
| Scripting | Blueprint Visual Scripting | Game logic, data binding |
| Integration | C# (.NET 9.0) | API gateway, data transformation |
| API | ASP.NET Core | REST endpoints, WebSocket server |
| Caching | Redis | Real-time data, session management |
| Time-series DB | TimescaleDB | Sensor data, historical metrics |
| Metadata DB | PostgreSQL | Configuration, definitions |
| Containerization | Docker Desktop | Development environment |
| Version Control | Git | Source control |
| IDE | VS Code Insiders | Development environment |

## Success Metrics

1. **Performance**: Maintain 30+ FPS on threshold hardware with full shop visualization
2. **Accuracy**: <5% deviation between digital twin and real shop state
3. **Latency**: <500ms from event occurrence to visual update
4. **Scalability**: Support up to 50 concurrent machines
5. **Usability**: C-suite executives can navigate without training

## Future Expansion Possibilities

- VR mode for immersive shop walkthroughs
- AR overlays for on-floor guidance
- AI-driven optimization recommendations
- Integration with scheduling/planning systems
- Mobile companion app
- Multi-facility support
- Supplier/customer collaboration portals
