using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharp_Parking_API.Migrations
{
    /// <inheritdoc />
    public partial class FixSessionVehicleFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "vehicle_id",
                table: "Sessions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "paid_at",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "reservation_id",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_user_id",
                table: "Vehicles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_parking_lot_id",
                table: "Sessions",
                column: "parking_lot_id");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_vehicle_id",
                table: "Sessions",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_parking_lot_id",
                table: "Reservations",
                column: "parking_lot_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_user_id",
                table: "Reservations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_vehicle_id",
                table: "Reservations",
                column: "vehicle_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Parkinglots_parking_lot_id",
                table: "Reservations",
                column: "parking_lot_id",
                principalTable: "Parkinglots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_user_id",
                table: "Reservations",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Vehicles_vehicle_id",
                table: "Reservations",
                column: "vehicle_id",
                principalTable: "Vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Parkinglots_parking_lot_id",
                table: "Sessions",
                column: "parking_lot_id",
                principalTable: "Parkinglots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Vehicles_vehicle_id",
                table: "Sessions",
                column: "vehicle_id",
                principalTable: "Vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Users_user_id",
                table: "Vehicles",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Parkinglots_parking_lot_id",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_user_id",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Vehicles_vehicle_id",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Parkinglots_parking_lot_id",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Vehicles_vehicle_id",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Users_user_id",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_user_id",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_parking_lot_id",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_vehicle_id",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_parking_lot_id",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_user_id",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_vehicle_id",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "vehicle_id",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "id",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "paid_at",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "reservation_id",
                table: "Payments");
        }
    }
}
