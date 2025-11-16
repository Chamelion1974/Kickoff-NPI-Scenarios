-- Shop Steward Hub Digital Twin - PostgreSQL Metadata Database
-- This database stores configuration, definitions, and metadata

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================================
-- DEPARTMENTS
-- ============================================================================
CREATE TABLE departments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,  -- 'CNCMill', 'CNCLathe', 'Programming', etc.
    description TEXT,

    -- Physical layout in UE5 coordinates
    size_x FLOAT NOT NULL DEFAULT 10.0,
    size_y FLOAT NOT NULL DEFAULT 10.0,
    size_z FLOAT NOT NULL DEFAULT 3.0,
    position_x FLOAT NOT NULL DEFAULT 0.0,
    position_y FLOAT NOT NULL DEFAULT 0.0,
    position_z FLOAT NOT NULL DEFAULT 0.0,
    rotation FLOAT NOT NULL DEFAULT 0.0,  -- Degrees

    -- Configuration
    max_machines INT NOT NULL DEFAULT 10,
    is_active BOOLEAN NOT NULL DEFAULT true,

    -- Metadata
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT valid_department_type CHECK (
        type IN ('CNCMill', 'CNCLathe', 'Programming', 'Saw',
                 'ShippingReceiving', 'Deburr', 'PartsCleaning',
                 'FrontOffice', 'ToolCrib')
    )
);

CREATE INDEX idx_departments_type ON departments(type);
CREATE INDEX idx_departments_active ON departments(is_active);

-- ============================================================================
-- MACHINES
-- ============================================================================
CREATE TABLE machines (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    department_id UUID NOT NULL REFERENCES departments(id) ON DELETE CASCADE,

    -- Identity
    name VARCHAR(100) NOT NULL UNIQUE,
    machine_number VARCHAR(50),
    type VARCHAR(50) NOT NULL,  -- '3AxisMill', '5AxisMill', 'TurningCenter', etc.
    model VARCHAR(100),
    manufacturer VARCHAR(100),
    serial_number VARCHAR(100),

    -- Installation
    installation_date DATE,
    last_maintenance_date DATE,
    next_maintenance_date DATE,

    -- Physical location within department
    position_x FLOAT NOT NULL DEFAULT 0.0,
    position_y FLOAT NOT NULL DEFAULT 0.0,
    position_z FLOAT NOT NULL DEFAULT 0.0,
    rotation FLOAT NOT NULL DEFAULT 0.0,

    -- Capabilities
    max_spindle_speed INT,  -- RPM
    max_feed_rate FLOAT,    -- mm/min or in/min
    work_envelope_x FLOAT,  -- mm or inches
    work_envelope_y FLOAT,
    work_envelope_z FLOAT,
    number_of_axes INT,
    has_tool_changer BOOLEAN DEFAULT false,
    tool_capacity INT,

    -- Configuration
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_maintenance BOOLEAN NOT NULL DEFAULT false,
    controller_type VARCHAR(50),  -- 'Fanuc', 'Haas', 'Mazak', etc.
    controller_ip VARCHAR(45),

    -- Metadata
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_machines_department ON machines(department_id);
CREATE INDEX idx_machines_type ON machines(type);
CREATE INDEX idx_machines_active ON machines(is_active);
CREATE INDEX idx_machines_name ON machines(name);

-- ============================================================================
-- JOB ROUTINGS (NPI and Production)
-- ============================================================================
CREATE TABLE job_routings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    -- Job identity
    job_number VARCHAR(50) NOT NULL UNIQUE,
    part_number VARCHAR(50) NOT NULL,
    revision VARCHAR(10),
    description TEXT,
    customer VARCHAR(100),

    -- Job details
    quantity INT NOT NULL,
    quantity_completed INT NOT NULL DEFAULT 0,
    priority INT NOT NULL DEFAULT 5,  -- 1 (highest) to 10 (lowest)
    job_type VARCHAR(20) NOT NULL,  -- 'NPI', 'Production', 'Rework', 'Prototype'

    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'Created',  -- 'Created', 'Released', 'InProgress', 'OnHold', 'Completed', 'Cancelled'

    -- Dates
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    released_at TIMESTAMP,
    started_at TIMESTAMP,
    due_date TIMESTAMP,
    completed_at TIMESTAMP,

    -- Estimates
    estimated_total_hours FLOAT,
    actual_total_hours FLOAT,

    -- ITAR flag
    is_itar BOOLEAN NOT NULL DEFAULT false,

    -- Metadata
    created_by VARCHAR(100),
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT valid_job_status CHECK (
        status IN ('Created', 'Released', 'InProgress', 'OnHold', 'Completed', 'Cancelled')
    ),
    CONSTRAINT valid_job_type CHECK (
        job_type IN ('NPI', 'Production', 'Rework', 'Prototype')
    )
);

