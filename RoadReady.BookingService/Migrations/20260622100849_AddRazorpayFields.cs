using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadReady.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentUrl",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayPaymentLinkId",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentUrl",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RazorpayPaymentLinkId",
                table: "Bookings");
        }
    }
}
