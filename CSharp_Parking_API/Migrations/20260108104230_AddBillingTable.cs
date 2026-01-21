using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharp_Parking_API.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Billing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    session_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    reservation_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    payment_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    currency = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    due_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    paid = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    paid_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    invoice_number = table.Column<string>(type: "TEXT", nullable: true),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billing", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Billing");
        }
    }
}
