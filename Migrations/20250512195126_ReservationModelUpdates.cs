using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class ReservationModelUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RelatedReservationId",
                table: "Reservations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RelatedReservationId",
                table: "Reservations");
        }
    }
}
