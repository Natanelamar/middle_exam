# נתוני בדיקה ל-AssetsController

**בסיס URL לדוגמה:** `http://localhost:5000/api/assets`  
יש להחליף בהתאם לפורט שעליו רץ ה-ApiService (ניתן לראות ב-`launchSettings.json`).

---

## 1. קבלת Asset לפי ID

### 1.1 קבלת Asset קיים
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets/1`
- **Expected Status:** `200 OK`
- **Expected Body (דוגמה):**
  ```json
  {
    "id": 1,
    "unitId": 1,
    "assetSerial": "SENSOR-NORTH-872",
    "type": "PerimeterSensor"
  }
  ```

### 1.2 ID לא חוקי (שלילי או אפס)
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets/0`
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  Invalid Id
  ```

### 1.3 ID לא קיים
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets/9999`
- **Expected Status:** `404 Not Found`
- **Expected Body:** ריק

---

## 2. יצירת Unit חדש

### 2.1 יצירת Unit תקין
- **Method:** `POST`
- **URL:** `http://localhost:5000/api/assets/units`
- **Body:**
  ```json
  {
    "unitName": "Test Unit Alpha",
    "sector": "Northern Sector"
  }
  ```
- **Expected Status:** `201 Created`
- **Expected Body:** ריק (או האובייקט שנוצר, תלוי ברפוזיטוריה)

### 2.2 חסר UnitName
- **Method:** `POST`
- **URL:** `http://localhost:5000/api/assets/units`
- **Body:**
  ```json
  {
    "unitName": "",
    "sector": "Northern Sector"
  }
  ```
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  UnitName is required
  ```

### 2.3 UnitName רווחים בלבד
- **Method:** `POST`
- **URL:** `http://localhost:5000/api/assets/units`
- **Body:**
  ```json
  {
    "unitName": "   ",
    "sector": "Northern Sector"
  }
  ```
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  UnitName is required
  ```

### 2.4 חסר Sector
- **Method:** `POST`
- **URL:** `http://localhost:5000/api/assets/units`
- **Body:**
  ```json
  {
    "unitName": "Test Unit Beta"
  }
  ```
- **Expected Status:** `201 Created` (מכיוון שאין ולידציה על `Sector` בקונטרולר)
- **Expected Body:** ריק

---

## 3. עדכון Asset

### 3.1 עדכון Asset תקין
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/1`
- **Body:**
  ```json
  {
    "unitId": 2,
    "assetSerial": "UAV-NORTH-999",
    "type": "UAV"
  }
  ```
- **Expected Status:** `200 OK`
- **Expected Body (דוגמה):**
  ```json
  {
    "id": 1,
    "unitId": 2,
    "assetSerial": "UAV-NORTH-999",
    "type": "UAV"
  }
  ```

**הערה:** `type` צריך להיות מחרוזת (`"UAV"`, `"PerimeterSensor"`).

### 3.2 עדכון עם סוג כמחרוזת
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/2`
- **Body:**
  ```json
  {
    "unitId": 3,
    "assetSerial": "SENSOR-EAST-111",
    "type": "PerimeterSensor"
  }
  ```
- **Expected Status:** `200 OK`

### 3.3 ID לא חוקי
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/-1`
- **Body:**
  ```json
  {
    "unitId": 1,
    "assetSerial": "UAV-TEST-001",
    "type": 0
  }
  ```
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  Invalid Id
  ```

### 3.4 Asset לא נמצא
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/9999`
- **Body:**
  ```json
  {
    "unitId": 1,
    "assetSerial": "UAV-TEST-002",
    "type": 0
  }
  ```
- **Expected Status:** `404 Not Found`

### 3.5 Body ריק
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/1`
- **Body:**
  ```json
  {}
  ```
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  Asset is required
  ```

### 3.6 חסר AssetSerial
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/1`
- **Body:**
  ```json
  {
    "unitId": 1,
    "type": 0
  }
  ```
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  AssetSerial is required
  ```

### 3.7 AssetSerial ריק
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/1`
- **Body:**
  ```json
  {
    "unitId": 1,
    "assetSerial": "   ",
    "type": 0
  }
  ```
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  AssetSerial is required
  ```

### 3.8 סוג Asset לא חוקי (מחרוזת שלא קיימת ב-enum)
- **Method:** `PUT`
- **URL:** `http://localhost:5000/api/assets/1`
- **Body:**
  ```json
  {
    "unitId": 1,
    "assetSerial": "UAV-TEST-003",
    "type": "UnknownType"
  }
  ```
- **Expected Status:** `400 Bad Request`
- **Expected Body:** שגיאת JSON deserialization (ASP.NET יחזיר הודעה על שגיאת המרה ל-enum)

---

## 4. מחיקת Asset

