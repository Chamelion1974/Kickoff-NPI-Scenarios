# Shop Steward Hub Digital Twin - Development Roadmap

## Quick Start Guide

### Prerequisites Checklist
- [x] Unreal Engine 5.4+ installed
- [x] VS Code Insiders installed
- [x] Docker Desktop installed
- [x] Claude Code available
- [ ] Git LFS installed (for UE5 assets)
- [ ] .NET 9.0 SDK installed
- [ ] Node.js (for tooling) installed

### Initial Setup (Day 1)

```bash
# 1. Install Git LFS for large UE5 assets
git lfs install

# 2. Verify .NET SDK
dotnet --version  # Should show 9.0.x

# 3. Verify Docker
docker --version
docker-compose --version

# 4. Create UE5 project structure
# (Done through UE5 Editor - see detailed steps below)
```

## Phase 1: Foundation (Weeks 1-4)

### Week 1: Project Structure Setup

#### Day 1-2: UE5 Project Creation
**Manual Steps (UE5 Editor):**
1. Launch Unreal Engine 5
2. Create New Project
   - Template: Blank
   - Project Type: Blueprint
   - Target Platform: Desktop
   - Quality Preset: Scalable
   - Starter Content: No (we'll create custom assets)
   - Raytracing: Disabled (for performance)
   - Location: `/home/user/Kickoff-NPI-Scenarios/UnrealProject/`
   - Name: `ShopStewardDigitalTwin`

3. Initial Project Settings:
   - Edit → Project Settings → Engine → Rendering
     - Default Settings: Set to "Scalable"
     - Anti-Aliasing: TAA
     - Global Illumination: Lumen (can be toggled)
     - Reflections: Lumen (can be toggled)

4. Create Folder Structure:
```
Content/
├── Blueprints/
│   ├── Departments/
│   ├── Machines/
│   ├── UI/
│   └── Core/
├── Materials/
│   ├── Machines/
│   ├── Floors/
│   └── UI/
├── Models/
│   ├── Machines/
│   └── Infrastructure/
├── Data/
│   ├── Tables/
│   └── Configs/
└── UI/
    ├── Widgets/
    └── HUD/
```

**Deliverables:**
- [ ] UE5 project created and version controlled
- [ ] Basic folder structure established
- [ ] Project settings optimized for performance
- [ ] .uproject file committed to git

#### Day 3-5: C# Integration Layer Setup

**Tasks:**
1. Expand `Class1.cs` to `ShopDataGateway.cs`
2. Add NuGet packages for ASP.NET Core
3. Create basic REST API structure
4. Set up WebSocket hub skeleton
5. Implement health check endpoints

**Code Structure:**
```
Kickoff-NPI-Scenarios/
├── Source/
│   ├── API/
│   │   ├── Controllers/
│   │   │   ├── DepartmentController.cs
│   │   │   ├── MachineController.cs
│   │   │   └── WorkflowController.cs
│   │   └── Hubs/
│   │       └── RealtimeDataHub.cs
│   ├── Services/
│   │   ├── DataTransformService.cs
│   │   ├── CacheService.cs
│   │   └── ValidationService.cs
│   ├── Models/
│   │   ├── Department.cs
│   │   ├── Machine.cs
│   │   ├── Job.cs
│   │   └── Workflow.cs
│   └── Infrastructure/
│       ├── Database/
│       └── Caching/
├── Tests/
│   └── UnitTests/
└── Docker/
    └── docker-compose.dev.yml
```

**Deliverables:**
- [ ] API project structure created
- [ ] Basic REST endpoints functional
- [ ] WebSocket connection tested
- [ ] Unit tests for core services

#### Day 6-7: Docker Development Environment

**Tasks:**
1. Create `docker-compose.dev.yml`
2. Configure PostgreSQL container
3. Configure TimescaleDB container
4. Configure Redis container
5. Set up volume mounts
6. Create initialization scripts

**Deliverables:**
- [ ] All containers start successfully
- [ ] Database schemas initialized
- [ ] Sample data loaded
- [ ] Health checks passing

### Week 2: Core Department Module System

#### Blueprint: BP_DepartmentBase

**Purpose:** Master blueprint that all departments inherit from

**Components:**
```
BP_DepartmentBase
├── SceneRoot (Scene Component)
├── FloorMesh (Static Mesh - resizable)
├── BoundingBox (Box Component - for collision/selection)
├── MachineArray (Array of BP_MachineBase references)
├── DataDisplayWidget (Widget Component)
└── ConnectionPoints (Array of Scene Components)
```

**Variables:**
```cpp
// Department Configuration
FString DepartmentID;
FString DepartmentName;
FVector DepartmentSize;  // X, Y, Z dimensions
int32 MaxMachines;
TArray<FMachineSlot> MachineSlots;

// Real-time Data
float CurrentUtilization;  // 0.0 to 1.0
int32 ActiveJobs;
int32 QueuedJobs;
TMap<FString, float> Metrics;

// Visual Settings
bool bShowHeatMap;
bool bShowLabels;
bool bShowConnections;
FLinearColor StatusColor;
```

**Functions:**
```cpp
// Initialization
void ConstructDepartment();
void LoadConfiguration();

// Runtime
void UpdateRealTimeData(FDepartmentData NewData);
void ResizeDepartment(FVector NewSize);
void AddMachine(BP_MachineBase* Machine, FTransform Location);
void RemoveMachine(FString MachineID);

// Visualization
void UpdateHeatMap();
void UpdateStatusColor();
void HighlightActiveJobs();

// Events
UFUNCTION(BlueprintImplementableEvent)
void OnDataReceived(const FDepartmentData& Data);

UFUNCTION(BlueprintImplementableEvent)
void OnMachineStatusChanged(const FString& MachineID, EMachineStatus Status);
```

**Deliverables:**
- [ ] BP_DepartmentBase created and tested
- [ ] Resizing system working
- [ ] Visual feedback functioning
- [ ] Data binding structure in place

#### Child Blueprints: Specific Departments

**Week 2 Focus: Create 3 departments**
1. **BP_CNCMillDepartment**
   - Inherits from BP_DepartmentBase
   - Specific machine types: 3-axis, 4-axis, 5-axis mills
   - Default layout: Linear arrangement
   - Special features: Tool changer visualization

2. **BP_CNCLatheDepartment**
   - Inherits from BP_DepartmentBase
   - Machine types: 2-axis, multi-axis lathes
   - Default layout: Opposing arrangement
   - Special features: Bar feeder simulation

3. **BP_ProgrammingRoom**
   - Inherits from BP_DepartmentBase
   - Assets: Desks, computers, verification stations
   - Special features: Code flow visualization

**Deliverables:**
- [ ] 3 department blueprints functional
- [ ] Each can be placed and resized
- [ ] Basic machine placement working
- [ ] Department-specific visuals implemented

### Week 3: Machine Asset Library

#### Blueprint: BP_MachineBase

**Components:**
```
BP_MachineBase
├── MachineMesh (Static Mesh - LOD enabled)
├── StatusLight (Point Light - color-coded)
├── ToolPosition (Scene Component - for tool path viz)
├── DataWidget (Widget Component - machine HUD)
├── AudioComponent (for operational sounds)
└── ParticleSystem (for cutting/coolant effects)
```

**States:**
```cpp
UENUM(BlueprintType)
enum class EMachineStatus : uint8
{
    Offline,
    Idle,
    Running,
    Paused,
    Alarm,
    Maintenance
};
```

**Real-time Data:**
```cpp
// Machine Identity
FString MachineID;
FString MachineName;
FString MachineType;

// Status
EMachineStatus CurrentStatus;
float UtilizationPercent;
float SpindleLoad;
float FeedRate;

// Current Job
FString CurrentJobID;
FString CurrentOperation;
float PercentComplete;
FDateTime EstimatedCompletion;

// Historical
int32 PartsCompletedToday;
float UpTimePercent;
TArray<FAlarmHistory> RecentAlarms;
```

**Child Machine Blueprints:**
1. **BP_3AxisMill**
2. **BP_5AxisMill**
3. **BP_TurningCenter**
4. **BP_MultiAxisLathe**
5. **BP_BandSaw**
6. **BP_CAMWorkstation**

**Deliverables:**
- [ ] BP_MachineBase with LOD system
- [ ] 6 specific machine types created
- [ ] Status visualization working
- [ ] Data binding to API functional

### Week 4: Data Integration & Testing

#### C# API Implementation

**Endpoints to Implement:**
```csharp
// Department Management
GET    /api/departments
GET    /api/departments/{id}
PUT    /api/departments/{id}/layout
GET    /api/departments/{id}/metrics

// Machine Status
GET    /api/machines
GET    /api/machines/{id}
GET    /api/machines/{id}/status
GET    /api/machines/{id}/history

// Jobs & Workflows
GET    /api/jobs/active
GET    /api/jobs/{id}/routing
GET    /api/workflows/npi/{id}

// Real-time Stream
WS     /ws/realtime
```

**Sample Data Generation:**
```csharp
public class SampleDataGenerator
{
    public static List<Machine> GenerateShopFloor()
    {
        // Create 20 machines across departments
        // Simulate varying utilization
        // Generate realistic job queues
        // Create sample NPI workflows
    }

    public static void StartRealTimeSimulation()
    {
        // Update machine states every 1s
        // Progress jobs through operations
        // Trigger random events (alarms, completions)
    }
}
```

**Deliverables:**
- [ ] All API endpoints functional
- [ ] WebSocket streaming working
- [ ] Sample data generator creating realistic scenarios
- [ ] UE5 can query and receive all data types

## Phase 2: Core Visualization (Weeks 5-8)

### Week 5: Complete Department Library

**Remaining 6 Departments:**
1. **BP_SawArea**
2. **BP_ShippingReceiving**
3. **BP_DeburrDepartment**
4. **BP_PartsCleaningDepartment**
5. **BP_FrontOffice**
6. **BP_ToolCrib**

**Layout Editor Interface:**
- Drag-and-drop department placement
- Snap-to-grid with configurable grid size
- Rotation in 90-degree increments
- Resize handles on selected departments
- Save/load layout configurations

**Deliverables:**
- [ ] All 9 departments implemented
- [ ] Layout editor functional
- [ ] Save/load system working
- [ ] Preset layouts (small/medium/large shop)

### Week 6: UMG UI System

#### Main HUD Widget

**Components:**
```
WBP_MainHUD
├── Top Bar
│   ├── Shop Name
│   ├── Current Shift
│   ├── Overall Utilization
│   └── Alert Counter
├── Left Panel (collapsible)
│   ├── Department List
│   ├── Machine Status Summary
│   └── Active Jobs List
├── Bottom Timeline
│   ├── Time Scrubber
│   ├── Playback Controls
│   └── Speed Control
└── Right Panel (collapsible)
    ├── Selected Item Details
    ├── Metrics Graphs
    └── Recent Events Log
```

#### Department Detail Widget

**Shown when hovering/selecting a department:**
- Department name and type
- Current utilization
- Machine count and status breakdown
- Active jobs
- Queue depth
- Performance vs. plan

**Deliverables:**
- [ ] Main HUD implemented
- [ ] Department detail widgets
- [ ] Machine detail widgets
- [ ] Responsive layout on different resolutions

### Week 7: Real-time Data Binding

**Blueprint Function Library: FL_DataBinding**

```cpp
// Fetch data from API
UFUNCTION(BlueprintCallable)
static FDepartmentData GetDepartmentData(FString DepartmentID);

UFUNCTION(BlueprintCallable)
static FMachineData GetMachineData(FString MachineID);

UFUNCTION(BlueprintCallable)
static TArray<FJobData> GetActiveJobs();

// WebSocket callbacks
UFUNCTION(BlueprintImplementableEvent)
void OnMachineStatusUpdate(FMachineStatusUpdate Update);

UFUNCTION(BlueprintImplementableEvent)
void OnJobProgress(FJobProgressUpdate Update);
```

**Data Structures (UE5 Structs):**
```cpp
USTRUCT(BlueprintType)
struct FDepartmentData
{
    UPROPERTY(BlueprintReadWrite)
    FString DepartmentID;

    UPROPERTY(BlueprintReadWrite)
    float Utilization;

    UPROPERTY(BlueprintReadWrite)
    TArray<FMachineData> Machines;

    UPROPERTY(BlueprintReadWrite)
    int32 ActiveJobs;
};

USTRUCT(BlueprintType)
struct FMachineData
{
    UPROPERTY(BlueprintReadWrite)
    FString MachineID;

    UPROPERTY(BlueprintReadWrite)
    EMachineStatus Status;

    UPROPERTY(BlueprintReadWrite)
    float UtilizationPercent;

    UPROPERTY(BlueprintReadWrite)
    FJobData CurrentJob;
};
```

**Deliverables:**
- [ ] HTTP request library functional
- [ ] WebSocket plugin integrated
- [ ] Data structures match API models
- [ ] Automatic updates every 1 second
- [ ] Error handling and reconnection logic

### Week 8: Visualization Effects

**Heat Map System:**
- Material function for color gradient (green → yellow → red)
- Apply to department floor meshes
- Update based on utilization metrics
- Smooth transitions between states

**Status Indicators:**
- Point lights color-coded by status
- Particle effects for active machines
- Warning animations for alarms
- Pulsing effects for attention

**Job Flow Visualization:**
- Animated paths between departments
- Particle systems showing job movement
- Color-coded by priority/type
- Speed indicates urgency

**Deliverables:**
- [ ] Heat map materials created
- [ ] Status light system working
- [ ] Job flow particles functional
- [ ] Performance optimized (<2ms per frame)

## Phase 3: Data Integration (Weeks 9-12)

### Week 9: Database Schema & Seeding

**PostgreSQL Tables:**
```sql
-- Departments
CREATE TABLE departments (
    id UUID PRIMARY KEY,
    name VARCHAR(100),
    type VARCHAR(50),
    size_x FLOAT,
    size_y FLOAT,
    size_z FLOAT,
    position_x FLOAT,
    position_y FLOAT,
    position_z FLOAT,
    rotation FLOAT,
    max_machines INT
);

-- Machines
CREATE TABLE machines (
    id UUID PRIMARY KEY,
    department_id UUID REFERENCES departments(id),
    name VARCHAR(100),
    type VARCHAR(50),
    model VARCHAR(100),
    manufacturer VARCHAR(100),
    installation_date DATE,
    position_x FLOAT,
    position_y FLOAT,
    position_z FLOAT
);

-- Job Routings
CREATE TABLE job_routings (
    id UUID PRIMARY KEY,
    job_number VARCHAR(50),
    part_number VARCHAR(50),
    quantity INT,
    priority INT,
    created_at TIMESTAMP,
    due_date TIMESTAMP
);

-- Operations
CREATE TABLE operations (
    id UUID PRIMARY KEY,
    routing_id UUID REFERENCES job_routings(id),
    sequence INT,
    operation_code VARCHAR(20),
    department_id UUID REFERENCES departments(id),
    estimated_hours FLOAT,
    setup_hours FLOAT,
    status VARCHAR(20)
);
```

**TimescaleDB Tables:**
```sql
-- Machine status history (time-series)
CREATE TABLE machine_status_history (
    time TIMESTAMPTZ NOT NULL,
    machine_id UUID NOT NULL,
    status VARCHAR(20),
    utilization FLOAT,
    spindle_load FLOAT,
    feed_rate FLOAT,
    job_id UUID
);

SELECT create_hypertable('machine_status_history', 'time');

-- Job progress history
CREATE TABLE job_progress_history (
    time TIMESTAMPTZ NOT NULL,
    job_id UUID NOT NULL,
    operation_id UUID NOT NULL,
    percent_complete FLOAT,
    actual_hours FLOAT
);

SELECT create_hypertable('job_progress_history', 'time');
```

**Deliverables:**
- [ ] Database schemas created
- [ ] Seed data for 50+ jobs
- [ ] Historical data for 30 days
- [ ] Migration scripts

### Week 10: REST API Completion

**Implementation Details:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDataService _dataService;
    private readonly ICache _cache;

    [HttpGet]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll()
    {
        var departments = await _dataService.GetDepartmentsAsync();
        return Ok(departments);
    }

    [HttpGet("{id}/metrics")]
    public async Task<ActionResult<DepartmentMetrics>> GetMetrics(
        Guid id,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end)
    {
        var metrics = await _dataService.GetDepartmentMetricsAsync(
            id, start ?? DateTime.UtcNow.AddHours(-1), end ?? DateTime.UtcNow);
        return Ok(metrics);
    }
}
```

**Caching Strategy:**
- Static data (department configs): 1 hour cache
- Machine status: 5 second cache
- Active jobs: 10 second cache
- Historical data: 24 hour cache
- Invalidate on updates via Redis pub/sub

**Deliverables:**
- [ ] All CRUD operations implemented
- [ ] Caching working correctly
- [ ] API documentation (Swagger)
- [ ] Rate limiting configured

### Week 11: WebSocket Real-time Streaming

**Hub Implementation:**
```csharp
public class RealtimeDataHub : Hub
{
    public async Task SubscribeToMachine(string machineId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"machine-{machineId}");
    }

    public async Task SubscribeToDepartment(string departmentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"department-{departmentId}");
    }

    public async Task UnsubscribeAll()
    {
        // Remove from all groups
    }
}

