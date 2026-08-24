# Military Asset Tracking System - Exam Project

## Project Overview
This project is a microservices-based military asset tracking system that monitors and manages field reports for military units and their assets (UAVs and Perimeter Sensors). The system uses Kafka for message streaming, MySQL for data persistence, Redis for caching, and ASP.NET Core for the API layer.

## Architecture
- **ProducerService**: Reads field reports and publishes them to Kafka
- **ConsumerWorker**: Consumes messages from Kafka and processes them into the database
- **ApiService**: REST API for querying asset statuses and unit information
- **Infrastructure**: Kafka, MySQL, Redis (all running in Docker containers)

---

## Setup and Running Instructions

### Prerequisites
- Docker and Docker Compose installed
- .NET 8.0 SDK installed

### Step 1: Review the Database Schema
Before running the project, please review the student's database schema and seed data:
```bash
seed_database.sql
```

This file contains the complete database structure including:
- Units table (military units with sectors)
- Assets table (UAVs and sensors)
- AssetLiveStatuses table (real-time asset status tracking)
- Sample data for testing

### Step 2: Start Docker Infrastructure
Start all required services (Kafka, MySQL, Redis):
```bash
docker-compose up -d
```

Wait approximately 30 seconds for all services to initialize properly.

Verify all containers are running:
```bash
docker ps
```

You should see three containers: `broker` (Kafka), `db` (MySQL), and `redis-db`.

### Step 3: Initialize the Database
Inject the seed data into the MySQL database:
```bash
docker exec -i db mysql -uroot -proot < ConsumerWorker/seed_database.sql
```

Verify the database was created successfully:
```bash
docker exec -it db mysql -uroot -proot -e "USE testDb; SHOW TABLES;"
```

### Step 4: Run the Producer Service
The Producer reads field reports and publishes them to Kafka:
```bash
dotnet run
```

The Producer will:
- Load field reports from `data/field_reports.json`
- Publish messages to the Kafka topic 
- Display progress in the console

**Note**: Keep this terminal open or let it complete its execution.

### Step 5: Run the Consumer Worker
Open a **new terminal** and run the Consumer:
```bash

dotnet run
```

The Consumer will:
- Subscribe to the Kafka topic 
- Process incoming messages
- Update the MySQL database with asset statuses
- Cache results in Redis


### Step 6: Run the API Service
Open a **third terminal** and run the API:
```bash
dotnet run
```
---

## Project Structure
```
.
├── ProducerService/          # Kafka producer service
│   ├── data/                 # Field reports JSON data
│   ├── Services/             # Kafka producer logic
│   └── Program.cs
├── ConsumerWorker/           # Kafka consumer service
│   ├── Services/             # Message processing logic
│   ├── seed_database.sql     # Database schema and seed data
│   └── Program.cs
├── ApiService/               # REST API
│   ├── Controllers/          # API endpoints
│   ├── Repositories/         # Data access layer
│   ├── Services/             # Business logic
│   └── Program.cs
└── docker-compose.yaml       # Infrastructure configuration
```

---

**Good luck with the evaluation! ***