### 4.1 מחיקת Asset קיים
- **Method:** `DELETE`
- **URL:** `http://localhost:5000/api/assets/1`
- **Expected Status:** `204 No Content`
- **Expected Body:** ריק

### 4.2 ID לא חוקי
- **Method:** `DELETE`
- **URL:** `http://localhost:5000/api/assets/0`
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  Invalid Id
  ```

### 4.3 ID לא קיים
- **Method:** `DELETE`
- **URL:** `http://localhost:5000/api/assets/9999`
- **Expected Status:** `404 Not Found`
- **Expected Body:** ריק

---

## 5. דוגמאות לבדיקה עם curl

### 5.1 קבלת Asset
```bash
curl -X GET http://localhost:5000/api/assets/1
```

### 5.2 יצירת Unit
```bash
curl -X POST http://localhost:5000/api/assets/units \
  -H "Content-Type: application/json" \
  -d '{"unitName":"Test Unit","sector":"North"}'
```

### 5.3 עדכון Asset
```bash
curl -X PUT http://localhost:5000/api/assets/1 \
  -H "Content-Type: application/json" \
  -d '{"unitId":2,"assetSerial":"UAV-TEST-01","type":"UAV"}'
```

### 5.4 מחיקת Asset
```bash
curl -X DELETE http://localhost:5000/api/assets/1
```

---

## 6. הערות חשובות

- ה-DB צריך להיות מאותחל עם `seed_database.sql` או `seed_data.sql` המעודכן.
- עמודת `Type` בטבלת `Assets` היא `VARCHAR(50)` והערכים המותרים הם `UAV` ו-`PerimeterSensor`.
- הקונטרולר תומך ב-`Type` כ-`enum` בקוד, אבל שומר אותו כמחרוזת ב-DB בעזרת `HasConversion<string>()`.

---

## 7. בדיקות ל-AssetsStatusController

**בסיס URL לדוגמה:** `http://localhost:5000/api/assets-status`

### 7.1 קבלת כל המצבים
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets-status`
- **Expected Status:** `200 OK`
- **Expected Body (רשימה, דוגמה לפריט אחד):**
  ```json
  [
    {
      "assetId": 1,
      "assetSerial": "SENSOR-NORTH-872",
      "assetType": "PerimeterSensor",
      "unitName": "Alpha Unit",
      "sector": "North",
      "rawValue": "42.5",
      "processedStatus": "Stable",
      "isVerified": true,
      "asset": {
        "id": 1,
        "unitId": 1,
        "assetSerial": "SENSOR-NORTH-872",
        "type": "PerimeterSensor"
      },
      "lastUpdate": "2026-08-23T10:00:00"
    }
  ]
  ```

### 7.2 סינון לפי סטטוס
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets-status/status?status=Stable`
- **Expected Status:** `200 OK`
- **Expected Body:** רשימה של סטטוסים שבהם `ProcessedStatus` = `Stable`

**ערכים חוקיים ל-`status`:** `Stable`, `Warning`.

### 7.3 חסר פרמטר status
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets-status/status`
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  Status is required
  ```

### 7.4 ערך status לא חוקי
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets-status/status?status=InvalidStatus`
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  Invalid status value
  ```

### 7.5 קבלת מצב לפי Asset ID
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets-status/1`
- **Expected Status:** `200 OK`
- **Expected Body (דוגמה):**
  ```json
  {
    "assetId": 1,
    "assetSerial": "SENSOR-NORTH-872",
    "assetType": "PerimeterSensor",
    "unitName": "Alpha Unit",
    "sector": "North",
    "rawValue": "42.5",
    "processedStatus": "Stable",
    "isVerified": true,
    "asset": {
      "id": 1,
      "unitId": 1,
      "assetSerial": "SENSOR-NORTH-872",
      "type": "PerimeterSensor"
    },
    "lastUpdate": "2026-08-23T10:00:00"
  }
  ```

### 7.6 Asset ID לא חוקי
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets-status/0`
- **Expected Status:** `400 Bad Request`
- **Expected Body:**
  ```
  Invalid Id
  ```

### 7.7 Asset ID לא קיים
- **Method:** `GET`
- **URL:** `http://localhost:5000/api/assets-status/9999`
- **Expected Status:** `404 Not Found`
- **Expected Body:** ריק

---

## 8. דוגמאות לבדיקה עם curl ל-AssetsStatusController

### 8.1 כל המצבים
```bash
curl -X GET http://localhost:5000/api/assets-status
```

### 8.2 סינון לפי סטטוס
```bash
curl -X GET "http://localhost:5000/api/assets-status/status?status=Stable"
```

### 8.3 מצב לפי Asset ID
```bash
curl -X GET http://localhost:5000/api/assets-status/1
```
