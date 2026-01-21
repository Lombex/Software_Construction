CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Parkinglots" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Parkinglots" PRIMARY KEY,
    "name" TEXT NULL,
    "location" TEXT NULL,
    "address" TEXT NULL,
    "capacity" INTEGER NOT NULL,
    "reserved" INTEGER NOT NULL,
    "daytarriff" REAL NOT NULL,
    "created_at" TEXT NOT NULL,
    "coordinates_lat" REAL NULL,
    "coordinates_lng" REAL NULL
);

CREATE TABLE "Payments" (
    "hash" TEXT NOT NULL CONSTRAINT "PK_Payments" PRIMARY KEY,
    "transactions" TEXT NULL,
    "amount" REAL NOT NULL,
    "initiator" TEXT NULL,
    "created_at" TEXT NOT NULL,
    "completed" TEXT NOT NULL,
    "t_data_amount" REAL NULL,
    "t_data_date" TEXT NULL,
    "t_data_method" TEXT NULL,
    "t_data_issuer" TEXT NULL,
    "t_data_bank" TEXT NULL,
    "session_id" TEXT NOT NULL,
    "parking_lot_id" TEXT NOT NULL
);

CREATE TABLE "Reservations" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Reservations" PRIMARY KEY,
    "user_id" TEXT NOT NULL,
    "parking_lot_id" TEXT NOT NULL,
    "vehicle_id" TEXT NOT NULL,
    "start_time" TEXT NOT NULL,
    "end_time" TEXT NOT NULL,
    "status" INTEGER NOT NULL,
    "created_at" TEXT NOT NULL,
    "cost" REAL NOT NULL
);

CREATE TABLE "Sessions" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Sessions" PRIMARY KEY,
    "parking_lot_id" TEXT NOT NULL,
    "license_plate" TEXT NULL,
    "started" TEXT NOT NULL,
    "stopped" TEXT NOT NULL,
    "user" TEXT NULL,
    "duration_minutes" INTEGER NOT NULL,
    "cost" REAL NOT NULL,
    "status" INTEGER NOT NULL
);

CREATE TABLE "Users" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
    "username" TEXT NULL,
    "password" TEXT NULL,
    "name" TEXT NULL,
    "email" TEXT NULL,
    "phone" TEXT NULL,
    "role" INTEGER NOT NULL,
    "created_at" TEXT NOT NULL,
    "birth_year" TEXT NOT NULL,
    "active" INTEGER NOT NULL
);

CREATE TABLE "Vehicles" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Vehicles" PRIMARY KEY,
    "user_id" TEXT NOT NULL,
    "license_plate" TEXT NULL,
    "make" TEXT NULL,
    "model" TEXT NULL,
    "color" TEXT NULL,
    "year" TEXT NOT NULL,
    "created_at" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251030115143_InitialCreate', '9.0.9');

ALTER TABLE "Sessions" ADD "vehicle_id" TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "Payments" ADD "id" TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "Payments" ADD "paid_at" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE "Payments" ADD "reservation_id" TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE INDEX "IX_Vehicles_user_id" ON "Vehicles" ("user_id");

CREATE INDEX "IX_Sessions_parking_lot_id" ON "Sessions" ("parking_lot_id");

CREATE INDEX "IX_Sessions_vehicle_id" ON "Sessions" ("vehicle_id");

CREATE INDEX "IX_Reservations_parking_lot_id" ON "Reservations" ("parking_lot_id");

CREATE INDEX "IX_Reservations_user_id" ON "Reservations" ("user_id");

CREATE INDEX "IX_Reservations_vehicle_id" ON "Reservations" ("vehicle_id");