CREATE INDEX idx_job_routings_number ON job_routings(job_number);
CREATE INDEX idx_job_routings_part ON job_routings(part_number);
CREATE INDEX idx_job_routings_status ON job_routings(status);
CREATE INDEX idx_job_routings_type ON job_routings(job_type);
CREATE INDEX idx_job_routings_due ON job_routings(due_date);

-- ============================================================================
-- OPERATIONS (Steps in a job routing)
-- ============================================================================
CREATE TABLE operations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    routing_id UUID NOT NULL REFERENCES job_routings(id) ON DELETE CASCADE,

    -- Sequencing
    sequence INT NOT NULL,  -- 10, 20, 30, etc.
    operation_code VARCHAR(20) NOT NULL,  -- '10', '20', '30', etc.
    description TEXT,

    -- Routing
    department_id UUID NOT NULL REFERENCES departments(id),
    machine_id UUID REFERENCES machines(id),  -- NULL if not assigned yet

    -- Estimates
    estimated_setup_hours FLOAT NOT NULL DEFAULT 0.0,
    estimated_cycle_hours FLOAT NOT NULL DEFAULT 0.0,
    estimated_total_hours FLOAT GENERATED ALWAYS AS (
        estimated_setup_hours + estimated_cycle_hours
    ) STORED,

    -- Actuals
    actual_setup_hours FLOAT,
    actual_cycle_hours FLOAT,
    actual_total_hours FLOAT GENERATED ALWAYS AS (
        COALESCE(actual_setup_hours, 0) + COALESCE(actual_cycle_hours, 0)
    ) STORED,

    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',  -- 'Pending', 'InProgress', 'Completed', 'Skipped'

    -- Dates
    started_at TIMESTAMP,
    completed_at TIMESTAMP,

    -- Program info
    program_number VARCHAR(50),
    program_path TEXT,
    gcode_path TEXT,

    -- Quality
    requires_first_article BOOLEAN NOT NULL DEFAULT false,
    first_article_completed BOOLEAN NOT NULL DEFAULT false,
    inspection_required BOOLEAN NOT NULL DEFAULT false,

    -- Metadata
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT valid_operation_status CHECK (
        status IN ('Pending', 'InProgress', 'Completed', 'Skipped')
    ),
    CONSTRAINT unique_routing_sequence UNIQUE (routing_id, sequence)
);

CREATE INDEX idx_operations_routing ON operations(routing_id);
CREATE INDEX idx_operations_department ON operations(department_id);
CREATE INDEX idx_operations_machine ON operations(machine_id);
CREATE INDEX idx_operations_status ON operations(status);
CREATE INDEX idx_operations_sequence ON operations(routing_id, sequence);

-- ============================================================================
-- SHOP LAYOUTS (Saved configurations)
-- ============================================================================
CREATE TABLE shop_layouts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    layout_type VARCHAR(20) NOT NULL,  -- 'Small', 'Medium', 'Large', 'Custom'
    is_active BOOLEAN NOT NULL DEFAULT false,  -- Only one can be active

    -- JSON blob of complete layout
    layout_data JSONB NOT NULL,

    -- Metadata
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100)
);

CREATE INDEX idx_shop_layouts_active ON shop_layouts(is_active);

