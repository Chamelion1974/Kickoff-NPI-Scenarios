# main.py - Shop Steward Hub API
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional

app = FastAPI(title="Shop Steward Hub API")

# Allow UE5 and frontend to call this API
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# ============================================================================
# MODELS
# ============================================================================

class Job(BaseModel):
    job_number: str
    customer: str
    part_number: str
    operations: List[str]

class KickoffChecklist(BaseModel):
    drawing_reviewed: bool
    operations_defined: bool
    material_confirmed: bool
    tooling_verified: bool
    risks_flagged: List[str]

class TimelineEvent(BaseModel):
    day: Optional[int] = None
    week: Optional[int] = None
    event: str
    status: str

class ScenarioOutcome(BaseModel):
    profit: int
    customer_satisfaction: str
    ending: str

class Scenario(BaseModel):
    timeline: List[TimelineEvent]
    outcome: ScenarioOutcome

# ============================================================================
# SIMPLE IN-MEMORY STORAGE (Replace with database later)
# ============================================================================

jobs_db = {}

# ============================================================================
# ENDPOINTS
# ============================================================================

@app.get("/")
def root():
    """Root endpoint - API health check"""
    return {
        "message": "Shop Steward Hub API",
        "version": "1.0.0",
        "status": "running"
    }

@app.post("/api/jobs")
async def create_job(job: Job):
    """Create a new job in the system"""
    # Store job in our simple in-memory database
    jobs_db[job.job_number] = job.dict()

    return {
        "status": "success",
        "job_id": job.job_number,
        "message": f"Job {job.job_number} created successfully"
    }

@app.get("/api/jobs/{job_id}")
async def get_job(job_id: str):
    """Get job details by job ID"""
    if job_id not in jobs_db:
        raise HTTPException(status_code=404, detail=f"Job {job_id} not found")

    return jobs_db[job_id]

@app.get("/api/jobs/{job_id}/kickoff")
async def get_kickoff_checklist(job_id: str):
    """Return kickoff checklist for a specific job"""
    # In a real app, this would check the database
    # For now, return a sample checklist

    return KickoffChecklist(
        drawing_reviewed=False,
        operations_defined=False,
        material_confirmed=False,
        tooling_verified=False,
        risks_flagged=[]
    )

@app.get("/api/scenario/{scenario_type}")
def get_scenario(scenario_type: str):
    """
    Returns data for Utopia or Dystopia scenario
    This is what UE5 will call to get the timeline and outcome data
    """

    if scenario_type.lower() == "utopia":
        return Scenario(
            timeline=[
                TimelineEvent(
                    day=1,
                    event="Kickoff meeting - EDM feature caught early",
                    status="success"
                ),
                TimelineEvent(
                    day=2,
                    event="Customer approved upcharge for EDM work",
                    status="success"
                ),
                TimelineEvent(
                    day=5,
                    event="Material ordered with correct specs",
                    status="success"
                ),
                TimelineEvent(
                    week=2,
                    event="First article inspection - PASSED",
                    status="success"
                ),
                TimelineEvent(
                    week=18,
                    event="Parts shipped on time, customer thrilled",
                    status="success"
                )
            ],
            outcome=ScenarioOutcome(
                profit=35000,
                customer_satisfaction="high",
                ending="boat"
            )
        )

    elif scenario_type.lower() == "dystopia":
        return Scenario(
            timeline=[
                TimelineEvent(
                    day=1,
                    event="Job entered without kickoff meeting",
                    status="warning"
                ),
                TimelineEvent(
                    week=1,
                    event="Started machining without drawing review",
                    status="warning"
                ),
                TimelineEvent(
                    week=3,
                    event="First article FAILURE - EDM feature discovered too late",
                    status="critical"
                ),
                TimelineEvent(
                    week=5,
                    event="Customer refuses upcharge, demands rework",
                    status="critical"
                ),
                TimelineEvent(
                    week=12,
                    event="Scrambling for EDM vendor, losing money daily",
                    status="critical"
                ),
                TimelineEvent(
                    week=16,
                    event="Parts shipped late, customer furious",
                    status="failure"
                ),
                TimelineEvent(
                    week=18,
                    event="Customer moves business to competitor",
                    status="failure"
                )
            ],
            outcome=ScenarioOutcome(
                profit=-135000,
                customer_satisfaction="lost",
                ending="for_sale_sign"
            )
        )

    else:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid scenario type: {scenario_type}. Use 'utopia' or 'dystopia'"
        )

@app.get("/api/jobs")
async def list_jobs():
    """List all jobs in the system"""
    return {
        "jobs": list(jobs_db.values()),
        "count": len(jobs_db)
    }

# ============================================================================
# RUN THE APP
# ============================================================================

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
