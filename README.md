# Shop Steward Hub Digital Twin Platform

**A real-time 3D digital twin for manufacturing operations powered by Unreal Engine 5**

Transform your shop floor into an immersive, data-driven 3D experience that enables strategic planning, NPI process optimization, and bottleneck identification through interactive simulation.

![Version](https://img.shields.io/badge/version-1.0.0--alpha-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Unreal](https://img.shields.io/badge/Unreal-5.4+-black)
![License](https://img.shields.io/badge/license-MIT-green)

## What is This?

This project creates a **virtual representation of your machine shop** that:

- **Visualizes** your shop floor layout in stunning 3D
- **Tracks** machine status, job progress, and workflow in real-time
- **Simulates** CNC operations with material removal and tool path verification
- **Analyzes** bottlenecks and capacity through interactive scenarios
- **Presents** data in an executive-friendly format that drives decision-making

### Key Features

✅ **Modular Department System** - Build your shop floor from pre-configured department blocks (CNC Mill, Lathe, Programming, etc.)

✅ **Real-time Machine Telemetry** - Live status, utilization, and alerts from actual machines

✅ **NPI Workflow Visualization** - Track new part introduction from programming to first article

✅ **Timeline & Simulation** - Replay history or simulate future scenarios

✅ **Performance Optimized** - Runs on threshold hardware with scalability settings

✅ **REST + WebSocket API** - Clean integration with ERP/MES/machine controllers

✅ **Docker-based Stack** - TimescaleDB + PostgreSQL + Redis for robust data management

## Quick Start

**Get up and running in under 30 minutes:**

1. **Clone the repository**
   ```bash
   git clone <repo-url>
   cd Kickoff-NPI-Scenarios
   ```

2. **Start backend services**
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

3. **Run the API**
   ```bash
   dotnet restore
   dotnet run
   ```

4. **Open Swagger UI**
   - Navigate to http://localhost:5000/swagger
   - Test endpoints and explore the API

5. **Create UE5 project**
   - See [UE5_SETUP.md](./UE5_SETUP.md) for detailed instructions

👉 **Full instructions**: [QUICK_START.md](./QUICK_START.md)

## Architecture Overview

```
┌─────────────────────────────────────────┐
│     UE5 Digital Twin (Visualization)    │
│  • 3D Shop Floor                        │
│  • Interactive Dashboards                │
│  • CNC Simulation                       │
└─────────────────┬───────────────────────┘
                  │ REST + WebSocket
┌─────────────────▼───────────────────────┐
│     C# API Gateway (Integration)        │
│  • REST Endpoints                       │
│  • SignalR Real-time Hub                │
│  • Data Transformation                  │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│     Docker Services (Data Layer)        │
│  • TimescaleDB (Telemetry)             │
│  • PostgreSQL (Metadata)                │
│  • Redis (Cache)                        │
└─────────────────────────────────────────┘
```

## Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Visualization** | Unreal Engine 5.4+ | 3D rendering, simulation, UI |
| **Scripting** | Blueprint Visual Scripting | Game logic, data binding |
| **Integration** | C# .NET 9.0 | API gateway, data transformation |
| **API** | ASP.NET Core | REST endpoints, WebSocket server |
| **Caching** | Redis | Real-time data, session management |
| **Time-series** | TimescaleDB | Sensor data, historical metrics |
| **Metadata** | PostgreSQL | Configuration, job definitions |
| **Containerization** | Docker Desktop | Development environment |

## Project Structure

```
Kickoff-NPI-Scenarios/
├── Source/                      # C# source code
│   ├── API/
│   │   ├── Controllers/         # REST API endpoints
│   │   └── Hubs/                # SignalR WebSocket hubs
│   ├── Services/                # Business logic
│   ├── Models/                  # Data models and DTOs
│   └── Infrastructure/
│       ├── Database/            # EF Core contexts
│       └── Caching/             # Redis service
├── Database/                    # SQL initialization scripts
│   ├── init-postgres.sql        # Metadata schema
│   └── init-timescale.sql       # Time-series schema
├── UnrealProject/               # UE5 project (created separately)
│   └── ShopStewardDigitalTwin/
├── docker-compose.dev.yml       # Development environment
├── ARCHITECTURE.md              # Detailed system design
├── DEVELOPMENT_ROADMAP.md       # 24-week implementation plan
├── QUICK_START.md               # Getting started guide
├── UE5_SETUP.md                 # Unreal Engine setup guide
└── README.md                    # This file
```

## API Endpoints

### Departments
- `GET /api/departments` - All departments with metrics
- `GET /api/departments/{id}` - Specific department
- `GET /api/departments/{id}/metrics` - Real-time metrics

### Machines
- `GET /api/machines` - All machines
- `GET /api/machines/{id}` - Specific machine
- `GET /api/machines/{id}/status` - Current status
- `GET /api/machines/department/{id}` - Machines by department

### Jobs
- `GET /api/jobs/active` - Active jobs
- `GET /api/jobs/{id}` - Specific job
- `GET /api/jobs/status/{status}` - Jobs by status

### Workflows
- `GET /api/workflows/npi/{jobId}` - NPI workflow visualization
- `GET /api/workflows/npi/active` - All active NPI workflows

### WebSocket
- `WS /ws/realtime` - Real-time updates (SignalR)

## Use Cases

### 1. Shop Floor Planning
- Drag-and-drop departments to test new layouts
- Simulate throughput with different configurations
- Visualize material flow and identify bottlenecks

### 2. NPI Process Management
- Track new part introduction from quote to production
- Visualize job routing through departments
- Compare estimated vs. actual timelines
- Identify where delays occur

### 3. Capacity Analysis
- See real-time machine utilization
- Model "what-if" scenarios for new work
- Identify underutilized resources
- Plan capital equipment purchases

### 4. Executive Communication
- Walk C-suite through operations in 3D
- Present ITAR compliance boundaries visually
- Demonstrate cybersecurity zones
- Show ROI of process improvements

### 5. Operator Training
- Train on CNC setup without tying up machines
- Simulate crash scenarios safely
- Practice tool changes and part loading
- Learn shop workflow for new employees

### 6. CNC Programming Verification
- Visualize tool paths in 3D before cutting chips
- Check for collisions with fixtures
- Verify material removal
- Optimize cycle times

## Sample Data

The system comes pre-loaded with:

- **9 Departments**: Mill, Lathe, Programming, Saw, Shipping/Receiving, Deburr, Parts Cleaning, Office, Tool Crib
- **9 Machines**: 5 CNC Mills, 4 CNC Lathes
- **3 Sample Jobs**: Including an active NPI workflow
- **10+ Operations**: Showing job routing through departments

## Development Roadmap

This is a **24-week project** broken into 6 phases:

1. **Foundation** (Weeks 1-4) - Project setup, basic structure
2. **Core Visualization** (Weeks 5-8) - Department & machine modules
3. **Data Integration** (Weeks 9-12) - Live data connections
4. **NPI Workflow** (Weeks 13-16) - Timeline and tracking
5. **CNC Simulation** (Weeks 17-20) - Tool paths and material removal
6. **Polish & Optimization** (Weeks 21-24) - Performance and UX

👉 **Full roadmap**: [DEVELOPMENT_ROADMAP.md](./DEVELOPMENT_ROADMAP.md)

## Performance Considerations

### For Threshold Hardware

The system is optimized to run on mid-range hardware:

- **CPU**: Intel i5-8th gen or AMD Ryzen 5 3600
- **GPU**: NVIDIA GTX 1660 / AMD RX 5600 XT
- **RAM**: 16GB minimum
- **Storage**: 50GB SSD

### Optimization Strategies

- Aggressive LOD (Level of Detail) chains
- Selective use of Nanite and Lumen
- Instanced meshes for repeated objects
- Response caching and Redis for API
- Delta updates via WebSocket
- Scalability presets (Low/Medium/High/Cinematic)

## Security & Compliance

### ITAR Considerations

- Visual boundaries for controlled areas
- Access control integration
- Audit logging for all interactions
- Data encryption in transit and at rest

### Production Deployment

Before deploying to production:

1. ✅ Change all default passwords
2. ✅ Enable HTTPS with proper certificates
3. ✅ Implement OAuth 2.0 / JWT authentication
4. ✅ Add authorization policies
5. ✅ Enable comprehensive audit logging
6. ✅ Review and restrict CORS origins
7. ✅ Implement rate limiting
8. ✅ Add input validation and sanitization
9. ✅ Set up monitoring and alerting
10. ✅ Perform security audit

## Documentation

- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Detailed system architecture and design decisions
- **[DEVELOPMENT_ROADMAP.md](./DEVELOPMENT_ROADMAP.md)** - Complete 24-week development plan
- **[QUICK_START.md](./QUICK_START.md)** - Get up and running in 30 minutes
- **[UE5_SETUP.md](./UE5_SETUP.md)** - Step-by-step Unreal Engine 5 setup
- **[/Database/](./Database/)** - SQL schemas and sample data

## Contributing

This is currently a private project. Contributions are welcome from authorized team members:

1. Create a feature branch from `main`
2. Make your changes
3. Write/update tests
4. Update documentation
5. Submit pull request

## Troubleshooting

### Common Issues

**Docker containers won't start**
```bash
docker-compose -f docker-compose.dev.yml down
docker-compose -f docker-compose.dev.yml up -d
```

**API can't connect to database**
```bash
# Verify containers are running
docker-compose -f docker-compose.dev.yml ps

# Check PostgreSQL is ready
docker exec shopsteward_postgres pg_isready -U shopsteward
```

**UE5 performance issues**
- Lower quality settings in Project Settings
- Disable Lumen temporarily
- Reduce viewport resolution
- Close other applications

## Monitoring

### Grafana Dashboard

Access at http://localhost:3000 (admin/admin)

- Machine utilization over time
- Department throughput metrics
- API response times
- Database query performance

### API Health Check

http://localhost:5000/health

Returns status of all services (PostgreSQL, TimescaleDB, Redis)

## Future Enhancements

Planned features for future releases:

- 🚀 VR mode for immersive walkthroughs
- 🚀 AR overlays for on-floor guidance
- 🚀 AI-driven optimization recommendations
- 🚀 Integration with major ERP/MES systems (SAP, Oracle, etc.)
- 🚀 Mobile companion app
- 🚀 Multi-facility support
- 🚀 Supplier/customer collaboration portals
- 🚀 Advanced physics simulation
- 🚀 Digital twin for quality inspection
- 🚀 Predictive maintenance integration

## License

[MIT License](./LICENSE.txt)

## Support & Contact

For questions, issues, or feedback:

- **Documentation**: Check the guides in this repo first
- **API Issues**: Review logs with `docker-compose logs -f`
- **UE5 Issues**: Check Output Log in Unreal Editor
- **GitHub Issues**: Submit detailed bug reports or feature requests

---

## Getting Started

Ready to dive in? Choose your path:

- **I want to run the API**: → [QUICK_START.md](./QUICK_START.md)
- **I want to understand the system**: → [ARCHITECTURE.md](./ARCHITECTURE.md)
- **I want to build the UE5 project**: → [UE5_SETUP.md](./UE5_SETUP.md)
- **I want to see the roadmap**: → [DEVELOPMENT_ROADMAP.md](./DEVELOPMENT_ROADMAP.md)

**Questions?** Start with the documentation above or explore the `/Database/` folder to see the data model.

---

Built with ⚙️ for manufacturing excellence
