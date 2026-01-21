import requests
import json
from datetime import datetime
from pathlib import Path
import sqlite3
import re
import getpass

Admin_Token = {"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlMjQ3MDJmMy0yMzA4LTRkYzItODk5NS04ZGEyNTI4YjI4MDgiLCJ1bmlxdWVfbmFtZSI6InN1cGVyYWRtaW4iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImUyNDcwMmYzLTIzMDgtNGRjMi04OTk1LThkYTI1MjhiMjgwOCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzdXBlcmFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3VwZXJBZG1pbiIsIm5iZiI6MTc2OTAyMjM4NiwiZXhwIjoxNzY5MDI5NTg2LCJpc3MiOiJDU2hhcnBQYXJraW5nQVBJIiwiYXVkIjoiQ1NoYXJwUGFya2luZ0FQSUNsaWVudHMifQ.L6XPE8puQZZQAmpPYsODPYwFICEoaD0z1jriEr4ASDk"}

DATA_FILES = [
    {
        "file": "parking-lots.json",
        "endpoint": "http://localhost:5001/api/v2/parkinglots"
    },
    # Add more files as needed
]

TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlMjQ3MDJmMy0yMzA4LTRkYzItODk5NS04ZGEyNTI4YjI4MDgiLCJ1bmlxdWVfbmFtZSI6InN1cGVyYWRtaW4iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImUyNDcwMmYzLTIzMDgtNGRjMi04OTk1LThkYTI1MjhiMjgwOCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzdXBlcmFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3VwZXJBZG1pbiIsIm5iZiI6MTc2OTAyMjM4NiwiZXhwIjoxNzY5MDI5NTg2LCJpc3MiOiJDU2hhcnBQYXJraW5nQVBJIiwiYXVkIjoiQ1NoYXJwUGFya2luZ0FQSUNsaWVudHMifQ.L6XPE8puQZZQAmpPYsODPYwFICEoaD0z1jriEr4ASDk"
if not TOKEN or TOKEN == Admin_Token:
    print("This script requires an admin token. Please paste your admin token below (input is hidden):")
    TOKEN = getpass.getpass("Admin token: ")
ERROR_LOG = "import_errors.log.json"

headers = {
    "Authorization": f"Bearer {TOKEN}",
    "Content-Type": "application/json"
}

def log_error(item, endpoint, response):
    error_entry = {
        "endpoint": endpoint,
        "item": item,
        "status_code": response.status_code,
        "error": response.text,
        "timestamp": datetime.utcnow().isoformat()
    }
    try:
        with open(ERROR_LOG, "a", encoding="utf-8") as f:
            f.write(json.dumps(error_entry) + "\n")
    except Exception as e:
        print("Fout bij loggen:", e)

for data_info in DATA_FILES:
    file_path = data_info["file"]
    endpoint = data_info["endpoint"]
    if not Path(file_path).exists():
        print(f"Bestand niet gevonden: {file_path}")
        continue
    with open(file_path, "r", encoding="utf-8") as f:
        try:
            data = json.load(f)
        except Exception as e:
            print(f"Fout bij laden van {file_path}: {e}")
            continue
    print(f"Importeren uit {file_path} naar {endpoint}...")

    # Only process top-level objects (no flattening)
    if isinstance(data, dict):
        items = list(data.values())
    elif isinstance(data, list):
        items = data
    else:
        print(f"Onbekend dataformaat in {file_path}")
        continue

    for item in items:
        obj = dict(item) if isinstance(item, dict) else item
        if isinstance(obj, dict):
            obj.pop("id", None)
            if "tariff" in obj:
                obj.pop("tariff", None)
        # Validation for parking lot required fields
        is_parking_lot = "daytariff" in obj or "daytarriff" in obj
        if is_parking_lot:
            missing_fields = []
            for field in ["name", "location", "coordinates"]:
                if field not in obj or obj[field] in (None, ""):
                    missing_fields.append(field)
            if missing_fields:
                print(f"Skipping parking lot due to missing required fields: {', '.join(missing_fields)}. Data: {obj}")
                continue
        # Always send the object directly (no wrapper), keeps it generic
        payload = obj
        response = requests.post(endpoint, headers=headers, json=payload)
        ref = obj.get('id') if isinstance(obj, dict) else obj
        if response.ok:
            print(f"Toegevoegd aan {endpoint}: {ref}")
        else:
            print(f"Fout bij {ref}: {response.status_code}")
            log_error(obj, endpoint, response)

# DB insert logic
conn = sqlite3.connect(r'C:\Users\noah\OneDrive - Hogeschool Rotterdam\Documenten\Software-Construction-git\Software_Construction\CSharp Parking API\Database\parking.db')
cursor = conn.cursor()
table_name = ""
for item in items:
    IsHashed = False
    if not isinstance(item, dict):
        continue
    db_item = item.copy()
    api_payload = item
    table_name = None
    # Parkinglots
    if 'daytariff' in db_item or 'daytarriff' in db_item:
        table_name = 'Parkinglots'
        if 'daytariff' in db_item:
            db_item['daytarriff'] = db_item.pop('daytariff')
        db_item.pop('tariff', None)
        if 'coordinates' in db_item:
            db_item['coordinates_lat'] = db_item['coordinates']['lat']
            db_item['coordinates_lng'] = db_item['coordinates']['lng']
            db_item.pop('coordinates', None)
        api_payload = {"lot": item}
    # Reservations
    elif 'user_id' in db_item and 'parking_lot_id' in db_item and 'start_time' in db_item:
        table_name = 'Reservations'
        for date_field in ['start_time', 'end_time', 'created_at']:
            if date_field in db_item and isinstance(db_item[date_field], str):
                try:
                    db_item[date_field] = datetime.fromisoformat(db_item[date_field])
                except Exception:
                    pass
    # Vehicles
    elif 'license_plate' in db_item and 'user_id' in db_item:
        table_name = 'Vehicles'
        if 'year' in db_item:
            if isinstance(db_item['year'], int):
                db_item['year'] = datetime(db_item['year'], 1, 1)
            elif isinstance(db_item['year'], str):
                try:
                    db_item['year'] = datetime.fromisoformat(db_item['year'])
                except Exception:
                    pass
        if 'created_at' in db_item and isinstance(db_item['created_at'], str):
            try:
                db_item['created_at'] = datetime.fromisoformat(db_item['created_at'])
            except Exception:
                pass
    if table_name:
        columns = ', '.join(db_item.keys())
        placeholders = ', '.join(['?'] * len(db_item))
        sql = f"INSERT INTO {table_name} ({columns}) VALUES ({placeholders})"
        # print(f"SQL: {sql}")
        try:
            cursor.execute(sql, tuple(db_item.values()))
        except Exception as e:
            print(f"Error inserting {db_item.get('id', db_item.get('license_plate', ''))}: {e}")
    else:
        print(f"Error: table_name is not set for item: {db_item}")
conn.commit()
conn.close()

def looks_like_hash(s):
    s = str(s)
    hash_lengths = [32, 40, 64, 128]
    if len(s) in hash_lengths and re.fullmatch(r'[0-9a-fA-F]+', s):
        return True
    if len(s) % 4 == 0 and re.fullmatch(r'[A-Za-z0-9+/=]+', s):
        if len(s) >= 22:
            return True
    return False