-- ============================================================================
-- USERS (Basic user management)
-- ============================================================================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    full_name VARCHAR(200),
    role VARCHAR(50) NOT NULL,  -- 'Admin', 'Manager', 'Operator', 'Viewer'
    is_active BOOLEAN NOT NULL DEFAULT true,

    -- Permissions
    can_edit_layout BOOLEAN NOT NULL DEFAULT false,
    can_modify_jobs BOOLEAN NOT NULL DEFAULT false,
    can_view_itar BOOLEAN NOT NULL DEFAULT false,

    -- Metadata
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);

-- ============================================================================
-- SEED DATA
-- ============================================================================

-- Create default departments
INSERT INTO departments (name, type, size_x, size_y, size_z, position_x, position_y, max_machines) VALUES
    ('Mill Department', 'CNCMill', 15.0, 20.0, 3.5, 0.0, 0.0, 8),
    ('Lathe Department', 'CNCLathe', 12.0, 18.0, 3.5, 20.0, 0.0, 6),
    ('Programming Room', 'Programming', 8.0, 10.0, 3.0, 40.0, 0.0, 4),
    ('Saw Area', 'Saw', 6.0, 8.0, 3.5, 55.0, 0.0, 2),
    ('Shipping & Receiving', 'ShippingReceiving', 12.0, 15.0, 4.0, 0.0, 25.0, 0),
    ('Deburr Station', 'Deburr', 8.0, 10.0, 3.0, 20.0, 25.0, 4),
    ('Parts Cleaning', 'PartsCleaning', 6.0, 8.0, 3.0, 35.0, 25.0, 2),
    ('Front Office', 'FrontOffice', 10.0, 12.0, 3.0, 50.0, 25.0, 0),
    ('Tool Crib', 'ToolCrib', 8.0, 10.0, 3.0, 65.0, 0.0, 0);

-- Create sample machines
INSERT INTO machines (department_id, name, machine_number, type, model, manufacturer, installation_date,
                     position_x, position_y, number_of_axes, has_tool_changer, tool_capacity, controller_type)
SELECT
    d.id,
    'Mill-' || ROW_NUMBER() OVER (PARTITION BY d.id ORDER BY d.id),
    'M' || LPAD(ROW_NUMBER() OVER (PARTITION BY d.id ORDER BY d.id)::TEXT, 3, '0'),
    '3AxisMill',
    'VF-4SS',
    'Haas',
    CURRENT_DATE - INTERVAL '2 years',
    (ROW_NUMBER() OVER (PARTITION BY d.id ORDER BY d.id) - 1) * 3.0,
    0.0,
    3,
    true,
    24,
    'Haas NGC'
FROM departments d
WHERE d.type = 'CNCMill'
LIMIT 5;

INSERT INTO machines (department_id, name, machine_number, type, model, manufacturer, installation_date,
                     position_x, position_y, number_of_axes, has_tool_changer, tool_capacity, controller_type)
SELECT
    d.id,
    'Lathe-' || ROW_NUMBER() OVER (PARTITION BY d.id ORDER BY d.id),
    'L' || LPAD(ROW_NUMBER() OVER (PARTITION BY d.id ORDER BY d.id)::TEXT, 3, '0'),
    'TurningCenter',
    'ST-20Y',
    'Haas',
    CURRENT_DATE - INTERVAL '1 year',
    (ROW_NUMBER() OVER (PARTITION BY d.id ORDER BY d.id) - 1) * 2.5,
    0.0,
    2,
    true,
    12,
    'Haas NGC'
FROM departments d
WHERE d.type = 'CNCLathe'
LIMIT 4;

-- Create sample jobs
INSERT INTO job_routings (job_number, part_number, revision, description, customer, quantity, priority, job_type, status, released_at, due_date)
VALUES
    ('JOB-001', 'PN-12345', 'A', 'Gear Housing - Aluminum', 'Acme Corp', 10, 1, 'NPI', 'InProgress', CURRENT_TIMESTAMP - INTERVAL '2 days', CURRENT_TIMESTAMP + INTERVAL '5 days'),
    ('JOB-002', 'PN-67890', 'B', 'Shaft - Steel', 'Beta Industries', 25, 3, 'Production', 'InProgress', CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP + INTERVAL '7 days'),
    ('JOB-003', 'PN-11111', 'A', 'Bracket - Titanium', 'Gamma Systems', 5, 2, 'Prototype', 'Released', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + INTERVAL '3 days');

