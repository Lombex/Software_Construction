import json
import sqlite3
from datetime import datetime
from pathlib import Path
import uuid

# Paths to JSON files
USERS_FILE = 'users.json'
VEHICLES_FILE = 'vehicles.json'
RESERVATIONS_FILE = 'reservations.json'
PARKING_LOTS_FILE = 'parking-lots.json'

# Path to SQLite database
DB_PATH = r'CSharp Parking API/Database/parking.db'

def parse_date(date_str):
    if not date_str:
        return None
    try:
        if 'T' in date_str:
            return datetime.fromisoformat(date_str.replace('Z', '+00:00'))
        return datetime.fromisoformat(date_str)
    except Exception:
        return None

def to_guid(id_str):
    try:
        return str(uuid.UUID(id_str))
    except Exception:
        # fallback: generate deterministic UUID from string
        return str(uuid.uuid5(uuid.NAMESPACE_DNS, id_str))

def insert_users(cursor, users):
    for user in users:
        db_user = {
            'id': to_guid(user['id']),
            'username': user.get('username'),
            'password': user.get('password'),
            'name': user.get('name'),
            'email': user.get('email'),
            'phone': user.get('phone'),
            'role': 0 if user.get('role', '').upper() == 'USER' else 1,  # Map roles as needed
            'parking_lot_id': None,
            'created_at': parse_date(user.get('created_at')).isoformat() if parse_date(user.get('created_at')) else None,
            'birth_year': str(user.get('birth_year')) if user.get('birth_year') else None,
            'active': int(user.get('active', True)),
        }
        columns = ', '.join(db_user.keys())
        placeholders = ', '.join(['?'] * len(db_user))
        sql = f"INSERT OR IGNORE INTO Users ({columns}) VALUES ({placeholders})"
        cursor.execute(sql, tuple(db_user.values()))

def insert_vehicles(cursor, vehicles):
    for vehicle in vehicles:
        db_vehicle = {
            'id': to_guid(vehicle['id']),
            'user_id': to_guid(vehicle['user_id']),
            'license_plate': vehicle.get('license_plate'),
            'make': vehicle.get('make'),
            'model': vehicle.get('model'),
            'color': vehicle.get('color'),
            'year': datetime(vehicle.get('year'), 1, 1).isoformat() if isinstance(vehicle.get('year'), int) else (parse_date(vehicle.get('year')).isoformat() if parse_date(vehicle.get('year')) else None),
            'created_at': parse_date(vehicle.get('created_at')).isoformat() if parse_date(vehicle.get('created_at')) else None,
        }
        columns = ', '.join(db_vehicle.keys())
        placeholders = ', '.join(['?'] * len(db_vehicle))
        sql = f"INSERT OR IGNORE INTO Vehicles ({columns}) VALUES ({placeholders})"
        cursor.execute(sql, tuple(db_vehicle.values()))

def insert_reservations(cursor, reservations):
    for reservation in reservations:
        db_res = {
            'id': to_guid(reservation['id']),
            'user_id': to_guid(reservation['user_id']),
            'parking_lot_id': to_guid(reservation['parking_lot_id']),
            'vehicle_id': to_guid(reservation['vehicle_id']),
            'start_time': parse_date(reservation.get('start_time')).isoformat() if parse_date(reservation.get('start_time')) else None,
            'end_time': parse_date(reservation.get('end_time')).isoformat() if parse_date(reservation.get('end_time')) else None,
            'status': 2 if reservation.get('status') == 'confirmed' else 0,  # Map as needed
            'created_at': parse_date(reservation.get('created_at')).isoformat() if parse_date(reservation.get('created_at')) else None,
            'cost': reservation.get('cost', 0.0),
        }
        columns = ', '.join(db_res.keys())
        placeholders = ', '.join(['?'] * len(db_res))
        sql = f"INSERT OR IGNORE INTO Reservations ({columns}) VALUES ({placeholders})"
        cursor.execute(sql, tuple(db_res.values()))

def insert_parkinglots(cursor, parkinglots):
    for lot in parkinglots.values() if isinstance(parkinglots, dict) else parkinglots:
        db_lot = {
            'id': to_guid(lot['id']),
            'name': lot.get('name'),
            'location': lot.get('location'),
            'address': lot.get('address'),
            'capacity': lot.get('capacity', 0),
            'reserved': lot.get('reserved', 0),
            'daytarriff': lot.get('daytariff') or lot.get('daytarriff', 0.0),
            'created_at': parse_date(lot.get('created_at')).isoformat() if parse_date(lot.get('created_at')) else None,
            'coordinates_lat': lot.get('coordinates', {}).get('lat') if lot.get('coordinates') else None,
            'coordinates_lng': lot.get('coordinates', {}).get('lng') if lot.get('coordinates') else None,
        }
        columns = ', '.join(db_lot.keys())
        placeholders = ', '.join(['?'] * len(db_lot))
        sql = f"INSERT OR IGNORE INTO Parkinglots ({columns}) VALUES ({placeholders})"
        cursor.execute(sql, tuple(db_lot.values()))

