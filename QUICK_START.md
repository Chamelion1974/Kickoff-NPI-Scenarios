# Quick Start Guide - Shop Steward Hub Digital Twin

This guide will help you get the development environment running in under 30 minutes.

## Prerequisites

Before you begin, ensure you have:

- [x] Docker Desktop installed and running
- [x] .NET 9.0 SDK installed
- [x] VS Code Insiders (or regular VS Code)
- [ ] Git LFS installed (`git lfs install`)
- [ ] At least 16GB RAM available
- [ ] 50GB free disk space

## Step 1: Clone and Setup (5 minutes)

```bash
# Clone the repository
cd /path/to/your/projects
git clone <repository-url>
cd Kickoff-NPI-Scenarios

# Verify .NET SDK
dotnet --version  # Should show 9.0.x

# Restore NuGet packages
dotnet restore
```

## Step 2: Start Docker Services (10 minutes)

```bash
# Start all backend services
docker-compose -f docker-compose.dev.yml up -d

# Verify all containers are running
docker-compose -f docker-compose.dev.yml ps

# You should see:
# - shopsteward_timescale (TimescaleDB)
# - shopsteward_postgres (PostgreSQL)
# - shopsteward_redis (Redis)
# - shopsteward_grafana (Optional monitoring)
```

### Wait for Database Initialization

The databases will automatically run initialization scripts. Wait ~30 seconds, then verify:

```bash
# Check PostgreSQL is ready
docker exec shopsteward_postgres pg_isready -U shopsteward

# Check TimescaleDB is ready
docker exec shopsteward_timescale pg_isready -U shopsteward

# Check Redis is ready
docker exec shopsteward_redis redis-cli -a dev_password_change_in_prod ping
```

All should respond with "ready" or "PONG".

## Step 3: Run the API (5 minutes)

```bash
# Build the project
dotnet build

# Run migrations (if needed)
# dotnet ef database update --context MetadataDbContext

# Start the API
dotnet run

# The API will start on:
# - HTTP:  http://localhost:5000
# - HTTPS: https://localhost:5001
```

### Verify API is Running

Open your browser and navigate to:

- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

You should see the interactive API documentation and a healthy status.

## Step 4: Test the Endpoints (5 minutes)

### Using Swagger UI

1. Open http://localhost:5000/swagger
2. Try these endpoints:
   - `GET /api/departments` - Should return 9 departments
   - `GET /api/machines` - Should return sample machines
   - `GET /api/jobs/active` - Should return sample jobs

### Using curl

```bash
# Get all departments
curl http://localhost:5000/api/departments

# Get all machines
curl http://localhost:5000/api/machines

# Get active jobs
curl http://localhost:5000/api/jobs/active
```

### Test WebSocket Connection

```javascript
// Open browser console and paste:
const connection = new WebSocket('ws://localhost:5000/ws/realtime');
connection.onopen = () => console.log('Connected to Digital Twin!');
connection.onmessage = (msg) => console.log('Received:', msg.data);
```

## Step 5: Explore the Data (5 minutes)

### View in Grafana (Optional)

1. Open http://localhost:3000
2. Login: `admin` / `admin`
3. Add PostgreSQL data source:
   - Host: `shopsteward_postgres:5432`
   - Database: `shopsteward_metadata`
   - User: `shopsteward`
   - Password: `dev_password_change_in_prod`

### Query the Database Directly

```bash
# Connect to PostgreSQL
docker exec -it shopsteward_postgres psql -U shopsteward -d shopsteward_metadata

# Run some queries:
SELECT id, name, type FROM departments;
SELECT id, name, type, department_id FROM machines;
SELECT job_number, part_number, status FROM job_routings;

# Exit with \q
```

## Next Steps

### For API Development

1. Review `/ARCHITECTURE.md` for system design
2. Review `/DEVELOPMENT_ROADMAP.md` for implementation phases
3. Explore the service layer in `/Source/Services/`
4. Add new endpoints in `/Source/API/Controllers/`

### For UE5 Integration

1. **Create UE5 Project** (see UE5_SETUP.md)
2. **Install HTTP Plugin**: VaRest or built-in HTTP module
3. **Install WebSocket Plugin**: SocketIOClient or native WebSocket
4. **Test API Connection**: Create blueprint to call `/api/departments`

### For Database Development

1. Schema files: `/Database/init-postgres.sql` and `/Database/init-timescale.sql`
2. Migrations: Use EF Core migrations for schema changes
3. Seed data: Add to initialization scripts

