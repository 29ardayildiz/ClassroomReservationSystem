using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTermBasedReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReservationDate",
                table: "Reservations",
                newName: "TermStartDate");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TermEndDate",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TermEndDate",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "TermStartDate",
                table: "Reservations",
                newName: "ReservationDate");
        }
    }
}