def insert_payments(cursor, payments):
    for payment in payments:
        db_payment = {
            'transactions': payment.get('transaction'),
            'amount': payment.get('amount'),
            'initiator': payment.get('initiator'),
            'created_at': payment.get('created_at'),
            'completed': payment.get('completed'),
            'hash': payment.get('hash'),
            # The following fields are omitted or set to None/dummy for now:
            'id': payment.get('hash'),
            'reservation_id': None,
            'paid_at': payment.get('created_at'),
            'session_id': None,
            'parking_lot_id': None,
        }
        # Remove t_data fields from flat insert, as they are owned/complex
        columns = ', '.join(db_payment.keys())
        placeholders = ', '.join(['?'] * len(db_payment))
        sql = f"INSERT OR IGNORE INTO Payments ({columns}) VALUES ({placeholders})"
        cursor.execute(sql, tuple(db_payment.values()))

def main():
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    # Load and insert users
    if Path(USERS_FILE).exists():
        with open(USERS_FILE, encoding='utf-8') as f:
            users = json.load(f)
        insert_users(cursor, users)
    # Load and insert vehicles
    if Path(VEHICLES_FILE).exists():
        with open(VEHICLES_FILE, encoding='utf-8') as f:
            vehicles = json.load(f)
        insert_vehicles(cursor, vehicles)
    # Load and insert reservations
    if Path(RESERVATIONS_FILE).exists():
        with open(RESERVATIONS_FILE, encoding='utf-8') as f:
            reservations = json.load(f)
        insert_reservations(cursor, reservations)
    # Load and insert parking lots
    if Path(PARKING_LOTS_FILE).exists():
        with open(PARKING_LOTS_FILE, encoding='utf-8') as f:
            parkinglots = json.load(f)
        insert_parkinglots(cursor, parkinglots)
    # Insert a sample payment (linked to real reservation, parking lot, vehicle)
    # Load the first reservation for linking
    reservation = None
    if Path(RESERVATIONS_FILE).exists():
        with open(RESERVATIONS_FILE, encoding='utf-8') as f:
            reservations = json.load(f)
        if isinstance(reservations, list) and reservations:
            reservation = reservations[0]
    if reservation:
        sample_payment = {
            "transaction": "1535349fea5cca288b217d491838f836AA",
            "amount": 5.5,
            "initiator": "testuser",
            "created_at": "22-05-2025 09:09:1747898315",
            "completed": "22-05-2025 09:09:1747898330",
            "hash": "d15acc14-02c3-4c9d-b047-c6b16befc302",
            "reservation_id": to_guid(reservation["id"]),
            "parking_lot_id": to_guid(reservation["parking_lot_id"]),
            "vehicle_id": to_guid(reservation["vehicle_id"]),
            "t_data": {
                "amount": 5.5,
                "date": "2025-05-22 22:22:22",
                "method": "ideal",
                "issuer": "XYY910HH",
                "bank": "ABN-NL"
            }
        }
        def insert_payments_linked(cursor, payments):
            for payment in payments:
                db_payment = {
                    'transactions': payment.get('transaction'),
                    'amount': payment.get('amount'),
                    'initiator': payment.get('initiator'),
                    'created_at': payment.get('created_at'),
                    'completed': payment.get('completed'),
                    'hash': payment.get('hash'),
                    'id': payment.get('hash'),
                    'reservation_id': payment.get('reservation_id'),
                    'paid_at': payment.get('created_at'),
                    'session_id': None,
                    'parking_lot_id': payment.get('parking_lot_id'),
                }
                columns = ', '.join(db_payment.keys())
                placeholders = ', '.join(['?'] * len(db_payment))
                sql = f"INSERT OR IGNORE INTO Payments ({columns}) VALUES ({placeholders})"
                cursor.execute(sql, tuple(db_payment.values()))
        insert_payments_linked(cursor, [sample_payment])
    conn.commit()
    conn.close()
    print('Data import completed.')

if __name__ == '__main__':
    main()
