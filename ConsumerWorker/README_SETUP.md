# 🚀 IronGrid Consumer - Setup Instructions

## 📋 Prerequisites
- Docker & Docker Compose running
- MySQL client (optional, for verification)

---

## 🔧 Step 1: Start Docker Services

```bash
cd /Users/natanelamar/Desktop/Courses/Kod-Kod-5/מגמות\ /Exam_middel
docker compose up -d
```

Verify services are running:
```bash
docker compose ps
```

---

## 🗄️ Step 2: Initialize Database

### Option A: Using MySQL CLI (Recommended)

```bash
mysql -h 127.0.0.1 -P 3306 -u root -proot < ConsumerWorker/seed_database.sql
```

### Option B: Using Docker Exec

```bash
docker exec -i $(docker compose ps -q db) mysql -uroot -proot < ConsumerWorker/seed_database.sql
```

### Option C: Manual (MySQL Workbench / phpMyAdmin)

1. Connect to MySQL: `localhost:3306`, user: `root`, password: `root`
2. Open `ConsumerWorker/seed_database.sql`
3. Execute the entire script

---

## ✅ Step 3: Verify Database

```bash
mysql -h 127.0.0.1 -P 3306 -u root -proot -e "
USE testDb;
SELECT 'Units:' as Table_Name, COUNT(*) as Count FROM Units
UNION ALL
SELECT 'Assets:', COUNT(*) FROM Assets
UNION ALL
SELECT 'UAVs:', COUNT(*) FROM Assets WHERE Type = 0
UNION ALL
SELECT 'Sensors:', COUNT(*) FROM Assets WHERE Type = 1;
"
```

**Expected Output:**
```
+------------+-------+
| Table_Name | Count |
+------------+-------+
| Units:     |   100 |
| Assets:    |   100 |
| UAVs:      |    50 |
| Sensors:   |    50 |
+------------+-------+
```

---

## 🎯 Step 4: Run Consumer Application

```bash
cd ConsumerWorker
dotnet run
```

---

## 📊 Database Schema

### Tables Created:

1. **Units** (100 records)
   - `Id` (PK)
   - `UnitName`
   - `Sector`

2. **Assets** (100 records)
   - `Id` (PK)
   - `UnitId` (FK → Units)
   - `AssetSerial`
   - `Type` (0=UAV, 1=PerimeterSensor, 2=GenericAsset)

3. **AssetLiveStatuses** (empty, populated by Kafka)
   - `Id` (PK, AUTO_INCREMENT)
   - `AssetId` (FK → Assets, UNIQUE)
   - `AssetType`
   - `RawValue`
   - `ProcessedStatus`
   - `IsVerified`
   - `LastUpdate`

### Relationships:
- **Unit → Assets**: One-to-Many
- **Asset → AssetLiveStatus**: One-to-One (or Zero-to-One)

---

## 🔍 Troubleshooting

### Database connection failed
```bash
# Check if MySQL is running
docker compose ps

# Restart MySQL
docker compose restart db
```

### Can't connect to MySQL
```bash
# Test connection
mysql -h 127.0.0.1 -P 3306 -u root -proot -e "SELECT 1;"
```

### Reset database
```bash
mysql -h 127.0.0.1 -P 3306 -u root -proot -e "DROP DATABASE IF EXISTS testDb;"
mysql -h 127.0.0.1 -P 3306 -u root -proot < ConsumerWorker/seed_database.sql
```

---

## 📝 Notes for Students

- **No EF Migrations needed!** The SQL script creates everything.
- The `OnModelCreating` in `IronGridDbContext.cs` defines relationships for EF Core to use at runtime.
- `AssetLiveStatuses` table starts empty and gets populated when Kafka messages arrive.
- Each Asset can have only ONE live status (latest report).

---

## 🎓 What You Need to Implement

1. ✅ Database is ready (done by SQL script)
2. ✅ Models are defined (Unit, Asset, AssetLiveStatus)
3. ✅ DbContext is configured
4. ✅ DataProcessingService has the business logic
5. ⚠️ **TODO**: Implement Kafka Consumer Service
6. ⚠️ **TODO**: Wire up Program.cs to consume messages and call DataProcessingService

---

Good luck! 🚀
