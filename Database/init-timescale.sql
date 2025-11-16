-- Shop Steward Hub Digital Twin - TimescaleDB Time-Series Database
-- This database stores real-time machine telemetry and historical performance data

-- Enable TimescaleDB extension
CREATE EXTENSION IF NOT EXISTS timescaledb;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================================
-- MACHINE STATUS HISTORY (Real-time telemetry)
-- ============================================================================
CREATE TABLE machine_status_history (
    time TIMESTAMPTZ NOT NULL,
    machine_id UUID NOT NULL,

    -- Status
    status VARCHAR(20) NOT NULL,  -- 'Offline', 'Idle', 'Running', 'Paused', 'Alarm', 'Maintenance'

    -- Utilization metrics
    utilization_percent FLOAT,  -- 0.0 to 100.0
    spindle_load FLOAT,         -- Percentage of max load
    feed_rate FLOAT,            -- Current feed rate
    rapid_rate FLOAT,           -- Current rapid rate

    -- Machine-specific telemetry
    spindle_speed INT,          -- RPM
    spindle_override INT,       -- Percentage (usually 100%)
    feed_override INT,          -- Percentage
    axis_x_position FLOAT,
    axis_y_position FLOAT,
    axis_z_position FLOAT,

    -- Current job info
    job_id UUID,
    operation_id UUID,
    program_number VARCHAR(50),
    block_number INT,           -- Current G-code line

    -- Alarms and errors
    alarm_code VARCHAR(20),
    alarm_message TEXT,

    -- Power consumption (if available)
    power_consumption_watts FLOAT,

    -- Temperature sensors (if available)
    spindle_temp_celsius FLOAT,
    coolant_temp_celsius FLOAT
);

-- Convert to hypertable (time-series optimized)
SELECT create_hypertable('machine_status_history', 'time');

-- Create indexes for common queries
CREATE INDEX idx_machine_status_machine_time ON machine_status_history (machine_id, time DESC);
CREATE INDEX idx_machine_status_status ON machine_status_history (status, time DESC);
CREATE INDEX idx_machine_status_job ON machine_status_history (job_id, time DESC) WHERE job_id IS NOT NULL;

-- ============================================================================
-- JOB PROGRESS HISTORY (Track job completion over time)
-- ============================================================================
CREATE TABLE job_progress_history (
    time TIMESTAMPTZ NOT NULL,
    job_id UUID NOT NULL,
    operation_id UUID NOT NULL,
    machine_id UUID,

    -- Progress
    percent_complete FLOAT NOT NULL,  -- 0.0 to 100.0
    pieces_completed INT NOT NULL DEFAULT 0,
    pieces_scrapped INT NOT NULL DEFAULT 0,

    -- Time tracking
    actual_hours FLOAT,
    estimated_remaining_hours FLOAT,

    -- Status changes
    status VARCHAR(20) NOT NULL,

    -- Events
    event_type VARCHAR(50),  -- 'Started', 'Paused', 'Resumed', 'Completed', 'Scrapped'
    event_notes TEXT
);

SELECT create_hypertable('job_progress_history', 'time');

CREATE INDEX idx_job_progress_job_time ON job_progress_history (job_id, time DESC);
CREATE INDEX idx_job_progress_operation ON job_progress_history (operation_id, time DESC);
CREATE INDEX idx_job_progress_event ON job_progress_history (event_type, time DESC);

-- ============================================================================
-- DEPARTMENT METRICS (Aggregated department-level data)
-- ============================================================================
CREATE TABLE department_metrics_history (
    time TIMESTAMPTZ NOT NULL,
    department_id UUID NOT NULL,

    -- Utilization
    average_utilization FLOAT,
    machine_count INT,
    active_machine_count INT,
    idle_machine_count INT,
    alarm_machine_count INT,

    -- Throughput
    parts_completed_hour INT,
    parts_scrapped_hour INT,
    jobs_completed_hour INT,

    -- Queue metrics
    queue_depth INT,
    average_queue_wait_minutes FLOAT,

    -- Performance
    oee_percent FLOAT,  -- Overall Equipment Effectiveness
    availability_percent FLOAT,
    performance_percent FLOAT,
    quality_percent FLOAT
);

SELECT create_hypertable('department_metrics_history', 'time');

CREATE INDEX idx_dept_metrics_dept_time ON department_metrics_history (department_id, time DESC);

