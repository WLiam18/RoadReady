using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadReady.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleInspections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OdometerReading = table.Column<int>(type: "int", nullable: false),
                    FuelLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleImagePaths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentImagePaths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleInspections_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_BookingId",
                table: "VehicleInspections",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleInspections");
        }
    }
}