-- Create operations for JOB-001 (NPI workflow)
INSERT INTO operations (routing_id, sequence, operation_code, description, department_id, estimated_setup_hours, estimated_cycle_hours, status, requires_first_article)
SELECT
    jr.id,
    10,
    '10',
    'Program and Verify',
    d.id,
    4.0,
    1.0,
    'Completed',
    false
FROM job_routings jr, departments d
WHERE jr.job_number = 'JOB-001' AND d.type = 'Programming';

INSERT INTO operations (routing_id, sequence, operation_code, description, department_id, estimated_setup_hours, estimated_cycle_hours, status, requires_first_article)
SELECT
    jr.id,
    20,
    '20',
    'Mill Main Features',
    d.id,
    2.0,
    0.5,
    'InProgress',
    true
FROM job_routings jr, departments d
WHERE jr.job_number = 'JOB-001' AND d.type = 'CNCMill';

INSERT INTO operations (routing_id, sequence, operation_code, description, department_id, estimated_setup_hours, estimated_cycle_hours, status)
SELECT
    jr.id,
    30,
    '30',
    'Deburr',
    d.id,
    0.25,
    0.15,
    'Pending'
FROM job_routings jr, departments d
WHERE jr.job_number = 'JOB-001' AND d.type = 'Deburr';

-- Create admin user
INSERT INTO users (username, email, full_name, role, can_edit_layout, can_modify_jobs, can_view_itar)
VALUES ('admin', 'admin@shopsteward.local', 'System Administrator', 'Admin', true, true, true);

-- ============================================================================
-- FUNCTIONS & TRIGGERS
-- ============================================================================

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Apply update trigger to all tables
CREATE TRIGGER update_departments_updated_at BEFORE UPDATE ON departments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_machines_updated_at BEFORE UPDATE ON machines
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_job_routings_updated_at BEFORE UPDATE ON job_routings
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_operations_updated_at BEFORE UPDATE ON operations
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- VIEWS
-- ============================================================================

-- Active jobs with progress
CREATE VIEW v_active_jobs AS
SELECT
    jr.id,
    jr.job_number,
    jr.part_number,
    jr.description,
    jr.quantity,
    jr.quantity_completed,
    jr.priority,
    jr.status,
    jr.due_date,
    COUNT(o.id) AS total_operations,
    COUNT(o.id) FILTER (WHERE o.status = 'Completed') AS completed_operations,
    ROUND((COUNT(o.id) FILTER (WHERE o.status = 'Completed')::NUMERIC / NULLIF(COUNT(o.id), 0)) * 100, 2) AS percent_complete,
    SUM(o.estimated_total_hours) AS estimated_hours,
    SUM(o.actual_total_hours) AS actual_hours
FROM job_routings jr
LEFT JOIN operations o ON jr.id = o.routing_id
WHERE jr.status IN ('Released', 'InProgress')
GROUP BY jr.id;

-- Machine utilization summary
CREATE VIEW v_machine_summary AS
SELECT
    m.id,
    m.name,
    m.type,
    d.name AS department_name,
    m.is_active,
    m.is_maintenance,
    COUNT(o.id) FILTER (WHERE o.status = 'InProgress') AS active_jobs,
    COUNT(o.id) FILTER (WHERE o.status = 'Pending') AS queued_jobs
FROM machines m
JOIN departments d ON m.department_id = d.id
LEFT JOIN operations o ON m.id = o.machine_id
GROUP BY m.id, d.name;

-- Department utilization
CREATE VIEW v_department_utilization AS
SELECT
    d.id,
    d.name,
    d.type,
    COUNT(DISTINCT m.id) AS machine_count,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'InProgress') AS active_operations,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'Pending') AS pending_operations
FROM departments d
LEFT JOIN machines m ON d.id = m.department_id AND m.is_active = true
LEFT JOIN operations o ON d.id = o.department_id
GROUP BY d.id;

COMMENT ON DATABASE shopsteward_metadata IS 'Shop Steward Hub Digital Twin - Metadata and Configuration Database';
