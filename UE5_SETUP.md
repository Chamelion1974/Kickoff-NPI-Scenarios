# Unreal Engine 5 Setup Guide - Shop Steward Hub Digital Twin

This guide walks you through creating the UE5 project and connecting it to the Shop Steward Hub API.

## Prerequisites

- Unreal Engine 5.4 or later installed via Epic Games Launcher
- Completed API setup (see QUICK_START.md)
- Basic understanding of Blueprint visual scripting
- Recommended: 16GB+ RAM, NVIDIA RTX 2060 or better (or equivalent AMD)

## Part 1: Create the UE5 Project (30 minutes)

### Step 1: Create New Project

1. **Launch Unreal Engine** from Epic Games Launcher
2. Click **"New Project"**
3. Configure:
   - **Template**: Select "Blank"
   - **Project Type**: Blueprint
   - **Target Platform**: Desktop
   - **Quality Preset**: Scalable
   - **Starter Content**: No (we'll create custom assets)
   - **Raytracing**: Disabled (for performance)
4. **Project Location**: `Kickoff-NPI-Scenarios/UnrealProject/`
5. **Project Name**: `ShopStewardDigitalTwin`
6. Click **"Create"**

### Step 2: Initial Project Settings

Once the project opens:

1. **Edit → Project Settings**
2. Navigate to **Engine → Rendering**
   - Anti-Aliasing Method: TAA
   - Default Settings: Scalable
   - Dynamic Global Illumination: Lumen (toggle-able)
   - Reflections: Lumen (toggle-able)

3. Navigate to **Engine → Performance**
   - Frame Rate Smoothing: Enabled
   - Min FPS: 30
   - Max FPS: 60

4. Navigate to **Project → Maps & Modes**
   - Default Maps → Editor Startup Map: (we'll create this)
   - Default Maps → Game Default Map: (we'll create this)

### Step 3: Create Folder Structure

In the Content Browser, create this structure:

```
Content/
├── Blueprints/
│   ├── Core/
│   │   ├── BP_GameMode
│   │   ├── BP_PlayerController
│   │   └── BP_GameState
│   ├── Departments/
│   │   ├── BP_DepartmentBase
│   │   └── (department-specific blueprints)
│   ├── Machines/
│   │   ├── BP_MachineBase
│   │   └── (machine-specific blueprints)
│   ├── Data/
│   │   └── BP_APIManager
│   └── UI/
│       └── (widget blueprints)
├── Materials/
│   ├── M_Floor
│   ├── M_Machine
│   └── M_HeatMap
├── Models/
│   └── (static meshes)
├── UI/
│   ├── WBP_MainHUD
│   ├── WBP_DepartmentInfo
│   └── WBP_MachineStatus
└── Maps/
    ├── L_ShopFloor
    └── L_TestLevel
```

### Step 4: Create Main Level

1. **File → New Level**
2. Select "Empty Level"
3. Save as `L_ShopFloor` in `Content/Maps/`
4. Add basic lighting:
   - Place **Directional Light** (for sun)
   - Place **Sky Light** (for ambient)
   - Place **Sky Atmosphere** (for realistic sky)
5. Add a large **Plane** or **Cube** for the floor
6. Save

## Part 2: Install Required Plugins (15 minutes)

### VaRest Plugin (for HTTP requests)

1. **Edit → Plugins**
2. Search for "VaRest"
3. **Install** and **Enable**
4. Restart UE5 when prompted

**Alternative**: Use built-in HTTP module (more complex but no plugin needed)

### WebSocket Plugin (for real-time data)

**Option A: SocketIOClient** (Recommended for beginners)
1. Download from: https://github.com/getnamo/socketio-client-ue4
2. Extract to `Plugins/SocketIOClient` in your project folder
3. Restart UE5
4. Enable in Edit → Plugins

**Option B: Built-in WebSocket**
- More complex, requires C++ code
- Better performance
- Recommended for production

### Datasmith CAD (for importing CAD models)

1. **Edit → Plugins**
2. Search for "Datasmith CAD"
3. **Enable**
4. Restart UE5

This allows importing STEP/IGES files for CNC machine models.

## Part 3: Create the API Manager Blueprint (45 minutes)

### Step 1: Create the Blueprint

1. **Right-click** in `Content/Blueprints/Data/`
2. **Blueprint Class → Actor**
3. Name it `BP_APIManager`
4. **Double-click** to open

### Step 2: Create Variables

Add these variables in the **Variables** panel:

```
API Variables:
- APIBaseURL (String) = "http://localhost:5000/api"
- WebSocketURL (String) = "ws://localhost:5000/ws/realtime"
- IsConnected (Boolean) = false
- UpdateInterval (Float) = 1.0 (seconds)

Data Cache:
- Departments (Array of String) - stores JSON responses
- Machines (Array of String)
- ActiveJobs (Array of String)
```

### Step 3: Create HTTP Request Function

Create a new **Function** called `MakeHTTPRequest`:

**Inputs:**
- Endpoint (String) - e.g., "/departments"
- OnSuccess (Delegate) - callback with response
- OnFail (Delegate) - callback on error

**Blueprint Logic:**
```
1. Construct JSON → Create HTTP Request
2. Set URL = APIBaseURL + Endpoint
3. Set Method = "GET"
4. Set Header: Content-Type = "application/json"
5. On Response Received:
   - If Status Code == 200:
     - Call OnSuccess with Response Body
   - Else:
     - Call OnFail with Error Message
6. Process Request
```

### Step 4: Create Data Fetching Functions

Create these functions:

**GetDepartments**
```
1. Call MakeHTTPRequest("/departments")
2. On Success:
   - Parse JSON (use VaRest or built-in)
   - Store in Departments array
   - Broadcast "OnDepartmentsUpdated" event
```

**GetMachines**
```
1. Call MakeHTTPRequest("/machines")
2. On Success:
   - Parse JSON
   - Store in Machines array
   - Broadcast "OnMachinesUpdated" event
```

**GetActiveJobs**
```
1. Call MakeHTTPRequest("/jobs/active")
2. On Success:
   - Parse JSON
   - Store in ActiveJobs array
   - Broadcast "OnJobsUpdated" event
```

### Step 5: Implement Auto-Update

In **Event Graph**:

```
Event BeginPlay:
1. Call GetDepartments
2. Call GetMachines
3. Call GetActiveJobs
4. Set Timer by Function Name:
   - Function: RefreshAllData
   - Time: UpdateInterval
   - Looping: Yes

RefreshAllData (Custom Event):
1. Call GetDepartments
2. Call GetMachines
3. Call GetActiveJobs
```

### Step 6: Test the API Connection

1. Add `BP_APIManager` to your level
2. **Play in Editor**
3. Open **Output Log** (Window → Developer Tools → Output Log)
4. You should see successful HTTP requests

## Part 4: Create Department Blueprint (60 minutes)

### Step 1: Create BP_DepartmentBase

1. **Right-click** in `Content/Blueprints/Departments/`
2. **Blueprint Class → Actor**
3. Name: `BP_DepartmentBase`
4. Open the blueprint

### Step 2: Add Components

In **Components** panel:

```
DefaultSceneRoot
├── FloorMesh (Static Mesh Component)
│   └── Set mesh to a plane or cube
├── BoundingBox (Box Component)
│   └── For selection and collision
├── MachineGrid (Scene Component)
│   └── Parent for machine actors
└── InfoWidget (Widget Component)
    └── Shows department name and status
```

### Step 3: Create Variables

```
Department Info:
- DepartmentID (String)
- DepartmentName (String)
- DepartmentType (String)

Layout:
- SizeX (Float) = 10.0
- SizeY (Float) = 10.0
- SizeZ (Float) = 3.0

Machines:
- MachineActors (Array of Actor References)
- MaxMachines (Integer) = 10

Metrics:
- CurrentUtilization (Float) = 0.0
- ActiveMachines (Integer) = 0
- IdleMachines (Integer) = 0

Visual:
- FloorMaterial (Material Reference)
- HeatMapEnabled (Boolean) = false
- StatusColor (Linear Color)
```

### Step 4: Create Construction Script

```
Construction Script:
1. Get SizeX, SizeY, SizeZ
2. Set FloorMesh scale:
   - X = SizeX / 100  (assuming 1m cube)
   - Y = SizeY / 100
   - Z = SizeZ / 100
3. Set BoundingBox extent:
   - X = SizeX * 50
   - Y = SizeY * 50
   - Z = SizeZ * 50
4. Update InfoWidget position (above department)
```

### Step 5: Create Update Function

**UpdateFromAPIData** (Custom Function):

**Input**: DepartmentData (String) - JSON from API

**Logic**:
```
1. Parse JSON to get:
   - DepartmentName
   - Metrics (utilization, machine counts)
2. Set variables
3. Update visual representation:
   - Change floor color based on utilization
   - Update InfoWidget text
4. Calculate StatusColor:
   - If utilization > 80%: Red
   - If utilization > 50%: Yellow
   - Else: Green
5. Apply dynamic material to floor
```

### Step 6: Create Heat Map Material

1. Create new **Material** in `Content/Materials/`
2. Name: `M_HeatMap`
3. Add **Material Parameter** nodes:
   - Utilization (Scalar Parameter)
4. Use **Lerp** nodes to blend:
   - Low utilization: Blue/Green
   - Medium utilization: Yellow
   - High utilization: Red/Orange
5. Apply to FloorMesh

### Step 7: Test Department

1. Place `BP_DepartmentBase` in level
2. Set DepartmentName in details panel
3. Set size (e.g., 15x20x3)
4. Play and verify it appears correctly
5. Test resizing in editor

## Part 5: Create Machine Blueprint (45 minutes)

### Step 1: Create BP_MachineBase

1. Create new **Blueprint Class → Actor**
2. Name: `BP_MachineBase`

### Step 2: Add Components

```
DefaultSceneRoot
├── MachineMesh (Static Mesh)
│   └── Placeholder cube for now
├── StatusLight (Point Light)
│   └── Color indicates status
├── ToolPosition (Scene Component)
│   └── For tool path visualization
└── StatusWidget (Widget Component)
    └── Shows machine name and status
```

### Step 3: Create Variables

```
Machine Info:
- MachineID (String)
- MachineName (String)
- MachineType (String)

Status:
- CurrentStatus (Enum: Offline, Idle, Running, Paused, Alarm)
- UtilizationPercent (Float) = 0.0
- SpindleLoad (Float) = 0.0

Visual:
- StatusColor (Linear Color)
- BaseMaterial (Material)
```

### Step 4: Create Status Update Function

**UpdateStatus** (Custom Function):

**Input**: StatusData (String) - JSON from API

```
1. Parse JSON to get:
   - Status
   - UtilizationPercent
   - SpindleLoad
2. Set CurrentStatus enum
3. Update StatusColor:
   - Offline: Gray
   - Idle: Blue
   - Running: Green
   - Paused: Yellow
   - Alarm: Red
4. Set StatusLight color
5. Update StatusWidget text
```

### Step 5: Add Visual Feedback

**Event Tick** (or Timer):
```
If CurrentStatus == Running:
   - Pulse StatusLight brightness
   - Maybe add particle effect
   - Rotate tool position slightly
```

## Part 6: Create Main HUD (30 minutes)

### Step 1: Create Widget Blueprint

1. **Right-click** in `Content/UI/`
2. **User Interface → Widget Blueprint**
3. Name: `WBP_MainHUD`

### Step 2: Design Layout

Add these widgets in the **Designer**:

```
Canvas Panel
├── Top Bar (Horizontal Box)
│   ├── Text: Shop Name
│   ├── Spacer
│   ├── Text: Current Shift
│   └── Text: Overall Utilization
├── Left Panel (Vertical Box)
│   ├── Text: "Departments"
│   └── Scroll Box (for department list)
├── Bottom Panel (Horizontal Box)
│   ├── Button: Play
│   ├── Button: Pause
│   ├── Slider: Timeline
│   └── Text: Current Time
└── Right Panel (Vertical Box)
    ├── Text: "Selected Info"
    └── Details Panel
```

### Step 3: Bind Data

In the **Graph**:

```
Event Construct:
1. Get BP_APIManager reference
2. Bind to OnDepartmentsUpdated event
3. Bind to OnMachinesUpdated event

OnDepartmentsUpdated:
1. Clear department list
2. For each department:
   - Create list item widget
   - Add to scroll box
```

### Step 4: Add to Level

In your **Level Blueprint** or **GameMode**:

```
Event BeginPlay:
1. Create WBP_MainHUD widget
2. Add to Viewport
3. Set input mode to UI only (or Game and UI)
```

## Part 7: Connect Everything (30 minutes)

### Step 1: Create GameMode

1. Create `BP_ShopStewardGameMode` blueprint
2. In **Class Defaults**:
   - Set Player Controller Class
   - Set HUD Class
   - Set Game State Class

### Step 2: Level Setup

In `L_ShopFloor`:

1. Place `BP_APIManager` in the level
2. Place departments using `BP_DepartmentBase`
3. Arrange departments in a realistic layout
4. Place machines using `BP_MachineBase` inside departments

### Step 3: Wire Up Data Flow

In **Level Blueprint**:

```
Event BeginPlay:
1. Get BP_APIManager reference
2. Bind to OnDepartmentsUpdated
3. Bind to OnMachinesUpdated

OnDepartmentsUpdated:
1. For each Department actor in level:
   - Find matching data from API by ID
   - Call UpdateFromAPIData

OnMachinesUpdated:
1. For each Machine actor in level:
   - Find matching data from API by ID
   - Call UpdateStatus
```

### Step 4: Test End-to-End

1. **Start the API** (see QUICK_START.md)
2. **Play in UE5**
3. Verify:
   - Departments load and display
   - Machines show correct status
   - Data updates every second
   - HUD shows information

## Part 8: Performance Optimization (20 minutes)

### Enable Scalability Settings

1. **Edit → Project Settings → Engine → Scalability**
2. Create scalability settings for:
   - Low (for threshold hardware)
   - Medium
   - High
   - Cinematic (for presentations)

### Optimize Rendering

1. Use **LODs** on all meshes:
   - LOD 0: High detail (< 10m)
   - LOD 1: Medium detail (10-50m)
   - LOD 2: Low detail (> 50m)

2. **Limit tick events**:
   - Don't update every frame
   - Use timers instead (1-2 second intervals)

3. **Cull off-screen objects**:
   - Enable distance culling
   - Use occlusion culling

### Monitor Performance

Press **`** (tilde) in Play mode and type:
- `stat fps` - Show FPS
- `stat unit` - Show frame time breakdown
- `stat gpu` - Show GPU timing

## Part 9: Advanced Features (Optional)

### WebSocket Real-time Updates

Instead of polling every second, use WebSocket for instant updates:

1. Install SocketIO plugin
2. Connect to `/ws/realtime`
3. Subscribe to machine updates
4. Update UI immediately on events

### Camera System

Create a camera that:
- Can orbit the shop floor
- Zoom to specific departments
- Follow active jobs through workflow
- Switch between overview and detail views

### Timeline Playback

Create a system to:
- Scrub through historical data
- Replay job workflows
- Compare estimated vs. actual timelines

## Troubleshooting

### API Connection Fails

1. Check API is running: http://localhost:5000/health
2. Check firewall isn't blocking
3. Verify URL in BP_APIManager is correct
4. Check Output Log for error messages

### Low FPS in Editor

1. Set Editor quality to "Low" temporarily
2. Disable Lumen for development
3. Reduce viewport resolution
4. Close other applications

### Blueprints Not Updating

1. **Compile** blueprints
2. **Save** all assets
3. Restart UE5 editor
4. Clear derived data cache if needed

## Next Steps

1. **Add more departments**: Create children of BP_DepartmentBase
2. **Import CAD models**: Use Datasmith to import real machine models
3. **Implement CNC simulation**: Tool path visualization
4. **Create workflow visualization**: NPI process flow
5. **Add VR support**: For immersive walkthroughs

## Resources

- **UE5 Documentation**: https://docs.unrealengine.com
- **VaRest Plugin**: https://github.com/ufna/VaRest
- **SocketIO Plugin**: https://github.com/getnamo/socketio-client-ue4
- **Blueprint Tutorials**: Epic Games Learning Portal

---

**Congratulations!** You now have a functional UE5 digital twin connected to your shop floor data. Start customizing and adding features specific to your manufacturing environment.