-- ============================================================================
-- ALARM EVENTS (Track all alarms and their resolution)
-- ============================================================================
CREATE TABLE alarm_events (
    time TIMESTAMPTZ NOT NULL,
    machine_id UUID NOT NULL,

    -- Alarm details
    alarm_code VARCHAR(20) NOT NULL,
    alarm_category VARCHAR(50),  -- 'Crash', 'Maintenance', 'Operator', 'System'
    alarm_severity VARCHAR(20),  -- 'Critical', 'Warning', 'Info'
    alarm_message TEXT,

    -- Context
    job_id UUID,
    operation_id UUID,
    program_number VARCHAR(50),
    block_number INT,

    -- Resolution
    cleared_at TIMESTAMPTZ,
    downtime_minutes FLOAT,
    cleared_by VARCHAR(100),
    resolution_notes TEXT,

    -- Impact
    parts_affected INT,
    cost_impact_usd DECIMAL(10, 2)
);

SELECT create_hypertable('alarm_events', 'time');

CREATE INDEX idx_alarm_events_machine ON alarm_events (machine_id, time DESC);
CREATE INDEX idx_alarm_events_code ON alarm_events (alarm_code, time DESC);
CREATE INDEX idx_alarm_events_severity ON alarm_events (alarm_severity, time DESC);
CREATE INDEX idx_alarm_events_uncleared ON alarm_events (machine_id, time DESC) WHERE cleared_at IS NULL;

-- ============================================================================
-- TOOL USAGE HISTORY (Track tool life and changes)
-- ============================================================================
CREATE TABLE tool_usage_history (
    time TIMESTAMPTZ NOT NULL,
    machine_id UUID NOT NULL,
    tool_number INT NOT NULL,

    -- Tool identity
    tool_id VARCHAR(50),
    tool_description TEXT,

    -- Usage
    total_cuts INT,
    total_time_minutes FLOAT,
    remaining_life_percent FLOAT,

    -- Events
    event_type VARCHAR(50),  -- 'Loaded', 'Changed', 'Broken', 'Expired'
    event_notes TEXT,

    -- Job context
    job_id UUID,
    operation_id UUID
);

SELECT create_hypertable('tool_usage_history', 'time');

CREATE INDEX idx_tool_usage_machine ON tool_usage_history (machine_id, time DESC);
CREATE INDEX idx_tool_usage_tool ON tool_usage_history (tool_id, time DESC);
CREATE INDEX idx_tool_usage_event ON tool_usage_history (event_type, time DESC);

-- ============================================================================
-- CONTINUOUS AGGREGATES (Pre-computed rollups for fast queries)
-- ============================================================================

-- Hourly machine utilization
CREATE MATERIALIZED VIEW machine_utilization_hourly
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 hour', time) AS bucket,
    machine_id,
    AVG(utilization_percent) AS avg_utilization,
    MAX(utilization_percent) AS max_utilization,
    MIN(utilization_percent) AS min_utilization,
    COUNT(*) FILTER (WHERE status = 'Running') AS running_count,
    COUNT(*) FILTER (WHERE status = 'Idle') AS idle_count,
    COUNT(*) FILTER (WHERE status = 'Alarm') AS alarm_count,
    AVG(spindle_load) AS avg_spindle_load,
    AVG(power_consumption_watts) AS avg_power_watts
FROM machine_status_history
GROUP BY bucket, machine_id
WITH NO DATA;

