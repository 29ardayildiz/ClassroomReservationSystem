using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToAcademicTerm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AcademicTerms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AcademicTerms");
        }
    }
}