// Background service to push updates
public class DataPushService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var updates = await GetMachineUpdates();
            foreach (var update in updates)
            {
                await _hubContext.Clients
                    .Group($"machine-{update.MachineId}")
                    .SendAsync("MachineStatusUpdate", update);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

**Deliverables:**
- [ ] WebSocket hub functional
- [ ] Subscription system working
- [ ] Auto-reconnect logic
- [ ] Message compression enabled

### Week 12: UE5 Integration Testing

**Integration Tests:**
1. HTTP request latency <100ms
2. WebSocket message delivery <50ms
3. Data synchronization accuracy
4. Reconnection scenarios
5. Load testing (50 concurrent connections)

**Blueprint Test Map:**
- Spawns all 9 departments
- Populates with 30 machines
- Connects to API
- Runs for 1 hour
- Logs any errors or desyncs

**Deliverables:**
- [ ] All integration tests passing
- [ ] Performance benchmarks met
- [ ] Error handling validated
- [ ] Documentation updated

## Phase 4: NPI Workflow Visualization (Weeks 13-16)

### Week 13: Workflow Data Model

**Workflow Definition:**
```csharp
public class NPIWorkflow
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; }
    public string Description { get; set; }
    public List<WorkflowStep> Steps { get; set; }
    public WorkflowStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class WorkflowStep
{
    public int Sequence { get; set; }
    public string StepType { get; set; }  // "Programming", "FirstArticle", "Production"
    public Guid DepartmentId { get; set; }
    public Guid? MachineId { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public TimeSpan? ActualDuration { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; }
}
```

**Visualization Blueprint: BP_WorkflowTimeline**

**Components:**
- Timeline widget showing all steps
- Gantt-style chart
- Estimated vs. actual comparison bars
- Critical path highlighting
- Current step indicator

**Deliverables:**
- [ ] Workflow data model implemented
- [ ] API endpoints for workflows
- [ ] Sample NPI workflows created
- [ ] Basic timeline widget functional

### Week 14: Timeline Mode Implementation

**Features:**
1. **Scrubbing**: Move slider to any point in timeline
2. **Playback**: Animate through workflow execution
3. **Speed Control**: 1x, 2x, 5x, 10x playback
4. **Comparison View**: Split-screen estimated vs. actual

**Visual Effects:**
- Job token travels between departments
- Department highlights as job enters
- Machine animates during operation
- Completion checkmarks appear

**Deliverables:**
- [ ] Timeline scrubber functional
- [ ] Playback controls working
- [ ] Visual effects implemented
- [ ] Performance optimized

### Week 15: Real-time Tracking Mode

**Live Dashboard:**
```
┌─────────────────────────────────────────┐
│  Job: #12345 - Gear Housing (Qty: 10)  │
├─────────────────────────────────────────┤
│  Current: Op 30 - 5-Axis Mill (75%)    │
│  Next: Op 40 - Deburr                  │
│  Queue: 3 jobs ahead                    │
│  Est. Completion: Today 3:45 PM         │
└─────────────────────────────────────────┘
```

**Features:**
- Camera auto-follows active job
- Department zoom-in on operation
- Queue visualization at each station
- Alert notifications for delays

**Deliverables:**
- [ ] Real-time tracking functional
- [ ] Camera system working
- [ ] Queue visualization implemented
- [ ] Alerts integrated

### Week 16: Bottleneck Analysis

**Analysis Features:**
1. **Utilization Heat Map**: Show which departments are overloaded
2. **Queue Depth Tracking**: Visualize where jobs stack up
3. **Throughput Metrics**: Parts per hour by department
4. **Critical Path Analysis**: Highlight longest pole in tent

**Simulation Engine:**
- Adjust capacities (add/remove machines)
- Change routing decisions
- Model "what-if" scenarios
- Compare before/after metrics

**Deliverables:**
- [ ] Heat map analysis working
- [ ] Simulation engine functional
- [ ] Comparison tools implemented
- [ ] Reports generation ready

## Phase 5: CNC Simulation (Weeks 17-20)

### Week 17: CAD Import Pipeline

**Datasmith Setup:**
1. Install Datasmith CAD plugin
2. Configure import settings for STEP/IGES
3. Create material mapping rules
4. Set up LOD generation

**Import Process:**
```cpp
// Blueprint function
UFUNCTION(BlueprintCallable)
static UStaticMesh* ImportCADFile(FString FilePath)
{
    // Use Datasmith to import
    // Generate collision
    // Create LODs
    // Apply default material
}
```

**Deliverables:**
- [ ] Datasmith plugin configured
- [ ] Import process automated
- [ ] Sample parts imported
- [ ] Material library created

### Week 18: Tool Path Visualization

**G-Code Parser:**
```csharp
public class GCodeParser
{
    public List<ToolPathSegment> ParseFile(string gcodePath)
    {
        // Parse G-code into movement segments
        // Extract feed rates, spindle speeds
        // Identify rapid vs. cutting moves
        // Generate 3D path coordinates
    }
}

public class ToolPathSegment
{
    public Vector3 StartPoint { get; set; }
    public Vector3 EndPoint { get; set; }
    public ToolPathType Type { get; set; }  // Rapid, Linear, Arc
    public float FeedRate { get; set; }
    public float SpindleSpeed { get; set; }
}
```

**UE5 Visualization:**
- Line renderer for tool path
- Color-coded by move type (rapid=blue, cut=red)
- Animation of tool following path
- Collision detection with part/fixtures

**Deliverables:**
- [ ] G-code parser implemented
- [ ] Path visualization working
- [ ] Animation system functional
- [ ] Collision detection active

### Week 19: Material Removal Simulation

**Approach: Voxel-based Subtraction**

```cpp
// Simplified approach for threshold hardware
class VoxelMaterialRemoval
{
public:
    void Initialize(UStaticMesh* StockMaterial, float VoxelSize);
    void RemoveMaterialAlongPath(FVector Start, FVector End, float ToolDiameter);
    UProceduralMeshComponent* GenerateCurrentState();
};
```

**Optimization:**
- Coarse voxel grid (5mm) for real-time
- Fine grid (0.5mm) for final verification
- Only update visible regions
- Use compute shaders if GPU capable

**Deliverables:**
- [ ] Basic voxel system working
- [ ] Material removal visible
- [ ] Performance acceptable (>15 FPS)
- [ ] Toggle for detail levels

### Week 20: Training Mode

**Interactive Scenarios:**
1. **Setup Verification**: Check fixtures, part orientation
2. **First Part Run**: Step through program with pauses
3. **In-Process Inspection**: Verify critical features
4. **Tool Change Procedure**: Simulate tool breakage recovery
5. **Crash Scenarios**: Show what happens with errors

**Scoring System:**
- Points for correct actions
- Deductions for errors
- Time-based bonuses
- Leaderboard integration

**Deliverables:**
- [ ] 5 training scenarios created
- [ ] Scoring system functional
- [ ] Progress tracking implemented
- [ ] Feedback system working

## Phase 6: Polish & Optimization (Weeks 21-24)

### Week 21: Performance Profiling

**Profiling Tools:**
1. UE5 built-in profiler (stat fps, stat unit)
2. GPU profiling (stat gpu)
3. Memory tracking (stat memory)
4. Network profiling for API calls

**Optimization Targets:**
- Maintain 30+ FPS with full shop (50 machines)
- <500ms API response times
- <100MB memory footprint for UI
- <50ms frame time for interactions

**Common Bottlenecks:**
- Too many tick events → convert to timers
- Expensive materials → simplify shaders
- Too many draw calls → use instancing
- Unoptimized blueprints → nativize or convert to C++

**Deliverables:**
- [ ] Performance baseline established
- [ ] Bottlenecks identified
- [ ] Optimization plan created
- [ ] 50% improvement in worst-case scenarios

### Week 22: UI/UX Refinement

**User Testing:**
- Test with 5 shop floor personnel
- Test with 3 managers
- Test with 2 executives
- Gather feedback on usability

**Improvements:**
- Simplify navigation
- Add tooltips and help
- Improve color schemes for accessibility
- Add keyboard shortcuts

**Deliverables:**
- [ ] User testing completed
- [ ] Feedback incorporated
- [ ] Accessibility improvements made
- [ ] Tutorial system added

### Week 23: Documentation & Training

**Documentation:**
1. **User Manual**: How to navigate and use the system
2. **API Documentation**: For developers extending the system
3. **Deployment Guide**: How to set up in production
4. **Troubleshooting Guide**: Common issues and solutions

**Training Materials:**
1. **Video Tutorials**: 5-10 minute walkthroughs
2. **Quick Start Guide**: One-page PDF
3. **Interactive Demo**: Guided tour mode in UE5
4. **FAQ Document**: Based on user testing

**Deliverables:**
- [ ] All documentation completed
- [ ] Training videos recorded
- [ ] Interactive demo functional
- [ ] FAQ published

### Week 24: Deployment & Presentation

**Deployment Package:**
```
Release/
├── UnrealProject/
│   └── Packaged/
│       ├── WindowsClient/
│       └── README.md
├── API/
│   ├── Docker/
│   │   └── docker-compose.prod.yml
│   └── Deployment.md
├── Database/
│   ├── Schemas/
│   └── Migrations/
├── Documentation/
│   ├── UserManual.pdf
│   ├── APIReference.pdf
│   └── Videos/
└── Presentation/
    ├── ExecutiveDemo.mp4
    └── SlideDecks/
```

**C-Suite Presentation Template:**
1. Opening: Interactive 3D shop tour
2. Problem Statement: Show current bottleneck
3. Solution: Simulate improvement
4. ROI Calculator: Real-time what-if scenarios
5. Next Steps: Phased implementation plan

**Deliverables:**
- [ ] Production deployment successful
- [ ] Executive presentation completed
- [ ] All stakeholders trained
- [ ] Feedback loop established

## Maintenance & Future Phases

### Ongoing (Post-Launch)

**Monthly:**
- Performance monitoring
- User feedback review
- Bug fixes
- Security updates

**Quarterly:**
- Feature enhancements
- New department types
- Integration with additional systems
- VR/AR capabilities exploration

**Annually:**
- UE5 version upgrade
- Major feature releases
- Hardware requirement reassessment
- Strategic roadmap update

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Performance issues on threshold hardware | Aggressive LOD, scalability settings, quality presets |
| Data integration complexity | Start with mocked data, add real sources incrementally |
| Scope creep | Strict phase gates, MVP focus |
| UE5 learning curve | Leverage Blueprint, extensive prototyping |
| API stability | Comprehensive testing, versioning strategy |

## Success Criteria

- [ ] All 9 departments functional and performant
- [ ] Real-time data updates with <1s latency
- [ ] 30+ FPS on threshold hardware
- [ ] Executive demo generates C-suite engagement
- [ ] Identified at least 1 bottleneck in real shop
- [ ] Training mode reduces onboarding time by 50%
- [ ] System scales to 50+ machines
- [ ] Positive user feedback from all stakeholder groups

---

**Next Steps:**
1. Review and approve this roadmap
2. Set up development environment (Day 1 tasks)
3. Create UE5 project (Week 1, Day 1-2)
4. Begin C# API structure (Week 1, Day 3-5)