SELECT add_continuous_aggregate_policy('machine_utilization_hourly',
    start_offset => INTERVAL '3 hours',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour');

-- Daily department metrics
CREATE MATERIALIZED VIEW department_metrics_daily
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 day', time) AS bucket,
    department_id,
    AVG(average_utilization) AS avg_utilization,
    SUM(parts_completed_hour) AS total_parts_completed,
    SUM(parts_scrapped_hour) AS total_parts_scrapped,
    AVG(oee_percent) AS avg_oee,
    AVG(queue_depth) AS avg_queue_depth,
    MAX(queue_depth) AS max_queue_depth
FROM department_metrics_history
GROUP BY bucket, department_id
WITH NO DATA;

SELECT add_continuous_aggregate_policy('department_metrics_daily',
    start_offset => INTERVAL '3 days',
    end_offset => INTERVAL '1 day',
    schedule_interval => INTERVAL '1 day');

-- ============================================================================
-- RETENTION POLICIES (Automatically remove old data)
-- ============================================================================

-- Keep raw machine status for 30 days
SELECT add_retention_policy('machine_status_history', INTERVAL '30 days');

-- Keep job progress for 90 days
SELECT add_retention_policy('job_progress_history', INTERVAL '90 days');

-- Keep department metrics for 1 year
SELECT add_retention_policy('department_metrics_history', INTERVAL '1 year');

-- Keep alarm events for 1 year
SELECT add_retention_policy('alarm_events', INTERVAL '1 year');

-- Keep tool usage for 6 months
SELECT add_retention_policy('tool_usage_history', INTERVAL '6 months');

-- ============================================================================
-- SAMPLE DATA GENERATION
-- ============================================================================

-- Function to generate realistic machine status data
CREATE OR REPLACE FUNCTION generate_sample_machine_data(
    p_machine_id UUID,
    p_start_time TIMESTAMPTZ,
    p_end_time TIMESTAMPTZ,
    p_interval INTERVAL DEFAULT INTERVAL '1 second'
)
RETURNS void AS $$
DECLARE
    v_current_time TIMESTAMPTZ := p_start_time;
    v_status VARCHAR(20);
    v_utilization FLOAT;
    v_spindle_load FLOAT;
BEGIN
    WHILE v_current_time <= p_end_time LOOP
        -- Simulate varying conditions
        CASE (EXTRACT(EPOCH FROM v_current_time)::INT % 100)
            WHEN 0 THEN
                v_status := 'Idle';
                v_utilization := 0.0;
                v_spindle_load := 0.0;
            WHEN 90 THEN
                v_status := 'Alarm';
                v_utilization := 0.0;
                v_spindle_load := 0.0;
            ELSE
                v_status := 'Running';
                v_utilization := 60.0 + (RANDOM() * 35.0);  -- 60-95%
                v_spindle_load := 40.0 + (RANDOM() * 40.0);  -- 40-80%
        END CASE;

        INSERT INTO machine_status_history (
            time,
            machine_id,
            status,
            utilization_percent,
            spindle_load,
            feed_rate,
            spindle_speed,
            spindle_override,
            feed_override,
            power_consumption_watts
        ) VALUES (
            v_current_time,
            p_machine_id,
            v_status,
            v_utilization,
            v_spindle_load,
            500.0 + (RANDOM() * 500.0),  -- Feed rate
            6000 + (RANDOM() * 6000)::INT,  -- Spindle speed
            100,  -- Override
            100,  -- Override
            2000.0 + (RANDOM() * 3000.0)  -- Power consumption
        );

        v_current_time := v_current_time + p_interval;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

-- Generate sample data for the last 24 hours for demo purposes
-- (This will be called by the API or data generator service)

-- ============================================================================
-- USEFUL QUERIES / VIEWS
-- ============================================================================

-- Current machine status (latest record for each machine)
CREATE VIEW v_current_machine_status AS
SELECT DISTINCT ON (machine_id)
    machine_id,
    time,
    status,
    utilization_percent,
    spindle_load,
    job_id,
    operation_id,
    program_number,
    alarm_code,
    alarm_message
FROM machine_status_history
ORDER BY machine_id, time DESC;

-- Active alarms (not yet cleared)
CREATE VIEW v_active_alarms AS
SELECT
    machine_id,
    time,
    alarm_code,
    alarm_category,
    alarm_severity,
    alarm_message,
    job_id,
    EXTRACT(EPOCH FROM (NOW() - time))/60 AS minutes_active
FROM alarm_events
WHERE cleared_at IS NULL
ORDER BY alarm_severity DESC, time DESC;

-- Real-time department summary
CREATE VIEW v_realtime_department_summary AS
WITH latest_metrics AS (
    SELECT DISTINCT ON (department_id)
        department_id,
        time,
        average_utilization,
        machine_count,
        active_machine_count,
        queue_depth,
        oee_percent
    FROM department_metrics_history
    ORDER BY department_id, time DESC
)
SELECT
    department_id,
    time AS last_update,
    average_utilization,
    machine_count,
    active_machine_count,
    queue_depth,
    oee_percent,
    CASE
        WHEN average_utilization >= 80 THEN 'High'
        WHEN average_utilization >= 50 THEN 'Medium'
        ELSE 'Low'
    END AS utilization_status
FROM latest_metrics;

COMMENT ON DATABASE shopsteward_timeseries IS 'Shop Steward Hub Digital Twin - Time-Series Database for Machine Telemetry';
