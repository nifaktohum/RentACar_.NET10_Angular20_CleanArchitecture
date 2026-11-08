using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _01_AddRefundFieldsToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ForfeitedAmount",
                table: "Reservations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 34);

            migrationBuilder.AddColumn<bool>(
                name: "IsDepositRefunded",
                table: "Reservations",
                type: "boolean",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 32);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "Reservations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 33);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForfeitedAmount",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsDepositRefunded",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "Reservations");
        }
    }
}