CREATE TABLE "ef_temp_Reservations" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Reservations" PRIMARY KEY,
    "cost" REAL NOT NULL,
    "created_at" TEXT NOT NULL,
    "end_time" TEXT NOT NULL,
    "parking_lot_id" TEXT NOT NULL,
    "start_time" TEXT NOT NULL,
    "status" INTEGER NOT NULL,
    "user_id" TEXT NOT NULL,
    "vehicle_id" TEXT NOT NULL,
    CONSTRAINT "FK_Reservations_Parkinglots_parking_lot_id" FOREIGN KEY ("parking_lot_id") REFERENCES "Parkinglots" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reservations_Users_user_id" FOREIGN KEY ("user_id") REFERENCES "Users" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reservations_Vehicles_vehicle_id" FOREIGN KEY ("vehicle_id") REFERENCES "Vehicles" ("id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Reservations" ("id", "cost", "created_at", "end_time", "parking_lot_id", "start_time", "status", "user_id", "vehicle_id")
SELECT "id", "cost", "created_at", "end_time", "parking_lot_id", "start_time", "status", "user_id", "vehicle_id"
FROM "Reservations";

CREATE TABLE "ef_temp_Sessions" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Sessions" PRIMARY KEY,
    "cost" REAL NOT NULL,
    "duration_minutes" INTEGER NOT NULL,
    "license_plate" TEXT NULL,
    "parking_lot_id" TEXT NOT NULL,
    "started" TEXT NOT NULL,
    "status" INTEGER NOT NULL,
    "stopped" TEXT NOT NULL,
    "user" TEXT NULL,
    "vehicle_id" TEXT NOT NULL,
    CONSTRAINT "FK_Sessions_Parkinglots_parking_lot_id" FOREIGN KEY ("parking_lot_id") REFERENCES "Parkinglots" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_Sessions_Vehicles_vehicle_id" FOREIGN KEY ("vehicle_id") REFERENCES "Vehicles" ("id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Sessions" ("id", "cost", "duration_minutes", "license_plate", "parking_lot_id", "started", "status", "stopped", "user", "vehicle_id")
SELECT "id", "cost", "duration_minutes", "license_plate", "parking_lot_id", "started", "status", "stopped", "user", "vehicle_id"
FROM "Sessions";

CREATE TABLE "ef_temp_Vehicles" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Vehicles" PRIMARY KEY,
    "color" TEXT NULL,
    "created_at" TEXT NOT NULL,
    "license_plate" TEXT NULL,
    "make" TEXT NULL,
    "model" TEXT NULL,
    "user_id" TEXT NOT NULL,
    "year" TEXT NOT NULL,
    CONSTRAINT "FK_Vehicles_Users_user_id" FOREIGN KEY ("user_id") REFERENCES "Users" ("id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Vehicles" ("id", "color", "created_at", "license_plate", "make", "model", "user_id", "year")
SELECT "id", "color", "created_at", "license_plate", "make", "model", "user_id", "year"
FROM "Vehicles";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Reservations";

ALTER TABLE "ef_temp_Reservations" RENAME TO "Reservations";

DROP TABLE "Sessions";

ALTER TABLE "ef_temp_Sessions" RENAME TO "Sessions";

DROP TABLE "Vehicles";

ALTER TABLE "ef_temp_Vehicles" RENAME TO "Vehicles";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Reservations_parking_lot_id" ON "Reservations" ("parking_lot_id");

CREATE INDEX "IX_Reservations_user_id" ON "Reservations" ("user_id");

CREATE INDEX "IX_Reservations_vehicle_id" ON "Reservations" ("vehicle_id");

CREATE INDEX "IX_Sessions_parking_lot_id" ON "Sessions" ("parking_lot_id");

CREATE INDEX "IX_Sessions_vehicle_id" ON "Sessions" ("vehicle_id");

CREATE INDEX "IX_Vehicles_user_id" ON "Vehicles" ("user_id");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251106105417_FixSessionVehicleFk', '9.0.9');

BEGIN TRANSACTION;
ALTER TABLE "Users" ADD "parking_lot_id" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251117111803_UpdateUserRoles', '9.0.9');

CREATE TABLE "RevokedTokens" (
    "TokenId" TEXT NOT NULL CONSTRAINT "PK_RevokedTokens" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "RevokedAt" TEXT NOT NULL,
    "ExpiresAt" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260108103628_AddRevokedTokensTable', '9.0.9');

CREATE TABLE "Billing" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Billing" PRIMARY KEY,
    "user_id" TEXT NOT NULL,
    "session_id" TEXT NULL,
    "reservation_id" TEXT NULL,
    "payment_id" TEXT NULL,
    "amount" TEXT NOT NULL,
    "currency" TEXT NOT NULL,
    "description" TEXT NULL,
    "due_date" TEXT NOT NULL,
    "paid" INTEGER NOT NULL,
    "created_at" TEXT NOT NULL,
    "paid_at" TEXT NULL,
    "invoice_number" TEXT NULL,
    "type" INTEGER NOT NULL,
    "status" INTEGER NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260108104230_AddBillingTable', '9.0.9');

CREATE TABLE "BalanceTransactions" (
    "id" TEXT NOT NULL CONSTRAINT "PK_BalanceTransactions" PRIMARY KEY,
    "user_id" TEXT NOT NULL,
    "balance_id" TEXT NULL,
    "amount" TEXT NOT NULL,
    "currency" TEXT NOT NULL,
    "type" INTEGER NOT NULL,
    "description" TEXT NULL,
    "payment_id" TEXT NULL,
    "session_id" TEXT NULL,
    "created_at" TEXT NOT NULL
);

CREATE TABLE "Companies" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Companies" PRIMARY KEY,
    "name" TEXT NOT NULL,
    "tax_id" TEXT NULL,
    "email" TEXT NULL,
    "phone" TEXT NULL,
    "address" TEXT NULL,
    "primary_contact_user_id" TEXT NULL,
    "active" INTEGER NOT NULL,
    "created_at" TEXT NOT NULL,
    "monthly_billing_enabled" INTEGER NOT NULL
);

CREATE TABLE "CompanyUsers" (
    "id" TEXT NOT NULL CONSTRAINT "PK_CompanyUsers" PRIMARY KEY,
    "company_id" TEXT NOT NULL,
    "user_id" TEXT NOT NULL,
    "role" INTEGER NOT NULL,
    "joined_at" TEXT NOT NULL
);

CREATE TABLE "HotelGuests" (
    "id" TEXT NOT NULL CONSTRAINT "PK_HotelGuests" PRIMARY KEY,
    "hotel_id" TEXT NOT NULL,
    "user_id" TEXT NOT NULL,
    "check_in" TEXT NOT NULL,
    "check_out" TEXT NULL,
    "reservation_number" TEXT NULL,
    "discount_applied" INTEGER NOT NULL,
    "created_at" TEXT NOT NULL
);

CREATE TABLE "Hotels" (
    "id" TEXT NOT NULL CONSTRAINT "PK_Hotels" PRIMARY KEY,
    "name" TEXT NOT NULL,
    "address" TEXT NULL,
    "phone" TEXT NULL,
    "email" TEXT NULL,
    "discount_percentage" TEXT NOT NULL,
    "active" INTEGER NOT NULL,
    "created_at" TEXT NOT NULL
);

CREATE TABLE "UserBalances" (
    "id" TEXT NOT NULL CONSTRAINT "PK_UserBalances" PRIMARY KEY,
    "user_id" TEXT NOT NULL,
    "balance" TEXT NOT NULL,
    "currency" TEXT NOT NULL,
    "last_updated" TEXT NOT NULL,
    "created_at" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260113114852_FixedDatabase', '9.0.9');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260114200220_DatabaseFix', '9.0.9');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260115084427_DatabaseTest', '9.0.9');

COMMIT;

