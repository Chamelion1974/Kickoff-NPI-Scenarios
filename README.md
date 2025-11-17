# Shop Steward Hub - Kickoff NPI Scenarios

A simple Python FastAPI backend for the Shop Steward Hub project. This API provides data for the Utopia vs Dystopia scenario demonstration in Unreal Engine 5.

## The Vision

**Show leadership the cost of skipping kickoff meetings through an immersive UE5 experience:**

- **Utopia Scenario**: Proper kickoff catches EDM requirement early → $35K profit → Buy a boat
- **Dystopia Scenario**: No kickoff, late discovery → $135K loss → FOR SALE sign on the shop

## Project Status

**Current Phase**: Python API Development (waiting for RAM upgrade for UE5)

**What's Done**:
- ✅ FastAPI backend with job management
- ✅ Scenario endpoints for Utopia/Dystopia timelines
- ✅ Docker configuration for easy deployment
- ✅ CORS enabled for UE5 integration

**Next Steps** (after RAM arrives):
1. Load Haas machine model in UE5
2. Create split-screen walkthrough
3. Connect UE5 Blueprint to this API
4. Display scenario data in 3D environment

## Why Python Instead of C#?

**Simple**: You already know Python. The C# .NET project was enterprise-grade overkill for an MVP.

**This gets you building in 30 minutes instead of weeks.**

The C# project is archived in git history - we can revisit it later if needed for enterprise integration.

## Quick Start

### Option 1: Docker (Recommended)

```bash
# Start everything (API + PostgreSQL)
docker-compose up -d

# Check logs
docker-compose logs -f backend

# Stop everything
docker-compose down
```

The API will be available at `http://localhost:8000`

### Option 2: Local Development

```bash
# Install dependencies
cd backend
pip install -r requirements.txt

# Run the server
uvicorn main:app --reload
```

## API Endpoints

### Health Check
```bash
curl http://localhost:8000/
```

### Scenario Data (For UE5)

**Get Utopia scenario:**
```bash
curl http://localhost:8000/api/scenario/utopia
```

Response:
```json
{
  "timeline": [
    {"day": 1, "event": "Kickoff meeting - EDM feature caught early", "status": "success"},
    {"day": 2, "event": "Customer approved upcharge for EDM work", "status": "success"},
    {"week": 18, "event": "Parts shipped on time, customer thrilled", "status": "success"}
  ],
  "outcome": {
    "profit": 35000,
    "customer_satisfaction": "high",
    "ending": "boat"
  }
}
```

**Get Dystopia scenario:**
```bash
curl http://localhost:8000/api/scenario/dystopia
```

Response shows the nightmare timeline leading to -$135K loss.

### Job Management

**Create a job:**
```bash
curl -X POST http://localhost:8000/api/jobs \
  -H "Content-Type: application/json" \
  -d '{
    "job_number": "12345",
    "customer": "Boeing",
    "part_number": "WNG-001",
    "operations": ["Mill", "EDM", "Inspect"]
  }'
```

**Get job details:**
```bash
curl http://localhost:8000/api/jobs/12345
```

**Get kickoff checklist:**
```bash
curl http://localhost:8000/api/jobs/12345/kickoff
```

**List all jobs:**
```bash
curl http://localhost:8000/api/jobs
```

## Interactive API Documentation

FastAPI auto-generates beautiful interactive docs:

- **Swagger UI**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

You can test all endpoints directly in the browser!

## Connecting to UE5

Once your RAM upgrade arrives, use Blueprints to fetch scenario data:

```
Blueprint: FetchScenarioData
├─ HTTP GET Request node
├─ URL: http://localhost:8000/api/scenario/utopia
├─ Parse JSON response
├─ Extract timeline events
└─ Display in 3D UI widgets
```

## Project Structure

```
Kickoff-NPI-Scenarios/
├── backend/
│   ├── main.py              # FastAPI application
│   ├── requirements.txt     # Python dependencies
│   └── Dockerfile          # Container configuration
├── docker-compose.yml      # Multi-container setup
└── README.md              # This file
```

## Development Notes

**In-Memory Storage**: Currently uses a simple dictionary for job storage. Data is lost when the server restarts. This is perfect for the demo - we'll add PostgreSQL persistence later if needed.

**CORS Enabled**: The API allows requests from any origin (`allow_origins=["*"]`) so UE5 can call it. Tighten this in production.

**Hot Reload**: The Docker setup mounts the backend directory, so code changes reload automatically.

## Future Enhancements

When we need them (not now):

- [ ] PostgreSQL persistence (Docker is already configured)
- [ ] User authentication
- [ ] Real-time updates via WebSockets
- [ ] Integration with actual shop floor systems
- [ ] Mobile app for shop floor workers

## The C# Project

The original .NET project is preserved in git history. We can return to it if we need:
- Native Windows integration
- ERP system connectivity
- Enterprise deployment requirements

For now, **Python + FastAPI gets us to the UE5 demo faster.**

## License

See LICENSE.txt

---

**Remember**: The goal is to show the boat vs the FOR SALE sign. Everything else is just a means to that end.