## Troubleshooting

### Docker containers won't start

```bash
# Check Docker is running
docker info

# Stop all containers and restart
docker-compose -f docker-compose.dev.yml down
docker-compose -f docker-compose.dev.yml up -d

# Check logs
docker-compose -f docker-compose.dev.yml logs -f
```

### API won't connect to database

```bash
# Verify connection strings in appsettings.json
cat appsettings.Development.json

# Test PostgreSQL connection
docker exec -it shopsteward_postgres psql -U shopsteward -d shopsteward_metadata -c "SELECT 1;"

# Check API logs
dotnet run --verbosity detailed
```

### Port conflicts

If ports 5000, 5432, 5433, 6379, or 3000 are already in use:

1. Edit `docker-compose.dev.yml` to change port mappings
2. Update `appsettings.json` connection strings accordingly

### Redis connection issues

```bash
# Test Redis connection
docker exec -it shopsteward_redis redis-cli -a dev_password_change_in_prod

# Once connected, try:
PING  # Should return PONG
INFO  # Shows server info
```

## Development Workflow

### Making Code Changes

```bash
# 1. Make your changes in VS Code
# 2. Build
dotnet build

# 3. Run (with hot reload)
dotnet watch run

# 4. Test your changes in Swagger
```

### Adding a New Endpoint

1. Create/modify service in `/Source/Services/`
2. Add controller method in `/Source/API/Controllers/`
3. Test in Swagger
4. Document in API comments

### Database Schema Changes

```bash
# 1. Modify models in /Source/Models/
# 2. Create migration
dotnet ef migrations add YourMigrationName --context MetadataDbContext

# 3. Apply migration
dotnet ef database update --context MetadataDbContext
```

## Sample Data

The databases are seeded with:

- **9 Departments**: Mill, Lathe, Programming, Saw, Shipping/Receiving, Deburr, Parts Cleaning, Office, Tool Crib
- **9 Machines**: 5 Mills, 4 Lathes
- **3 Sample Jobs**: Including an active NPI job
- **10+ Operations**: Across different departments

## API Endpoints Reference

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/departments` | GET | All departments with machines and metrics |
| `/api/departments/{id}` | GET | Specific department details |
| `/api/departments/{id}/metrics` | GET | Real-time department metrics |
| `/api/machines` | GET | All machines |
| `/api/machines/{id}` | GET | Specific machine details |
| `/api/machines/{id}/status` | GET | Current machine status |
| `/api/machines/department/{id}` | GET | All machines in a department |
| `/api/jobs/active` | GET | All active jobs |
| `/api/jobs/{id}` | GET | Specific job details |
| `/api/jobs/status/{status}` | GET | Jobs filtered by status |
| `/api/workflows/npi/{jobId}` | GET | NPI workflow visualization data |
| `/api/workflows/npi/active` | GET | All active NPI workflows |

## WebSocket Events

Subscribe to real-time updates:

| Event | Description | Payload |
|-------|-------------|---------|
| `MachineStatusUpdate` | Machine status changed | `MachineStatusUpdateMessage` |
| `JobProgressUpdate` | Job progress updated | `JobProgressUpdateMessage` |
| `AlarmEvent` | Machine alarm triggered | `AlarmEventMessage` |

## Performance Tips

- Use Redis caching for frequently accessed data
- Enable Response Caching on GET endpoints
- Use `Include()` strategically in EF queries
- Monitor with Grafana dashboard
- Use SignalR for real-time updates instead of polling

## Security Notes

**WARNING**: This is a development environment setup. Before production:

1. Change all default passwords
2. Enable HTTPS with proper certificates
3. Implement authentication (OAuth 2.0 / JWT)
4. Add authorization policies
5. Enable audit logging
6. Review CORS allowed origins
7. Implement rate limiting
8. Add input validation and sanitization

## Resources

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System architecture
- [DEVELOPMENT_ROADMAP.md](./DEVELOPMENT_ROADMAP.md) - Development plan
- [Database Schema](./Database/) - SQL initialization scripts
- [Swagger UI](http://localhost:5000/swagger) - Interactive API docs
- [Grafana](http://localhost:3000) - Monitoring dashboard

## Support

For issues or questions:

1. Check the troubleshooting section above
2. Review the architecture documentation
3. Examine Docker logs: `docker-compose logs -f`
4. Check API logs in console output

---

**You're all set!** The API is running and ready to connect to Unreal Engine 5. Next step: Create your UE5 project (see `UE5_SETUP.md`).
